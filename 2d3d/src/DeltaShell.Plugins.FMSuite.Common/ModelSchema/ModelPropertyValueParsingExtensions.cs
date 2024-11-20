using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaShell.Plugins.FMSuite.Common.ModelSchema
{
    public static class ModelPropertyValueParsingExtensions
    {
        /// <summary>
        /// Returns the model property values (file names) as strings.
        /// </summary>
        /// <param name="modelProperty"> The Model Property for which the values are returned. </param>
        /// <param name="extension"> Extension of the files. </param>
        /// <param name="separator"> A character that delimits the substrings in the string. </param>
        /// <returns> </returns>
        public static IEnumerable<string> GetFileNames(this ModelProperty modelProperty, string extension,
                                                       char separator)
        {
            string concatenatedNames = modelProperty.GetValueAsString();

            string[] names = concatenatedNames.Split(new[]
            {
                extension
            }, StringSplitOptions.RemoveEmptyEntries);

            return names.Select(n => n.Trim(separator) + extension);
        }
    }
}