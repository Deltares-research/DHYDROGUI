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
    /// <see cref="IObservationPoint"/> data.
    /// </summary>
    public class ObservationPointTableViewCreationContext : ITableViewCreationContext<IObservationPoint, ObservationPointRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Observation point table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<IObservationPoint> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.ObservationPoints, data);
        }

        /// <inheritdoc/>
        public ObservationPointRow CreateFeatureRowObject(IObservationPoint feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new ObservationPointRow(feature);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<IObservationPoint> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}