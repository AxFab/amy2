
using Amy.Core.Bytes;

namespace Amy.Core.Addons.Intel
{
    [Flags]
    enum IntelFlags
    {
        Carry = 1,
        Reserved = 1 << 1,
        Parity = 1 << 2,
        AuxCarry = 1 << 4,
        Zero = 1 << 6,
        Signed = 1 << 7,
        Trap = 1 << 8,
        Interrupt = 1 << 9,
        Direction = 1 << 10,
        Overflow = 1 << 11,
        Nested = 1 << 14,
        Resume = 1 << 16,
        VirtualMode = 1 << 17,
        AlignCheck = 1 << 18,
        AccessControl = 1 << 18,
        VirtualInterrupt = 1 << 19,
        VirtualInterruptPending = 1 << 20,
        IDFlags = 1 << 21,
    }

    public partial class IntelVirtualMachine
    {
        public IntelVirtualMemory Memory { get; } = new IntelVirtualMemory();
        public IntelDisassembler Disassembler { get; } = new IntelDisassembler();
        public IntelWriter AsmWriter { get; } = new IntelWriter();

        private ElfFile Elf { get; set; }
        public static IntelVirtualMachine LoadKernel(string uri)
        {
            var stream = File.OpenRead(uri);
            var elf = new ElfFile(stream);
            elf.Filename = "img/boot/kora-i386.krn";
            elf.OpenHeader(false);

            var vm = new IntelVirtualMachine();

            // Load kernel
            vm._ip = (long)elf.Entry;
            for (int i = 0; i < elf.PhCount; ++i)
            {
                var head = elf.ReadSector(i);
                if (head.Type != 1)
                    continue;
                var offset = head.VirtAddr;
                var size = (head.VirtSize + (vm.Memory.PageSize - 1)) & ~(vm.Memory.PageSize - 1);
                var pageCount = size / vm.Memory.PageSize;
                for (int j = 0; j < pageCount; ++j)
                {
                    var buf = vm.Memory.FetchPage(offset / vm.Memory.PageSize + j);
                    elf.LoadPhContent(head, j * vm.Memory.PageSize, buf);
                }
            }

            // Multiboot
            vm._ax = 0x2bad;
            vm._bx = 0x10000;

            vm.Elf = elf;
            return vm;
        }

        // General-purpose registers
        private long _ax;
        private long _cx;
        private long _dx;
        private long _bx;
        private long _sp;
        private long _bp;
        private long _si;
        private long _di;

        private long _r8;
        private long _r9;
        private long _r10;
        private long _r11;
        private long _r12;
        private long _r13;
        private long _r14;
        private long _r15;

        // segment registers
        private ushort _cs;
        private ushort _ss;
        private ushort _ds;
        private ushort _es;
        private ushort _fs;
        private ushort _gs;

        // Program registers
        private long _rflags;
        private long _ip;

        // x87 register
        private long _st0;
        private long _st1;
        private long _st2;
        private long _st3;
        private long _st4;
        private long _st5;
        private long _st6;
        private long _st7;

        // MMX registers

        // Control register
        private long _cr0;
        private long _cr1;
        private long _cr2;
        private long _cr3;

        private long _gdtr;
        private long _ldtr;
        private long _idtr;
        private long _tskr;

        private long _dr0;
        private long _dr1;
        private long _dr2;
        private long _dr3;
        private long _dr6;
        private long _dr7;

        public int IOpl => (int)((_rflags >> 12) & 3);
        public int Cpl => 3;


