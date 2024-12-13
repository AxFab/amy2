using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class FieldInfo : MemberInfo
    {
        internal FieldInfo(TypeInfo declaringType, TypeReference fieldType, string name, MemberAttributes attributes) 
            : base(declaringType, name, MemberType.Field, attributes) 
        {
            FieldType = fieldType;
        }


        public TypeReference FieldType { get; }
        public object DefaultValue { get; internal set; }
    }


}
