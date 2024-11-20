using System;
using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes
{
    /// <summary>
    /// <see cref="GaussViewShape"/> defines the Gauss shape within the View layer.
    /// </summary>
    /// <seealso cref="IViewShape"/>
    [Description("Gauss")]
    public class GaussViewShape : IViewShape
    {
        private readonly GaussShape observedGaussShape;

        /// <summary>
        /// Creates a new <see cref="GaussViewShape"/>.
        /// </summary>
        /// <param name="gaussShape"> The underlying gauss shape. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="gaussShape"/> is <c>null</c>.
        /// </exception>
        public GaussViewShape(GaussShape gaussShape)
        {
            observedGaussShape = gaussShape ?? throw new ArgumentNullException(nameof(gaussShape));
        }

        /// <summary>
        /// Gets or sets the gaussian spread.
        /// </summary>
        public double GaussianSpread
        {
            get => observedGaussShape.GaussianSpread;
            set => observedGaussShape.GaussianSpread = value;
        }

        public IBoundaryConditionShape ObservedShape => observedGaussShape;
    }
}