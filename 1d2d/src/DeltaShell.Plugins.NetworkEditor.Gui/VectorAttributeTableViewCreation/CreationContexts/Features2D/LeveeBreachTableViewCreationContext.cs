using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
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

        public Feature2DRow CreateFeatureRowObject(Feature2D feature, IEnumerable<Feature2D> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new Feature2DRow(feature, nameValidator);
        }

        public void CustomizeTableView(VectorLayerAttributeTableView view, IEnumerable<Feature2D> data, GuiContainer guiContainer)
        {
            // no customization needed
        }
    }
}