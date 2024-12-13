using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public enum TypeAttributes
    {
        Class = 1,
        Struct = 2,
        Interafce = 4,
        Enum = 8,

        Abstract, 
        Static, 
        Sealed,
        SequentialLayout,
        ExplicitLayout, 
        Import, 
        Serializable,
    }
}
