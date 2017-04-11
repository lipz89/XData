using System.Linq;
using System.Linq.Expressions;

namespace XData.Core.ExpressionVisitors
{
    /// <summary>
    /// 查询表达式参数替换访问器
    /// </summary>
    internal class ParameterReplaceVisitor : ExpressionVisitor
    {
        #region Fields
        private readonly ParameterExpression[] parameters;
        #endregion

        #region Constuctors
        public ParameterReplaceVisitor(params ParameterExpression[] parameters)
        {
            this.parameters = parameters;
        }
        #endregion

        #region Override ExpressionVisitor Methods
        protected override Expression VisitParameter(ParameterExpression node)
        {
            var parameter = parameters.FirstOrDefault(x => x.Type == node.Type);
            return parameter ?? node;
        }
        #endregion

        #region StaticMethods

        public static Expression Replace(Expression expression, ParameterExpression parameter)
        {
            return new ParameterReplaceVisitor(parameter).Visit(expression);
        }
        #endregion
    }
}