        public void Execute()
        {
            Memory.Segment = _cs;
            Memory.Address = _ip;
            var op = Disassembler.NextInstruction(Memory);
            var buf = Memory.ReadLast();

            AsmWriter.Display(Console.Out, op, Elf, _ip, buf);
            _ip += buf.Length;
            // Print operand

            switch (op.Opcode.ToString().ToUpper())
            {
                case IntelMnemonics.AAA:
                case IntelMnemonics.AAD:
                case IntelMnemonics.AAM:
                case IntelMnemonics.AAS:
                    ExecAAX(op);
                    break;
                case IntelMnemonics.ADC:
                case IntelMnemonics.ADD:
                case IntelMnemonics.AND:
                case IntelMnemonics.ANDN:
                case IntelMnemonics.CMP:
                case IntelMnemonics.OR:
                case IntelMnemonics.SBB:
                case IntelMnemonics.SUB:
                case IntelMnemonics.TEST:
                case IntelMnemonics.XOR:
                    ExecALU(op);
                    break;

                case IntelMnemonics.ADCX:
                case IntelMnemonics.ADDPD:
                case IntelMnemonics.ADDPS:
                case IntelMnemonics.ADDSD:
                case IntelMnemonics.ADDSS:
                case IntelMnemonics.ADDSUBPD:
                case IntelMnemonics.ADDSUBPS:
                case IntelMnemonics.ADOX:
                case IntelMnemonics.AESDEC:
                case IntelMnemonics.AESDECLAST:
                case IntelMnemonics.AESENC:
                case IntelMnemonics.AESENCLAST:
                case IntelMnemonics.AESIMC:
                case IntelMnemonics.AESKEYGENASSIST:
                case IntelMnemonics.ANDPD:
                case IntelMnemonics.ANDPS:
                case IntelMnemonics.ANDNPD:
                case IntelMnemonics.ANDNPS:
                case IntelMnemonics.ARPL:
                case IntelMnemonics.BEXTR:
                case IntelMnemonics.BLENDPD:
                case IntelMnemonics.BLENDPS:
                case IntelMnemonics.BLENDVPD:
                case IntelMnemonics.BLENDVPS:
                case IntelMnemonics.BLSI:
                case IntelMnemonics.BLSMSK:
                case IntelMnemonics.BLSR:
                case IntelMnemonics.BNDCL:
                case IntelMnemonics.BNDCU:
                case IntelMnemonics.BNDCN:
                case IntelMnemonics.BNDLDX:
                case IntelMnemonics.BNDMK:
                case IntelMnemonics.BNDMOV:
                case IntelMnemonics.BNDSTX:
                case IntelMnemonics.BOUND:
                case IntelMnemonics.BSF:
                case IntelMnemonics.BSR:
                case IntelMnemonics.BSWAP:
                case IntelMnemonics.BT:
                case IntelMnemonics.BTC:
                case IntelMnemonics.BTR:
                case IntelMnemonics.BTS:
                case IntelMnemonics.BZHI:
                    throw new NotImplementedException();

                case IntelMnemonics.CALL:
                    ExecCALL(op);
                    break;

                case IntelMnemonics.CBW:
                case IntelMnemonics.CWDE:
                case IntelMnemonics.CDQE:
                    throw new NotImplementedException();

                case IntelMnemonics.CLAC:
                    ExecCLFLG(op, IntelFlags.AuxCarry);
                    break;
                case IntelMnemonics.CLC:
                    ExecCLFLG(op, IntelFlags.Carry);
                    break;
                case IntelMnemonics.CLD:
                    ExecCLFLG(op, IntelFlags.Direction);
                    break;

                case IntelMnemonics.CLFLUSH:
                    throw new NotImplementedException();
                case IntelMnemonics.CLI:
                    ExecCLI(op);
                    break;
                case IntelMnemonics.CLTS:
                case IntelMnemonics.CMC:
                case IntelMnemonics.CMOVcc:
                case IntelMnemonics.CMPPD:
                case IntelMnemonics.CMPPS:
                case IntelMnemonics.CMPS:
                case IntelMnemonics.CMPSB:
                case IntelMnemonics.CMPSW:
                case IntelMnemonics.CMPSD:
                case IntelMnemonics.CMPSQ:
                case IntelMnemonics.CMPSS:
                case IntelMnemonics.CMPXCHG:
                case IntelMnemonics.CMPXCHG8B:
                case IntelMnemonics.CMPXCHG816B:
                case IntelMnemonics.COMISD:
                case IntelMnemonics.COMISS:
                case IntelMnemonics.CPUID:
                case IntelMnemonics.CRC32:
                case IntelMnemonics.CVTDQ2PD:
                case IntelMnemonics.CVTDQ2PS:
                case IntelMnemonics.CVTPD2DQ:
                case IntelMnemonics.CVTPD2PI:
                case IntelMnemonics.CVTPD2PS:
                case IntelMnemonics.CVTPI2PD:
                case IntelMnemonics.CVTPI2PS:
                case IntelMnemonics.CVTPS2DQ:
                case IntelMnemonics.CVTPS2PD:
                case IntelMnemonics.CVTPS2PI:
                case IntelMnemonics.CVTSD2SI:
                case IntelMnemonics.CVTSD2SS:
                case IntelMnemonics.CVTSI2SD:

                case IntelMnemonics.DAA:
                case IntelMnemonics.DAS:
                case IntelMnemonics.DEC:
                case IntelMnemonics.DIV:
                case IntelMnemonics.DIVPD:
                case IntelMnemonics.DIVPS:
                case IntelMnemonics.DIVSD:
                case IntelMnemonics.DIVSS:
                case IntelMnemonics.DPPD:
                case IntelMnemonics.DPPS:
                case IntelMnemonics.EMMS:
                case IntelMnemonics.ENTER:
                case IntelMnemonics.EXTRACTPS:
                case IntelMnemonics.F2XM1:
                case IntelMnemonics.FABS:
                case IntelMnemonics.FADD:
                case IntelMnemonics.FADDP:
                case IntelMnemonics.FIADD:
                case IntelMnemonics.FBLD:
                case IntelMnemonics.FBSTP:
                case IntelMnemonics.FCHS:
                case IntelMnemonics.FCLEX:
                case IntelMnemonics.FNCLEX:
                case IntelMnemonics.FCMOVcc:
                case IntelMnemonics.FCOM:
                case IntelMnemonics.FCOMP:
                case IntelMnemonics.FCOMPP:
                case IntelMnemonics.FCOMI:
                case IntelMnemonics.FCOMIP:
                case IntelMnemonics.FUCOMI:
                case IntelMnemonics.FUCOMIP:
                case IntelMnemonics.FCOS:
                case IntelMnemonics.FDECSTP:
                case IntelMnemonics.FDIV:
                case IntelMnemonics.FDIVP:
                case IntelMnemonics.FIDIV:
                case IntelMnemonics.FDIVR:
                case IntelMnemonics.FDIVRP:
                case IntelMnemonics.FIDIVRP:
                case IntelMnemonics.FFREE:
                case IntelMnemonics.FICOM:
                case IntelMnemonics.FICOMP:
                case IntelMnemonics.FILD:
                case IntelMnemonics.FINCSTP:
                case IntelMnemonics.FINIT:
                case IntelMnemonics.FNINIT:
                case IntelMnemonics.FIST:
                case IntelMnemonics.FISTP:
                case IntelMnemonics.FISTTP:
                case IntelMnemonics.FLD:
                case IntelMnemonics.FLD1:

                case IntelMnemonics.HLT:
                case IntelMnemonics.HSUBPD:
                case IntelMnemonics.HSUBPS:
                case IntelMnemonics.IDIV:
                case IntelMnemonics.IMUL:
                case IntelMnemonics.IN:
                    throw new NotImplementedException();
                case IntelMnemonics.INC:
                    ExecINC(op);
                    break;
                case IntelMnemonics.INS:
                case IntelMnemonics.INSERTPS:
                case IntelMnemonics.INT:
                case IntelMnemonics.INTO:
                case IntelMnemonics.INT3:
                case IntelMnemonics.INVD:
                case IntelMnemonics.INVPG:
                case IntelMnemonics.INVPCID:
                case IntelMnemonics.IRET:
                    throw new NotImplementedException();

                case IntelMnemonics.JCC:
                case IntelMnemonics.JMP:
                    ExecJMP(op);
                    break;

                case IntelMnemonics.LAHF:
                case IntelMnemonics.LAR:
                case IntelMnemonics.LDDQU:
                case IntelMnemonics.LDMXCSR:
                case IntelMnemonics.LDS:
                case IntelMnemonics.LES:
                case IntelMnemonics.LFS:
                case IntelMnemonics.LGS:
                case IntelMnemonics.LSS:
                case IntelMnemonics.LEA:
                    throw new NotImplementedException();
                case IntelMnemonics.LEAVE:
                    ExecLEAVE(op);
                    break;
                case IntelMnemonics.LFENCE:
                case IntelMnemonics.LGDT:
                case IntelMnemonics.LIDT:
                case IntelMnemonics.LLDT:
                case IntelMnemonics.LMSW:
                case IntelMnemonics.LODS:
                case IntelMnemonics.LOOP:
                case IntelMnemonics.LOOPCC:
                case IntelMnemonics.LSL:
                case IntelMnemonics.LTR:
                case IntelMnemonics.LZCNT:
                case IntelMnemonics.MASKMOVDQU:
                case IntelMnemonics.MASKMOVQ:
                case IntelMnemonics.MAXPD:
                case IntelMnemonics.MAXPS:
                case IntelMnemonics.MAXSD:
                case IntelMnemonics.MAXSS:
                case IntelMnemonics.MFENCE:
                case IntelMnemonics.MINPD:
                case IntelMnemonics.MINPS:
                case IntelMnemonics.MINSD:
                case IntelMnemonics.MINSS:
                case IntelMnemonics.MONITOR:
                    throw new NotImplementedException();

                case IntelMnemonics.MOV:
                    ExecMOV(op);
                    break;

                case IntelMnemonics.MOVAPD:
                case IntelMnemonics.MOVAPS:
                case IntelMnemonics.MOVBE:
                case IntelMnemonics.MOVD:
                case IntelMnemonics.MOVSX:
                    ExecMOVSX(op);
                    break;
                case IntelMnemonics.MOVQ:
                    throw new NotImplementedException();

                case IntelMnemonics.MOVZX:
                    ExecMOVZX(op);
                    break;
                case IntelMnemonics.MOVDDUP:
                case IntelMnemonics.MOVDQA:
                case IntelMnemonics.MOVDQU:
                case IntelMnemonics.MOVDQ2Q:
                case IntelMnemonics.MOVHPS:
                case IntelMnemonics.MOVLHPS:
                case IntelMnemonics.MOVLPD:
                case IntelMnemonics.MOVLPS:
                case IntelMnemonics.MOVMSKPD:
                case IntelMnemonics.MOVMSKPS:
                case IntelMnemonics.MOVNTDQA:
                case IntelMnemonics.MOVNTDQ:
                case IntelMnemonics.MOVNTI:
                case IntelMnemonics.MOVNTPD:
                case IntelMnemonics.MOVNTPS:
                case IntelMnemonics.MOVNTPQ:

                case IntelMnemonics.MUL:
                case IntelMnemonics.MULPD:
                case IntelMnemonics.MULPS:
                case IntelMnemonics.MULSD:
                case IntelMnemonics.MULSS:
                case IntelMnemonics.MULX:
                case IntelMnemonics.MWAIT:
                case IntelMnemonics.NEG:
                case IntelMnemonics.NOP:
                case IntelMnemonics.NOT:
                case IntelMnemonics.ORPD:
                case IntelMnemonics.ORPS:
                    throw new NotImplementedException();
                case IntelMnemonics.OUT:
                    ExecOUT(op);
                    break;
                case IntelMnemonics.OUTS:
                case IntelMnemonics.PABSB:
                case IntelMnemonics.PACKSSWB:
                    throw new NotImplementedException();

                case IntelMnemonics.PAUSE:
                    break;
                case IntelMnemonics.POP:
                    ExecPOP(op);
                    break;
                case IntelMnemonics.POPA:
                case IntelMnemonics.POPAD:
                    ExecPOPA(op);
                    break;
                case IntelMnemonics.POPF:
                    ExecPOPF(op);
                    break;
                case IntelMnemonics.PUSH:
                    ExecPUSH(op);
                    break;
                case IntelMnemonics.PUSHA:
                case IntelMnemonics.PUSHAD:
                    ExecPUSHA(op);
                    break;
                case IntelMnemonics.PUSHF:
                case IntelMnemonics.PUSHFD:
                    ExecPUSHF(op);
                    break;
                case IntelMnemonics.RCL:
                case IntelMnemonics.RCR:
                case IntelMnemonics.ROL:
                case IntelMnemonics.ROR:
                    ExecRCL(op);
                    break;

                case IntelMnemonics.RDMSR:
                case IntelMnemonics.RDPKRU:
                case IntelMnemonics.RDPMC:
                case IntelMnemonics.RDRAND:
                case IntelMnemonics.RDSEED:
                case IntelMnemonics.RDTSC:
                case IntelMnemonics.RDTSCP:
                    throw new NotImplementedException();
                case IntelMnemonics.RET:
                    ExecRET(op);
                    break;

                case IntelMnemonics.SAL:
                case IntelMnemonics.SAR:
                case IntelMnemonics.SHL:
                case IntelMnemonics.SHR:
                    ExecSAL(op);
                    break;

                case IntelMnemonics.STAC:
                    ExecSTFLG(op, IntelFlags.AuxCarry);
                    break;
                case IntelMnemonics.STC:
                    ExecSTFLG(op, IntelFlags.Carry);
                    break;
                case IntelMnemonics.STD:
                    ExecSTFLG(op, IntelFlags.Direction);
                    break;
                case IntelMnemonics.STI:
                    break;

                case IntelMnemonics.STOS:
                    ExecSTOS(op);
                    break;
                case IntelMnemonics.XCHG:
                default:
                    throw new NotImplementedException();

            }
        }

