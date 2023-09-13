using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.IO;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.SpatiallyDefinedDataComponents;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.GeometricDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.Plugins.FMSuite.Wave.DataAccess
{
    /// <summary>
    /// Visits <see cref="ISpatiallyDefinedDataComponent"/> to build a <see cref="IniSection"/>
    /// with the relevant spectrum data.
    /// </summary>
    /// <seealso cref="ISpatiallyDefinedDataComponentVisitor"/>
    public class SpectrumDataComponentVisitor : ISpatiallyDefinedDataComponentVisitor
    {
        private readonly IniSection section;
        private readonly SpectrumParametersVisitor parametersVisitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="SpectrumDataComponentVisitor"/> class.
        /// </summary>
        /// <param name="section">The INI section.</param>
        /// <param name="filesManager">The files manager.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="section"/> or <paramref name="filesManager"/> is <c>null</c>.
        /// </exception>
        public SpectrumDataComponentVisitor(IniSection section, IFilesManager filesManager)
        {
            Ensure.NotNull(section, nameof(section));
            Ensure.NotNull(filesManager, nameof(filesManager));

            this.section = section;
            parametersVisitor = new SpectrumParametersVisitor(section, filesManager);
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
                section.AddOrUpdateProperty(KnownWaveProperties.Spectrum, parametersVisitor.SpectrumFile);
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

                section.AddSpatialProperty(KnownWaveProperties.CondSpecAtDist, kvp.Key.Distance);
                section.AddProperty(KnownWaveProperties.Spectrum, parametersVisitor.SpectrumFile);
            }
        }
    }
}