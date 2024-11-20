using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.IO.DataObjects.InitialConditions;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Creates <see cref="ChannelInitialConditionDefinition"/> from Sobek data.
    /// </summary>
    public class InitialConditionsBuilder
    {
        private const double Tolerance = 0.00001;

        private static readonly ILog Log = LogManager.GetLogger(typeof(InitialConditionsBuilder));

        private readonly ICollection<FlowInitialCondition> flowInitialConditions;
        private readonly IHydroNetwork network;
        private readonly List<string> listOfWarnings = new List<string>();

        public InitialConditionsBuilder(IEnumerable<FlowInitialCondition> flowInitialConditions,
            IHydroNetwork network)
        {
            if (flowInitialConditions == null || network == null) throw new ArgumentNullException();

            this.flowInitialConditions = flowInitialConditions.ToList();
            this.network = network;
            ChannelInitialConditionDefinitionsDict = new Dictionary<string, ChannelInitialConditionDefinition>(StringComparer.InvariantCultureIgnoreCase);
            HasSetGlobals = false;
        }

        public Dictionary<string, ChannelInitialConditionDefinition> ChannelInitialConditionDefinitionsDict { get; private set; }
        public InitialConditionQuantity GlobalQuantity { get; private set; }
        public double GlobalValue { get; private set; }
        public bool HasSetGlobals { get; private set; }
        

        public void Build()
        {
            ProcessGlobalDefinition();

            var branchesDictionary = GenerateBranchesDict();
            if (branchesDictionary == null || branchesDictionary.Count == 0)
            {
                return;
            }

            FilterUnusedFlowInitialConditions(branchesDictionary);
            CreateChannelInitialConditionDefinitions(branchesDictionary);

            FilterInvalidChannelInitialConditionDefinitions();

            if (listOfWarnings.Any())
                Log.Warn($"While importing initial conditions we encountered the following {listOfWarnings.Count} warnings: {Environment.NewLine}{string.Join(Environment.NewLine, listOfWarnings)}");
        }

        private Dictionary<string, IBranch> GenerateBranchesDict()
        {
            return network.Branches?.ToDictionary(branch => branch.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        private void FilterUnusedFlowInitialConditions(IDictionary<string, IBranch> branchesDictionary)
        {
            var conditionsWithoutBranch = flowInitialConditions.Where(c => IsUnusedCondition(c, branchesDictionary)).ToArray();
            string unfoundConditions = string.Join(Environment.NewLine, conditionsWithoutBranch.Select(c => $"(branch \"{c.BranchID}\", cond. id \"{c.ID}\")"));

            if (unfoundConditions.Any())
            {
                Log.Warn($"For the following initial conditions the channels where not found; skipped {Environment.NewLine + unfoundConditions}");
            }

            conditionsWithoutBranch.ForEach(c => flowInitialConditions.Remove(c));
        }

        private static bool IsUnusedCondition(FlowInitialCondition initialCondition, IDictionary<string, IBranch> branchesDictionary)
        {
            return !initialCondition.IsGlobalDefinition
                   && ((initialCondition.IsLevelBoundary || initialCondition.IsQBoundary) && !branchesDictionary.ContainsKey(initialCondition.BranchID));
        }

        /// <summary>
        /// Processes the initial conditions to determine if there are global definitions present.
        /// </summary>
        private void ProcessGlobalDefinition()
        {
            var globalDefinition = flowInitialConditions.FirstOrDefault(ic => ic.IsGlobalDefinition);
            if (globalDefinition == null)
            {
                Log.WarnFormat("Globally defined flow conditions are not imported yet.");
                return;
            }

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

            HasSetGlobals = true;

            // Remove it so we don't parse it later on
            flowInitialConditions.Remove(globalDefinition);
        }

        private void CreateChannelInitialConditionDefinitions(IDictionary<string, IBranch> branchesDictionary)
        {
            foreach (var initialCondition in flowInitialConditions)
            {
                if (!initialCondition.IsLevelBoundary)
                {
                    listOfWarnings.Add($"Cannot import {initialCondition.ID}. Only WaterDepth and WaterLevel initial conditions are currently supported.");
                    continue;
                } 
                
                var branchName = initialCondition.BranchID;
                if (!branchesDictionary.ContainsKey(branchName))
                {
                    listOfWarnings.Add($"Could not find branch {branchName}. Skipping import of this initial condition.");
                    continue;
                }

                if (ChannelInitialConditionDefinitionsDict.ContainsKey(branchName))
                {
                    listOfWarnings.Add($"Could not import channel initial conditions again for branch {branchName}, it was already imported. Skipping import of this initial condition.");
                    continue;
                }

                var branch = branchesDictionary[branchName];
                var channel = branch as Channel;
                if (channel == null)
                {
                    listOfWarnings.Add($"Could not import channel initial conditions for branch {branchName}, it is not of type Channel. Skipping import of this initial condition.");
                    continue;
                }

                var channelInitialConditionDefinition = initialCondition.Level.IsConstant 
                    ? CreateConstantChannelInitialConditionDefinition(channel, initialCondition)
                    : CreateSpatialChannelInitialConditionDefinition(channel, initialCondition);

                ChannelInitialConditionDefinitionsDict.Add(branchName, channelInitialConditionDefinition);
            }
        }

        private ChannelInitialConditionDefinition CreateConstantChannelInitialConditionDefinition(Channel channel,
            FlowInitialCondition initialCondition)
        {
            return new ChannelInitialConditionDefinition(channel)
            {
                SpecificationType = ChannelInitialConditionSpecificationType.ConstantChannelInitialConditionDefinition,
                ConstantChannelInitialConditionDefinition =
                {
                    Quantity = ConvertInitialConditionQuantity(initialCondition.WaterLevelType),
                    Value = initialCondition.Level.Constant
                }
            };
        }

        private ChannelInitialConditionDefinition CreateSpatialChannelInitialConditionDefinition(Channel channel,
            FlowInitialCondition initialCondition)
        {
            var channelInitialConditionDefinition = new ChannelInitialConditionDefinition(channel);
            channelInitialConditionDefinition.SpecificationType = ChannelInitialConditionSpecificationType.SpatialChannelInitialConditionDefinition;
            channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition.Quantity = ConvertInitialConditionQuantity(initialCondition.WaterLevelType);

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
                var constantSpatialChannelInitialConditionDefinition =
                    new ConstantSpatialChannelInitialConditionDefinition
                    {
                        Chainage = offset,
                        Value = value
                    };
                channelInitialConditionDefinition.SpatialChannelInitialConditionDefinition
                    .ConstantSpatialChannelInitialConditionDefinitions
                    .Add(constantSpatialChannelInitialConditionDefinition);
            }

            return channelInitialConditionDefinition;
        }

        private InitialConditionQuantity ConvertInitialConditionQuantity(FlowInitialCondition.FlowConditionType initialConditionWaterLevelType)
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

        private void FilterInvalidChannelInitialConditionDefinitions()
        {
            var keysToRemove = new List<string>();
            var nonMatchingDefinitionsWarnings = new List<string>();
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
                nonMatchingDefinitionsWarnings.Add($"definition \"{keyToRemove}\" - global \"{GlobalQuantity}\"");
            }

            if (nonMatchingDefinitionsWarnings.Any())
            {
                Log.Warn($"Initial condition definition does match the global quantity. Skipping import." + Environment.NewLine + string.Join(Environment.NewLine, nonMatchingDefinitionsWarnings));
            }
        }
    }
}