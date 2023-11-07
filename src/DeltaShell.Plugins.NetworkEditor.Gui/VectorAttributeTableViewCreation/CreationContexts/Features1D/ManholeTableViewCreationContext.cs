using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="IManhole"/> data.
    /// </summary>
    public sealed class ManholeTableViewCreationContext : ITableViewCreationContext<IManhole, ManholeRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription() => "Manhole node table view";

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<IManhole> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Manholes, data);
        }

        /// <inheritdoc/>
        public ManholeRow CreateFeatureRowObject(IManhole feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new ManholeRow(feature);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<IManhole> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}