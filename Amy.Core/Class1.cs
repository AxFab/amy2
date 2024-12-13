using Amy.Core.Bytes;
using System;
using System.ComponentModel.Design;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Amy.Core
{
    public class SmoInstruction
    {
        public string Opcode;
        public SmoOperand[] Operands;
        public SmoOrder Order;
        public string Prefix;

        public SmoInstruction(string opcode)
        {
            Opcode = opcode;
            Order = SmoOrder.NoOperand; 
        }

        public SmoInstruction(string opcode, SmoOrder order, params SmoOperand[] operands)
        {
            Opcode = opcode;
            Order = order;
            Operands = operands;
        }
    }

    public enum SmoOperandType
    {
        Register, Address, Memory, Value,
        MemoryOffset,
        Sib1, Sib2, Sib3,
        Condition,
        NearRelativeJmp,
        LongJmp,
        FarRegJump
    }
    public enum SmoOrder
    {
        NoOperand,
        UnaryD, BinaryDS,
        UnaryDS, BinaryDSS,
        TernaryDSS,
        UnaryCD,
        BinaryFJ,
    }

    enum SmoJcc
    {
        // Unsigned
        Ja, Jnbe, // Fc or Fz = 0
        Jae, Jnb, // Fc = 0
        Jb, Jnae, // Fc = 1
        Jbe, Jna, // Fc or Fz = 1

        // Signed
        Jg, Jnle,  // !(Fs ^ Fo) and Fz = 0
        Jge, Jnl, // Fs ^ Fo = 0
        Jnge, Jl, // Fs ^ Fo = 1
        Jng, Jle,  // Fs ^ Fo or Fz = 1

        // Flags
        Jc, // Fc = 1
        Jz, Je, // Fz = 1
        Jnc, // Fc = 0,
        Jnz, Jne, // Fz = 0

        Jnp, Jpo, // Fp = 0 (Not parity, parity odd)
        Jp, Jpe, // Fp = 1

        Jno, // Fo = 0 Not overflow
        Jns, // Fs = 0 Not negative
        Jo,
        Js,

        Jcxz, // Cx = 0
        Jncxz, // Cx != 0
    }

    public class SmoOperand
    {
        public SmoOperand(SmoOperandType type, SmoSize size, long value, long value2 = 0, long value3 = 0, long value4 = 0)
        {
            Type = type;
            Size = size;
            Value = value;
            Value2 = value2;
            Value3 = value3;
            Value4 = value4;
        }

        public SmoOperandType Type { get; }
        public SmoSize Size { get; set; }
        public long Value { get; }
        public long Value2 { get; }
        public long Value3 { get; }
        public long Value4 { get; }
        public string Prefix { get; internal set; }
    }

    public enum SmoOpcode
    {
        Nop, Mov,
        Add, Or, Adc, Sbb, And, Sub, Xor, Cmp, Test, 
        Rol, Ror, Rcl, Rcr, Shl, Shr, Sar, Sal,
        Not, Neg, Mul, IMul, Div, IDiv,
        Inc, Dec, Call, Jmp, Push, Pop,
        Ret, Hlt, 
        Xchg, Xadd, CmpXchg,
        
        Leave, Syscall, Clts, Sysret, Cpuid, Movsx, Movzx, Lea,

        Jcc,
        Setcc,
        Movcc,
        Pause,

        Cli,


        STOS, LODS, SCAS
    }
    public enum SmoSize
    {
        Byte = 8, Word = 16, Double = 32, Quad = 64, Octo = 128,
        FSingle = 256,
        FDouble = 512,
    }


    public interface IAsmWriter
    {
        string Stringify(SmoInstruction instruction, long offset, int size, byte[] buf);
    }

    public interface IDisassembler // Byte to SMO
    {
        SmoInstruction NextInstruction(IByteReader reader);

    }

}
