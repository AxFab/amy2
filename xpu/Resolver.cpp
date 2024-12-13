#include "Resolver.h"

//
bool TypeUtils::is_op_assignment(ExpressionType op)
{
    switch (op) {
    case ExpressionType::AddAssign:
    case ExpressionType::SubtractAssign:
    case ExpressionType::MultiplyAssign:
    case ExpressionType::AddAssignChecked:
    case ExpressionType::SubtractAssignChecked:
    case ExpressionType::MultiplyAssignChecked:
    case ExpressionType::DivideAssign:
    case ExpressionType::ModuloAssign:
    case ExpressionType::PowerAssign:
    case ExpressionType::AndAssign:
    case ExpressionType::OrAssign:
    case ExpressionType::RightShiftAssign:
    case ExpressionType::LeftShiftAssign:
    case ExpressionType::ExclusiveOrAssign:
        return true;
    }
    return false;
}

// Return the corresponding Op of an assignment op.
ExpressionType TypeUtils::binary_op_from_assignment_op(ExpressionType op)
{
    switch (op) {
    case ExpressionType::AddAssign:
        return ExpressionType::Add;
    case ExpressionType::SubtractAssign:
        return ExpressionType::Subtract;
    case ExpressionType::MultiplyAssign:
        return ExpressionType::Multiply;
    case ExpressionType::AddAssignChecked:
        return ExpressionType::AddChecked;
    case ExpressionType::SubtractAssignChecked:
        return ExpressionType::SubtractChecked;
    case ExpressionType::MultiplyAssignChecked:
        return ExpressionType::MultiplyChecked;
    case ExpressionType::DivideAssign:
        return ExpressionType::Divide;
    case ExpressionType::ModuloAssign:
        return ExpressionType::Modulo;
    case ExpressionType::PowerAssign:
        return ExpressionType::Power;
    case ExpressionType::AndAssign:
        return ExpressionType::And;
    case ExpressionType::OrAssign:
        return ExpressionType::Or;
    case ExpressionType::RightShiftAssign:
        return ExpressionType::RightShift;
    case ExpressionType::LeftShiftAssign:
        return ExpressionType::LeftShift;
    case ExpressionType::ExclusiveOrAssign:
        return ExpressionType::ExclusiveOr;
    }
    throw "ContractUtils.Unreachable";
}

bool TypeUtils::is_nullable_type(TypeCode type)
{

}

bool TypeUtils::are_reference_assignable(TypeCode a, TypeCode b)
{

}


bool TypeUtils::are_equivalent(TypeCode a, TypeCode b)
{

}

// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


// Reduces the binary expression node to a simpler expression.  If CanReduce returns true, this should return a valid expression. This method is allowed to return another node which itself must be reduced.
Expression *BinaryExpression::reduce()
{
    // Only reduce OpAssignment expressions.
    if (is_op_assignment(node_type())) {
        switch (_left->node_type()) {
        case ExpressionType::MemberAccess:
            return reduce_member();
        case ExpressionType::Index:
            return reduce_index();
        default:
            return reduce_variable();
        }
    }
    return this;
}

//
Expression *BinaryExpression::reduce_member()
{

    MemberExpression *member = (MemberExpression *)left();

    if (member->expression() == nullptr) {
        // static member, reduce the same as variable
        return reduce_variable();
    } else {
        // left.b (op)= r
        // ... is reduced into ...
        // temp1 = left
        // temp2 = temp1.b (op) r
        // temp1.b = temp2
        // temp2
        ParameterExpression *temp1 = Expression::variable(member->expression().Type, "temp1");

        // 1. temp1 = left
        Expression e1 = Expression::assign(temp1, member->expression());

        // 2. temp2 = temp1.b (op) r
        ExpressionType op = GetBinaryOpFromAssignmentOp(NodeType);
        Expression e2 = Expression.MakeBinary(op, Expression.MakeMemberAccess(temp1, member.Member), Right, false, Method);
        LambdaExpression ? conversion = GetConversion();
        if (conversion != null) {
            e2 = Expression.Invoke(conversion, e2);
        }
        ParameterExpression temp2 = Variable(e2.Type, "temp2");
        e2 = Expression.Assign(temp2, e2);

        // 3. temp1.b = temp2
        Expression e3 = Expression.Assign(Expression.MakeMemberAccess(temp1, member.Member), temp2);

        // 3. temp2
        Expression e4 = temp2;

        return Expression.Block(
            new TrueReadOnlyCollection<ParameterExpression>(temp1, temp2),
            new TrueReadOnlyCollection<Expression>(e1, e2, e3, e4)
        );
    }
}

