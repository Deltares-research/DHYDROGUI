using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.Hydro;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Core;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.RgfGrid;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using Rhino.Mocks;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;
using LandBoundary2D = DelftTools.Hydro.LandBoundary2D;
using ObservationCrossSection2D = DelftTools.Hydro.ObservationCrossSection2D;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class WaterFlowFMModelSaveAsToDirectoryTest
    {
        private WaterFlowFMModel model;
        private string saveAsPath;
        private string testdataPath;
        private string mduFilePath;
        [SetUp]
        public void Setup()
        {
             mduFilePath = @"TestPlanFM\modelA.mdu";
             testdataPath = @"TestPlanFM";
             saveAsPath = @"TestPlanFM\SaveAs";
             model = new WaterFlowFMModel(mduFilePath);
        }

        [Test]
        public void GivenANewFmModelWithTrachytopesWithoutMorphologyAndWindWhenUserSavesTheModelAsThenInputFolderIsCreated()
        {
            using (var app = new DeltaShellApplication())
            {

                AddPluginsAndRun(app);
             //   app.SaveProjectAs(mduFilePath); // save to initialize file repository..

                //Create unstructured grid
                var nc_Path = Path.Combine(TestHelper.GetDataDir(), @"TestPlanFM\UGrid\UnstructuredGrid.nc");
                Assert.IsTrue(File.Exists(nc_Path));

                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = nc_Path;
                Assert.IsNotNull(model.NetFilePath);

                // Create land boundaries
                //var ldb_Path = Path.Combine(TestHelper.GetDataDir(), @"TestPlanFM\UGrid\landboundaries");
                //model.ModelDefinition.GetModelProperty(KnownProperties.LandBoundaryFile).Value = ldb_Path;
                var landBoundary = new LandBoundary2D()
                {
                    GroupName = "LandBoundary"
                };

                model.Area.LandBoundaries.Add(landBoundary);

                //Add Dry area, OP,Fixed weir, opcs
                var hydroArea = new HydroArea();
                var polygon = new GroupableFeature2DPolygon()
                {
                    GroupName = "dryArea1",

                };
                hydroArea.DryAreas.Add(polygon);

                var point = new GroupableFeature2DPoint()
                {
                };
                var fixedWeir = new FixedWeir()
                {

                };
                var observationCrossSection = new ObservationCrossSection2D()
                {
                };

                model.Area.DryPoints.Add(new GroupablePointFeature()
                {
                    GroupName = Path.Combine(TestHelper.GetDataDir(), @"TestPlanFM\drypoints.xyz")
                });

                model.Area = hydroArea;
                model.Area.ObservationPoints.Add(point);
                model.Area.FixedWeirs.Add(fixedWeir);
                model.Area.ObservationCrossSections.Add(observationCrossSection);

                Assert.IsNotNull(model.Area.ObservationPoints[0]);
                Assert.IsNotNull(model.Area);
                Assert.IsNotNull(model.Area.FixedWeirs);
                Assert.IsNotNull(model.Area.ObservationCrossSections);

                //Add boundary condition
                var boundary1 = new Feature2D() { Name = "Boundary1" };
                var boundaryConditionSet = new BoundaryConditionSet();
                model.BoundaryConditionSets.Add(boundaryConditionSet);
              
                boundaryConditionSet.BoundaryConditions.Add(
                    new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge, BoundaryConditionDataType.Empty)
                {
                    Feature = boundary1,
                    TracerName = "tracer1"
                });
                
                var boundary2 = new Feature2D() { Name = "Boundary2" };
                var boundaryConditionSet2 = new BoundaryConditionSet();
                model.BoundaryConditionSets.Add(boundaryConditionSet2);
                boundaryConditionSet2.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Empty)
                {
                    Feature = boundary2,
                });

                Assert.IsNotNull(model.BoundaryConditionSets);
                Assert.That(model.BoundaryConditionSets.Count, Is.EqualTo(2));
                Assert.That(model.BoundaryConditionSets[0].BoundaryConditions[0], Is.TypeOf(typeof(FlowBoundaryCondition)));               
                Assert.That(model.BoundaryConditionSets[1].BoundaryConditions[0], Is.TypeOf(typeof(FlowBoundaryCondition)));

                //Add grid, tracers
                var grid =UnstructuredGridFileHelper.LoadFromFile(nc_Path);
                model.Grid = grid;
                Assert.IsNotNull(model.Grid);
              
                var gridCellCoverage = new UnstructuredGridCellCoverage(grid, true);
                model.InitialTracers.Add(gridCellCoverage);
                Assert.IsNotNull(model.InitialTracers);
                Assert.That(model.InitialTracers.Count, Is.EqualTo(1));
                
                //enable salinity, temperature, specify waq interval, starttime,stoptime, enable writesnapped features
                model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).SetValueAsString("true");
                model.ModelDefinition.GetModelProperty(GuiProperties.UseTemperature).SetValueAsString("true");
                model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputInterval).SetValueAsString("true");
                model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStartTime).SetValueAsString("true");
                model.ModelDefinition.GetModelProperty(GuiProperties.SpecifyWaqOutputStopTime).SetValueAsString("true");
                model.ModelDefinition.GetModelProperty(GuiProperties.WriteSnappedFeatures).SetValueAsString("true");
         
                //app.Project.RootFolder.Add(model);
                //app.SaveProjectAs(mduFilePath);
            }
        }

        private static void AddPluginsAndRun(DeltaShellApplication app)
        {
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new FlowFMApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Run();
        }
    }
}
