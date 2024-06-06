using System.Globalization;

namespace CH.CleanArchitecture.Common.Helpers
{
    public static class CurrencyHelper
    {
        public static string ToEuroAmountFormatted(int amountInCents) {
            decimal euroAmount = ToEuroAmount(amountInCents);
            return euroAmount.ToString("0.00 €", CultureInfo.InvariantCulture);
        }

        public static string ToEuroAmountFormatted(decimal euroAmount) {
            return euroAmount.ToString("0.00 €", CultureInfo.InvariantCulture);
        }

        public static decimal ToEuroAmount(int amountInCents) {
            decimal euroAmount = amountInCents / 100m; // Convert to euros
            return euroAmount;
        }
    }
}
