using DelftTools.Utils.Guards;
using NUnit.Framework.Constraints;

namespace DeltaShell.NGHS.TestUtils.AssertConstraints
{
    /// <summary>
    /// Contains extension methods for <see cref="ConstraintExpression"/>.
    /// </summary>
    public static class ConstraintExpressionExtensions
    {
        /// <summary>
        /// Append the <see cref="ExistConstraint"/> to the specified <paramref name="expression"/>.
        /// </summary>
        /// <param name="expression">The constraint expression.</param>
        /// <returns>The exist constraint.</returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="expression"/> is <c>null</c>.
        /// </exception>
        public static ExistConstraint Exist(this ConstraintExpression expression)
        {
            Ensure.NotNull(expression, nameof(expression));

            var constraint = new ExistConstraint();
            expression.Append(constraint);

            return constraint;
        }
    }
}