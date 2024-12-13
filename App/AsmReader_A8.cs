using System.Text;
using System.Text.RegularExpressions;

namespace App
{
    public class AsmReader_A8
    {

        // Matches: "label: INSTRUCTION (["')OPERAND1(]"'), (["')OPERAND2(]"')
        // GROUPS:      1       2               3                    7
        static readonly Regex rdLbl = new Regex("^\\s*([.A-Za-z]\\w*):");
        static readonly Regex rdIns = new Regex("^\\s*([A-Za-z]{2,4})(\\s+|$)");
        static readonly Regex rdPrm = new Regex("^\\s*(\\[(\\w+((\\+|-)\\d+)?)\\]|\\\".+?\\\"|\\'.+?\\'|[.A-Za-z0-9]\\w*)");
        static readonly Regex regexIns = new Regex("^\\s *(?:([.A-Za-z]\\w*)[:]\\s*)?(([A-Za-z]{2,4})(?:\\s+(\\[(\\w+((\\+|-)\\d+)?)\\]|\\\".+?\\\"|\\'.+?\\'|[.A-Za-z0-9]\\w*)(?:\\s*[,]\\s*(\\[(\\w+((\\+|-)\\d+)?)\\]|\\\".+?\\\"|\\'.+?\\'|[.A-Za-z0-9]\\w*))?)?)?");
        static readonly Regex regexNum = new Regex("^[-+]?[0-9]+$"); // MATCHES: "(+|-)INTEGER"
        static readonly Regex regexLbl = new Regex("^[.A-Za-z]\\w*$"); // MATCHES: "(.L)abel"


        public void ReadLines(string[] lines)
        {
            for (int i = 0; i < lines.Length; ++i)
                ReadLine(lines[i], i + 1);
        }

        public bool Relloc()
        {
            int missing = 0;
            foreach (var rel in _reloc)
            {
                if (!_labels.ContainsKey(rel.Value))
                {
                    missing++;
                    continue;
                }

                long offset = _labels[rel.Value];
                _code.Seek(rel.Key, SeekOrigin.Begin);
                _code.WriteByte((byte)offset);
            }
            return missing == 0;
        }

        public byte[] ToBuffer()
        {
            _code.Seek(0, SeekOrigin.Begin);
            var buf = new byte[_code.Length];
            _code.Read(buf);
            return buf;
        }

        public void ReadLine(string line, int row)
        {
            const int P1 = 3;
            const int P2 = 7;

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart()[0] == ';')
                return;

            Match match = null;

            match = rdLbl.Match(line);
            if (match.Success && match.Length != 0)
            {
                WriteLabel(match.Groups[1].Value);
                line = line.Substring(match.Length).Trim();
            }

            if (string.IsNullOrWhiteSpace(line) || line.TrimStart()[0] == ';')
                return;

            match = rdIns.Match(line);
            if (!match.Success || match.Length == 0)
                throw new Exception($"Syntax error at line {row}");

            if (match.Groups[1].Value.ToUpper() == "DB")
            {
                line = line.Substring(match.Length).Trim();
                while (line.Length > 0)
                {
                    match = rdPrm.Match(line);
                    if (!match.Success || match.Length == 0)
                        throw new Exception($"Syntax error at line {row}");
                    ParseData(match.Groups[1].Value);
                    line = line.Substring(match.Length).Trim();
                    if (string.IsNullOrWhiteSpace(line) || line[0] == ';')
                        break;
                    else if (line[0] == ',')
                        line = line[1..];
                    else
                        throw new Exception($"Syntax error at line {row}");
                }
                return;
            }

            OpcodeMnemonic op;
            Jcc jcc = Jcc.Je;
            var operands = new List<AsmOperand>();

            if (Enum.TryParse<Jcc>(match.Groups[1].Value, true, out jcc))
            {
                op = OpcodeMnemonic.Jcc;
                operands.Add(new AsmOperand
                {
                    Type = OperandType.Special,
                    Value = (ulong)jcc,
                });
            }
            else
                op = Enum.Parse<OpcodeMnemonic>(match.Groups[1].Value, true);
            line = line.Substring(match.Length).Trim();

