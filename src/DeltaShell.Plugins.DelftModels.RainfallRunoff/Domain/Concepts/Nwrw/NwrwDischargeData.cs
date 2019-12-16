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
    public enum DischargeType
    {
        [Description("VWD")]
        DryWeatherFlow,

        [Description("LAT")]
        Lateral
    }

    public class NwrwDischargeData : Unique<long>, INwrwFeature
    {
        public string Name { get; set; }
        public DischargeType DischargeType { get; set; }
        public string DischargeId { get; set; }
        public double PollutingUnits { get; set; }
        public double Surface { get; set; }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null || rrModel.NwrwDischargeData.Any(dd => dd.Equals(this))) return;

            rrModel.NwrwDischargeData.Add(this);
        }
    }
}
