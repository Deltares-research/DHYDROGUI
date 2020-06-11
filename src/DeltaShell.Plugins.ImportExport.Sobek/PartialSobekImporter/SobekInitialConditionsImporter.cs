using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Sobek.Readers.Readers;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    //todo: FM1D2D-660
    public class SobekInitialConditionsImporter: PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekInitialConditionsImporter));

        public override string DisplayName => "Initial Conditions";
        
        protected override void PartialImport()
        {
            Log.DebugFormat("Importing initial conditions ...");

            var fmModel = GetModel<WaterFlowFMModel>();
            
            var initialPath = GetFilePath(SobekFileNames.SobekInitialConditionsFileName);
            if (!File.Exists(initialPath))
            {
                Log.WarnFormat("Initial condition file [{0}] not found; skipping...", initialPath);
                return;
            }
            
            var initalFlowConditionsReader = new InitalFlowConditionsReader();
            var flowInitialConditions = initalFlowConditionsReader.Read(initialPath);

            var builder = new InitialConditionsBuilder(flowInitialConditions, HydroNetwork);
            builder.Build();

            var channelInitialConditionDefinitions = fmModel.ChannelInitialConditionDefinitions;
            UpdateChannelInitialConditions(builder.ChannelInitialConditionDefinitionsDict, channelInitialConditionDefinitions);

            if (builder.GlobalsHaveBeenSet)
            {
                UpdateInitialConditionGlobalSettings(fmModel.ModelDefinition, builder.GlobalQuantity, builder.GlobalValue);
            }
        }

        private void UpdateInitialConditionGlobalSettings(WaterFlowFMModelDefinition modelDefinition,
            InitialConditionQuantity globalQuantity, double globalValue)
        {
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalValue1D, globalValue.ToString(CultureInfo.InvariantCulture));
            modelDefinition.SetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D, $"{ (int)globalQuantity }");
        }

        private void UpdateChannelInitialConditions(
            Dictionary<string, ChannelInitialConditionDefinition> builderChannelInitialConditionDefinitionsDict, 
            ICollection<ChannelInitialConditionDefinition> channelInitialConditionDefinitions)
        {
            var channelInitialConditionDict =
                channelInitialConditionDefinitions.ToDictionary(definition => definition.Channel.Name, StringComparer.InvariantCultureIgnoreCase);
            
            foreach (var channelInitialConditionDefinitionKeyValuePair in builderChannelInitialConditionDefinitionsDict)
            {
                var builderChannelInitialConditionName = channelInitialConditionDefinitionKeyValuePair.Key;
                var builderChannelInitialCondition = channelInitialConditionDefinitionKeyValuePair.Value;

                if (channelInitialConditionDict.TryGetValue(builderChannelInitialConditionName, out var originalDefinition))
                {
                    originalDefinition.CopyFrom(builderChannelInitialCondition);
                }
            }
        }
    }
}
