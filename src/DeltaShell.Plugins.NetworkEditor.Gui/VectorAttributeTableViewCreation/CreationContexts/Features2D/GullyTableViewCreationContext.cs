using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="Gully"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{Gully,GullyRow}"/>
    public class GullyTableViewCreationContext : GroupableFeatureTableViewCreationContext<Gully, GullyRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Gully table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<Gully> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Gullies, data);
        }

        public override GullyRow CreateFeatureRowObject(Gully feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new GullyRow(feature);
        }
    }
}