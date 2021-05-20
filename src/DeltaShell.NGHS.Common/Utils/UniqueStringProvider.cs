using System.Collections.Generic;
using DelftTools.Utils.Guards;

namespace DeltaShell.NGHS.Common.Utils
{
    /// <summary>
    /// This class provides a unique string based on the inputs.
    /// </summary>
    public class UniqueStringProvider
    {
        private readonly IDictionary<string, int> cachedStrings = new Dictionary<string, int>();

        /// <summary>
        /// Returns a unique string based on the specified <paramref name="str"/>.
        /// </summary>
        /// <param name="str"> The string for which to generate a unique string. </param>
        /// <returns>
        /// The <paramref name="str"/> if it is unique, otherwise
        /// the <paramref name="str"/> with an appended index.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="str"/> is <c>null</c>.
        /// </exception>
        public string GetUniqueStringFor(string str)
        {
            Ensure.NotNull(str, nameof(str));

            if (!cachedStrings.ContainsKey(str))
            {
                cachedStrings[str] = 0;
                return str;
            }

            cachedStrings[str]++;

            return str + $" {cachedStrings[str]}";
        }
    }
}