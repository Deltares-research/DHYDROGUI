using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Properties;
using log4net;


namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.ModelDefinition
{
    /// <summary>
    /// WaterFlowModelPropertySetter provides the methods to set different
    /// model wide aspects based upon a data access model of the md1d file.
    /// </summary>
    public static class WaterFlowModelPropertySetter
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowModel1D));

        public static void SetWaterFlowModelProperties(IEnumerable<DelftIniCategory> modelSettingsCategories, WaterFlowModel1D model, Action<string, IList<string>> createAndAddErrorReport)
        {
            foreach (var category in modelSettingsCategories)
            {
                try
                {
                    var propertySetter = WaterFlowModelPropertySetterFactory.GetPropertySetter(category);
                    propertySetter.SetProperties(category, model, createAndAddErrorReport);
                }
                catch (Exception)
                {
                    Log.WarnFormat(Resources.WaterFlowModelPropertySetter_SetWaterFlowModelProperties_There_is_unrecognized_data_read_from_the_md1d_file_with_header___0___, category.Name);
                }
            }
        }

        /// <summary>
        /// Set the WaterFlowModel1DOutputSettingData engine parameters to the
        /// values specified in <paramref name="modelSettingsCategories"/>.
        /// </summary>
        /// <param name="modelSettingsCategories">A set of DelftIniCategories describing a md1d file. </param>
        /// <param name="outputSettings"> reference to WaterFlowModel1DOutputSettingData of some WaterFlow1DModel.</param>
        /// <remarks>
        /// Pre-condition: modelSettingsCategories != null && outputSettings != null
        /// </remarks>
        public static void SetOutputProperties(IEnumerable<DelftIniCategory> modelSettingsCategories,
                                                                             WaterFlowModel1DOutputSettingData outputSettings)
        {
            var headerMapping = new Dictionary<string, ElementSet>
            {
                [ModelDefinitionsRegion.ResultsNodesHeader] = ElementSet.GridpointsOnBranches,
                [ModelDefinitionsRegion.ResultsBranchesHeader] = ElementSet.ReachSegElmSet,
                [ModelDefinitionsRegion.ResultsStructuresHeader] = ElementSet.Structures,
                [ModelDefinitionsRegion.ResultsPumpsHeader] = ElementSet.Pumps,
                [ModelDefinitionsRegion.ResultsObservationsPointsHeader] = ElementSet.Observations,
                [ModelDefinitionsRegion.ResultsRetentionsHeader] = ElementSet.Retentions,
                [ModelDefinitionsRegion.ResultsLateralsHeader] = ElementSet.Laterals,
                [ModelDefinitionsRegion.ResultsWaterBalanceHeader] = ElementSet.ModelWide,
                [ElementSet.FiniteVolumeGridOnGridPoints.ToString()] = ElementSet.FiniteVolumeGridOnGridPoints, // DELWAQ value, currently not defined in ModelDefinitionsRegion.
            };

            foreach (var cat in modelSettingsCategories)
            {
                // Determine element set
                ElementSet elementSet;
                if (!headerMapping.TryGetValue(cat.Name, out elementSet)) continue;

                foreach (var prop in cat.Properties)
                {
                    // Determine quantity type
                    QuantityType qType;
                    try
                    {
                        qType = (QuantityType) Enum.Parse(typeof(QuantityType), prop.Name);
                    }
                    catch (ArgumentException)
                    {
                        // Kernel expects the property Lateral1D2D. GUI defines this as
                        // QTotal_1d2d. As such we need to explicitly test for this.
                        if (prop.Name == "Lateral1D2D")
                            qType = QuantityType.QTotal_1d2d;
                        else
                            continue;
                    }

                    // Kernel expects Dispersion in ResultsBranchesHeader. GUI puts it
                    // in ElementSet.GridpointsOnBranches, as such we need to 
                    // explicitly correct this. 
                    var engineParameter = outputSettings.GetEngineParameter(qType,
                        qType == QuantityType.Dispersion && elementSet == ElementSet.ReachSegElmSet
                            ? ElementSet.GridpointsOnBranches
                            : elementSet,
                        DataItemRole.Output);

                    if (engineParameter == null) continue;

                    // determine AggregateOption
                    AggregationOptions aggregateOption;
                    try
                    {
                        aggregateOption = (AggregationOptions) Enum.Parse(typeof(AggregationOptions), prop.Value);
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                    
                    engineParameter.AggregationOptions = aggregateOption;
                }
            }
        }
    }
}
