using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Test helper for the structure parser tests.
    /// </summary>
    public static class StructureParserTestHelper
    {
        /// <summary>
        /// Creates a new <see cref="IDelftIniCategory"/> for a structure.
        /// </summary>
        /// <param name="lineNumber">Optional: the line number the category starts on.</param>
        /// <returns></returns>
        public static IDelftIniCategory CreateStructureCategory(int lineNumber = 0)
        {
            return new DelftIniCategory("[structure]") { LineNumber = lineNumber };
        }

        /// <summary>
        /// Adds a new key-value property to an existing category.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="lineNumber"></param>
        public static void AddProperty(this IDelftIniCategory category, string key, string value, int lineNumber = 0)
        {
            Ensure.NotNull(category, nameof(category));

            category.Properties.Add(new DelftIniProperty(key, value, string.Empty));
            category.Properties.Last().LineNumber = lineNumber;
        }
    }
}