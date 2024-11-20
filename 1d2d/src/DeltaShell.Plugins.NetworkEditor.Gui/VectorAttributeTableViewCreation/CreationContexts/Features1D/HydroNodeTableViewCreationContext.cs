using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features1D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="IHydroNode"/> data.
    /// </summary>
    public sealed class HydroNodeTableViewCreationContext : ITableViewCreationContext<IHydroNode, HydroNodeRow, IHydroNetwork>
    {
        /// <inheritdoc/>
        public string GetDescription()
        {
            return "Hydro node table view";
        }

        /// <inheritdoc/>
        public bool IsRegionData(IHydroNetwork region, IEnumerable<IHydroNode> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.HydroNodes, data);
        }

        /// <inheritdoc/>
        public HydroNodeRow CreateFeatureRowObject(IHydroNode feature, IEnumerable<IHydroNode> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new HydroNodeRow(feature, nameValidator);
        }

        /// <inheritdoc/>
        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<IHydroNode> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}