using System;

namespace CH.CleanArchitecture.Common
{
    public static class GuidExtensions
    {
        public static bool IsEmpty(this Guid guid) {
            return guid == Guid.Empty;
        }
    }
}
