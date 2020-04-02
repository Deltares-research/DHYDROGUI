using System.Linq;
using DelftTools.Hydro;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public abstract class BasinFeatureFromGisImporterBase : FeatureFromGisImporterBase
    {
        public IDrainageBasin DrainageBasin
        {
            get
            {
                return HydroRegion is IDrainageBasin
                           ? HydroRegion as IDrainageBasin
                           : HydroRegion.SubRegions.OfType<IDrainageBasin>().First();
            }
        }
    }
}