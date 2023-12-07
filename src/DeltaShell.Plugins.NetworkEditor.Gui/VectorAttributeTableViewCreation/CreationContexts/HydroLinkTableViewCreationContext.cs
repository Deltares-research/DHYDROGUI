using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
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

        public HydroLinkRow CreateFeatureRowObject(HydroLink feature, IEnumerable<HydroLink> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new HydroLinkRow(feature, nameValidator);
        }

        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<HydroLink> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}