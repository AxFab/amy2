using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public enum MemberType
    {
        Field, Property, Method, Constructor, Destructor,
    }
    public class MemberInfo
    {
        public MemberInfo(MemberType memberType, TypeInfo declaringType, string name, TypeQualifier qualifiers)
        {
            MemberType = memberType;
            DeclaringType = declaringType;
            Name = name;
            Qualifiers = qualifiers;
        }

        public MemberType MemberType { get; }
        public TypeInfo DeclaringType { get; }
        public string Name { get; }
        public TypeQualifier Qualifiers { get; }
    }
    public class FieldInfo : MemberInfo
    {
        public FieldInfo(TypeInfo declaringType, string name, TypeInfo fieldType, TypeQualifier qualifiers)
            : base(MemberType.Field, declaringType, name, qualifiers)
        {
            FieldType = fieldType;
        }
        public TypeInfo FieldType { get; }
    }

    // public class PropertyInfo : MemberInfo { }
    public class MethodInfo : MemberInfo {
        public MethodInfo(TypeInfo declaringType, string name, TypeInfo methodType, TypeQualifier qualifiers, List<ParameterInfo> parameters)
        : base(MemberType.Method, declaringType, name, qualifiers)
        {
            Parameters = parameters;
            MethodType = methodType;
        }

        public TypeInfo MethodType { get; }
        public List<ParameterInfo> Parameters { get; }
    }
    // public class EventInfo : MemberInfo { }
    public class ConstructorInfo : MemberInfo
    {
        public ConstructorInfo(TypeInfo declaringType, TypeQualifier qualifiers, List<ParameterInfo> parameters)
            : base(MemberType.Constructor, declaringType, declaringType.Name, qualifiers)
        {
            Parameters = parameters;
        }

        public List<ParameterInfo> Parameters { get; }
    }
    // public class DestructorInfo : MemberInfo { }
}
