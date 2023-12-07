using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Utils.Guards;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;

namespace DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts.Features2D
{
    /// <summary>
    /// Provides the creation context for a <see cref="TableViewInfoCreator"/> that should create the table view info for
    /// <see cref="IObservationPoint"/> data.
    /// </summary>
    /// <seealso cref="GroupableFeatureTableViewCreationContext{GroupableFeature2DPoint,GroupableFeature2DPointRow}"/>
    public class ObservationPointTableViewCreationContext : GroupableFeatureTableViewCreationContext<GroupableFeature2DPoint, GroupableFeature2DPointRow>
    {
        /// <inheritdoc/>
        public override string GetDescription()
        {
            return "Observation point table view";
        }

        /// <inheritdoc/>
        public override bool IsRegionData(HydroArea region, IEnumerable<GroupableFeature2DPoint> data)
        {
            Ensure.NotNull(region, nameof(region));
            Ensure.NotNull(data, nameof(data));

            return ReferenceEquals(region.ObservationPoints, data);
        }

        public override GroupableFeature2DPointRow CreateFeatureRowObject(GroupableFeature2DPoint feature, IEnumerable<GroupableFeature2DPoint> allFeatures)
        {
            Ensure.NotNull(feature, nameof(feature));
            Ensure.NotNull(allFeatures, nameof(allFeatures));
            
            var nameValidator = NameValidator.CreateDefault();
            nameValidator.AddValidator(new UniqueNameValidator(allFeatures));

            return new GroupableFeature2DPointRow(feature, nameValidator);
        }
    }
}