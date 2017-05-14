using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp.Helper
{
    static class ExpressionHelper
    {
        public static Expression ReplaceLambdaParameter(LambdaExpression lambdaExpression, Expression replacedExpression)
        {
            var oldParameter = lambdaExpression.Parameters[0];
            return new ReplaceExpressionVisitor(oldParameter, replacedExpression).Visit(lambdaExpression.Body);
        }
        class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
        public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method;
        }
    }
}