        private bool Flg(IntelFlags flag)
            => ((IntelFlags)_rflags).HasFlag(flag);
        private void SetFlg(IntelFlags flag, bool enable)
        {
            if (enable)
                _rflags |= (long)flag;
            else
                _rflags &= ~(long)flag;
        }

        private bool Msb(long value, SmoSize size)
            => size switch
            {
                SmoSize.Byte => ((value >> 7) & 1) != 0,
                SmoSize.Word => ((value >> 15) & 1) != 0,
                SmoSize.Double => ((value >> 31) & 1) != 0,
                SmoSize.Quad => ((value >> 63) & 1) != 0,
            };

        private bool Lsb(long value, SmoSize size = SmoSize.Byte)
            => (value & 1) != 0;

        private void Push(long value, int cnt = 1)
        {

            if (false) // Check stack
                throw new Exception("#SS");
            _sp -= 4;
            Memory.Segment = _ss;
            Memory.Address = _sp;
            // Console.WriteLine($"PUSH {Memory.Address:x8} -> {value:x8}");
            Memory.WriteBytes(BitConverter.GetBytes((uint)value));
        }

        private long Pop(int cnt = 1)
        {
            if (false) // Check stack
                throw new Exception("#SS");

            Memory.Segment = _ss;
            Memory.Address = _sp;
            var value = (long)BitConverter.ToUInt32(Memory.ReadBytes(new byte[4]));
            // Console.WriteLine($"POP {Memory.Address:x8} -> {value:x8}");
            _sp += 4;
            return value;
        }

