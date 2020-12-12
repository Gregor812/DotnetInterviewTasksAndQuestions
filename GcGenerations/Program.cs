using System;
using System.Collections.Generic;

namespace GcGenerations
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GC.GetGeneration(new object[50]));
            Console.WriteLine(GC.GetGeneration(new object[50000]));
            Console.WriteLine(GC.GetGeneration(new List<object>(50)));
            Console.WriteLine(GC.GetGeneration(new List<object>(50000)));
        }
    }
}
