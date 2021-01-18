namespace RemoveDuplicates.Solution
{
    public class Solution
    {
        public int RemoveDuplicates(int[] values)
        {
            if (values.Length < 2)
            {
                return values.Length;
            }

            var checkedValue = values[0];
            var currentPosition = 1;
            var distinctNumbers = 1;

            while (currentPosition < values.Length)
            {
                if (values[currentPosition] != checkedValue)
                {
                    checkedValue = values[currentPosition];
                    ++currentPosition;
                    ++distinctNumbers;
                }
                else
                {
                    while (++currentPosition < values.Length && values[currentPosition] == checkedValue)
                    { }

                    if (currentPosition < values.Length)
                    {
                        checkedValue = values[currentPosition];
                        values[distinctNumbers] = checkedValue;
                        ++distinctNumbers;
                    }
                }
            }

            return distinctNumbers;
        }
    }
}
