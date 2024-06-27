using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using Deltares.Infrastructure.API.Guards;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Represent a hydro link between two instances of an <see cref="IHydroObject"/>.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class HydroLink : Unique<long>, IHydroLink
    {
        private IHydroObject source;
        private IHydroObject target;

        /// <summary>
        /// Initializes a new instance of the <see cref="HydroLink"/> class.
        /// </summary>
        /// <remarks>
        /// This constructor should currently only be used by NHibernate.
        /// Using this constructor might otherwise lead to an invalid state of this <see cref="HydroLink"/>.
        /// </remarks>
        public HydroLink()
        {
            // Used by NHibernate.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HydroLink"/> class.
        /// </summary>
        /// <param name="source"> The source hydro object to link from. </param>
        /// <param name="target"> The target hydro object to link to. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="source"/> or <paramref name="target"/> is <c>null</c>.
        /// </exception>
        public HydroLink(IHydroObject source, IHydroObject target)
        {
            Ensure.NotNull(source, nameof(source));
            Ensure.NotNull(target, nameof(target));
            
            Source = source;
            Target = target;
            Name = $"HL_{Source.Name}_{Target.Name}";
        }

        /// <summary>
        /// Gets or set the name of this hydro link.
        /// </summary>
        [FeatureAttribute]
        public virtual string Name { get; set; }
        
        /// <inheritdoc/>
        [FeatureAttribute]
        [Aggregation]
        public virtual IHydroObject Source
        {
            get => source;
            set
            {
                Ensure.NotNull(value, nameof(Source));
                source = value;
            }
        }

        /// <inheritdoc/>
        [FeatureAttribute]
        [Aggregation]
        public virtual IHydroObject Target
        {
            get => target;
            set
            {
                Ensure.NotNull(value, nameof(Target));
                target = value;
            }
        }

        /// <summary>
        /// Clones this hydro link instance.
        /// </summary>
        /// <returns>
        /// The cloned object.
        /// </returns>
        public virtual object Clone()
        {
            return new HydroLink(Source, Target)
            {
                Attributes = (IFeatureAttributeCollection)Attributes?.Clone(),
                Geometry = (IGeometry)Geometry?.Clone()
            };
        }

        /// <summary>
        /// Gets or sets the geometry of this hydro link.
        /// </summary>
        public virtual IGeometry Geometry { get; set; }

        /// <summary>
        /// Gets or set the attribute collection of this hydro link.
        /// </summary>
        public virtual IFeatureAttributeCollection Attributes { get; set; }

        /// <summary>
        /// Gets a string representing this hydro link, including the source and target hydro objects.
        /// </summary>
        /// <returns>
        /// The  string representation of a <see cref="HydroLink"/>.
        /// </returns>
        public override string ToString()
        {
            return $"{Name} ({Source} -> {Target})";
        }
    }
}