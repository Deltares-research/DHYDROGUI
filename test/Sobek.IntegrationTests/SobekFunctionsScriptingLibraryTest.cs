using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using DelftTools.Hydro.Roughness;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DeltaShell.Gui;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.CommonTools.Gui;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.DelftModels.HydroModel;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using DeltaShell.Plugins.NetCDF;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.NetworkEditor.Gui;
using DeltaShell.Plugins.ProjectExplorer;
using DeltaShell.Plugins.Scripting;
using DeltaShell.Plugins.Scripting.Gui;
using DeltaShell.Plugins.SharpMapGis;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.Toolbox;
using DeltaShell.Plugins.Toolbox.Gui;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using Point = NetTopologySuite.Geometries.Point;

namespace Sobek.IntegrationTests
{
    [TestFixture]
    [Category(TestCategory.Integration)]
    [Category(TestCategory.Slow)]
    public class SobekFunctionsScriptingLibraryTest
    {
        [TestFixtureSetUp]
        public void TestFixture()
        {
            var standardLibPath = @"plugins\DeltaShell.Plugins.Scripting\Lib";
            var sitePackagesPath = Path.Combine(standardLibPath, "site-packages");

            ScriptHost.AdditionalSearchPaths.Add(standardLibPath);
            ScriptHost.AdditionalSearchPaths.Add(sitePackagesPath);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            ScriptHost.AdditionalSearchPaths.Clear();
        }

        [Test]
        public void SobekFunctionsCreateIntegratedModelTest()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                var script = "from Libraries.SobekFunctions import CreateIntegratedModel\n" +
                             "integratedModel = CreateIntegratedModel ([flowModel], WorkingDir)\n";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", new WaterFlowModel1D()},
                        {"WorkingDir", Path.GetFullPath(Path.Combine(".", TestHelper.GetCurrentMethodName()))}
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                    {
                        var app = gui.Application;

                        var declaredVariables = app.ScriptRunner.RunScript(script, variables);
                        var integratedModel = declaredVariables.FirstOrDefault(kvp => kvp.Key == "integratedModel").Value as HydroModel;
                        
                        Assert.NotNull(integratedModel, "Integrated model should be added");

                        Assert.AreEqual(variables["flowModel"], integratedModel.Activities[0], "Flow model should be added to integrated model");
                    });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsCheckImports()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                var script = "from Libraries.SobekWaterFlowFunctions import *";

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    var declaredVariables = gui.Application.ScriptRunner.RunScript(script);
                    Assert.IsTrue(declaredVariables.Select(kvp => kvp.Key).Any(s => s == "ElementSet"));
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsEnableOutput()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                var script = "from Libraries.SobekWaterFlowFunctions import EnableOutput, AggregationOptions\n" +
                             "EnableOutput(flowModel, elementSet, quantity, AggregationOptions.Average)";

