using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="HydroLink"/> data.
    /// </summary>
    public class HydroLinkTableViewCreationContext : ITableViewCreationContext<HydroLink, HydroLinkRow, IHydroRegion>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Hydro link table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IHydroRegion region, IEnumerable<HydroLink> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Links, data);
        }

        public HydroLinkRow CreateFeatureRowObject(HydroLink feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new HydroLinkRow(feature);
        }

        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<HydroLink> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}