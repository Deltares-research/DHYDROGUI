using System;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Provides helper methods for repeating activities
    /// </summary>
    public static class Repeat
    {
        /// <summary>
        /// Repeats a specified <paramref name="action"/> a number of times
        /// </summary>
        /// <param name="n">The number of times the action should be repeated.</param>
        /// <param name="action">The action.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="action"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="n"/> is smaller than 0.
        /// </exception>
        public static void Action(int n, Action action)
        {
            Ensure.NotNull(action, nameof(action));
            Ensure.NotNegative(n, nameof(n), "Number of times cannot be a negative integer.");

            for (var i = 0; i < n; i++)
            {
                action();
            }
        }
    }
}