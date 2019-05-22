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
        public virtual string Name { get; set; }

        [FeatureAttribute]
        [Aggregation]
        public virtual IHydroObject Source { get; set; }

        [FeatureAttribute]
        [Aggregation]
        public virtual IHydroObject Target { get; set; }

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

        public virtual object Clone()
        {
            return new HydroLink
            {
                Source = Source,
                Target = Target,
                Attributes = Attributes != null ? (IFeatureAttributeCollection) Attributes.Clone() : null,
                Geometry = Geometry != null ? (IGeometry) Geometry.Clone() : null
            };
        }

        public virtual IGeometry Geometry { get; set; }

        public virtual IFeatureAttributeCollection Attributes { get; set; }

        public override string ToString()
        {
            return Name + " (" + Source + " -> " + Target + ")";
        }
    }
}