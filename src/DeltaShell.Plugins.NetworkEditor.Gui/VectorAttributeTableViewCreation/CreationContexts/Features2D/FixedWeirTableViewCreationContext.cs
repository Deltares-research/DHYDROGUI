using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="FixedWeir"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{FixedWeir,FixedWeirRow}"/>
    public class FixedWeirTableViewCreationContext : GroupableFeatureTableViewCreationContext<FixedWeir, FixedWeirRow>
    {
        /// <inheritdoc/>
        public override string GetDescription() => "Fixed weir table view";

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<FixedWeir> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.FixedWeirs, data);
        }

        public override FixedWeirRow CreateFeatureRowObject(FixedWeir feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new FixedWeirRow(feature);
        }
    }
}