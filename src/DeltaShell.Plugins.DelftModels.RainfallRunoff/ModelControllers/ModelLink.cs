using DelftTools.Hydro;
using DelftTools.Utils;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.ModelControllers
{
    public class ModelLink
    {
        public IFeature FromFeature { get; set; }
        public IFeature ToFeature { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }

        public string Name { get; set; }

        public HydroLink RealLink { get; private set; }

        public ModelLink(string linkName, IFeature fromFeature, HydroLink realLink, string toId=null)
        {
            Name = linkName;
            RealLink = realLink;

            FromFeature = fromFeature;
            FromId = ((INameable) fromFeature).Name;
            ToId = toId ?? FromId + RainfallRunoffModel.BoundarySuffix;
        }

        public ModelLink(string linkName, IFeature fromFeature, HydroLink realLink, IFeature toFeature)
            : this(linkName, fromFeature, realLink, ((INameable)toFeature).Name)
        {
            ToFeature = toFeature;
        }
    }
}