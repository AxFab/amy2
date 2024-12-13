using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class EventInfo : MemberInfo
    {
        internal EventInfo(TypeInfo declaringType, TypeReference eventHandlerType, string name, MemberAttributes attributes)
            : base(declaringType, name, MemberType.Event, attributes)
        {
            EventHandlerType = eventHandlerType;
        }


        public TypeReference EventHandlerType { get; }
        public MethodInfo RaiseMethod { get; internal set; }
        public MethodInfo AddMethod { get; internal set; }
        public MethodInfo RemoveMethod { get; internal set; }
    }
}
