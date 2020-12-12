using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Models
{
    // First
    public static class CompiledExpressions<TExpression, TDelegate>
    {
        private static readonly ConcurrentDictionary<
            Expression<Func<TExpression, TDelegate>>,
            Func<TExpression, TDelegate>> Cache = new ();

        public static Func<TExpression, TDelegate> AsFunc(Expression<Func<TExpression, TDelegate>> expr) =>
            Cache.GetOrAdd(expr, expr.Compile());
    }
}
