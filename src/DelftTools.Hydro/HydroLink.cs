using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Data;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using log4net;

namespace DelftTools.Hydro
{
    /// <summary>
    /// Represent a hydro link between two instances of an <see cref="IHydroObject"/>.
    /// </summary>
    [Entity(FireOnCollectionChange = false)]
    public class HydroLink : Unique<long>, IFeature, IHasNameValidation
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(HydroLink));
        private readonly NameValidator nameValidator;
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
            nameValidator = NameValidator.CreateDefault();
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
            nameValidator = NameValidator.CreateDefault();
        }

        /// <summary>
        /// Gets or set the name of this hydro link.
        /// </summary>
        [FeatureAttribute]
        public virtual string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the source hydro object of this hydro link.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when this property is set with <c>null</c>.
        /// </exception>
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

        /// <summary>
        /// Gets or sets the target hydro object of this hydro link.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when this property is set with <c>null</c>.
        /// </exception>
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

        /// <inheritdoc/>
        public virtual void SetNameIfValid(string name)
        {
            if (ValidateName(name))
            {
                Name = name;
            }
        }
        
        private bool ValidateName(string name)
        {
            ValidationResult result = nameValidator.Validate(name);
            if (result.Valid)
            {
                return true;
            }

            log.Warn(result.Message);
            return false;
        }

        /// <inheritdoc/>
        public virtual void AttachNameValidator(IValidator<string> subValidator)
        {
            Ensure.NotNull(subValidator, nameof(subValidator));
            nameValidator.AddValidator(subValidator);
        }

        /// <inheritdoc/>
        public virtual void DetachNameValidator(IValidator<string> subValidator)
        {
            Ensure.NotNull(subValidator, nameof(subValidator));
            nameValidator.RemoveValidator(subValidator);
        }
    }
}