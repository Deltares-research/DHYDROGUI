using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;
using Deltares.Infrastructure.Extensions;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    /// <summary>
    /// Defines the supported variables for the statistical analysis configuration file (*.fou).
    /// </summary>
    public sealed class FouFileDefinition
    {
        private readonly Dictionary<string, FouFileVariable> variables;

        /// <summary>
        /// Defines the supported variables for the *.fou file, which is an input configuration file used 
        /// for specifying parameters for statistical analysis on FM model quantities.
        /// </summary>
        public FouFileDefinition()
        {
            variables = GenerateFouFileVariables();
        }
        
        private static Dictionary<string, FouFileVariable> GenerateFouFileVariables()
        {
            var variables = new Dictionary<string, FouFileVariable>();

            foreach (string quantity in FouFileQuantities.SupportedQuantities)
            {
                string basePropertyName = char.ToUpper(quantity[0]) + quantity.Substring(1);
                
                int? layerNumber = FouFileQuantities.Is3DQuantity(quantity) ? 1 : (int?)null;

                variables[$"Write{basePropertyName}Average"] = new FouFileVariable { Quantity = quantity, LayerNumber = layerNumber };
                variables[$"Write{basePropertyName}Maximum"] = new FouFileVariable { Quantity = quantity, LayerNumber = layerNumber, AnalysisType = "max" };
                variables[$"Write{basePropertyName}Minimum"] = new FouFileVariable { Quantity = quantity, LayerNumber = layerNumber, AnalysisType = "min" };
            }

            return variables;
        }

        /// <summary>
        /// Gets the supported fou file variables.
        /// </summary>
        public IEnumerable<FouFileVariable> Variables => variables.Values;

        /// <summary>
        /// Gets the Flow FM model property names corresponding to the supported fou file variables.
        /// </summary>
        public IEnumerable<string> ModelPropertyNames => variables.Keys;

        /// <summary>
        /// Gets the Flow FM model property name corresponding to the specified fou file variable.
        /// </summary>
        /// <param name="variable">The variable for which to get the model property name.</param>
        /// <returns>The Flow FM model property name corresponding to the specified variable or <c>null</c> when the variable is unknown.</returns>
        /// <exception cref="ArgumentNullException">When <paramref name="variable"/> is <c>null</c>.</exception>
        public string GetModelPropertyName(FouFileVariable variable)
        {
            Ensure.NotNull(variable, nameof(variable));

            return variables.Where(kvp => kvp.Value.Quantity.EqualsCaseInsensitive(variable.Quantity) &&
                                          kvp.Value.AnalysisType.EqualsCaseInsensitive(variable.AnalysisType))
                            .Select(kvp => kvp.Key)
                            .FirstOrDefault();
        }
    }
}