using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class MethodInfo : MemberInfo
    {
        internal MethodInfo(TypeInfo declaringType, string name, TypeReference returnType, MemberAttributes attributes)
            : base(declaringType, name, MemberType.Method, attributes) 
        {
            ReturnType = returnType;
        }
        public TypeReference ReturnType { get; }
        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
        public GenericParameterInfo[] GenericParameters { get; internal set; }
    }
}
