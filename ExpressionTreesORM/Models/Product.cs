using System;
using System.Linq.Expressions;
using Utils;
using Utils.Specification;

namespace Models
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int InStock { get; set; }
        public bool IsForSale { get; set; }

        // First
        // public bool IsAvailable => IsForSale && InStock > 0;

        // Second
        // IT DOESN'T WORK BECAUSE I DON'T KNOW (MAYBE THAT'S TRUE ONLY FOR SQLITE PROVIDER?)

        //private static readonly Expression<Func<Product, bool>>
        //    IsAvailableExpression = x => x.IsForSale && x.InStock > 0;

        //public bool IsAvailable => IsAvailableExpression.AsFunc()(this);

        // Fourth
        // It works as well!
        public static Spec<Product> IsInStockSpec => new (p => p.InStock > 0);
        public static Spec<Product> IsForSaleSpec => new (p => p.IsForSale);
        public static Spec<Product> IsAvailable => new AndSpec<Product>(IsInStockSpec, IsForSaleSpec);
    }
}
