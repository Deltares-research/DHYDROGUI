using System.Linq;
using DelftTools.Hydro;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public abstract class BasinFeatureFromGisImporterBase : FeatureFromGisImporterBase
    {
        public DrainageBasin DrainageBasin
        {
            get
            {
                return HydroRegion is DrainageBasin
                           ? HydroRegion as DrainageBasin
                           : HydroRegion.SubRegions.OfType<DrainageBasin>().First();
            }
        }
    }
}