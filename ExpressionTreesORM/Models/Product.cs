﻿using System;
using System.Linq.Expressions;

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
        private static readonly Expression<Func<Product, bool>>
            IsAvailableExpression = x => x.IsForSale && x.InStock > 0;

        public bool IsAvailable => CompiledExpressions<Product, bool>.AsFunc(IsAvailableExpression)(this);
    }
}