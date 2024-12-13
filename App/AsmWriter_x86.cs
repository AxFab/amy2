using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    internal class AsmWriter_x86
    {
        static readonly string[] regGen8 = new string[] { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh", };
        static readonly string[] regGen16 = new string[] { "ax", "cx", "dx", "bx", "sp", "bp", "si", "di", };
        static readonly string[] regGen32 = new string[] { "eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi", };
        static readonly string[] regGen64 = new string[] { "rax", "rcx", "rdx", "rbx", "rsp", "rbp", "rsi", "rdi", };

        public static string TranslateOperand(AsmOperand operand)
        {

            if (operand.Type == OperandType.Register && operand.Word == WordSize.Byte && operand.Value < 8)
                return "%" + regGen8[operand.Value];
            if (operand.Type == OperandType.Register && operand.Word == WordSize.Word && operand.Value < 8)
                return "%" + regGen16[operand.Value];
            if (operand.Type == OperandType.Register && operand.Word == WordSize.Double && operand.Value < 8)
                return "%" + regGen32[operand.Value];
            if (operand.Type == OperandType.Register && operand.Word == WordSize.Quad && operand.Value < 8)
                return "%" + regGen64[operand.Value];

            if (operand.Type == OperandType.Memory && operand.Value < 8 && operand.Value2 == 0)
                return $"(%{regGen32[operand.Value]})";
            if (operand.Type == OperandType.Memory && operand.Value < 8)
                return $"0x{operand.Value2:x}(%{regGen32[operand.Value]})";

            if (operand.Type == OperandType.Value)
                return $"$0x{operand.Value:x}";
            if (operand.Type == OperandType.Address)
                return $"0x{operand.Value:x}";

            return "__";
        }

        public static string TranslateOperand2(AsmOperand operand)
        {
            if (operand.Type == OperandType.Register && operand.Word == WordSize.Byte && operand.Value < 8)
                return regGen8[operand.Value];
            if (operand.Type == OperandType.Register && operand.Word == WordSize.Word && operand.Value < 8)
                return regGen16[operand.Value];
            if (operand.Type == OperandType.Register && operand.Word == WordSize.Double && operand.Value < 8)
                return regGen32[operand.Value];
            if (operand.Type == OperandType.Register && operand.Word == WordSize.Quad && operand.Value < 8)
                return regGen64[operand.Value];

            if (operand.Type == OperandType.Memory && operand.Value < 8 && operand.Value2 == 0)
                return $"[{regGen32[operand.Value]}]";
            if (operand.Type == OperandType.Memory && operand.Value < 8)
                return $"[{regGen32[operand.Value]} + 0x{operand.Value2:x}]";

            if (operand.Type == OperandType.Value)
                return $"0x{operand.Value:x}";
            if (operand.Type == OperandType.Address)
                return $"[0x{operand.Value:x}]";

            return "__";
        }


        public static string TranslateOpcode(AsmOpcode op)
        {
            var mnemonic = op.Opcode.ToString().ToLower();
            var operands = "";
            if (op.Sources?.Length == 2 && op.Destinations?.Length == 1)
            {
                operands = TranslateOperand(op.Sources[0]) + "," + TranslateOperand(op.Destinations[0]);
                if ((op.Destinations[0].Type == OperandType.Address || op.Destinations[0].Type == OperandType.Memory) 
                    && op.Sources[0].Type != OperandType.Register)
                    mnemonic += op.Destinations[0].Word switch
                    {
                        WordSize.Byte => 'b',
                        WordSize.Word => 'w',
                        WordSize.Double => 'l',
                        WordSize.Quad => 'q',
                        _ => '_',
                    };
            }
            else if (op.Sources?.Length == 1 && op.Destinations?.Length <= 1)
                operands = TranslateOperand(op.Sources[0]);
            else if (mnemonic.EndsWith("cc") && op.Sources?.Length == 2)
            {
                mnemonic = mnemonic.Substring(0, mnemonic.Length - 2) + ((Jcc)op.Sources[1].Value).ToString().Substring(1).ToLowerInvariant();
                operands = TranslateOperand(op.Sources[0]);
            }

            return $"{mnemonic,-6} {operands}";
        }

        public static string TranslateOpcode2(AsmOpcode op)
        {
            var mnemonic = op.Opcode.ToString().ToLower();
            var operands = "";
            if (op.Sources?.Length == 2 && op.Destinations?.Length == 1)
            {
                operands = TranslateOperand2(op.Destinations[0]) + ", " + TranslateOperand2(op.Sources[0]);
                if ((op.Destinations[0].Type == OperandType.Address || op.Destinations[0].Type == OperandType.Memory) && op.Sources[0].Type != OperandType.Register)
                    mnemonic += op.Destinations[0].Word switch
                    {
                        WordSize.Byte => " byte",
                        WordSize.Word => " word",
                        WordSize.Double => " dword",
                        WordSize.Quad => " qword",
                        _ => '_',
                    };
            }
            else if (op.Sources?.Length == 1 && op.Destinations?.Length <= 1)
                operands = TranslateOperand2(op.Sources[0]);
            else if (mnemonic.EndsWith("cc") && op.Sources?.Length == 2)
            {
                mnemonic = mnemonic.Substring(0, mnemonic.Length - 2) + ((Jcc)op.Sources[1].Value).ToString().Substring(1).ToLowerInvariant();
                operands = TranslateOperand2(op.Sources[0]);
            }

            return $"{mnemonic} {operands}";
        }
    }
}
