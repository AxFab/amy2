using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Text
{
    public enum TokenType
    {
        Undefined = 0,
        Operator,
        Identifier,
        Number,
        String,
        Comment,
        Character,
        Preprocessor,
        Joined
    }
}