            while (line.Length > 0 && line[0] != ';')
            {
                match = rdPrm.Match(line);
                if (!match.Success || match.Length == 0)
                    throw new Exception($"Syntax error at line {row}");
                AsmOperand? param = ParseOperand(match.Groups[1].Value);
                if (param == null)
                    throw new Exception($"Syntax error at line {row}");
                operands.Add(param);
                line = line.Substring(match.Length).Trim();
                if (string.IsNullOrWhiteSpace(line) || line[0] == ';')
                    break;
                else if (line[0] == ',')
                    line = line[1..];
                else
                    throw new Exception($"Syntax error at line {row}");
            }

            Asm8B opcode = Asm8B.NONE;
            AsmOperand? p1 = operands.Count > 0 ? operands[0] : null;
            AsmOperand? p2 = operands.Count > 1 ? operands[1] : null;
            // AsmOperand? p3 = operands.Count > 2 ? operands[2] : null;
            switch (op)
            {
                case OpcodeMnemonic.Mov:
                    if (operands.Count != 2)
                        throw new Exception();
                    else if (p1.Type == OperandType.Register && p2.Type == OperandType.Register)
                        opcode = Asm8B.MOV_REG_TO_REG;
                    else if (p1.Type == OperandType.Register && p2.Type == OperandType.Memory)
                        opcode = Asm8B.MOV_REGADDRESS_TO_REG;
                    else if (p1.Type == OperandType.Register && (p2.Type == OperandType.Address || p2.Type == OperandType.MAddRef))
                        opcode = Asm8B.MOV_ADDRESS_TO_REG;
                    else if (p1.Type == OperandType.Register && (p2.Type == OperandType.Value || p2.Type == OperandType.VAddRef))
                        opcode = Asm8B.MOV_NUMBER_TO_REG;
                    else if (p1.Type == OperandType.Memory && p2.Type == OperandType.Register)
                        opcode = Asm8B.MOV_REG_TO_REGADDRESS;
                    else if ((p1.Type == OperandType.Address || p1.Type == OperandType.MAddRef) && p2.Type == OperandType.Register)
                        opcode = Asm8B.MOV_REG_TO_ADDRESS;
                    else if (p1.Type == OperandType.Memory && (p2.Type == OperandType.Value || p2.Type == OperandType.VAddRef))
                        opcode = Asm8B.MOV_NUMBER_TO_REGADDRESS;
                    else if ((p1.Type == OperandType.Address || p1.Type == OperandType.MAddRef) && (p2.Type == OperandType.Value || p2.Type == OperandType.VAddRef))
                        opcode = Asm8B.MOV_NUMBER_TO_ADDRESS;
                    else
                        throw new Exception($"{op} does not support those operands");
                    break;
                case OpcodeMnemonic.Add:
                case OpcodeMnemonic.Sub:
                case OpcodeMnemonic.And:
                case OpcodeMnemonic.Or:
                case OpcodeMnemonic.Xor:
                case OpcodeMnemonic.Cmp:
                case OpcodeMnemonic.Shl:
                case OpcodeMnemonic.Shr:
                case OpcodeMnemonic.Sal:
                case OpcodeMnemonic.Sar:
                    if (operands.Count != 2)
                        throw new Exception();
                    else if (p1.Type == OperandType.Register && p2.Type == OperandType.Register)
                        opcode = Asm8B.ADD_REG_TO_REG;
                    else if (p1.Type == OperandType.Register && p2.Type == OperandType.Memory)
                        opcode = Asm8B.ADD_REGADDRESS_TO_REG;
                    else if (p1.Type == OperandType.Register && (p2.Type == OperandType.Address || p2.Type == OperandType.MAddRef))
                        opcode = Asm8B.ADD_ADDRESS_TO_REG;
                    else if (p1.Type == OperandType.Register && (p2.Type == OperandType.Value || p2.Type == OperandType.VAddRef))
                        opcode = Asm8B.ADD_NUMBER_TO_REG;
                    else
                        throw new Exception($"{op} does not support those operands");
                    switch (op)
                    {
                        case OpcodeMnemonic.Sub:
                            opcode += Asm8B.SUB_REG_FROM_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.And:
                            opcode += Asm8B.AND_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.Or:
                            opcode += Asm8B.OR_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.Xor:
                            opcode += Asm8B.XOR_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.Cmp:
                            opcode += Asm8B.CMP_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.Shl:
                            opcode += Asm8B.SHL_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.Shr:
                            opcode += Asm8B.SHR_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.Sal:
                            opcode += Asm8B.SHL_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                        case OpcodeMnemonic.Sar:
                            opcode += Asm8B.SHR_REG_WITH_REG - Asm8B.ADD_REG_TO_REG;
                            break;
                    }
                    break;
                case OpcodeMnemonic.Inc:
                case OpcodeMnemonic.Dec:
                    if (operands.Count != 1)
                        throw new Exception();
                    if (p1.Type == OperandType.Register)
                        opcode = op == OpcodeMnemonic.Inc ? Asm8B.INC_REG : Asm8B.DEC_REG;
                    else
                        throw new Exception($"{op} does not support those operands");
                    break;
                case OpcodeMnemonic.Jmp:
                    if (operands.Count != 1)
                        throw new Exception();
                    if (p1.Type == OperandType.Register)
                        opcode = Asm8B.JMP_REGADDRESS;
                    else if (p1.Type == OperandType.Value || p1.Type == OperandType.VAddRef)
                        opcode = Asm8B.JMP_ADDRESS;
                    else
                        throw new Exception($"{op} does not support those operands");
                    break;
                case OpcodeMnemonic.Jcc:
                    if (operands.Count != 2 || operands[0].Type != OperandType.Special)
                        throw new Exception();
                    if (operands[1].Type != OperandType.Register && operands[1].Type != OperandType.Value && operands[1].Type != OperandType.VAddRef)
                        throw new Exception($"{op} does not support those operands");
                    opcode = (Jcc)operands[0].Value switch
                    {
                        Jcc.Ja => operands[1].Type == OperandType.Register ? Asm8B.JA_REGADDRESS : Asm8B.JA_ADDRESS,
                        Jcc.Jnbe => operands[1].Type == OperandType.Register ? Asm8B.JA_REGADDRESS : Asm8B.JA_ADDRESS,
                        Jcc.Jae => operands[1].Type == OperandType.Register ? Asm8B.JNC_REGADDRESS : Asm8B.JNC_ADDRESS,
                        Jcc.Jnb => operands[1].Type == OperandType.Register ? Asm8B.JNC_REGADDRESS : Asm8B.JNC_ADDRESS,
                        Jcc.Jb => operands[1].Type == OperandType.Register ? Asm8B.JC_REGADDRESS : Asm8B.JC_ADDRESS,
                        Jcc.Jnae => operands[1].Type == OperandType.Register ? Asm8B.JC_REGADDRESS : Asm8B.JC_ADDRESS,
                        Jcc.Jbe => operands[1].Type == OperandType.Register ? Asm8B.JNA_REGADDRESS : Asm8B.JNA_ADDRESS,
                        Jcc.Jna => operands[1].Type == OperandType.Register ? Asm8B.JNA_REGADDRESS : Asm8B.JNA_ADDRESS,
                        Jcc.Jc => operands[1].Type == OperandType.Register ? Asm8B.JC_REGADDRESS : Asm8B.JC_ADDRESS,
                        Jcc.Je => operands[1].Type == OperandType.Register ? Asm8B.JZ_REGADDRESS : Asm8B.JZ_ADDRESS,
                        Jcc.Jz => operands[1].Type == OperandType.Register ? Asm8B.JZ_REGADDRESS : Asm8B.JZ_ADDRESS,
                        Jcc.Jnc => operands[1].Type == OperandType.Register ? Asm8B.JNC_REGADDRESS : Asm8B.JNC_ADDRESS,
                        Jcc.Jne => operands[1].Type == OperandType.Register ? Asm8B.JNZ_REGADDRESS : Asm8B.JNZ_ADDRESS,
                        Jcc.Jnz => operands[1].Type == OperandType.Register ? Asm8B.JNZ_REGADDRESS : Asm8B.JNZ_ADDRESS,
                        _ => throw new NotImplementedException(),
                    };
                    PushCode(new byte[] { (byte)opcode, OperandByte(p2) });
                    if (p2.Type == OperandType.VAddRef)
                        AddReloc(-1, p2.Note);
                    return;
                case OpcodeMnemonic.Push:
                    if (operands.Count != 1)
                        throw new Exception();
                    if (p1.Type == OperandType.Register)
                        opcode = Asm8B.PUSH_REG;
                    else if (p1.Type == OperandType.Memory)
                        opcode = Asm8B.PUSH_REGADDRESS;
                    else if (p1.Type == OperandType.Address || p1.Type == OperandType.MAddRef)
                        opcode = Asm8B.PUSH_NUMBER;
                    else if (p1.Type == OperandType.Value || p1.Type == OperandType.VAddRef)
                        opcode = Asm8B.PUSH_NUMBER;
                    else
                        throw new Exception($"{op} does not support those operands");
                    break;
                case OpcodeMnemonic.Pop:
                    if (operands.Count != 1)
                        throw new Exception();
                    if (p1.Type != OperandType.Register)
                        throw new Exception($"{op} does not support those operands");
                    opcode = Asm8B.POP_REG;
                    break;
                case OpcodeMnemonic.Call:
                    if (operands.Count != 1)
                        throw new Exception();
                    if (p1.Type == OperandType.Register)
                        opcode = Asm8B.CALL_REGADDRESS;
                    else if (p1.Type == OperandType.Value || p1.Type == OperandType.VAddRef)
                        opcode = Asm8B.CALL_ADDRESS;
                    else
                        throw new Exception($"{op} does not support those operands");
                    break;
                case OpcodeMnemonic.Ret:
                    if (operands.Count != 0)
                        throw new Exception();
                    opcode = Asm8B.RET;
                    break;
                case OpcodeMnemonic.Hlt:
                    if (operands.Count != 0)
                        throw new Exception();
                    opcode = Asm8B.NONE;
                    break;
                case OpcodeMnemonic.Mul:
                case OpcodeMnemonic.Div:
                default:
                    throw new Exception($"Unsupported opcode {op}");
            }

