using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Bytes
{
    public interface IByteReader
    {
        byte NextByte();
        bool Continue();
        int ReadLength();
        byte[] ReadLast();
    }
}
