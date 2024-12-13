using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App
{
    public class ContextBuilder
    {
        private List<ScopeInfo> _usings = new List<ScopeInfo>();
        private UnresolvedScopeInfo? _unresolved;
        public ContextBuilder(ContextBuilder? parent, ScopeInfo scope, bool isType = false)
        {
            Parent = parent;
            Scope = scope;
            IsType = isType;
            if (parent != null)
            {
                _usings.AddRange(parent._usings);
            }
        }
        public ContextBuilder? Parent { get; private set; }
        public ScopeInfo Scope { get; private set; }
        public bool IsType { get; private set; }

        public UnresolvedScopeInfo Unresolved => _unresolved ??= new UnresolvedScopeInfo(this);

        public void AddBase(TypeInfo baseType)
        {
            if (!(Scope is TypeInfo type))
                throw new InvalidOperationException($"Cannot set base type to {Scope.GetType().Name}");
            type.AddBase(baseType);
        }

        public void Using(ScopeInfo? scope)
        {
            if (scope != null)
                _usings.Add(scope);
        }
        internal IEnumerable<string> UsingList()
        {
            yield return Scope.Fullname;
            foreach (var use in _usings)
                yield return use.Fullname;
        }

        public FieldInfo CreateField(Token nameToken, TypeInfo fieldType, TypeQualifier qualifiers)
        {
            if (!(Scope is TypeInfo type))
                throw new InvalidOperationException($"Cannot set base type to {Scope.GetType().Name}");
            if (type.Qualifiers.HasFlag(TypeQualifier.Static) && !qualifiers.HasFlag(TypeQualifier.Static))
                Error(nameToken, "All member of a static class must be static");
            var field = new FieldInfo(type, nameToken.Literal, fieldType, qualifiers);
            type.NewMember(field);
            Console.WriteLine($"  create field {type.Fullname}.{field.Name}");
            return field;
        }
        public ConstructorInfo CreateConstructor(Token nameToken, TypeQualifier qualifiers, List<ParameterInfo> parameters)
        {
            if (!(Scope is TypeInfo type))
                throw new InvalidOperationException($"Cannot set base type to {Scope.GetType().Name}");
            if (type.Qualifiers.HasFlag(TypeQualifier.Static))
                Error(nameToken, "Static class can't declare a constructor");
            if (qualifiers.HasFlag(TypeQualifier.Static) && parameters.Count != 0)
                Error(nameToken, "Static constructors can't declare any parameters");
            var cstor = new ConstructorInfo(type, qualifiers, parameters);
            type.NewMember(cstor);
            Console.WriteLine($"  create constructor {type.Fullname}()");
            return cstor;
        }

        public MethodInfo CreateMethod(Token nameToken, TypeInfo methodType, TypeQualifier qualifiers, List<ParameterInfo> parameters)
        {
            if (!(Scope is TypeInfo type))
                throw new InvalidOperationException($"Cannot set base type to {Scope.GetType().Name}");
            if (type.Qualifiers.HasFlag(TypeQualifier.Static) && !qualifiers.HasFlag(TypeQualifier.Static))
                Error(nameToken, "All member of a static class must be static");
            var method = new MethodInfo(type, nameToken.Literal, methodType, qualifiers, parameters);
            type.NewMember(method);
            Console.WriteLine($"  create method {type.Fullname}.{method.Name}()");
            return method;
        }

        public void Warning(Token token, string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[Warning] {token} - {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        public void Error(Token token, string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error] {token} - {message}");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

    }


    public class CSharpParser
    {
        private Lexer _lexer;
        private Dictionary<string, ScopeInfo> _scopes = new Dictionary<string, ScopeInfo>();
        private Dictionary<string, TypeInfo> _aliases = new Dictionary<string, TypeInfo>();
        private List<TypeInfo> _types = new List<TypeInfo>();
        private List<TypeInfo> _unresolved_types = new List<TypeInfo>();
        private List<ContextBuilder> _ctxs = new List<ContextBuilder>();
        private ContextBuilder _context;

        public CSharpParser()
        {
            Initialize();
        }

        public void Initialize()
        {
            var sys = new ScopeInfo(null, "System");
            _scopes.Add("System", sys);

            _aliases.Add("void", new TypeInfo(sys, "Void", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("char", new TypeInfo(sys, "Char", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("byte", new TypeInfo(sys, "Byte", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("sbyte", new TypeInfo(sys, "SByte", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("short", new TypeInfo(sys, "Int16", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("ushort", new TypeInfo(sys, "UInt16", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("int", new TypeInfo(sys, "Int32", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("uint", new TypeInfo(sys, "UInt32", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("long", new TypeInfo(sys, "Int64", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("ulong", new TypeInfo(sys, "UInt64", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("flaot", new TypeInfo(sys, "Single", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("double", new TypeInfo(sys, "Double", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("decimal", new TypeInfo(sys, "Decimal", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("bool", new TypeInfo(sys, "Boolean", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("intptr", new TypeInfo(sys, "IntPtr", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("uintptr", new TypeInfo(sys, "UIntPtr", TypeQualifier.Public | TypeQualifier.Struct));
            _aliases.Add("object", new TypeInfo(sys, "Object", TypeQualifier.Public | TypeQualifier.Class));
            _aliases.Add("string", new TypeInfo(sys, "String", TypeQualifier.Public | TypeQualifier.Class));

            foreach (var alias in _aliases)
                _scopes.Add(alias.Value.Fullname, alias.Value);

            _context = OpenContext(null);
        }
        public void Parse(string path)
        {
            _lexer = new Lexer(path);
            ReadScope();
        }

        public void ParseText(string text)
        {
            _lexer = Lexer.FromText(text);
            ReadScope();
        }

        public void ParseExpr(string text)
        {
            _lexer = Lexer.FromText(text);
            var ex = ReadExpression();
            var cc = new Compilor();
            cc.Handle(ex);
        }

        protected ContextBuilder OpenContext(ScopeInfo scope)
        {
            ContextBuilder ctx = new ContextBuilder(_context, scope);
            _ctxs.Add(ctx);
            return ctx;
        }
        protected ContextBuilder OpenTypeContext(string name, TypeQualifier qualifiers)
        {
            var type = new TypeInfo(_context.Scope, name, qualifiers);
            _types.Add(type);
            _scopes.Add(type.Fullname, type);
            ContextBuilder ctx = new ContextBuilder(_context, type, true);
            _ctxs.Add(ctx);
            return ctx;
        }

        protected void CloseContext()
        {
            _context = _context.Parent;
            if (_context == null)
                _context = _ctxs[0];
        }
        protected void ReadScope()
        {
            var qualifiers = TypeQualifier.QualNone;
            for (; ; )
            {
                var token = _lexer.NextToken();
                if (token.Type == TokenType.Identifier && token.Literal == "using")
                {
                    _context.Using(ReadScopeName(null, ";"));
                    qualifiers = TypeQualifier.QualNone;
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "namespace")
                {
                    ScopeInfo ns = ReadScopeName(_context.Scope, "{");
                    if (ns == null)
                        return;
                    _context = OpenContext(ns);
                    qualifiers = TypeQualifier.QualNone;
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "public")
                    SetQualifier(token, TypeQualifier.Public, ref qualifiers);
                else if (token.Type == TokenType.Identifier && token.Literal == "private")
                    SetQualifier(token, TypeQualifier.Private, ref qualifiers);
                else if (token.Type == TokenType.Identifier && token.Literal == "internal")
                    SetQualifier(token, TypeQualifier.Internal, ref qualifiers);
                else if (token.Type == TokenType.Identifier && token.Literal == "class")
                {
                    ReadTypeDeclaration(qualifiers | TypeQualifier.Class);
                    qualifiers = TypeQualifier.QualNone;
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "interface")
                {
                    ReadTypeDeclaration(qualifiers | TypeQualifier.Interface);
                    qualifiers = TypeQualifier.QualNone;
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "struct")
                {
                    ReadTypeDeclaration(qualifiers | TypeQualifier.Struct);
                    qualifiers = TypeQualifier.QualNone;
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "enum")
                {
                    ReadTypeDeclaration(qualifiers | TypeQualifier.Enum);
                    qualifiers = TypeQualifier.QualNone;
                }
                else if (_context.IsType)
                {

                    if (token.Literal == _context.Scope.Name)
                        ReadConstructor(token, qualifiers);
                    else if (token.Type == TokenType.Operator && token.Literal == "~")
                    {
                        ReadDestructor(qualifiers);
                    }
                    else
                    {
                        _lexer.PushBack(token);
                        ReadMember(qualifiers);
                    }
                    qualifiers = TypeQualifier.QualNone;
                }
                else
                {
                    ErrorUnexpected(token);
                }
            }
        }

        protected void ReadTypeDeclaration(TypeQualifier qualifiers)
        {
            var token = _lexer.NextToken();
            // TODO -- Check identifier--non reserved word
            _context = OpenTypeContext(token.Literal, qualifiers | TypeQualifier.Class);
            token = _lexer.NextToken();
            if (token.Type == TokenType.Operator && token.Literal == ":")
            {
                for (; ; )
                {
                    TypeInfo type = ReadUnresolvedType();
                    _context.AddBase(type);
                    token = _lexer.NextToken();
                    if (token.Type != TokenType.Operator || token.Literal != ",")
                        break;
                }
            }
            if (token.Type != TokenType.Operator || token.Literal != "{")
            {
                ErrorUnexpected(token, "{");
                CloseContext();
            }
        }

        protected TypeInfo ReadUnresolvedType()
        {
            ScopeInfo scope = _context.Unresolved;
            for (; ; )
            {
                var token = _lexer.NextToken();
                if (token.Type != TokenType.Identifier)
                {
                    ErrorUnexpected(token);
                    return null;
                }
                // TODO -- Check identifier--non reserved word
                var name = token.Literal;
                token = _lexer.NextToken();
                if (token.Type == TokenType.Operator && token.Literal == ".")
                {
                    scope = OpenScope(scope, name);
                    continue;
                }

                _lexer.PushBack(token);
                if (scope == _context.Unresolved && _aliases.ContainsKey(name))
                    return _aliases[name];
               
                TypeInfo type = new TypeInfo(_context.Unresolved, name, TypeQualifier.QualNone); // TODO -- Set unresolved
                _unresolved_types.Add(type);
                // _scopes.insert(type.Fullname, type)
                return type;
            }
        }
        protected void ReadConstructor(Token memberToken, TypeQualifier qualifiers)
        {
            var token = _lexer.NextToken();
            if (token.Type == TokenType.Operator && token.Literal == "<")
            {
                var gparameters = ReadGenericParameters();
                token = _lexer.NextToken();
            }

            if (token.Literal != "(")
            {
                ErrorUnexpected(token, "(");
                return;
            }
            var parameters = ReadParameters();
            token = _lexer.NextToken();
            
            if (token.Type == TokenType.Operator && token.Literal == ":")
            {
                throw new NotImplementedException();
            }

            _context.CreateConstructor(memberToken, qualifiers, parameters);
            ReadBody(token, "Constructor need to declare a body");
        }

        protected void ReadBody(Token token, string error)
        {
            Compilor cc;
            if (token.Type == TokenType.Operator && token.Literal == "{")
            {
                cc = ReadBlock();
            }
            else if (token.Type == TokenType.Operator && token.Literal == "=>")
            {
                cc = new Compilor();
                var expressions = ReadExpression();
                cc.Handle(expressions);
            }
            else if (token.Type == TokenType.Operator && token.Literal == ";")
            {
                Error(token, error);
            }
        }

        protected void ReadDestructor(TypeQualifier qualifiers) { throw new NotImplementedException(); }
        protected void ReadMember(TypeQualifier qualifiers)
        {

            var type = ReadUnresolvedType();
            var memberToken = _lexer.NextToken();
            var token = _lexer.NextToken();
            if (token.Literal == ".")
            {
                // Explicit implementation
            }

            if (token.Type == TokenType.Operator && (token.Literal == ";" || token.Literal == "="))
            {
                if (token.Literal == "=")
                {
                    var expr = ReadExpression();
                    // Read expression
                }
                _context.CreateField(memberToken, type, qualifiers);
                return;
            }
            
            if (token.Type == TokenType.Operator && token.Literal == "<")
            {
                var gparameters = ReadGenericParameters();
                token = _lexer.NextToken();
            }

            if (token.Type == TokenType.Operator && token.Literal == "(")
            {
                var parameters = ReadParameters();
                _context.CreateMethod(memberToken, type, qualifiers, parameters);
                token = _lexer.NextToken();
                // Is on interface or abstract !?
                ReadBody(token, "Method must declare a body as they are not marked abstract, extern or partial");
                return;
            }

            throw new NotImplementedException();
        }

        protected object ReadGenericParameters() { throw new NotImplementedException(); }
        protected List<ParameterInfo> ReadParameters()
        {
            var parameters = new List<ParameterInfo>();

            var token = _lexer.NextToken();
            if (token.Literal == ")")
                return parameters;
            _lexer.PushBack(token);

            for (; ; )
            {
                ParamQualifier qualifier = ParamQualifier.None;
                var ptype = ReadUnresolvedType();
                if (ptype == null)
                    throw new Exception();
                for (; ; )
                {
                    token = _lexer.NextToken();
                    if (token.Literal == "in")
                        qualifier |= ParamQualifier.In;
                    else if (token.Literal == "out")
                        qualifier |= ParamQualifier.Out;
                    else if (token.Literal == "ref")
                        qualifier |= ParamQualifier.Ref;
                    else if (token.Type != TokenType.Identifier)
                        ErrorUnexpected(token);
                    else
                    {
                        parameters.Add(new ParameterInfo(parameters.Count, ptype, token.Literal, qualifier));
                        break;
                    }
                }
                token = _lexer.NextToken();
                if (token.Literal == ",")
                    continue;
                if (token.Literal == ")")
                    return parameters;
                if (token.Literal == "=")
                {
                    var expr = ReadExpression();
                    throw new NotImplementedException();
                }
                ErrorUnexpected(token, ",", ")");
            }
        }
        protected Compilor ReadBlock()
        {
            var cc = new Compilor();
            for (; ; )
            {
                var token = _lexer.NextToken();
                if (token.Type == TokenType.Operator && token.Literal == "}")
                    break;
                else if (token.Type == TokenType.Identifier && token.Literal == "if")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "else")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "for")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "while")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "do")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "switch")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "throw")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "return")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "delete")
                {
                }
                else if (token.Type == TokenType.Identifier && token.Literal == "var")
                {
                    var expr = ReadExpression(); 
                    cc.Handle(expr);
                }
                else if (token.Type == TokenType.Identifier)
                {
                    var token2 = _lexer.NextToken();
                    if (token2.Literal == "." || token2.Type == TokenType.Identifier)
                    {
                        // This is a type
                    } else
                    {
                        _lexer.PushBack(token, token2);
                        var expr = ReadExpression();
                        cc.Handle(expr);
                    }
                } else {
                    var expr = ReadExpression();
                    cc.Handle(expr);
                }
            }
            return cc;
        }
        protected List<ExprOperand> ReadExpression()
        {
            var builder = new ExpresionBuilder();
            for (; ; )
            {
                var token = _lexer.NextToken();
                if (token.Type == TokenType.Operator)
                {
                    if (token.Literal == "!")
                        builder.PushOperator(token, ExprOperator.Not);
                    else if (token.Literal == "~")
                        builder.PushOperator(token, ExprOperator.BitwiseNot);
                    else if (token.Literal == "*")
                        builder.PushOperator(token, ExprOperator.Mul);
                    else if (token.Literal == "/")
                        builder.PushOperator(token, ExprOperator.Div);
                    else if (token.Literal == "%")
                        builder.PushOperator(token, ExprOperator.Mod);
                    else if (token.Literal == "+")
                        builder.PushOperator(token, ExprOperator.Add);
                    else if (token.Literal == "=")
                        builder.PushOperator(token, ExprOperator.Assign);
                    else if (token.Literal == "<<")
                        builder.PushOperator(token, ExprOperator.ShiftLeft);
                    else if (token.Literal == ">>")
                        builder.PushOperator(token, ExprOperator.ShiftRight);
                    else if (token.Literal == "<")
                        builder.PushOperator(token, ExprOperator.Less);
                    else if (token.Literal == ">")
                        builder.PushOperator(token, ExprOperator.More);
                    else if (token.Literal == "<=")
                        builder.PushOperator(token, ExprOperator.LessOrEq);
                    else if (token.Literal == ">=")
                        builder.PushOperator(token, ExprOperator.MoreOrEq);
                    else if (token.Literal == "==")
                        builder.PushOperator(token, ExprOperator.Equals);
                    else if (token.Literal == "!=")
                        builder.PushOperator(token, ExprOperator.NotEquals);
                    else if (token.Literal == "&")
                        builder.PushOperator(token, ExprOperator.BitwiseAnd);
                    else if (token.Literal == "^")
                        builder.PushOperator(token, ExprOperator.BitwiseXor);
                    else if (token.Literal == "|")
                        builder.PushOperator(token, ExprOperator.BitwiseOr);
                    else if (token.Literal == "&&")
                        builder.PushOperator(token, ExprOperator.And);
                    else if (token.Literal == "||")
                        builder.PushOperator(token, ExprOperator.Or);
                    else if (token.Literal == "??")
                        builder.PushOperator(token, ExprOperator.NullCoalessence);
                    else if (token.Literal == "=")
                        builder.PushOperator(token, ExprOperator.Assign);
                    else if (token.Literal == ".")
                        builder.PushOperator(token, ExprOperator.Dot);
                    else if (token.Literal == "-")
                    {
                        if (builder.State == ExprState.Call || builder.State == ExprState.Operand)
                            builder.PushOperator(token, ExprOperator.Sub);
                        else
                            builder.PushOperator(token, ExprOperator.Negative);
                    }
                    else if (token.Literal == "++")
                    {
                        if (builder.State == ExprState.Call || builder.State == ExprState.Operand)
                            builder.PushOperator(token, ExprOperator.IncSfx);
                        else
                            builder.PushOperator(token, ExprOperator.IncPfx);
                    }
                    else if (token.Literal == "--")
                    {
                        if (builder.State == ExprState.Call || builder.State == ExprState.Operand)
                            builder.PushOperator(token, ExprOperator.DecSfx);
                        else
                            builder.PushOperator(token, ExprOperator.DecPfx);
                    }
                    else if (token.Literal == ",")
                        builder.Comma(token);
                    else if (token.Literal == "(")
                        builder.OpenParenthese(token);
                    else if (token.Literal == ")")
                        builder.CloseParenthese(token);
                    else if (token.Literal == ";")
                    {
                        builder.Resolve();
                        return builder.Results;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (token.Literal == "throw")
                {
                    builder.PushOperator(token, ExprOperator.Throw);
                }
                else if (token.Literal == "new")
                {
                    var callToken = ReadFullname();
                    token = _lexer.NextToken();
                    if (token.Literal != "(")
                        ErrorUnexpected(token, "(");
                    else
                    {
                        builder.PushOperand(callToken);
                        builder.OpenParenthese(token);
                    }
                } 
                else
                {
                    builder.PushOperand(token);
                }
            }
        }

        protected Token ReadFullname()
        {
            var list = new List<Token>();
            for (; ; )
            {
                var token = _lexer.NextToken();
                if (token.Type != TokenType.Identifier)
                    break; // Error !?
                list.Add(token);
                token = _lexer.NextToken();
                if (token.Literal == ".")
                {
                    list.Add(token);
                    continue;
                }
                if (token.Literal == "<")
                    throw new NotImplementedException();
                _lexer.PushBack(token);
                break;
            }

            return Token.Join(list.ToArray());
        }

        protected ScopeInfo ReadScopeName(ScopeInfo scope, string ending)
        {
            for (; ; )
            {
                var token = _lexer.NextToken();
                if (token.Type != TokenType.Identifier)
                {
                    ErrorUnexpected(token);
                    return null;
                }
                // TODO -- Check identifier--non reserved word
                scope = OpenScope(scope, token.Literal);

                token = _lexer.NextToken();
                if (token.Type == TokenType.Operator && token.Literal == ".")
                    continue;
                if (token.Type == TokenType.Operator && token.Literal == ending)
                    return scope;
                ErrorUnexpected(token, ".", ending);
                return null;
            }
        }

        protected ScopeInfo OpenScope(ScopeInfo parent, string name)
        {
            var fullname = parent != null ? parent.Fullname + "." + name : name;
            if (_scopes.TryGetValue(fullname, out var scope))
                return scope;
            scope = new ScopeInfo(parent, name);
            _scopes[fullname] = scope;
            return scope;
        }

        private void SetQualifier(Token token, TypeQualifier qual, ref TypeQualifier qualifiers)
        {
            if (qualifiers.HasFlag(qual))
                Warning(token, "Duplicate type qualifier 'public'");
            else if (qualifiers != 0 && TypeQualifier.VisibilityMask.HasFlag(qualifiers) && TypeQualifier.VisibilityMask.HasFlag(qual))
                Warning(token, "Already specified visibility type qualifier");
            qualifiers |= qual;

        }


        public void Warning(Token token, string message) => _context.Warning(token, message);
        public void Error(Token token, string message) => _context.Error(token, message);
        public void ErrorUnexpected(Token token, params string[] expected)
        {
            if (expected == null || expected.Length == 0)
                Error(token, $"Unexpected token '{token.Literal}'.");
            else if (expected.Length == 1)
                Error(token, $"Unexpected token '{token.Literal}', expecting '{expected[0]}'");
            else
                Error(token, $"Unexpected token '{token.Literal}', would expect on of: {string.Join(", ", expected.Select(x => $"'{x}'"))}");
        }
    }
}
