using System;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.NGHS.Utils.Extensions
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

        /// <summary>
        /// Splits the specified string on empty spaces and removes the empty strings.
        /// </summary>
        /// <param name="value"> The original string to be split. </param>
        /// <returns> An array of the split parts. </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is <c>null</c>.
        /// </exception>
        public static string[] SplitOnEmptySpace(this string value)
        {
            Ensure.NotNull(value, nameof(value));

            return value.Split(Array.Empty<char>(), StringSplitOptions.RemoveEmptyEntries);
        }

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

        /// <summary>
        /// Gets the last substring between two characters in a string.
        /// </summary>
        /// <param name="source"> The source string. </param>
        /// <param name="start"> The first enclosing character. </param>
        /// <param name="end"> The last enclosing character. </param>
        /// <returns>
        /// The last substring between the two given characters if found; otherwise an empty string. 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="source"/> is <c>null</c>.
        /// </exception>
        public static string LastStringBetween(this string source, char start, char end)
        {
            Ensure.NotNull(source, nameof(source));

            int startIndex = source.LastIndexOf(start);
            int endIndex = source.LastIndexOf(end);

            if (startIndex == -1 || endIndex == -1 || startIndex > endIndex)
            {
                return string.Empty;
            }

            return source.Substring(startIndex + 1, endIndex - startIndex - 1);
        }
    }
}