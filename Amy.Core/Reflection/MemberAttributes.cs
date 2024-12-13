using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public enum MemberAttributes
    {
        Public = 1, Protected = 2, Private = 4, Internal = 8, VisibilityMask = 15,
        Static = 16,
        Sealed = 32,
    }
}
