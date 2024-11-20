using System;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;

namespace DeltaShell.Plugins.FMSuite.FlowFM.FeatureData.Laterals
{
    /// <summary>
    /// Class representing a lateral discharge time series function.
    /// </summary>
    public sealed class LateralDischargeFunction : TimeSeries
    {
        private const string functionName = "Discharge";
        private const string dischargeVariableName = "Discharge";
        private const string dischargeUnitDescription = "cubic meters per second";
        private const string dischargeUnitSymbol = "m3/s";

        /// <summary>
        /// Initialize a new instance of the <see cref="LateralDischargeFunction"/> class.
        /// </summary>
        public LateralDischargeFunction()
        {
            Name = functionName;
            AddVariable(dischargeVariableName,
                        dischargeUnitDescription,
                        dischargeUnitSymbol);
        }

        /// <summary>
        /// Get the discharge component.
        /// </summary>
        public IVariable DischargeComponent => Components[0];
        
        /// <summary>
        /// Gets and Sets the timezone.
        /// </summary>
        public TimeSpan TimeZone { get; set; }

        private void AddVariable(string name, string unitDescription, string unitSymbol)
        {
            Components.Add(CreateVariable(name, unitDescription, unitSymbol));
        }

        private static IVariable CreateVariable(string name, string unitDescription, string unitSymbol)
        {
            return new Variable<double>(name) { Unit = new Unit(unitDescription, unitSymbol) };
        }
    }
}