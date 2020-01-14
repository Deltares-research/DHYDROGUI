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

    public class NwrwDryWeatherFlowDefinition : Unique<long>, INwrwFeature, IUrbanRrDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwDryWeatherFlowDefinition));
        public NwrwDryWeatherFlowDefinition()
        {
            HourlyPercentageDailyVolume = new double[24];
        }

        public string Name { get; set; }
        public string Remark { get; set; }
        public DwfDistributionType DistributionType { get; set; }
        public int DayNumber { get; set; }
        public double DailyVolume { get; set; }
        public double[] HourlyPercentageDailyVolume { get; set; }

        public void SetGeometry(IGeometry geometry)
        {
            
        }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;

            var nwrwRrData = rrModel?.UrbanRrData.OfType<NwrwRrData>().FirstOrDefault();
            if (nwrwRrData == null)
            {
                Log.Warn($"Could not add {nameof(NwrwDryWeatherFlowDefinition)} to {nameof(RainfallRunoffModel)}");
                return;
            }
            nwrwRrData?.UrbanRrFlowDefinitions.Add(this);
        }
    }
}
