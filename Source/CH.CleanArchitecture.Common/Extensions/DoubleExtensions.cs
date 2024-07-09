namespace CH.CleanArchitecture.Common
{
    public static class DoubleExtensions
    {
        public static double ToMegabytes(this long bytes) {
            return (bytes / 1024f) / 1024f;
        }

        public static double ToMegabytes(this int bytes) {
            return (bytes / 1024f) / 1024f;
        }
    }
}
