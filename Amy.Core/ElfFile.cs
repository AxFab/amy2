using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core
{
    public class ElfFile
    {
        struct Elf32Header
        {
            public ushort Type;
            public ushort Machine;
            public uint Version;
            public uint Entry;
            public uint PhOff;
            public uint ShOff;
            public uint Flags;
            public ushort EhSize;
            public ushort PhSize;
            public ushort PhCount;
            public ushort ShSize;
            public ushort ShCount;
            public ushort ShStrNdx;
        }

        public struct Elf32PHead
        {
            public uint Type;
            public uint FileAddr;
            public uint VirtAddr;
            public uint PhysAddr;
            public uint FileSize;
            public uint VirtSize;
            public uint Flags;
            public uint Align;
        }

        public struct Elf32SHead
        {
            public uint NameIdx;
            public uint Type;
            public uint Flags;
            public uint Addr;
            public uint Offset;
            public uint Size;
            public uint Link;
            public uint Info;
            public uint Align;
            public uint ESize;
        }

        struct Elf32Symbol
        {

            public uint Name;
            public uint Value;
            public uint Size;
            public byte Info;
            public byte Other;
            public ushort Shndx;
        }

        public class ElfSymbol
        {
            public uint Offset;
            public uint Size;
            public string Name;
            public int Type;
            public string Section;

            public override string ToString() => $"{Name} <{Type}> ({Offset})";
        }

        private Stream _stream;
        private BinaryReader _reader;
        Elf32Header _header;
        Elf32SHead _shStringTable;
        Elf32SHead _stringTable;
        Elf32SHead _symbolTable;

        public TextWriter Output { get; set; } = Console.Out;
        public string Filename { get; set; }

        public int PhCount => _header.PhCount;
        public int ShCount => _header.ShCount;
        public ulong Entry => _header.Entry;

        List<ElfSymbol> _symbols = new List<ElfSymbol>();
        public ElfFile(Stream stream)
        {
            _stream = stream;
            _reader = new BinaryReader(stream);
        }

        public void OpenHeader(bool write = true)
        {
            var magic = new byte[16];
            _reader.Read(magic);
            if (magic[0] != 0x7f || magic[1] != 'E' || magic[2] != 'L' || magic[3] != 'F')
                return;

            _header.Type = _reader.ReadUInt16();
            _header.Machine = _reader.ReadUInt16();
            _header.Version = _reader.ReadUInt32();
            _header.Entry = _reader.ReadUInt32();
            _header.PhOff = _reader.ReadUInt32();
            _header.ShOff = _reader.ReadUInt32();
            _header.Flags = _reader.ReadUInt32();
            _header.EhSize = _reader.ReadUInt16();
            _header.PhSize = _reader.ReadUInt16();
            _header.PhCount = _reader.ReadUInt16();
            _header.ShSize = _reader.ReadUInt16();
            _header.ShCount = _reader.ReadUInt16();
            _header.ShStrNdx = _reader.ReadUInt16();

            var format = "";
            if (_header.Machine == 3)
                format = "elf32-i386";
            else if (_header.Machine == 62)
                format = "elf64-x86_64";
            var arch = format.Substring(6);
            Output.WriteLine($"\n{Filename + ":":-28}     file format {format}");

            if (write)
            {
                Output.WriteLine(Filename);
                Output.WriteLine($"architecture: {arch}, flags 0x{274:x8}:");
                Output.WriteLine("EXEC_P, HAS_SYMS, D_PAGED");
                Output.WriteLine($"start address 0x{_header.Entry:x8}");

            }

            ReadPhHeaders(write);
            _shStringTable = ReadSection(_header.ShStrNdx);
            ReadShHeaders(write);
            ReadSymbolTable(write);

            if (write)
                Output.WriteLine();
            Output.WriteLine();            
        }

        public void ReadPhHeaders(bool write)
        { 
            if (write)
            Output.WriteLine($"\nProgram Header:");
            for (int i = 0, n = _header.PhCount; i < n; i++)
            {
                Elf32PHead head = ReadSector(i);
                var rights = new string[] { "---", "--x", "-w-", "-wx", "r--", "r-x", "rw-", "rwx" };
                var type = head.Type < 7 ? new string[] { null, "LOAD", "DYNAMIC", "INTERP", "NOTE", "SHLIB", "PHDR" }[head.Type] : "";
                if (write)
                {
                    Output.WriteLine($"{type,8} off    0x{head.FileAddr:x8} vaddr 0x{head.VirtAddr:x8} paddr 0x{head.PhysAddr:x8} align 2**{Math.Log2(head.Align)}");
                    Output.WriteLine($"{"  ",8} filesz 0x{head.FileSize:x8} memsz 0x{head.VirtSize:x8} flags {rights[head.Flags & 7]}");
                }
            }
        }

        public void ReadShHeaders(bool write)
        {
            if (write)
            {
                Output.WriteLine($"\nSections:");
                Output.WriteLine($"Idx Name          Size      VMA       LMA       File off  Algn");
            }

            for (int i = 1, n = _header.ShCount; i < n; i++)
            {
                Elf32SHead head = ReadSection(i);

                var name = ReadShString((int)head.NameIdx);
                var opts = "";
                if (name.StartsWith(".text"))
                    opts = "CONTENTS, ALLOC, LOAD, READONLY, CODE";
                else if (name.StartsWith(".data") || name.StartsWith(".got"))
                    opts = "CONTENTS, ALLOC, LOAD, DATA";
                else if (name.StartsWith(".bss"))
                    opts = "ALLOC";
                else if (name.StartsWith(".debug"))
                    opts = "CONTENTS, READONLY, DEBUGGING, OCTETS";

                if (name == ".symtab")
                    _symbolTable = head;
                else if (name == ".strtab")
                    _stringTable = head;
                else if (name == ".shstrtab")
                    _shStringTable = head;

                else if (write)
                {
                    Output.WriteLine($"{i - 1,3} {name,-13} {head.Size:x8}  {head.Addr:x8}  {head.Addr:x8}  {head.Offset:x8}  2**{Math.Log2(head.Align)}");
                    Output.WriteLine($"{"",17} {opts}");
                }
            }

        }

        public void ReadSymbolTable(bool write)
        {
            if (write)
                Output.WriteLine($"SYMBOL TABLE:");
            for (int i = 1; ; ++i)
            {
                _reader.BaseStream.Seek(_symbolTable.Offset + i * 16, SeekOrigin.Begin);
                Elf32Symbol symbol;
                symbol.Name = _reader.ReadUInt32();
                symbol.Value = _reader.ReadUInt32();
                symbol.Size = _reader.ReadUInt32();
                symbol.Info = _reader.ReadByte();
                symbol.Other = _reader.ReadByte();
                symbol.Shndx = _reader.ReadUInt16();

                var name = ReadString((int)symbol.Name);
                var sname = name;
                if (symbol.Shndx < _header.ShCount)
                {
                    var sec = ReadSection((int)symbol.Shndx);
                    sname = ReadShString((int)sec.NameIdx);
                    if (symbol.Name == 0)
                        name = sname;
                }
                var opt = "";
                if (symbol.Other == 2 && symbol.Info == 18)
                    opt = ".hidden ";

                var type = symbol.Info switch
                {
                    0 => "l      ",
                    1 => "l     O",
                    2 => "l     F",
                    3 => "l    d ",
                    4 => "l    df",
                    16 => "g      ",
                    17 => "g     O",
                    18 => "g     F",
                    _ => "-     -",
                };

                if (symbol.Info == 4)
                    sname = "*ABS*";

                _symbols.Add(new ElfSymbol
                {
                    Name = name,
                    Offset = symbol.Value,
                    Size = symbol.Size,
                    Type = symbol.Info,
                    Section = sname,
                });

                if (write)
                    Output.WriteLine($"{symbol.Value:x8} {type} {sname:-5}\t{symbol.Size:x8} {opt}{name}");

                if (symbol.Value == 0x0004d64c)
                    break;
            }

        }

        public int SearchSection(string name)
        {
            for (int i = 1, n = _header.ShCount; i < n; i++)
            {
                Elf32SHead head = ReadSection(i);
                if (name == ReadShString((int)head.NameIdx))
                    return i;
            }
            return 0;
        }

        public Elf32PHead ReadSector(int index)
        {
            if (index >= _header.PhCount)
                throw new Exception();
            _reader.BaseStream.Seek(_header.PhOff + index * _header.PhSize, SeekOrigin.Begin);
            Elf32PHead head;
            head.Type = _reader.ReadUInt32();
            head.FileAddr = _reader.ReadUInt32();
            head.VirtAddr = _reader.ReadUInt32();
            head.PhysAddr = _reader.ReadUInt32();
            head.FileSize = _reader.ReadUInt32();
            head.VirtSize = _reader.ReadUInt32();
            head.Flags = _reader.ReadUInt32();
            head.Align = _reader.ReadUInt32();
            return head;
        }
        public Elf32SHead ReadSection(int index)
        {
            if (index >= _header.ShCount)
                throw new Exception();
            _reader.BaseStream.Seek(_header.ShOff + index * _header.ShSize, SeekOrigin.Begin);
            Elf32SHead head;
            head.NameIdx = _reader.ReadUInt32();
            head.Type = _reader.ReadUInt32();
            head.Flags = _reader.ReadUInt32();
            head.Addr = _reader.ReadUInt32();
            head.Offset = _reader.ReadUInt32();
            head.Size = _reader.ReadUInt32();
            head.Link = _reader.ReadUInt32();
            head.Info = _reader.ReadUInt32();
            head.Align = _reader.ReadUInt32();
            head.ESize = _reader.ReadUInt32();
            return head;
        }

        string ReadShString(int offset)
        {
            if (offset > _shStringTable.Size)
                throw new Exception();
            _reader.BaseStream.Seek(_shStringTable.Offset + offset, SeekOrigin.Begin);
            var str = string.Empty;
            for (; ; )
            {
                var ch = _reader.ReadChar();
                if (ch == '\0')
                    return str;
                str += ch;
            }
        }
        string ReadString(int offset)
        {
            if (offset > _stringTable.Size)
                throw new Exception();
            _reader.BaseStream.Seek(_stringTable.Offset + offset, SeekOrigin.Begin);
            var str = string.Empty;
            for (; ; )
            {
                var ch = _reader.ReadChar();
                if (ch == '\0')
                    return str;
                str += ch;
            }
        }

        public ElfSymbol SearchSymbolAt(long offset)
        {
            var list = _symbols.Where(x => x.Offset == offset).ToList();
            if (list.Count == 0)
                return null;
            return list.FirstOrDefault(x => x.Type != 3);
        }

        public ElfSymbol SearchSymbolBefore(long offset)
        {
            var list = _symbols.Where(x => x.Offset <= offset && x.Type != 3).OrderByDescending(x => x.Offset).Take(10).ToList();
            return list.FirstOrDefault();
        }

        internal void LoadPhContent(Elf32PHead head, long offset, byte[] buf)
        {
            var maxLength = Math.Min(head.FileSize - offset, buf.Length);
            Array.Clear(buf, 0, buf.Length);
            if (maxLength < 0)
                return;
            _reader.BaseStream.Seek(head.FileAddr + offset, SeekOrigin.Begin);
            _reader.Read(buf, 0, (int)maxLength);
        }
    }

}