        private void WriteOperandValue(SmoOperand dst, long tmp)
        {
            Memory.Segment = ReadSegment(dst);
            switch (dst.Type)
            {
                case SmoOperandType.Register:
                    // TODO !!
                    WriteRegister(dst.Value, tmp, dst.Size);
                    return;
                case SmoOperandType.Address:
                    Memory.Address = dst.Value;
                    break;
                case SmoOperandType.Memory:
                    Memory.Address = ReadRegister(dst.Value, SmoSize.Double);
                    break;
                case SmoOperandType.MemoryOffset:
                    Memory.Address = ReadRegister(dst.Value, SmoSize.Double) + dst.Value2;
                    break;
                default:
                    throw new NotImplementedException();
            }


            if (dst.Size == SmoSize.Byte)
            {
                Memory.WriteBytes([(byte)tmp]);
                // Console.WriteLine($"WRITE 8 {Memory.Address:x8} -> {tmp:x2}");
            }
            else if (dst.Size == SmoSize.Word)
            {
                Memory.WriteBytes(BitConverter.GetBytes((ushort)tmp));
                // Console.WriteLine($"WRITE 16 {Memory.Address:x8} -> {tmp:x4}");
            }
            else if (dst.Size == SmoSize.Double)
            {
                Memory.WriteBytes(BitConverter.GetBytes((uint)tmp));
                // Console.WriteLine($"WRITE 32 {Memory.Address:x8} -> {tmp:x8}");
            }
            else if (dst.Size == SmoSize.Quad)
                Memory.WriteBytes(BitConverter.GetBytes((ulong)tmp));
            else
                throw new NotImplementedException();
        }

