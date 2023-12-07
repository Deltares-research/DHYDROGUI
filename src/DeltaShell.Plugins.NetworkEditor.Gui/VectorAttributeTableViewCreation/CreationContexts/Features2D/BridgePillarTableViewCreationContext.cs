using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="BridgePillar"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{BridgePillar, BridgePillarRow}"/>
    public class BridgePillarTableViewCreationContext : GroupableFeatureTableViewCreationContext<BridgePillar, BridgePillarRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Bridge pillar table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<BridgePillar> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.BridgePillars, data);
        }

        /// <inheritdoc/>
        public override BridgePillarRow CreateFeatureRowObject(BridgePillar feature, IEnumerable<BridgePillar> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new BridgePillarRow(feature, nameValidator);
        }
    }
}