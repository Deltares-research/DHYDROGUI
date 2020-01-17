using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public enum DwfDistributionType
    {
        [Description("DAG")]
        Daily,

        [Description("VAR")]
        Variable,

        [Description("CST")]
        Constant,
    }

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

        public void SetGeometry(NwrwData nwrwData, IGeometry geometry)
        {
        }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
           var rrModel = model as RainfallRunoffModel;

            if (rrModel == null || rrModel.NwrwDryWeatherFlowDefinitions.Any(dwfd => dwfd.Equals(this)))
            {
                Log.Warn($"Could not add {nameof(NwrwDryWeatherFlowDefinition)} to {nameof(RainfallRunoffModel)}");
                return;
            }

            // The kernel only supports DWF definitions of type 'DAG' or
            // of type 'CST' where VER_DAG is empty.
            if (this.DistributionType == DwfDistributionType.Variable || 
                (this.DistributionType == DwfDistributionType.Constant && this.DayNumber != default(int)))
            {
                Log.Warn($"Could not add {nameof(NwrwDryWeatherFlowDefinition)} to {nameof(RainfallRunoffModel)}. This distribution type is not yet supported.");
                return;
            }
            
            rrModel?.NwrwDryWeatherFlowDefinitions.Add(this);
        }
    }
}
