using System;
using Xunit;

namespace RemoveDuplicates.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void EmptyArrayZeroResult()
        {
            var input = new int[] { };
            var expected = 0;
            
            var result = new Solution.Solution().RemoveDuplicates(input);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void OneElementArrayOneResult()
        {
            var input = new int[] { 15 };
            var expected = 1;

            var result = new Solution.Solution().RemoveDuplicates(input);
            Assert.Equal(expected, result);
        }
    }
}
