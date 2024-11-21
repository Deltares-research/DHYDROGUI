using FluentValidation;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Validator for new style external forcings file data.
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - All boundary data objects must be valid.
    /// - All lateral data objects must be valid.
    /// - All meteo data objects must be valid.
    /// </remarks>
    public sealed class BndExtForceFileDataValidator : AbstractValidator<BndExtForceFileData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceFileDataValidator"/> class.
        /// </summary>
        public BndExtForceFileDataValidator()
        {
            RuleForEach(x => x.BoundaryForcings).SetValidator(new BndExtForceBoundaryDataValidator());
            RuleForEach(x => x.LateralForcings).SetValidator(new BndExtForceLateralDataValidator());
            RuleForEach(x => x.MeteoForcings).SetValidator(new BndExtForceMeteoDataValidator());
        }
    }
}