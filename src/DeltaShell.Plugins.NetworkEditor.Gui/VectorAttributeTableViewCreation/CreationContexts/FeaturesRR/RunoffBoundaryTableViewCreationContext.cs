using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.FeaturesRR
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="RunoffBoundary"/> data.
    /// </summary>
    public class RunoffBoundaryTableViewCreationContext : ITableViewCreationContext<RunoffBoundary, RunoffBoundaryRow, IDrainageBasin>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Runoff boundary table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IDrainageBasin region, IEnumerable<RunoffBoundary> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Boundaries, data);
        }

        public RunoffBoundaryRow CreateFeatureRowObject(RunoffBoundary feature, IEnumerable<RunoffBoundary> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new RunoffBoundaryRow(feature, nameValidator);
        }

        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<RunoffBoundary> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}