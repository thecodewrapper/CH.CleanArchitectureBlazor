namespace CH.CleanArchitecture.Common.Helpers
{
    public static class MathHelper
    {
        /// <summary>
        /// Divides a decimal number into two parts given a ratio (ex. 2:3 or 8:7)
        /// </summary>
        /// <param name="ratioFrom"></param>
        /// <param name="ratioTo"></param>
        /// <param name="numberToDivide"></param>
        /// <returns></returns>
        public static (int, int) DivideIntoTwoParts(int ratioFrom, int ratioTo, decimal numberToDivide) {
            int numberFlattened = (int)decimal.Truncate(numberToDivide);
            decimal part1 = (ratioFrom / (ratioFrom + (decimal)ratioTo)) * numberFlattened;
            decimal part2 = (ratioTo / (ratioFrom + (decimal)ratioTo)) * numberFlattened;

            return ((int)decimal.Truncate(part1), (int)decimal.Truncate(part2));
        }
    }
}
