using System;
using System.CodeDom;
using DeltaShell.NGHS.Common;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Parameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.
    BoundaryParameterSpecific
{
    public abstract class ConstantParametersViewModel
    {
        public abstract double Height { get; set; }
        public abstract double Period { get; set; }
        public abstract double Direction { get; set; }
        public abstract double Spreading { get; set; }
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

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        public override double Height
        {
            get => ObservedParameters.Height;
            set => ObservedParameters.Height = value;
        }

        /// <summary>
        /// Gets or sets the period.
        /// </summary>
        public override double Period
        {
            get => ObservedParameters.Period;
            set => ObservedParameters.Period = value;
        }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        public override double Direction
        {
            get => ObservedParameters.Direction;
            set => ObservedParameters.Direction = value;
        }

        /// <summary>
        /// Gets or sets the spreading.
        /// </summary>
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
