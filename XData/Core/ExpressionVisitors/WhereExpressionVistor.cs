using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;
using XData.Extentions;
using XData.XBuilder;

namespace XData.Core.ExpressionVisitors
{
    /// <summary>
    /// 查询表达式访问器
    /// </summary>
    internal class WhereExpressionVistor : ExpressionVisitor
    {
        #region Fields
        private readonly SqlBuilber sqlBuilber;
        private readonly Expression expression;
        private readonly Stack<ExpressionType> nodeTypes = new Stack<ExpressionType>();
        private readonly Stack<string> methodTypes = new Stack<string>();
        private string sql = string.Empty;
        private Dictionary<Type, string> types;

        #endregion

        #region Constuctors
        public WhereExpressionVistor(Expression expression, SqlBuilber sqlBuilber, Dictionary<Type, string> types)
        {
            this.expression = expression;
            this.sqlBuilber = sqlBuilber;
            this.types = types;
            nodeTypes.Push(ExpressionType.Block);
        }

        #endregion

        #region Override ExpressionVisitor Methods
        protected override Expression VisitBinary(BinaryExpression node)
        {
            nodeTypes.Push(node.NodeType);
            sql += "(";

            if (node.Left is ConstantExpression && (node.Left as ConstantExpression).Value == null)
            {
                base.Visit(node.Right);
                if (node.NodeType == ExpressionType.Equal)
                {
                    sql += " IS NULL ";
                }
                else if (node.NodeType == ExpressionType.NotEqual)
                {
                    sql += " IS NOT NULL ";
                }
                else
                {
                    throw Error.NotSupportedException("不支持NULL的二元运算:" + node.NodeType);
                }
            }
            else if (node.Right is ConstantExpression && (node.Right as ConstantExpression).Value == null)
            {
                base.Visit(node.Left);
                if (node.NodeType == ExpressionType.Equal)
                {
                    sql += " IS NULL ";
                }
                else if (node.NodeType == ExpressionType.NotEqual)
                {
                    sql += " IS NOT NULL ";
                }
                else
                {
                    throw Error.NotSupportedException("不支持NULL的二元运算:" + node.NodeType);
                }
            }
            else
            {
                var sp = string.Empty;
                switch (node.NodeType)
                {
                    case ExpressionType.AndAlso:
                        sp = " AND ";
                        break;
                    case ExpressionType.OrElse:
                        sp = " OR ";
                        break;
                    case ExpressionType.Add:
                        sp = " + ";
                        break;
                    case ExpressionType.Subtract:
                        sp = " - ";
                        break;
                    case ExpressionType.Multiply:
                        sp = " * ";
                        break;
                    case ExpressionType.Divide:
                        sp = " / ";
                        break;
                    case ExpressionType.Modulo:
                        sp = " % ";
                        break;
                    case ExpressionType.Equal:
                        sp = " = ";
                        break;
                    case ExpressionType.NotEqual:
                        sp = " != ";
                        break;
                    case ExpressionType.GreaterThan:
                        sp = " > ";
                        break;
                    case ExpressionType.GreaterThanOrEqual:
                        sp = " >= ";
                        break;
                    case ExpressionType.LessThan:
                        sp = " < ";
                        break;
                    case ExpressionType.LessThanOrEqual:
                        sp = " <= ";
                        break;
                    case ExpressionType.And:
                        sp = " & ";
                        break;
                    case ExpressionType.Or:
                        sp = " | ";
                        break;
                    case ExpressionType.ExclusiveOr:
                        sp = " ^ ";
                        break;
                    default:
                        throw Error.NotSupportedException("不支持的二元运算:" + node.NodeType);
                }
                base.Visit(node.Left);
                sql += sp;
                base.Visit(node.Right);
            }
            sql += ")";
            nodeTypes.Pop();
            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            var lastNode = nodeTypes.Peek();
            nodeTypes.Push(node.NodeType);
            var value = node.Value;// builber.Context.DatabaseType.MapParameterValue(node.Value);
            if (value is IEnumerable && !(value is string))
            {
                var enumer = value as IEnumerable;
                SetEnumerable(enumer);
            }
            else if (lastNode == ExpressionType.Call && methodTypes.Any())
            {
                ConvertMethod(true, value);
            }
            else
            {
                sql += sqlBuilber.GetParameterIndex();
                sqlBuilber._parameters.Add(value);
            }
            nodeTypes.Pop();
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            Type t;
            object value;
            var hasValue = TryGetExpressionValue(node, out value);
            var member = node.Member;
            if (member.ReflectedType != null && member.ReflectedType.IsAssignableFromOneOf(types.Keys, out t))
            {
                var lastNode = nodeTypes.Peek();
                nodeTypes.Push(node.NodeType);
                if (lastNode == ExpressionType.Call && methodTypes.Any())
                {
                    var fieldName = sqlBuilber.EscapeSqlIdentifier(types[t]) + "." + sqlBuilber.GetColumnName(member, node.Expression.Type);
                    ConvertMethod(hasValue, value, fieldName);
                }
                else
                {
                    if (hasValue)
                    {
                        sql += sqlBuilber.GetParameterIndex();
                        sqlBuilber._parameters.Add(value);
                    }
                    else
                    {
                        base.VisitMember(node);
                        if (node.Expression is ParameterExpression)
                        {
                            Type pt;
                            if (node.Expression.Type.IsAssignableFromOneOf(types.Keys, out pt))
                            {
                                sql += sqlBuilber.EscapeSqlIdentifier(types[t]) + ".";
                            }
                            else
                            {
                                throw Error.NotSupportedException("不支持的参数类型。");
                                //sql += sqlBuilber.GetTableName(node.Expression.Type) + ".";
                            }
                        }
                        sql += sqlBuilber.GetColumnName(member, node.Expression.Type);
                        if (lastNode == ExpressionType.MemberAccess)
                        {
                            throw Error.NotSupportedException("不支持嵌套属性。");
                        }

                        if (node.Type.NonNullableType() == typeof(bool)
                            && (lastNode == ExpressionType.AndAlso
                                || lastNode == ExpressionType.OrElse
                                || lastNode == ExpressionType.Not
                                || lastNode == ExpressionType.Block))
                        {
                            sql += " = ";
                            sql += sqlBuilber.GetParameterIndex();
                            sqlBuilber._parameters.Add(true);
                        }
                    }
                }
                nodeTypes.Pop();
            }
            else if (member == typeof(string).GetProperty("Length"))
            {
                sql += "LEN(";
                if (hasValue)
                {
                    sql += sqlBuilber.GetParameterIndex();
                    sqlBuilber._parameters.Add(value);
                }
                else
                {
                    base.VisitMember(node);
                }
                sql += ")";
            }
            else if (methodTypes.Any() && methodTypes.Peek() == Constans.EnumerableContains)
            {
                if (hasValue)
                {
                    SetEnumerable(value as IEnumerable);
                }
                else
                {
                    throw Error.NotSupportedException("不支持动态枚举的Contains方法。");
                }
            }
            else
            {
                if (hasValue)
                {
                    sql += sqlBuilber.GetParameterIndex();
                    sqlBuilber._parameters.Add(value);
                }
                else
                {
                    throw Error.NotSupportedException("不支持的成员。");
                }
            }
            return node;
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            nodeTypes.Push(node.NodeType);
            sql += "(";
            switch (node.NodeType)
            {
                //case ExpressionType.Convert:
                //case ExpressionType.Unbox:
                //    var tt = node.Type;
                //    var st = string.Empty;
                //    if (tt == typeof(int))
                //    {
                //        st = "INT";
                //    }
                //    sql += "CONVERT(" + st + ",";
                //    base.Visit(node.Operand);
                //    sql += ")";
                //    break;
                case ExpressionType.Not:
                    sql += " NOT( ";
                    base.Visit(node.Operand);
                    sql += ")";
                    break;
                default:
                    throw Error.NotSupportedException("不支持的一元运算:" + node.NodeType);
            }
            sql += ")";
            nodeTypes.Pop();
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            nodeTypes.Push(node.NodeType);
            var method = node.Method;
            if (method == Constans.MethodStringContains)
            {
                base.Visit(node.Object);
                sql += " LIKE ";
                methodTypes.Push(Constans.StringContains);
                base.Visit(node.Arguments[0]);
                methodTypes.Pop();
            }
            else if (method == Constans.MethodStringStartsWith)
            {
                base.Visit(node.Object);
                sql += " LIKE ";
                methodTypes.Push(Constans.StringStartsWith);
                base.Visit(node.Arguments[0]);
                methodTypes.Pop();
            }
            else if (method == Constans.MethodStringEndsWith)
            {
                base.Visit(node.Object);
                sql += " LIKE ";
                methodTypes.Push(Constans.StringEndsWith);
                base.Visit(node.Arguments[0]);
                methodTypes.Pop();
            }
            else if (method == Constans.MethodStringSqlLike)
            {
                base.Visit(node.Arguments[0]);
                sql += " LIKE ";
                methodTypes.Push(Constans.StringSqlLike);
                base.Visit(node.Arguments[1]);
                methodTypes.Pop();
            }
            else if (method.IsGenericMethod && method.GetGenericMethodDefinition() == Constans.MethodEnumerableContains)
            {
                base.Visit(node.Arguments[1]);
                sql += " IN ";
                methodTypes.Push(Constans.EnumerableContains);
                base.Visit(node.Arguments[0]);
                methodTypes.Pop();
            }
            else if (Constans.IsListContains(method))
            {
                base.Visit(node.Arguments[0]);
                sql += " IN ";
                methodTypes.Push(Constans.EnumerableContains);
                base.Visit(node.Object);
                methodTypes.Pop();
            }
            else if (method == Constans.MethodObjectEquals)
            {
                base.Visit(node.Object);
                sql += " = ";
                methodTypes.Push(Constans.ObjectEquals);
                base.Visit(node.Arguments[0]);
                methodTypes.Pop();
            }
            else
            {
                throw Error.NotSupportedException("不支持的方法运算:" + method.Name);
            }

            nodeTypes.Pop();
            return node;
        }

