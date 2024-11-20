using System;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Gui.Forms.ViewManager;
using DeltaShell.NGHS.TestUtils.Builders;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui
{
    [TestFixture, Apartment(ApartmentState.STA)]
    [Category(TestCategory.WindowsForms)]
    public class BoundaryConditionEditorTest
    {
        private static FlowBoundaryCondition CreateBoundaryCondition(FlowBoundaryQuantityType quantity,
                                                                     BoundaryConditionDataType dataType =
                                                                         BoundaryConditionDataType.TimeSeries)
        {
            var feature = new Feature2D
                {
                    Geometry = new LineString(new []
                        {
                            new Coordinate(0, 0), new Coordinate(50, 10),
                            new Coordinate(100, 0), new Coordinate(150, -10)
                        }),
                    Name = "TestFeature"
                };

            var boundaryData = new FlowBoundaryCondition(quantity, dataType) {Feature = feature};

            return boundaryData;
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowNew()
        {
            WindowsFormsTestHelper.ShowModal(new BoundaryConditionEditor());
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithSalinityTimeSeriesDataAndSigmaLayers()
        {
            var bc = CreateBoundaryCondition(FlowBoundaryQuantityType.Salinity);

            bc.AddPoint(0);
            bc.PointDepthLayerDefinitions[0] = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 0);
            var data = bc.GetDataAtPoint(0);

            data[new DateTime(2001, 1, 1)] = new[] { 50.0 };
            data[new DateTime(2001, 1, 2)] = new[] { 80.0 };

            bc.AddPoint(3);
            data = bc.GetDataAtPoint(3);
            data[new DateTime(2001, 1, 1)] = 60.0;
            data[new DateTime(2001, 1, 2)] = 90.0;

            var view = new BoundaryConditionEditor
            {
                ShowSupportPointNames = true,
                BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                Controller = new FlowBoundaryConditionEditorController(),
                Data =
                    new BoundaryConditionSet
                    {
                        Feature = bc.Feature,
                        BoundaryConditions = new EventedList<IBoundaryCondition> { bc }
                    }
            };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithTimeSeriesData()
        {
            var bc = CreateBoundaryCondition(FlowBoundaryQuantityType.WaterLevel);

            bc.AddPoint(0);
            var data = bc.GetDataAtPoint(0);
            data[new DateTime(2001, 1, 1)] = 5.0;
            data[new DateTime(2001, 1, 2)] = 8.0;

            bc.AddPoint(3);
            data = bc.GetDataAtPoint(3);
            data[new DateTime(2001, 1, 1)] = 6.0;
            data[new DateTime(2001, 1, 2)] = 9.0;

            var view = new BoundaryConditionEditor
                {
                    ShowSupportPointNames = true,
                    BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                    BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                    Controller = new FlowBoundaryConditionEditorController(),
                    Data =
                        new BoundaryConditionSet
                            {
                                Feature = bc.Feature,
                                BoundaryConditions = new EventedList<IBoundaryCondition> {bc}
                            }
                };

            WindowsFormsTestHelper.ShowModal(view);
        }
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowInGuiWithImportedDataAndFakeImportActionsAfterActivityRunnedCorrectly()
        {
            //JIRA: FM1D2D-567
            //Problem with the refreshing the 2D boundary condition view upon importing
            //resolved differently in B&O line...

            using (IGui gui = new DHYDROGuiBuilder().WithFlowFM().Build())
            {
                IProjectService projectService = gui.Application.ProjectService;
                
                gui.Run();

                Project project = projectService.CreateProject();

                Action mainWindowShown = delegate
                {
                    //step 1: User adds an empty D - FlowFM
                    using (var model = new WaterFlowFMModel())
                    {
                        project.RootFolder.Add(model);
                        //step 2 : draw a new 2d boundary condition 
                        var feature2D = new Feature2D
                        {
                            Geometry =
                                new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(2, 0) }),
                            Name = "Boundary01"
                        };
                        
                        model.BoundaryConditionSets.Add(new BoundaryConditionSet { Feature = feature2D });

                        //step 3 & 4 : RMB click on boundary01 node within the project tree & import file
                        var fileName = TestHelper.GetTestFilePath("BcFiles\\WaterLevel.bc");
                        var importer = new BcFileImporter();
                        var importedObject = importer.ImportItem(fileName, model.BoundaryConditionSets.FirstOrDefault());
                        
                        //step 4b : import is done in activity runner, these steps are run after import is finished
                        gui.Selection = importedObject;
                        gui.CommandHandler.OpenViewForSelection();
                        Assert.DoesNotThrow(()=>((ViewList)gui.DocumentViews).ResumeAllViewUpdates());
                    }
                };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, mainWindowShown);
            }


            

            

            /*
            var view = new BoundaryConditionEditor
            {
                ShowSupportPointNames = true,
                BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                Controller = new FlowBoundaryConditionEditorController(),
                Data =
                    boundaryConditionSet
            };

            WindowsFormsTestHelper.ShowModal(view);*/

        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithTimeSeriesAndAstroData()
        {
            var bc1 = CreateBoundaryCondition(FlowBoundaryQuantityType.WaterLevel);

            bc1.AddPoint(0);
            var data = bc1.GetDataAtPoint(0);
            data[new DateTime(2000, 1, 1)] = 5.0;
            data[new DateTime(2000, 1, 15)] = 8.0;

            bc1.AddPoint(3);
            data = bc1.GetDataAtPoint(3);
            data[new DateTime(2000, 1, 1)] = 6.0;
            data[new DateTime(2000, 1, 12)] = 9.0;

            var bc2 = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                BoundaryConditionDataType.AstroComponents) {Feature = bc1.Feature};
            
            bc2.AddPoint(0);
            var data2 = bc2.GetDataAtPoint(0);
            data2["M1"] = new[] { 0.5, 0 };
            data2["M2"] = new[] { 0.5, 200 };

            bc2.AddPoint(2);
            data2 = bc2.GetDataAtPoint(2);
            data2["M1"] = new[] { 0.2, 200 };
            data2["M2"] = new[] { 0.8, 0 };

            var view = new BoundaryConditionEditor
                {
                    ShowSupportPointNames = true,
                    BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                    BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                    Controller = new FlowBoundaryConditionEditorController
                        {
                            Model =
                                new WaterFlowFMModel
                                    {
                                        StartTime = new DateTime(2000, 1, 1),
                                        StopTime = new DateTime(2000, 1, 10),
                                    }
                        },
                    Data =
                        new BoundaryConditionSet
                            {
                                Feature = bc1.Feature,
                                BoundaryConditions = new EventedList<IBoundaryCondition> {bc1, bc2}
                            }
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithAstroData()
        {
            var bc = CreateBoundaryCondition(FlowBoundaryQuantityType.NormalVelocity, BoundaryConditionDataType.AstroComponents);
            bc.AddPoint(0);
            var data = bc.GetDataAtPoint(0);

            data["M1"] = new[] {0.5, 0};
            data["M2"] = new[] {0.5, 200};

            bc.AddPoint(2);
            data = bc.GetDataAtPoint(2);
            data["M1"] = new[] {0.2, 200};
            data["M2"] = new[] {0.8, 0};

            var view = new BoundaryConditionEditor
                {
                    ShowSupportPointNames = true,
                    BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                    BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                    Controller =
                        new FlowBoundaryConditionEditorController
                            {
                                Model =
                                    new WaterFlowFMModel
                                        {
                                            StartTime = new DateTime(2000, 1, 1),
                                            StopTime = new DateTime(2000, 1, 10),
                                        }
                            },
                    Data =
                        new BoundaryConditionSet
                            {
                                Feature = bc.Feature,
                                BoundaryConditions = new EventedList<IBoundaryCondition> { bc }
                            },
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithAstroCorrectionData()
        {
            var bc = CreateBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.AstroCorrection);
            bc.AddPoint(0);
            var data = bc.GetDataAtPoint(0);

            data["M1"] = new[] {0.5, 1, 0, 0};
            data["M2"] = new[] {0.5, 1.25, 200, -30};

            bc.AddPoint(2);
            data = bc.GetDataAtPoint(2);
            data["M1"] = new[] {0.2, 0.75, 200, 30};
            data["M2"] = new[] {0.8, 1.11, 0, 0};

            var view = new BoundaryConditionEditor
            {
                ShowSupportPointNames = true,
                BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                Controller =
                    new FlowBoundaryConditionEditorController
                    {
                        Model =
                            new WaterFlowFMModel
                            {
                                StartTime = new DateTime(2000, 1, 1),
                                StopTime = new DateTime(2000, 1, 10),
                            }
                    },
                Data =
                    new BoundaryConditionSet
                    {
                        Feature = bc.Feature,
                        BoundaryConditions = new EventedList<IBoundaryCondition> { bc }
                    },
            };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithAstroDataAndSalinity()
        {
            var waterBc = CreateBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                  BoundaryConditionDataType.AstroComponents);

            waterBc.AddPoint(0);
            var data = waterBc.GetDataAtPoint(0);

            data["M1"] = new[] {0.5, 100};
            data["M2"] = new[] {0.5, 0};
            
            waterBc.AddPoint(2);
            data = waterBc.GetDataAtPoint(2);

            data["M1"] = new[] {0.8, 200};
            data["M2"] = new[] {0.2, 0};

            var saltBc = CreateBoundaryCondition(FlowBoundaryQuantityType.Salinity);

            saltBc.AddPoint(0);
            data = saltBc.GetDataAtPoint(0);
            data[new DateTime(2000, 1, 1)] = 3.0;
            data[new DateTime(2000, 1, 10)] = 4.0;

            saltBc.AddPoint(1);
            data = saltBc.GetDataAtPoint(1);
            data[new DateTime(2000, 1, 1)] = 3.0;
            data[new DateTime(2000, 1, 10)] = 1.0;

            var view = new BoundaryConditionEditor
                {
                    ShowSupportPointNames = true,
                    BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                    BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                    Controller = new FlowBoundaryConditionEditorController
                        {
                            Model =
                                new WaterFlowFMModel
                                    {
                                        StartTime = new DateTime(2000, 1, 1),
                                        StopTime = new DateTime(2000, 1, 10),
                                    }
                        },
                    Data =
                        new BoundaryConditionSet
                            {
                                Feature = waterBc.Feature,
                                BoundaryConditions = new EventedList<IBoundaryCondition> {waterBc, saltBc}
                            }
                };

            ((FlowBoundaryConditionEditorController) view.Controller).Model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithAstroVectorVelocitiesAndLayers()
        {
            var vectorBc = CreateBoundaryCondition(FlowBoundaryQuantityType.VelocityVector,
                                                   BoundaryConditionDataType.AstroComponents);

            vectorBc.AddPoint(0);

            var data = vectorBc.GetDataAtPoint(0);
            Assert.AreEqual(4, data.Components.Count);

            data["M1"] = new[] {0.1, 100, 0.2, 200};
            data["M2"] = new[] {0.3, 300, 0.4, 400};

            var view = new BoundaryConditionEditor
                {
                    ShowSupportPointNames = true,
                    BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                    BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                    Controller = new FlowBoundaryConditionEditorController()
                        {
                            Model =
                                new WaterFlowFMModel
                                    {
                                        StartTime = new DateTime(2000, 1, 1),
                                        StopTime = new DateTime(2000, 1, 10),
                                    }
                        },
                    Data =
                        new BoundaryConditionSet
                            {
                                Feature = vectorBc.Feature,
                                BoundaryConditions = new EventedList<IBoundaryCondition> {vectorBc}
                            },
                };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithQhDataType()
        {
            var waterLeveBc = CreateBoundaryCondition(FlowBoundaryQuantityType.WaterLevel, BoundaryConditionDataType.Qh);

            waterLeveBc.AddPoint(0);

            var data = waterLeveBc.GetDataAtPoint(0);
            
            data[0.0] = 0.0;
            data[1.0] = 1000.0;
            data[2.0] = 4000.0;
            data[3.0] = 9000.0;
            
            var view = new BoundaryConditionEditor
            {
                ShowSupportPointNames = true,
                BoundaryConditionFactory = new FlowBoundaryConditionFactory(),
                BoundaryConditionPropertiesControl = new FlowBoundaryConditionPropertiesControl(),
                Controller = new FlowBoundaryConditionEditorController()
                {
                    Model =
                        new WaterFlowFMModel
                        {
                            StartTime = new DateTime(2000, 1, 1),
                            StopTime = new DateTime(2000, 1, 10),
                        }
                },
                Data =
                    new BoundaryConditionSet
                    {
                        Feature = waterLeveBc.Feature,
                        BoundaryConditions = new EventedList<IBoundaryCondition> { waterLeveBc }
                    },
            };

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Slow)]
        public void ShowBoundaryConditionsFromGui()
        {
            var mduPath = TestHelper.GetTestFilePath(@"roughness\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            using (var gui = CreateGui())
            {
                gui.Run();

                Project project = gui.Application.ProjectService.CreateProject();
                
                Action mainWindowShown = delegate
                    {
                        project.RootFolder.Add(model);

                        // open view for boundary condition
                        gui.CommandHandler.OpenView(model.BoundaryConditions.First());

                        Assert.IsInstanceOf<BoundaryConditionEditor>(gui.DocumentViews.ActiveView);

                        gui.DocumentViews.Remove(gui.DocumentViews.ActiveView);

                        Assert.IsNull(gui.DocumentViews.ActiveView);

                        // open view for boundary (note: not the condition!)
                        gui.CommandHandler.OpenView(model.Boundaries.First());

                        Assert.IsInstanceOf<BoundaryConditionEditor>(gui.DocumentViews.ActiveView);
                    };

                WpfTestHelper.ShowModal((Control) gui.MainWindow, mainWindowShown);
            }
        }
        
        private static IGui CreateGui()
        {
            return new DHYDROGuiBuilder().WithFlowFM().Build();
        }
    }
}
