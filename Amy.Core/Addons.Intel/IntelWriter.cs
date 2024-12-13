using Amy.Core.Bytes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Addons.Intel
{
    public class IntelWriter : IAsmWriter
    {
        static readonly string[] regGen8 = new string[] { "al", "cl", "dl", "bl", "ah", "ch", "dh", "bh", };
        static readonly string[] regGen16 = new string[] { "ax", "cx", "dx", "bx", "sp", "bp", "si", "di", };
        static readonly string[] regGen32 = new string[] { "eax", "ecx", "edx", "ebx", "esp", "ebp", "esi", "edi", };
        static readonly string[] regGen64 = new string[] { "rax", "rcx", "rdx", "rbx", "rsp", "rbp", "rsi", "rdi", };
        static readonly string[] regSeg16 = new string[] { "es", "cs", "ss", "ds", "fs", "gs" };
        static readonly string[] regCtrl32 = new string[] { "cr0", "cr1", "cr2", "cr3", "cr4" };
        static readonly string[] regDbg32 = new string[] { "dbg0", "dbg1", "dbg2", "dbg3", "dbg4", null, null, "dbg7" };

        private bool _useIntel = false;
        public string Stringify(SmoInstruction instruction, long offset, int size, byte[] buf)
        {
            var prefix = string.IsNullOrEmpty(instruction.Prefix) ? string.Empty : $"{instruction.Prefix} ";
            var mnemonic = instruction.Opcode.ToString().ToLower();
            var array = instruction.Operands?.Select(x => WriteOperand(x, offset, size)).ToArray();
            var operands = string.Empty;

            if (instruction.Order == SmoOrder.BinaryDS)
            {
                if ((instruction.Opcode == IntelMnemonics.SHL || instruction.Opcode == IntelMnemonics.SAL ||
                    instruction.Opcode == IntelMnemonics.SHR || instruction.Opcode == IntelMnemonics.SAR ||
                    instruction.Opcode == IntelMnemonics.RCL || instruction.Opcode == IntelMnemonics.ROL ||
                    instruction.Opcode == IntelMnemonics.RCR || instruction.Opcode == IntelMnemonics.ROR) &&
                    instruction.Operands[1].Type == SmoOperandType.Value && instruction.Operands[1].Value == 1 &&
                    instruction.Opcode != "ENTER")
                {
                    var dst = instruction.Operands[0];
                    operands = _useIntel ? $"{array[0]}" : $"{array[0]}";
                    if (dst.Type != SmoOperandType.Register)
                        mnemonic += dst.Size switch
                        {
                            SmoSize.Byte => _useIntel ? " byte" : "b",
                            SmoSize.Word => _useIntel ? " word" : "w",
                            SmoSize.Double => _useIntel ? " dword" : "l",
                            SmoSize.Quad => _useIntel ? " qword" : "q",
                            _ => throw new NotImplementedException(),
                        };
                }
                else if ((instruction.Opcode == IntelMnemonics.MUL || instruction.Opcode == IntelMnemonics.IMUL ||
                    instruction.Opcode == IntelMnemonics.DIV || instruction.Opcode == IntelMnemonics.IDIV) && buf[0] != 0x0f &&
                    instruction.Operands[0].Type == SmoOperandType.Register && instruction.Operands[0].Value == 0 && 
                    (instruction.Operands[1].Type == SmoOperandType.Register || instruction.Operands[1].Type == SmoOperandType.Memory || instruction.Operands[1].Type == SmoOperandType.MemoryOffset) &&
                    instruction.Opcode != "ENTER")
                {
                    var src = instruction.Operands[1];
                    operands = _useIntel ? $"{array[1]}" : $"{array[1]}";
                    if (src.Type != SmoOperandType.Register)
                        mnemonic += src.Size switch
                        {
                            SmoSize.Byte => _useIntel ? " byte" : "b",
                            SmoSize.Word => _useIntel ? " word" : "w",
                            SmoSize.Double => _useIntel ? " dword" : "l",
                            SmoSize.Quad => _useIntel ? " qword" : "q",
                            _ => throw new NotImplementedException(),
                        };
                }
                else if (instruction.Opcode == IntelMnemonics.MOVZX || instruction.Opcode == IntelMnemonics.MOVSX)
                {
                    var dst = instruction.Operands[0];
                    var src = instruction.Operands[1];
                    mnemonic = mnemonic[0..(mnemonic.Length - 1)];
                    mnemonic += src.Size switch
                    {
                        SmoSize.Byte => "b",
                        SmoSize.Word => "w",
                        SmoSize.Double => "l",
                        SmoSize.Quad => "q",
                        _ => throw new NotImplementedException(),
                    };
                    mnemonic += dst.Size switch
                    {
                        SmoSize.Byte => "b",
                        SmoSize.Word => "w",
                        SmoSize.Double => "l",
                        SmoSize.Quad => "q",
                        _ => throw new NotImplementedException(),
                    };
                    operands = _useIntel ? $"{array[0]}, {array[1]}" : $"{array[1]},{array[0]}";
                }
                else if (instruction.Opcode == IntelMnemonics.ENTER)
                {
                    operands = _useIntel ? $"{array[0]}, {array[1]}" : $"{array[0]},{array[1]}";
                }
                else
                {
                    var dst = instruction.Operands[0];
                    var src = instruction.Operands[1];
                    operands = _useIntel ? $"{array[0]}, {array[1]}" : $"{array[1]},{array[0]}";
                    if (dst.Type != SmoOperandType.Register && src.Type != SmoOperandType.Register)
                        mnemonic += dst.Size switch
                        {
                            SmoSize.Byte => _useIntel ? " byte" : "b",
                            SmoSize.Word => _useIntel ? " word" : "w",
                            SmoSize.Double => _useIntel ? " dword" : "l",
                            SmoSize.Quad => _useIntel ? " qword" : "q",
                            _ => throw new NotImplementedException(),
                        };
                }
            }
            else if (instruction.Order == SmoOrder.UnaryD)
            {
                var dst = instruction.Operands[0];
                operands = array[0];
                if (dst.Type != SmoOperandType.Register && dst.Type != SmoOperandType.Value && dst.Type != SmoOperandType.NearRelativeJmp && 
                    instruction.Opcode != "INVLPG")
                    mnemonic += dst.Size switch
                    {
                        SmoSize.Byte => _useIntel ? " byte" : "b",
                        SmoSize.Word => _useIntel ? " word" : "w",
                        SmoSize.Double => _useIntel ? " dword" : "l",
                        SmoSize.Quad => _useIntel ? " qword" : "q",
                        SmoSize.FSingle => _useIntel ? " -?" : "t",
                        SmoSize.FDouble => _useIntel ? " -?" : "l",
                        _ => throw new NotImplementedException(),
                    };
            }
            else if (instruction.Order == SmoOrder.UnaryCD)
            {
                var dst = instruction.Operands[1];
                mnemonic = mnemonic.Substring(0, mnemonic.Length - 2) + array[0];
                operands = array[1];
                if (dst.Type != SmoOperandType.Register && dst.Type != SmoOperandType.Value && dst.Type != SmoOperandType.NearRelativeJmp && 
                    instruction.Opcode != "SETcc")
                    mnemonic += dst.Size switch
                    {
                        SmoSize.Byte => _useIntel ? " byte" : "b",
                        SmoSize.Word => _useIntel ? " word" : "w",
                        SmoSize.Double => _useIntel ? " dword" : "l",
                        SmoSize.Quad => _useIntel ? " qword" : "q",
                        _ => throw new NotImplementedException(),
                    };
            }
            else if (instruction.Order == SmoOrder.TernaryDSS)
            {
                operands = _useIntel ? $"{array[0]} {array[1]} {array[2]}" : $"{array[2]},{array[1]},{array[0]}";
            }
            else if (instruction.Order == SmoOrder.BinaryFJ)
            {
                var dst = instruction.Operands[0];
                var src = instruction.Operands[1];
                operands = _useIntel ? $"{array[1]} {array[2]}" : $"{array[1]},{array[0]}";
                if (dst.Size != SmoSize.Double)
                    mnemonic += dst.Size switch
                    {
                        SmoSize.Word => _useIntel ? " word" : "w",
                        SmoSize.Double => _useIntel ? " dword" : "l",
                        SmoSize.Quad => _useIntel ? " qword" : "q",
                        _ => throw new NotImplementedException(),
                    };
            }
            else if (instruction.Order != SmoOrder.NoOperand)
            {
                throw new NotImplementedException();
            }


            return _useIntel ? $"{prefix}{mnemonic} {operands}" : $"{prefix + mnemonic,-6} {operands}";
        }

        private string WriteOperand(SmoOperand operand, long offset, int size)
        {
            var prefix = string.Empty;
            if (!string.IsNullOrEmpty(operand.Prefix))
                prefix = operand.Prefix == "*" ? "*" : $"%{operand.Prefix}:";


            if (operand.Type == SmoOperandType.Register)
                return prefix + (_useIntel ? "" : "%") + WriteRegister(operand.Size, operand.Value);
            if (operand.Type == SmoOperandType.Memory)
                return prefix + string.Format(_useIntel ? "[{0}]" : "(%{0})", WriteRegister(SmoSize.Double, operand.Value));
            if (operand.Type == SmoOperandType.MemoryOffset)
                return prefix + string.Format(_useIntel ? "[{0} {2} 0x{1:x}]" : "{2}0x{1:x}(%{0})", WriteRegister(SmoSize.Double, operand.Value), Math.Abs(operand.Value2), operand.Value2 < 0 ? "-" : (_useIntel ? "+" : ""));
            if (operand.Type == SmoOperandType.Value)
            {
                if (operand.Size == SmoSize.Byte)
                    return (_useIntel ? "" : "$") + $"0x{(byte)operand.Value:x}";
                if (operand.Size == SmoSize.Word)
                    return (_useIntel ? "" : "$") + $"0x{(ushort)operand.Value:x}";
                if (operand.Size == SmoSize.Double)
                    return (_useIntel ? "" : "$") + $"0x{(uint)operand.Value:x}";
                return (_useIntel ? "" : "$") + $"0x{operand.Value:x}";
            }
            if (operand.Type == SmoOperandType.Address)
                return prefix + string.Format(_useIntel ? "[0x{0:x}]" : "0x{0:x}", operand.Value);

            if (operand.Type == SmoOperandType.Condition)
                return ((SmoJcc)operand.Value).ToString().Substring(1).ToLower();

            if (operand.Type == SmoOperandType.NearRelativeJmp)
                return (_useIntel ? "0x" : "") + $"{(operand.Value + offset + size):x}";
            //if (operand.Type == SmoOperandType.FarRegJump)
            //    return (_useIntel ? "" : "*%") + WriteRegister(operand.Size, operand.Value);
            if (operand.Type == SmoOperandType.LongJmp)
                return (_useIntel ? "0x" : "") + $"{operand.Value:x}";


            if (operand.Type == SmoOperandType.Sib1)
                return prefix + string.Format(_useIntel ? "[{0} * {2} * {1} {4} 0x{3:x}]" : "{4}0x{3:x}(%{2},%{0},{1})", 
                    WriteRegister(SmoSize.Double, operand.Value), operand.Value2, WriteRegister(SmoSize.Double, operand.Value3), Math.Abs(operand.Value4), operand.Value4 < 0 ? "-" : (_useIntel ? "+" : ""));
            // -0x585c(%eax,%edx,4)
            if (operand.Type == SmoOperandType.Sib2)
                return prefix + string.Format(_useIntel ? "[{0} * {1} {3} 0x{2:x}]" : "{3}0x{2:x}(,%{0},{1})", 
                    WriteRegister(SmoSize.Double, operand.Value), operand.Value2, Math.Abs(operand.Value3), operand.Value3 < 0 ? "-" : (_useIntel ? "+" : ""));
            
            if (operand.Type == SmoOperandType.Sib3) // (%edx,%eax,1)
                return prefix + string.Format(_useIntel ? "[{0} * {1} * {2}]" : "(%{2},%{0},{1})", 
                    WriteRegister(SmoSize.Double, operand.Value), operand.Value2, WriteRegister(SmoSize.Double, operand.Value3));

            throw new NotImplementedException();
        }

        private string WriteRegister(SmoSize size, long value)
        {
            if (size == SmoSize.Byte && value < 8)
                return regGen8[value];
            if (size == SmoSize.Word && value < 8)
                return regGen16[value];
            if (size == SmoSize.Double && value < 8)
                return regGen32[value];
            if (size == SmoSize.Quad && value < 8)
                return regGen64[value];

            if (size == SmoSize.Word && value >= 8 && value < 8 + 6)
                return regSeg16[value - 8];

            if (size == SmoSize.Double && value >= 16 && value < 16 + 5)
                return regCtrl32[value - 16];
            if (size == SmoSize.Double && value >= 24 && value < 24 + 8)
                return regDbg32[value - 24];

            if (value == 32)
                return "dx";

            throw new NotImplementedException();
        }


        public void Display(TextWriter output, SmoInstruction op, ElfFile elf, long offset, byte[] buf)
        {

            var sym = elf.SearchSymbolAt(offset);
            if (sym != null)
            {
                elf.Output.WriteLine($"\n{offset:x8} <{sym.Name}>:");
            }

            if (op == null)
            {
                var cod = buf.ToHexArray();
                elf.Output.WriteLine($"{offset,8:x}:\t{cod,-15}  \t(bad)");
                return;
            }

            var txt = Stringify(op, offset, buf.Length, buf);
            var smn = "";
            if (op.Operands != null && op.Operands.Length != 0 && op.Operands.Last().Type == SmoOperandType.NearRelativeJmp)
            {
                var off = op.Operands.Last().Value + offset + (uint)buf.Length;
                var prv = elf.SearchSymbolBefore(off);
                if (prv != null)
                {
                    smn = $" <{prv.Name}>";
                    if (off > prv.Offset)
                        smn = $" <{prv.Name}+0x{(off - prv.Offset):x}>";
                }
            }
            if (buf.Length < 8)
            {
                var cod = buf.ToHexArray();
                output.WriteLine($"{offset,8:x}:\t{cod,-15}  \t{txt}{smn}");
            }
            else
            {
                var cod1 = buf[0..7].ToHexArray();
                var cod2 = buf[7..].ToHexArray();
                output.WriteLine($"{offset,8:x}:\t{cod1,-15}  \t{txt}");
                output.WriteLine($"{(offset + 7),8:x}:\t{cod2,-15}");
            }
        }

    }

}