        private long ReadOperandValue(SmoOperand src)
        {
            Memory.Segment = ReadSegment(src);
            switch (src.Type)
            {
                case SmoOperandType.Register:
                    return ReadRegister(src.Value, src.Size);
                case SmoOperandType.Value:
                    return src.Value;
                case SmoOperandType.Address:
                    Memory.Address = src.Value;
                    break;
                case SmoOperandType.Memory:
                    Memory.Address = ReadRegister(src.Value, SmoSize.Double);
                    break;
                case SmoOperandType.MemoryOffset:
                    Memory.Address = ReadRegister(src.Value, SmoSize.Double) + src.Value2;
                    break;
                case SmoOperandType.Sib1:
                    break;
                case SmoOperandType.Sib2:
                    break;
                case SmoOperandType.Sib3:
                    break;
                default:
                    throw new NotImplementedException();
            }

            long value = 0;
            Memory.Continue();
            if (src.Size == SmoSize.Byte)
            {
                value = Memory.ReadBytes(new byte[1])[0];
                // Console.WriteLine($"READ 8 {Memory.Address:x8} -> {value:x2}");
            }
            else if (src.Size == SmoSize.Word)
            {
                value = BitConverter.ToUInt16(Memory.ReadBytes(new byte[2]));
                // Console.WriteLine($"READ 16 {Memory.Address:x8} -> {value:x4}");
            }
            else if (src.Size == SmoSize.Double)
            {
                value = BitConverter.ToUInt32(Memory.ReadBytes(new byte[4]));
                // Console.WriteLine($"READ 32 {Memory.Address:x8} -> {value:x8}");
            }
            else if (src.Size == SmoSize.Quad)
            {
                value = BitConverter.ToInt64(Memory.ReadBytes(new byte[8]));
                // Console.WriteLine($"READ 64 {Memory.Address:x8} -> {value:x16}");
            }
            else
                throw new NotImplementedException();
            return value;
        }

