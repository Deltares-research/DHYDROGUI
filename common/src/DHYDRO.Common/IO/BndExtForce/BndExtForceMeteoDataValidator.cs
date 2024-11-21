using System.IO.Abstractions;
using DHYDRO.Common.IO.Validation;
using DHYDRO.Common.Properties;
using FluentValidation;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Validator for the new style external forcings file meteo data.
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Quantity name must be provided.
    /// - Forcing file name must be provided.
    /// - Forcing file must be a valid path.
    /// - Interpolation method must be provided.
    /// - Operand must be provided.
    /// </remarks>
    public sealed class BndExtForceMeteoDataValidator : AbstractValidator<BndExtForceMeteoData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceMeteoDataValidator"/> class.
        /// </summary>
        public BndExtForceMeteoDataValidator()
        {
            RuleForQuantity();
            RuleForForcingFile();
            RuleForInterpolationMethod();
            RuleForOperand();
        }

        /// <summary>
        /// Provides access to the file system.
        /// </summary>
        public IFileSystem FileSystem { get; set; } = new FileSystem();

        private void RuleForQuantity()
        {
            RuleFor(data => data.Quantity)
                .NotEmpty().WithMissingValueMessage(BndExtForceFileConstants.Keys.Quantity, data => data.LineNumber);
        }

        private void RuleForForcingFile()
        {
            RuleFor(x => x.ForcingFile)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMissingValueMessage(BndExtForceFileConstants.Keys.ForcingFile, x => x.LineNumber)
                .Must(FileExists).WithMessage(x => string.Format(Resources.Forcing_file_does_not_exist_0_, x.ForcingFile), x => x.LineNumber);
        }

        private bool FileExists(string fileName)
            => FileSystem.File.Exists(fileName);

        private void RuleForInterpolationMethod()
        {
            RuleFor(data => data.InterpolationMethod)
                .NotEqual(BndExtForceInterpolationMethod.None)
                .WithMissingValueMessage(BndExtForceFileConstants.Keys.InterpolationMethod, data => data.LineNumber);
        }

        private void RuleForOperand()
        {
            RuleFor(data => data.Operand)
                .NotEqual(BndExtForceOperand.None)
                .WithMissingValueMessage(BndExtForceFileConstants.Keys.Operand, data => data.LineNumber);
        }
    }
}