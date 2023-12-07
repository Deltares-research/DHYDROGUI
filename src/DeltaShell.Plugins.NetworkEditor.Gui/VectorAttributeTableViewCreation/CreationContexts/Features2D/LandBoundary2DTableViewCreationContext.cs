using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="LandBoundary2D"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{LandBoundary2D,LandBoundary2DRow}"/>
    public class LandBoundary2DTableViewCreationContext : GroupableFeatureTableViewCreationContext<LandBoundary2D, LandBoundary2DRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Land boundary 2D table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<LandBoundary2D> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.LandBoundaries, data);
        }

        public override LandBoundary2DRow CreateFeatureRowObject(LandBoundary2D feature, IEnumerable<LandBoundary2D> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));
            
            return new LandBoundary2DRow(feature, nameValidator);
        }
    }
}