        protected override Expression VisitListInit(ListInitExpression node)
        {
            if (methodTypes.Any() && methodTypes.Peek() == Constans.EnumerableContains)
            {
                var enumer = node.Initializers.SelectMany(x => x.Arguments.Select(GetExpressionValue));
                this.SetEnumerable(enumer);
            }
            return node;
        }

        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            if (methodTypes.Any() && methodTypes.Peek() == Constans.EnumerableContains)
            {
                var enumer = node.Expressions.Select(GetExpressionValue);
                this.SetEnumerable(enumer);
            }
            return node;
        }

        #endregion

        #region Private Methods

        private void ConvertMethod(bool hasValue, object value, string fieldName = null)
        {
            var methodType = methodTypes.Peek();
            switch (methodType)
            {
                case Constans.StringContains:
                    if (hasValue)
                    {
                        sql += "'%'+";
                        sql += sqlBuilber.GetParameterIndex();
                        sql += "+'%'";
                        sqlBuilber._parameters.Add(value);
                    }
                    else
                    {
                        sql += "'%'+";
                        sql += fieldName;
                        sql += "+'%'";
                    }
                    break;
                case Constans.StringStartsWith:
                    if (hasValue)
                    {
                        sql += sqlBuilber.GetParameterIndex();
                        sql += "+'%'";
                        sqlBuilber._parameters.Add(value);
                    }
                    else
                    {
                        sql += fieldName;
                        sql += "+'%'";
                    }
                    break;
                case Constans.StringEndsWith:
                    if (hasValue)
                    {
                        sql += "'%'+";
                        sql += sqlBuilber.GetParameterIndex();
                        sqlBuilber._parameters.Add(value);
                    }
                    else
                    {
                        sql += "'%'+";
                        sql += fieldName;
                    }
                    break;
                case Constans.StringSqlLike:
                    if (hasValue)
                    {
                        sql += sqlBuilber.GetParameterIndex();
                        sqlBuilber._parameters.Add(value);
                    }
                    else
                    {
                        sql += fieldName;
                    }
                    break;
                case Constans.ObjectEquals:
                    if (hasValue)
                    {
                        sql += sqlBuilber.GetParameterIndex();
                        sqlBuilber._parameters.Add(value);
                    }
                    else
                    {
                        sql += fieldName;
                    }
                    break;
            }
        }
        private bool TryGetExpressionValue(Expression exp, out object value)
        {
            if (exp is ConstantExpression)
            {
                value = (exp as ConstantExpression).Value;
                return true;
            }
            if (exp is MemberExpression)
            {
                var member = exp as MemberExpression;
                var m = member.Member;

                object obj;
                if (TryGetExpressionValue(member.Expression, out obj))
                {
                    if (m is PropertyInfo)
                    {
                        var p = m as PropertyInfo;
                        value = p.GetValue(obj);
                        return true;
                    }
                    if (m is FieldInfo)
                    {
                        var f = m as FieldInfo;
                        value = f.GetValue(obj);
                        return true;
                    }
                }
            }
            value = null;
            return false;
        }
        private object GetExpressionValue(Expression exp)
        {
            object obj;
            TryGetExpressionValue(exp, out obj);
            return obj;
        }

        private void SetEnumerable(IEnumerable enumer)
        {
            sql += "(";
            foreach (var v in enumer)
            {
                sql += sqlBuilber.GetParameterIndex() + ",";
                sqlBuilber._parameters.Add(v);
            }
            sql = sql.TrimEnd(',');
            sql += ")";
        }

        #endregion

        #region Public Methods
        public string ToSql()
        {
            if (sql.IsNullOrWhiteSpace())
            {
                this.Visit(expression);
            }
            return sql;
        }
        #endregion
    }
}