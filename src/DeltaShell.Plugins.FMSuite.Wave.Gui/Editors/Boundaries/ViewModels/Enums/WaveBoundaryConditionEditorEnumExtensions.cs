using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions;
using DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.Boundaries.ViewModels.Enums;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.Enums
{
    /// <summary>
    /// <see cref="WaveBoundaryConditionEditorEnumExtension"/> provides several conversion extensions
    /// to transform the ViewTypes to their corresponding model layer types.
    /// </summary>
    public static class WaveBoundaryConditionEditorEnumExtension
    {
        // Note these methods work because the Enums have been defined with the same values. Keep in mind that changing either of the enums, 
        // will potentially break this functionality.

        /// <summary>
        /// Converts the <see cref="BoundaryConditionPeriodType"/> to its corresponding <see cref="PeriodViewType"/>.
        /// </summary>
        /// <param name="periodType"> The enum to transform.</param>
        /// <returns>
        /// The corresponding <see cref="PeriodViewType"/>.
        /// </returns>
        public static PeriodViewType ConvertToPeriodViewType(this BoundaryConditionPeriodType periodType)
        {
            return (PeriodViewType)periodType;
        }

        /// <summary>
        /// Converts the <see cref="PeriodViewType"/> to its corresponding <see cref="BoundaryConditionPeriodType"/>.
        /// </summary>
        /// <param name="periodViewType"> The enum to transform.</param>
        /// <returns>
        /// The corresponding <see cref="BoundaryConditionPeriodType"/>.
        /// </returns>
        public static BoundaryConditionPeriodType ConvertToBoundaryConditionPeriodType(this PeriodViewType periodViewType)
        {
            return (BoundaryConditionPeriodType)periodViewType;
        }

        /// <summary>
        /// Converts the <see cref="BoundaryConditionDirectionalSpreadingType"/> to its corresponding <see cref="DirectionalSpreadingViewType"/>.
        /// </summary>
        /// <param name="spreadingType"> The enum to transform.</param>
        /// <returns>
        /// The corresponding <see cref="DirectionalSpreadingViewType"/>.
        /// </returns>
        public static DirectionalSpreadingViewType ConvertToDirectionalSpreadingViewType(this BoundaryConditionDirectionalSpreadingType spreadingType)
        {
            return (DirectionalSpreadingViewType)spreadingType;
        }

        /// <summary>
        /// Converts the <see cref="DirectionalSpreadingViewType"/> to its corresponding <see cref="BoundaryConditionDirectionalSpreadingType"/>.
        /// </summary>
        /// <param name="spreadingViewType"> The enum to transform.</param>
        /// <returns>
        /// The corresponding <see cref="BoundaryConditionDirectionalSpreadingType"/>.
        /// </returns>
        public static BoundaryConditionDirectionalSpreadingType ConvertToBoundaryConditionDirectionalSpreadingType(this DirectionalSpreadingViewType spreadingViewType)
        {
            return (BoundaryConditionDirectionalSpreadingType)spreadingViewType;
        }
    }
}