using System.Linq;
using DelftTools.Utils.Guards;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures.Parsers
{
    /// <summary>
    /// Test helper for the structure parser tests.
    /// </summary>
    public static class StructureParserTestHelper
    {
        /// <summary>
        /// Creates a new <see cref="IniSection"/> for a structure.
        /// </summary>
        /// <param name="lineNumber">Optional: the line number the INI section starts on.</param>
        /// <returns></returns>
        public static IniSection CreateStructureIniSection(int lineNumber = 0)
        {
            return new IniSection("[structure]") { LineNumber = lineNumber };
        }

        /// <summary>
        /// Adds a new key-value property to an existing INI section.
        /// </summary>
        /// <param name="iniSection"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="lineNumber"></param>
        public static void AddProperty(this IniSection iniSection, string key, string value, int lineNumber = 0)
        {
            Ensure.NotNull(iniSection, nameof(iniSection));

            iniSection.AddProperty(new IniProperty(key, value, string.Empty));
            iniSection.Properties.Last().LineNumber = lineNumber;
        }
    }
}