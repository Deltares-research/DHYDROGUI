using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="ThinDam2D"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{ThinDam2D,ThinDam2DRow}"/>
    public class ThinDam2DTableViewCreationContext : GroupableFeatureTableViewCreationContext<ThinDam2D, ThinDam2DRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Thin dam 2D table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<ThinDam2D> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.ThinDams, data);
        }

        public override ThinDam2DRow CreateFeatureRowObject(ThinDam2D feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new ThinDam2DRow(feature);
        }
    }
}