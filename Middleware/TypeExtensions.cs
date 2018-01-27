using System;
using System.Linq;
using Vabulu.Attributes;

namespace Vabulu.Middleware {
    internal static class TypeExtensions {

        public static TableAttribute Table(this Type type) => type.GetCustomAttribute<TableAttribute>();
        public static ViewAttribute View(this Type type) => type.GetCustomAttribute<ViewAttribute>() ?? type.GetCustomAttribute<TableAttribute>();
        public static bool IsTable(this Type type) => type.GetCustomAttributes(typeof(TableAttribute), false).Any();
        public static bool IsView(this Type type) => type.GetCustomAttributes(typeof(ViewAttribute), false).Any() || type.GetCustomAttributes(typeof(TableAttribute), false).Any();

        public static bool IsSimple(this Type type) {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                return IsSimple(type.GetGenericArguments() [0]);
            }
            return type.IsPrimitive ||
                type.IsEnum ||
                type.Equals(typeof(string)) ||
                type.Equals(typeof(decimal));
        }
    }
}