using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Compilor
    {
        // Get The context ... (Assemblies, 
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
        public void Handle(List<ExprOperand> expressions)
        {
            foreach (var node in expressions)
                Handle(node);
        }
        public AstOperand Handle(ExprOperand node)
        {
            if (node.Operator == ExprOperator.Assign)
            {
                if (node.Left == null || node.Left.Operator != ExprOperator.Operand)
                    Error(node.Left.Token, "Left side of assignation must be a variable or data member");
                var store = HandleOperand(node.Left);
                // TODO -- Must be an operand, a variable

                var val = Handle(node.Right);
                Console.WriteLine($"${store.Id} <= ${val.Id}");
                return store;
            }
            else if (node.Operator == ExprOperator.Method)
            {
                foreach (var paramNode in node.SubResults.Reverse<ExprOperand>())
                {
                    var param = Handle(paramNode);
                    Console.WriteLine($"PUSH ${param.Id}");
                }
                var method = node.Token.Literal;
                Console.WriteLine($"$0 <= CALL {method}");
                return new AstOperand()
                {
                    Id = 0,
                    Type = AstOpdType.Undefined,
                };
            }
            else if (node.Operator == ExprOperator.Operand)
            {
                return HandleOperand(node);
            }
            else if (node.Operator == ExprOperator.Cast)
            {
                //var type = HandleType(node.Left);
                var val = Handle(node.Right);
                var ast = new AstOperand()
                {
                    Id = ++_storeId,
                    Type = AstOpdType.Undefined,

                };
                Console.WriteLine($"${ast.Id} <= CAST {val.Id}");
                return ast;
            }
            else if (node.Operator == ExprOperator.Dot)
            {
                if ((node.Right.Operator != ExprOperator.Operand || node.Right.Operator != ExprOperator.Method)
                    && node.Right.Token.Type != TokenType.Identifier)
                    throw new Exception("Bad member");
                var left = HandleOperand(node.Left);
                // Must resolve Right
                return Handle(node.Right);
            }

            return null;
        }

        private int _storeId = 0;
        protected virtual AstOperand HandleOperand(ExprOperand node)
        {
            if (node.Token.Type == TokenType.Number)
                return new AstOperand()
                {
                    Id = ++_storeId,
                    Type = AstOpdType.Int32,
                    Value = 0,
                };

            if (node.Token.Type == TokenType.String)
            {
                var uid = Guid.NewGuid().ToString();
                // Save to assembly
                return new AstOperand()
                {
                    Id = ++_storeId,
                    Type = AstOpdType.RoData,
                    Value = 0,
                    Assembly = "",
                    Name = uid,
                };
            }

            if (node.Token.Type == TokenType.Identifier)
            {
                return new AstOperand()
                {
                    Id = ++_storeId,
                    Type = AstOpdType.Undefined,
                    Name = node.Token.Literal,

                };
                // Member, Local, Parameter
            }

            // Resolve the operand
            // Primitive Value
            // ROData Value (Add to current assembly)
            // Static variable (Assemby + Name..Addres)
            // Object variable (Object + Name..Offset)
            // Local variable (Stack + Name..Offset)
            // Parameter variable (Stack + Name..Offset)
            // Namespace only
            return new AstOperand()
            {
                Id = ++_storeId,
                Type = AstOpdType.Undefined,
            };
        }
    }

    public enum AstOpdType
    {
        Undefined,
        Int8, UInt8, Int16, UInt16, Int32, UInt32, Int64, UInt64,
        Single, Double, Decimal, Boolean,
        RoData, Assembly, Object, Local,
    }

    public class AstOperand
    {
        public int Id { get; set; }
        public AstOpdType Type { get; set; }
        public ulong Value { get; set; }
        public string Name { get; set; }
        public string Assembly { get; set; }
    }
}
