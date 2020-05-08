using System.IO;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Visits <see cref="IForcingTypeDefinedParameters"/> to build a <see cref="DelftIniCategory"/>
    /// with spectrum data.
    /// </summary>
    /// <seealso cref="IForcingTypeDefinedParametersVisitor"/>
    public class SpectrumParametersVisitor : IForcingTypeDefinedParametersVisitor
    {
        private readonly DelftIniCategory category;
        private readonly IFilesManager filesManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumParametersVisitor"/> class.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="filesManager">The files manager.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="category"/> or <paramref name="filesManager"/>
        /// is <c>null</c>.
        /// </exception>
        public SpectrumParametersVisitor(DelftIniCategory category, IFilesManager filesManager)
        {
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(filesManager, nameof(filesManager));

            this.category = category;
            this.filesManager = filesManager;
        }

        /// <summary>
        /// Gets the spectrum file name.
        /// </summary>
        public string SpectrumFile { get; private set; }

        /// <summary>
        /// Gets the spectrum type of the boundary.
        /// </summary>
        public SpectrumImportExportType SpectrumType { get; private set; }

        public void Visit<T>(ConstantParameters<T> constantParameters) where T : IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(constantParameters, nameof(constantParameters));

            SpectrumType = SpectrumImportExportType.Parametrized;
            SetSpectrumProperty();
        }

        public void Visit<T>(TimeDependentParameters<T> timeDependentParameters) where T : IBoundaryConditionSpreading, new()
        {
            Ensure.NotNull(timeDependentParameters, nameof(timeDependentParameters));

            SpectrumType = SpectrumImportExportType.Parametrized;
            SetSpectrumProperty();
        }

        public void Visit(FileBasedParameters fileBasedParameters)
        {
            Ensure.NotNull(fileBasedParameters, nameof(fileBasedParameters));

            SpectrumType = SpectrumImportExportType.FromFile;
            SetSpectrumProperty();

            string filePath = fileBasedParameters.FilePath;
            if (filePath == string.Empty)
            {
                // this string should not be empty, because the DelftIniWriter
                // only writes properties with values that are not null or empty.
                SpectrumFile = " ";
                return;
            }

            filesManager.Add(filePath, s => fileBasedParameters.FilePath = s);
            SpectrumFile = Path.GetFileName(filePath);
        }

        private void SetSpectrumProperty()
        {
            category.SetProperty(KnownWaveProperties.SpectrumSpec, SpectrumType.GetDescription());
        }
    }
}