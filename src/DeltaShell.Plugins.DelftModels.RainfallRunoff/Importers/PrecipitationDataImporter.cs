using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class PrecipitationDataImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PrecipitationDataImporter));

        public string Name
        {
            get { return "Precipitation Data from BUI Files"; }
        }

        public string Category { get; private set; }
        public string Description
        {
            get { return string.Empty; }
        }

        public Bitmap Image { get; private set; }

        public IEnumerable<Type> SupportedItemTypes
        {
            get { yield return typeof(MeteoData); }
        }

        public bool CanImportOn(object targetObject)
        {
            return true;
        }

        public bool CanImportOnRootLevel
        {
            get { return false; }
        }

        public string FileFilter
        {
            get { return "SOBEK BUI Files (*.bui)|*.bui"; }
        }

        public bool OpenViewAfterImport { get { return false; } }

        public string TargetDataDirectory { get; set; }
        
        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            var precipitation = target as MeteoData;
            if (precipitation == null)
            {
                Log.Error("Precipitation data: importing on invalid item.");

                return target;
            }

            var buiFileReader = new SobekRRBuiFileReader();

            if (!buiFileReader.ReadBuiHeaderData(path))
            {
                Log.Error("Precipitation import failed, could not read header data from BUI file.");

                return target;
            }

            var measurements = buiFileReader.ReadMeasurementData(path);

            SetPrecipitationValues(buiFileReader, precipitation, measurements);

            return target;
        }

        [InvokeRequired]
        private static void SetPrecipitationValues(SobekRRBuiFileReader buiFileReader, MeteoData precipitation,
                                                   IEnumerable<MeteoStationsMeasurement> measurements)
        {
            try
            {
                if (buiFileReader.StationNames.Count > 1)
                {
                    precipitation.BeginEdit(new DefaultEditAction("import data"));
                    precipitation.DataDistributionType = MeteoDataDistributionType.PerStation;
                    precipitation.Data.Clear();
                    precipitation.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    precipitation.Data.Arguments[1].SetValues(buiFileReader.StationNames);

                    foreach (var measurement in measurements)
                    {
                        precipitation.Data.SetValues(measurement.MeasuredValues,
                                                     new VariableValueFilter<DateTime>(precipitation.Data.Arguments[0],
                                                                                       measurement.TimeOfMeasurement));
                    }
                    precipitation.EndEdit();
                }
                else if (buiFileReader.StationNames.Count == 1)
                {
                    precipitation.BeginEdit(new DefaultEditAction("import data"));
                    precipitation.DataDistributionType = MeteoDataDistributionType.Global;
                    precipitation.Data.Clear();
                    precipitation.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    foreach (var measurement in measurements)
                    {
                        precipitation.Data.SetValues(measurement.MeasuredValues,
                                                     new VariableValueFilter<DateTime>(precipitation.Data.Arguments[0],
                                                                                       measurement.TimeOfMeasurement));
                    }
                    precipitation.EndEdit();
                }
            }
            catch (Exception e)
            {
                Log.Error("Precipitation import failed: ", e);
            }
        }
    }
}