using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Extentions
{
    public static class LocationAliasesExtensions
    {
        /// <summary>
        /// Parse the location alias' <see cref="IHasLocationAliases.LocationAliases"/>
        /// property into a list of aliases that is was comma separated.
        /// Removes empty entries and whitespace entries.
        /// </summary>
        public static List<string> ParseLocationAliases(this IHasLocationAliases location)
        {
            var result = new List<string>();
            char[] stringSplitters =
            {
                ' ',
                '\n',
                '\r',
                '\t'
            };

            string[] aliasList = location.LocationAliases != null
                                     ? location.LocationAliases.Split(new[]
                                     {
                                         ','
                                     }, StringSplitOptions.RemoveEmptyEntries)
                                     : new string[0];

            foreach (string alias in aliasList)
            {
                string trimmed = alias.Trim(stringSplitters);

                // remove whitespace, because delwaq cannot handle whitespace strings.
                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    result.Add(trimmed);
                }
            }

            return result;
        }
    }
}