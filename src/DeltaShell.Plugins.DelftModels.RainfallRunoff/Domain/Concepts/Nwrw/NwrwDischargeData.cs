using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Data;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public enum DischargeType
    {
        [Description("VWD")]
        DryWeatherFlow,

        [Description("LAT")]
        Lateral
    }

    public class NwrwDischargeData : Unique<long>, INwrwFeature, IUrbanRrDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwDischargeData));
        public string Name { get; set; }
        public string Remark { get; set; }
        public DischargeType DischargeType { get; set; }
        public string DischargeId { get; set; }
        public double PollutingUnits { get; set; }
        public double Surface { get; set; }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;

            var nwrwRrData = rrModel?.UrbanRrData.OfType<NwrwRrData>().FirstOrDefault();
            if (nwrwRrData == null)
            {
                Log.Warn($"Could not add {nameof(NwrwDischargeData)} to {nameof(RainfallRunoffModel)}");
                return;
            }
            nwrwRrData?.UrbanRrDischargeDefinitions.Add(this);
        }
    }
}
