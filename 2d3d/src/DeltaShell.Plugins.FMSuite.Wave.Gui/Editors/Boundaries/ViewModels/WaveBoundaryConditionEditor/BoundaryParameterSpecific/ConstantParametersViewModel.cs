namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="ConstantParametersViewModel"/> defines the abstract view model for the ConstantParametersView.
    /// The actual values are set in the generic child class.
    /// </summary>
    public abstract class ConstantParametersViewModel
    {
        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public abstract double Height { get; set; }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        public abstract double Period { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public abstract double Direction { get; set; }

        /// <summary>
        /// Gets or sets the spreading.
        /// </summary>
        public abstract double Spreading { get; set; }

        /// <summary>
        /// Gets the spreading unit.
        /// </summary>
        public abstract string SpreadingUnit { get; }
    }
}