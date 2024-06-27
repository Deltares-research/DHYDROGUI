using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// roof area, <see cref="GroupableFeature2DPolygon"/>, data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{GroupableFeature2DPolygon,GroupableFeature2DPolygonRow}"/>
    public class RoofAreaTableViewCreationContext : GroupableFeatureTableViewCreationContext<GroupableFeature2DPolygon, GroupableFeature2DPolygonRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Roof area table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<GroupableFeature2DPolygon> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.RoofAreas, data);
        }

        public override GroupableFeature2DPolygonRow CreateFeatureRowObject(GroupableFeature2DPolygon feature, IEnumerable<GroupableFeature2DPolygon> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new GroupableFeature2DPolygonRow(feature, nameValidator);
        }
    }
}