//
Expression *BinaryExpression::reduce_index()
{
    // left[a0, a1, ... aN] (op)= r
    //
    // ... is reduced into ...
    //
    // tempObj = left
    // tempArg0 = a0
    // ...
    // tempArgN = aN
    // tempValue = tempObj[tempArg0, ... tempArgN] (op) r
    // tempObj[tempArg0, ... tempArgN] = tempValue

    var index = (IndexExpression)Left;

    var vars = new ArrayBuilder<ParameterExpression>(index.ArgumentCount + 2);
    var exprs = new ArrayBuilder<Expression>(index.ArgumentCount + 3);

    ParameterExpression tempObj = Expression.Variable(index.Object!.Type, "tempObj");
    vars.UncheckedAdd(tempObj);
    exprs.UncheckedAdd(Expression.Assign(tempObj, index.Object));

    int n = index.ArgumentCount;
    var tempArgs = new ArrayBuilder<Expression>(n);
    for (var i = 0; i < n; i++) {
        Expression arg = index.GetArgument(i);
        ParameterExpression tempArg = Expression.Variable(arg.Type, "tempArg" + i);
        vars.UncheckedAdd(tempArg);
        tempArgs.UncheckedAdd(tempArg);
        exprs.UncheckedAdd(Expression.Assign(tempArg, arg));
    }

    IndexExpression tempIndex = Expression.MakeIndex(tempObj, index.Indexer, tempArgs.ToReadOnly());

    // tempValue = tempObj[tempArg0, ... tempArgN] (op) r
    ExpressionType binaryOp = GetBinaryOpFromAssignmentOp(NodeType);
    Expression op = Expression.MakeBinary(binaryOp, tempIndex, Right, false, Method);
    LambdaExpression ? conversion = GetConversion();
    if (conversion != null) {
        op = Expression.Invoke(conversion, op);
    }
    ParameterExpression tempValue = Expression.Variable(op.Type, "tempValue");
    vars.UncheckedAdd(tempValue);
    exprs.UncheckedAdd(Expression.Assign(tempValue, op));

    // tempObj[tempArg0, ... tempArgN] = tempValue
    exprs.UncheckedAdd(Expression.Assign(tempIndex, tempValue));

    return Expression.Block(vars.ToReadOnly(), exprs.ToReadOnly());
}

//
Expression *BinaryExpression::reduce_variable()
{
    // v (op)= r
    // ... is reduced into ...
    // v = v (op) r
    ExpressionType op = binary_op_from_assignment_op(node_type());
    Expression *r = Expression::make_binary(op, left(), right(), false, method());
    LambdaExpression *conversion = conversion();
    if (conversion != nullptr) {
        r = Expression::invoke(conversion, r);
    }
    return Expression::assign(left(), r);
}

bool BinaryExpression::can_reduce() const
{
    return TypeUtils::is_op_assignment(node_type());
}

bool BinaryExpression::is_lifted()
{
    if (node_type() == ExpressionType::Coalesce || node_type() == ExpressionType::Assign)
        return false;
    if (TypeUtils::is_nullable_type(left()->type_code())) {
        void *meth = method();
        return meth == nullptr ||
            !TypeUtils::are_equivalent(meth.GetParametersCached()[0].ParameterType.GetNonRefType(),
                left()->type_code());
    }
    return false;
}


bool BinaryExpression::is_lifted_to_null()
{
    return is_lifted() && TypeUtils::is_nullable_type(type_code());
}


// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


Expression *Expression::make_binary(ExpressionType node_type, Expression *left, Expression *right, TypeCode type, void *method, void *conversion)
{
    if (node_type == ExpressionType::Assign)
        throw "Bad arguments";
    if (conversion != nullptr) {
        if (!(method == nullptr && TypeUtils::are_equivalent(type, right->type_code()) && node_type == ExpressionType::Coalesce))
            throw "Bad arguments";
        return new CoalesceConversionBinaryExpression(left, right, conversion);
    }
    if (method != nullptr) {
        return new MethodBinaryExpression(node_type, left, right, type, method);
    }
    if (type == TypeCode::Boolean) {
        return new LogicalBinaryExpression(node_type, left, right);
    }
    return new SimpleBinaryExpression(node_type, left, right, type);
}

Expression *Expression::invoke(Expression *expression, ...)
{
}

Expression *Expression::assign(Expression *var, Expression *value)
{
}

Expression *Expression::variable(TypeCode code, const std::string &name)
{
}


// -=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=

