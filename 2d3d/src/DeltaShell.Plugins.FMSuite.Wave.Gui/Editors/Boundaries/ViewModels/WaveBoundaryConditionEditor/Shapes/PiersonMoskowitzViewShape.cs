using System;
using System.ComponentModel;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Shapes;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.Shapes
{
    /// <summary>
    /// <see cref="PiersonMoskowitzViewShape"/> defines the Pierson-Moskowitz shape within the View layer.
    /// </summary>
    /// <seealso cref="IViewShape"/>
    [Description("Pierson-Moskowitz")]
    public class PiersonMoskowitzViewShape : IViewShape
    {
        /// <summary>
        /// Creates a new instance of the <see cref="PiersonMoskowitzViewShape"/>.
        /// </summary>
        /// <param name="piersonMoskowitzShape">The underlying Pierson-moskowitz shape.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="piersonMoskowitzShape"/> is <c>null</c>.
        /// </exception>
        public PiersonMoskowitzViewShape(PiersonMoskowitzShape piersonMoskowitzShape)
        {
            ObservedShape = piersonMoskowitzShape ?? throw new ArgumentNullException(nameof(piersonMoskowitzShape));
        }

        public IBoundaryConditionShape ObservedShape { get; }
    }
}