using DelftTools.Hydro;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Creates <see cref="ChannelInitialConditionDefinition"/> from Sobek data.
    /// </summary>
    public class InitialConditionsBuilder
    {
        private const double Tolerance = 0.00001;

        private static readonly ILog Log = LogManager.GetLogger(typeof(InitialConditionsBuilder));

        private IList<FlowInitialCondition> flowInitialConditions;
        private readonly IHydroNetwork network;
        private List<string> listOfWarnings = new List<string>();
        private Dictionary<string, IBranch> branchesDict;

        public InitialConditionsBuilder(IEnumerable<FlowInitialCondition> flowInitialConditions,
            IHydroNetwork network)
        {
            this.flowInitialConditions = flowInitialConditions.ToList();
            this.network = network;
            ChannelInitialConditionDefinitionsDict = new Dictionary<string, ChannelInitialConditionDefinition>(StringComparer.InvariantCultureIgnoreCase);
        }

        public Dictionary<string, ChannelInitialConditionDefinition> ChannelInitialConditionDefinitionsDict { get; private set; }
        public InitialConditionQuantity GlobalQuantity { get; private set; }
        public double GlobalValue { get; private set; }
        public bool GlobalsHaveBeenSet { get; private set; }
        

        public void Build()
        {
            ReadAndRemoveGlobalDefinition();
            GenerateBranchesDict();
            if (branchesDict == null || branchesDict.Count == 0)
            {
                return;
            }

            RemoveAndWarnForUnusedConditions();
            CreateChannelInitialConditionDefinitions();

            RemoveAndWarnForUnusedDefinitions();

            if (listOfWarnings.Any())
                Log.Warn($"While importing initial conditions we encountered the following {listOfWarnings.Count} warnings: {Environment.NewLine}{string.Join(Environment.NewLine, listOfWarnings)}");
        }

        private void GenerateBranchesDict()
        {
            branchesDict = network.Branches.ToDictionary(branch => branch.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        private void RemoveAndWarnForUnusedConditions()
        {
            var conditionsWithoutBranch = flowInitialConditions.Where(c => !c.IsGlobalDefinition) //non globals
                .Where(c => (c.IsLevelBoundary || c.IsQBoundary) && !branchesDict.ContainsKey(c.BranchID)).ToList();
            foreach (var condition in conditionsWithoutBranch)
            {
                Log.WarnFormat("Channel {0} for initial condition {1} not found; skipped",
                    condition.BranchID, condition.ID);
            }
            //exclude the 'bad' conditions
            flowInitialConditions = flowInitialConditions.Except(conditionsWithoutBranch).ToList();
        }

        private void ReadAndRemoveGlobalDefinition()
        {
            var globalDefinition = flowInitialConditions.FirstOrDefault(ic => ic.IsGlobalDefinition);
            if (globalDefinition == null)
            {
                Log.WarnFormat("Globally defined flow conditions are not imported yet.");
            }
            else
            {
                if (globalDefinition.WaterLevelType == FlowInitialCondition.FlowConditionType.WaterDepth)
                {
                    GlobalValue = globalDefinition.Level.Constant;
                    GlobalQuantity = InitialConditionQuantity.WaterDepth;
                }
                else
                {
                    GlobalQuantity = InitialConditionQuantity.WaterLevel;
                    GlobalValue = globalDefinition.Level.Constant;
                }

                GlobalsHaveBeenSet = true;
                //remove it so we don't parse it later on
                flowInitialConditions.Remove(globalDefinition);
            }
        }

        private void CreateChannelInitialConditionDefinitions()
        {
            foreach (var initialCondition in flowInitialConditions)
            {
                if (initialCondition.IsLevelBoundary == false)
                {
                    Log.WarnFormat($"Cannot import {initialCondition.ID}. Only WaterDepth and WaterLevel initial conditions are currently supported.");
                    continue;
                } 

                var branchName = initialCondition.BranchID;
                if (!branchesDict.ContainsKey(branchName))
                {
                    listOfWarnings.Add($"Could not find branch {branchName}. Skipping import of this initial condition.");
                    continue;
                }

                var branch = branchesDict[branchName];
                var channel = branch as Channel;
                if (channel == null) throw new ArgumentException();
                
                var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(channel);
                var quantity = GetInitialConditionQuantity(initialCondition.WaterLevelType);

                if (initialCondition.Level.IsConstant)
                {
                    channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition;
                    channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Quantity = quantity;
                    channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition.Value = initialCondition.Level.Constant;
                }
                else
                {
                    channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
                    channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.Quantity = quantity;

                    //In sobek you can define two values at one point ( 0 = 5.0, 2500 = 5.0, 2500 = 3.0, 5000 = 3.0)
                    //In this case we add 0.01 to the second definition
                    var lastOffset = -1.0;
                    foreach (DataRow row in initialCondition.Level.Data.Rows)
                    {
                        var offset = (double) row[0];
                        if (Math.Abs(lastOffset - offset) < Tolerance)
                        {
                            offset += 0.01;
                        }

                        lastOffset = offset;
                        var value = (double) row[1]; //* correctionFactor?;
                        var constantSpatialChannelInitialConditionDefinition = new ConstantSpatialChannelInitialConditionDefinition()
                        {
                            Chainage = offset,
                            Value = value
                        };
                        channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.ConstantSpatialChannelInitialConditionDefinitions.Add(constantSpatialChannelInitialConditionDefinition);
                    }
                }
                ChannelInitialConditionDefinitionsDict.Add(branchName, channelInitialConditionDefinition);
            }
        }

        private InitialConditionQuantity GetInitialConditionQuantity(FlowInitialCondition.FlowConditionType initialConditionWaterLevelType)
        {
            switch (initialConditionWaterLevelType)
            {
                case FlowInitialCondition.FlowConditionType.WaterDepth:
                    return InitialConditionQuantity.WaterDepth;
                case FlowInitialCondition.FlowConditionType.WaterLevel:
                    return InitialConditionQuantity.WaterLevel;
                default:
                    throw new InvalidOperationException();
            }
        }

        private void RemoveAndWarnForUnusedDefinitions()
        {
            var keysToRemove = new List<string>();
            foreach (var channelInitialConditionDefinition in ChannelInitialConditionDefinitionsDict.Values)
            {
                InitialConditionQuantity quantity;
                var constantDefinition = channelInitialConditionDefinition.ConstantChannelInitialConditionDefinition;
                var spatialDefinition = channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition;
                if (constantDefinition != null)
                {
                    quantity = constantDefinition.Quantity;
                }
                else if (spatialDefinition != null)
                {
                    quantity = spatialDefinition.Quantity;
                }
                else
                {
                    continue;
                }

                if (quantity != GlobalQuantity)
                {
                    var channelName = channelInitialConditionDefinition.Channel.Name;
                    keysToRemove.Add(channelName);
                }
            }

            foreach (var keyToRemove in keysToRemove)
            {
                ChannelInitialConditionDefinitionsDict.Remove(keyToRemove);
                Log.WarnFormat($"Initial condition definition for '{keyToRemove}' does match the global quantity {GlobalQuantity}. Skipping import.");
            }
        }
    }
}