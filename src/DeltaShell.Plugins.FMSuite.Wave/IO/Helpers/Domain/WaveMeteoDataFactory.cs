using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Common.Wind;

namespace DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Domain
{
    /// <summary>
    /// Helper factory for creating <see cref="WaveDomainData"/>.
    /// </summary>
    public static class WaveMeteoDataFactory
    {
        private const string quantityKey = "quantity1";
        private const string xWind = "x_wind";
        private const string yWind = "y_wind";

        /// <summary>
        /// Creates the spider web meteo data.
        /// </summary>
        /// <param name="spwFile">The spiderweb file.</param>
        /// <returns>The created <see cref="WaveMeteoData"/></returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="spwFile"/> is <c>null</c> or empty.
        /// </exception>
        public static WaveMeteoData CreateSpiderWebMeteoData(string spwFile)
        {
            Ensure.NotNullOrEmpty(spwFile, nameof(spwFile));

            return new WaveMeteoData
            {
                FileType = WindDefinitionType.SpiderWebGrid,
                HasSpiderWeb = true,
                SpiderWebFilePath = spwFile
            };
        }

        /// <summary>
        /// Creates the xy component meteo data.
        /// </summary>
        /// <param name="xFile">The file for the x component.</param>
        /// <param name="yFile">The file for the y component.</param>
        /// <param name="spwFile">The optional spider web file.</param>
        /// <returns>The created <see cref="WaveMeteoData"/></returns>
        /// ///
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="xFile"/> or <paramref name="yFile"/> is <c>null</c> or empty.
        /// </exception>
        public static WaveMeteoData CreateXYComponentMeteoData(string xFile, string yFile, string spwFile = null)
        {
            Ensure.NotNullOrEmpty(xFile, nameof(xFile));
            Ensure.NotNullOrEmpty(yFile, nameof(yFile));

            return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXWindY,
                HasSpiderWeb = spwFile != null,
                SpiderWebFilePath = spwFile,
                XComponentFilePath = xFile,
                YComponentFilePath = yFile
            };
        }

        /// <summary>
        /// Creates the xy component meteo data for wnd files. Determines the wind quantity (x/y) by reading the property
        /// 'quantity1' from the file.
        /// </summary>
        /// <param name="file1">The first wnd file.</param>
        /// <param name="file2">The second wnd file.</param>
        /// <param name="spwFile">The optional spider web file.</param>
        /// <returns>
        /// The created <see cref="WaveMeteoData"/> if the data is valid and supported; otherwise, <c>null</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="file1"/> or <paramref name="file2"/> is <c>null</c> or empty.
        /// </exception>
        /// <exception cref="FileNotFoundException">
        /// Thrown when <paramref name="file1"/> or <paramref name="file2"/> does not exist.
        /// </exception>
        /// <remarks>
        /// The supported quantities are "x_wind" and "y_wind", and both need to be present in either one of the specified files.
        /// </remarks>
        public static WaveMeteoData CreateWndXYComponentMeteoData(string file1, string file2, string spwFile = null)
        {
            Ensure.NotNullOrEmpty(file1, nameof(file1));
            Ensure.NotNullOrEmpty(file2, nameof(file2));

            if (!File.Exists(file1))
            {
                throw new FileNotFoundException("Meteo file does not exist.", file1);
            }

            if (!File.Exists(file2))
            {
                throw new FileNotFoundException("Meteo file does not exist.", file2);
            }

            string quantity1 = GetQuantity(file1);
            string quantity2 = GetQuantity(file2);

            if (quantity1 == quantity2)
            {
                return null;
            }

            if (!IsSupported(quantity1))
            {
                return null;
            }

            if (!IsSupported(quantity2))
            {
                return null;
            }

            var mapping = new Dictionary<string, string>
            {
                [quantity1] = file1,
                [quantity2] = file2
            };

            return CreateXYComponentMeteoData(mapping[xWind], mapping[yWind], spwFile);
        }

        /// <summary>
        /// Creates the vector meteo data.
        /// </summary>
        /// <param name="xyFile">The xy file.</param>
        /// <param name="spwFile">The optional spw file.</param>
        /// <returns>The created <see cref="WaveMeteoData"/></returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="spwFile"/> is <c>null</c> or empty.
        /// </exception>
        public static WaveMeteoData CreateVectorMeteoData(string xyFile, string spwFile = null)
        {
            Ensure.NotNullOrEmpty(xyFile, nameof(xyFile));

            return new WaveMeteoData
            {
                FileType = WindDefinitionType.WindXY,
                HasSpiderWeb = spwFile != null,
                SpiderWebFilePath = spwFile,
                XYVectorFilePath = xyFile,
            };
        }

        private static bool IsSupported(string quantity) => quantity == xWind || quantity == yWind;

        private static string GetQuantity(string file)
        {
            using (var sr = new StreamReader(new FileStream(file, FileMode.Open)))
            {
                string line;

                while ((line = sr.ReadLine())?.Trim() != null)
                {
                    if (line.StartsWith(quantityKey, StringComparison.Ordinal))
                    {
                        return line.Split('=').ElementAt(1).Trim();
                    }
                }

                return null;
            }
        }
    }
}