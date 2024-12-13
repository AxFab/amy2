using App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class ExprOperand
    {
        private ExpresionBuilder _subExpression;
        public ExprOperand(Token token, ExprOperator op)
        {
            Token = token;
            Operator = op;
        }

        public Token Token { get; }
        public ExprOperator Operator { get; private set; }
        public ExprOperand? Left { get; set; }
        public ExprOperand? Right { get; set; }
        public int Priority => Operator switch
        {
            ExprOperator.Operand => 0,
            ExprOperator.Parenthesis => 99,
            ExprOperator.Method => 1,

            ExprOperator.IncPfx => 3,
            ExprOperator.DecPfx => 3,

            ExprOperator.IncSfx => 2,
            ExprOperator.DecSfx => 2,
            ExprOperator.Not => 3,
            ExprOperator.BitwiseNot => 3,
            ExprOperator.Negative => 3,

            ExprOperator.Dot => 1,
            ExprOperator.Cast => 4,
            ExprOperator.Mul => 5,
            ExprOperator.Div => 5,
            ExprOperator.Mod => 5,
            ExprOperator.Add => 6,
            ExprOperator.Sub => 6,

            ExprOperator.ShiftLeft => 7,
            ExprOperator.ShiftRight => 7,
            ExprOperator.Less => 8,
            ExprOperator.More => 8,
            ExprOperator.LessOrEq => 8,
            ExprOperator.MoreOrEq => 8,
            ExprOperator.Equals => 9,
            ExprOperator.NotEquals => 9,
            ExprOperator.BitwiseAnd => 10,
            ExprOperator.BitwiseXor => 11,
            ExprOperator.BitwiseOr => 12,
            ExprOperator.And => 13,
            ExprOperator.Or => 14,
            ExprOperator.NullCoalessence => 15,
            ExprOperator.Assign => 16,
            ExprOperator.Throw => 17,
            ExprOperator.Comma => 18,

            ExprOperator.Condition => 15,

            _ => -1,
        };

        public bool IsOperand => Operator == ExprOperator.Operand;
        public bool IsUnaryLR => Operator > ExprOperator._StartUnaryLR && Operator < ExprOperator._EndUnaryLR;
        public bool IsBinary => Operator > ExprOperator._StartBinary && Operator < ExprOperator._EndBinary;
        public bool IsAssociativityLeftToRight
            => (IsBinary && Operator != ExprOperator.NullCoalessence && Operator != ExprOperator.Assign && Operator != ExprOperator.Throw)
                || Operator < ExprOperator._EndUnaryRL;

        public List<ExprOperand> SubResults => _subExpression.Results;

        public bool UseParenthesis { get; set; } = false;

        public void AsFunction(ExpresionBuilder subExpression)
        {
            if (Operator != ExprOperator.Operand)
                throw new Exception();
            Operator = ExprOperator.Method;
            _subExpression = subExpression;
        }
    }
    public enum ExprState
    {
        Start,
        Operand,
        Call,
        BinaryOperator,
        UnaryOperatorLeftRight,
        End,
    }

    public enum ExprOperator
    {
        Operand,
        Parenthesis,
        Method,

        _StartUnaryRL,
        IncSfx,
        DecSfx,
        _EndUnaryRL,

        _StartUnaryLR,
        IncPfx,
        DecPfx,
        Not,
        BitwiseNot,
        Negative,
        _EndUnaryLR,

        _StartBinary,
        Dot,
        Cast,
        Mul,
        Div,
        Mod,
        Add,
        Sub,
        ShiftLeft,
        ShiftRight,
        Less,
        More,
        LessOrEq,
        MoreOrEq,
        Equals,
        NotEquals,
        BitwiseAnd,
        BitwiseXor,
        BitwiseOr,
        And,
        Or,
        NullCoalessence,
        Assign,
        Throw,
        Comma,
        _EndBinary,

        Condition,
    };

    public class ExpresionBuilder
    {
        private Stack<ExprOperand> _postFix = new Stack<ExprOperand>();
        private Stack<ExprOperand> _inFix = new Stack<ExprOperand>();
        private ExpresionBuilder _subExpression = null;
        private int _parenthesisCount = 0;
        private bool _isSub;

        public ExpresionBuilder(bool isSub = false)
        {
            _isSub = isSub;
        }
        public ExprState State { get; private set; } = ExprState.Start;

        public List<ExprOperand> Results { get; } = new List<ExprOperand>();

        public void PushOperand(Token token)
        {
            if (_subExpression != null)
            {
                _subExpression.PushOperand(token);
                return;
            }

            SetStatus(ExprOperator.Operand);
            _postFix.Push(new ExprOperand(token, ExprOperator.Operand));
        }

        public void PushOperator(Token token, ExprOperator op)
        {
            if (_subExpression != null)
            {
                _subExpression.PushOperand(token);
                return;
            }

            SetStatus(op);
            ExprOperand node = new ExprOperand(token, op);
            while (_inFix.Count > 0 && _inFix.Peek().Priority <= node.Priority)
            {
                ExprOperand nd = _inFix.Pop();
                AddOperator(nd);
            }

            _inFix.Push(node);
        }


        public void OpenParenthese(Token token)
        {
            if (_subExpression != null)
            {
                _subExpression.OpenParenthese(token);
                return;
            }

            SetStatus(ExprOperator.Parenthesis);
            if (State == ExprState.Call)
            {
                if (_postFix.Count < 1)
                    throw new Exception("Internal Error: Missing operand to use as function name.");              
                var node = _postFix.Peek();
                _subExpression = new ExpresionBuilder(true);
                node.AsFunction(_subExpression);
            }
            else
            {
                _parenthesisCount++;
                ExprOperand node = new ExprOperand(token, ExprOperator.Parenthesis);
                _inFix.Push(node);
            }
        }

        public bool CloseParenthese(Token token)
        {
            if (_subExpression != null)
            {
                var res = _subExpression.CloseParenthese(token);
                if (res)
                {
                    _subExpression = null;
                    State = ExprState.Operand;
                }
                return false;
            }

            if (State == ExprState.Start && _isSub) 
            {
                State = ExprState.End;
                return true;
            }

            if (State != ExprState.Operand)
                throw new Exception("unexpected parenthese");
            
            if (_parenthesisCount == 0)
            {
                if (_isSub)
                {
                    Resolve();
                    return true;
                }
                throw new Exception("closing parenthese without openning");
            }

            var node = _inFix.Pop();
            while (node.Operator != ExprOperator.Parenthesis)
            {
                AddOperator(node);
                if (_inFix.Count == 0)
                    throw new Exception("closing parenthese without openning");
                node = _inFix.Pop();
            }

            // In order to prapre for cast
            _postFix.Peek().UseParenthesis = true;
            return false;
        }

        public void Comma(Token token)
        {
            if (_subExpression != null)
            {
                _subExpression.Comma(token);
            }
            else
            {
                Resolve();
                _parenthesisCount = 0;
                State = ExprState.Start;
            }
        }

        public void Resolve()
        {
            if (_subExpression != null)
                throw new Exception("Missing closing parenthese");

            if (_inFix.Any(x => x.Operator == ExprOperator.Parenthesis))
                throw new Exception("Missing closing parenthese");

            if (State != ExprState.Operand)
                throw new Exception("Expression incompleted, expected operand");
            
            while (_inFix.Count > 0)
                AddOperator(_inFix.Pop());

            if (_postFix.Count != 1)
                throw new Exception("Unexpected error, unable to resolve the expression");

            Results.Add(_postFix.Pop());
            State = ExprState.End;
        }

        private void SetStatus(ExprOperator op)
        {
            if (op == ExprOperator.Parenthesis && State == ExprState.Operand)
            {
                State = ExprState.Call;
            }
            else if (op == ExprOperator.Operand || op == ExprOperator.Parenthesis)
            {
                if (State == ExprState.Operand)
                {
                    // Check cast ?
                    if (_postFix.Count > 0 && _postFix.Peek().UseParenthesis && _postFix.Peek().Operator == ExprOperator.Operand)
                        PushOperator(null, ExprOperator.Cast);
                }
                if (State != ExprState.Start && State != ExprState.BinaryOperator && State != ExprState.UnaryOperatorLeftRight)
                    throw new Exception("Unexpected operand");
                State = op == ExprOperator.Operand ? ExprState.Operand : ExprState.Start;
            }
            else if (op > ExprOperator._StartUnaryLR && op < ExprOperator._EndUnaryLR)
            {
                if (State == ExprState.Operand)
                    throw new Exception("Unexpected prefix operator");
                State = ExprState.UnaryOperatorLeftRight;
            }
            else if (op > ExprOperator._StartBinary && op < ExprOperator._EndBinary)
            {
                if (State != ExprState.Operand)
                    throw new Exception("Unexpected operator");
                State = ExprState.BinaryOperator;
            }
            else
            {
                throw new Exception();
            }
        }

        private void AddOperator(ExprOperand node)
        {
            if (node.Operator == ExprOperator.Operand)
                throw new Exception("Internal error, can't push operands on the postfix stack: " + node.Operator);

            if (node.Operator > ExprOperator._StartUnaryLR && node.Operator < ExprOperator._EndUnaryLR)
            {
                if (_postFix.Count < 1)
                    throw new Exception("Missing operand for the operator: " + node.Operator);
                node.Right = _postFix.Pop();
            }
            else if (node.Operator > ExprOperator._StartBinary && node.Operator < ExprOperator._EndBinary)
            {
                if (_postFix.Count < 2)
                    throw new Exception("Missing operand for the operator: " + node.Operator);
                node.Right = _postFix.Pop();
                node.Left = _postFix.Pop();
            }
            else
            {
                throw new Exception("Invalid operator: " + node.Operator);
            }

            _postFix.Push(node);
        }

        internal static ExprOperator FindOperator(string literal, bool allowAssign = true, bool simpleEq = false)
        {
            return literal switch
            {
                "!" => ExprOperator.Not,
                "~" => ExprOperator.BitwiseNot,
                "." => ExprOperator.Dot,
                "*" => ExprOperator.Mul,
                "/" => ExprOperator.Div,
                "%" => ExprOperator.Mod,
                "+" => ExprOperator.Add,
                "-" => ExprOperator.Sub,

                "<<" => ExprOperator.ShiftLeft,
                ">>" => ExprOperator.ShiftRight,
                "<" => ExprOperator.Less,
                ">" => ExprOperator.More,
                "<=" => ExprOperator.LessOrEq,
                ">=" => ExprOperator.MoreOrEq,
                "==" => !simpleEq ? ExprOperator.Equals : throw new Exception(),
                "!=" => ExprOperator.NotEquals,
                "&" => ExprOperator.BitwiseAnd,
                "^" => ExprOperator.BitwiseXor,
                "|" => ExprOperator.BitwiseOr,
                "&&" => ExprOperator.And,
                "||" => ExprOperator.Or,

                "??" => allowAssign ? ExprOperator.NullCoalessence : throw new Exception(),
                "=" => allowAssign ? ExprOperator.Assign : (simpleEq ? ExprOperator.Equals : throw new Exception()),

                "*=" => throw new NotImplementedException(),
                "/=" => throw new NotImplementedException(),
                "%=" => throw new NotImplementedException(),
                "+=" => throw new NotImplementedException(),
                "-=" => throw new NotImplementedException(),
                "<<=" => throw new NotImplementedException(),
                ">>=" => throw new NotImplementedException(),
                "&=" => throw new NotImplementedException(),
                "^=" => throw new NotImplementedException(),
                "|=" => throw new NotImplementedException(),

                "<<<" => throw new NotImplementedException(),
                ">>>" => throw new NotImplementedException(),
                "<<<=" => throw new NotImplementedException(),
                ">>>=" => throw new NotImplementedException(),

                _ => throw new Exception(),
            };
        }
    }
}
