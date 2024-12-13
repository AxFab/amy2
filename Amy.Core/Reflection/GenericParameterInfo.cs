using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class GenericParameterInfo
    {
        public string Name { get; }
        public TypeReference Type { get; }
        public TypeReference[] RestrictionTypes { get; internal set; }
    }
}
