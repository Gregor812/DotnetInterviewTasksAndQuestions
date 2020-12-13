using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Models
{
    // Second
    // IT DOESN'T WORK
    public static class CompiledExpressions
    {
        private static readonly ConcurrentDictionary<LambdaExpression, Delegate> Cache = new();

        public static T AsFunc<T>(this Expression<T> expr) => (T)(object)Cache.GetOrAdd(expr, (Delegate)(object)expr.Compile());
    }
}
