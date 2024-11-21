using System.IO.Abstractions;
using DHYDRO.Common.IO.Validation;
using DHYDRO.Common.Properties;
using FluentValidation;

namespace DHYDRO.Common.IO.BndExtForce
{
    /// <summary>
    /// Validator for the new style external forcings file boundary data.
    /// </summary>
    /// <remarks>
    /// Validation rules:
    /// - Location file(s) must be a valid path.
    /// - Forcing file must be a valid path.
    /// </remarks>
    public sealed class BndExtForceBoundaryDataValidator : AbstractValidator<BndExtForceBoundaryData>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BndExtForceBoundaryDataValidator"/> class.
        /// </summary>
        public BndExtForceBoundaryDataValidator()
        {
            RuleForLocationFile();
            RulesForForcingFiles();
        }

        /// <summary>
        /// Provides access to the file system.
        /// </summary>
        public IFileSystem FileSystem { get; set; } = new FileSystem();

        private void RuleForLocationFile()
        {
            RuleFor(x => x.LocationFile)
                .Must(FileExists)
                .WithMessage(x => string.Format(Resources.Location_file_does_not_exist_0_, x.LocationFile), x => x.LineNumber)
                .When(x => !string.IsNullOrEmpty(x.LocationFile));
        }

        private void RulesForForcingFiles()
        {
            RuleForEach(x => x.ForcingFiles)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMissingValueMessage(BndExtForceFileConstants.Keys.ForcingFile, x => x.LineNumber)
                .Must(FileExists).WithMessage((_, x) => string.Format(Resources.Forcing_file_does_not_exist_0_, x), x => x.LineNumber);
        }

        private bool FileExists(string fileName)
            => FileSystem.File.Exists(fileName);
    }
}