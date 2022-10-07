using System;
using System.IO;
using System.Linq;
using DelftTools.Functions.Filters;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Converters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.DataAccessObjects;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.FileWriters;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Exporters
{
    /// <summary>
    /// Exports an <see cref="EvaporationMeteoData"/> to an evaporation file.
    /// </summary>
    public sealed class EvaporationExporter : IEvaporationExporter
    {
        private readonly EvaporationFileWriter evaporationFileWriter;
        private readonly EvaporationFileCreator evaporationFileCreator;
        private readonly EvaporationFileNameConverter evaporationFileNameConverter;
        private readonly IOEvaporationMeteoDataSourceConverter meteoDataSourceConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaporationExporter"/> class.
        /// </summary>
        /// <param name="evaporationFileWriter"> The evaporation file writer. </param>
        /// <param name="evaporationFileCreator"> The evaporation file creator. </param>
        /// <param name="evaporationFileNameConverter"> The evaporation file name converter. </param>
        /// <param name="meteoDataSourceConverter"> The meteo data source converter. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="evaporationFileWriter"/>, <paramref name="meteoDataSourceConverter"/> or
        /// <paramref name="evaporationFileCreator"/> Thrown when any argument is <c>null</c>.
        /// </exception>
        public EvaporationExporter(EvaporationFileWriter evaporationFileWriter,
                                   EvaporationFileCreator evaporationFileCreator,
                                   EvaporationFileNameConverter evaporationFileNameConverter,
                                   IOEvaporationMeteoDataSourceConverter meteoDataSourceConverter)
        {
            Ensure.NotNull(evaporationFileWriter, nameof(evaporationFileWriter));
            Ensure.NotNull(evaporationFileCreator, nameof(evaporationFileCreator));
            Ensure.NotNull(evaporationFileNameConverter, nameof(evaporationFileNameConverter));
            Ensure.NotNull(meteoDataSourceConverter, nameof(meteoDataSourceConverter));

            this.evaporationFileWriter = evaporationFileWriter;
            this.evaporationFileCreator = evaporationFileCreator;
            this.evaporationFileNameConverter = evaporationFileNameConverter;
            this.meteoDataSourceConverter = meteoDataSourceConverter;
        }

        /// <inheritdoc/>
        /// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
        /// <list type="bullet">
        /// <item>Thrown when <paramref name="evaporationMeteoDataSource"/> is not a defined <see cref="MeteoDataSource"/>.</item>
        /// <item>Thrown when <paramref name="meteoDataSource"/> is not a defined <see cref="IOEvaporationMeteoDataSource"/>.</item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <list type="bullet">
        /// <item>Thrown when <paramref name="evaporationMeteoDataSource"/> is not a supported value.</item>
        /// <item>Thrown when <paramref name="meteoDataSource"/> is not a supported value.</item>
        /// </list>
        /// </exception>
        public void Export(EvaporationMeteoData evaporationMeteoData, DirectoryInfo directory)
        {
            Ensure.NotNull(evaporationMeteoData, nameof(evaporationMeteoData));
            Ensure.NotNull(directory, nameof(directory));

            IOEvaporationMeteoDataSource meteoDataSource = meteoDataSourceConverter.ToIOMeteoDataSource(evaporationMeteoData.SelectedMeteoDataSource);
            string evaporationFileName = evaporationFileNameConverter.ToFileName(meteoDataSource);

            var file = new FileInfo(Path.Combine(directory.FullName, evaporationFileName));

            Export(evaporationMeteoData, file, meteoDataSource);
        }

        /// <inheritdoc/>
        public void Export(EvaporationMeteoData evaporationMeteoData, FileInfo file)
        {
            Ensure.NotNull(evaporationMeteoData, nameof(evaporationMeteoData));
            Ensure.NotNull(file, nameof(file));

            IOEvaporationMeteoDataSource meteoDataSource = meteoDataSourceConverter.ToIOMeteoDataSource(evaporationMeteoData.SelectedMeteoDataSource);

            Export(evaporationMeteoData, file, meteoDataSource);
        }

        private void Export(EvaporationMeteoData evaporationMeteoData, FileInfo file, IOEvaporationMeteoDataSource meteoDataSource)
        {
            IEvaporationFile evaporationFile = evaporationFileCreator.CreateFor(meteoDataSource);

            AddDataToFile(evaporationMeteoData, evaporationFile);

            using (StreamWriter writer = File.CreateText(file.FullName))
            {
                evaporationFileWriter.Write(evaporationFile, writer);
            }
        }

        private static void AddDataToFile(IMeteoData meteoData, IEvaporationFile evaporationFile)
        {
            IVariable<DateTime> timeArgument = meteoData.Data.Arguments.OfType<IVariable<DateTime>>().Single();
            foreach (DateTime time in timeArgument.Values)
            {
                var filter = new VariableValueFilter<DateTime>(timeArgument, time);
                double[] valuesForTime = meteoData.Data.GetValues<double>(filter).ToArray();

                evaporationFile.Add(time, valuesForTime);
            }
        }
    }
}