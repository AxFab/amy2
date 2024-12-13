using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Bytes
{
    public class FileByteReader : IByteReader
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

}
