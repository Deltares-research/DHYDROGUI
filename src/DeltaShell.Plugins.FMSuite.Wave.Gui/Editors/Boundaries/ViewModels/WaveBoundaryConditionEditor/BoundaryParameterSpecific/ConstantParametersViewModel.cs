using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="ConstantParametersViewModel"/> defines the view model for the ConstantParametersView.
    /// </summary>
    public class ConstantParametersViewModel
    {
        /// <summary>
        /// Creates a new <see cref="ConstantParametersViewModel"/>.
        /// </summary>
        /// <param name="parameters"> The observed constant parameters. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>;
        /// </exception>
        public ConstantParametersViewModel(ConstantParameters parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ObservedParameters = parameters;
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public ConstantParameters ObservedParameters { get; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public double Height
        {
            get => ObservedParameters.Height;
            set => ObservedParameters.Height = value;
        }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        public double Period
        {
            get => ObservedParameters.Period;
            set => ObservedParameters.Period = value;
        }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public double Direction
        {
            get => ObservedParameters.Direction;
            set => ObservedParameters.Direction = value;
        }

        /// <summary>
        /// Gets or sets the spreading.
        /// </summary>
        public double Spreading
        {
            get => ObservedParameters.Spreading;
            set => ObservedParameters.Spreading = value;
        }
    }
}