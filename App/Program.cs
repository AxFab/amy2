using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization.Metadata;

namespace App
{
    class FileByteReader : IByteReader
    {
        private Stream _stream;
        private int _readLength = 0;

        public FileByteReader(Stream stream)
        {
            _stream = stream;
        }
        public FileByteReader(string path)
        {
            _stream = File.OpenRead(path);
        }


        public bool Continue()
        {
            _readLength = 0;
            return _stream.Position < _stream.Length;
        }

        public byte NextByte()
        {
            _readLength++;
            return (byte)_stream.ReadByte();
        }

        public int ReadLength() => _readLength;

        public byte[] ReadLast()
        {
            _stream.Seek(-_readLength, SeekOrigin.Current);
            var buf = new byte[_readLength];
            _stream.Read(buf, 0, buf.Length);
            return buf;
        }

        public void SetOffset(long offset)
        {
            _stream.Seek(offset, SeekOrigin.Begin);
        }
    }

    class ByteArrayReader : IByteReader
    {
        private byte[] _buffer;
        private int _offset;
        private int _index;

        public ByteArrayReader(byte[] buffer, int start = 0)
        {
            _buffer = buffer;
            _offset = start;
            _index = 0;
        }

        public bool Continue()
        {
            _offset += _index;
            _index = 0;
            return _buffer.Length > _offset;
        }

        public byte NextByte()
        {
            if (_offset + _index < _buffer.Length)
                return _buffer[_offset + _index++];
            return 0;
        }

        public int ReadLength() =>_index;

        public byte[] ReadLast()
        {
            return _buffer.AsSpan(_offset, _index).ToArray();
        }
    }

    public static class ByteArrayExtension
    {
        public static string ToHexArray(this byte[] arr, int mx = 0)
        {
            StringBuilder hex = new StringBuilder(arr.Length * 3);
            int n = 0;
            foreach (byte b in arr)
            {
                hex.AppendFormat("{0:x2} ", b);
                if (++n == mx)
                {
                    hex.Append("\r\n");
                    n = 0;
                }
            }
            return hex.ToString().TrimEnd();
        }
    }

    public partial class MyVM : AbstractInMemoryVm
    {
        const int RegEax = 0;
        const int RegEbx = 1;
        const int RegEcx = 2;
        const int RegEdx = 3;
        const int RegBp = 4;
        const int RegSp = 5;
        const int RegIp = 6;
        const int RegFl = 7;
        const int FlagC = 1; // Carry
        const int FlagZ = 2; // Zero
        const int FlagS = 4; // Signed
        const int FlagO = 8; // Overflow
        const int FlagP = 16; // Parity
        const int FlagI = 32; // Interruptible
        const int FlagF = 64; // Fault
        const int FlagH = 128; // Halted

        public MyVM()
            : base(0x10000, 4, 8)
        {
        }

        public uint Eax
        {
            get => this.ReadRegister32(RegEax);
            set => this.WriteRegister32(RegEax, value);
        }
        public uint Ebx
        {
            get => this.ReadRegister32(RegEbx);
            set => this.WriteRegister32(RegEbx, value);
        }

        public uint Ecx
        {
            get => this.ReadRegister32(RegEcx);
            set => this.WriteRegister32(RegEcx, value);
        }
        public uint Edx
        {
            get => this.ReadRegister32(RegEdx);
            set => this.WriteRegister32(RegEdx, value);
        }
        public ushort Bp
        {
            get => this.ReadRegister16(RegBp);
            set => this.WriteRegister16(RegBp, value);
        }
        public ushort Sp
        {
            get => this.ReadRegister16(RegSp);
            set => this.WriteRegister16(RegSp, value);
        }

        public ushort Ip
        {
            get => this.ReadRegister16(RegIp);
            set => this.WriteRegister16(RegIp, value);
        }

        public bool Fc
        {
            get => GetFlags(RegFl, FlagC);
            set => SetFlags(RegFl, FlagC, value);
        }
        public bool Fz
        {
            get => GetFlags(RegFl, FlagZ);
            set => SetFlags(RegFl, FlagZ, value);
        }
        public bool Fs
        {
            get => GetFlags(RegFl, FlagS);
            set => SetFlags(RegFl, FlagS, value);
        }
        public bool Fo
        {
            get => GetFlags(RegFl, FlagO);
            set => SetFlags(RegFl, FlagO, value);
        }
        public bool Fp
        {
            get => GetFlags(RegFl, FlagP);
            set => SetFlags(RegFl, FlagP, value);
        }

        private bool GetFlags(uint rego, ushort selector)
        {
            var flags = this.ReadRegister16(rego);
            return (flags & selector) != 0;
        }

        private void SetFlags(uint rego, ushort selector, bool value)
        {
            var flags = this.ReadRegister16(rego);
            if (value)
                flags |= selector;
            else
                flags &= (byte)~selector;
            this.WriteRegister16(rego, flags);
        }
        public override bool Halted => !GetFlags(RegFl, FlagH);

