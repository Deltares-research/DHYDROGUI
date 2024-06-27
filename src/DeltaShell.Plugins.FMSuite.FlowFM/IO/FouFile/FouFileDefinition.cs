using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Defines the supported variables for statistical analysis.
    /// </summary>
    public sealed class FouFileDefinition
    {
        private readonly Dictionary<string, FouFileVariable> variables = new Dictionary<string, FouFileVariable>
        {
            ["WriteWlAverage"] = new FouFileVariable { Name = "wl" },
            ["WriteWlMaximum"] = new FouFileVariable { Name = "wl", EllipticParameters = "max" },
            ["WriteWlMinimum"] = new FouFileVariable { Name = "wl", EllipticParameters = "min" },
            ["WriteUcAverage"] = new FouFileVariable { Name = "uc", LayerNumber = 1 },
            ["WriteUcMaximum"] = new FouFileVariable { Name = "uc", LayerNumber = 1, EllipticParameters = "max" },
            ["WriteUcMinimum"] = new FouFileVariable { Name = "uc", LayerNumber = 1, EllipticParameters = "min" },
            ["WriteFbAverage"] = new FouFileVariable { Name = "fb" },
            ["WriteFbMaximum"] = new FouFileVariable { Name = "fb", EllipticParameters = "max" },
            ["WriteFbMinimum"] = new FouFileVariable { Name = "fb", EllipticParameters = "min" },
            ["WriteWdogAverage"] = new FouFileVariable { Name = "wdog" },
            ["WriteWdogMaximum"] = new FouFileVariable { Name = "wdog", EllipticParameters = "max" },
            ["WriteWdogMinimum"] = new FouFileVariable { Name = "wdog", EllipticParameters = "min" },
            ["WriteVogAverage"] = new FouFileVariable { Name = "vog" },
            ["WriteVogMaximum"] = new FouFileVariable { Name = "vog", EllipticParameters = "max" },
            ["WriteVogMinimum"] = new FouFileVariable { Name = "vog", EllipticParameters = "min" }
        };

        /// <summary>
        /// Gets the supported variables for statistical analysis.
        /// </summary>
        public IEnumerable<FouFileVariable> Variables => variables.Values;

        /// <summary>
        /// Gets the Flow FM model property names corresponding to the supported variables.
        /// </summary>
        public IEnumerable<string> ModelPropertyNames => variables.Keys;

        /// <summary>
        /// Gets the Flow FM model property name corresponding to the specified variable.
        /// </summary>
        /// <param name="variable">The variable for which to get the model property name.</param>
        /// <returns>The Flow FM model property name corresponding to the specified variable or <c>null</c> when the variable is unknown.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="variable"/> is <c>null</c>.</exception>
        public string GetModelPropertyName(FouFileVariable variable)
        {
            Ensure.NotNull(variable, nameof(variable));
            
            return variables.Where(kvp => string.Equals(kvp.Value.Name, variable.Name, StringComparison.OrdinalIgnoreCase) &&
                                          string.Equals(kvp.Value.EllipticParameters, variable.EllipticParameters, StringComparison.OrdinalIgnoreCase))
                            .Select(kvp => kvp.Key)
                            .FirstOrDefault();
        }
    }
}