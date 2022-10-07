using System;
using System.IO;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Utils.Extensions;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters
{
    /// <summary>
    /// Converter between an evaporation file name and an <see cref="IOEvaporationMeteoDataSource"/>.
    /// </summary>
    public sealed class EvaporationFileNameConverter
    {
        private const string userDefinedFileExtension = ".evp";
        private const string userDefinedFileName = "default" + userDefinedFileExtension;
        private const string guideLineSewersSystemsFileName = "EVAPOR.PLV";
        private const string longTermAverageFileName = "EVAPOR.GEM";

        /// <summary>
        /// Converts the specified file name to the evaporation meteo data source.
        /// </summary>
        /// <param name="fileName"> The evaporation file name. </param>
        /// <param name="logHandler"> The log handlers to report messages with. </param>
        /// <returns>
        /// A <see cref="IOEvaporationMeteoDataSource"/>:
        /// <list type="bullet">
        /// <item><description><see cref="IOEvaporationMeteoDataSource.UserDefined"/> if the file name ends with .evp or if the file name cannot be converted; </description></item>
        /// <item><description><see cref="IOEvaporationMeteoDataSource.LongTermAverage"/> if the file name equals EVAPOR.GEM; </description></item>
        /// <item><description><see cref="IOEvaporationMeteoDataSource.GuidelineSewerSystems"/> if the file name equals EVAPOR.PLV. </description></item>
        /// </list>
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="fileName"/> or <paramref name="logHandler"/> is <c>null</c>.
        /// </exception>
        /// <remarks>
        /// Reports an error to the <paramref name="logHandler"/> when the <paramref name="fileName"/> cannot be converted.
        /// </remarks>
        public IOEvaporationMeteoDataSource FromFileName(string fileName, ILogHandler logHandler)
        {
            Ensure.NotNull(fileName, nameof(fileName));
            Ensure.NotNull(logHandler, nameof(logHandler));

            if (fileName.EqualsCaseInsensitive(guideLineSewersSystemsFileName))
            {
                return IOEvaporationMeteoDataSource.GuidelineSewerSystems;
            }

            if (fileName.EqualsCaseInsensitive(longTermAverageFileName))
            {
                return IOEvaporationMeteoDataSource.LongTermAverage;
            }

            if (Path.GetExtension(fileName).EqualsCaseInsensitive(userDefinedFileExtension))
            {
                return IOEvaporationMeteoDataSource.UserDefined;
            }

            logHandler.ReportErrorFormat(Resources._0_is_not_a_supported_evaporation_file, fileName);
            return IOEvaporationMeteoDataSource.UserDefined;
        }

        /// <summary>
        /// Converts the provided <paramref name="ioEvaporationMeteoDataSource"/> to a file name.
        /// </summary>
        /// <param name="ioEvaporationMeteoDataSource"> The IO evaporation meteo data source type. </param>
        /// <returns>
        /// A file name:
        /// <list type="bullet">
        /// <item><description> default.evp if meteo data source is user-defined; </description></item>
        /// <item><description> EVAPOR.GEM if meteo data source is long term average; </description></item>
        /// <item><description> EVAPOR.PLV if meteo data source is guideline sewer systems. </description></item>
        /// </list>
        /// </returns>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// Thrown when <paramref name="ioEvaporationMeteoDataSource"/> is not a defined <see cref="IOEvaporationMeteoDataSource"/>
        /// .
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="ioEvaporationMeteoDataSource"/> is not a supported value.
        /// </exception>
        public string ToFileName(IOEvaporationMeteoDataSource ioEvaporationMeteoDataSource)
        {
            Ensure.IsDefined(ioEvaporationMeteoDataSource, nameof(ioEvaporationMeteoDataSource));

            switch (ioEvaporationMeteoDataSource)
            {
                case IOEvaporationMeteoDataSource.UserDefined:
                    return userDefinedFileName;
                case IOEvaporationMeteoDataSource.LongTermAverage:
                    return longTermAverageFileName;
                case IOEvaporationMeteoDataSource.GuidelineSewerSystems:
                    return guideLineSewersSystemsFileName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ioEvaporationMeteoDataSource), ioEvaporationMeteoDataSource, null);
            }
        }
    }
}