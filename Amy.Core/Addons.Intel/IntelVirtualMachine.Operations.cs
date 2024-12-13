using Amy.Core.Bytes;
using Amy.Core.Reflection;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Addons.Intel
{
    public partial class IntelVirtualMachine
    {
        private void ExecAAX(SmoInstruction op)
        {
            throw new NotImplementedException();
        }
        private void ExecALU(SmoInstruction op)
        {
            var dst = ReadOperandValue(op.Operands[0]);
            var src = ReadOperandValue(op.Operands[1]);
            var value = 0l;
            var mx = false;
            var carry = ((IntelFlags)_rflags).HasFlag(IntelFlags.Carry) ? 1 : 0; 

            switch (op.Opcode)
            {
                case IntelMnemonics.ADD:
                    value = dst + src;
                    mx = true;
                    break;
                case IntelMnemonics.ADC:
                    value = dst + src + carry;
                    mx = true;
                    break;
                case IntelMnemonics.SUB:
                case IntelMnemonics.CMP:
                    value = dst - src;
                    mx = true;
                    break;
                case IntelMnemonics.SBB:
                    value = dst - src - carry;
                    mx = true;
                    break;
                case IntelMnemonics.AND:
                case IntelMnemonics.TEST:
                    value = dst & src;
                    break;
                case IntelMnemonics.OR:
                    value = dst | src;
                    break;
                case IntelMnemonics.XOR:
                    value = dst ^ src;
                    break;
            }

            SetFlg(IntelFlags.Zero, value == 0);
            SetFlg(IntelFlags.Parity, (value & 1) == 1);
            SetFlg(IntelFlags.Signed, value < 0);

            if (mx)
            {
                if (op.Operands[0].Size == SmoSize.Byte && value > 0x100)
                    SetFlg(IntelFlags.Carry, true);
                else if (op.Operands[0].Size == SmoSize.Word && value > 0x10000)
                    SetFlg(IntelFlags.Carry, true);
                else if (op.Operands[0].Size == SmoSize.Double && value > 0x100000000)
                    SetFlg(IntelFlags.Carry, true);
                else if (op.Operands[0].Size == SmoSize.Quad)
                    throw new NotImplementedException();
                else
                    SetFlg(IntelFlags.Carry, false);

                SetFlg(IntelFlags.Overflow, Flg(IntelFlags.Carry));
            }
            else
            {
                SetFlg(IntelFlags.Carry, false);
                SetFlg(IntelFlags.Overflow, false);
                SetFlg(IntelFlags.AuxCarry, false);
            }

            SetFlg(IntelFlags.AuxCarry, Flg(IntelFlags.Carry) || false);// TODO  bellow at bit3

            if (op.Opcode != IntelMnemonics.CMP && op.Opcode != IntelMnemonics.TEST)
                WriteOperandValue(op.Operands[0], value);
        }

        private void ExecCALL(SmoInstruction op)
        {
            if (op.Operands[0].Type == SmoOperandType.NearRelativeJmp)
            {
                var tmp = _ip + op.Operands[0].Value;
                Push(_ip);
                _ip = tmp & 0xffffffff;
            }
            else
                throw new NotImplementedException();
        }

        private void ExecCLI(SmoInstruction op)
        {
            if (true) // PE == 0
                SetFlg(IntelFlags.Interrupt, false);
            else if (!((IntelFlags)_rflags).HasFlag(IntelFlags.VirtualMode))
            {
                if (IOpl > Cpl)
                    SetFlg(IntelFlags.Interrupt, false);
                else if (Cpl == 3 && false) // (PVI = 1))
                    SetFlg(IntelFlags.VirtualInterrupt, false);
                else
                    throw new Exception("#GP(0)");
            } 
            else
            {
                if (IOpl == 3)
                    SetFlg(IntelFlags.Interrupt, false);
                else if (false) // (VME = 1)
                    SetFlg(IntelFlags.VirtualInterrupt, false);
                else
                    throw new Exception("#GP(0)");
            }
        }

        private void ExecINC(SmoInstruction op)
        {
            var size = op.Operands[0].Size;
            var src = ReadOperandValue(op.Operands[0]);
            var value = src + 1;
            SetFlg(IntelFlags.Zero, value == 0);
            SetFlg(IntelFlags.Overflow, Msb(src, size) != Msb(value, size)); // TODO !?
            SetFlg(IntelFlags.Parity, (value & 1) == 1);
            SetFlg(IntelFlags.Signed, value < 0);
            SetFlg(IntelFlags.AuxCarry, value == 0 || false); // TODO  bellow at bit3

            WriteOperandValue(op.Operands[0], value); 
        }
        private void ExecJMP(SmoInstruction op)
        {
            var src = op.Operands[0];
            if (src.Type == SmoOperandType.Condition)
            {
                var cc = (SmoJcc)op.Operands[0].Value;
                src = op.Operands[1];
                var fl = cc switch
                {
                    SmoJcc.Ja => throw new NotImplementedException(),
                    SmoJcc.Jnbe => throw new NotImplementedException(),
                    SmoJcc.Jae => !Flg(IntelFlags.Carry),
                    SmoJcc.Jnb => !Flg(IntelFlags.Carry),
                    SmoJcc.Jb => Flg(IntelFlags.Carry),
                    SmoJcc.Jnae => Flg(IntelFlags.Carry),
                    SmoJcc.Jbe => throw new NotImplementedException(),
                    SmoJcc.Jna => throw new NotImplementedException(),
                    SmoJcc.Jg => !(Flg(IntelFlags.Signed) != Flg(IntelFlags.Overflow)) && !Flg(IntelFlags.Zero),
                    SmoJcc.Jnle => !(Flg(IntelFlags.Signed) != Flg(IntelFlags.Overflow)) && !Flg(IntelFlags.Zero),
                    SmoJcc.Jge => Flg(IntelFlags.Signed) == Flg(IntelFlags.Overflow),
                    SmoJcc.Jnl => Flg(IntelFlags.Signed) == Flg(IntelFlags.Overflow),
                    SmoJcc.Jnge => Flg(IntelFlags.Signed) != Flg(IntelFlags.Overflow),
                    SmoJcc.Jl => Flg(IntelFlags.Signed) != Flg(IntelFlags.Overflow),
                    SmoJcc.Jng => (Flg(IntelFlags.Signed) != Flg(IntelFlags.Overflow)) || Flg(IntelFlags.Zero),
                    SmoJcc.Jle => (Flg(IntelFlags.Signed) != Flg(IntelFlags.Overflow)) || Flg(IntelFlags.Zero),
                    SmoJcc.Jc => Flg(IntelFlags.Carry),
                    SmoJcc.Jz => Flg(IntelFlags.Zero),
                    SmoJcc.Je => Flg(IntelFlags.Zero),
                    SmoJcc.Jnc => !Flg(IntelFlags.Carry),
                    SmoJcc.Jnz => !Flg(IntelFlags.Zero),
                    SmoJcc.Jne => !Flg(IntelFlags.Zero),
                    SmoJcc.Jnp => !Flg(IntelFlags.Parity),
                    SmoJcc.Jpo => !Flg(IntelFlags.Parity),
                    SmoJcc.Jp => Flg(IntelFlags.Parity),
                    SmoJcc.Jpe => Flg(IntelFlags.Parity),
                    SmoJcc.Jno => !Flg(IntelFlags.Overflow),
                    SmoJcc.Jns => !Flg(IntelFlags.Signed),
                    SmoJcc.Jo => Flg(IntelFlags.Overflow),
                    SmoJcc.Js => Flg(IntelFlags.Signed),
                    SmoJcc.Jcxz => _cx == 0,
                    SmoJcc.Jncxz => _cx != 0,
                    _ => throw new NotImplementedException(),
                };
                if (!fl)
                    return;
            }

            var mask = src.Size switch
            {
                SmoSize.Word => 0xffffl,
                SmoSize.Double => 0xffffffffl,
                _ => 0,
            };

            if (src.Type == SmoOperandType.NearRelativeJmp)
            {
                var tmp = mask == 0 ? _ip + src.Value : (_ip + src.Value) & mask;
                // TODO -- Check segment
                _ip = tmp;
            }
            else
                throw new NotImplementedException();
        }

        private void ExecLEAVE(SmoInstruction op)
        {
            _sp = _bp;
            _bp = Pop();
        }

        private void ExecMOV(SmoInstruction op)
        {
            var src = ReadOperandValue(op.Operands[1]);
            // CHECK dst == segmetns !
            WriteOperandValue(op.Operands[0], src);
        }
        private void ExecMOVSX(SmoInstruction op)
        {
            var src = ReadOperandValue(op.Operands[1]);
            WriteOperandValue(op.Operands[0], src);
        }
        private void ExecMOVZX(SmoInstruction op)
        {
            var src = ReadOperandValue(op.Operands[1]);
            WriteOperandValue(op.Operands[0], src);
        }
        
        private void ExecOUT(SmoInstruction op)
        {
            var src = ReadOperandValue(op.Operands[1]);
            var dst = ReadRegister(2, SmoSize.Word);
        }

        private void ExecPOP(SmoInstruction op)
        {
            var src = Pop();
            WriteOperandValue(op.Operands[0], src);
        }

        private void ExecPOPA(SmoInstruction op)
        {
            var ax = Pop(8);
            WriteRegister(0, ax, SmoSize.Double);
            for (int i = 1; i < 8; ++i)
                WriteRegister(i, Pop(), SmoSize.Double);
        }
        private void ExecPOPF(SmoInstruction op)
        {
            _rflags = Pop();
        }

        private void ExecPUSH(SmoInstruction op)
        {
            var src = ReadOperandValue(op.Operands[0]);
            Push(src);
        }

        private void ExecPUSHA(SmoInstruction op)
        {
            Push(_ax, 8);
            Push(_cx);
            Push(_dx);
            Push(_bx);
            Push(_sp);
            Push(_bp);
            Push(_si);
            Push(_di);
        }

        private void ExecPUSHF(SmoInstruction op)
        {
            Push(_rflags);
        }

        private void ExecRCL(SmoInstruction op)
        {
            throw new NotImplementedException();
        }
        private void ExecRET(SmoInstruction op)
        {
            var ip = Pop();
            _ip = ip;
        }

        private void ExecSAL(SmoInstruction op)
        {
            var countMask = 0x1f;
            var size = op.Operands[0].Size;
            var dst = ReadOperandValue(op.Operands[0]);
            var src = ReadOperandValue(op.Operands[1]);
            var count = src & countMask;
            var odst = dst;
            while (count != 0)
            {
                if (op.Opcode == IntelMnemonics.SAL || op.Opcode == IntelMnemonics.SHL)
                {
                    SetFlg(IntelFlags.Carry, Msb(dst, size));
                    dst = dst << 1; 
                }
                else if (op.Opcode == IntelMnemonics.SAR)
                {
                    SetFlg(IntelFlags.Carry, Lsb(dst, size));
                    dst = dst / 2;
                }
                else
                {
                    SetFlg(IntelFlags.Carry, Lsb(dst, size));
                    dst = (long)((ulong)dst / 2);
                }
                count--;
            }

            if ((src & countMask) == 1)
            {
                if (op.Opcode == IntelMnemonics.SAL || op.Opcode == IntelMnemonics.SHL)
                    SetFlg(IntelFlags.Overflow, Msb(dst, size) != Flg(IntelFlags.Carry));
                else if (op.Opcode == IntelMnemonics.SAR)
                    SetFlg(IntelFlags.Overflow, false);
                else
                    SetFlg(IntelFlags.Overflow, Msb(odst, size));
            }
        }

        private void ExecSTOS(SmoInstruction op)
        {
            var src = ReadOperandValue(op.Operands[1]);
            var dir = Flg(IntelFlags.Direction) ? -1 : 1;
            if (op.Operands[1].Size == SmoSize.Word)
                dir *= 2;
            else if (op.Operands[1].Size == SmoSize.Double)
                dir *= 4;
            else if (op.Operands[1].Size == SmoSize.Quad)
                dir *= 8;

            if (op.Prefix == "rep")
            {
                while (_cx != 0)
                {
                    WriteOperandValue(op.Operands[0], src);
                    _di += dir;
                    _cx--;
                }
            }
            else
            {
                WriteOperandValue(op.Operands[0], src);
                _di += dir;
                _cx--;
            }
        }




        private void ExecCLFLG(SmoInstruction op, IntelFlags flag)
            => SetFlg(flag, false);

        private void ExecSTFLG(SmoInstruction op, IntelFlags flag)
            => SetFlg(flag, true);
    }
}
