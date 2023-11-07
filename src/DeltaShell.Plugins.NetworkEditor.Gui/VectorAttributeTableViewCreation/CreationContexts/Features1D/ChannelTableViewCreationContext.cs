using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="IChannel"/> data.
    /// </summary>
    public class ChannelTableViewCreationContext : ITableViewCreationContext<IChannel, ChannelRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription() => "Channel table view";

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<IChannel> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Channels, data);
        }

        /// <inheritdoc/>
        public ChannelRow CreateFeatureRowObject(IChannel feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new ChannelRow(feature);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<IChannel> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}