            if (operands.Count == 0)
                PushCode(new byte[] { (byte)opcode });
            else if (operands.Count == 1)
                PushCode(new byte[] { (byte)opcode, OperandByte(p1) });
            else if (operands.Count == 2)
                PushCode(new byte[] { (byte)opcode, OperandByte(p1), OperandByte(p2) });
            if (p1 != null && (p1.Type == OperandType.VAddRef || p1.Type == OperandType.MAddRef))
                AddReloc(p2 != null ? -2 : -1, p1.Note);
            if (p2 != null && (p2.Type == OperandType.VAddRef || p2.Type == OperandType.MAddRef))
                AddReloc(-1, p2.Note);
        }

        private byte OperandByte(AsmOperand operand)
        {
            return operand.Type switch
            {
                OperandType.Register => (byte)operand.Value,
                OperandType.Address => (byte)operand.Value,
                OperandType.Memory => (byte)(operand.Value + operand.Value2 * 8),
                OperandType.Value => (byte)operand.Value,
                OperandType.VAddRef => 0,
                OperandType.MAddRef => 0,
                _ => throw new Exception()
            };
        }

        private void ParseData(string input)
        {
            input = input.Trim();
            if (input[0] == '"')
            { // "String"
                int idx = input.IndexOf('"', 1);
                if (idx < 0) // input[input.Length - 1] != input[0])
                    throw new Exception();
                input = input[1..idx];
                PushCode(Encoding.ASCII.GetBytes(input));
            }
            else if (input[0] == '\'')
            { // 'C'
                if (input[2] != input[0])
                    throw new Exception();
                //if (input.Length != 3)
                //    throw new Exception("Only one character is allowed. Use String instead");
                PushCode(new byte[] { (byte)input[1] });
            }
            else
            { // NUMBER
                int val = ParseNumber(input);
                if (val < -128 || val > 127)
                    throw new Exception();
                PushCode(new byte[] { (byte)val });
            }
        }

