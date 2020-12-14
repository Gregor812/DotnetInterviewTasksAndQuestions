using System;
using System.Linq.Expressions;

namespace Utils.Specification
{
    public abstract class Spec<T>
    {
        public abstract Expression<Func<T, bool>> ToExpression();

        public static implicit operator Expression<Func<T, bool>>(Spec<T> spec) =>
            spec?.ToExpression() ?? throw new ArgumentNullException(nameof(spec));

        public static implicit operator Func<T, bool>(Spec<T> spec) =>
            spec.IsSatisfiedBy;

        public static bool operator true(Spec<T> s) => false;

        public static bool operator false(Spec<T> s) => false;

        public bool IsSatisfiedBy(T t) => ToExpression().AsFunc()(t);

        public static Spec<T> operator &(Spec<T> spec1, Spec<T> spec2) =>
            new AndSpec<T>(spec1, spec2);

        public static Spec<T> operator |(Spec<T> spec1, Spec<T> spec2) =>
            new OrSpec<T>(spec1, spec2);

        public static Spec<T> operator !(Spec<T> spec) =>
            new NotSpec<T>(spec);
    }
}