                var waterFlowModel1D = new WaterFlowModel1D();
                var quantity = QuantityType.WaterLevel;
                var elementSet = ElementSet.Laterals;

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"quantity", quantity},
                        {"elementSet", elementSet},
                    };

                var engineParameter = waterFlowModel1D.OutputSettings.GetEngineParameter(quantity, elementSet);
                Assert.True(engineParameter.AggregationOptions == AggregationOptions.None, "Waterlevel for laterals is default off (None)");

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(AggregationOptions.Average, engineParameter.AggregationOptions, "Flow model should have output waterlevel on laterals set at average");
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsGetComputationGridLocationByName()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1"){Geometry = new Point(0,0)};
                var node2 = new HydroNode("Node1"){Geometry = new Point(0,10)};
                var node3 = new HydroNode("Node1"){Geometry = new Point(0,20)};

                var channel1 = new Channel(node1, node2)
                    {
                        Name = "Channel1",
                        Geometry = new LineString(new[] {node1.Geometry.Coordinate, node2.Geometry.Coordinate})
                    };

                var channel2 = new Channel(node1, node2)
                    {
                        Name = "Channel2",
                        Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                    };

                var network = new HydroNetwork();
                network.Branches.AddRange(new []{channel1, channel2});
                network.Nodes.AddRange(new[] {node1, node2, node3});
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D {Network = network};
                var calcPoint1 = new NetworkLocation(channel1, 2) {Name = "CalcPoint1"};
                var calcPoint2 = new NetworkLocation(channel2, 4) { Name = "CalcPoint2" };

                waterFlowModel1D.NetworkDiscretization.Locations.AddValues(new[] { calcPoint1, calcPoint2 });

                var script = "from Libraries.SobekWaterFlowFunctions import GetComputationGridLocationByName\n" +
                             "location = GetComputationGridLocationByName(flowModel, \"CalcPoint1\")";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D}
                    };
                
                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    var declaredVariables = gui.Application.ScriptRunner.RunScript(script, variables);
                    var location = declaredVariables.FirstOrDefault(kvp => kvp.Key == "location").Value as NetworkLocation;

                    Assert.AreEqual(calcPoint1, location);
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsGetBoundaryDataByName()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();
                
                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };
                var boundaryDataNode1 = waterFlowModel1D.BoundaryConditions.First(c => c.Node == node1);

                var script = "from Libraries.SobekWaterFlowFunctions import GetBoundaryDataByName\n" +
                             "boundaryData = GetBoundaryDataByName(flowModel, \"Node1\")";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D}
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    var declaredVariables = gui.Application.ScriptRunner.RunScript(script, variables);
                    var boundaryData = declaredVariables.FirstOrDefault(kvp => kvp.Key == "boundaryData").Value as Model1DBoundaryNodeData;

                    Assert.AreEqual(boundaryDataNode1, boundaryData);
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsGetLateralDataByName()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network with lateral
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var lateral1 = new LateralSource{Name = "Lateral1"};
                
                NetworkHelper.AddBranchFeatureToBranch(lateral1, channel1, 4);

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };
                var lateral1SourceData = waterFlowModel1D.LateralSourceData.First(c => c.Feature == lateral1);

                var script = "from Libraries.SobekWaterFlowFunctions import GetLateralDataByName\n" +
                             "lateralData = GetLateralDataByName(flowModel, \"Lateral1\")";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D}
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    var declaredVariables = gui.Application.ScriptRunner.RunScript(script, variables);
                    var lateralData = declaredVariables.FirstOrDefault(kvp => kvp.Key == "lateralData").Value as Model1DLateralSourceData;

                    Assert.AreEqual(lateral1SourceData, lateralData);
                });
            }
        }
        
        [Test]
        public void SobekWaterFlowFunctionsGetTimeSeriesFromWaterFlowModel()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };
                var location = new NetworkLocation(channel1, 3);
                var now = DateTime.Now;
                var nowPlusOneHour = now.AddHours(1);
                var nowPlusTwoHour = now.AddHours(2);
                var nowPlusThreeHour = now.AddHours(3);
                var nowPlusFourHour = now.AddHours(4);
                
                // add output to flow coverage ("discharge")
                waterFlowModel1D.OutputFlow[now, location] = 12.0;
                waterFlowModel1D.OutputFlow[nowPlusOneHour, location] = 13.0;
                waterFlowModel1D.OutputFlow[nowPlusTwoHour, location] = 14.0;
                waterFlowModel1D.OutputFlow[nowPlusThreeHour, location] = 13.0;
                waterFlowModel1D.OutputFlow[nowPlusFourHour, location] = 12.0;

                var script = "from Libraries.SobekWaterFlowFunctions import GetTimeSeriesFromWaterFlowModel\n" +
                             "timeSeries = GetTimeSeriesFromWaterFlowModel(flowModel, location, \"Discharge\")";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"location", location}
                    };

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    var declaredVariables = gui.Application.ScriptRunner.RunScript(script, variables);
                    var timeSeries = declaredVariables.FirstOrDefault(kvp => kvp.Key == "timeSeries").Value as IList;
                    Assert.NotNull(timeSeries, "Timeseries should be created");

                    var values = new List<double>();
                    for (int i = 0; i < 5; i++)
                    {
                        values.Add((double)((IList) timeSeries[i])[1]);
                    }
                    
                    Assert.AreEqual(new[] { 12.0, 13.0, 14.0, 13.0, 12.0 }, values.ToArray(), "Sequence should be the same");
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsCreateComputationalGrid()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import CreateComputationalGrid\n" +
                             "timeSeries = CreateComputationalGrid(flowModel, gridAtFixedLength = True, fixedLength = 1)";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D}
                    };

                Assert.AreEqual(0, waterFlowModel1D.NetworkDiscretization.Locations.Values.Count);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);
                    
                    Assert.AreEqual(22, waterFlowModel1D.NetworkDiscretization.Locations.Values.Count, "22 calculation points should be generated");
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsSetBoundaryCondition()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };
                var boundaryDataNode1 = waterFlowModel1D.BoundaryConditions.First(c => c.Node == node1);

                var script = "from Libraries.SobekWaterFlowFunctions import SetBoundaryCondition, BoundaryConditionType\n" +
                             "SetBoundaryCondition(flowModel, \"Node1\" ,BoundaryConditionType.FlowConstant, 6.0)";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D}
                    };

                Assert.AreEqual(Model1DBoundaryNodeDataType.None, boundaryDataNode1.DataType);
                Assert.AreEqual(0, boundaryDataNode1.Flow);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(Model1DBoundaryNodeDataType.FlowConstant, boundaryDataNode1.DataType);
                    Assert.AreEqual(6.0, boundaryDataNode1.Flow);
                });
            }
        }
        
        [Test]
        public void SobekWaterFlowFunctionsSetLateralData()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network with lateral
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var lateral1 = new LateralSource { Name = "Lateral1" };

                NetworkHelper.AddBranchFeatureToBranch(lateral1, channel1, 4);

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };
                var lateralSourceData1 = waterFlowModel1D.LateralSourceData.First(c => c.Feature == lateral1);

                var script = "from Libraries.SobekWaterFlowFunctions import SetLateralData, LateralDataType\n" +
                             "SetLateralData(flowModel, \"Lateral1\" ,LateralDataType.FlowConstant , 6.0)";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D}
                    };

                Assert.AreEqual(Model1DLateralDataType.FlowTimeSeries, lateralSourceData1.DataType);
                Assert.AreEqual(0, lateralSourceData1.Flow);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(Model1DLateralDataType.FlowConstant, lateralSourceData1.DataType);
                    Assert.AreEqual(6.0, lateralSourceData1.Flow);
                });
            }
        }
                
        [Test]
        public void SobekWaterFlowFunctionsAddNewRoughnessSection()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import AddNewRoughnessSection\n" +
                             "AddNewRoughnessSection(flowModel, \"FloodPlain1\")";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D}
                    };

                Assert.AreEqual(1, waterFlowModel1D.RoughnessSections.Count);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(2, waterFlowModel1D.RoughnessSections.Count);
                    Assert.AreEqual("FloodPlain1", waterFlowModel1D.RoughnessSections[1].Name);
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsSetDefaultRoughness()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import SetDefaultRoughness\n" +
                             "SetDefaultRoughness(flowModel, \"Main\", roughnessType, 10.0)";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"roughnessType", RoughnessType.StricklerKn}
                    };

                Assert.AreEqual(RoughnessType.Chezy, waterFlowModel1D.RoughnessSections[0].GetDefaultRoughnessType());
                Assert.AreEqual(45, waterFlowModel1D.RoughnessSections[0].GetDefaultRoughnessValue());

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(RoughnessType.StricklerKn, waterFlowModel1D.RoughnessSections[0].GetDefaultRoughnessType());
                    Assert.AreEqual(10.0, waterFlowModel1D.RoughnessSections[0].GetDefaultRoughnessValue());
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsAddCrossSectionRoughness()
        {
            using (var gui = new DeltaShellGui())
            {   
                AddPlugins(gui);
                gui.Run();

                #region Create network with crossSection (XY)
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var crossSection1 = new CrossSection(new CrossSectionDefinitionYZ("SectionDefinition")
                    {
                        Thalweg = 2.5,
                        YZDataTable = new FastYZDataTable
                            {
                                // y, z, storage
                                new double[] {0, 0, 0},
                                new double[] {1, 0, 0},
                                new double[] {2, -10, 0},
                                new double[] {3, -10, 0},
                                new double[] {4, 0, 0},
                                new double[] {5, 0, 0}
                            }
                    });

                NetworkHelper.AddBranchFeatureToBranch(crossSection1, channel1, 3.0);

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var floodPlain1 = new CrossSectionSectionType { Name = "FloodPlain1" };
                network.CrossSectionSectionTypes.Add(floodPlain1);

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import AddCrossSectionRoughness\n" +
                             "AddCrossSectionRoughness(flowModel, crossSection1, 0.0, 2.0, floodPlain1)";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"crossSection1", crossSection1},
                        {"floodPlain1", floodPlain1}
                    };
                
                Assert.AreEqual(0, crossSection1.Definition.Sections.Count);
                
                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(1, crossSection1.Definition.Sections.Count);
                    Assert.AreEqual(0, crossSection1.Definition.Sections[0].MinY);
                    Assert.AreEqual(2, crossSection1.Definition.Sections[0].MaxY);
                    Assert.AreEqual(floodPlain1, crossSection1.Definition.Sections[0].SectionType);
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsAddRoughnessAtLocation()
        {
            using (var gui = new DeltaShellGui())
            {   
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import AddRoughnessAtLocation, RoughnessType\n" +
                             "AddRoughnessAtLocation(flowModel, \"Main\", channel1, 2.0, RoughnessType.Chezy, 6.1)";
                
                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"channel1", channel1}
                    };

                var mainRoughnessSection = waterFlowModel1D.RoughnessSections.First();
                Assert.NotNull(mainRoughnessSection);

                var roughnessNetworkCoverage = mainRoughnessSection.RoughnessNetworkCoverage;
                Assert.AreEqual(0, roughnessNetworkCoverage.Locations.Values.Count);
                
                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(1, roughnessNetworkCoverage.Locations.Values.Count);
                    var location = roughnessNetworkCoverage.Locations.Values[0];

                    Assert.AreEqual(channel1, location.Branch);
                    Assert.AreEqual(2.0, location.Chainage);
                    Assert.AreEqual(6.1, roughnessNetworkCoverage[location]);
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsSetRoughnessFunctionTypeByChannel()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import SetRoughnessFunctionTypeByChannel, RoughnessType, RoughnessFuntionType, AddRoughnessAtLocation\n" +
                             "AddRoughnessAtLocation(flowModel, \"Main\", channel1, 2, RoughnessType.Chezy, 42)\n" +
                             "AddRoughnessAtLocation(flowModel, \"Main\", channel1, 5, RoughnessType.Chezy, 40)\n" +
                             "AddRoughnessAtLocation(flowModel, \"Main\", channel1, 8, RoughnessType.Chezy, 45)\n" +
                             "SetRoughnessFunctionTypeByChannel(flowModel, \"Main\", channel1, RoughnessFuntionType.Waterlevel , [2.0, 5.0, 8.0], [[0.5, 41.0, 42.0, 43.0], [1.0, 42.0, 43.0, 44.0]])";
                             
                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"channel1", channel1}
                    };

                var mainRoughnessSection = waterFlowModel1D.RoughnessSections.First();
                Assert.NotNull(mainRoughnessSection);

                var roughnessNetworkCoverage = mainRoughnessSection.RoughnessNetworkCoverage;
                Assert.AreEqual(0, roughnessNetworkCoverage.Locations.Values.Count);
                
                Assert.AreEqual(RoughnessFunction.Constant, mainRoughnessSection.GetRoughnessFunctionType(channel1));

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);
                    
                    Assert.AreEqual(3, roughnessNetworkCoverage.Locations.Values.Count);

                    Assert.AreEqual(RoughnessFunction.FunctionOfH, mainRoughnessSection.GetRoughnessFunctionType(channel1));
                    Assert.AreEqual(6, mainRoughnessSection.FunctionOfH(channel1).GetValues().Count);
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsSetInitialConditionType()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import SetInitialConditionType, InitialConditionType\n" +
                             "SetInitialConditionType(flowModel, InitialConditionType.WaterLevel)";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"channel1", channel1}
                    };

                Assert.AreEqual(waterFlowModel1D.InitialConditionsType, InitialConditionsType.Depth);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(waterFlowModel1D.InitialConditionsType, InitialConditionsType.WaterLevel);
                });
            }
        }

        [Test]
        public void SobekWaterFlowFunctionsAddInitialValueAtLocation()
        {
            using (var gui = new DeltaShellGui())
            {
                AddPlugins(gui);
                gui.Run();

                #region Create network
                var node1 = new HydroNode("Node1") { Geometry = new Point(0, 0) };
                var node2 = new HydroNode("Node1") { Geometry = new Point(0, 10) };
                var node3 = new HydroNode("Node1") { Geometry = new Point(0, 20) };

                var channel1 = new Channel(node1, node2)
                {
                    Name = "Channel1",
                    Geometry = new LineString(new[] { node1.Geometry.Coordinate, node2.Geometry.Coordinate })
                };

                var channel2 = new Channel(node1, node2)
                {
                    Name = "Channel2",
                    Geometry = new LineString(new[] { node2.Geometry.Coordinate, node3.Geometry.Coordinate })
                };

                var network = new HydroNetwork();
                network.Branches.AddRange(new[] { channel1, channel2 });
                network.Nodes.AddRange(new[] { node1, node2, node3 });
                #endregion

                var waterFlowModel1D = new WaterFlowModel1D { Network = network };

                var script = "from Libraries.SobekWaterFlowFunctions import AddInitialValueAtLocation\n" +
                             "AddInitialValueAtLocation(flowModel, channel1, 5.0, 10.3)";

                var variables = new Dictionary<string, object>
                    {
                        {"flowModel", waterFlowModel1D},
                        {"channel1", channel1}
                    };

                Assert.AreEqual(0, waterFlowModel1D.InitialConditions.Locations.Values.Count);

                WpfTestHelper.ShowModal((Control)gui.MainWindow, () =>
                {
                    gui.Application.ScriptRunner.RunScript(script, variables);

                    Assert.AreEqual(1, waterFlowModel1D.InitialConditions.Locations.Values.Count);
                    var location = waterFlowModel1D.InitialConditions.Locations.Values[0];

                    Assert.AreEqual(channel1, location.Branch);
                    Assert.AreEqual(5.0, location.Chainage);
                    Assert.AreEqual(10.3, waterFlowModel1D.InitialConditions[location]);
                });
            }
        }

        private static void AddPlugins(IGui gui)
        {
            var app = gui.Application;

            app.Plugins.Add(new CommonToolsApplicationPlugin());
            app.Plugins.Add(new SharpMapGisApplicationPlugin());
            app.Plugins.Add(new NetworkEditorApplicationPlugin());
            app.Plugins.Add(new WaterFlowModel1DApplicationPlugin());
            app.Plugins.Add(new HydroModelApplicationPlugin());
            app.Plugins.Add(new NHibernateDaoApplicationPlugin());
            app.Plugins.Add(new NetCdfApplicationPlugin());
            app.Plugins.Add(new ToolboxApplicationPlugin());
            app.Plugins.Add(new ScriptingApplicationPlugin());

            gui.Plugins.Add(new ProjectExplorerGuiPlugin());
            gui.Plugins.Add(new SharpMapGisGuiPlugin());
            gui.Plugins.Add(new CommonToolsGuiPlugin());
            gui.Plugins.Add(new NetworkEditorGuiPlugin());
            gui.Plugins.Add(new WaterFlowModel1DGuiPlugin());
            gui.Plugins.Add(new HydroModelGuiPlugin());
            gui.Plugins.Add(new ToolboxGuiPlugin());
            gui.Plugins.Add(new ScriptingGuiPlugin());
        }
    }
}