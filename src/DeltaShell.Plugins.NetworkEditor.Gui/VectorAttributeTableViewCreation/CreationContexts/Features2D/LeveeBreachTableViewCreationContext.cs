using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// levee breach, <see cref="Feature2D"/>, data.
    /// </summary>
    public class LeveeBreachTableViewCreationContext : ITableViewCreationContext<Feature2D, Feature2DRow, HydroArea>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Levee breach table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(HydroArea region, IEnumerable<Feature2D> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.LeveeBreaches, data);
        }

        public Feature2DRow CreateFeatureRowObject(Feature2D feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new Feature2DRow(feature);
        }

        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<Feature2D> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}