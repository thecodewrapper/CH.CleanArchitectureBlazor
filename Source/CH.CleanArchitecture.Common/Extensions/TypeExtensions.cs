using System;

namespace CH.CleanArchitecture.Common
{
    public static class TypeExtensions
    {
        public static bool IsSubclassOfGeneric(this Type type, Type genericBaseType) {
            while (type != null && type != typeof(object)) {
                var cur = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
                if (cur == genericBaseType)
                    return true;
                type = type.BaseType;
            }
            return false;
        }
    }
}
