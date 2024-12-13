using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public abstract class MemberInfo
    {
        protected MemberInfo(TypeInfo declaringType, string name, MemberType type, MemberAttributes attributes)
        {
            DeclaringType = declaringType;
            Name = name;
            Attributes = attributes;
            Type = type;
        }

        public MemberAttributes Attributes { get; }
        public List<CustomAttributeInfo> CustomAttributes { get; } = new List<CustomAttributeInfo>();
        public TypeInfo DeclaringType { get; }
        public string Name { get; }
        public MemberType Type { get; }
    }
}
