using System;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.
    BoundaryParameterSpecific
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

    /// <summary>
    /// <see cref="ConstantParameters{TSpreading}"/> defines the view model for the ConstantParametersView.
    /// </summary>
    public class ConstantParametersViewModel<TSpreading> : ConstantParametersViewModel
        where TSpreading : IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Creates a new <see cref="ConstantParameters{TSpreading}"/>.
        /// </summary>
        /// <param name="parameters"> The observed constant parameters. </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>;
        /// </exception>
        public ConstantParametersViewModel(ConstantParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));
            ObservedParameters = parameters;

            SpreadingUnit = GetSpreadingUnit();
        }

        private static string GetSpreadingUnit()
        {
            if (typeof(TSpreading) == typeof(PowerDefinedSpreading))
            {
                return "-";
            }
            if (typeof(TSpreading) == typeof(DegreesDefinedSpreading))
            {
                return "deg";
            }

            return "";
        }

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public ConstantParameters<TSpreading> ObservedParameters { get; }

        public override double Height
        {
            get => ObservedParameters.Height;
            set => ObservedParameters.Height = value;
        }

        public override double Period
        {
            get => ObservedParameters.Period;
            set => ObservedParameters.Period = value;
        }

        public override double Direction
        {
            get => ObservedParameters.Direction;
            set => ObservedParameters.Direction = value;
        }

        public override double Spreading
        {
            get => GetSpreadingValue();
            set => SetSpreadingValue(value);
        }

        public override string SpreadingUnit { get; }

        private double GetSpreadingValue()
        {
            // Awkward cast due to behaviour of C# 7, required to make type
            // matching on generics work (Roslyn will create a compilation error otherwise).
            // This could be removed if we ever decide to switch C# 7.1 or higher.
            object spreading = ObservedParameters.Spreading;

            switch (spreading)
            {
                case PowerDefinedSpreading powerDefinedSpreading:
                    return powerDefinedSpreading.SpreadingPower;
                case DegreesDefinedSpreading degreesDefinedSpreading:
                    return degreesDefinedSpreading.DegreesSpreading;
                default:
                    throw new NotSupportedException($"The type {typeof(TSpreading)} is not supported.");
            }
        }

        private void SetSpreadingValue(double value)
        {
            // Awkward cast due to behaviour of C# 7, required to make type
            // matching on generics work (Roslyn will create a compilation error otherwise).
            // This could be removed if we ever decide to switch C# 7.1 or higher.
            object spreading = ObservedParameters.Spreading;

            switch (spreading)
            {
                case PowerDefinedSpreading powerDefinedSpreading:
                    powerDefinedSpreading.SpreadingPower = value;
                    break;
                case DegreesDefinedSpreading degreesDefinedSpreading:
                    degreesDefinedSpreading.DegreesSpreading = value;
                    break;
                default:
                    throw new NotSupportedException($"The type {typeof(TSpreading)} is not supported.");
            }
        }
    }
}
