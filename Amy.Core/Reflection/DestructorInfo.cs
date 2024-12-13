using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class DestructorInfo : MemberInfo
    {
        internal DestructorInfo(TypeInfo declaringType, string name, MemberAttributes attributes)
            : base(declaringType, name, MemberType.Destructor, attributes)
        {
        }
    }
}
