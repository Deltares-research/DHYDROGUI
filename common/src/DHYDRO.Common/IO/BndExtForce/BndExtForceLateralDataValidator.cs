using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using DHYDRO.Common.IO.Validation;
using DHYDRO.Common.Properties;
using FluentValidation;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Validator for the new style external forcings file lateral data.
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Identifier must be provided.
    /// - X/Y coordinates count must be equal to the specified number of coordinates.
    /// - Location file must be a valid path.
    /// - Discharge must be provided.
    /// - Discharge must be a valid value.
    /// </remarks>
    public sealed class BndExtForceLateralDataValidator : AbstractValidator<BndExtForceLateralData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceBoundaryDataValidator"/> class.
        /// </summary>
        public BndExtForceLateralDataValidator()
        {
            RuleForId();
            RuleForXCoordinates();
            RuleForYCoordinates();
            RuleForLocationFile();
            RuleForDischarge();
        }

        /// <summary>
        /// Provides access to the file system.
        /// </summary>
        public IFileSystem FileSystem { get; set; } = new FileSystem();

        private void RuleForId()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMissingValueMessage(BndExtForceFileConstants.Keys.Id, data => data.LineNumber);
        }

        private void RuleForXCoordinates()
        {
            RuleFor(x => x.XCoordinates)
                .Must((x, _) => HasValidCount(x.XCoordinates, x.NumCoordinates))
                .WithMessage(x => string.Format(Resources.X_coordinates_count_must_be_equal_to_0_, x.NumCoordinates), x => x.LineNumber);
        }

        private void RuleForYCoordinates()
        {
            RuleFor(x => x.YCoordinates)
                .Must((x, _) => HasValidCount(x.YCoordinates, x.NumCoordinates))
                .WithMessage(x => string.Format(Resources.Y_coordinates_count_must_be_equal_to_0_, x.NumCoordinates), x => x.LineNumber);
        }

        private static bool HasValidCount(IEnumerable<double> coordinates, int expectedCount)
            => coordinates == null && expectedCount == 0 || coordinates?.Count() == expectedCount;

        private void RuleForLocationFile()
        {
            RuleFor(x => x.LocationFile)
                .Must(FileExists)
                .WithMessage(x => string.Format(Resources.Location_file_does_not_exist_0_, x.LocationFile), x => x.LineNumber)
                .When(x => !string.IsNullOrEmpty(x.LocationFile));
        }

        private bool FileExists(string fileName)
            => FileSystem.File.Exists(fileName);

        private void RuleForDischarge()
        {
            RuleFor(x => x.Discharge)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMissingValueMessage(BndExtForceFileConstants.Keys.Discharge, data => data.LineNumber)
                .SetValidator(_ => new BndExtForceDischargeDataValidator { FileSystem = FileSystem });
        }
    }
}