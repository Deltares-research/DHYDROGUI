using System;
using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelOutputSetter is responsible for interpreting Results regions in a .md1d
    /// data access model and set these on the OutputSettings of a model.
    /// </summary>
    /// <remarks>
    /// The following result regions are currently handled:
    ///   ResultsNodes
    ///   ResultsBranches
    ///   ResultsPumps
    ///   ResultsObservationPoints
    ///   ResultsLaterals
    ///   ResultsWaterBalance
    ///   FiniteVolumeGridOnGridPoints.
    ///
    /// see the D-Flow1d Technical reference manual for all possible options supported by the kernel.
    /// </remarks>
    /// <seealso cref="DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition.IWaterFlowModelCategoryPropertySetter" />
    public class WaterFlowModelOutputSetter : IWaterFlowModelCategoryPropertySetter
    {
        private readonly Dictionary<string, ElementSet> headerMapping = new Dictionary<string, ElementSet>
        {
            [ModelDefinitionsRegion.ResultsNodesHeader] = ElementSet.GridpointsOnBranches,
            [ModelDefinitionsRegion.ResultsBranchesHeader] = ElementSet.ReachSegElmSet,
            [ModelDefinitionsRegion.ResultsStructuresHeader] = ElementSet.Structures,
            [ModelDefinitionsRegion.ResultsPumpsHeader] = ElementSet.Pumps,
            [ModelDefinitionsRegion.ResultsObservationsPointsHeader] = ElementSet.Observations,
            [ModelDefinitionsRegion.ResultsRetentionsHeader] = ElementSet.Retentions,
            [ModelDefinitionsRegion.ResultsLateralsHeader] = ElementSet.Laterals,
            [ModelDefinitionsRegion.ResultsWaterBalanceHeader] = ElementSet.ModelWide,
            [ModelDefinitionsRegion.FiniteVolumeGridOnGridPoints] = ElementSet.FiniteVolumeGridOnGridPoints
        };

        /// <summary>
        /// Set the OutputSettings of <paramref name="model"/> to the values specified in <paramref name="category"/>.
        /// </summary>
        /// <param name="category">A set of DelftIniCategory describing a Results region of the md1d file. </param>
        /// <param name="model"> reference to WaterFlowModel1D containg OuputSettings which will be set.</param>
        /// <param name="errorMessages"> A collection of error messages that can be added to in case errors occur in this method. </param>
        /// <remarks>
        /// No engine parameters will be changed which are not specified in <paramref name="category"/>.
        /// If a property is not recognised, it is quietly ignored.
        /// 
        /// Pre-condition: categories != null && model.OutputSettings != null
        /// </remarks>
        public void SetProperties(DelftIniCategory category, WaterFlowModel1D model, IList<string> errorMessages)
        {
            SetProperties(category, model.OutputSettings);
        }

        /// <summary>
        /// Set the engine parameters in <paramref name="outputSettings"/> to the values specified in <paramref name="category"/> 
        /// </summary>
        /// <param name="category">A set of DelftIniCategory describing a Results region of the md1d file. </param>
        /// <param name="outputSettings"> reference to WaterFlowModel1DOutputSettingData of some WaterFlow1DModel.</param>
        /// <remarks>
        /// No engine parameters will be changed which are not specified in <paramref name="category"/>.
        /// If a property is not recognised, it is quietly ignored.
        /// 
        /// Pre-condition: categories != null && outputSettings != null
        /// </remarks>
        public void SetProperties(DelftIniCategory category, WaterFlowModel1DOutputSettingData outputSettings)
        {
            // Determine element set
            ElementSet elementSet;
            if (!headerMapping.TryGetValue(category.Name, out elementSet)) return;

            foreach (var property in category.Properties)
            {
                var quantityType = GetQuantityType(property);
                if (quantityType == QuantityType.UndeterminedValue) continue;

                // Kernel expects Dispersion in ResultsBranchesHeader. GUI puts it
                // in ElementSet.GridpointsOnBranches, as such we need to 
                // explicitly correct this. 
                var engineParameter = outputSettings.GetEngineParameter(quantityType,
                    quantityType == QuantityType.Dispersion && elementSet == ElementSet.ReachSegElmSet
                        ? ElementSet.GridpointsOnBranches
                        : elementSet,
                    DataItemRole.Output);

                if (engineParameter == null) continue;

                // determine AggregateOption
                AggregationOptions aggregateOption;
                try
                {
                    aggregateOption = (AggregationOptions)Enum.Parse(typeof(AggregationOptions), property.Value);
                }
                catch (ArgumentException)
                {
                    continue;
                }

                engineParameter.AggregationOptions = aggregateOption;
            }
        }

        private static QuantityType GetQuantityType(IDelftIniProperty property)
        {
            QuantityType quantityType;
            try
            {
                quantityType = (QuantityType)Enum.Parse(typeof(QuantityType), property.Name);
            }
            catch (ArgumentException)
            {
                // Kernel expects the property Lateral1D2D. GUI defines this as
                // QTotal_1d2d. As such we need to explicitly test for this.
                quantityType = property.Name == "Lateral1D2D" 
                    ? QuantityType.QTotal_1d2d 
                    : QuantityType.UndeterminedValue;
            }

            return quantityType;
        }
    }
}
