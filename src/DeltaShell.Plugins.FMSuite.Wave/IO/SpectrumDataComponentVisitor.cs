using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.IO.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.IO
{
    /// <summary>
    /// Visits <see cref="ISpatiallyDefinedDataComponent"/> to build a <see cref="DelftIniCategory"/>
    /// with the relevant spectrum data.
    /// </summary>
    /// <seealso cref="ISpatiallyDefinedDataComponentVisitor"/>
    public class SpectrumDataComponentVisitor : ISpatiallyDefinedDataComponentVisitor
    {
        private readonly DelftIniCategory category;
        private readonly SpectrumParametersVisitor parametersVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumDataComponentVisitor"/> class.
        /// </summary>
        /// <param name="category">The delft ini category.</param>
        /// <param name="filesManager">The files manager.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="category"/> or <paramref name="filesManager"/> is <c>null</c>.
        /// </exception>
        public SpectrumDataComponentVisitor(DelftIniCategory category, IFilesManager filesManager)
        {
            Ensure.NotNull(category, nameof(category));
            Ensure.NotNull(filesManager, nameof(filesManager));

            this.category = category;
            parametersVisitor = new SpectrumParametersVisitor(category, filesManager);
        }

        /// <summary>
        /// Gets spectrum type of the boundary.
        /// </summary>
        public SpectrumImportExportType SpectrumType { get; private set; }

        public void Visit<T>(UniformDataComponent<T> uniformDataComponent) where T : IForcingTypeDefinedParameters
        {
            Ensure.NotNull(uniformDataComponent, nameof(uniformDataComponent));

            uniformDataComponent.Data.AcceptVisitor(parametersVisitor);

            SpectrumType = parametersVisitor.SpectrumType;
            if (SpectrumType == SpectrumImportExportType.FromFile)
            {
                category.SetProperty(KnownWaveProperties.Spectrum, parametersVisitor.SpectrumFile);
            }
        }

        public void Visit<T>(SpatiallyVaryingDataComponent<T> spatiallyVaryingDataComponent) where T : IForcingTypeDefinedParameters
        {
            Ensure.NotNull(spatiallyVaryingDataComponent, nameof(spatiallyVaryingDataComponent));

            IOrderedEnumerable<KeyValuePair<SupportPoint, T>> sortedData =
                spatiallyVaryingDataComponent.Data.OrderBy(kvp => kvp.Key.Distance);

            foreach (KeyValuePair<SupportPoint, T> kvp in sortedData)
            {
                kvp.Value.AcceptVisitor(parametersVisitor);

                SpectrumType = parametersVisitor.SpectrumType;
                if (SpectrumType != SpectrumImportExportType.FromFile)
                {
                    return;
                }

                category.AddSpatialProperty(KnownWaveProperties.CondSpecAtDist, kvp.Key.Distance);
                category.AddProperty(KnownWaveProperties.Spectrum, parametersVisitor.SpectrumFile);
            }
        }
    }
}