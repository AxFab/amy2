using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Amy.Core.Reflection
{
    public class TypeInfo : MemberInfo
    {
        public TypeInfo(TypeInfo declaringType, string namespc, string name, MemberAttributes attributes, TypeAttributes typeAttributes)
            : base(declaringType, name, MemberType.TypeInfo, attributes)
        {
            Namespace = namespc;
            TypeAttributes = typeAttributes;
        }

        public string Namespace { get; }
        public TypeAttributes TypeAttributes { get; }
        public List<ConstructorInfo> DeclaredConstructors { get; } = new List<ConstructorInfo>();
        public List<PropertyInfo> DeclaredProperties { get; } = new List<PropertyInfo>(); 
        public List<TypeInfo> DeclaredNestedTypes { get; } = new List<TypeInfo>();
        public List<MethodInfo> DeclaredMethods { get; } = new List<MethodInfo>();
        public List<FieldInfo> DeclaredFields { get; } = new List<FieldInfo>();
        public List<EventInfo> DeclaredEvents { get; } = new List<EventInfo>();
        public DestructorInfo DeclaredDestructor { get; internal set; }
        public GenericParameterInfo[] GenericParameters { get; internal set; }
        public TypeInfo[] ImplementedInterfaces { get; internal set; }
        public TypeInfo BaseType { get; internal set; }
    }
}
