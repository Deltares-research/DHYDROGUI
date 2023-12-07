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
    /// <see cref="Pump2D"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{Pump2D,Pump2DRow}"/>
    public sealed class Pump2DTableViewCreationContext : GroupableFeatureTableViewCreationContext<Pump2D, Pump2DRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Pump 2D table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<Pump2D> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.Pumps, data);
        }

        public override Pump2DRow CreateFeatureRowObject(Pump2D feature, IEnumerable<Pump2D> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new Pump2DRow(feature, nameValidator);
        }
    }
}