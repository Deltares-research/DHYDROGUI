using System;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="ConstantParametersViewModel"/> defines the view model for the ConstantParametersView.
    /// </summary>
    public class ConstantParametersViewModel
    {
        private readonly ConstantParameters observedConstantParameters;

        /// <summary>
        /// Creates a new <see cref="ConstantParametersViewModel"/>.
        /// </summary>
        /// <param name="parameters"> The observed constant parameters. </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>;
        /// </exception>
        public ConstantParametersViewModel(ConstantParameters parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            observedConstantParameters = parameters;
        }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public double Height
        {
            get => observedConstantParameters.Height;
            set => observedConstantParameters.Height = value;
        }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        public double Period
        {
            get => observedConstantParameters.Period;
            set => observedConstantParameters.Period = value;
        }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public double Direction
        {
            get => observedConstantParameters.Direction;
            set => observedConstantParameters.Direction = value;
        }

        /// <summary>
        /// Gets or sets the spreading.
        /// </summary>
        public double Spreading
        {
            get => observedConstantParameters.Spreading;
            set => observedConstantParameters.Spreading = value;
        }
    }
}