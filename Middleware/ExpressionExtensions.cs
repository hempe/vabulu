using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Vabulu.Middleware {

    /// <summary>
    /// Extensions for <see cref="LambdaExpression"/>.
    /// </summary>
    internal static class ExpressionExtensions {

        /// <summary>
        /// Gets the property access list.
        /// </summary>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns>List of PropertyInfos</returns>
        /// <exception cref="ArgumentException">If the expression does not have property paths</exception>
        public static IReadOnlyList<PropertyInfo> GetPropertyAccessList(this LambdaExpression propertyAccessExpression) {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1, "Assumes expression has only one parameter");

            var propertyPaths = MatchPropertyAccessList(propertyAccessExpression, (p, e) => e.MatchSimplePropertyAccess(p));

            if (propertyPaths == null) {
                throw new ArgumentException(nameof(propertyAccessExpression));
            }

            return propertyPaths;
        }

        /// <summary>
        /// Get all properties that match the property access expression.
        /// </summary>
        /// <param name="lambdaExpression">The lambda expression.</param>
        /// <param name="propertyMatcher">The property matcher.</param>
        /// <returns>List of PropertyInfos</returns>
        private static IReadOnlyList<PropertyInfo> MatchPropertyAccessList(this LambdaExpression lambdaExpression, Func<Expression, Expression, PropertyInfo> propertyMatcher) {
            Debug.Assert(lambdaExpression.Body != null, "Assumes the lambda expression has a body");

            var newExpression = RemoveConvert(lambdaExpression.Body) as NewExpression;

            var parameterExpression = lambdaExpression.Parameters.Single();

            if (newExpression != null) {
                var propertyInfos = newExpression
                    .Arguments
                    .Select(a => propertyMatcher(a, parameterExpression))
                    .Where(p => p != null)
                    .ToList();

                return propertyInfos.Count != newExpression.Arguments.Count ? null : propertyInfos;
            }

            var propertyPath = propertyMatcher(lambdaExpression.Body, parameterExpression);

            return propertyPath != null ? new [] { propertyPath } : null;
        }

        /// <summary>
        /// Remove expression conversion.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>Expression without convert</returns>
        private static Expression RemoveConvert(this Expression expression) {
            while ((expression != null) &&
                ((expression.NodeType == ExpressionType.Convert) ||
                    (expression.NodeType == ExpressionType.ConvertChecked))) {
                expression = RemoveConvert(((UnaryExpression) expression).Operand);
            }

            return expression;
        }

        /// <summary>
        /// Simple property matcher 
        /// </summary>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns>The PropertyInfo</returns>
        private static PropertyInfo MatchSimplePropertyAccess(this Expression parameterExpression, Expression propertyAccessExpression) {
            var propertyInfos = MatchPropertyAccess(parameterExpression, propertyAccessExpression);

            return (propertyInfos != null) && (propertyInfos.Count == 1) ? propertyInfos[0] : null;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccess(
            this Expression parameterExpression, Expression propertyAccessExpression) {
            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do {
                memberExpression = RemoveConvert(propertyAccessExpression) as MemberExpression;

                var propertyInfo = memberExpression?.Member as PropertyInfo;

                if (propertyInfo == null) {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (memberExpression.Expression.RemoveConvert() != parameterExpression);

            return propertyInfos;
        }
    }
}