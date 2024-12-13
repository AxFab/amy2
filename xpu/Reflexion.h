#pragma once
#include <unordered_map>
class ContextInfo;
class TypeInfo;

enum TypeQualifier
{
    TypeQualNone = 0,
    TypePublic = 1,
    TypePrivate = 2,
    TypeInternal = 4,
    TypeVisibilityMask = 7,
};
TypeQualifier operator |=(TypeQualifier a, TypeQualifier b)
{
    return (TypeQualifier)((unsigned)a | (unsigned)b);
}

class ScopeInfo
{
private:
    ScopeInfo *_parent;
    std::string _name;
public:
    ScopeInfo(ScopeInfo *parent, const std::string &name)
        : _parent(parent), _name(name)
    {
    }
    std::string name() const { return _name; }
    std::string fullname() const { return _parent ? _parent->fullname() + "." + _name : _name; }
};

class MemberInfo : public ScopeInfo
{
private:
    TypeQualifier _qualifier;
public:
    MemberInfo(ScopeInfo *parent, const std::string &name, TypeQualifier qualifier)
        : ScopeInfo(parent, name), _qualifier(qualifier)
    {
    }
};


class FieldInfo : public MemberInfo
{
    friend class ContextInfo;
private:
    TypeInfo *_type;
public:
    FieldInfo(TypeInfo *parent, const std::string &name, TypeInfo *field_type, TypeQualifier qualifier)
        : MemberInfo((ScopeInfo *)parent, name, qualifier), _type(field_type)
    {
    }
};

class ParameterInfo
{
private:
    TypeInfo *_type;
    std::string _name;
    int _qual;
public:
    ParameterInfo(TypeInfo *type, const std::string &name, int qual)
        : _type(type), _name(name), _qual(qual)
    {
    }
};

class TypeInfo : public MemberInfo
{
    friend class ContextInfo;
private:
    std::vector<TypeInfo *> _base_types;
    std::vector<FieldInfo *> _fields;
    std::vector<FieldInfo *> _properties;
    std::vector<FieldInfo *> _methods;
public:
    TypeInfo(ScopeInfo *parent, const std::string &name, TypeQualifier qualifier)
        : MemberInfo(parent, name, qualifier)
    {
    }
};



class UnresolvedScopeInfo : public ScopeInfo
{
private:
    ContextInfo *_context;
public:
    UnresolvedScopeInfo(ContextInfo *context)
        : _context(context), ScopeInfo(nullptr, "?")
    {
    }
};

class ContextInfo
{
private:
    ContextInfo *_parent;
    ScopeInfo *_scope;
    UnresolvedScopeInfo *_unresolved_scope;
    bool _type;
public:
    ContextInfo(ContextInfo *parent, ScopeInfo *scope, bool type = false)
        : _parent(parent), _scope(scope), _type(type)
    {
        _unresolved_scope = new UnresolvedScopeInfo(this);
    }
    ~ContextInfo()
    {
        delete _unresolved_scope;
    }
    bool is_type() const { return _type; }
    ScopeInfo *scope() const { return _scope; }
    ContextInfo *parent() const { return _parent; }
    ScopeInfo *unresolved_scope() const { return _unresolved_scope; }

    void add_base(TypeInfo *type) { static_cast<TypeInfo *>(_scope)->_base_types.push_back(type); }

    void create_field(const std::string &name, TypeInfo *field_type, TypeQualifier qualifier)
    {
        TypeInfo *type = static_cast<TypeInfo *>(_scope);
        FieldInfo *field = new FieldInfo(type, name, field_type, qualifier);
        type->_fields.push_back(field);
    }
};
