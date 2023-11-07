using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="Weir2D"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{Weir2D,Weir2DRow}"/>
    public class Weir2DTableViewCreationContext : GroupableFeatureTableViewCreationContext<Weir2D, Weir2DRow>
    {
        /// <inheritdoc/>
        public override string GetDescription() => "Weir 2D table view";

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<Weir2D> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Weirs, data);
        }

        public override Weir2DRow CreateFeatureRowObject(Weir2D feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new Weir2DRow(feature);
        }
    }
}