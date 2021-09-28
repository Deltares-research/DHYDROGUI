using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Shell.Core;
using DelftTools.Utils.Aop;
using DeltaShell.NGHS.Common.Extensions;
using DeltaShell.NGHS.Utils;
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
                var stationNames = buiFileReader.StationNames;
                if (stationNames.Count == 0) return;

                var distributionType = stationNames.Count == 1 
                               ? MeteoDataDistributionType.Global 
                               : MeteoDataDistributionType.PerStation;

                using (precipitation.InEditMode("import data"))
                {
                    if (precipitation.DataDistributionType != distributionType)
                    {
                        precipitation.DataDistributionType = distributionType;
                    }

                    IFunction data = precipitation.Data;

                    // clear function (MDAs)
                    foreach (IMultiDimensionalArray currentValues in data.Components.Concat(data.Arguments).Select(v => v.Values))
                    {
                        currentValues.DoWithPropertySet(nameof(currentValues.FireEvents), false, ()=> currentValues.Clear());
                    }

                    data.Arguments[0].SetValues(buiFileReader.MeasurementTimes);

                    if (distributionType == MeteoDataDistributionType.PerStation)
                    {
                        data.Arguments[1].SetValues(stationNames);
                    }

                    var values = measurements.SelectMany(m => m.MeasuredValues).ToArray();
                    data.Components[0].Values.DoWithPropertySet(nameof(IMultiDimensionalArray.FireEvents), false, () =>
                    {
                        data.Components[0].SetValues(values);
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error("Precipitation import failed: ", e);
            }
        }
    }
}