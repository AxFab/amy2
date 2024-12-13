using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class AsmOperand
    {
        public ulong Value { get; set; }
        public WordSize Word { get; set; }
        public OperandType Type { get; set; }
        public ulong Value2 { get; internal set; }
        public ulong Value3 { get; internal set; }
        public string Note { get; internal set; }
    }

    public class AsmOpcode
    {
        public OpcodeMnemonic Opcode { get; set; }
        public AsmOperand[] Sources { get; set; }
        public AsmOperand[] Destinations { get; set; }
    }

    public enum WordSize
    {
        Unsized = 0, Byte = 1, Word = 2, Double = 4, Quad = 8, Octo = 16,
    }
    public enum OperandType
    {
        Register, // ReadRegister[Word](Value)
        Address, // ReadMemory[Word](Value)
        Memory, // ReadMemory[Word](ReadRegister[Addr](Value))
        Value, // [Word]Value
        Special, //  => ScaleBaseIndex    ReadMemory[Word2](ReadRegister[Addr](Base) + ReadRegister[Word](Index) * Scale)

        // Intel 
        Sib1, // [REG.1 * VAL.2 + REG.3]
        Sib2, // [REG.1 * VAL.2 + VAL.3]
        Sib3, // [REG.1 * VAL.2 + VAL.3 + EBP]


        VAddRef,
        MAddRef,
    }

    public enum OpcodeMnemonic
    {
        Mov,
        Add, Sub, Adc, Sbb,
        And, Or, Xor,
        Mul, Div, IMul, IDiv,
        Shl, Shr, Sal, Sar, Rol, Ror, Rcr, Rcl,
        Inc, Dec,
        Not, Neg,
        Cmp, Test,
        Push, Pop,
        Jmp, Jcc,
        Call, Ret,
        Hlt, Nop,

        Xchg, Xadd, CmpXchg,

        // Special
        Leave, Syscall, Clts, Sysret, Cpuid,
        Movsx, Movzx,

        Setcc, Movcc, Lea,
    }

    public enum Jcc
    {
        // Unsigned
        Ja, Jnbe, // Fc or Fz = 0
        Jae, Jnb, // Fc = 0
        Jb, Jnae, // Fc = 1
        Jbe, Jna, // Fc or Zf = 1

        // Signed
        Jg, Jnle,  // !(Fs ^ Fo) and Fz = 0
        Jge, Jnl, // Fs ^ Fo = 0
        Jnge, Jl, // Fs ^ Fo = 1
        Jng, Jle,  // Fs ^ Fo or Fz = 1

        // Flags
        Jc, // Fc = 1
        Je, Jz, // Fz = 1
        Jnc, // Fc = 1,
        Jne, Jnz, // Fz = 0

        Jnp, Jpo, // Fp = 0 (Not parity, parity odd)
        Jp, Jpe, // Fp = 1

        Jno, // Fo = 0 Not overflow
        Jns, // Fs = 0 Not negative
        Jo,
        Js,

        Jcxz, // Cx = 0
        Jncxz, // Cx != 0
    }



}
