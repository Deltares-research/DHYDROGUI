using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.SourcesAndSinks
{
    /// <summary>
    /// This represent the function of the <see cref="SourceAndSink"/>.
    /// The variable order of the function always remains the same:
    /// Discharge | Salinity | Temperature | Sediment Fractions | Secondary Flow | Tracers
    /// </summary>
    /// <remarks>
    /// This variables of this function should not be changed from outside this class.
    /// </remarks>
    public sealed class SourceAndSinkFunction : Function
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceAndSinkFunction"/> class.
        /// </summary>
        public SourceAndSinkFunction()
        {
            Arguments.Add(new Variable<DateTime>(SourceSinkVariableInfo.TimeVariableName) {DefaultValue = DateTime.Today});

            AddVariable(SourceSinkVariableInfo.DischargeVariableName, SourceSinkVariableInfo.DischargeUnitDescription,
                        SourceSinkVariableInfo.DischargeUnitSymbol);
            AddVariable(SourceSinkVariableInfo.SalinityVariableName, SourceSinkVariableInfo.SalinityUnitDescription,
                        SourceSinkVariableInfo.SalinityUnitSymbol);
            AddVariable(SourceSinkVariableInfo.TemperatureVariableName, SourceSinkVariableInfo.TemperatureUnitDescription,
                        SourceSinkVariableInfo.TemperatureUnitSymbol);
            AddVariable(SourceSinkVariableInfo.SecondaryFlowVariableName, SourceSinkVariableInfo.SecondaryFlowUnitDescription,
                        SourceSinkVariableInfo.SecondaryFlowUnitSymbol);
        }

        /// <summary>
        /// Removes the sediment fraction variable with the specified <paramref name="name"/> from the function.
        /// </summary>
        /// <param name="name">The name of the sediment fraction.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        public void RemoveSedimentFraction(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            var variable = GetVariable<SedimentFractionVariable>(name);
            Components.Remove(variable);
        }

        /// <summary>
        /// Removes the tracer variable with the specified <paramref name="name"/> from the function.
        /// </summary>
        /// <param name="name">The name of the tracer.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty.
        /// </exception>
        public void RemoveTracer(string name)
        {
            Ensure.NotNullOrEmpty(name, nameof(name));

            var variable = GetVariable<TracerVariable>(name);
            Components.Remove(variable);
        }

        /// <summary>
        /// Adds a new tracer variable to the function.
        /// </summary>
        /// <param name="name">The name of the tracer.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty or when a tracet component already exists with this name.
        /// </exception>
        public void AddTracer(string name)
        {
            ThrowIfContains<TracerVariable>(name);

            Components.Add(new TracerVariable(name));
        }

        /// <summary>
        /// Adds a new sediment fraction variable to the function.
        /// </summary>
        /// <param name="name">The name of the sediment fraction.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="name"/> is <c>null</c> or empty or when a sediment fraction component already exists with this name.
        /// </exception>
        public void AddSedimentFraction(string name)
        {
            ThrowIfContains<SedimentFractionVariable>(name);

            int index = Components.FindIndex(c => c.Name == SourceSinkVariableInfo.SecondaryFlowVariableName);

            Components.Insert(index, new SedimentFractionVariable(name));
        }

        private void ThrowIfContains<T>(string name) where T : IVariable
        {
            if (GetVariable<T>(name) != null)
            {
                throw new ArgumentException(string.Format(Resources.SourceAndSinkFunction_Already_contains_a_component_with_name, nameof(SourceAndSinkFunction), name));
            }
        }

        private T GetVariable<T>(string name) where T : IVariable
        {
            return Components.OfType<T>().FirstOrDefault(v => v.Name == name);
        }

        private void AddVariable(string name, string unitDescription, string unitSymbol)
        {
            Components.Add(CreateVariable(name, unitDescription, unitSymbol));
        }

        private static IVariable CreateVariable(string name, string unitDescription, string unitSymbol)
        {
            return new Variable<double>(name) {Unit = new Unit(unitDescription, unitSymbol)};
        }
    }
}