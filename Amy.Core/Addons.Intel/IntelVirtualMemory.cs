using Amy.Core.Bytes;

namespace Amy.Core.Addons.Intel
{
    public class IntelVirtualMemory : IByteReader
    {
        private Dictionary<long, byte[]> _memory = new Dictionary<long, byte[]>();

        public long Segment { get; set; }
        public long Address { get; set; }
        public long PageSize { get; private set; } = 4096;

        private int _reading;
        public bool Continue()
        {
            _reading = 0;
            return true;
        }

        public byte NextByte()
        {
            var pageNo = (Address + _reading) / PageSize;
            var pageOff = (Address + _reading) % PageSize;
            var page = FetchPage(pageNo);
            _reading++;
            return page[pageOff];
        }

        public byte[] ReadLast()
        {
            var buf = new byte[_reading];
            ReadBytes(buf);
            return buf;
        }

        public int ReadLength() => _reading;

        public byte[] ReadBytes(byte[] value)
        {
            long loadNo = -1;
            byte[] page = null;
            for (int i = 0; i < value.Length; ++i)
            {
                var pageNo = (Address + i) / PageSize;
                var pageOff = (Address + i) % PageSize;
                if (loadNo != pageNo)
                {
                    loadNo = pageNo;
                    page = FetchPage(pageNo);
                }
                value[i] = page[pageOff];
            }
            return value;
        }

        public byte[] WriteBytes(byte[] value)
        {
            long loadNo = -1;
            byte[] page = null;
            for (int i = 0; i < value.Length; ++i)
            {
                var pageNo = (Address + i) / PageSize;
                var pageOff = (Address + i) % PageSize;
                if (loadNo != pageNo)
                {
                    loadNo = pageNo;
                    page = FetchPage(pageNo);
                }
                page[pageOff] = value[i];
            }
            return value;
        }

        public byte[] FetchPage(long pageNo)
        {
            lock (_memory)
            {
                if (!_memory.TryGetValue(pageNo, out var page))
                {
                    page = new byte[PageSize];
                    _memory[pageNo] = page;
                }
                return page;
            }
        }
    }

}
