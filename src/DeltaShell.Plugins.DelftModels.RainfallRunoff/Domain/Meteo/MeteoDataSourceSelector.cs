using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Functions;
using DelftTools.Units;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.FileWriter;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo
{
    /// <summary>
    /// Selector to get correct meteo data time series based on active meteo data source.
    /// </summary>
    public class MeteoDataSourceSelector : IMeteoDataSourceSelector
    {
        private const string guideLineSewersSystemsFileName = "EVAPOR.PLV";
        private const string longTermAverageFileName = "EVAPOR.GEM";

        private readonly IManifestRetriever manifestRetriever;
        private readonly MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator;
        private readonly SobekRREvaporationReader evaporationReader = new SobekRREvaporationReader();
        private IFunction longTermAverageTimeSeries;
        private IFunction guideLineSewersSystemsTimeSeries;

        private readonly Unit unit = new Unit(RainfallRunoffModelDataSet.EvaporationName, "mm");

        /// <summary>
        /// Initializes a new instance of the <see cref="MeteoDataSourceSelector"/> class.
        /// </summary>
        /// <param name="manifestRetriever"> The manifest retriever. </param>
        /// <param name="meteoTimeSeriesInstanceCreator"> The meteo time series instance creator. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="manifestRetriever"/> or <paramref name="meteoTimeSeriesInstanceCreator"/> is <c>null</c>.
        /// </exception>
        public MeteoDataSourceSelector(IManifestRetriever manifestRetriever,
                                       MeteoTimeSeriesInstanceCreator meteoTimeSeriesInstanceCreator)
        {
            Ensure.NotNull(manifestRetriever, nameof(manifestRetriever));
            Ensure.NotNull(meteoTimeSeriesInstanceCreator, nameof(meteoTimeSeriesInstanceCreator));

            this.manifestRetriever = manifestRetriever;
            this.meteoTimeSeriesInstanceCreator = meteoTimeSeriesInstanceCreator;
        }
        
        /// <inheritdoc />
        public IFunction GetMeteoTimeSeries(MeteoDataSource meteoDataSource, DirectoryInfo modelDirectory)
        {
            Ensure.IsDefined(meteoDataSource, nameof(meteoDataSource));
            Ensure.NotNull(modelDirectory, nameof(modelDirectory));

            switch (meteoDataSource)
            {
                case MeteoDataSource.GuidelineSewerSystems:
                    return GetGuideLineSewersSystemsTimeSeries(modelDirectory);
                case MeteoDataSource.LongTermAverage:
                    return GetLongTermAverageTimesSeries(modelDirectory);
                case MeteoDataSource.UserDefined:
                    return meteoTimeSeriesInstanceCreator.CreateGlobalTimeSeries(unit);
                default:
                    throw new ArgumentOutOfRangeException(nameof(meteoDataSource), meteoDataSource, null);
            }
        }

        private IFunction GetGuideLineSewersSystemsTimeSeries(DirectoryInfo modelDirectory)
        {
            if (guideLineSewersSystemsTimeSeries == null)
            {
                guideLineSewersSystemsTimeSeries = GetTimeSeries(guideLineSewersSystemsFileName, modelDirectory);
            }

            return guideLineSewersSystemsTimeSeries;
        }

        private IFunction GetLongTermAverageTimesSeries(DirectoryInfo modelDirectory)
        {
            if (longTermAverageTimeSeries == null)
            {
                longTermAverageTimeSeries = GetTimeSeries(longTermAverageFileName, modelDirectory);
            }

            return longTermAverageTimeSeries;
        }

        /// <summary>
        /// Gets the time series from the evaporation file.
        /// </summary>
        /// <param name="evaporationFileName"> The evaporation file name. </param>
        /// <param name="modelDirectory"> The model directory. </param>
        /// <returns>
        /// The time series data from the evaporation file in the model directory if this file exists;
        /// otherwise, the time series data from the evaporation file from the manifest.
        /// </returns>
        private IFunction GetTimeSeries(string evaporationFileName, DirectoryInfo modelDirectory)
        {
            string modelFilePath = Path.Combine(modelDirectory.FullName, evaporationFileName);

            SobekRREvaporation evaporation = File.Exists(modelFilePath)
                                                 ? ReadModelEvaporation(modelFilePath)
                                                 : ReadManifestEvaporation(evaporationFileName);

            TimeSeries timeSeriesData = meteoTimeSeriesInstanceCreator.CreateGlobalTimeSeries(unit);
            foreach (KeyValuePair<DateTime, double[]> data in evaporation.Data)
            {
                DateTime dateTime = data.Key;
                double value = data.Value[0];
                timeSeriesData[dateTime] = value;
            }

            return timeSeriesData;
        }

        private SobekRREvaporation ReadModelEvaporation(string evaporationFilePath)
        {
            using (var stream = new FileStream(evaporationFilePath, FileMode.Open))
            {
                return evaporationReader.Read(stream);
            }
        }

        private SobekRREvaporation ReadManifestEvaporation(string sourceLocation)
        {
            using (Stream stream = manifestRetriever.GetFixedStream(sourceLocation))
            {
                return evaporationReader.Read(stream);
            }
        }
    }
}