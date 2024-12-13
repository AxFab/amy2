using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{

    public class VirtualMachine_A8 : AbstractInMemoryVm
    {
        public VirtualMachine_A8()
            : base(256, 1, 7, 1)
        {
            Reset();
        }

        public override void Reset(byte[] rom = null)
        {
            base.Reset(rom);
            SP = 0xE7;
        }

        public override bool Halted => Ff;
        public override ulong InstructionPointer => IP;
        protected override IDisassembler Disassembler => new Dissasemble_A8(this);

        public string Console
            =>  new string(ReadAsMmu(0xe8, 0x100 - 0xe8).Select(x => x >= 32 && x <= 0x7f ? (char)x : '.').ToArray());

        public byte Ax => this.ReadRegister8(0);
        public byte Bx => this.ReadRegister8(1);
        public byte Cx => this.ReadRegister8(2);
        public byte Dx => this.ReadRegister8(3);
        public byte SP
        {
            get => this.ReadRegister8(4);
            set => this.WriteRegister8(4, value);
        }
        public byte IP
        {
            get => this.ReadRegister8(5);
            set => this.WriteRegister8(5, value);
        }
        public bool Fc
        {
            get => (this.ReadRegister8(6) & 1) != 0;
            set => SetFlags(value, 1);
        }
        public bool Fz
        {
            get => (this.ReadRegister8(6) & 2) != 0;
            set => SetFlags(value, 2);
        }
        public bool Ff
        {
            get => (this.ReadRegister8(6) & 4) != 0;
            set => SetFlags(value, 4);
        }

        public bool Fs { get; set; }
        public bool Fo { get; set; }
        public bool Fp { get; set; }


        private void SetFlags(bool value, byte selector)
        {
            var flags = this.ReadRegister8(6);
            if (value)
                flags |= selector;
            else
                flags &= (byte)~selector;
            this.WriteRegister8(6, flags);
        }



        public override void Execute(AsmOpcode opcode, int len)
        {
            byte ip = IP;
            byte o1, o2, res, sp;
            uint re1, re2;
            switch (opcode.Opcode)
            {
                case OpcodeMnemonic.Mov:
                    o1 = ReadOperand(opcode.Sources[0]);
                    WriteOperand(opcode.Destinations[0], o1);
                    break;
                case OpcodeMnemonic.Add:
                case OpcodeMnemonic.Sub:
                case OpcodeMnemonic.And:
                case OpcodeMnemonic.Xor:
                case OpcodeMnemonic.Or:
                    o1 = ReadOperand(opcode.Destinations[0]);
                    o2 = ReadOperand(opcode.Sources[0]);
                    res = Arithmetic8(opcode.Opcode, o1, o2);
                    WriteOperand(opcode.Destinations[0], res);
                    break;
                case OpcodeMnemonic.Inc:
                    o1 = ReadOperand(opcode.Destinations[0]);
                    WriteOperand(opcode.Destinations[0], (byte)(o1 + 1));
                    break;
                case OpcodeMnemonic.Dec:
                    o1 = ReadOperand(opcode.Destinations[0]);
                    WriteOperand(opcode.Destinations[0], (byte)(o1 - 1));
                    break;
                case OpcodeMnemonic.Cmp:
                    o1 = ReadOperand(opcode.Destinations[0]);
                    o2 = ReadOperand(opcode.Sources[0]);
                    Arithmetic8(OpcodeMnemonic.Sub, o1, o2);
                    break;
                case OpcodeMnemonic.Jmp:
                    IP = ReadOperand(opcode.Sources[0]);
                    break;
                case OpcodeMnemonic.Jcc:
                    o1 = ReadOperand(opcode.Sources[1]);
                    if (Condtion((Jcc)o1))
                        IP = ReadOperand(opcode.Sources[0]);
                    break;
                case OpcodeMnemonic.Push:
                    o1 = ReadOperand(opcode.Sources[0]);
                    sp = SP;
                    this.WriteMemory8(sp, o1);
                    SP = (byte)(sp - 1);
                    break;
                case OpcodeMnemonic.Pop:
                    sp = (byte)(SP + 1);
                    o1 = this.ReadMemory8(sp);
                    WriteOperand(opcode.Destinations[0], o1);
                    SP = sp;
                    break;
                case OpcodeMnemonic.Call:
                    o1 = (byte)(IP + len);
                    sp = SP;
                    res = ReadOperand(opcode.Sources[0]);
                    this.WriteMemory8(sp, o1);
                    SP = (byte)(sp - 1);
                    IP = res;
                    break;
                case OpcodeMnemonic.Ret:
                    sp = (byte)(SP + 1);
                    o1 = this.ReadMemory8(sp);
                    SP = sp;
                    IP = o1;
                    break;
                case OpcodeMnemonic.Mul:
                    o1 = ReadOperand(opcode.Sources[0]);
                    re1 = (uint)o1 * Ax;
                    this.WriteRegister8(0, (byte)re1);
                    this.WriteRegister8(3, (byte)(re1 >> 8));
                    break;
                case OpcodeMnemonic.Div:
                    byte op = ReadOperand(opcode.Sources[0]);
                    if (Ax == 0)
                    {
                        Ff = true;
                    }
                    else
                    {
                        re1 = (uint)op / Ax;
                        re2 = (uint)op % Ax;
                        this.WriteRegister8(0, (byte)re1);
                        this.WriteRegister8(3, (byte)(re2 >> 8));
                    }
                    break;
                case OpcodeMnemonic.Shl:
                case OpcodeMnemonic.Shr:
                    break;
                case OpcodeMnemonic.Hlt:
                    Ff = true;
                    break;
                default:
                    throw new Exception();
            }

            if (IP == ip)
                IP = (byte)(ip + len);
        }

        public void WriteOperand(AsmOperand operand, byte value)
        {
            if (operand.Type == OperandType.Register && operand.Value < 6)
                this.WriteRegister8((uint)operand.Value, value);
            else if (operand.Type == OperandType.Memory && operand.Value < 6)
                this.WriteMemory8(this.ReadRegister8((uint)operand.Value), value);
            else if (operand.Type == OperandType.Address)
                this.WriteMemory8(operand.Value, value);
            else
                throw new Exception();
        }

        public byte ReadOperand(AsmOperand operand)
        {
            if (operand.Type == OperandType.Register && operand.Value < 6)
                return this.ReadRegister8((uint)operand.Value);
            if (operand.Type == OperandType.Memory && operand.Value < 6)
                return this.ReadMemory8(this.ReadRegister8((uint)operand.Value) + operand.Value2);
            if (operand.Type == OperandType.Address)
                return this.ReadMemory8(operand.Value);
            if (operand.Type == OperandType.Value)
                return (byte)(operand.Value & 0xFF);
            if (operand.Type == OperandType.Special)
                return (byte)(operand.Value & 0xFF);
            throw new Exception();
        }

        private byte Arithmetic8(OpcodeMnemonic op, byte o1, byte o2)
        {
            int res = op switch
            {
                OpcodeMnemonic.Add => o1 + o2,
                OpcodeMnemonic.Adc => o1 + o2 + (Fc ? 1 : 0),
                OpcodeMnemonic.Sub => o1 - o2,
                OpcodeMnemonic.Sbb => o1 - o2 - (Fc ? 1 : 0),
                OpcodeMnemonic.And => (byte)(o1 & o2),
                OpcodeMnemonic.Or => (byte)(o1 | o2),
                OpcodeMnemonic.Xor => (byte)(o1 ^ o2),
                _ => throw new Exception(),
            };
            Fc = (uint)res > 0xFF;
            Fz = res == 0;
            return (byte)res;
        }
        private bool Condtion(Jcc jcc)
        {
            return jcc switch
            {
                // Unsigned
                Jcc.Ja => !Fc || !Fz,
                Jcc.Jnbe => !Fc || !Fz,
                Jcc.Jae => !Fc,
                Jcc.Jnb => !Fc,
                Jcc.Jb => Fc,
                Jcc.Jnae => Fc,
                Jcc.Jbe => Fc || Fz,
                Jcc.Jna => Fc || Fz,
                Jcc.Je => Fz,
                Jcc.Jne => !Fz,
                // Signed
                Jcc.Jg => Fs == Fo && !Fz,
                Jcc.Jnle => Fs == Fo && !Fz,
                Jcc.Jge => Fs == Fo,
                Jcc.Jnl => Fs == Fo,
                Jcc.Jnge => Fs != Fo,
                Jcc.Jl => Fs != Fo,
                Jcc.Jng => Fs != Fo || Fz,
                Jcc.Jle => Fs != Fo || Fz,
                // Flags
                Jcc.Jc => Fc,
                Jcc.Jz => Fz,
                Jcc.Jnc => !Fc,
                Jcc.Jnz => !Fz,
                Jcc.Jnp => !Fp,
                Jcc.Jpo => !Fp,
                Jcc.Jp => Fp,
                Jcc.Jpe => Fp,
                Jcc.Jno => !Fo,
                Jcc.Jns => !Fs,
                Jcc.Jo => Fo,
                Jcc.Js => Fs,
                // Counter
                Jcc.Jcxz => Cx == 0,
                Jcc.Jncxz => Cx != 0,
                _ => throw new Exception(),
            };
        }
    }
}
