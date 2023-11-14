using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.FeaturesRR
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="WasteWaterTreatmentPlant"/> data.
    /// </summary>
    public class WasteWaterTreatmentPlantTableViewCreationContext : ITableViewCreationContext<WasteWaterTreatmentPlant, WasteWaterTreatmentPlantRow, IDrainageBasin>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Waste water treatment plant table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IDrainageBasin region, IEnumerable<WasteWaterTreatmentPlant> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.WasteWaterTreatmentPlants, data);
        }

        public WasteWaterTreatmentPlantRow CreateFeatureRowObject(WasteWaterTreatmentPlant feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new WasteWaterTreatmentPlantRow(feature);
        }

        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<WasteWaterTreatmentPlant> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}