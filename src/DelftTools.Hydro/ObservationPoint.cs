using System.ComponentModel;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [Entity(FireOnCollectionChange = false)]
    public class ObservationPoint : BranchFeatureHydroObject, IObservationPoint
    {
        public ObservationPoint()
        {
            Name = "observation";
        }

        public virtual IHydroNetwork HydroNetwork => Network as IHydroNetwork;

        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        public static ObservationPoint CreateDefault(IBranch branch)
        {
            var observationPoint = new ObservationPoint
            {
                Branch = branch,
                Network = branch.Network,
                Chainage = 0,
                Geometry = new Point(branch.Geometry.Coordinates[0])
            };
            observationPoint.Name =
                HydroNetworkHelper.GetUniqueFeatureName(observationPoint.Network as HydroNetwork, observationPoint);
            return observationPoint;
        }

        public override void CopyFrom(object source)
        {
            base.CopyFrom(source);
            var copyFrom = (ObservationPoint) source;
        }
    }
}