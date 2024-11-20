using System.ComponentModel;
using DelftTools.Shell.Gui;
using DelftTools.Utils.ComponentModel;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    /// <summary>
    /// Class defining the properties in the properties windows for <see cref="Samples"/> data.
    /// </summary>
    [DisplayName("Samples")]
    public sealed class SamplesProperties : ObjectProperties<Samples>
    {
        /// <summary>
        /// The name of the source file with the samples.
        /// </summary>
        [Category("General")]
        [DisplayName("File")]
        [Description("The name of the samples file.")]
        public string File => data.SourceFileName;

        /// <summary>
        /// The operand for how the data is set onto the grid.
        /// </summary>
        [Category("General")]
        [DisplayName("Operand")]
        [Description("The operand for how the data is set onto the grid.")]
        [DynamicReadOnly]
        public PointwiseOperationType Operand
        {
            get => data.Operand;
            set => data.Operand = value;
        }

        /// <summary>
        /// The method for how the data is interpolated onto the grid.
        /// </summary>
        [Category("General")]
        [DisplayName("Interpolation method")]
        [Description("The method for how the data is interpolated onto the grid.")]
        [DynamicReadOnly]
        public SpatialInterpolationMethod InterpolationMethod
        {
            get => data.InterpolationMethod;
            set => data.InterpolationMethod = value;
        }

        /// <summary>
        /// The averaging method when <see cref="SpatialInterpolationMethod.Averaging"/> is used.
        /// </summary>
        [Category("General")]
        [DisplayName("Averaging method")]
        [Description("The grid cell averaging method to use.")]
        [DynamicReadOnly]
        public GridCellAveragingMethod AveragingMethod
        {
            get => data.AveragingMethod;
            set => data.AveragingMethod = value;
        }

        /// <summary>
        /// The relative search cell size when <see cref="SpatialInterpolationMethod.Averaging"/> is used.
        /// </summary>
        [Category("General")]
        [DisplayName("Relative search cell size")]
        [Description("The relative search cell size.")]
        [DynamicReadOnly]
        public double RelativeSearchCellSize
        {
            get => data.RelativeSearchCellSize;
            set => data.RelativeSearchCellSize = value;
        }

        /// <summary>
        /// The extrapolation tolerance when <see cref="SpatialInterpolationMethod.Triangulation"/> is used.
        /// </summary>
        [Category("General")]
        [DisplayName("Extrapolation tolerance")]
        [Description("The extrapolation tolerance.")]
        [DynamicReadOnly]
        public double ExtrapolationTolerance
        {
            get => data.ExtrapolationTolerance;
            set => data.ExtrapolationTolerance = value;
        }

        /// <summary>
        /// Checks whether or not the provided property should be read-only.
        /// </summary>
        /// <param name="propertyName"> The name of a property of <see cref="SamplesProperties"/>.</param>
        /// <returns><c>true</c> if the property should be read-only; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown when <paramref name="propertyName"/> is <c>null</c> or white space.
        /// </exception>
        [DynamicReadOnlyValidationMethod]
        public bool IsReadonly(string propertyName)
        {
            Ensure.NotNullOrWhiteSpace(propertyName, nameof(propertyName));

            if (!data.HasData)
            {
                return true;
            }

            switch (propertyName)
            {
                case nameof(AveragingMethod):
                case nameof(RelativeSearchCellSize):
                    return InterpolationMethod != SpatialInterpolationMethod.Averaging;
                case nameof(ExtrapolationTolerance):
                    return InterpolationMethod != SpatialInterpolationMethod.Triangulation;
                default:
                    return false;
            }
        }
    }
}