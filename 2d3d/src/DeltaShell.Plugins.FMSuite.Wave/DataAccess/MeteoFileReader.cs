using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Reader for meteo files (e.g. *.wnd).
    /// </summary>
    /// <seealso cref="NGHSFileBase"/>
    /// .
    public class MeteoFileReader : NGHSFileBase
    {
        /// <summary>
        /// Reads the properties from the meteo file.
        /// </summary>
        /// <param name="filePath">The path to the file to read.</param>
        /// <returns>A collection of properties read from the meteo file.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="filePath"/> is <c>null</c>.</exception>
        public IEnumerable<MeteoFileProperty> Read(string filePath)
        {
            Ensure.NotNull(filePath, nameof(filePath));

            OpenInputFile(new FileStream(filePath, FileMode.Open, FileAccess.Read));

            var properties = new List<MeteoFileProperty>();
            try
            {
                string line = GetNextLine();
                while (line != null)
                {
                    string[] values = line.Split(new[]
                    {
                        '='
                    }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();

                    if (values.Length == 2)
                    {
                        properties.Add(new MeteoFileProperty(values[0], values[1]));
                    }

                    line = GetNextLine();
                }
            }
            finally
            {
                CloseInputFile();
            }

            return properties;
        }
    }
}