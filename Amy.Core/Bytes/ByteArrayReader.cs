using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Bytes
{
    public class ByteArrayReader : IByteReader
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

        public int ReadLength() => _index;

        public byte[] ReadLast()
        {
            return _buffer.AsSpan(_offset, _index).ToArray();
        }
    }

}
