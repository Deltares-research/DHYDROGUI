using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="Embankment"/> data.
    /// </summary>
    public class EmbankmentTableViewCreationContext : ITableViewCreationContext<Embankment, EmbankmentRow, HydroArea>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Embankment table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(HydroArea region, IEnumerable<Embankment> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Embankments, data);
        }

        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<Embankment> data, GuiContainer guiContainer)
        {
            // no customization needed
        }

        public EmbankmentRow CreateFeatureRowObject(Embankment feature, IEnumerable<Embankment> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new EmbankmentRow(feature, nameValidator);
        }
    }
}