using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class Dissasemble_x86 : IDisassembler
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
            "JO Jb", "JNO Jb", "JC Jb", "JNC Jb", "JZ Jb", "JNZ Jb", "JNA Jb", "JA Jb",
            "JS Jb", "JNS Jb", "JP Jb", "JNP Jb", "JL Jb", "JNL Jb", "JNG Jb", "JG Jb",

            // 0x80
            null, null, null, null,
            "TEST Eb, Gb", "TEST Ev, Gv", "XCHG Eb, Gb", "XCHG Ev, Gv",
            "MOV Eb, Gb", "MOV Ev, Gv", "MOV Gb, Eb", "MOV Gv, Ev",
            "MOV Ev, Sw", "LEA Gv, M", "MOV Sw, Ew", null,

            // 0x90
            "NOP", null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,

            // 0xA0
            "MOV AL, Ob", "MOV rAX, Ov", "MOV Ob, AL", "MOV Ov, rAX",
            null, null, null, null,
            null, null, null, null, null, null, null, null,

            // 0xB0
            "MOV AL, lb", "MOV CL, lb", "MOV DL, lb", "MOV BL, lb",
            "MOV AH, lb", "MOV CH, lb", "MOV DH, lb", "MOV BH, lb",

            "MOV rAX, lv", "MOV rCX, lv", "MOV rDX, lv", "MOV rBX, lv",
            "MOV rSP, lv", "MOV rBP, lv", "MOV rSI, lv", "MOV rDI, lv",

            // 0xC0
            null, null, null, "RET", null, null, null, null,
            null, "LEAVE", null, null, null, null, null, null,

            // 0xD0
            null, null, null, null, "AAM lb", "AAD lb", "(bad)", "XLAT",
            null, null, null, null, null, null, null, null,

            // 0xE0
            null, null, null, null, null, null, null, null,
            "CALL Jz", "JMP Jz", "JMP Ap", "Jmp Jb",
            "IN AL, DX", "IN eAX, DX", "OUT DX, AL", "OUT DX, eAX",

            // 0xF0
            null, "(bad)", null, null, "HLT", "CMC", null, null,
            "CLC", "STC", "CLI", "STI", "CLD", "STD", null, null,
        };

        private static string[] tableMnemonicGrp1 = new string[]
        { "ADD", "OR", "ADC", "SBB", "AND", "SUB", "XOR", "CMP" };

        private static string?[] tableMnemonicGrp2 = new string?[]
        { "ROL", "ROR", "RCL", "RCR", "SHL", "SHR", null, "SAL" };

        private static string?[] tableMnemonicGrp3 = new string?[]
        { "TEST", null, "NOT", "NEG", "MUL", "IMUL", "DIV", "IDIV" };

        private static string?[] tableMnemonicGrp5 = new string?[]
        {  "INC", "DEC", "CALL", null, "JMP", null, "PUSH", null };


        private static string?[] tableMnemonic0F = new string?[]
        {  
            // 0x00
            null, null, "LAR Gv, Ew", "LSL Gv, Ew", null, "SYSCALL", "CLTS", "SYSRET",
            "INVD", "WBINVD", null, null, null, null, null, null,
            "vmovups Vps, Wps", "vmovups Wps, Vps", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0x20
            "MOV Rd, Cd", "MOV Rd, Rd", "MOV Cd, Rd", "MOV Dd, Rd", null, null, null, null,
            null, null, null, null, null, null, null, null,
            "WRMSR", "RDTSC", "RDMSR", "RDPMC", "SYSENTER", "SYSEXIT", null, "GETSEC",
            null, null, null, null, null, null, null, null,
            // 0x40
            "MOVO Jz", "MOVNO Jz", "MOVC Jz", "MOVNC Jz", "MOVZ Jz", "MOVNZ Jz", "MOVNA Jz", "MOVA Jz",
            "MOVS Jz", "MOVNS Jz", "MOVP Jz", "MOVNP Jz", "MOVL Jz", "MOVNL Jz", "MOVNG Jz", "MOVG Jz",
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0x60
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0x80
            "JO Jz", "JNO Jz", "JC Jz", "JNC Jz", "JZ Jz", "JNZ Jz", "JNA Jz", "JA Jz",
            "JS Jz", "JNS Jz", "JP Jz", "JNP Jz", "JL Jz", "JNL Jz", "JNG Jz", "JG Jz",
            "SETO Eb", "SETNO Eb", "SETC Eb", "SETNC Eb", "SETZ Eb", "SETNZ Eb", "SETNA Eb", "SETA Eb",
            "SETS Eb", "SETNS Eb", "SETP Eb", "SETNP Eb", "SETL Eb", "SETNL Eb", "SETNG Eb", "SETG Eb",
            // 0xa0
            "PUSH FS", "POP FS", "CPUID", "BT Ev, Gv", "SHLD Ev, Gv, lb", "SHLD Ev, Gv, CL", null, null,
            "PUSH GS", "POP GS", "RSM", "BTS Ev, Gv", "SHRD Ev, Gv, lb", "SHRD Ev, Gv, CL", null, "IMUL Gv, Ev",
            "CMPXCHG Eb, Gb", "CMPXCHG Ev, Gv", null, null, null, null, "MOVZX Gv, Eb", null,
            null, null, null, null, null, null, null, null,
            // 0xc0
            "XADD Eb, Gb", "XADD Ev, Gv", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            // 0xe0
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null,
        };


        private IByteReader _byteReader;
        public Dissasemble_x86(IByteReader byteReader)
        {
            _byteReader = byteReader;
        }

        private byte Read() => _byteReader.NextByte();
        private byte Read8() => _byteReader.NextByte();
        private ushort Read16()
        {
            var a = _byteReader.NextByte();
            var b = _byteReader.NextByte();
            return (ushort)(a | (b << 8));
        }
        private uint Read32()
        {
            var a = _byteReader.NextByte();
            var b = _byteReader.NextByte();
            var c = _byteReader.NextByte();
            var d = _byteReader.NextByte();
            return (uint)(a | (b << 8) | (c << 16) | (d << 24));
        }
        private ulong Read64()
        {
            var lo = Read32();
            var hi = Read32();
            return lo | (ulong)hi << 32;
        }

        public AsmOpcode? Disassemble()
        {
            byte by = Read();

            // Check prefix
            if (by == 0xf0)
            {
                var op = Disassemble();
                // LOCK
                return op;
            }
            else if (by == 0xf2)
            {
                var op = Disassemble();
                // REPNE
                return op;
            }
            else if (by == 0xf3)
            {
                var op = Disassemble();
                // REP (PAUSE)
                return op;
            }

            var mnemonic = tableMnemonic[by];
            if (mnemonic == "(bad)")
                return null;
            if (mnemonic != null)
                return ReadWithMnemonic(mnemonic);

            if (by == 0x0f)
            {
                by = Read();
                mnemonic = tableMnemonic0F[by];
                if (mnemonic == "(bad)" || mnemonic == null)
                    return null;
                if (mnemonic != null)
                    return ReadWithMnemonic(mnemonic);
                // 0f 95 0c SETNE al
            }
            else if (by == 0x80)
            {
                by = Read();
                return BuildDS(tableMnemonicGrp1[(by >> 3) & 7], ReadEx(by, WordSize.Byte), ReadImm(WordSize.Byte));
            }
            else if (by == 0x81)
            {
                by = Read();
                return BuildDS(tableMnemonicGrp1[(by >> 3) & 7], ReadEx(by, WordSize.Double), ReadImm(WordSize.Double));
            }
            else if (by == 0x82)
            {
                by = Read();
                return BuildDS(tableMnemonicGrp1[(by >> 3) & 7], ReadEx(by, WordSize.Byte), ReadImm(WordSize.Byte));
            }
            else if (by == 0x83)
            {
                by = Read();
                return BuildDS(tableMnemonicGrp1[(by >> 3) & 7], ReadEx(by, WordSize.Double), ReadImm(WordSize.Byte));
            }
            else if (by == 0xc0)
                return ReadGroup2(WordSize.Byte, 0);
            else if (by == 0xc1)
                return ReadGroup2(WordSize.Double, 0);
            else if (by == 0xc6)
            {
                by = Read();
                var reg = (by >> 3) & 7;
                if (reg == 0)
                    return BuildDS("MOV", ReadEx(by, WordSize.Byte), ReadImm(WordSize.Byte));
                return null;
            }
            else if (by == 0xc7)
            {
                by = Read();
                var reg = (by >> 3) & 7;
                if (reg == 0)
                    return BuildDS("MOV", ReadEx(by, WordSize.Double), ReadImm(WordSize.Double));
                return null;
            }
            else if (by == 0xd0)
                return ReadGroup2(WordSize.Byte, 1);
            else if (by == 0xd1)
                return ReadGroup2(WordSize.Double, 1);
            else if (by == 0xd2)
                return ReadGroup2(WordSize.Byte, 2);
            else if (by == 0xd3)
                return ReadGroup2(WordSize.Double, 2);
            else if (by == 0xf6)
                return ReadGroup3(WordSize.Byte);
            else if (by == 0xf7)
                return ReadGroup3(WordSize.Double);
            else if (by == 0xfe)
            { // Group 4
                by = Read();
                var reg = (by >> 3) & 7;
                if (reg > 1)
                    return null;
                return BuildD(tableMnemonicGrp5[reg], ReadEx(by, WordSize.Byte));
            }
            else if (by == 0xff)
            { // Group 5
                by = Read();
                var reg = (by >> 3) & 7;
                if (tableMnemonicGrp5[reg] == null)
                    return null;
                return BuildD(tableMnemonicGrp5[reg], ReadEx(by, WordSize.Double));
            }

            return null;
        }

        private AsmOpcode BuildDSS(string mnemonic, AsmOperand dst, AsmOperand src, AsmOperand src2)
        {
            return new AsmOpcode
            {
                Opcode = Enum.Parse<OpcodeMnemonic>(mnemonic, true),
                Sources = new AsmOperand[] { src, src2, dst, },
                Destinations = new AsmOperand[] { dst, }
            };
        }

        private AsmOpcode BuildDS(string mnemonic, AsmOperand dst, AsmOperand src)
        {
            return new AsmOpcode
            {
                Opcode = Enum.Parse<OpcodeMnemonic>(mnemonic, true),
                Sources = new AsmOperand[] { src, dst, },
                Destinations = new AsmOperand[] { dst, }
            };
        }

        private AsmOpcode BuildD(string mnemonic, AsmOperand src)
        {
            if (Enum.TryParse<OpcodeMnemonic>(mnemonic, true, out var mnc))
            {
                return new AsmOpcode
                {
                    Opcode = Enum.Parse<OpcodeMnemonic>(mnemonic, true),
                    Sources = new AsmOperand[] { src, },
                    Destinations = new AsmOperand[0],
                };
            }


            Jcc jcc = Jcc.Ja;
            if (mnemonic.ToLower().StartsWith("j") && Enum.TryParse(mnemonic, true, out jcc))
                mnc = OpcodeMnemonic.Jcc;
            else if (mnemonic.ToLower().StartsWith("set") && Enum.TryParse(mnemonic.Replace("SET", "J"), true, out jcc))
                mnc = OpcodeMnemonic.Setcc;
            else if (mnemonic.ToLower().StartsWith("mov") && Enum.TryParse(mnemonic.Replace("MOV", "J"), true, out jcc))
                mnc = OpcodeMnemonic.Movcc;
            else
                throw new Exception();

            return new AsmOpcode
            {
                Opcode = mnc,
                Sources = new AsmOperand[] { src, new AsmOperand {
                    Type = OperandType.Special, Value = (ulong)jcc,
                    }
                },
                Destinations = new AsmOperand[0],
            };
        }

        private AsmOpcode? ReadWithMnemonic(string mnemonic)
        {
            int idx = mnemonic.IndexOf(' ');
            if (idx < 0)
                return new AsmOpcode()
                {
                    Opcode = Enum.Parse<OpcodeMnemonic>(mnemonic, true),
                };

            var opcode = mnemonic.Substring(0, idx);
            var operands = mnemonic.Substring(idx + 1).Trim();

            byte b2 = 0;
            var arr = operands.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            if (arr.Any(x => x.StartsWith('E') || x == "M"))
                b2 = Read();
            WordSize wordsize = WordSize.Double;
            WordSize datasize = wordsize == WordSize.Word ? WordSize.Word : WordSize.Double;
            var oprds = new AsmOperand?[arr.Length];
            for (var i = 0; i < arr.Length; ++i)
            {
                oprds[i] = arr[i] switch
                {
                    "Jb" => ReadImm(WordSize.Byte),
                    "Jz" => ReadImm(datasize),
                    "lb" => ReadImm(WordSize.Byte),
                    "lv" => ReadImm(wordsize),
                    "lz" => ReadImm(datasize),

                    "Eb" => ReadEx(b2, WordSize.Byte),
                    "Ev" => ReadEx(b2, wordsize),
                    "Gb" => ReadGx(b2, WordSize.Byte),
                    "Gv" => ReadGx(b2, wordsize),

                    "M" => ((b2 >> 6) & 3) == 3  ? null : ReadEx(b2, wordsize),

                    "AL" => ReadAx(WordSize.Byte, 0),
                    "CL" => ReadAx(WordSize.Byte, 1),
                    "DL" => ReadAx(WordSize.Byte, 2),
                    "BL" => ReadAx(WordSize.Byte, 3),
                    "AH" => ReadAx(WordSize.Byte, 4),
                    "CH" => ReadAx(WordSize.Byte, 5),
                    "DH" => ReadAx(WordSize.Byte, 6),
                    "BH" => ReadAx(WordSize.Byte, 7),
                    "rAX" => ReadAx(wordsize, 0),
                    "rCX" => ReadAx(wordsize, 1),
                    "rDX" => ReadAx(wordsize, 2),
                    "rBX" => ReadAx(wordsize, 3),
                    "rSP" => ReadAx(wordsize, 4),
                    "rBP" => ReadAx(wordsize, 5),
                    "rSI" => ReadAx(wordsize, 6),
                    "rDI" => ReadAx(wordsize, 7),

                    "Sw" => ReadSw(b2),

                    _ => throw new NotImplementedException(),
                };
            }

            if (oprds.Any(x => x == null))
                return null;
            if (oprds.Length == 1)
                return BuildD(opcode, oprds[0]);
            else if (oprds.Length == 2)
                return BuildDS(opcode, oprds[0], oprds[1]);
            else if (oprds.Length == 3)
                return BuildDSS(opcode, oprds[0], oprds[1], oprds[2]);
            else
                throw new NotImplementedException();
        }

        private AsmOpcode? ReadGroup2(WordSize size, int mode)
        {
            var by = Read();
            var reg = (by >> 3) & 7;
            if (tableMnemonicGrp2[reg] == null)
                return null;
            if (mode == 0)
                return BuildDS(tableMnemonicGrp2[reg], ReadEx(by, size), ReadImm(WordSize.Byte));
            if (mode == 1)
                return BuildDS(tableMnemonicGrp2[reg], ReadEx(by, size), new AsmOperand
                {
                    Type = OperandType.Value,
                    Value = 1,
                    Word = WordSize.Byte,
                });
            if (mode == 2)
                return BuildDS(tableMnemonicGrp2[reg], ReadEx(by, size), ReadAx(WordSize.Byte, 1));
            return null;
        }

        private AsmOpcode? ReadGroup3(WordSize size)
        {
            var by = Read();
            var reg = (by >> 3) & 7;
            if (reg == 0)
                return BuildDS("TEST", ReadEx(by, size), ReadImm(size));
            if (reg == 1)
                return BuildDS("TEST", ReadEx(by, size), ReadImm(size));
            if (reg == 2)
                return BuildD("NOT", ReadEx(by, size));
            if (reg == 3)
                return BuildD("NEG", ReadEx(by, size));
            if (reg == 4)
                return BuildDS("MUL", ReadAx(size), ReadEx(by, size));
            if (reg == 5)
                return BuildDS("IMUL", ReadAx(size), ReadEx(by, size));
            if (reg == 6)
                return BuildDS("DIV", ReadAx(size), ReadEx(by, size));
            if (reg == 7)
                return BuildDS("IDIV", ReadAx(size), ReadEx(by, size));
            throw new Exception();
        }




        const int RegOffGeneral = 0; // ax, cx, dx, bx, sp, bp, si, di
        const int RegOffSegments = 8; // es, cs, ss, ds, fs, gs
        const int RegOffControls = 16; // cr0, -, cr2, cr3, cr4, -
        const int RegOffDebugs = 24; // dr0, dr1, dr2, dr3, -, - , dr6, dr7


        private AsmOperand ReadAx(WordSize size, int reg = 0)
        {
            return new AsmOperand
            {
                Type = OperandType.Register,
                Word = size,
                Value = (ulong)reg,
            };
        }
        private AsmOperand ReadImm(WordSize size)
        {
            return new AsmOperand
            {
                Type = OperandType.Value,
                Word = size,
                Value = size == WordSize.Byte ? Read8()
                    : size == WordSize.Double ? Read32()
                    : size == WordSize.Quad ? Read64()
                    : Read16(),
            };
        }
        private AsmOperand ReadEx(byte by, WordSize size)
        {
            var mod = (by >> 6) & 3;
            var rm = by & 7;

            if (mod == 3)
                return new AsmOperand()
                {
                    Type = OperandType.Register,
                    Word = size,
                    Value = (ulong)rm,
                };
            else if (rm == 4)
            {
                var sib = Read();
                var scale = (sib >> 6) & 3;
                var index = (sib >> 3) & 7;
                var basev = sib & 7;

                if (index == 4)
                    return new AsmOperand()
                    {
                        Type = OperandType.Memory,
                        Word = size,
                        Value = (ulong)basev,
                    };
                else if (basev == 5)
                    return new AsmOperand()
                    {
                        Type = mod == 0 ? OperandType.Sib2 : OperandType.Sib3,
                        Word = size,
                        Value = (ulong)index,
                        Value2 = 1UL << scale,
                        Value3 = mod == 1 ? Read8() : Read32(),
                    };
                else
                    return new AsmOperand()
                    {
                        Type = OperandType.Sib1,
                        Word = size,
                        Value = (ulong)index,
                        Value2 = 1UL << scale,
                        Value3 = (ulong)basev,
                    };
            }
            else if (mod == 0 && rm == 5)
                return new AsmOperand()
                {
                    Type = OperandType.Address,
                    Word = size,
                    Value = Read32(),
                };
            else if (mod == 0)
                return new AsmOperand()
                {
                    Type = OperandType.Memory,
                    Word = size,
                    Value = (ulong)rm
                };
            else if (mod == 1)
                return new AsmOperand()
                {
                    Type = OperandType.Memory,
                    Word = size,
                    Value = (ulong)rm,
                    Value2 = Read8(),
                };
            else if (mod == 2)
                return new AsmOperand()
                {
                    Type = OperandType.Memory,
                    Word = size,
                    Value = (ulong)rm,
                    Value2 = Read32(),
                };
            else
                throw new Exception();
        }
        private AsmOperand ReadGx(byte by, WordSize size)
        {
            var reg = (by >> 3) & 7;
            return new AsmOperand()
            {
                Type = OperandType.Register,
                Word = size,
                Value = (ulong)reg,
            };
        }
        private AsmOperand ReadSw(byte by)
        {
            var reg = (by >> 3) & 7;
            if (reg >= 6)
                return null;
            return new AsmOperand
            {
                Type = OperandType.Register,
                Word = WordSize.Word,
                Value = (ulong)(RegOffSegments + reg),
            };
        }
    }
}
