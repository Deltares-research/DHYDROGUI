using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Data;

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

    public class NwrwDryWeatherFlowDefinition : Unique<long>, INwrwFeature
    {
        public NwrwDryWeatherFlowDefinition()
        {
            HourlyPercentageDailyVolume = new double[24];
        }

        public string Name { get; set; }
        public DwfDistributionType DistributionType { get; set; }
        public int DayNumber { get; set; }
        public double DailyVolume { get; set; }
        public double[] HourlyPercentageDailyVolume { get; set; }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null || rrModel.NwrwDryWeatherFlowDefinitions.Contains((this))) return;
            rrModel.NwrwDryWeatherFlowDefinitions.Add(this);
        }
    }
}
