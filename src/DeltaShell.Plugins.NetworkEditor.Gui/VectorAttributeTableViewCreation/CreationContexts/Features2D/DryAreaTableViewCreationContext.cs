using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// dry area, <see cref="GroupableFeature2DPolygon"/>, data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{GroupableFeature2Dpolygon,GroupableFeature2DPolygonRow}"/>
    public class DryAreaTableViewCreationContext : GroupableFeatureTableViewCreationContext<GroupableFeature2DPolygon, GroupableFeature2DPolygonRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Dry area table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<GroupableFeature2DPolygon> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.DryAreas, data);
        }

        public override GroupableFeature2DPolygonRow CreateFeatureRowObject(GroupableFeature2DPolygon feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new GroupableFeature2DPolygonRow(feature);
        }
    }
}