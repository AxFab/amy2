using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class ConstructorInfo : MemberInfo
    {
        internal ConstructorInfo(TypeInfo declaringType, string name, MemberAttributes attributes)
            : base(declaringType, name, MemberType.Constructor, attributes)
        {
        }
        public List<ParameterInfo> Parameters { get; } = new List<ParameterInfo>();
    }
}