        private void WriteRegister(long reg, long value, SmoSize size)
        {
            var mask = size switch
            {
                SmoSize.Byte => 0xffl,
                SmoSize.Word => 0xffffl,
                SmoSize.Double => 0xffffffffl,
                _ => 0,
            };

            switch (reg)
            {
                case 0:
                    _ax = (_ax & ~mask) | (value & mask);
                    break;
                case 1:
                    _cx = (_cx & ~mask) | (value & mask);
                    break;
                case 2:
                    _dx = (_dx & ~mask) | (value & mask);
                    break;
                case 3:
                    _bx = (_bx & ~mask) | (value & mask);
                    break;
                case 4:
                    if (size == SmoSize.Byte)
                        _ax = (_ax & ~0xff00) | ((value << 8) & 0xff00);
                    else
                        _sp = (_sp & ~mask) | (value & mask);
                    break;
                case 5:
                    if (size == SmoSize.Byte)
                        _cx = (_cx & ~0xff00) | ((value << 8) & 0xff00);
                    else
                        _bp = (_bp & ~mask) | (value & mask);
                    break;
                case 6:
                    if (size == SmoSize.Byte)
                        _dx = (_dx & ~0xff00) | ((value << 8) & 0xff00);
                    else
                        _si = (_si & ~mask) | (value & mask);
                    break;
                case 7:
                    if (size == SmoSize.Byte)
                        _bx = (_bx & ~0xff00) | ((value << 8) & 0xff00);
                    else
                        _di = (_di & ~mask) | (value & mask);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private long ReadRegister(long reg, SmoSize size)
        {
            var mask = size switch
            {
                SmoSize.Byte => 0xffl,
                SmoSize.Word => 0xffffl,
                SmoSize.Double => 0xffffffffl,
                _ => 0,
            };

            var value = reg switch
            {
                0 => _ax,
                1 => _cx,
                2 => _dx,
                3 => _bx,
                4 => size == SmoSize.Byte ? _ax >> 8 : _sp,
                5 => size == SmoSize.Byte ? _cx >> 8 : _bp,
                6 => size == SmoSize.Byte ? _dx >> 8 : _si,
                7 => size == SmoSize.Byte ? _bx >> 8 : _di,
                _ => throw new NotImplementedException(),
            };
            return mask == 0 ? value : value & mask;
        }

        private ushort ReadSegment(SmoOperand ope)
        {
            if (string.IsNullOrEmpty(ope.Prefix) || ope.Prefix == "ds")
                return _ds;
            if (ope.Prefix == "es")
                return _es;
            if (ope.Prefix == "fs")
                return _fs;
            if (ope.Prefix == "gs")
                return _gs;
            if (ope.Prefix == "ss")
                return _ss;
            throw new NotImplementedException();
        }

    }
}


