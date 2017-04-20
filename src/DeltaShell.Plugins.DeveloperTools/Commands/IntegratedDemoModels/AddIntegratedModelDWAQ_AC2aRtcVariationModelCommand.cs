using System;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Meteo;
using DeltaShell.Plugins.DelftModels.RealTimeControl;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DeveloperTools.Commands.IntegratedDemoModels
{
    // TODO: Merge duplicate code of DWAQ_AC2a to common class
    public class AddIntegratedModelDWAQ_AC2aRtcVariationModelCommand : Command, IGuiCommand
    {
        private const string HydroNode1Name = "1";
        private const string HydroNode8Name = "8";
        private const string HydroNode3Name = "3";
        private const string HydroNode9Name = "9";
        private const string HydroNode21Name = "21";
        private const string Channel1Name = "1";
        private const string Channel2Name = "2";
        private const string Channel3Name = "3";
        private const string Channel4Name = "4";
        private const string LateralSourceL1Name = "L1";
        private const string LateralSource7Name = "7";
        private const string nearPipeObservationPointName = "Near pipe";

        protected override void OnExecute(params object[] arguments)
        {
            var model = HydroModel.BuildModel(ModelGroup.All);
            model.Name = "RR-FLOW-RTC demo model";

            var modelsToRemove = model.Activities.Where(m => !(m is WaterFlowModel1D || m is RainfallRunoffModel || m is RealTimeControlModel)).ToList();
            foreach (var modelToRemove in modelsToRemove)
            {
                model.Activities.Remove(modelToRemove);
            }

            var flowModel1D = model.Activities.OfType<WaterFlowModel1D>().First();
            var rrModel1D = model.Activities.OfType<RainfallRunoffModel>().First();
            var rtcModel = model.Activities.OfType<RealTimeControlModel>().First();

            // Setup Network:
            var hydroNetwork = model.Region.SubRegions.OfType<HydroNetwork>().First();
            var drainageBasin = model.Region.SubRegions.OfType<DrainageBasin>().First();
            ConfigureHydroNetwork(hydroNetwork);
            ConfigureBasin(drainageBasin);
            ConfigureHydroRegion(hydroNetwork, drainageBasin);

            ConfigureHydroModel(model);
            ConfigureFlowModel(flowModel1D);
            ConfigureRainfallRunoffModel(rrModel1D);
            ConfigureRtcModel(rtcModel);

            var gui = DeveloperToolsGuiPlugin.Instance.Gui;
            var app = gui.Application;
            if (app.Project == null)
            {
                app.CreateNewProject();
            }
            app.Project.RootFolder.Add(model);
        }

        private void ConfigureRtcModel(RealTimeControlModel rtcModel)
        {
            #region Create Control Group: Water level controller

            var nearPipeWaterLevel = new Input();
            var weir6CrestLevel = new Output();

            #region Setup PID Rule:

            var pidWaterLevelController = new PIDRule
            {
                Name = "Weir controller",
                LongName = "Steers weir 6 such that water level at 'near pipe' does not go over 0",
                PidRuleSetpointType = PIDRule.PIDRuleSetpointType.Constant,
                ConstantValue = 0.0,
                Kp = 1.0,
                Ki = 0.0,
                Kd = 0.0,
                Setting = { Max = 1.0, MaxSpeed = 1.0, Min = -0.9 }
            };
            pidWaterLevelController.Inputs.Add(nearPipeWaterLevel);
            pidWaterLevelController.Outputs.Add(weir6CrestLevel);

            #endregion

            #region Setup Standard Condition:

            var hydroCondition = new StandardCondition
            {
                Name = "Weir controller activator",
                LongName = "Activates weir controller in case water level at 'near pipe' is (about to go) above 0",
                Operation = Operation.Greater,
                Value = -0.05,
                Input = nearPipeWaterLevel,
            };
            hydroCondition.TrueOutputs.Add(pidWaterLevelController);

            #endregion

            #region Setup Control Group:

            var controlGroup = new ControlGroup { Name = "Water level controller" };
            controlGroup.Inputs.Add(nearPipeWaterLevel);
            controlGroup.Outputs.Add(weir6CrestLevel);
            controlGroup.Rules.Add(pidWaterLevelController);
            controlGroup.Conditions.Add(hydroCondition);
            rtcModel.ControlGroups.Add(controlGroup);

            #endregion

            #region Link Input/Output:

            var flowModel = rtcModel.ControlledModels.OfType<WaterFlowModel1D>().First();

            var nearPipeObservationPoint =
                flowModel.Network.ObservationPoints.First(op => op.Name == nearPipeObservationPointName);
            var nearPipeObservationDataItem = flowModel.GetChildDataItems(nearPipeObservationPoint)
                     .First(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            var inputNearPipeWaterLevelDataItem = rtcModel.GetDataItemByValue(nearPipeWaterLevel);
            inputNearPipeWaterLevelDataItem.LinkTo(nearPipeObservationDataItem);

            var weir6 = flowModel.Network.Weirs.First();
            var weir6CrestLevelDataItem =
                flowModel.GetChildDataItems(weir6).First(di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            var outputWeir6CrestLevelDataItem = rtcModel.GetDataItemByValue(weir6CrestLevel);
            weir6CrestLevelDataItem.LinkTo(outputWeir6CrestLevelDataItem);

            #endregion

            #endregion
        }

        private void ConfigureHydroRegion(HydroNetwork network, DrainageBasin basin)
        {
            // Link Catchment with lateral Source '7'
            var lateral = network.LateralSources.First(ls => ls.Name == LateralSource7Name);
            var catchment = basin.Catchments.First();
            basin.AddNewLink(catchment, lateral);
            catchment.Links[0].Geometry = new LineString(new[] { catchment.InteriorPoint.Coordinate, lateral.Geometry.Coordinate });
        }

        private void ConfigureHydroNetwork(HydroNetwork hydroNetwork)
        {
            #region Define HydroNodes:

            const string sobekNodeTypeAttribute = "Sobek type";
            var node1 = new HydroNode(HydroNode1Name) { Geometry = new Point(0.0, 0.0) };
            var node8 = new HydroNode(HydroNode8Name) { Geometry = new Point(294.0, -2.0) };
            var node3 = new HydroNode(HydroNode3Name) { Geometry = new Point(969.0, 0.0) };
            var node9 = new HydroNode(HydroNode9Name) { Geometry = new Point(305.0, 175.0) };
            var node21 = new HydroNode(HydroNode21Name) { Geometry = new Point(2000.0, 0.0) };

            node1.Attributes.Add(sobekNodeTypeAttribute, "Upstream Node");
            node8.Attributes.Add(sobekNodeTypeAttribute, "Flow - Connection Node");
            node3.Attributes.Add(sobekNodeTypeAttribute, "Flow - Connection Node");
            node9.Attributes.Add(sobekNodeTypeAttribute, "Flush");
            node21.Attributes.Add(sobekNodeTypeAttribute, "Flow - Boundary");

            hydroNetwork.Nodes.AddRange(new[] { node1, node8, node3, node9, node21 });

            #endregion

            #region Define Branches:

            const string sobekBranchTypeAttribute = "Sobek type";
            const string surfaceWaterTypeAttribute = "Surface water type";

            var branch1 = new Channel(node1, node8, 294.33)
            {
                Name = Channel1Name,
                Geometry = new LineString(new[] { node1.Geometry.Coordinate, node8.Geometry.Coordinate })
            };
            var branch2 = new Channel(node3, node21, 294.33)
            {
                Name = Channel2Name,
                Geometry = new LineString(new[] { node3.Geometry.Coordinate, node21.Geometry.Coordinate })
            };
            var branch3 = new Channel(node8, node3, 294.33)
            {
                Name = Channel3Name,
                Geometry = new LineString(new[] { node8.Geometry.Coordinate, node3.Geometry.Coordinate })
            };
            var branch4 = new Channel(node9, node8, 294.33)
            {
                Name = Channel4Name,
                Geometry = new LineString(new[] { node9.Geometry.Coordinate, node8.Geometry.Coordinate })
            };

            branch1.Attributes.Add(sobekBranchTypeAttribute, "Flow - Channel");
            branch2.Attributes.Add(sobekBranchTypeAttribute, "SWTType2");
            branch3.Attributes.Add(sobekBranchTypeAttribute, "Flow - Channel");
            branch4.Attributes.Add(sobekBranchTypeAttribute, "Flow - Channel");

            branch1.Attributes.Add(surfaceWaterTypeAttribute, "Normal");
            branch2.Attributes.Add(surfaceWaterTypeAttribute, "SWTType2");
            branch3.Attributes.Add(surfaceWaterTypeAttribute, "Normal");
            branch4.Attributes.Add(surfaceWaterTypeAttribute, "Normal");

            hydroNetwork.Branches.AddRange(new[] { branch1, branch2, branch3, branch4 });

            #endregion

            #region Define Cross-Sections:

            var yzCrossSectionDefinition = new CrossSectionDefinitionYZ("test");
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(0.0, 1.0, 0.0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(4.0, -1.0, 0.0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(5.0, -1.0, 0.0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(9.0, 1.0, 0.0);
            yzCrossSectionDefinition.Thalweg = 4.5;
            hydroNetwork.SharedCrossSectionDefinitions.Add(yzCrossSectionDefinition);

            var crossSection10 = new CrossSectionDefinitionProxy(yzCrossSectionDefinition);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, crossSection10, 67.37).Name = "10";

            var crossSection4 = new CrossSectionDefinitionProxy(yzCrossSectionDefinition);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch2, crossSection4, 128.92).Name = "4";

            var crossSection2 = new CrossSectionDefinitionProxy(yzCrossSectionDefinition);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch3, crossSection2, 431.03).Name = "2";

            var crossSection11 = new CrossSectionDefinitionProxy(yzCrossSectionDefinition);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch4, crossSection11, 79.56).Name = "11";

            #endregion

            #region Define Pumps:

            var compositeStructure12 = new CompositeBranchStructure("12 [compound]", 120.09)
            {
                Geometry = new Point(296.11, 53.29)
            };

            var pump12 = new Pump("12")
            {
                DirectionIsPositive = true,
                Capacity = 0.0,
                OffsetY = 0.0,
                StartSuction = 0.8,
                StopSuction = 0.6,
                StartDelivery = 0.0,
                StopDelivery = 0.0,
                ControlDirection = PumpControlDirection.SuctionSideControl,
                Geometry = new Point(296.11, 53.29),
                ParentStructure = compositeStructure12
            };
            pump12.ReductionTable[0.0] = 1.0;
            compositeStructure12.Structures.Add(pump12);

            NetworkHelper.AddBranchFeatureToBranch(compositeStructure12, branch4, 120.09);
            NetworkHelper.AddBranchFeatureToBranch(pump12, branch4, 120.09);

            #endregion

            #region Define Weirs:
            var compositeStructure6 = new CompositeBranchStructure("6 [compound]", 251.40)
            {
                Geometry = new Point(branch1.Length + branch3.Length + 251.40, 0.0)
            };

            var weir6 = new Weir("6")
            {
                WeirFormula = new SimpleWeirFormula
                {
                    DischargeCoefficient = 1.0,
                    LateralContraction = 1.0,
                },
                CrestShape = CrestShape.Sharp,
                CrestLevel = 0.0,
                CrestWidth = 3.0,
                OffsetY = 3.0,
                AllowPositiveFlow = true,
                AllowNegativeFlow = true,
                FlowDirection = FlowDirection.Both,
                Geometry = new Point(branch1.Length + branch3.Length + 251.40, 0.0),
                ParentStructure = compositeStructure6
            };
            compositeStructure6.Structures.Add(weir6);

            NetworkHelper.AddBranchFeatureToBranch(compositeStructure6, branch2, 251.40);
            NetworkHelper.AddBranchFeatureToBranch(weir6, branch2, 251.40);

            #endregion

            #region Define Lateral Sources:

            const string sobekLateralSourceTypeAttribute = "Sobek type";
            const double lateralL1Chainage = 155.68;
            var lateralSourceL1 = new LateralSource { Name = LateralSourceL1Name, Description = "Overstort", Geometry = new Point(branch1.Length + lateralL1Chainage, 0.0) };
            NetworkHelper.AddBranchFeatureToBranch(lateralSourceL1, branch3, lateralL1Chainage);

            const double lateral7Chainage = 614.79;
            var lateralSource7 = new LateralSource { Name = LateralSource7Name, Geometry = new Point(branch1.Length + lateral7Chainage, 0.0) };
            NetworkHelper.AddBranchFeatureToBranch(lateralSource7, branch3, lateral7Chainage);

            lateralSourceL1.Attributes.Add(sobekLateralSourceTypeAttribute, "CSO");
            lateralSource7.Attributes.Add(sobekLateralSourceTypeAttribute, "");

            #endregion

            #region Define Observation Points:

            const double observationPointStart = 150;
            const double observationPointFarField = 880.62;
            const double observationPointUpstreamPipe = 55.68;
            const double observationPointNearPipe = 225.67;

            var obsStart = new ObservationPoint { Name = "Start", Description = "O1", Geometry = new Point(observationPointStart, 0.0) };
            var obsFarField = new ObservationPoint { Name = "Far field", Description = "O4", Geometry = new Point(branch1.Length + branch3.Length + observationPointFarField, 0.0) };
            var obsUpstreamPipe = new ObservationPoint { Name = "Upstream pipe", Description = "O2", Geometry = new Point(branch1.Length + observationPointUpstreamPipe, 0.0) };
            var obsNearPipe = new ObservationPoint { Name = nearPipeObservationPointName, Description = "O3", Geometry = new Point(branch1.Length + observationPointNearPipe, 0.0) };

            NetworkHelper.AddBranchFeatureToBranch(obsStart, branch1, observationPointStart);
            NetworkHelper.AddBranchFeatureToBranch(obsFarField, branch2, observationPointFarField);
            NetworkHelper.AddBranchFeatureToBranch(obsUpstreamPipe, branch3, observationPointUpstreamPipe);
            NetworkHelper.AddBranchFeatureToBranch(obsNearPipe, branch3, observationPointNearPipe);

            #endregion
        }

        private void ConfigureBasin(DrainageBasin drainageBasin)
        {
            #region Define Catchments:

            var unpavedCatchment = Catchment.CreateDefault();
            unpavedCatchment.Geometry = new Point(905, 160);
            unpavedCatchment.SetAreaSize(1E+05);
            unpavedCatchment.Name = "5";
            unpavedCatchment.CatchmentType = CatchmentType.Unpaved;

            drainageBasin.Catchments.Add(unpavedCatchment);

            #endregion
        }

        private void ConfigureHydroModel(HydroModel model)
        {
            #region Set Timers:

            model.OverrideStartTime = true;
            model.StartTime = new DateTime(2010, 1, 1, 0, 0, 0);

            model.OverrideStopTime = true;
            model.StopTime = new DateTime(2010, 1, 7, 1, 0, 0);

            model.OverrideTimeStep = true;
            model.TimeStep = new TimeSpan(0, 0, 10, 0);

            #endregion
        }

        private void ConfigureFlowModel(WaterFlowModel1D flowModel1D)
        {
            // Discretization:
            flowModel1D.NetworkDiscretization.BeginEdit(new DefaultEditAction("Generating network discretization"));
            HydroNetworkHelper.GenerateDiscretization(flowModel1D.NetworkDiscretization, true, false, 100.0, false, 100.0,
                                                      false, false, true, 100.0);
            flowModel1D.NetworkDiscretization.EndEdit();

            #region Boundary Conditions:

            var node1BoundaryConditions =
                    flowModel1D.BoundaryConditions.First(
                        bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == HydroNode1Name));
            node1BoundaryConditions.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelTimeSeries;
            node1BoundaryConditions.Data[new DateTime(2010, 1, 1, 0, 0, 0)] = 0.01;
            node1BoundaryConditions.Data[new DateTime(2010, 1, 2, 0, 0, 0)] = 0.02;
            node1BoundaryConditions.Data[new DateTime(2010, 1, 3, 0, 0, 0)] = 0.05;
            node1BoundaryConditions.Data[new DateTime(2010, 1, 7, 0, 0, 0)] = 0.01;
            node1BoundaryConditions.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
            node1BoundaryConditions.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;

            var node9BoundaryConditions =
                flowModel1D.BoundaryConditions.First(
                    bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == HydroNode9Name));
            node9BoundaryConditions.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            node9BoundaryConditions.WaterLevel = 0.0;

            var node21BoundaryConditions =
                flowModel1D.BoundaryConditions.First(
                    bc => bc.Feature == flowModel1D.Network.Nodes.First(n => n.Name == HydroNode21Name));
            node21BoundaryConditions.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            node21BoundaryConditions.WaterLevel = -0.5;

            #endregion

            #region Lateral Data:

            var lateralDataL1 =
                    flowModel1D.LateralSourceData.First(
                        lsd => ReferenceEquals(lsd.Feature, flowModel1D.Network.LateralSources.First(ls => ls.Name == LateralSourceL1Name)));
            lateralDataL1.DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
            lateralDataL1.Data.Arguments[0].ExtrapolationType = ExtrapolationType.None;
            lateralDataL1.Data.Arguments[0].InterpolationType = InterpolationType.Constant;
            lateralDataL1.Data[new DateTime(2010, 1, 1, 0, 0, 0)] = 0.0;
            lateralDataL1.Data[new DateTime(2010, 1, 3, 0, 0, 0)] = 0.3;
            lateralDataL1.Data[new DateTime(2010, 1, 3, 3, 0, 0)] = 0.0;
            lateralDataL1.Data[new DateTime(2010, 1, 7, 0, 0, 0)] = 0.0;

            var lateralData7 =
                flowModel1D.LateralSourceData.First(
                    lsd => ReferenceEquals(lsd.Feature, flowModel1D.Network.LateralSources.First(ls => ls.Name == LateralSource7Name)));
            lateralData7.DataType = WaterFlowModel1DLateralDataType.FlowTimeSeries;
            lateralData7.Data.Arguments[0].ExtrapolationType = ExtrapolationType.Constant;
            lateralData7.Data.Arguments[0].InterpolationType = InterpolationType.Linear;
            lateralData7.Data.Clear();

            #endregion

            #region Roughness:

            var roughnessNetworkCoverage = flowModel1D.RoughnessSections[0].RoughnessNetworkCoverage;
            roughnessNetworkCoverage.DefaultRoughnessType = RoughnessType.Chezy;
            roughnessNetworkCoverage.DefaultValue = 45.0;
            roughnessNetworkCoverage[new NetworkLocation(flowModel1D.Network.Branches.First(b => b.Name == Channel1Name), 67.368)] =
                new object[] { 35.0, RoughnessType.Chezy };
            roughnessNetworkCoverage[new NetworkLocation(flowModel1D.Network.Branches.First(b => b.Name == Channel2Name), 128.92)] =
                new object[] { 35.0, RoughnessType.Chezy };
            roughnessNetworkCoverage[new NetworkLocation(flowModel1D.Network.Branches.First(b => b.Name == Channel3Name), 431.03)] =
                new object[] { 35.0, RoughnessType.Chezy };
            roughnessNetworkCoverage[new NetworkLocation(flowModel1D.Network.Branches.First(b => b.Name == Channel4Name), 79.56)] =
                new object[] { 35.0, RoughnessType.Chezy };

            #endregion

            // No Initial Conditions.

            #region Model Configuration:

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

            #endregion

            // Use default model output settings.
        }

        private void ConfigureRainfallRunoffModel(RainfallRunoffModel rrModel1D)
        {
            #region Precipitation:

            rrModel1D.Precipitation.DataDistributionType = MeteoDataDistributionType.Global;
            var time = rrModel1D.StartTime;
            var rainingStart = new DateTime(2010, 1, 1, 7, 0, 0);
            var rainingStop = new DateTime(2010, 1, 2, 5, 0, 0);
            while (time < rrModel1D.StopTime)
            {
                if (time >= rainingStart && time <= rainingStop)
                {
                    rrModel1D.Precipitation.Data[time] = 3.0;
                }
                else
                {
                    rrModel1D.Precipitation.Data[time] = 0.0;
                }
                time += new TimeSpan(0, 1, 0, 0);
            }

            #endregion

            #region Evaporation:

            rrModel1D.Evaporation.DataDistributionType = MeteoDataDistributionType.Global;
            time = rrModel1D.StartTime;
            while (time <= rrModel1D.StopTime)
            {
                rrModel1D.Evaporation.Data[time] = 0.0;
                time += new TimeSpan(1, 0, 0, 0);
            }

            #endregion

            #region Unpaved Catchment configuration:

            var unpavedModelData = (UnpavedData)rrModel1D.GetCatchmentModelData(rrModel1D.Basin.Catchments[0]);
            unpavedModelData.InitialLandStorage = 0.0;
            unpavedModelData.InitialGroundWaterLevelConstant = -1.0;

            // Crops:
            foreach (var cropKVP in unpavedModelData.AreaPerCrop)
            {
                if (cropKVP.Key == UnpavedEnums.CropType.Grass)
                {
                    unpavedModelData.AreaPerCrop[cropKVP.Key] = 1E+05;
                }
                else
                {
                    unpavedModelData.AreaPerCrop[cropKVP.Key] = 0.0;
                }
            }
            unpavedModelData.UseDifferentAreaForGroundWaterCalculations = true;
            unpavedModelData.CalculationArea = 1E+05;

            // Surface & Soil:
            unpavedModelData.SurfaceLevel = 0.5;
            unpavedModelData.SoilType = UnpavedEnums.SoilType.sand_maximum;
            unpavedModelData.SoilTypeCapsim = UnpavedEnums.SoilTypeCapsim.soiltype_capsim_1;

            // Groundwater:
            unpavedModelData.GroundWaterLayerThickness = 5.0;
            unpavedModelData.MaximumAllowedGroundWaterLevel = 1.5;
            unpavedModelData.InitialGroundWaterLevelSource = UnpavedEnums.GroundWaterSourceType.FromLinkedNode;

            // Storage & Infiltration:
            unpavedModelData.InfiltrationCapacityUnit = RainfallRunoffEnums.RainfallCapacityUnit.mm_hr;
            unpavedModelData.InfiltrationCapacity = 5;
            unpavedModelData.MaximumLandStorage = 0.0;
            unpavedModelData.InitialLandStorage = 0.0;
            unpavedModelData.LandStorageUnit = RainfallRunoffEnums.StorageUnit.mm;

            // Drainage:
            unpavedModelData.SwitchDrainageFormula<DeZeeuwHellingaDrainageFormula>();
            var formula = (DeZeeuwHellingaDrainageFormula)unpavedModelData.DrainageFormula;
            formula.SurfaceRunoff = 100;
            formula.LevelOneEnabled = true;
            formula.LevelOneTo = 0.0;
            formula.LevelOneValue = 0.0;
            formula.LevelTwoEnabled = true;
            formula.LevelTwoTo = 0.0;
            formula.LevelTwoValue = 0.0;
            formula.LevelThreeEnabled = true;
            formula.LevelThreeTo = 0.0;
            formula.LevelThreeValue = 0;
            formula.InfiniteDrainageLevelRunoff = 0.3;
            formula.HorizontalInflow = 0.05;

            // Seepage:
            unpavedModelData.SeepageSource = UnpavedEnums.SeepageSourceType.Constant;
            unpavedModelData.SeepageConstant = 2.0;

            #endregion
        }

        public override bool Enabled {get { return true; }}

        public IGui Gui { get; set; }
    }
}