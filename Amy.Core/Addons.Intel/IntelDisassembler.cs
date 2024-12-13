using Amy.Core.Bytes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Addons.Intel
{
    public class IntelDisassembler : IDisassembler
    {

        private static string?[] tableMnemonic = new string?[]
        {    
            // 0x00
            "ADD Eb, Gb", "ADD Ev, Gv", "ADD Gb, Eb", "ADD Gv, Ev",
            "ADD AL, lb", "ADD rAX, lz", "PUSH es", "POP es",

            "OR Eb, Gb", "OR Ev, Gv", "OR Gb, Eb", "OR Gv, Ev",
            "OR AL, lb", "OR rAX, lz", "PUSH cs", null,

            "ADC Eb, Gb", "ADC Ev, Gv", "ADC Gb, Eb", "ADC Gv, Ev",
            "ADC AL, lb", "ADC rAX, lz", "PUSH ss", "POP ss",

            "SBB Eb, Gb", "SBB Ev, Gv", "SBB Gb, Eb", "SBB Gv, Ev",
            "SBB AL, lb", "SBB rAX, lz", "PUSH ds", "POP ds",

            // 0x20
            "AND Eb, Gb", "AND Ev, Gv", "AND Gb, Eb", "AND Gv, Ev",
            "AND AL, lb", "AND rAX, lz", ":es", "DAA",

            "SUB Eb, Gb", "SUB Ev, Gv", "SUB Gb, Eb", "SUB Gv, Ev",
            "SUB AL, lb", "SUB rAX, lz", ":cs", "DAS",

            "XOR Eb, Gb", "XOR Ev, Gv", "XOR Gb, Eb", "XOR Gv, Ev",
            "XOR AL, lb", "XOR rAX, lz", ":ss", "AAA",

            "CMP Eb, Gb", "CMP Ev, Gv", "CMP Gb, Eb", "CMP Gv, Ev",
            "CMP AL, lb", "CMP rAX, lz", ":ds", "AAS",

            // 0x40
            "INC rAX", "INC rCX", "INC rDX", "INC rBX",
            "INC rSP", "INC rBP", "INC rSI", "INC rDI",

            "DEC rAX", "DEC rCX", "DEC rDX", "DEC rBX",
            "DEC rSP", "DEC rBP", "DEC rSI", "DEC rDI",

            // 0x50
            "PUSH rAX", "PUSH rCX", "PUSH rDX", "PUSH rBX",
            "PUSH rSP", "PUSH rBP", "PUSH rSI", "PUSH rDI",

            "POP rAX", "POP rCX", "POP rDX", "POP rBX",
            "POP rSP", "POP rBP", "POP rSI", "POP rDI",

            // 0x60
            "PUSHA", "POPA", "BOUND Gv, Ma", null,
            ":fs", ":gs", null, null,

            "PUSH lz", "IMUL Gv, Ev, lz", "PUSH lb", "IMUL Gv, Ev, lb",
            "INS Yb, DX", "INS Yz, DX", "OUTS DX, Yb", "OUTS DX, Yz",

            // 0x70
            "JO Jb", "JNO Jb", "JB Jb", "JAE Jb", "JE Jb", "JNE Jb", "JBE Jb", "JA Jb",
            "JS Jb", "JNS Jb", "JP Jb", "JNP Jb", "JL Jb", "JGE Jb", "JLE Jb", "JG Jb",

            // 0x80
            null, null, null, null,
            "TEST Eb, Gb", "TEST Ev, Gv", "XCHG Eb, Gb", "XCHG Ev, Gv",
            "MOV Eb, Gb", "MOV Ev, Gv", "MOV Gb, Eb", "MOV Gv, Ev",
            "MOV Ev, Sw", "LEA Gv, M", "MOV Sw, Ev", null,

            // 0x90
            "NOP", "XCHG rCX, rAX", "XCHG rDX, rAX", "XCHG rBX, rAX",
            "XCHG rSP, rAX", "XCHG rBP, rAX", "XCHG rSI, rAX", "XCHG rDI, rAX",
            "CLTB", "CLTD", "LCALL Ap", "FWAIT", 
            "PUSHF", "POPF", "SAHF", "LAHF",

            // 0xA0
            "MOV AL, Ob", "MOV rAX, Ov", "MOV Ob, AL", "MOV Ov, rAX",
            "MOVS Yb, Xb", "MOVS Yv, Xv", "CMPS Yb, Xb", "CMPS Yv, Xv",
            "TEST AL, lb", "TEST rAX, lz", "STOS Yb, AL", "STOS Yv, rAX", 
            "LODS AL, Xb", "LODS rAX, Xv", "SCAS AL, Yb", "SCAS rAX, Yv",

            // 0xB0
            "MOV AL, lb", "MOV CL, lb", "MOV DL, lb", "MOV BL, lb",
            "MOV AH, lb", "MOV CH, lb", "MOV DH, lb", "MOV BH, lb",

            "MOV rAX, lv", "MOV rCX, lv", "MOV rDX, lv", "MOV rBX, lv",
            "MOV rSP, lv", "MOV rBP, lv", "MOV rSI, lv", "MOV rDI, lv",

            // 0xC0
            null, null, "RET lw", "RET", "LES Gz, Mp", "LDS Gz, Mp", null, null,
            "ENTER lw, lb", "LEAVE", "LRET lw", "LRET", "INT3", "INT lb", "INTO", "IRET",

            // 0xD0
            null, null, null, null, "AAM lb", "AAD lb", "(bad)", "XLAT",
            null, null, null, null, null, null, null, null,

            // 0xE0
            "LOOPNE Jb", "LOOPE Jb", "LOOP Jb", "JCXZ Jb", 
            "IN AL, lb", "IN rAX, lb", "OUT lb, AL", "OUT lb, rAX",
            "CALL Jz", "JMP Jz", "LJMP Ap", "JMP Jb",
            "IN AL, DX", "IN rAX, DX", "OUT DX, AL", "OUT DX, rAX",

            // 0xF0
            null, "(bad)", null, null, "HLT", "CMC", null, null,
            "CLC", "STC", "CLI", "STI", "CLD", "STD", null, null,
        };

        private static string[] tableMnemonicGrp1 = new string[]
        { "ADD", "OR", "ADC", "SBB", "AND", "SUB", "XOR", "CMP" };

        private static string?[] tableMnemonicGrp2 = new string?[]
        { "ROL", "ROR", "RCL", "RCR", "SHL", "SHR", "SHL", "SAR" };

        private static string?[] tableMnemonicGrp3 = new string?[]
        { "TEST", "TEST", "NOT", "NEG", "MUL", "IMUL", "DIV", "IDIV" };

        private static string?[] tableMnemonicGrp5 = new string?[]
        {  "INC", "DEC", "CALL", null, "JMP", null, "PUSH", null };


        private static string?[] tableMnemonic0F = new string?[]
        {  
            // 0x00
            null, null, "LAR Gv, Ew", "LSL Gv, Ew", null, "SYSCALL", "CLTS", "SYSRET",
            "INVD", "WBINVD", null, null, null, null, null, null,
            // 0x10
            "vmovups Vps, Wps", "vmovups Wps, Vps", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0x20
            "MOV Rd, Cd", "MOV Rd, Rd", "MOV Cd, Rd", "MOV Dd, Rd", null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0x30
            "WRMSR", "RDTSC", "RDMSR", "RDPMC", "SYSENTER", "SYSEXIT", null, "GETSEC",
            null, null, null, null, null, null, null, null,
            // 0x40
            "MOVO Jz", "MOVNO Jz", "MOVB Jz", "MOVNB Jz", "MOVE Jz", "MOVNE Jz", "MOVNA Jz", "MOVA Jz",
            "MOVS Jz", "MOVNS Jz", "MOVP Jz", "MOVNP Jz", "MOVL Jz", "MOVNL Jz", "MOVNG Jz", "MOVG Jz",
            // 0x50
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0x60
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0x70
            null, null, null, null, null, null, null, null,
            "VMREAD Ey, Gy", "VMWRITE Gy, Ey", null, null, null, null, null, null,
            // 0x80
            "JO Jz", "JNO Jz", "JB Jz", "JAE Jz", "JE Jz", "JNE Jz", "JBE Jz", "JA Jz",
            "JS Jz", "JNS Jz", "JP Jz", "JNP Jz", "JL Jz", "JGE Jz", "JLE Jz", "JG Jz",
            // 0x90
            "SETO Eb", "SETNO Eb", "SETC Eb", "SETAE Eb", "SETE Eb", "SETNE Eb", "SETBE Eb", "SETA Eb",
            "SETS Eb", "SETNS Eb", "SETP Eb", "SETNP Eb", "SETL Eb", "SETGE Eb", "SETLE Eb", "SETG Eb",
            // 0xa0
            "PUSH fs", "POP fs", "CPUID", "BT Ev, Gv", "SHLD Ev, Gv, lb", "SHLD Ev, Gv, CL", null, null,
            "PUSH gs", "POP gs", "RSM", "BTS Ev, Gv", "SHRD Ev, Gv, lb", "SHRD Ev, Gv, CL", null, "IMUL Gv, Ev",
            // 0xb0
            "CMPXCHG Eb, Gb", "CMPXCHG Ev, Gv", null, null, null, null, "MOVZX Gv, Eb", "MOVZX Gv, Ew",
            null, null, null, "BTC Ev, Gv", "BSF Gv, Ev", "BSR Gv, Ev", "MOVSX Gv, Eb", "MOVSX Gv, Ew",
            // 0xc0
            "XADD Eb, Gb", "XADD Ev, Gv", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0xd0
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0xe0
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0xf0
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
        };

        private static string?[] tableMnemonicGrp6 = new string?[]
        {  "SLDT Rv, Mw", "STR Rv, Mw", "LLDT Ew", "LTR Ew", "VERR Ew", "VERW Ew", null, null };

        private static string?[] tableMnemonicGrp7 = new string?[]
        {  "SGDT Ms", "SIDT Ms", "LGDT Ms", "LIDT Ms", "SMSW Mw, Rv", null, "LMSW Ew", "INVLPG Mb" };

        public SmoSize Wordsize { get; private set; } = SmoSize.Double;
        public SmoSize Datasize { get; private set; } = SmoSize.Double;


        public SmoInstruction NextInstruction(IByteReader reader)
        {
            reader.Continue();
            byte by = reader.Read8();

            // Check prefix
            if (by == 0xf0)
            {
                var op = ReadInstruction(reader, reader.Read8());
                op.Prefix = "lock";
                return op;
            }
            else if (by == 0x66)
            {
                Wordsize = SmoSize.Word;
                var op = ReadInstruction(reader, reader.Read8());
                Wordsize = SmoSize.Double;
                if (op.Opcode == IntelMnemonics.NOP)
                    return new SmoInstruction(IntelMnemonics.XCHG, SmoOrder.BinaryDS,
                        new SmoOperand(SmoOperandType.Register, SmoSize.Word, 0),
                        new SmoOperand(SmoOperandType.Register, SmoSize.Word, 0));
                if (op.Opcode == "CLTB")
                    return new SmoInstruction("CBTW", SmoOrder.NoOperand);
                return op;
            }
            else if (by == 0x67)
            {
                var op = ReadInstruction(reader, reader.Read8());
                op.Prefix = "W:";
                return op;
            }
            else if (by == 0xf2)
            {
                var op = ReadInstruction(reader, reader.Read8());
                op.Prefix = "repne";
                return op;
            }
            else if (by == 0xf3)
            {
                var op = ReadInstruction(reader, reader.Read8());
                if (op.Opcode == "NOP")
                    op.Opcode = "PAUSE";
                else
                    op.Prefix = "rep"; 
                return op;
            }

            return ReadInstruction(reader, by);
        }
        private SmoInstruction ReadInstruction(IByteReader reader, byte by)
        {
            var mnemonic = tableMnemonic[by];
            if (mnemonic != null)
                return ReadWithMnemonic(mnemonic, reader);
            else if (by == 0x0f)
                return ReadInstruction0F(reader, reader.Read8());
            else if (by == 0x63 || by == 0x66 || by == 0x67)
                throw new NotSupportedException();
            else if (by == 0x80)
                return ReadGroup1(SmoSize.Byte, SmoSize.Byte, reader);
            else if (by == 0x81)
                return ReadGroup1(Wordsize, Wordsize, reader);
            else if (by == 0x82)
                return ReadGroup1(SmoSize.Byte, SmoSize.Byte, reader);
            else if (by == 0x83)
                return ReadGroup1(Wordsize, SmoSize.Byte, reader);
            else if (by == 0x8f)
                throw new NotSupportedException();
            else if (by == 0xc0)
                return ReadGroup2(SmoSize.Byte, 0, reader);
            else if (by == 0xc1)
                return ReadGroup2(Wordsize, 0, reader);
            else if (by == 0xc6)
                return ReadGroupA(SmoSize.Byte, reader);
            else if (by == 0xc7)
                return ReadGroupA(Wordsize, reader);
            else if (by == 0xd0)
                return ReadGroup2(SmoSize.Byte, 1, reader);
            else if (by == 0xd1)
                return ReadGroup2(Wordsize, 1, reader);
            else if (by == 0xd2)
                return ReadGroup2(SmoSize.Byte, 2, reader);
            else if (by == 0xd3)
                return ReadGroup2(Wordsize, 2, reader);
            else if (by >= 0xd8 && by <= 0xdf)
                return ReadCoprocessor(by, reader);
            else if (by == 0xf0 || by == 0xf2 || by == 0xf3)
                throw new NotSupportedException();
            else if (by == 0xf6)
                return ReadGroup3(SmoSize.Byte, reader);
            else if (by == 0xf7)
                return ReadGroup3(Wordsize, reader);
            else if (by == 0xfe)
                return ReadGroup5(SmoSize.Byte, reader);
            else if (by == 0xff)
                return ReadGroup5(Wordsize, reader);

            throw new NotSupportedException();
        }

        private SmoInstruction ReadInstruction0F(IByteReader reader, byte by)
        {
            var mnemonic = tableMnemonic0F[by];
            if (mnemonic != null)
                return ReadWithMnemonic(mnemonic, reader);
            else if (by == 0x00)
                return ReadGroup6(reader);
            else if (by == 0x01)
                return ReadGroup7(reader);
            // 0x18 -> grp16
            // 0x71 -> grp12, 0x72 -> grp13, 0x73 ->grp14
            // 0xc7 -> grp9

            throw new NotSupportedException();
        }

        private SmoInstruction ReadWithMnemonic(string mnemonic, IByteReader reader, byte b2 = 0, bool readm = false)
        {
            if (mnemonic == "(bad)" || mnemonic == null)
                return null;

            int idx = mnemonic.IndexOf(' ');
            if (idx < 0)
                return new SmoInstruction(mnemonic);

            var operands = mnemonic.Substring(idx + 1).Trim();
            var mno = mnemonic.Substring(0, idx).Trim();

            if (operands == "Ap")
            {
                ulong addr = 0;
                if (Wordsize == SmoSize.Word)
                    addr = reader.Read16();
                else if (Wordsize == SmoSize.Double)
                    addr = reader.Read32();
                else if (Wordsize == SmoSize.Quad)
                    addr = reader.Read64();
                else
                    throw new Exception();
                return new SmoInstruction(mno, SmoOrder.BinaryFJ,
                    new SmoOperand(SmoOperandType.Value, Wordsize, (long)addr), 
                    new SmoOperand(SmoOperandType.Value, SmoSize.Word, reader.Read16()));
            }

            var arr = operands.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (!readm && arr.Any(x => x.StartsWith('E') || x == "M" || x.EndsWith('d')))
                b2 = reader.Read8();

            var oprds = arr.Select(x => x switch
            {
                "AL" => ReadAx(SmoSize.Byte, 0),
                "CL" => ReadAx(SmoSize.Byte, 1),
                "DL" => ReadAx(SmoSize.Byte, 2),
                "BL" => ReadAx(SmoSize.Byte, 3),
                "AH" => ReadAx(SmoSize.Byte, 4),
                "CH" => ReadAx(SmoSize.Byte, 5),
                "DH" => ReadAx(SmoSize.Byte, 6),
                "BH" => ReadAx(SmoSize.Byte, 7),
                "rAX" => ReadAx(Wordsize, 0),
                "rCX" => ReadAx(Wordsize, 1),
                "rDX" => ReadAx(Wordsize, 2),
                "rBX" => ReadAx(Wordsize, 3),
                "rSP" => ReadAx(Wordsize, 4),
                "rBP" => ReadAx(Wordsize, 5),
                "rSI" => ReadAx(Wordsize, 6),
                "rDI" => ReadAx(Wordsize, 7),
                "DX" => new SmoOperand(SmoOperandType.Memory, SmoSize.Word, REG_OUTPORT),
                // ReadAx(SmoSize.Word, 2, ),

                "ds" => ReadAx(SmoSize.Word, REG_SEGMENTS + 3),
                "es" => ReadAx(SmoSize.Word, REG_SEGMENTS + 0),
                "cs" => ReadAx(SmoSize.Word, REG_SEGMENTS + 1),
                "ss" => ReadAx(SmoSize.Word, REG_SEGMENTS + 2),
                "fs" => ReadAx(SmoSize.Word, REG_SEGMENTS + 4),
                "gs" => ReadAx(SmoSize.Word, REG_SEGMENTS + 5),

                "Cd" => ReadCx(Wordsize, b2, reader),
                "Dd" => ReadDx(Wordsize, b2, reader),

                "Eb" => ReadEx(SmoSize.Byte, b2, reader),
                "Ew" => ReadEx(SmoSize.Word, b2, reader),
                "Ev" => ReadEx(Wordsize, b2, reader),

                "Gb" => ReadGx(SmoSize.Byte, b2),
                "Gv" => ReadGx(Wordsize, b2),

                "Jb" => new SmoOperand(SmoOperandType.NearRelativeJmp, Datasize, ((sbyte)reader.Read8())),
                "Jz" => new SmoOperand(SmoOperandType.NearRelativeJmp, Datasize, ((int)reader.Read32())), // TODO 32 for 32bits !!?

                "M" => ((b2 >> 6) & 3) == 3 ? null : ReadEx(Wordsize, b2, reader),
                "Ms" => new SmoOperand(SmoOperandType.Address, Datasize, reader.Read32()),
                "Mb" => new SmoOperand(SmoOperandType.Memory, Datasize, REG_GENERALS + (b2 & 7)),
                "Mw" => new SmoOperand(SmoOperandType.Memory, Datasize, REG_GENERALS + (b2 & 7)),

                "Ob" => ReadImm(SmoSize.Byte, reader, SmoOperandType.Address),
                "Ov" => ReadImm(Wordsize, reader, SmoOperandType.Address),

                "Rd" => ReadRx(Wordsize, b2, reader),

                "Sw" => ReadSw(b2),

                "Xb" => new SmoOperand(SmoOperandType.Memory, SmoSize.Byte, REG_GENERALS + 6) { Prefix = "ds" },
                "Xv" => new SmoOperand(SmoOperandType.Memory, Wordsize, REG_GENERALS + 6) { Prefix = "ds" },
                "Yb" => new SmoOperand(SmoOperandType.Memory, SmoSize.Byte, REG_GENERALS + 7) { Prefix = "es" },
                "Yv" => new SmoOperand(SmoOperandType.Memory, Wordsize, REG_GENERALS + 7) { Prefix = "es" },

                "lb" => ReadImm(SmoSize.Byte, reader),
                "lw" => ReadImm(SmoSize.Word, reader),
                "lv" => ReadImm(Wordsize, reader),
                "lz" => ReadImm(Datasize, reader),

                _ => throw new NotImplementedException($"Unknown operand code: {x}"),
            }).ToArray();

            if (oprds.Any(x => x == null))
                throw new NotImplementedException();

            if (oprds.Length == 1)
            {
                SmoJcc jcc;
                if (mno.StartsWith("J") && Enum.TryParse(mno, true, out jcc))
                    return new SmoInstruction("Jcc", SmoOrder.UnaryCD, new SmoOperand(SmoOperandType.Condition, SmoSize.Byte, (long)jcc), oprds[0]);
                else if (mno.StartsWith("SET") && Enum.TryParse(mno.Replace("SET", "J"), true, out jcc))
                    return new SmoInstruction("SETcc", SmoOrder.UnaryCD, new SmoOperand(SmoOperandType.Condition, SmoSize.Byte, (long)jcc), oprds[0]);
                else if (mno.StartsWith("MOV") && Enum.TryParse(mno.Replace("MOV", "J"), true, out jcc))
                    return new SmoInstruction("MOVcc", SmoOrder.UnaryCD, new SmoOperand(SmoOperandType.Condition, SmoSize.Byte, (long)jcc), oprds[0]);
            }

            if (mno == IntelMnemonics.PUSH && oprds[0].Size == SmoSize.Byte)
                oprds[0].Size = SmoSize.Double;

            if (oprds.Length == 1)
                return new SmoInstruction(mno, SmoOrder.UnaryD, oprds[0]);
            if (oprds.Length == 2)
                return new SmoInstruction(mno, SmoOrder.BinaryDS, oprds[0], oprds[1]);
            if (oprds.Length == 3)
                return new SmoInstruction(mno, SmoOrder.TernaryDSS, oprds[0], oprds[1], oprds[2]);

            throw new NotImplementedException();
        }


        private SmoInstruction ReadGroup1(SmoSize sizeDst, SmoSize sizeSrc, IByteReader reader)
        {
            var by = reader.Read8();
            var opcode = tableMnemonicGrp1[(by >> 3) & 7];
            var dst = ReadEx(sizeDst, by, reader);
            SmoOperand src;
            if (sizeDst != SmoSize.Byte && sizeSrc == SmoSize.Byte)
                src = new SmoOperand(SmoOperandType.Value, Wordsize, (sbyte)reader.Read8());
            else
                src = ReadImm(sizeSrc, reader);
            var op = new SmoInstruction(opcode, SmoOrder.BinaryDS, dst, src);

            return op;
        }

        private SmoInstruction ReadGroup2(SmoSize size, int mode, IByteReader reader)
        {
            var by = reader.Read8();
            var reg = (by >> 3) & 7;
            if (tableMnemonicGrp2[reg] == null)
                return null;
            var opcode = tableMnemonicGrp2[reg];
            var dst = ReadEx(size, by, reader);
            SmoOperand src = null;
            if (mode == 0)
                src = ReadImm(SmoSize.Byte, reader);
            else if (mode == 1)
                src = new SmoOperand(SmoOperandType.Value, SmoSize.Byte, 1);
            else if (mode == 2)
                src = ReadAx(SmoSize.Byte, 1);
            return new SmoInstruction(opcode, SmoOrder.BinaryDS, dst, src);
        }

        private SmoInstruction ReadGroup3(SmoSize size, IByteReader reader)
        {
            var by = reader.Read8();
            var reg = (by >> 3) & 7;
            var opcode = tableMnemonicGrp3[reg];
            var ex = ReadEx(size, by, reader);
            if (reg >= 4)
                return new SmoInstruction(opcode, SmoOrder.BinaryDS, ReadAx(size), ex);
            if (reg >= 2)
                return new SmoInstruction(opcode, SmoOrder.UnaryD, ex);
            return new SmoInstruction(opcode, SmoOrder.BinaryDS, ex, ReadImm(size, reader));
        }

        private SmoInstruction ReadGroup5(SmoSize size, IByteReader reader)
        {
            var by = reader.Read8();
            var reg = (by >> 3) & 7;
            var opcode = tableMnemonicGrp5[reg];
            if (size == SmoSize.Byte && reg > 1)
                return null;
            if (tableMnemonicGrp5[reg] == null)
                return null;
            var ex = ReadEx(size, by, reader);
            var op = new SmoInstruction(opcode, SmoOrder.UnaryD, ex);
            if (opcode == "CALL" || opcode == "JMP")
                ex.Prefix = "*";
            return op;
        }

        private SmoInstruction ReadGroup6(IByteReader reader)
        {
            var by = reader.Read8();
            var reg = (by >> 3) & 7;
            var mod = (by >> 6) & 3;
            var rm = by & 7;

            return ReadWithMnemonic(tableMnemonicGrp6[reg], reader, by, true);
        }

        private SmoInstruction ReadGroup7(IByteReader reader)
        {
            var by = reader.Read8();
            var reg = (by >> 3) & 7;
            var mod = (by >> 6) & 3;
            var rm = by & 7;

            if (mod == 3)
                return null;
            return ReadWithMnemonic(tableMnemonicGrp7[reg], reader, by, true);
        }


        private SmoInstruction ReadGroupA(SmoSize size, IByteReader reader)
        {
            var by = reader.Read8();
            var reg = (by >> 3) & 7;
            if (reg == 0)
            {
                var dst = ReadEx(size, by, reader);
                var src = ReadImm(size, reader);
                return new SmoInstruction("MOV", SmoOrder.BinaryDS, dst, src);
            }
            return null;
        }




        private string[] tableMnemonicx87_A = new string[]
        {
            // 0xd8
            "FADD sr", "FMUL sr", "FCOM sr", "FCOMP sr", "FSUB sr", "FSUBR sr", "FDIV sr", "FDIVR sr",
            // 0xd9
            "FLD sr", null, "FST sr", "FSTP sr", "FLDENV db", "FLDCW db", "FSTENV 2b", "FSTCW 2b",
            // 0xda
            "FIADD di", "FIMUL di", "FICOM di", "FICOMP di", "FISUB di", "FISUBR di", "FIDIV di", "FIDIVR di",
            // 0xdb
            "FILD di", "FISTTP di", "FIST di", "FISTP di", null, "FLD er", null, "FSTP er",
            // 0xdc
            "FADD dr", "FMUL dr", "FCOM dr", "FCOMP dr", "FSUB dr", "FSUBR dr", "FDIV dr", "FDIVR dr",
            // 0xdd
            "FLD dr", "FISTTP i64", "FST dr", "FSTP dr", "FRSTOR ldb", null, "FSAVE ldb", "FSTSW 2b",
            // 0xde
            "FIADD wi", "FIMUL wi", "FICOM wi", "FICOMP wi", "FISUB wi", "FISUBR wi", "FIDIV wi", "FIDIVR wi",
            // 0xdf
            "FILD wi", "FISTTP wi", "FIST wi", "FISTP wi", "FBLD pb", "FILD qi", "FBSTP pb", "FISTP qi",
        };

        private SmoInstruction ReadCoprocessor(byte by, IByteReader reader)
        {
            var by2 = reader.Read8();
            var mod = (by2 >> 6) & 3;
            var reg = (by2 >> 3) & 7;
            var rm = by2 & 7;

            if (by2 < 0xc0)
            {
                var mno = tableMnemonicx87_A[(by - 0xd8) * 8 + reg];
                int idx = mno.IndexOf(' ');
                var operand = mno.Substring(idx + 1).Trim();
                var op = mno[0..idx];
                var size = SmoSize.FSingle;
                if (operand == "dr")
                    size = SmoSize.FDouble;
                return new SmoInstruction(op, SmoOrder.UnaryD, new SmoOperand(SmoOperandType.Memory, size, 0));
            }

            throw new NotImplementedException();
        }

        private const int REG_GENERALS = 0; // ax, cx, dx, bx, sp, bp, si, di
        private const int REG_SEGMENTS = 8; // es, cs, ss, ds, fs, gs
        private const int REG_CONTROLS = 16; // cr0, cr1, cr2, cr3, cr4
        private const int REG_DEBUGS = 24;
        private const int REG_OUTPORT = 32;

        private SmoOperand ReadImm(SmoSize size, IByteReader reader, SmoOperandType type = SmoOperandType.Value)
        {
            long value = size switch
            {
                SmoSize.Byte => (sbyte)reader.Read8(),
                SmoSize.Word => (short)reader.Read16(),
                SmoSize.Double => (int)reader.Read32(),
                SmoSize.Quad => (long)reader.Read64(),
                _ => throw new NotSupportedException(),
            };
            return new SmoOperand(type, size, value);
        }
        private SmoOperand ReadAx(SmoSize size, byte reg = 0)
        {
            //if (reg >= 8)
            //    return null;
            return new SmoOperand(SmoOperandType.Register, size, reg);
        }
        private SmoOperand ReadSw(byte by)
        {
            var reg = (by >> 3) & 7;
            if (reg >= 6)
                return null;
            return new SmoOperand(SmoOperandType.Register, SmoSize.Word, REG_SEGMENTS + reg);
        }

        private SmoOperand ReadGx(SmoSize size, byte by)
        {
            var reg = (by >> 3) & 7;
            return new SmoOperand(SmoOperandType.Register, size, REG_GENERALS + reg);
        }

        private SmoOperand ReadEx(SmoSize size, byte by, IByteReader reader)
        {
            var mod = (by >> 6) & 3;
            var rm = by & 7;

            if (mod == 3)
                return new SmoOperand(SmoOperandType.Register, size, REG_GENERALS + rm);

            if (rm == 4)
            {
                var sib = reader.Read8();
                var scale = (sib >> 6) & 3;
                var index = (sib >> 3) & 7;
                var basev = sib & 7;
                var dbg = $"{by:x2} {sib:x2}"; // 0x13c(%eax,%edx,4)
                if (index == 4)
                {
                    if (mod == 0)
                        return new SmoOperand(SmoOperandType.Memory, size, REG_GENERALS + basev);
                    if (mod == 1)
                        return new SmoOperand(SmoOperandType.MemoryOffset, size, REG_GENERALS + basev, reader.Read8());
                    if (mod == 2)
                        return new SmoOperand(SmoOperandType.MemoryOffset, size, REG_GENERALS + basev, reader.Read32());
                }
                if (basev == 5)
                {
                    if (mod == 0)
                        return new SmoOperand(SmoOperandType.Sib2, size, index, 1L << scale, (int)reader.Read32());
                    if (mod == 1)
                        return new SmoOperand(SmoOperandType.Sib1, size, index, 1L << scale, basev, (sbyte)reader.Read8());
                    if (mod == 2)
                        return new SmoOperand(SmoOperandType.Sib3, size, index, 1L << scale, basev);
                }

                if (mod == 0)
                    return new SmoOperand(SmoOperandType.Sib3, size, index, 1L << scale, REG_GENERALS + basev);
                if (mod == 1)
                    return new SmoOperand(SmoOperandType.Sib1, size, index, 1L << scale, REG_GENERALS + basev, (sbyte)reader.Read8());
                if (mod == 2)
                    return new SmoOperand(SmoOperandType.Sib1, size, index, 1L << scale, REG_GENERALS + basev, (int)reader.Read32());
            }

            if (mod == 2)
            {
                long val = (int)reader.Read32();
                return new SmoOperand(SmoOperandType.MemoryOffset, size, REG_GENERALS + rm, val);
            }
            if (mod == 1)
            {
                long val = (sbyte)reader.Read8();
                return new SmoOperand(SmoOperandType.MemoryOffset, size, REG_GENERALS + rm, val);
            }
            if (rm == 5)
                return new SmoOperand(SmoOperandType.Address, size, reader.Read32());
            return new SmoOperand(SmoOperandType.Memory, size, REG_GENERALS + rm);
        }


        private SmoOperand ReadRx(SmoSize size, byte by, IByteReader reader)
        {
            var mod = (by >> 6) & 3;
            var rm = by & 7;
            return new SmoOperand(SmoOperandType.Register, size, REG_GENERALS + rm);
        }
        private SmoOperand ReadCx(SmoSize size, byte by, IByteReader reader)
        {
            var reg = (by >> 3) & 7;
            return new SmoOperand(SmoOperandType.Register, size, REG_CONTROLS + reg);
        }
        private SmoOperand ReadDx(SmoSize size, byte by, IByteReader reader)
        {
            var reg = (by >> 3) & 7;
            return new SmoOperand(SmoOperandType.Register, size, REG_DEBUGS + reg);
        }

    }
}
