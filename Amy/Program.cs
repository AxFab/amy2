using Amy.Core;
using Amy.Core.Addons.Intel;
using Amy.Core.Bytes;
using System;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace Amy
{
    internal class Program
    {
        static void DissasmbleurSection(ElfFile elf, FileByteReader file, IDisassembler dis, IAsmWriter wrt, string name)
        {
            var tx = elf.ReadSection(elf.SearchSection(name));
            file.SetOffset(tx.Offset);
            elf.Output.WriteLine($"\nDisassembly of section {name}:");

            var offset = tx.Addr;
            int ln = 0;
            for (; file.Continue();)
            {
                var op = dis.NextInstruction(file);
                var buf = file.ReadLast();
                (wrt as IntelWriter).Display(elf.Output, op, elf, offset, buf);

                offset += (uint)buf.Length;
                if (offset >= tx.Addr + tx.Size)
                    break;
            }
        }

        static void Dissasmbleur(string uri, IDisassembler dis, IAsmWriter wrt, string outuri = null)
        {
            var stream = File.OpenRead(uri);
            var elf = new ElfFile(stream);
            elf.Filename = "img/boot/kora-i386.krn";
            if (!string.IsNullOrEmpty(outuri))
            {
                elf.Output = new StreamWriter(File.Open(outuri, FileMode.Create, FileAccess.Write), System.Text.Encoding.ASCII);
                (elf.Output as StreamWriter).NewLine = "\n";
            }
            elf.OpenHeader(false);


            var file = new FileByteReader(uri);
            try
            {
                DissasmbleurSection(elf, file, dis, wrt, ".text");
                DissasmbleurSection(elf, file, dis, wrt, ".text.__x86.get_pc_thunk.ax");
                DissasmbleurSection(elf, file, dis, wrt, ".text.__x86.get_pc_thunk.bx");
                DissasmbleurSection(elf, file, dis, wrt, ".text.__x86.get_pc_thunk.dx");
                DissasmbleurSection(elf, file, dis, wrt, ".text.__x86.get_pc_thunk.si");
                DissasmbleurSection(elf, file, dis, wrt, ".text.__x86.get_pc_thunk.cx");
                DissasmbleurSection(elf, file, dis, wrt, ".text.__x86.get_pc_thunk.di");
            }
            catch (Exception ex)
            {
                elf.Output.Flush();
            }
            finally
            {
                if (!string.IsNullOrEmpty(outuri))
                    elf.Output.Close();
            }
        }

        static void Main(string[] args)
        {
            var uri = "C:\\Users\\Aesga\\develop\\kora\\_i386-pc-kora\\img\\boot\\kora-i386.krn";


            // Dissasmbleur(uri, new IntelDisassembler(), new IntelWriter(), Path.ChangeExtension(uri, ".txt"));

            var vm = IntelVirtualMachine.LoadKernel(uri);
            for (; ; )
                vm.Execute();
        }

    }
}