using System;
using System.Linq.Expressions;

namespace Utils.Specification
{
    public class AndSpec<T> : Spec<T>
    {
        private readonly Spec<T> _spec1;
        private readonly Spec<T> _spec2;

        public AndSpec(Spec<T> spec1, Spec<T> spec2)
        {
            _spec1 = spec1 ?? throw new ArgumentNullException(nameof(spec1));
            _spec2 = spec2 ?? throw new ArgumentNullException(nameof(spec2));
        }

        public override Expression<Func<T, bool>> Expression
        {
            get
            {
                var expr1 = _spec1.Expression;
                var expr2 = _spec2.Expression;
                return expr1.AndAlso(expr2);
            }
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is AndSpec<T> other)
                return Equals(other);
            return false;
        }

        protected bool Equals(AndSpec<T> other)
        {
            return Equals(_spec1, other._spec1) && Equals(_spec2, other._spec2);
        }

        public override int GetHashCode()
        {
            return _spec1.GetHashCode() ^ _spec2.GetHashCode() ^ GetType().GetHashCode();
        }
    }
}
