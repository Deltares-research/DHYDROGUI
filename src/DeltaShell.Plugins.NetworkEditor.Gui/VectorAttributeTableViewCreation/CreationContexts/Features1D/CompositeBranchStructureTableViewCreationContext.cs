using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    public class CompositeBranchStructureTableViewCreationContext : ITableViewCreationContext<ICompositeBranchStructure, CompositeBranchStructureRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription() => "Composite branch structure table view";

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<ICompositeBranchStructure> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.CompositeBranchStructures, data);
        }

        /// <inheritdoc/>
        public CompositeBranchStructureRow CreateFeatureRowObject(ICompositeBranchStructure feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new CompositeBranchStructureRow(feature);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<ICompositeBranchStructure> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}