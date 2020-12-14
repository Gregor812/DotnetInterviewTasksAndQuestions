using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Utils
{
    // Second
    // IT DOESN'T WORK BECAUSE I DON'T KNOW (MAYBE THAT'S TRUE ONLY FOR SQLITE PROVIDER?)
    public static class CompiledExpressions
    {
        private static readonly
            ConcurrentDictionary<LambdaExpression, Delegate> Cache = new();

        public static T AsFunc<T>(this Expression<T> expr) =>
            (T)(object)Cache.GetOrAdd(expr, (Delegate)(object)expr.Compile());
    }
}
