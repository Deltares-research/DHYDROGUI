using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Utils.Data;
using GeoAPI.Geometries;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw
{
    /// <summary>
    /// Object for storing nwrw definitions from nwrw.csv.
    /// </summary>
    /// <seealso cref="INwrwFeature" />
    public class NwrwDefinition : Unique<long>, INwrwFeature
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NwrwDefinition));

        public string Name { get; set; } // AFV_IDE
        public NwrwSurfaceType SurfaceType { get; set; } // AFV_IDE
        public double SurfaceStorage { get; set; } // AFV_BRG
        public double InfiltrationCapacityMax { get; set; } // AFV_IFX
        public double InfiltrationCapacityMin { get; set; } // AFV_IFN
        public double InfiltrationCapacityReduction { get; set; } // AFV_IFH
        public double InfiltrationCapacityRecovery { get; set; } // AFV_AFS
        public double RunoffDelay { get; set; } // AFV_AFS
        public double RunoffLength { get; set; } // AFV_LEN
        public double RunoffSlope { get; set; } // AFV_HEL
        public string Remark { get; set; } // ALG_TOE

        public void SetGeometry(NwrwData nwrwData, IGeometry geometry)
        {
        }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model)
        {
            var rrModel = model as RainfallRunoffModel;

            if (rrModel == null || rrModel.NwrwDefinitions.Any(nd => nd.SurfaceType.Equals(this.SurfaceType)))
            {
                Log.Warn($"Could not add {nameof(NwrwDefinition)} to {nameof(RainfallRunoffModel)}");
                return;
            }
            rrModel?.NwrwDefinitions.Add(this);
        }
    }
}