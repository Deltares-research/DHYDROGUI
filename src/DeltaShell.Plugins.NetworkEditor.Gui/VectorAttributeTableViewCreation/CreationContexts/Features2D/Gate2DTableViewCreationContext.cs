using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.NameValidation;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="Gate2D"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{Gate2D,Gate2DRow}"/>
    public class Gate2DTableViewCreationContext : GroupableFeatureTableViewCreationContext<Gate2D, Gate2DRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Gate 2D table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<Gate2D> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Gates, data);
        }

        public override Gate2DRow CreateFeatureRowObject(Gate2D feature, IEnumerable<Gate2D> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new Gate2DRow(feature, nameValidator);
        }
    }
}