        private Stream _code = new MemoryStream();
        private Dictionary<string, long> _labels = new Dictionary<string, long>();
        private Dictionary<long, string> _reloc = new Dictionary<long, string>();
        protected void PushCode(byte[] code) => _code.Write(code);

        protected long WriteLabel(string label) => _labels[label] = _code.Length;

        protected void AddReloc(int offset, string label, int type = 0)
            => _reloc[_code.Length + offset] = label;


        // Allowed formats: 200, 200d, 0xA4, a4h, 048, 48o, 101b
        private int ParseNumber(string input)
        {
            if (input.StartsWith("0x"))
                return Convert.ToInt32(input[2..], 16);
            else if (input.EndsWith("h"))
                return Convert.ToInt32(input[..-1], 16);
            else if (input.StartsWith("0"))
                return Convert.ToInt32(input, 8);
            else if (input.EndsWith("o"))
                return Convert.ToInt32(input[..-1], 8);
            else if (input.EndsWith("b"))
                return Convert.ToInt32(input[..-1], 1);
            else if (input.EndsWith("d"))
                return Convert.ToInt32(input[..-1], 10);
            return Convert.ToInt32(input, 10);
        }

        private AsmOperand? ParseRegister(string input)
        {
            input = input.Trim().ToUpper();
            int reg = -1, sign = 0;
            if (input.StartsWith("A"))
                reg = 0;
            else if (input.StartsWith("B"))
                reg = 1;
            else if (input.StartsWith("C"))
                reg = 2;
            else if (input.StartsWith("D"))
                reg = 3;
            else if (input.StartsWith("SP"))
                reg = 4;
            else
                return null;

            if (input.Length != 1)
            {
                input = input.Substring(input.Length == 1 || input[1] == 'X' || input[1] == 'P' ? 2 : 1).TrimStart();
                if (input.Length != 0)
                    return null;
            }

            return new AsmOperand
            {
                Type = OperandType.Register,
                Word = WordSize.Byte,
                Value = (ulong)reg,
                Value2 = 0,
            };
        }

