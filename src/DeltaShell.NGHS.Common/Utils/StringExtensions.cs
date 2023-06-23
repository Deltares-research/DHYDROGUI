using System;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// Contains extensions methods for <see cref="T:System.String"/>.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether <paramref name="source"/> contains <paramref name="value"/>,
        /// ignoring the case of the strings.
        /// </summary>
        /// <param name="source"> The source string. </param>
        /// <param name="value"> The string to search for. </param>
        /// <returns> Whether or not the string contains <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public static bool ContainsCaseInsensitive(this string source, string value)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(value, nameof(value));

            return source.IndexOf(value, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}