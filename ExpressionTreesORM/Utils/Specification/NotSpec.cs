using System;
using System.Linq.Expressions;

namespace Utils.Specification
{
    public class NotSpec<T> : Spec<T>
    {
        private readonly Spec<T> _spec;

        public NotSpec(Spec<T> spec)
        {
            _spec = spec ?? throw new ArgumentNullException(nameof(spec));
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var expr = _spec.ToExpression();
            return Expression.Lambda<Func<T, bool>>(Expression.Not(expr.Body), expr.Parameters);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is NotSpec<T> other)
                return Equals(other);
            return false;
        }

        protected bool Equals(NotSpec<T> other)
        {
            return Equals(_spec, other._spec);
        }

        public override int GetHashCode()
        {
            return _spec.GetHashCode() ^ GetType().GetHashCode();
        }
    }
}
