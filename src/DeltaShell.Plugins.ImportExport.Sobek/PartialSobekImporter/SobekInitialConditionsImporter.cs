using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Sobek.Readers.Readers;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter
{
    public class SobekInitialConditionsImporter: PartialSobekImporterBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SobekInitialConditionsImporter));

        public override string DisplayName => "Initial Conditions";

        public override SobekImporterCategories Category { get; } = SobekImporterCategories.WaterFlow1D;

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
            
            var initialFlowConditionsReader = new InitalFlowConditionsReader();
            var flowInitialConditions = initialFlowConditionsReader.Read(initialPath);

            var builder = new InitialConditionsBuilder(flowInitialConditions, HydroNetwork);
            builder.Build();

            UpdateChannelInitialConditions(fmModel.ChannelInitialConditionDefinitions, builder.ChannelInitialConditionDefinitionsDict);

            if (builder.HasSetGlobals)
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
            ICollection<ChannelInitialConditionDefinition> channelInitialConditionDefinitions,
            Dictionary<string, ChannelInitialConditionDefinition> builderChannelInitialConditionDefinitionsDict)
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
