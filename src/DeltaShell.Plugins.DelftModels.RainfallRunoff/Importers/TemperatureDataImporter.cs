using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Functions.Filters;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers
{
    public class TemperatureDataImporter : IFileImporter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TemperatureDataImporter));

        public string Name
        {
            get { return "Temperatures Data from TMP Files"; }
        }

        public string Category { get; private set; }
        public string Description { get{ return Name; } }

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
            get { return "SOBEK TMP files (*.tmp)|*.tmp"; }
        }

        public bool OpenViewAfterImport { get { return false; } }

        public string TargetDataDirectory { get; set; }

        public bool ShouldCancel { get; set; }

        public ImportProgressChangedDelegate ProgressChanged { get; set; }

        public object ImportItem(string path, object target)
        {
            var temperatures = target as MeteoData;
            if (temperatures == null)
            {
                Log.Error("Temperature data: importing on invalid item.");

                return target;
            }

            var buiFileReader = new SobekRRBuiFileReader();

            if (!buiFileReader.ReadBuiHeaderData(path))
            {
                Log.Error("Temperature import failed, could not read header data from BUI file.");

                return target;
            }

            var measurements = buiFileReader.ReadMeasurementData(path);

            SetTemperatureValues(buiFileReader, temperatures, measurements);

            return target;
        }

        [InvokeRequired]
        private static void SetTemperatureValues(SobekRRBuiFileReader buiFileReader, MeteoData temperatures,
                                                 IEnumerable<MeteoStationsMeasurement> measurements)
        {
            try
            {
                if (buiFileReader.StationNames.Count > 1)
                {
                    temperatures.BeginEdit("import data");
                    temperatures.DataDistributionType = MeteoDataDistributionType.PerStation;
                    temperatures.Data.Clear();
                    temperatures.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    temperatures.Data.Arguments[1].SetValues(buiFileReader.StationNames);

                    foreach (var measurement in measurements)
                    {
                        temperatures.Data.SetValues(measurement.MeasuredValues,
                                                    new VariableValueFilter<DateTime>(temperatures.Data.Arguments[0],
                                                                                      measurement.TimeOfMeasurement));
                    }
                    temperatures.EndEdit();
                }
                else if (buiFileReader.StationNames.Count == 1)
                {
                    temperatures.BeginEdit("import data");
                    temperatures.DataDistributionType = MeteoDataDistributionType.Global;
                    temperatures.Data.Clear();
                    temperatures.Data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);
                    foreach (var measurement in measurements)
                    {
                        temperatures.Data.SetValues(measurement.MeasuredValues,
                                                    new VariableValueFilter<DateTime>(temperatures.Data.Arguments[0],
                                                                                      measurement.TimeOfMeasurement));
                    }
                    temperatures.EndEdit();
                }
            }
            catch (Exception e)
            {
                Log.Error("Temperature import failed: ", e);
            }
        }
    }
}
