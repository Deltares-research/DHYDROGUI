using System;
using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes
{
    /// <summary>
    /// <see cref="JonswapShape"/> defines the Jonswap shape within the View layer.
    /// </summary>
    /// <seealso cref="IViewShape"/>
    [Description("Jonswap")]
    public class JonswapViewShape : IViewShape
    {
        private readonly JonswapShape observedJonswapShape;

        /// <summary>
        /// Creates a new <see cref="JonswapViewShape"/>.
        /// </summary>
        /// <param name="jonswapShape"> The underlying jonswap shape. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="jonswapShape"/> is <c>null</c>.
        /// </exception>
        public JonswapViewShape(JonswapShape jonswapShape)
        {
            observedJonswapShape = jonswapShape ?? throw new ArgumentNullException(nameof(jonswapShape));
        }

        /// <summary>
        /// Gets or sets the peak enhancement factor.
        /// </summary>
        public double PeakEnhancementFactor
        {
            get => observedJonswapShape.PeakEnhancementFactor;
            set => observedJonswapShape.PeakEnhancementFactor = value;
        }

        public IBoundaryConditionShape ObservedShape => observedJonswapShape;
    }
}