using System;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.Boundaries.ViewModels.WaveBoundaryConditionEditor.BoundaryParameterSpecific
{
    /// <summary>
    /// <see cref="ConstantParametersViewModelGeneric{TSpreading}"/> defines the view model for the ConstantParametersView.
    /// </summary>
    public class ConstantParametersViewModelGeneric<TSpreading> : ConstantParametersViewModel
        where TSpreading : class, IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Creates a new <see cref="ConstantParametersViewModelGeneric{TSpreading}"/>.
        /// </summary>
        /// <param name="parameters">The observed constant parameters.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="parameters"/> is <c>null</c>.
        /// </exception>
        public ConstantParametersViewModelGeneric(ConstantParameters<TSpreading> parameters)
        {
            Ensure.NotNull(parameters, nameof(parameters));

            ObservedParameters = parameters;
            SpreadingUnit = SpreadingConversion.GetSpreadingUnit<TSpreading>().Symbol;
        }

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

        /// <summary>
        /// Gets the observed parameters.
        /// </summary>
        public ConstantParameters<TSpreading> ObservedParameters { get; }

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