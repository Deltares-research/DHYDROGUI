using System.Collections.Generic;
using DelftTools.Hydro;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// dry points, <see cref="GroupablePointFeature"/>, data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{GroupablePointFeature,GroupablePointFeatureRow}"/>
    public class DryPointTableViewCreationContext : GroupableFeatureTableViewCreationContext<GroupablePointFeature, GroupablePointFeatureRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Dry point table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<GroupablePointFeature> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.DryPoints, data);
        }

        public override GroupablePointFeatureRow CreateFeatureRowObject(GroupablePointFeature feature, IEnumerable<GroupablePointFeature> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));

            return new GroupablePointFeatureRow(feature);
        }
    }
}