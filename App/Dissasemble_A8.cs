using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{


    enum Asm8B
    {
        NONE = 0,
        MOV_REG_TO_REG = 1,
        MOV_ADDRESS_TO_REG = 2,
        MOV_REGADDRESS_TO_REG = 3,
        MOV_REG_TO_ADDRESS = 4,
        MOV_REG_TO_REGADDRESS = 5,
        MOV_NUMBER_TO_REG = 6,
        MOV_NUMBER_TO_ADDRESS = 7,
        MOV_NUMBER_TO_REGADDRESS = 8,
        
        ADD_REG_TO_REG = 10,
        ADD_REGADDRESS_TO_REG = 11,
        ADD_ADDRESS_TO_REG = 12,
        ADD_NUMBER_TO_REG = 13,
        
        SUB_REG_FROM_REG = 14,
        SUB_REGADDRESS_FROM_REG = 15,
        SUB_ADDRESS_FROM_REG = 16,
        SUB_NUMBER_FROM_REG = 17,
        
        INC_REG = 18,
        DEC_REG = 19,
        
        CMP_REG_WITH_REG = 20,
        CMP_REGADDRESS_WITH_REG = 21,
        CMP_ADDRESS_WITH_REG = 22,
        CMP_NUMBER_WITH_REG = 23,
        
        JMP_REGADDRESS = 30,
        JMP_ADDRESS = 31,
        JC_REGADDRESS = 32,
        JC_ADDRESS = 33,
        JNC_REGADDRESS = 34,
        JNC_ADDRESS = 35,
        JZ_REGADDRESS = 36,
        JZ_ADDRESS = 37,
        JNZ_REGADDRESS = 38,
        JNZ_ADDRESS = 39,
        JA_REGADDRESS = 40,
        JA_ADDRESS = 41,
        JNA_REGADDRESS = 42,
        JNA_ADDRESS = 43,
        
        PUSH_REG = 50,
        PUSH_REGADDRESS = 51,
        PUSH_ADDRESS = 52,
        PUSH_NUMBER = 53,
        POP_REG = 54,
        CALL_REGADDRESS = 55,
        CALL_ADDRESS = 56,
        RET = 57,
        
        MUL_REG = 60,
        MUL_REGADDRESS = 61,
        MUL_ADDRESS = 62,
        MUL_NUMBER = 63,
        
        DIV_REG = 64,
        DIV_REGADDRESS = 65,
        DIV_ADDRESS = 66,
        DIV_NUMBER = 67,
        
        AND_REG_WITH_REG = 70,
        AND_REGADDRESS_WITH_REG = 71,
        AND_ADDRESS_WITH_REG = 72,
        AND_NUMBER_WITH_REG = 73,
        
        OR_REG_WITH_REG = 74,
        OR_REGADDRESS_WITH_REG = 75,
        OR_ADDRESS_WITH_REG = 76,
        OR_NUMBER_WITH_REG = 77,
        
        XOR_REG_WITH_REG = 78,
        XOR_REGADDRESS_WITH_REG = 79,
        XOR_ADDRESS_WITH_REG = 80,
        XOR_NUMBER_WITH_REG = 81,
        
        NOT_REG = 82,
        
        SHL_REG_WITH_REG = 90,
        SHL_REGADDRESS_WITH_REG = 91,
        SHL_ADDRESS_WITH_REG = 92,
        SHL_NUMBER_WITH_REG = 93,
        
        SHR_REG_WITH_REG = 94,
        SHR_REGADDRESS_WITH_REG = 95,
        SHR_ADDRESS_WITH_REG = 96,
        SHR_NUMBER_WITH_REG = 97
    }

    public class AsmBuilder
    {
        public AsmOperand Reg8(int reg)
        {
            return new AsmOperand
            {
                Type = OperandType.Register,
                Word = WordSize.Byte,
                Value = (ulong)reg,
            };
        }
        public AsmOperand Addr8(ulong addr)
        {
            return new AsmOperand
            {
                Type = OperandType.Address,
                Word = WordSize.Byte,
                Value = addr,
            };
        }
        public AsmOperand Mem8(int reg)
        {
            int off = (reg >> 4) & 0xF;
            if (off > 15)
                off -= 32;
            return new AsmOperand
            {
                Type = OperandType.Memory,
                Word = WordSize.Byte,
                Value = (ulong)reg & 0xF,
                Value2 = (ulong)off,
            };
        }
        public AsmOperand Num8(byte value)
        {
            return new AsmOperand
            {
                Type = OperandType.Value,
                Word = WordSize.Byte,
                Value = (ulong)value,
            };
        }

        public AsmOperand Spc(ulong value)
        {
            return new AsmOperand
            {
                Type = OperandType.Special,
                Word = WordSize.Unsized,
                Value = value,
            };
        }
        public AsmOpcode Op(OpcodeMnemonic op)
        {
            return new AsmOpcode
            {
                Opcode = op,
                Destinations = new AsmOperand[] { },
                Sources = new AsmOperand[] { },
            };
        }
        public AsmOpcode OpD(OpcodeMnemonic op, AsmOperand dst)
        {
            return new AsmOpcode
            {
                Opcode = op,
                Destinations = new AsmOperand[] { dst },
                Sources = new AsmOperand[] { dst },
            };
        }
        public AsmOpcode OpS(OpcodeMnemonic op, AsmOperand src)
        {
            return new AsmOpcode
            {
                Opcode = op,
                Destinations = new AsmOperand[] { },
                Sources = new AsmOperand[] { src },
            };
        }
        public AsmOpcode OpSS(OpcodeMnemonic op, AsmOperand src1, AsmOperand src2)
        {
            return new AsmOpcode
            {
                Opcode = op,
                Destinations = new AsmOperand[] { },
                Sources = new AsmOperand[] { src1, src2 },
            };
        }
        public AsmOpcode OpDS(OpcodeMnemonic op, AsmOperand dst, AsmOperand src)
        {
            return new AsmOpcode
            {
                Opcode = op,
                Destinations = new AsmOperand[] { dst },
                Sources = new AsmOperand[] { src, dst },
            };
        }

    }

    public class Dissasemble_A8 : AsmBuilder, IDisassembler
    {
        IByteReader _reader;

        public Dissasemble_A8(IByteReader reader)
        {
            _reader = reader;
        }

        public byte Read() => _reader.NextByte();
        public AsmOpcode Disassemble()
        {
            byte op = _reader.NextByte();
            switch ((Asm8B)op)
            {
                case Asm8B.NONE:
                    return Op(OpcodeMnemonic.Hlt);
                case Asm8B.MOV_REG_TO_REG:
                    return OpDS(OpcodeMnemonic.Mov, Reg8(Read()), Reg8(Read()));
                case Asm8B.MOV_ADDRESS_TO_REG:
                    return OpDS(OpcodeMnemonic.Mov, Reg8(Read()), Addr8(Read()));
                case Asm8B.MOV_REGADDRESS_TO_REG:
                    return OpDS(OpcodeMnemonic.Mov, Reg8(Read()), Mem8(Read()));
                case Asm8B.MOV_REG_TO_ADDRESS:
                    return OpDS(OpcodeMnemonic.Mov, Addr8(Read()), Reg8(Read()));
                case Asm8B.MOV_REG_TO_REGADDRESS:
                    return OpDS(OpcodeMnemonic.Mov, Mem8(Read()), Reg8(Read()));
                case Asm8B.MOV_NUMBER_TO_REG:
                    return OpDS(OpcodeMnemonic.Mov, Reg8(Read()), Num8(Read()));
                case Asm8B.MOV_NUMBER_TO_ADDRESS:
                    return OpDS(OpcodeMnemonic.Mov, Addr8(Read()), Num8(Read()));
                case Asm8B.MOV_NUMBER_TO_REGADDRESS:
                    return OpDS(OpcodeMnemonic.Mov, Mem8(Read()), Num8(Read()));

                case Asm8B.ADD_REG_TO_REG:
                    return OpDS(OpcodeMnemonic.Add, Reg8(Read()), Reg8(Read()));
                case Asm8B.ADD_REGADDRESS_TO_REG:
                    return OpDS(OpcodeMnemonic.Add, Reg8(Read()), Mem8(Read()));
                case Asm8B.ADD_ADDRESS_TO_REG:
                    return OpDS(OpcodeMnemonic.Add, Reg8(Read()), Addr8(Read()));
                case Asm8B.ADD_NUMBER_TO_REG:
                    return OpDS(OpcodeMnemonic.Add, Reg8(Read()), Num8(Read()));

                case Asm8B.SUB_REG_FROM_REG:
                    return OpDS(OpcodeMnemonic.Sub, Reg8(Read()), Reg8(Read()));
                case Asm8B.SUB_REGADDRESS_FROM_REG:
                    return OpDS(OpcodeMnemonic.Sub, Reg8(Read()), Mem8(Read()));
                case Asm8B.SUB_ADDRESS_FROM_REG:
                    return OpDS(OpcodeMnemonic.Sub, Reg8(Read()), Addr8(Read()));
                case Asm8B.SUB_NUMBER_FROM_REG:
                    return OpDS(OpcodeMnemonic.Sub, Reg8(Read()), Num8(Read()));

                case Asm8B.INC_REG:
                    return OpD(OpcodeMnemonic.Inc, Reg8(Read()));
                case Asm8B.DEC_REG:
                    return OpD(OpcodeMnemonic.Dec, Reg8(Read()));

                case Asm8B.CMP_REG_WITH_REG:
                    return OpDS(OpcodeMnemonic.Cmp, Reg8(Read()), Reg8(Read()));
                case Asm8B.CMP_REGADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.Cmp, Reg8(Read()), Mem8(Read()));
                case Asm8B.CMP_ADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.Cmp, Reg8(Read()), Addr8(Read()));
                case Asm8B.CMP_NUMBER_WITH_REG:
                    return OpDS(OpcodeMnemonic.Cmp, Reg8(Read()), Num8(Read()));

                case Asm8B.JMP_REGADDRESS:
                    return OpS(OpcodeMnemonic.Jmp, Reg8(_reader.NextByte()));
                case Asm8B.JMP_ADDRESS:
                    return OpS(OpcodeMnemonic.Jmp, Num8(_reader.NextByte()));
                case Asm8B.JC_REGADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Reg8(_reader.NextByte()), Spc((ulong)Jcc.Jc));
                case Asm8B.JC_ADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Num8(_reader.NextByte()), Spc((ulong)Jcc.Jc));
                case Asm8B.JNC_REGADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Reg8(_reader.NextByte()), Spc((ulong)Jcc.Jnc));
                case Asm8B.JNC_ADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Num8(_reader.NextByte()), Spc((ulong)Jcc.Jnc));
                case Asm8B.JZ_REGADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Reg8(_reader.NextByte()), Spc((ulong)Jcc.Jz));
                case Asm8B.JZ_ADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Num8(_reader.NextByte()), Spc((ulong)Jcc.Jz));
                case Asm8B.JNZ_REGADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Reg8(_reader.NextByte()), Spc((ulong)Jcc.Jnz));
                case Asm8B.JNZ_ADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Num8(_reader.NextByte()), Spc((ulong)Jcc.Jnz));
                case Asm8B.JA_REGADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Reg8(_reader.NextByte()), Spc((ulong)Jcc.Ja));
                case Asm8B.JA_ADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Num8(_reader.NextByte()), Spc((ulong)Jcc.Ja));
                case Asm8B.JNA_REGADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Reg8(_reader.NextByte()), Spc((ulong)Jcc.Jna));
                case Asm8B.JNA_ADDRESS:
                    return OpSS(OpcodeMnemonic.Jcc, Num8(_reader.NextByte()), Spc((ulong)Jcc.Jna));

                case Asm8B.PUSH_REG:
                    return OpS(OpcodeMnemonic.Push, Reg8(_reader.NextByte()));
                case Asm8B.PUSH_REGADDRESS:
                    return OpS(OpcodeMnemonic.Push, Mem8(_reader.NextByte()));
                case Asm8B.PUSH_ADDRESS:
                    return OpS(OpcodeMnemonic.Push, Addr8(_reader.NextByte()));
                case Asm8B.PUSH_NUMBER:
                    return OpS(OpcodeMnemonic.Push, Num8(_reader.NextByte()));

                case Asm8B.POP_REG:
                    return OpD(OpcodeMnemonic.Pop, Reg8(_reader.NextByte()));
                case Asm8B.CALL_REGADDRESS:
                    return OpS(OpcodeMnemonic.Call, Mem8(_reader.NextByte()));
                case Asm8B.CALL_ADDRESS:
                    return OpS(OpcodeMnemonic.Call, Num8(_reader.NextByte()));
                case Asm8B.RET:
                    return Op(OpcodeMnemonic.Ret);

                case Asm8B.MUL_REG:
                    return OpS(OpcodeMnemonic.Mul, Reg8(_reader.NextByte()));
                case Asm8B.MUL_REGADDRESS:
                    return OpS(OpcodeMnemonic.Mul, Mem8(_reader.NextByte()));
                case Asm8B.MUL_ADDRESS:
                    return OpS(OpcodeMnemonic.Mul, Addr8(_reader.NextByte()));
                case Asm8B.MUL_NUMBER:
                    return OpS(OpcodeMnemonic.Mul, Num8(_reader.NextByte()));
                case Asm8B.DIV_REG:
                    return OpS(OpcodeMnemonic.Div, Reg8(_reader.NextByte()));
                case Asm8B.DIV_REGADDRESS:
                    return OpS(OpcodeMnemonic.Div, Mem8(_reader.NextByte()));
                case Asm8B.DIV_ADDRESS:
                    return OpS(OpcodeMnemonic.Div, Addr8(_reader.NextByte()));
                case Asm8B.DIV_NUMBER:
                    return OpS(OpcodeMnemonic.Div, Num8(_reader.NextByte()));

                case Asm8B.AND_REG_WITH_REG:
                    return OpDS(OpcodeMnemonic.And, Reg8(Read()), Reg8(Read()));
                case Asm8B.AND_REGADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.And, Reg8(Read()), Mem8(Read()));
                case Asm8B.AND_ADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.And, Reg8(Read()), Addr8(Read()));
                case Asm8B.AND_NUMBER_WITH_REG:
                    return OpDS(OpcodeMnemonic.And, Reg8(Read()), Num8(Read()));
                case Asm8B.OR_REG_WITH_REG:
                    return OpDS(OpcodeMnemonic.Or, Reg8(Read()), Reg8(Read()));
                case Asm8B.OR_REGADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.Or, Reg8(Read()), Mem8(Read()));
                case Asm8B.OR_ADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.Or, Reg8(Read()), Addr8(Read()));
                case Asm8B.OR_NUMBER_WITH_REG:
                    return OpDS(OpcodeMnemonic.Or, Reg8(Read()), Num8(Read()));
                case Asm8B.XOR_REG_WITH_REG:
                    return OpDS(OpcodeMnemonic.Xor, Reg8(Read()), Reg8(Read()));
                case Asm8B.XOR_REGADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.Xor, Reg8(Read()), Mem8(Read()));
                case Asm8B.XOR_ADDRESS_WITH_REG:
                    return OpDS(OpcodeMnemonic.Xor, Reg8(Read()), Addr8(Read()));
                case Asm8B.XOR_NUMBER_WITH_REG:
                    return OpDS(OpcodeMnemonic.Xor, Reg8(Read()), Num8(Read()));
                case Asm8B.NOT_REG:
                    return OpD(OpcodeMnemonic.Not, Reg8(Read()));

                case Asm8B.SHL_REG_WITH_REG:
                    return OpS(OpcodeMnemonic.Shl, Reg8(_reader.NextByte()));
                case Asm8B.SHL_REGADDRESS_WITH_REG:
                    return OpS(OpcodeMnemonic.Shl, Mem8(_reader.NextByte()));
                case Asm8B.SHL_ADDRESS_WITH_REG:
                    return OpS(OpcodeMnemonic.Shl, Addr8(_reader.NextByte()));
                case Asm8B.SHL_NUMBER_WITH_REG:
                    return OpS(OpcodeMnemonic.Shl, Num8(_reader.NextByte()));
                case Asm8B.SHR_REG_WITH_REG:
                    return OpS(OpcodeMnemonic.Shr, Reg8(_reader.NextByte()));
                case Asm8B.SHR_REGADDRESS_WITH_REG:
                    return OpS(OpcodeMnemonic.Shr, Mem8(_reader.NextByte()));
                case Asm8B.SHR_ADDRESS_WITH_REG:
                    return OpS(OpcodeMnemonic.Shr, Addr8(_reader.NextByte()));
                case Asm8B.SHR_NUMBER_WITH_REG:
                    return OpS(OpcodeMnemonic.Shr, Num8(_reader.NextByte()));
                default:
                    return Op(OpcodeMnemonic.Nop);
            }
        }

    }

}
