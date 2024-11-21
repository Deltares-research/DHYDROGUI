using System.IO.Abstractions;
using DHYDRO.Common.IO.Validation;
using DHYDRO.Common.Properties;
using FluentValidation;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Validator for the new style external forcings file discharge data.
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Time series file name must be provided when discharge type is time-varying.
    /// - Time series file must be a valid path.
    /// </remarks>
    public sealed class BndExtForceDischargeDataValidator : AbstractValidator<BndExtForceDischargeData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceDischargeDataValidator"/> class.
        /// </summary>
        public BndExtForceDischargeDataValidator()
        {
            RuleForTimeSeriesFile();
        }

        /// <summary>
        /// Provides access to the file system.
        /// </summary>
        public IFileSystem FileSystem { get; set; } = new FileSystem();

        private void RuleForTimeSeriesFile()
        {
            RuleFor(x => x.TimeSeriesFile)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMissingValueMessage(BndExtForceFileConstants.Keys.Discharge, x => x.LineNumber)
                .Must(FileExists).WithMessage(x => string.Format(Resources.Discharge_file_does_not_exist_0_, x.TimeSeriesFile), x => x.LineNumber)
                .When(x => x.DischargeType == BndExtForceDischargeType.TimeVarying);
        }

        private bool FileExists(string fileName)
            => FileSystem.File.Exists(fileName);
    }
}