        private AsmOperand? ParseOffsetAddressing(string input)
        {
            input = input.Trim().ToUpper();
            int reg = -1, sign = 0;
            if (input.StartsWith("A"))
                reg = 0;
            else if (input.StartsWith("B"))
                reg = 1;
            else if (input.StartsWith("C"))
                reg = 2;
            else if (input.StartsWith("D"))
                reg = 3;
            else if (input.StartsWith("SP"))
                reg = 4;
            else
                return null;

            input = input[1..];
            if (input.Length > 0)
                input = input.Substring(input[0] == 'X' || input[0] == 'P' ? 2 : 1).TrimStart();
            if (input.Length == 0)
                return new AsmOperand
                {
                    Type = OperandType.Memory,
                    Word = WordSize.Byte,
                    Value = (ulong)reg,
                    Value2 = 0,
                };

            if (input[0] == '-')
                sign = -1;
            else if (input[0] == '+')
                sign = 1;
            else
                return null;

            input = input[1..].TrimStart();
            int offset = sign * ParseNumber(input);

            if (offset < -16 || offset > 15)
                throw new Exception();
            return new AsmOperand
            {
                Type = OperandType.Memory,
                Word = WordSize.Byte,
                Value = (ulong)reg,
                Value2 = (ulong)offset,
            };
        }

        private AsmOperand? ParseRegOrNumber(string input, bool memory)
        {
            var reg = memory ? ParseOffsetAddressing(input) : ParseRegister(input);
            if (reg != null)
                return reg;

            if (regexLbl.IsMatch(input))
                return new AsmOperand
                {
                    Type = memory ? OperandType.MAddRef : OperandType.VAddRef,
                    Word = WordSize.Byte,
                    Value = (ulong)0,
                    Note = input,
                };

            int value = ParseNumber(input);
            if (memory && value != (byte)value)
                throw new Exception();
            if (!memory && value != (byte)value)
                throw new Exception();
            return new AsmOperand
            {
                Type = memory ? OperandType.Address : OperandType.Value,
                Word = WordSize.Byte,
                Value = (ulong)value,
            };
        }

        private AsmOperand? ParseOperand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;
            input = input.Trim();
            if (input[0] == '[')
            { // [number] or [register]
                if (input[input.Length - 1] != ']')
                    throw new Exception();
                return ParseRegOrNumber(input[1..(input.Length - 1)], true);
            }
            else if (input[0] == '"')
            { // "String"
                if (input[input.Length - 1] != input[0])
                    throw new Exception();
                input = input[1..(input.Length - 2)];
                throw new Exception();
            }
            else if (input[0] == '\'')
            { // 'C'
                if (input[input.Length - 1] != input[0])
                    throw new Exception();
                if (input.Length != 3)
                    throw new Exception("Only one character is allowed. Use String instead");
                return new AsmOperand
                {
                    Type = OperandType.Value,
                    Word = WordSize.Byte,
                    Value = input[1],
                };
            }
            else
            { // REGISTER, NUMBER or LABEL
                return ParseRegOrNumber(input, false);
            }
        }
    }
}
