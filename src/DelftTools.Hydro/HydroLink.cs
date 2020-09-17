using System;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro
{
    [Entity(FireOnCollectionChange = false)]
    public class HydroLink : Unique<long>, IFeature, INameable
    {
        public HydroLink() {}

        public HydroLink(IHydroObject source, IHydroObject target) {}

        [FeatureAttribute]
        [Aggregation]
        public virtual IHydroObject Source { get; set; }

        [FeatureAttribute]
        [Aggregation]
        public virtual IHydroObject Target { get; set; }

        public virtual IGeometry Geometry { get; set; }

        public virtual IFeatureAttributeCollection Attributes { get; set; }

        [FeatureAttribute]
        public virtual string Name { get; set; }

        public virtual object Clone()
        {
            throw new NotImplementedException();
        }
    }
}