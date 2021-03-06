﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using XData.Common;
using XData.Extentions;
using XData.Meta;
using XData.XBuilder;

namespace XData.Core.ExpressionVisitors
{
    /// <summary>
    /// 查询表达式访问器
    /// </summary>
    internal class SqlExpressionVistor : ExpressionVisitor
    {
        #region Fields
        private readonly SqlBuilber privoder;
        private readonly Expression expression;
        private readonly Stack<ExpressionType> nodeTypes = new Stack<ExpressionType>();
        private readonly Stack<string> methodTypes = new Stack<string>();
        private readonly TypeVisitor typeVisitor;
        private string sql = string.Empty;

        #endregion

        #region Constuctors
        private SqlExpressionVistor(Expression expression, SqlBuilber privoder)
        {
            this.expression = expression;
            this.privoder = privoder;
            this.typeVisitor = privoder.typeVisitor;
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
            var value = node.Value; // builber.Context.DatabaseType.MapParameterValue(node.Value);
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
                if (lastNode == ExpressionType.Block)
                {
                    if (value.Equals(true))
                    {
                        sql += "(1 = 1)";
                    }
                    else
                    {
                        sql += "(1 = 0)";
                    }
                }
                else
                {
                    sql += privoder.GetParameterIndex();
                    privoder.parameters.Add(value);
                }
            }

            nodeTypes.Pop();
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            var lastNode = nodeTypes.Peek();
            var member = node.Member;
            //if (node.Expression == null)
            //{
            //    throw Error.NotSupportedException("不支持静态成员转换成Sql。Member："+ member.DeclaringType.Name+"."+member.Name);
            //}

            if (member.DeclaringType.IsGenericType && member.DeclaringType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (member.Name == "Value")
                {
                    return Visit(node.Expression);
                }
                else if (member.Name == "HasValue")
                {
                    sql += "(";
                    var n = Visit(node.Expression);
                    sql += " IS NOT NULL)";
                    return n;
                }
            }

            object value;
            var hasValue = TryGetExpressionValue(node, out value);
            var namedType = typeVisitor.Get(node.Expression?.Type);
            if (namedType != null)
            {
                if (lastNode == ExpressionType.MemberAccess)
                {
                    throw Error.NotSupportedException("不支持嵌套属性。");
                }
                nodeTypes.Push(node.NodeType);
                if (lastNode == ExpressionType.Call && methodTypes.Any())
                {
                    var fieldName = namedType.GetSql(member, privoder);
                    ConvertMethod(hasValue, value, fieldName);
                }
                else
                {
                    if (hasValue)
                    {
                        sql += privoder.GetParameterIndex();
                        privoder.parameters.Add(value);
                    }
                    else
                    {
                        sql += namedType.GetSql(member, privoder);

                        if (node.Type.NonNullableType() == typeof(bool)
                            && (lastNode == ExpressionType.AndAlso
                                || lastNode == ExpressionType.OrElse
                                || lastNode == ExpressionType.Not
                                || lastNode == ExpressionType.Block))
                        {
                            sql += " = ";
                            sql += privoder.GetParameterIndex();
                            privoder.parameters.Add(true);
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
                    sql += privoder.GetParameterIndex();
                    privoder.parameters.Add(value);
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
                    sql += privoder.GetParameterIndex();
                    privoder.parameters.Add(value);
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
            switch (node.NodeType)
            {
                case ExpressionType.Convert:
                case ExpressionType.Unbox:
                    base.Visit(node.Operand);
                    break;
                case ExpressionType.Not:
                    sql += " NOT( ";
                    base.Visit(node.Operand);
                    sql += ")";
                    break;
                default:
                    throw Error.NotSupportedException("不支持的一元运算:" + node.NodeType);
            }
            nodeTypes.Pop();
            return node;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            nodeTypes.Push(node.NodeType);
            var method = node.Method;
            sql += "(";
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
            else if (method.IsListContains())
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
            else if (method.IsBetween())
            {
                base.Visit(node.Arguments[0]);
                sql += " BETWEEN ";
                methodTypes.Push(Constans.ObjectBetween);
                base.Visit(node.Arguments[1]);
                sql += " AND ";
                base.Visit(node.Arguments[2]);
                methodTypes.Pop();
            }
            else
            {
                throw Error.NotSupportedException("不支持的方法运算:" + method.Name);
            }
            sql += ")";
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

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (DbTypes.IsSimpleType(node.Type))
            {
                var namedType = typeVisitor.Get(node.Type);
                if (namedType != null)
                {
                    sql += namedType.GetSql(null);
                    return node;
                }
            }
            return base.VisitParameter(node);
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
                        sql += privoder.GetParameterIndex();
                        sql += "+'%'";
                        privoder.parameters.Add(value);
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
                        sql += privoder.GetParameterIndex();
                        sql += "+'%'";
                        privoder.parameters.Add(value);
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
                        sql += privoder.GetParameterIndex();
                        privoder.parameters.Add(value);
                    }
                    else
                    {
                        sql += "'%'+";
                        sql += fieldName;
                    }
                    break;
                case Constans.StringSqlLike:
                case Constans.ObjectEquals:
                case Constans.ObjectBetween:
                    if (hasValue)
                    {
                        sql += privoder.GetParameterIndex();
                        privoder.parameters.Add(value);
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
            if (exp.NodeType == ExpressionType.Convert)
            {
                var e = exp as UnaryExpression;
                if (e.Type.NonNullableType() == e.Operand.Type.NonNullableType())
                {
                    if (TryGetExpressionValue(e.Operand, out value))
                    {
                        return true;
                    }
                }
            }
            if (exp is ConstantExpression)
            {
                value = (exp as ConstantExpression).Value;
                return true;
            }
            if (exp is MemberExpression)
            {
                var member = exp as MemberExpression;
                var m = member.Member;

                object obj = null;
                var needInvoke = member.Expression == null;
                if (!needInvoke)
                {
                    needInvoke = TryGetExpressionValue(member.Expression, out obj);
                    if (needInvoke && obj == null)
                    {
                        throw new XDataException("表达式的节点值错误");
                    }
                }
                if (needInvoke)
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
                sql += privoder.GetParameterIndex() + ",";
                privoder.parameters.Add(v);
            }
            sql = sql.TrimEnd(',');
            sql += ")";
        }

        #endregion

        #region Public Methods

        private string ToSql()
        {
            if (sql.IsNullOrWhiteSpace())
            {
                this.Visit(expression);
            }
            return sql;
        }
        #endregion

        public static string Visit(Expression expression, SqlBuilber privoder)
        {
            var visitor = new SqlExpressionVistor(expression, privoder);
            return visitor.ToSql();
        }
    }
}