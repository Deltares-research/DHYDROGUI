using DelftTools.Hydro;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;
using log4net;
using System;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Object for storing dry weather flow definitions from verloop.csv.
    /// </summary>
    /// <seealso cref="INwrwFeature" />
    public class NwrwDryWeatherFlowDefinition : Unique<long>, INwrwFeature
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwDryWeatherFlowDefinition));

        public string Name { get; set; } //VER_IDE
        public string DryWeatherFlowId { get; set; } // VER_IDE
        public DwfDistributionType DistributionType { get; set; } // VER_TYPE
        public int DayNumber { get; set; } // VER_DAG
        public double DailyVolume { get; set; } // VER_VOL
        public double[] HourlyPercentageDailyVolume { get; set; } = new double[24]; // U00_DAG -- U23_DAG
        public string Remark { get; set; } // ALG_TOE

        public IGeometry Geometry { get; set; }


        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
           var rrModel = model as RainfallRunoffModel;

            if (rrModel == null || rrModel.NwrwDryWeatherFlowDefinitions.Contains(this))
            {
                Log.Warn($"Could not add {Name} DWF definition to {nameof(RainfallRunoffModel)}.");
                return;
            }

            // The kernel only supports DWF definitions of type 'DAG' or
            // of type 'CST' where VER_DAG is empty.
            if (DistributionType == DwfDistributionType.Variable)
            {
                Log.Warn($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported.");
                return;
            }

            if (DistributionType == DwfDistributionType.Constant && DayNumber != default(int))
            {
                Log.Warn($"Could not add '{Name}' DWF definition to {nameof(RainfallRunoffModel)}. The given distribution type '{DistributionType}' is not yet supported in combination with a value of '{DayNumber}' for VER_DAG.");
                return;
            }

            rrModel?.NwrwDryWeatherFlowDefinitions.Add(this);
        }

        public static NwrwDryWeatherFlowDefinition CreateDefaultDWADefinition()
        {
            return new NwrwDryWeatherFlowDefinition
            {
                Name = "Default_DWA",
                DryWeatherFlowId = "Default_DWA",
                DistributionType = DwfDistributionType.Constant,
                DailyVolume = 12
            };
        }
    }
}
