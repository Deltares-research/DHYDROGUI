using System;

namespace DeltaShell.NGHS.Common.Extensions
{
    /// <summary>
    /// Contains extensions methods for <see cref="T:System.String"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether two specified <see cref="T:System.String"/> objects have the same value,
        /// ignoring the case of the strings being compared.
        /// </summary>
        /// <param name="a">The first string to compare. </param>
        /// <param name="b">The second string to compare. </param>
        /// <returns>
        /// True if the value of <paramref name="a"/> is equal to the value of <paramref name="b"/>; otherwise, false.
        /// </returns>
        public static bool EqualsCaseInsensitive(this string a, string b)
        {
            return string.Equals(a, b, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}