using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class CustomAttributeInfo
    {
        public TypeReference AttributeType { get; }
        public List<ParameterInfo> Parameters { get; }
    }
}
