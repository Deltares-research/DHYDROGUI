using System.Collections.Concurrent;
using System.Linq;
using DelftTools.Hydro;
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
        public double TerrainRoughness { get; set; } // AFV_RUW
        public string Remark { get; set; } // ALG_TOE

 
        public IGeometry Geometry { get; set; }

        public void AddNwrwCatchmentModelDataToModel(IHydroModel model, NwrwImporterHelper helper)
        {
            var rrModel = model as RainfallRunoffModel;

            if (rrModel == null)
            {
                Log.Warn($"Could not add {nameof(NwrwDefinition)} to {nameof(RainfallRunoffModel)}.");
                return;
            }

            lock (rrModel.NwrwDefinitions)
            {
                if (rrModel.NwrwDefinitions.Any(nd => nd.SurfaceType.Equals(this.SurfaceType)))
                {
                    Log.Warn($"Could not add {nameof(NwrwDefinition)} to {nameof(RainfallRunoffModel)}.");
                    return;
                }
            }

            lock(rrModel.NwrwDefinitions)
                rrModel.NwrwDefinitions.Add(this);
        }

        public void InitializeNwrwCatchmentModelData(NwrwData nwrwData)
        {
            //nothing to initialize
        }
    }
}