using System.Collections.Generic;
using System.Data;
using System.Linq;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Extensions.Networks;
using log4net;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    /// <summary>
    /// Class builds initial depth coverage based on the following data:
    /// 1) Initial conditions read from initial.dat
    /// 2) List of crosssection needed to convert from depth to level and vice versa
    /// </summary>
    public class InitialConditionsBuilder
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(InitialConditionsBuilder));

        private IList<FlowInitialCondition> flowInitialConditions;
        
        private readonly IHydroNetwork network;
        private double globalFlow;
        private double globalInitialDepth;
        private double globalInitialWaterLevel;
        private INetworkCoverage bedLevelNetworkCoverage;
        private bool waterLevelIsLeading;
        
        public InitialConditionsBuilder(IEnumerable<FlowInitialCondition> flowInitialConditions,
            IHydroNetwork network)
        {
            this.flowInitialConditions = flowInitialConditions.ToList();
            this.network = network;
        }
        
        public double GlobalInitialWaterLevel
        {
            get { return globalInitialWaterLevel; }
        }
        public double GlobalInitialDepth
        {
            get { return globalInitialDepth; }
        }

        public void Build()
        {
            RemoveAndWarnForUnusedConditions();
            
            //define the global definition 
            ReadAndRemoveGlobalDefinition();

            CreateBedLevelCoverage();

            CreateInitialFlowCoverage();

            CreateInitialDepthCoverage();
        }

        private void CreateInitialDepthCoverage()
        {
            var interpolationHasBeenSet = false;

            InitialDepth = new NetworkCoverage
                               {
                                   Network = network, 
                                   DefaultValue = globalInitialDepth
                               };

            var bedLevelCoverage = new NetworkCoverage("bed level coverage for branch", false)
                {
                    Network = bedLevelNetworkCoverage.Network,
                };
            bedLevelCoverage.Locations.InterpolationType = InterpolationType.Linear;
            bedLevelCoverage.Locations.SetValues(bedLevelNetworkCoverage.Locations.Values); 
            bedLevelCoverage.Components[0].SetValues(bedLevelNetworkCoverage.Locations.Values.Select(bedLevelNetworkCoverage.Evaluate));

            foreach (var branch in network.Branches)
            {
                var bedLevelLocations = bedLevelCoverage.GetLocationsForBranch(branch);

                var firstLevelBoundaryCondition = flowInitialConditions.FirstOrDefault(c => c.IsLevelBoundary && c.BranchID == branch.Name.ToString());
                
                if (firstLevelBoundaryCondition != null)
                {
                    if (firstLevelBoundaryCondition.WaterLevelType == FlowInitialCondition.FlowConditionType.WaterDepth)
                    {
                        AddInitialConditionToNetworkCoverage(firstLevelBoundaryCondition.Level, InitialDepth, branch);
                    }
                    else
                    {
                        //no crossection could be found for the current branch so no conversion 
                        //to depth can be made...let's skip this condition)
                        if (!bedLevelLocations.Any())
                        {
                            log.WarnFormat("Initial condition with ID {0} skipped because no crossections are defined on branch {1} and no conversion to depth could be made",
                                                            firstLevelBoundaryCondition.ID, firstLevelBoundaryCondition.BranchID);
                            continue;//next condition
                        }

                        InitialCondition level = firstLevelBoundaryCondition.Level;
                        var waterLevelLocations = level.IsConstant
                                            ? new INetworkLocation[] {new NetworkLocation(branch, branch.Length/2)}
                                            : level.Data.Rows.OfType<DataRow>().Select(
                                                dr => new NetworkLocation(branch, (double) dr[0])).OfType<INetworkLocation>();

                        var waterLevelValues = level.IsConstant ? new[] { level.Constant } : level.Data.Rows.OfType<DataRow>().Select(row => (double)row[1]).ToArray();

                        var allLocations = waterLevelLocations.Concat(bedLevelLocations).Distinct();

                        var waterLevelForBranch = new NetworkCoverage("water level coverage for branch", false);
                        waterLevelForBranch.Locations.InterpolationType = InterpolationType.Linear;
                        waterLevelForBranch.Network = branch.Network;
                        waterLevelForBranch.Locations.SetValues(waterLevelLocations);
                        waterLevelForBranch.Components[0].SetValues(waterLevelValues);

                        foreach (var location in allLocations)
                        {
                            InitialDepth[location] = waterLevelForBranch.Evaluate(location) -
                                                        bedLevelCoverage.Evaluate(location);
                        }

                        waterLevelForBranch.Clear();
                        waterLevelForBranch.Network = null; // clean up events

                        SetAndCheckInterpolationNetworkCoverage(firstLevelBoundaryCondition.Level, InitialDepth, branch, ref interpolationHasBeenSet);
                    }
                }
                else
                {
                    if (waterLevelIsLeading)
                    {
                        if (!bedLevelLocations.Any())
                        {
                            log.WarnFormat("No crosssections are defined on branch {0}, so no conversion from level to depth could be made", branch.Name);
                            continue;
                        }
                        foreach (var loc in bedLevelLocations)
                        {
                            InitialDepth[loc] = globalInitialWaterLevel - (double)bedLevelCoverage[loc];
                        }
                    }
                }
            }
        }

        private void CreateInitialFlowCoverage()
        {
            var interpolationHasBeenSet = false;

            InitialFlow = new NetworkCoverage
                              {
                                  Network = network, 
                                  DefaultValue = globalFlow
                              };


            //handle Q-boundaries
            foreach (var condition in flowInitialConditions.Where(c=>c.IsQBoundary))
            {
                var branch = network.Branches.FirstOrDefault(b => b.Name.ToString() == condition.BranchID);
                if (branch != null)
                {
                    AddInitialConditionToNetworkCoverage(condition.Discharge, InitialFlow, branch);

                    SetAndCheckInterpolationNetworkCoverage(condition.Discharge, InitialFlow, branch, ref interpolationHasBeenSet);
                }
            }
        }

        private void RemoveAndWarnForUnusedConditions()
        {
            var conditionsWithoutBranch = flowInitialConditions.Where(c=>!c.IsGlobalDefinition) //non globals
                .Where(c => (c.IsLevelBoundary || c.IsQBoundary) && !network.Branches.Any(b => b.Name.ToString() == c.BranchID)).ToList();
            foreach (var condition in conditionsWithoutBranch)
            {
                log.WarnFormat("Channel {0} for initial condition {1} not found; skipped",
                                   condition.BranchID, condition.ID);
            }
            //exclude the 'bad' conditions
            flowInitialConditions = flowInitialConditions.Except(conditionsWithoutBranch).ToList();
        }

        private void CreateBedLevelCoverage()
        {
            bedLevelNetworkCoverage = BedLevelNetworkCoverageBuilder.BuildBedLevelCoverage(network);
        }

        private void ReadAndRemoveGlobalDefinition()
        {
            var globalDefinition = flowInitialConditions.FirstOrDefault(ic => ic.IsGlobalDefinition);
            if (globalDefinition == null)
            {
                log.WarnFormat("globally defined flow conditions are not imported yet");
            }
            else
            {
                if (globalDefinition.WaterLevelType == FlowInitialCondition.FlowConditionType.WaterDepth)
                {
                    globalInitialDepth = globalDefinition.Level.Constant;    
                }
                else
                {
                    waterLevelIsLeading = true;
                    globalInitialWaterLevel = globalDefinition.Level.Constant;
                }
                
                globalFlow = globalDefinition.Discharge.Constant;
                //remove it so we don't parse it later on
                flowInitialConditions.Remove(globalDefinition);
                //TODO: where is the global depth ??? Or how do we know which one was defined..
            }
        }

        public void AddInitialConditionToNetworkCoverage(InitialCondition flowInitialCondition, INetworkCoverage networkCoverage, IBranch branch)
        {
            AddInitialConditionToNetworkCoverage(flowInitialCondition, networkCoverage, branch, 1.0);
        }

        public static void AddInitialConditionToNetworkCoverage(InitialCondition flowInitialCondition, INetworkCoverage networkCoverage, IBranch branch, double correctionFactor)
        {
            if (flowInitialCondition.IsConstant)
            {
                networkCoverage[new NetworkLocation(branch, branch.Length / 2)] = flowInitialCondition.Constant;
            }
            else
            {
                //In sobek you can define two values at one point ( 0 = 5.0, 2500 = 5.0, 2500 = 3.0, 5000 = 3.0)
                //In this case we add 0.01 to the second definition
                var lastOffset = -1.0;
                foreach (DataRow row in flowInitialCondition.Data.Rows)
                {
                    var offset = (double)row[0];
                    if(lastOffset == offset)
                    {
                        offset += 0.01;
                    }
                    lastOffset = offset;
                    var value = (double)row[1] * correctionFactor;
                    networkCoverage[new NetworkLocation(branch, offset)] = value;
                }
            }
        }

        public INetworkCoverage InitialFlow { get; private set; }

        public INetworkCoverage InitialDepth { get; private set; }

        private void SetAndCheckInterpolationNetworkCoverage(InitialCondition initialCondition, INetworkCoverage networkCoverage, IBranch branch, ref bool interpolationHasBeenSet)
        {
            var channel = (IChannel)branch;

            var coverageName = networkCoverage == InitialDepth ? "Initial waterdepth" : "Initial waterlevel";

            if (!interpolationHasBeenSet && initialCondition.Interpolation != InitialCondition.InterpolationNotSetValue)
            {
                networkCoverage.Arguments[0].InterpolationType = initialCondition.Interpolation;
                interpolationHasBeenSet = true;
                log.WarnFormat("Interpolation {0} (of channel {1} ({2})) has been set as network-wide interpolation for '{3}'. Only a single interpolation type for entire network is supported.",
                    initialCondition.Interpolation, channel.Name, channel.LongName, coverageName);
            }

            if (initialCondition.Interpolation != networkCoverage.Arguments[0].InterpolationType &&
                initialCondition.Interpolation != InitialCondition.InterpolationNotSetValue)
            {
                log.WarnFormat("Interpolation {0} of channel {1} ({2}) cannot be set for {3}. Only one interpolation type supported for entire network. Interpolation is {4}.", 
                    initialCondition.Interpolation, channel.Name, channel.LongName, coverageName,
                    networkCoverage.Arguments[0].InterpolationType);
            }
        }


        
    }
}