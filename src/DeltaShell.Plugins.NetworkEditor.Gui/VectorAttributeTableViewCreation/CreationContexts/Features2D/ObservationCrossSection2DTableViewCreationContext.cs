using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="ObservationCrossSection2D"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{ObservationCrossSection2D,ObservationCrossSection2DRow}"/>
    public class ObservationCrossSection2DTableViewCreationContext : GroupableFeatureTableViewCreationContext<ObservationCrossSection2D, ObservationCrossSection2DRow>
    {
        /// <inheritdoc/>
        public override string GetDescription() => "Observation cross section 2D table view";

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<ObservationCrossSection2D> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.ObservationCrossSections, data);
        }

        public override ObservationCrossSection2DRow CreateFeatureRowObject(ObservationCrossSection2D feature)
        {
            Ensure.NotNull(feature, nameof(feature));
            return new ObservationCrossSection2DRow(feature);
        }
    }
}