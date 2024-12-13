using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class ParameterInfo
    {
        public ParameterInfo(string name, TypeReference type, ParameterAttributes attributes)
        {
            Name = name;
            Type = type;
            Attributes = attributes;
        }
        public ParameterAttributes Attributes { get; }
        public List<CustomAttributeInfo> CustomAttributes { get; } = new List<CustomAttributeInfo>();
        public string Name { get; }
        public TypeReference Type { get; }
        public object DefaultValue { get; internal set; }
    }
}
