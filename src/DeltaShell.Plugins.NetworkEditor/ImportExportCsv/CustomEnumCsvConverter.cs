using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections;

namespace DeltaShell.Plugins.NetworkEditor.ImportExportCsv
{
    /// <summary>
    /// class that support the conversion of string to enum and vice versa
    /// pros: gives descriptive error message
    ///       enabled different string values from enum values in external files
    ///         and allows simple change of these string values.
    /// cons : current implementation of Fromstring prossibly slow
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomEnumCsvConverter<T>
    {
        protected static string ToString(IDictionary<T, string> conversion, T source)
        {
            return conversion[source];
        }

        protected static T Fromstring(IDictionary<T, string> conversion, string source)
        {
            try
            {
                return conversion.Where(kv => kv.Value == source).Select(kv => kv.Key).First();
            }
            catch (Exception)
            {
                // in case of error enum all values that are allowed
                var validDescriptions = "";
                conversion.Values.ForEach(v => validDescriptions += "'" + v.ToString() + "', ");
                throw new ArgumentException(string.Format("Unknown {0} '{1}'; valid values: are {2}: ",
                                                          typeof(T).ToString().Split(new []{'.'}).Last(), source, validDescriptions));
            }
        }
    }
}