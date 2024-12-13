using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class PropertyInfo : MemberInfo
    {

        internal PropertyInfo(TypeInfo declaringType, TypeReference propertyType, string name, MemberAttributes attributes)
            : base(declaringType, name, MemberType.Property, attributes)
        {
            PropertyType = propertyType;
        }


        public TypeReference PropertyType { get; }
        public MethodInfo Getter { get; internal set; }
        public MethodInfo Setter { get; internal set; }
        public object DefaultValue { get; internal set; }
        public bool CanRead => Getter != null;
        public bool CanWrite => Setter != null;
    }
}
