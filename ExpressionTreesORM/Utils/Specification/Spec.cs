using System;
using System.Linq.Expressions;

namespace Utils.Specification
{
    public class Spec<T>
    {
        public virtual Expression<Func<T, bool>> Expression { get; init; }

        public Spec()
        { }

        public Spec(Expression<Func<T,bool>> expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public static implicit operator Expression<Func<T, bool>>(Spec<T> spec) =>
            spec?.Expression ?? throw new ArgumentNullException(nameof(spec));

        public static implicit operator Func<T, bool>(Spec<T> spec) =>
            spec.IsSatisfiedBy;

        public static bool operator true(Spec<T> _) => false;

        public static bool operator false(Spec<T> _) => false;

        public bool IsSatisfiedBy(T t) => Expression?.AsFunc()(t) ?? false;

        public static Spec<T> operator &(Spec<T> spec1, Spec<T> spec2) =>
            new AndSpec<T>(spec1, spec2);

        public static Spec<T> operator |(Spec<T> spec1, Spec<T> spec2) =>
            new OrSpec<T>(spec1, spec2);

        public static Spec<T> operator !(Spec<T> spec) =>
            new NotSpec<T>(spec);
    }
}
