using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.Spreading;

namespace DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.WaveEnergyFunctions
{
    /// <summary>
    /// <see cref="IWaveEnergyFunction{TSpreading}"/> defines the WaveEnergyFunction as used within a
    /// <see cref="TimeDependentParameters{TSpreading}"/>.
    /// </summary>
    /// <typeparam name="TSpreading">The type of the spreading.</typeparam>
    /// <remarks>
    /// The implementation of <see cref="IFunction"/> or <see cref="IVariable{T}"/> does not allow
    /// for much compile-time safety, as such these are exposed as-is. It is *STRONGLY* recommended
    /// not to try and edit the <see cref="DelftTools.Units.Unit"/>. Currently it assumed to be correctly
    /// set depending on the provided <typeparamref name="TSpreading"/>, no further checking is done once
    /// set.
    /// </remarks>
    public interface IWaveEnergyFunction<TSpreading> where TSpreading : IBoundaryConditionSpreading, new()
    {
        /// <summary>
        /// Gets the time argument.
        /// </summary>
        IVariable<DateTime> TimeArgument { get; }

        /// <summary>
        /// Gets the height component.
        /// </summary>
        IVariable<double> HeightComponent { get; }

        /// <summary>
        /// Gets the period component.
        /// </summary>
        IVariable<double> PeriodComponent { get; }

        /// <summary>
        /// Gets the direction variable.
        /// </summary>
        IVariable<double> DirectionComponent { get; }

        /// <summary>
        /// Gets the spreading component.
        /// </summary>
        IVariable<double> SpreadingComponent { get; }

        /// <summary>
        /// `
        /// Gets the underlying function.
        /// </summary>
        /// <remarks>
        /// Note that this is a direct reference to the underlying function.
        /// This allows for bypassing the extra verification provided in this
        /// class. Prefer using the properties and methods provided in this class,
        /// rather than relying on the underlying function.
        /// Note that adding or removing components or arguments will violate
        /// assumptions made in child functions. Use at your own caution.
        /// </remarks>
        IFunction UnderlyingFunction { get; }
    }
}