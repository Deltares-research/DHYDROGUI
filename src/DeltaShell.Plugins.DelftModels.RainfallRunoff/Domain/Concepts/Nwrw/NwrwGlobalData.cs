using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Data;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwGlobalData : Unique<long>, INwrwFeature
    {
        public string Name { get; set; }
        public NwrwSurfaceType SurfaceType { get; set; }
        public double SurfaceStorage { get; set; }
        public double InfiltrationCapacityMax { get; set; }
        public double InfiltrationCapacityMin { get; set; }
        public double InfiltrationCapacityReduction { get; set; }
        public double InfiltrationCapacityRecovery { get; set; }
        public double RunoffDelay { get; set; }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
            if (rrModel == null || rrModel.NwrwGlobalData.Any(ngd => ngd.SurfaceType.Equals(this.SurfaceType))) return;

            rrModel.NwrwGlobalData.Add(this);
        }
    }
}