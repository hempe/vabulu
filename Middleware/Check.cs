using System;
using System.Linq;
using System.Linq.Expressions;

namespace Vabulu.Middleware {
    /// <summary>
    /// Helper class for property selecting expressions
    /// </summary>
    internal static class Check {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <typeparam name="T">The type of the original class.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="expression">The expression for selecting the property.</param>
        /// <returns>The name of the property.</returns>
        public static string GetPropertyName<T, TProperty>(Expression<Func<T, TProperty>> expression) {
            return NotNull(expression, nameof(expression)).GetPropertyAccessList().Single().Name;
        }

        private static T NotNull<T>(T value, string parameterName) {
            if (ReferenceEquals(value, null)) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        private static string NotEmpty(string value, string parameterName) {
            Exception e = null;
            if (ReferenceEquals(value, null)) {
                e = new ArgumentNullException(parameterName);
            } else if (value.Trim().Length == 0) {
                e = new ArgumentException(nameof(parameterName));
            }

            if (e != null) {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }
    }
}