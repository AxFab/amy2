using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App
{
    public interface IDisassembler
    {
        AsmOpcode Disassemble();
    }

    public abstract partial class AbstractVm : IByteReader
    {
        protected abstract IDisassembler Disassembler { get; }

        public abstract bool Halted { get; }
        public abstract ulong InstructionPointer { get; }

        public abstract void Execute(AsmOpcode opCode, int opLength);

        public void Execute()
        {
            int cycles = 0;
            var dissembler = Disassembler;
            while (!Halted)
            {
                ((IByteReader)this).Continue();
                var op = dissembler.Disassemble();
                var bf = ((IByteReader)this).ReadLast();
                Execute(op, bf.Length);
                cycles++;
            }
        }

        private uint _ipReading = 0;
        byte IByteReader.NextByte() => GetMemory(InstructionPointer + _ipReading++, 1)[0];
        int IByteReader.ReadLength() => (int)_ipReading;
        bool IByteReader.Continue()
        {
            _ipReading = 0;
            return true;
        }
        byte[] IByteReader.ReadLast()
        {
            byte[] buf = new byte[_ipReading];
            for (uint i = 0; i < _ipReading; ++i)
                buf[i] = GetMemory(InstructionPointer + i, 1)[0];
            return buf;
        }

        public abstract byte[] GetRegister(uint rego);
        public abstract void PutRegister(uint rego, byte[] bytes);
        public abstract byte[] GetMemory(ulong addr, uint size);
        public abstract void PutMemory(ulong addr, byte[] bytes);
    }


    // Read write on memory and registers 
    public static class AbstractVmReadWrite 
    {
        public static byte ReadRegister8(this AbstractVm vm, uint rego)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 1)
                throw new ArgumentException("The number provided for the register is invalid");
            return bytes[0];
        }
        public static ushort ReadRegister16(this AbstractVm vm, uint rego)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 2)
                throw new ArgumentException("The number provided for the register is invalid");
            return (ushort)(bytes[0] | (bytes[1] << 8));
        }
        public static uint ReadRegister32(this AbstractVm vm, uint rego)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 4)
                throw new ArgumentException("The number provided for the register is invalid");
            return (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
        }
        public static ulong ReadRegister64(this AbstractVm vm, uint rego)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 8)
                throw new ArgumentException("The number provided for the register is invalid");
            var low = (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
            var hig = (uint)(bytes[4] | (bytes[5] << 8) | (bytes[6] << 16) | (bytes[7] << 24));
            return low | (ulong)hig << 32;
        }

        public static void WriteRegister8(this AbstractVm vm, uint rego, byte val)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 1)
                throw new ArgumentException("The number provided for the register is invalid");
            bytes[0] = val;
            vm.PutRegister(rego, bytes);
        }
        public static void WriteRegister16(this AbstractVm vm, uint rego, ushort val)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 2)
                throw new ArgumentException("The number provided for the register is invalid");
            bytes[0] = (byte)val;
            bytes[1] = (byte)(val >> 8);
            vm.PutRegister(rego, bytes);
        }
        public static void WriteRegister32(this AbstractVm vm, uint rego, uint val)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 4)
                throw new ArgumentException("The number provided for the register is invalid");
            bytes[0] = (byte)val;
            bytes[1] = (byte)(val >> 8);
            bytes[2] = (byte)(val >> 16);
            bytes[3] = (byte)(val >> 24);
            vm.PutRegister(rego, bytes);
        }
        public static void WriteRegister64(this AbstractVm vm, uint rego, ulong val)
        {
            var bytes = vm.GetRegister(rego);
            if (bytes == null || bytes.Length < 8)
                throw new ArgumentException("The number provided for the register is invalid");
            bytes[0] = (byte)val;
            bytes[1] = (byte)(val >> 8);
            bytes[2] = (byte)(val >> 16);
            bytes[3] = (byte)(val >> 24);
            bytes[4] = (byte)(val >> 32);
            bytes[5] = (byte)(val >> 40);
            bytes[6] = (byte)(val >> 48);
            bytes[7] = (byte)(val >> 56);
            vm.PutRegister(rego, bytes);
        }

        public static byte ReadMemory8(this AbstractVm vm, ulong addr)
        {
            var bytes = vm.GetMemory(addr, 1);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            return bytes[0];
        }
        public static ushort ReadMemory16(this AbstractVm vm, ulong addr)
        {
            var bytes = vm.GetMemory(addr, 2);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            return (ushort)(bytes[0] | (bytes[1] << 8));
        }
        public static uint ReadMemory32(this AbstractVm vm, ulong addr)
        {
            var bytes = vm.GetMemory(addr, 4);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            return (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
        }
        public static ulong ReadMemory64(this AbstractVm vm, ulong addr)
        {
            var bytes = vm.GetMemory(addr, 8);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            var low = (uint)(bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24));
            var hig = (uint)(bytes[4] | (bytes[5] << 8) | (bytes[6] << 16) | (bytes[7] << 24));
            return low | (ulong)hig << 32;
        }

        public static void WriteMemory8(this AbstractVm vm, ulong addr, byte val)
        {
            var bytes = vm.GetMemory(addr, 1);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            bytes[0] = val;
            vm.PutMemory(addr, bytes);
        }
        public static void WriteMemory16(this AbstractVm vm, ulong addr, ushort val)
        {
            var bytes = vm.GetMemory(addr, 2);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            bytes[0] = (byte)val;
            bytes[1] = (byte)(val >> 8);
            vm.PutMemory(addr, bytes);
        }
        public static void WriteMemory32(this AbstractVm vm, ulong addr, uint val)
        {
            var bytes = vm.GetMemory(addr, 4);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            bytes[0] = (byte)val;
            bytes[1] = (byte)(val >> 8);
            bytes[2] = (byte)(val >> 16);
            bytes[3] = (byte)(val >> 24);
            vm.PutMemory(addr, bytes);
        }
        public static void WriteMemory64(this AbstractVm vm, ulong addr, ulong val)
        {
            var bytes = vm.GetMemory(addr, 8);
            if (bytes == null)
                throw new ArgumentException("Memory data query is invalid");
            bytes[0] = (byte)val;
            bytes[1] = (byte)(val >> 8);
            bytes[2] = (byte)(val >> 16);
            bytes[3] = (byte)(val >> 24);
            bytes[4] = (byte)(val >> 32);
            bytes[5] = (byte)(val >> 40);
            bytes[6] = (byte)(val >> 48);
            bytes[7] = (byte)(val >> 56);
            vm.PutMemory(addr, bytes);
        }

    }






    // Read Write as Operands
    public class OperandReader 
    {
        protected AbstractVm _vm;
        protected WordSize _addrSize;
        protected WordSize _maxDataSize;
        private uint _sibExtraReg = 4;
        public OperandReader () {
            // Size supported !?
            // Operation supported !?
        }

        private ulong Cast(ulong value, WordSize size)
        {
            return size switch
            {
                WordSize.Byte => (byte)value,
                WordSize.Word => (ushort)value,
                WordSize.Double => (uint)value,
                WordSize.Quad => value,
                _ => throw new NotImplementedException(),
            };
        }
        
        private ulong ReadRegisterW(uint rego)
        {
            return _addrSize switch
            {
                WordSize.Byte => _vm.ReadRegister8(rego),
                WordSize.Word => _vm.ReadRegister16(rego),
                WordSize.Double => _vm.ReadRegister32(rego),
                WordSize.Quad => _vm.ReadRegister64(rego),
                _ => throw new NotImplementedException(),
            };
        }

        private ulong ReadRegisterX(uint rego, WordSize size)
        {
            if (size == WordSize.Byte)
                return _vm.ReadRegister8(rego);
            else if (size == WordSize.Word)
                return _vm.ReadRegister16(rego);
            else if (size == WordSize.Double)
                return _vm.ReadRegister32(rego);
            else if (size == WordSize.Quad)
                return _vm.ReadRegister64(rego);
            else
                throw new NotImplementedException("Don't support operand larger than 64bits");
        }

        private ulong ReadMemoryX(ulong addr, WordSize size)
        {
            if (size == WordSize.Byte)
                return _vm.ReadMemory8(addr);
            else if (size == WordSize.Word)
                return _vm.ReadMemory16(addr);
            else if (size == WordSize.Double)
                return _vm.ReadMemory32(addr);
            else if (size == WordSize.Quad)
                return _vm.ReadMemory64(addr);
            else
                throw new NotImplementedException("Don't support operand larger than 64bits");
        }

        private void WriteRegisterX(uint rego, WordSize size, ulong value)
        {
            if (size == WordSize.Byte)
                _vm.WriteRegister8(rego, (byte)value);
            else if (size == WordSize.Word)
                _vm.WriteRegister16(rego, (ushort)value);
            else if (size == WordSize.Double)
                _vm.WriteRegister32(rego, (uint)value);
            else if (size == WordSize.Quad)
                _vm.WriteRegister64(rego, value);
            else
                throw new NotImplementedException("Don't support operand larger than 64bits");
        }

        private void WriteMemoryX(ulong addr, WordSize size, ulong value)
        {
            if (size == WordSize.Byte)
                _vm.WriteMemory8(addr, (byte)value);
            else if (size == WordSize.Word)
                _vm.WriteMemory16(addr, (ushort)value);
            else if (size == WordSize.Double)
                _vm.WriteMemory32(addr, (uint)value);
            else if (size == WordSize.Quad)
                _vm.WriteMemory64(addr, value);
            else
                throw new NotImplementedException("Don't support operand larger than 64bits");
        }

        public ulong ReadOperand(AsmOperand operand)
        {
            if (operand.Word > _maxDataSize)
                throw new Exception();
            switch (operand.Type)
            {
                case OperandType.Register:
                    // IsReadbleRegister 
                    return ReadRegisterX((uint)operand.Value, operand.Word);
                case OperandType.Address:
                    return ReadMemoryX(operand.Value, operand.Word);
                case OperandType.Memory:
                    // IsReadbleRegister 
                    return ReadMemoryX(ReadRegisterW((uint)operand.Value) + operand.Value2, operand.Word);
                case OperandType.Value:
                    return Cast(operand.Value, operand.Word);
                case OperandType.Sib1:
                case OperandType.Sib2:
                case OperandType.Sib3:
                    {
                        // Is Supported !?
                        var index = ReadRegisterW((uint)operand.Value);
                        var scale = operand.Value2;
                        var off = operand.Type == OperandType.Sib1 ? 0 : operand.Value3;
                        var basev = 0ul;
                        if (operand.Type == OperandType.Sib1)
                            basev = ReadRegisterW((uint)operand.Value3);
                        else if (operand.Type == OperandType.Sib3)
                            basev = ReadRegisterW(_sibExtraReg);
                        return ReadMemoryX(scale * index + off + basev, operand.Word);
                    }

            }

            throw new Exception();
        }

        public void WriteOperand(AsmOperand operand, ulong value)
        {
            if (operand.Word > _maxDataSize)
                throw new Exception();
            switch (operand.Type)
            {
                case OperandType.Register:
                    // IsReadbleRegister 
                    WriteRegisterX((uint)operand.Value, operand.Word, value);
                    break;
                case OperandType.Address:
                    WriteMemoryX(operand.Value, operand.Word, value);
                    break;
                case OperandType.Memory:
                    // IsReadbleRegister 
                    WriteMemoryX(ReadRegisterW((uint)operand.Value) + operand.Value2, operand.Word, value);
                    break;
                case OperandType.Sib1:
                case OperandType.Sib2:
                case OperandType.Sib3:
                    {
                        // Is Supported !?
                        var index = ReadRegisterW((uint)operand.Value);
                        var scale = operand.Value2;
                        var off = operand.Type == OperandType.Sib1 ? 0 : operand.Value3;
                        var basev = 0ul;
                        if (operand.Type == OperandType.Sib1)
                            basev = ReadRegisterW((uint)operand.Value3);
                        else if (operand.Type == OperandType.Sib3)
                            basev = ReadRegisterW(_sibExtraReg);
                        WriteMemoryX(scale * index + off + basev, operand.Word, value);
                        break;
                    }

            }

            throw new Exception();
        }
    }

    // Operations
    public abstract class OpcodeExecutor : OperandReader
    {
        public bool Fc { get => GetFlag('c'); set => PutFlag('c', value); }
        public bool Fz { get => GetFlag('z'); set => PutFlag('z', value); }
        public bool Fs { get => GetFlag('s'); set => PutFlag('s', value); }
        public bool Fo { get => GetFlag('o'); set => PutFlag('o', value); }
        public bool Fp { get => GetFlag('p'); set => PutFlag('p', value); }
        public abstract ulong SP { get; set; }
        public abstract ulong IP { get; set; }
        public abstract void PutFlag(char f, bool b);
        public abstract bool GetFlag(char f);
        public abstract void RaiseException();


        public void Execute(AsmOpcode opcode, uint len)
        {
            var ip = IP;
            switch (opcode.Opcode)
            {
                case OpcodeMnemonic.Nop:
                    break;
                case OpcodeMnemonic.Mov:
                    Move(opcode.Destinations[0], opcode.Sources[0]);
                    break;
                case OpcodeMnemonic.Add:
                case OpcodeMnemonic.Sub:
                case OpcodeMnemonic.Adc:
                case OpcodeMnemonic.Sbb:
                case OpcodeMnemonic.And:
                case OpcodeMnemonic.Or:
                case OpcodeMnemonic.Xor:
                case OpcodeMnemonic.Cmp:
                case OpcodeMnemonic.Test:
                    Arithmetic(opcode.Opcode, opcode.Destinations[0], opcode.Sources[0], opcode.Sources[1]);
                    break;
                case OpcodeMnemonic.Inc:
                case OpcodeMnemonic.Dec:
                case OpcodeMnemonic.Not:
                case OpcodeMnemonic.Neg:
                    Arithmetic(opcode.Opcode, opcode.Destinations[0], opcode.Sources[0]);
                    break;
                case OpcodeMnemonic.Push:
                    Push(opcode.Sources[0]);
                    break;
                case OpcodeMnemonic.Pop:
                    Push(opcode.Destinations[0]);
                    break;
                case OpcodeMnemonic.Call:
                    Call(opcode.Sources[0], len);
                    break;
                case OpcodeMnemonic.Ret:
                    Ret();
                    break;
                case OpcodeMnemonic.Jmp:
                    Jmp(opcode.Sources[0]);
                    break;
                case OpcodeMnemonic.Jcc:
                    JumpCc(opcode.Sources[0], opcode.Sources[1]);
                    break;
                case OpcodeMnemonic.Mul:
                    Mul(opcode.Destinations[0], opcode.Destinations[1], opcode.Sources[0], opcode.Sources[1]);
                    break;
                // case OpcodeMnemonic.IMul:
                case OpcodeMnemonic.Div:
                    Div(opcode.Destinations[0], opcode.Destinations[1], opcode.Sources[0], opcode.Sources[1]);
                    break;
                // case OpcodeMnemonic.IDiv:
                case OpcodeMnemonic.Shl:
                case OpcodeMnemonic.Shr:
                case OpcodeMnemonic.Sal:
                case OpcodeMnemonic.Sar:
                case OpcodeMnemonic.Rol:
                case OpcodeMnemonic.Ror:
                case OpcodeMnemonic.Rcr:
                case OpcodeMnemonic.Rcl:
                    Shift(opcode.Opcode, opcode.Destinations[0], opcode.Sources[0], opcode.Sources[1]);
                    break;
                case OpcodeMnemonic.Hlt:
                default:
                    throw new Exception();
            }

            if (IP == ip)
                IP = ip + len;
        }


        public void Arithmetic(OpcodeMnemonic opcode, AsmOperand destination, AsmOperand source1, AsmOperand source2)
        {
            ulong val1 = ReadOperand(source1);
            ulong val2 = ReadOperand(source2);
            ulong value = opcode switch
            {
                OpcodeMnemonic.Add => val1 + val2,
                OpcodeMnemonic.Sub => val1 - val2,
                OpcodeMnemonic.Adc => val1 + val2,
                OpcodeMnemonic.Sbb => val1 - val2,
                OpcodeMnemonic.And => val1 & val2,
                OpcodeMnemonic.Or => val1 | val2,
                OpcodeMnemonic.Xor => val1 ^ val2,
                // OpcodeMnemonic.Shl => val1 << (int)val2,
                // OpcodeMnemonic.Shr => val1 >> (int)val2,
                OpcodeMnemonic.Cmp => val1 - val2,
                OpcodeMnemonic.Test => val1 ^ val2,
                _ => throw new Exception(),
            };
            value = ResultSetFlags(value, destination.Word);
            if (opcode != OpcodeMnemonic.Cmp && opcode != OpcodeMnemonic.Test) 
                WriteOperand(destination, value);
        }
        public void Arithmetic(OpcodeMnemonic opcode, AsmOperand destination, AsmOperand source)
        {
            ulong value = ReadOperand(source);
            value = opcode switch
            {
                // OpcodeMnemonic.Mov => value,
                OpcodeMnemonic.Inc => value + 1,
                OpcodeMnemonic.Dec => value - 1,
                OpcodeMnemonic.Not => ~value,
                OpcodeMnemonic.Neg => ~value + 1,
                _ => throw new Exception(),
            };
            value = ResultSetFlags(value, destination.Word);
            WriteOperand(destination, value);
        }
        public void Shift(OpcodeMnemonic opcode, AsmOperand destination, AsmOperand source1, AsmOperand source2)
        {
            ulong val1 = ReadOperand(source1);
            ulong val2 = ReadOperand(source2);
            ulong value = val1;

            ulong count = destination.Word switch
            {
                WordSize.Byte => (val2 & 0x1F) % 9,
                WordSize.Word => (val2 & 0x1F) % 17,
                WordSize.Double => (val2 & 0x1F),
                WordSize.Quad => (val2 & 0x3F),
                _ => throw new Exception(),
            };

            ulong msb = destination.Word switch
            {
                WordSize.Byte => 0x80,
                WordSize.Word => 0x8000,
                WordSize.Double => 0x80000000,
                WordSize.Quad => 0x8000000000000000,
                _ => throw new Exception(),
            };


            for (int i = 0; i < (int)count; ++i)
            {
                bool bk;
                switch (opcode)
                {
                    case OpcodeMnemonic.Shl:
                    case OpcodeMnemonic.Sal:
                        Fc = (value & msb) != 0;
                        value = value * 2;
                        break;
                    case OpcodeMnemonic.Shr:
                        Fc = (value & 1) != 0;
                        value = value / 2;
                        break;
                    case OpcodeMnemonic.Sar:
                        Fc = (value & 1) != 0;
                        value = (value & msb) | (value / 2);
                        break;
                    case OpcodeMnemonic.Rol:
                        Fc = (value & msb) != 0;
                        value = value * 2 + (Fc ? 1UL : 0UL);
                        break;
                    case OpcodeMnemonic.Rcl:
                        bk = (val1 & msb) != 0;
                        value = value * 2 + (Fc ? 1UL : 0UL);
                        Fc = bk;
                        break;
                    case OpcodeMnemonic.Ror:
                        Fc = (value & 1) != 0;
                        value = value / 2 + (Fc ? msb : 0UL);
                        break;
                    case OpcodeMnemonic.Rcr:
                        bk = (value & 1) != 0;
                        value = value / 2 + (Fc ? msb : 0UL);
                        Fc = bk;
                        break;
                }
            }

            switch (opcode)
            {
                case OpcodeMnemonic.Shl:
                case OpcodeMnemonic.Sal:
                case OpcodeMnemonic.Rol:
                case OpcodeMnemonic.Rcl:
                    Fo = Fc != ((value & msb) != 0); 
                    break;
                case OpcodeMnemonic.Shr:
                    Fo = (val1 & msb) != 0;
                    break;
                case OpcodeMnemonic.Sar:
                    Fo = false;
                    break;
                case OpcodeMnemonic.Ror:
                    Fo = Fc != ((val1 & msb) != 0);
                    break;
                case OpcodeMnemonic.Rcr:
                    Fo = Fc != ((value & msb) != 0);
                    break;
            }

            WriteOperand(destination, value);
        }
        public void Push (AsmOperand source)
        {
            ulong value = ReadOperand(source);
            PushStack(value);
        }
        public void Pop(AsmOperand destination)
        {
            ulong value = PopStack();
            WriteOperand(destination, value);
        }
        public void Call(AsmOperand source, uint opLength)
        {
            ulong ip = IP + opLength;
            ulong value = ReadOperand(source);
            PushStack(ip);
            IP = value;
        }
        public void Ret()
        {
            IP = PopStack();
        }
        public void Jmp(AsmOperand source)
        {
            IP = ReadOperand(source);
        }
        public void JumpCc(AsmOperand source, AsmOperand condition)
        {
            var jcc = (Jcc)condition.Value;
            if (Condition(jcc))
                IP = ReadOperand(source);
        }
        public void Move(AsmOperand destination, AsmOperand source)
        {
            ulong value = ReadOperand(source);
            WriteOperand(destination, value);
        }
        public void Mul(AsmOperand destination1, AsmOperand? destination2, AsmOperand source1, AsmOperand source2)
        {
            ulong val1 = ReadOperand(source1);
            ulong val2 = ReadOperand(source2);
            ulong value = val1 * val2;

            switch (destination1.Word)
            {
                case WordSize.Byte:
                    PutFlag('z', (value & 0xFFFF) == 0);
                    PutFlag('c', (value & ~0xFFUL) != 0);
                    PutFlag('s', false);
                    PutFlag('o', (value & ~0xFFUL) != 0);
                    WriteOperand(destination1, value & 0xFF);
                    if (destination2 != null)
                        WriteOperand(destination2, (value >> 8) & 0xFF);
                    break;
                case WordSize.Word:
                    PutFlag('z', (value & 0xFFFFFFFF) == 0);
                    PutFlag('c', (value & ~0xFFFFUL) != 0);
                    PutFlag('s', false);
                    PutFlag('o', (value & ~0xFFFFUL) != 0);
                    WriteOperand(destination1, value & 0xFFFF);
                    if (destination2 != null)
                        WriteOperand(destination2, (value >> 16) & 0xFFFF);
                    break;
                case WordSize.Double:
                    PutFlag('z', value == 0);
                    PutFlag('c', (value & ~0xFFFFFFFFUL) != 0);
                    PutFlag('s', false);
                    PutFlag('o', (value & ~0xFFFFFFFFUL) != 0);
                    WriteOperand(destination1, value & 0xFFFFFFFF);
                    if (destination2 != null)
                        WriteOperand(destination2, (value >> 32) & 0xFFFFFFFF);
                    break;
                default:
                    throw new Exception();
            }
        }
        public void Div(AsmOperand destination1, AsmOperand? destination2, AsmOperand source1, AsmOperand source2)
        {
            ulong val1 = ReadOperand(source1);
            ulong val2 = ReadOperand(source2);
            if (val2 == 0)
            {
                RaiseException();
                return;
            }
            ulong div = val1 / val2;
            ulong mod = val1 % val2;
            WriteOperand(destination1, div);
            if (destination2 != null)
                WriteOperand(destination2, mod);
        }


        private void PushStack(ulong value)
        {
            switch (_addrSize)
            {
                case WordSize.Byte:
                    _vm.WriteMemory8(SP, (byte)value);
                    SP = SP - 1;
                    break;
                case WordSize.Word:
                    _vm.WriteMemory16(SP, (ushort)value);
                    SP = SP - 2;
                    break;
                case WordSize.Double:
                    _vm.WriteMemory32(SP, (uint)value);
                    SP = SP - 4;
                    break;
                case WordSize.Quad:
                    _vm.WriteMemory64(SP, value);
                    SP = SP - 8;
                    break;
                default:
                    throw new Exception();
            }
        }
        private ulong PopStack()
        {
            switch (_addrSize)
            {
                case WordSize.Byte:
                    SP = SP + 1;
                    return _vm.ReadMemory8(SP);
                case WordSize.Word:
                    SP = SP + 2;
                    return _vm.ReadMemory16(SP);
                case WordSize.Double:
                    SP = SP + 4;
                    return _vm.ReadMemory32(SP);
                case WordSize.Quad:
                    SP = SP + 8;
                    return _vm.ReadMemory64(SP);
                default:
                    throw new Exception();
            };
        }
        private bool Condition(Jcc jcc)
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
                // Jcc.Jcxz => Cx == 0,
                // Jcc.Jncxz => Cx != 0,
                _ => throw new Exception(),
            };
        }
        private ulong ResultSetFlags(ulong value, WordSize word)
        {
            int sz = 8;
            if (word == WordSize.Byte)
            {
                PutFlag('z', (value & 0xFF) == 0);
                PutFlag('c', (value & 0x100) != 0);
                PutFlag('s', (value & 0x80) != 0);
                PutFlag('o', (value & ~0xFFUL) != 0);
                return value & 0xFF;
            }
            else if (word == WordSize.Word)
            {
                PutFlag('z', (value & 0xFFFF) == 0);
                PutFlag('c', (value & 0x10000) != 0);
                PutFlag('s', (value & 0x8000) != 0);
                PutFlag('o', (value & ~0xFFFFUL) != 0);
                return value & 0xFFFF;
            }
            else if (word == WordSize.Double)
            {
                PutFlag('z', (value & 0xFFFFFFFF) == 0);
                PutFlag('c', (value & 0x100000000) != 0);
                PutFlag('s', (value & 0x80000000) != 0);
                PutFlag('o', (value & ~0xFFFFFFFFUL) != 0);
                return value & 0xFFFFFFFF;
            }
            else if (word == WordSize.Quad)
            {
                PutFlag('z', value == 0);
                // SetFlag('c', (value & 0x100000000) != 0);
                PutFlag('s', (value & 0x8000000000000000) != 0);
                // SetFlag('o', (value & ~0xFFFFFFFFUL) != 0);
                throw new Exception();
                return value;
            }

            throw new Exception();
        }
    }






    // Set Word as byte / Set Memory as 256bis in mem
    public abstract class AbstractInMemoryVm : AbstractVm
    {
        private readonly byte[] _memory;
        private readonly byte[] _registers;
        private readonly int _memoryLength;
        private readonly int _memoryBandsize;
        private readonly int _registerLength;
        private readonly int _registerCount;

        public AbstractInMemoryVm(int memoryLength, int registerLength, int registerCount, int memoryBandsize = 8)
        {
            _memoryLength = memoryLength;
            _memoryBandsize = memoryBandsize;
            _registerLength = registerLength;
            _registerCount = registerCount;
            _memory = new byte[memoryLength];
            _registers = new byte[registerLength * registerCount];
        }

        public override byte[] GetMemory(ulong addr, uint size)
        {
            if (size > _memoryBandsize)
                throw new ArgumentException("memory request is to large for 8bits platform");
            return new byte[] { _memory[addr & 0xff] };
        }

        public override void PutMemory(ulong addr, byte[] bytes)
        {
            if (bytes.Length > _memoryBandsize)
                throw new ArgumentException("memory request is to large for 8bits platform");
            _memory[addr & 0xff] = bytes[0];
        }

        public override byte[] GetRegister(uint rego)
        {
            if (rego >= _registerCount)
                throw new ArgumentException("Bad register info");
            return new byte[] { _registers[rego] };
        }

        public override void PutRegister(uint rego, byte[] bytes)
        {
            if (bytes.Length > _registerLength || rego >= _registerCount)
                throw new ArgumentException("Bad register info");
            _registers[rego] = bytes[0];
        }

        public virtual void Reset(byte[] rom = null)
        {
            for (int i = 0; i < _registers.Length; i++)
                _registers[i] = 0;
            if (rom != null)
            {
                for (int i = 0; i < _memory.Length; i++)
                    _memory[i] = i < rom.Length ? rom[i] : (byte)0;
            } else
            {
                for (int i = 0; i < _memory.Length; i++)
                    _memory[i] = 0;
            }
        }

        protected byte[] ReadAsMmu(int offset, int length)
        {
            var buf = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buf[i] = _memory[i + offset];
            }
            return buf;
        }
    }
}
