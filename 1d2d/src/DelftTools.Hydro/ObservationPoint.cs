using System.ComponentModel;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro
{
    [Entity(FireOnCollectionChange=false)]
    public class ObservationPoint : BranchFeatureHydroObject, IObservationPoint 
    {
        public ObservationPoint()
        {
            Name = "observation";
        }
        
        public static ObservationPoint CreateDefault(IBranch branch)
        {
            var observationPoint = new ObservationPoint
                                       {
                                           Branch = branch,
                                           Network = branch.Network,
                                           Chainage = 0,
                                           Geometry = new Point(branch.Geometry.Coordinates[0])
                                       };
            observationPoint.Name = HydroNetworkHelper.GetUniqueFeatureName(observationPoint.Network as HydroNetwork, observationPoint);
            return observationPoint;
        }

        public virtual IHydroNetwork HydroNetwork
        {
            get { return Network as IHydroNetwork; }
        }
        
        [DisplayName("Long name")]
        [FeatureAttribute(Order = 2)]
        public virtual string LongName { get; set; }

        public virtual ICompositeNetworkPointFeature ParentPointFeature { get; set; }
        public virtual ICompositeBranchStructure ParentStructure { get; set; }
        public virtual double OffsetY { get; set; }
        public virtual StructureType GetStructureType()
        {
            return StructureType.ObservationPoint;
        }

        [DisplayName("Chainage")]
        [FeatureAttribute(Order = 5)]
        public override double Chainage { get => base.Chainage; set => base.Chainage = value; }
    }
}
