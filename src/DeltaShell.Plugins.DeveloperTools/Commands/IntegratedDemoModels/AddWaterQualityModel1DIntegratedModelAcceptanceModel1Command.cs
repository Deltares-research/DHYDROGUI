using System;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DeveloperTools.Commands.IntegratedDemoModels
{
    public class AddWaterQualityModel1DIntegratedModelAcceptanceModel1Command: Command, IGuiCommand
    {
        private const string DownstreamNodeName = "Downstream node";
        private const string UpstreamNodeName = "Upstream node";

        protected override void OnExecute(params object[] arguments)
        {
            var model = HydroModel.BuildModel(ModelGroup.All);
            model.Name = "FLOW demo model";
            // TODO: Remove these removal lines once BuildEmptyModel respects given model names:
            var modelsToRemove = model.Activities.Where(m => !(m is WaterFlowModel1D)).ToList();
            foreach (var modelToRemove in modelsToRemove)
            {
                model.Activities.Remove(modelToRemove);
            }
            
            var flowModel1D = model.Activities.OfType<WaterFlowModel1D>().First();
            
            // Setup Network:
            var hydroNetwork = model.Region.SubRegions.OfType<HydroNetwork>().First();
            ConfigureHydroNetwork(hydroNetwork);

            ConfigureHydroModel(model);
            ConfigureFlowModel(flowModel1D);

            // [David]: Reverse branch such that flow direction is positive and from upstream to downstream
            // Done here, to ensure waq acceptance model 1 code doesn't have to be changed (is under test)
            HydroNetworkHelper.ReverseBranch(hydroNetwork.Branches[0]);
            var n1 = hydroNetwork.Nodes.First(n => n.Name == DownstreamNodeName);
            var n2 = hydroNetwork.Nodes.First(n => n.Name == UpstreamNodeName);
            n1.Name = UpstreamNodeName;
            n2.Name = DownstreamNodeName;

            var gui = DeveloperToolsGuiPlugin.Instance.Gui;
            
            var app = gui.Application;
            if (app.Project == null)
            {
                app.CreateNewProject();
            }
            app.Project.RootFolder.Add(model);
        }

        private void ConfigureFlowModel(WaterFlowModel1D flowModel1D)
        {
            // Discretization:
            HydroNetworkHelper.GenerateDiscretization(flowModel1D.NetworkDiscretization, true, false, 100.0, false, 100.0,
                                                      false, false, true, 100.0);

            // Boundary Conditions:
            var downstreamNodeBoundaryConditions =
                flowModel1D.BoundaryConditions.First(
                    bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == DownstreamNodeName));
            downstreamNodeBoundaryConditions.DataType = WaterFlowModel1DBoundaryNodeDataType.FlowConstant;
            downstreamNodeBoundaryConditions.Flow = 0.01;

            var upstreamNodeBoundaryConditions =
                flowModel1D.BoundaryConditions.First(
                    bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == UpstreamNodeName));
            upstreamNodeBoundaryConditions.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            upstreamNodeBoundaryConditions.WaterLevel = 0.0;

            // Lateral Source data:
            var lateralInflow = flowModel1D.LateralSourceData.First();
            lateralInflow.DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
            lateralInflow.Data.Arguments[0].ExtrapolationType = ExtrapolationType.None;
            lateralInflow.Data.Arguments[0].InterpolationType = InterpolationType.Constant;
            lateralInflow.Data[new DateTime(2010, 1, 1, 0, 0, 0)] = 0.0;
            lateralInflow.Data[new DateTime(2010, 1, 3, 0, 0, 0)] = 0.01;
            lateralInflow.Data[new DateTime(2010, 1, 3, 3, 0, 0)] = 0.0;
            lateralInflow.Data[new DateTime(2010, 1, 7, 0, 0, 0)] = 0.0;
            
            // No Initial Conditions.

            // Roughness:
            var roughnessNetworkCoverage = flowModel1D.RoughnessSections[0].RoughnessNetworkCoverage;
            roughnessNetworkCoverage.DefaultRoughnessType = RoughnessType.Chezy;
            roughnessNetworkCoverage.DefaultValue = 45.0;
            roughnessNetworkCoverage[new NetworkLocation(flowModel1D.Network.Branches[0], 1274.6447)] =
                new object[] {35.0, RoughnessType.Chezy};

            // Model configuration:
            // * Initial Conditions:
            flowModel1D.DefaultInitialDepth = 0.1;
            flowModel1D.DefaultInitialWaterLevel = 0.0;
            flowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;
            flowModel1D.InitialConditions.Clear(); // Remove auto-added location
            // * Misc:
            flowModel1D.UseReverseRoughness = false;
            flowModel1D.UseReverseRoughnessInCalculation = false;
            flowModel1D.UseSalt = false;
            flowModel1D.UseSaltInCalculation = false;
            // * Run parameters:
            flowModel1D.UseRestart = false;
            flowModel1D.UseSaveStateTimeRange = false;
            flowModel1D.WriteRestart = false;

            // Use default model output settings.
        }
        
        private void ConfigureHydroModel(HydroModel model)
        {
            model.OverrideStartTime = true;
            model.StartTime = new DateTime(2010,1,1,0,0,0);

            model.OverrideStopTime = true;
            model.StopTime = new DateTime(2010,1,7,0,0,0);

            model.OverrideTimeStep = true;
            model.TimeStep = new TimeSpan(0,1,0,0);
        }

        private static void ConfigureHydroNetwork(HydroNetwork hydroNetwork)
        {
            const int brach1Length = 2000;

            // Define HydroNodes:
            var downstreamNode = new HydroNode(DownstreamNodeName) {Geometry = new Point(0, 0)};
            var upstreamNode = new HydroNode(UpstreamNodeName) {Geometry = new Point(brach1Length, 0)};
            hydroNetwork.Nodes.AddRange(new[]{downstreamNode, upstreamNode});

            // Define Branches:
            var branch1 = new Channel(upstreamNode, downstreamNode)
                {
                    Name = "1",
                    Geometry = new LineString(new[] { upstreamNode.Geometry.Coordinate, downstreamNode.Geometry.Coordinate })
                };
            hydroNetwork.Branches.Add(branch1);

            // Define Cross-sections:
            var yzCrossSectionDefinition = new CrossSectionDefinitionYZ("2");
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(0, 0, 0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(4, -2, 0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(5, -2, 0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(9, 0, 0);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, yzCrossSectionDefinition, 1274.6447);
            yzCrossSectionDefinition.Thalweg = 4.5;

            // Define Lateral sources:
            const int lateralChainage = 1550;
            var lateralInflow = new LateralSource {Name = "Lateral inflow", Description = "Overstort", Geometry = new Point(brach1Length-lateralChainage,0)};
            NetworkHelper.AddBranchFeatureToBranch(lateralInflow, branch1, lateralChainage);
            
            // Define Observation Points:
            const int o4Chainage = 150;
            const int o3Chainage = 1450;
            const int o2Chainage = 1650;
            const int o1Chainage = 1850;
            var obsFarField = new ObservationPoint { Name = "Far field", Description = "O4", Geometry = new Point(brach1Length - o4Chainage, 0) };
            var obsNearPipe = new ObservationPoint { Name = "Near pipe", Description = "O3", Geometry = new Point(brach1Length - o3Chainage, 0) };
            var obsUpstreamPipe = new ObservationPoint { Name = "Upstream pipe", Description = "O2", Geometry = new Point(brach1Length - o2Chainage, 0) };
            var obsStart = new ObservationPoint { Name = "Start", Description = "O1", Geometry = new Point(brach1Length - o1Chainage, 0) };

            NetworkHelper.AddBranchFeatureToBranch(obsFarField, branch1, o4Chainage);
            NetworkHelper.AddBranchFeatureToBranch(obsNearPipe, branch1, o3Chainage);
            NetworkHelper.AddBranchFeatureToBranch(obsUpstreamPipe, branch1, o2Chainage);
            NetworkHelper.AddBranchFeatureToBranch(obsStart, branch1, o1Chainage);
        }

        public override bool Enabled
        {
            get { return true; }
        }

        public IGui Gui { get; set; }
    }
}