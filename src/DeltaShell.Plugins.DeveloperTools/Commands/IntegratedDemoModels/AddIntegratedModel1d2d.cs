using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.DataObjects;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.FlowFM;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.DeveloperTools.Commands.IntegratedDemoModels
{
    public class AddIntegratedModel1d2d : Command, IGuiCommand
    {
        protected override void OnExecute(params object[] arguments)
        {
            Gui = DeveloperToolsGuiPlugin.Instance.Gui;
            var app = Gui.Application;
            Create1d2dModel(app); 
        }

        public override bool Enabled
        {
            get { return true; }
        }

        public IGui Gui { get; set; }

        // This method is also used in an integration test, which checks successful completion of this activity. 
        public static void Create1d2dModel(IApplication application)
        {
            var model = HydroModel.BuildModel(ModelGroup.OverLandFlow1D2D);

            var flowModel = model.Activities.OfType<WaterFlowModel1D>().First();
            var fmModel = model.Activities.OfType<WaterFlowFMModel>().First();

            // Setup Hydro region
            var hydroNetwork = flowModel.Network; 
            ConfigureHydroNetwork(hydroNetwork);

            var hydroArea = fmModel.Area; 
            ConfigureHydroArea(hydroArea);

            // Setup models
            ConfigureHydroModel(model);
            ConfigureFlowModel(flowModel);
            ConfigureFmModel(fmModel);
            
            if (application.Project == null)
            {
                application.CreateNewProject();
            }
            application.Project.RootFolder.Add(model);

            GenerateGrid(flowModel, fmModel);
        }

        private static void ConfigureHydroNetwork(IHydroNetwork hydroNetwork)
        {
            // Create nodes
            var node1 = new HydroNode("Node01") { Geometry = new Point(0.0, 0.0) };
            var node2 = new HydroNode("Node02") { Geometry = new Point(2000.0, 0.0) };
            hydroNetwork.Nodes.AddRange(new[] { node1, node2 });

            // Create a branch
            const string sobekBranchTypeAttribute = "Sobek type";
            const string surfaceWaterTypeAttribute = "Surface water type";
            var branch1 = new Channel(node1, node2)
            {
                Name = "Channel01",
                Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
            };
            branch1.Attributes.Add(sobekBranchTypeAttribute, "Flow - Channel");
            branch1.Attributes.Add(surfaceWaterTypeAttribute, "Normal");
            hydroNetwork.Branches.AddRange(new[] { branch1 });

            // Define a cross section
            var yzCrossSectionDefinition = new CrossSectionDefinitionYZ("test");
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(0.0, 1.0, 0.0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(5.0, -5.0, 0.0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(15.0, -5.0, 0.0);
            yzCrossSectionDefinition.YZDataTable.AddCrossSectionYZRow(20.0, 1.0, 0.0);
            yzCrossSectionDefinition.Thalweg = 4.5;
            hydroNetwork.SharedCrossSectionDefinitions.Add(yzCrossSectionDefinition);

            var crossSection10 = new CrossSectionDefinitionProxy(yzCrossSectionDefinition);
            HydroNetworkHelper.AddCrossSectionDefinitionToBranch(branch1, crossSection10, 1000.0d).Name = "10";
        }

        private static void ConfigureHydroArea(HydroArea hydroArea)
        {
            var embankment = new Embankment
            {
                Name = "Embankment01",
                Geometry = new LineString(new[]
                {
                new Coordinate(0.0, 100.0, 1.0), 
                new Coordinate(900.0, 100.0, 1.0), 
                new Coordinate(901, 100.0, -0.5), 
                new Coordinate(1099.0, 100.0, -0.5), 
                new Coordinate(1100.0, 100.0, 1.0), 
                new Coordinate(2000.0, 100.0, 1.0), 
                })
            };
            hydroArea.Embankments.Add(embankment);
        }

        private static void ConfigureHydroModel(HydroModel model)
        {
            // Set timers
            model.OverrideStartTime = true;
            model.StartTime = new DateTime(2010, 1, 1, 0, 0, 0);
            model.OverrideStopTime = true;
            model.StopTime = new DateTime(2010, 1, 1, 1, 0, 0);
            model.OverrideTimeStep = true;
            model.TimeStep = new TimeSpan(0, 0, 0, 20);

            // Set the correct workflow. 
            model.CurrentWorkflow = model.Workflows.First(w => w is Iterative1D2DCoupler);
        }

        private static void ConfigureFlowModel(WaterFlowModel1D flowModel1D)
        {
            // Discretization
            flowModel1D.NetworkDiscretization.BeginEdit(new DefaultEditAction("Generating network discretization"));
            HydroNetworkHelper.GenerateDiscretization(flowModel1D.NetworkDiscretization, true, false, 25.0, false, 25.0,
                false, false, true, 25.0);
            flowModel1D.NetworkDiscretization.EndEdit();

            // Boundary
            var boundary = new WaterFlowModel1DBoundaryNodeData();
            boundary.Feature = flowModel1D.Network.Nodes.First();
            boundary.DataType = WaterFlowModel1DBoundaryNodeDataType.WaterLevelConstant;
            boundary.WaterLevel = 0.5;
            flowModel1D.ReplaceBoundaryCondition(boundary);

            // Initial Conditions
            flowModel1D.DefaultInitialDepth = 0.1;
            flowModel1D.DefaultInitialWaterLevel = 0.0;
            flowModel1D.InitialConditionsType = InitialConditionsType.WaterLevel;
            flowModel1D.InitialConditions.Clear(); // Remove auto-added location

            // Misc:
            flowModel1D.UseReverseRoughness = false;
            flowModel1D.UseReverseRoughnessInCalculation = false;
            flowModel1D.UseSalt = false;
            flowModel1D.UseSaltInCalculation = false;

            // Run parameters
            flowModel1D.UseRestart = false;
            flowModel1D.WriteRestart = false;

            flowModel1D.OutputTimeStep = new TimeSpan(0, 0, 0, 20);

            // Use default model output settings.
        }

        private static void ConfigureFmModel(WaterFlowFMModel fmModel)
        {
            fmModel.ModelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.Caption == "Initial water level").Value = -2.0;
            fmModel.OutputTimeStep = new TimeSpan(0, 0, 0, 20);
        }

        private static void GenerateGrid(WaterFlowModel1D flowModel, WaterFlowFMModel fmModel)
        {
            var coordinateList = new List<Coordinate>() {new Coordinate(50, -100), new Coordinate(50, 1000), new Coordinate(1950, 1000), new Coordinate(1950, -100)};
            var closedCoordinates = new CoordinateList(coordinateList);
            closedCoordinates.CloseRing();
            var polygon = new Polygon(new LinearRing(closedCoordinates.ToCoordinateArray()));
            var rgfgridPolygons = GridWizardMapToolHelper.ComputePolygons(flowModel.NetworkDiscretization,
                fmModel.Area.Embankments.Select(e => (Feature2D) e.Clone()).ToList(), polygon, 25.0d, 5.0d);
            RgfGridEditor.OpenGrid(fmModel.NetFilePath, true, rgfgridPolygons, "polygon.pol");
            fmModel.ReloadGrid(false);
        }
    }
}