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
    /// <see cref="IPump"/> data.
    /// </summary>
    public class PumpTableViewCreationContext : ITableViewCreationContext<IPump, PumpRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription() => "Pump table view";

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<IPump> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Pumps, data);
        }

        /// <inheritdoc/>
        public PumpRow CreateFeatureRowObject(IPump feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new PumpRow(feature);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<IPump> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}