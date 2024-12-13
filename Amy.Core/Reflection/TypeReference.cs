using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class TypeReference
    {
        public string Name { get; internal set; }
        public string Fullname { get; internal set; }
        public string Namespace { get; internal set; }
        public List<string> Various { get; internal set; }
    }
}
