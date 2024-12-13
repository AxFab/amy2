using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public class ScopeInfo
    {
        public ScopeInfo(ScopeInfo? parent, string name)
        {
            Parent = parent;
            Name = name;
            Console.WriteLine($"Create scope {Fullname}");
        }

        public ScopeInfo? Parent { get; }
        public string Name { get; }
        public string Fullname => Parent != null ? $"{Parent.Fullname}.{Name}" : Name;
    }

    public class UnresolvedScopeInfo : ScopeInfo
    {
        List<string> _namespaces = new List<string>();
        public UnresolvedScopeInfo(ContextBuilder context)
            : base(null, "?")
        {
            _namespaces.AddRange(context.UsingList());
        }
    }
    public class TypeInfo : ScopeInfo
    {
        private List<TypeInfo> _bases;
        private List<MemberInfo> _members = new List<MemberInfo>();
        public TypeInfo(ScopeInfo? parent, string name, TypeQualifier qualifiers)
            : base(parent, name)
        {
            Qualifiers = qualifiers;
            Console.WriteLine($"Create type {Fullname} [{Qualifiers}]");
        }

        public TypeQualifier Qualifiers { get; }
        public void AddBase(TypeInfo baseType) => (_bases ?? new List<TypeInfo>()).Add(baseType);

        public void NewMember(MemberInfo member) => _members.Add(member);
    }

    [Flags]
    public enum TypeQualifier
    {
        QualNone = 0,

        Public = 1,
        Private = 2,
        Internal = 4,
        VisibilityMask = 7,

        Static = 8,

        Class = 0x10,
        Struct = 0x20,
        Enum = 0x30,
        Delegate = 0x40,
        Interface = 0x50,
    }

    [Flags]
    public enum ParamQualifier
    {
        None = 0, 
        In = 1, 
        Out = 2, 
        Ref = 4,
        Optional = 8,
    }

    public class ParameterInfo
    {
        public ParameterInfo(int position, TypeInfo parameterType, string name, ParamQualifier qualifier)
        {
            Position = position;
            ParameterType = parameterType;
            Name = name;
            Qualifier = qualifier;
        }

        public int Position { get; }
        public TypeInfo ParameterType { get; }
        public string Name { get; }
        public ParamQualifier Qualifier { get; }
    }
}
