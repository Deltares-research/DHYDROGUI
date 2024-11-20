using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="ICulvert"/> data.
    /// </summary>
    public class CulvertTableViewCreationContext : ITableViewCreationContext<ICulvert, CulvertRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Culvert table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<ICulvert> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Culverts, data);
        }

        /// <inheritdoc/>
        public CulvertRow CreateFeatureRowObject(ICulvert feature, IEnumerable<ICulvert> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new CulvertRow(feature, nameValidator);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<ICulvert> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}