        public override ulong InstructionPointer => Ip;

        protected override IDisassembler Disassembler => throw new NotImplementedException();

        public override void Execute(AsmOpcode opCode, int opLength)
        {
            throw new NotImplementedException();
        }
    }

    public partial class MyVM { 
        public void Step()
        {
            var ip = Ip;
            var opcode = GetMemory(ip, 8);
        }

        static uint B(bool b, int d = 0) => (b ? 1U : 0) << d;
        static bool Q(ulong q, int d = 0) => (q & (1U << d)) != 0;
        
        public static void Alu(uint ra, uint rb, uint sz, bool fc, out uint ors, out bool oc)
        {
            uint rp = ra ^ rb;
            uint rq = ra & rb;

            ulong rs = 0;
            ulong rn = 0;
            ulong ro = 0;
            bool carry = fc;
            for (int i = 0; i < 32; ++i)
            {
                rs |= B(carry, i) ^ B(Q(rp, i), i);
                rn |= B(carry, i) & B(Q(rp, i), i);
                ro = rs | rn;
                carry = Q(ro, i); 
            }
            ors = (uint)rs;
            oc = Q(rs, 32);
        }
    }

    class Program
    {
        static void TestDisasmx86()
        {
            var path = "C:/Users/Aesga/develop/kora/_i386-pc-kora/kernel/bin/kora-i386.krn";
            var rd = new FileByteReader(path);
            rd.SetOffset(0x1000);
            var dsm = new Dissasemble_x86(rd);
            long offset = 0x20000;

            for (; rd.Continue(); )
            {
                var op = dsm.Disassemble();
                var buf = rd.ReadLast();
                var mnemonic = AsmWriter_x86.TranslateOpcode(op);
                var code = buf.ToHexArray();
                Console.WriteLine($"{offset,8:x}\t{code,-15}  \t{mnemonic}");
                offset += buf.Length;
            }
        }
        static void TestCSharpParser()
        {
            var path = "C:/Users/Aesga/develop/Schema/Schema/Data.Amf/AmfReader.cs";

            var parser = new CSharpParser();
            // parser.Parse(path);
            parser.ParseExpr("mark = (AmfMarker)_stream.ReadByte();");
        }

        static void TestAsmVMA8()
        {
            var text = "; Simple example\r\n; Writes Hello World to the output\r\n\r\n\tJMP start\r\nhello: DB \"Hello World!\" ; Variable\r\n       DB 0\t; String terminator\r\n\r\nstart:\r\n\tMOV C, hello    ; Point to var \r\n\tMOV D, 232\t; Point to output\r\n\tCALL print\r\n        HLT             ; Stop execution\r\n\r\nprint:\t\t\t; print(C:*from, D:*to)\r\n\tPUSH A\r\n\tPUSH B\r\n\tMOV B, 0\r\n.loop:\r\n\tMOV A, [C]\t; Get char from var\r\n\tMOV [D], A\t; Write to output\r\n\tINC C\r\n\tINC D  \r\n\tCMP B, [C]\t; Check if end\r\n\tJNZ .loop\t; jump if not\r\n\r\n\tPOP B\r\n\tPOP A\r\n\tRET";
            var lines = text.Replace("\r", "").Split('\n');
            var asm = new AsmReader_A8();
            asm.ReadLines(lines);
            asm.Relloc();
            var prog = new byte[]
            {
                0x1f, 0x0f, 0x48, 0x65, 0x6c, 0x6c, 0x6f, 0x20,
                0x57, 0x6f, 0x72, 0x6c, 0x64, 0x21, 0x00, 0x06,
                0x02, 0x02, 0x06, 0x03, 0xe8, 0x38, 0x18, 0x00,
                0x32, 0x00, 0x32, 0x01, 0x06, 0x01, 0x00, 0x03,
                0x00, 0x02, 0x05, 0x03, 0x00, 0x12, 0x02, 0x12,
                0x03, 0x15, 0x01, 0x02, 0x27, 0x1F, 0x36, 0x01,
                0x36, 0x00, 0x39,
            };
            var buf = asm.ToBuffer();
            var hex = buf.ToHexArray(8);
            Console.WriteLine(hex);
            var vm = new VirtualMachine_A8();
            vm.Reset(buf);
            vm.Execute();
        }

        static void Main(string[] args)
        {
            TestAsmVMA8();
            // TestDisasmx86();
            // TestCSharpParser();
            // TestVirtualMachine();
            // TestAsmBuilder();




            //var lines = System.IO.File.ReadAllLines("C:/Users/Aesga/develop/kora/src/kernel/pages.txt").ToList();
            //while (lines.Count > 0)
            //{
            //    var line = lines.Last();
            //    lines.RemoveAt(lines.Count - 1);
            //    if (!line.StartsWith("<=== "))
            //        break;
            //    var search = "===> " + line.Substring(5);
            //    int idx = lines.LastIndexOf(search);
            //    if (idx < 0)
            //        break;
            //    lines.RemoveAt(idx);
            //}
        }
    }
}