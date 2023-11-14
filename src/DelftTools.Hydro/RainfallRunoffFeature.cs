using System.ComponentModel;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using GeoAPI.Extensions.Feature;
using log4net;
using NetTopologySuite.Extensions.Features;

namespace DelftTools.Hydro
{
    [Entity]
    public abstract class RainfallRunoffFeature: Feature, IHasNameValidation
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RainfallRunoffFeature));
        private readonly NameValidator nameValidator;

        protected RainfallRunoffFeature()
        {
            nameValidator = NameValidator.CreateDefault();
        }
        
        [DisplayName("Name")]
        [FeatureAttribute]
        public virtual string Name { get; set; }

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