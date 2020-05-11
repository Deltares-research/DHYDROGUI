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

        public HydroLink(IHydroObject source, IHydroObject target)
        {
            if (source != null && target != null)
            {
                Name = "HL_" + source.Name + "_" + target.Name;
            }

            Source = source;
            Target = target;
        }

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

        public override string ToString()
        {
            return Name + " (" + Source + " -> " + Target + ")";
        }

        public virtual object Clone()
        {
            return new HydroLink
            {
                Source = Source,
                Target = Target,
                Attributes = (IFeatureAttributeCollection) Attributes?.Clone(),
                Geometry = (IGeometry) Geometry?.Clone()
            };
        }

        // TODO: remove and use <any> in mapping
        protected virtual IFeature SourceFeature
        {
            get => Source;
            set => Source = (IHydroObject) value;
        }

        // TODO: remove and use <any> in mapping
        protected virtual IFeature TargetFeature
        {
            get => Target;
            set => Target = (IHydroObject) value;
        }
    }
}