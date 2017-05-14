using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DTSharp.Helper
{
    static class IQueryableHelper
    {
        public static IQueryable GroupBy(IQueryable collection, Expression lambdaExpression, Type collectionType, Type groupKey)
        {
            var method = typeof(Queryable).GetMethods().Where(x => x.Name == "GroupBy").First(x => x.GetParameters().Length == 2);
            method = method.MakeGenericMethod(collectionType, groupKey);
            return method.Invoke(null, new object[] { collection, lambdaExpression }) as IQueryable;
        }
        public static IQueryable Select(IQueryable collection, Expression lambdaExpression, Type elementType, Type selectedType)
        {
            var method = typeof(Queryable).GetMethods().Where(x => x.Name == "Select").First(x => x.GetParameters().Length == 2);
            method = method.MakeGenericMethod(elementType, selectedType);
            return method.Invoke(null, new object[] { collection, lambdaExpression }) as IQueryable;
        }
        public static IQueryable OrderBy(IQueryable collection, Expression lambdaExpression, Type collectionType, Type orderedType)
        {
            var method = typeof(Queryable).GetMethods().Where(x => x.Name == "OrderBy").First(x => x.GetParameters().Length == 2);
            method = method.MakeGenericMethod(collectionType, orderedType);
            return method.Invoke(null, new object[] { collection, lambdaExpression }) as IQueryable;
        }
    }
}
