using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    public class NwrwGlobalData : Unique<long>, INwrwFeature, IUrbanRrDefinition
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwGlobalData));
        public string Name { get; set; }
        public string Remark { get; set; }
        public NwrwSurfaceType SurfaceType { get; set; }
        public double SurfaceStorage { get; set; }
        public double InfiltrationCapacityMax { get; set; }
        public double InfiltrationCapacityMin { get; set; }
        public double InfiltrationCapacityReduction { get; set; }
        public double InfiltrationCapacityRecovery { get; set; }
        public double RunoffDelay { get; set; }

        public void SetGeometry(IGeometry geometry)
        {
            
        }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;
          
            var nwrwRrData = rrModel?.UrbanRrData.OfType<NwrwRrData>().FirstOrDefault();
            if (nwrwRrData == null)
            {
                Log.Warn($"Could not add {nameof(NwrwGlobalData)} to {nameof(RainfallRunoffModel)}");
                return; //could not add
            }

            nwrwRrData.UrbanRrGlobalDefinitions.Add(this);
        }
    }
}