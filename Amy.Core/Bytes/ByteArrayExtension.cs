using System.Text;

namespace Amy.Core.Bytes
{
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


        public static byte Read8(this IByteReader reader) => reader.NextByte();
        public static ushort Read16(this IByteReader reader)
        {
            var a = reader.NextByte();
            var b = reader.NextByte();
            return (ushort)(a | (b << 8));
        }
        public static uint Read32(this IByteReader reader)
        {
            var a = reader.NextByte();
            var b = reader.NextByte();
            var c = reader.NextByte();
            var d = reader.NextByte();
            return (uint)(a | (b << 8) | (c << 16) | (d << 24));
        }
        public static ulong Read64(this IByteReader reader)
        {
            var lo = reader.Read32();
            var hi = reader.Read32();
            return lo | (ulong)hi << 32;
        }

    }
}
