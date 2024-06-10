using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.CoverageDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;
using Category = NUnit.Framework.CategoryAttribute;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
        }

        [TearDown]
        public void TearDown()
        {
            mocks.VerifyAll();
        }

        [Test]
        public void Constructor_CorrectlyInitializesInstance()
        {
            // Call
            using (var model = new WaterFlowFMModel())
            {
                // Assert
                Assert.That(model.SnapVersion, Is.EqualTo(0));
                Assert.That(model.ValidateBeforeRun, Is.True);
            
                Assert.That(model.Bathymetry, Is.Not.Null);
                Assert.That(model.Bathymetry, Is.TypeOf<UnstructuredGridCellCoverage>());
                Assert.That(model.InitialWaterLevel, Is.Not.Null);
                Assert.That(model.InitialWaterLevel, Is.TypeOf<UnstructuredGridCellCoverage>());
                Assert.That(model.InitialTemperature, Is.Not.Null);
                Assert.That(model.InitialTemperature, Is.TypeOf<UnstructuredGridCellCoverage>());
                Assert.That(model.InitialSalinity, Is.Not.Null);
                Assert.That(model.InitialSalinity, Is.TypeOf<CoverageDepthLayersList>());
                Assert.That(model.Roughness, Is.Not.Null);
                Assert.That(model.Roughness, Is.TypeOf<UnstructuredGridFlowLinkCoverage>());
                Assert.That(model.Viscosity, Is.Not.Null);
                Assert.That(model.Viscosity, Is.TypeOf<UnstructuredGridFlowLinkCoverage>());
                Assert.That(model.Diffusivity, Is.Not.Null);
                Assert.That(model.Diffusivity, Is.TypeOf<UnstructuredGridFlowLinkCoverage>());
                Assert.That(model.Infiltration, Is.Not.Null);
                Assert.That(model.Infiltration, Is.TypeOf<UnstructuredGridCellCoverage>());
                Assert.That(model.InitialTracers, Is.Empty);
                Assert.That(model.InitialTracers, Is.TypeOf<EventedList<UnstructuredGridCellCoverage>>());
                Assert.That(model.InitialFractions, Is.Empty);
                Assert.That(model.InitialFractions, Is.TypeOf<EventedList<UnstructuredGridCellCoverage>>());

                string[] dataItemNames = model.DataItems.Select(d => d.Name).ToArray();
                Assert.That(dataItemNames.Contains("Bed Level"));
                Assert.That(dataItemNames.Contains("Initial Water Level"));
                Assert.That(dataItemNames.Contains("Initial Salinity"));
                Assert.That(dataItemNames.Contains("Initial Salinity"));
                Assert.That(dataItemNames.Contains("Initial Temperature"));
                Assert.That(dataItemNames.Contains("Viscosity"));
                Assert.That(dataItemNames.Contains("Diffusivity"));
                Assert.That(dataItemNames.Contains("Infiltration"));
            }
        }

        [Test]
        public void GivenWaterFlowFMModel_DoingOnPropertyChanged_ShouldFirePropertyChangedEvent()
        {
            //Arrange
            var fmModel = new WaterFlowFMModel();

            var count = 0;
            fmModel.PropertyChanged += (s, a) =>
            {
                Assert.AreEqual(nameof(WaterFlowFMModel.UseSalinity), a.PropertyName);
                count++;
            };

            // Act
            fmModel.OnPropertyChanged(nameof(WaterFlowFMModel.UseSalinity));

            // Assert
            Assert.AreEqual(1, count);
        }

        private static int GetValueOfDirtyCounter(WaterFlowFMModel model)
        {
            return (int)typeof(WaterFlowFMModel).GetField("dirtyCounter", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(model);
        }

        [Test]
        public void Model_Increases_DirtyCounterForNHibernate_After_BoundaryData_Changed()
        {
            using (var model = new WaterFlowFMModel())
            {
                var model1DBoundaryNodeData = new Model1DBoundaryNodeData();
                model.BoundaryConditions1D.Add(model1DBoundaryNodeData);

                int start =  GetValueOfDirtyCounter(model);

                //Action
                model1DBoundaryNodeData.DataType = Model1DBoundaryNodeDataType.FlowConstant;

                //Check test result
                int current =  GetValueOfDirtyCounter(model);
                Assert.Greater(current, start, "No update of the dirty counter");
            }
        }
        
        [Test]
        public void Model_Increases_DirtyCounterForNHibernate_After_RoughnessData_Changed()
        {
            using (var model = new WaterFlowFMModel())
            {
                RoughnessSection roughnessSection = model.RoughnessSections.First(r => r.Name.Equals("Main"));
                int start =  GetValueOfDirtyCounter(model);

                //Action
                roughnessSection.SetDefaultRoughnessValue(123.456);

                //Check test result
                int current =  GetValueOfDirtyCounter(model);
                Assert.Greater(current, start, "No update of the dirty counter");
            }
        }
        
        [Test]
        public void Model_Increases_DirtyCounterForNHibernate_After_AreaItem_Changed()
        {
            using (var model = new WaterFlowFMModel())
            {
                var weir2D = new Weir2D();
                model.Area.Weirs.Add(weir2D); 
           
                int start =  GetValueOfDirtyCounter(model);

                //Action
                weir2D.CrestLevel = 345.678;

                //Check test result
                int current =  GetValueOfDirtyCounter(model);
                Assert.Greater(current, start, "No update of the dirty counter");
            }

        }
        
        [Test]
        public void Model_Increases_DirtyCounterForNHibernate_After_Network_Changed()
        {
            using (var model = new WaterFlowFMModel())
            {
                var weir = new Weir
                {
                    Geometry = new Point(5, 0),
                    OffsetY = 100,
                    CrestWidth = 1,
                    CrestLevel = 1
                };

                //setup network with the weir
                var network = new HydroNetwork();
                var node1 = new HydroNode
                {
                    Name = "Node1",
                    Network = network
                };
                var node2 = new HydroNode
                {
                    Name = "Node2",
                    Network = network
                };
                node1.Geometry = new Point(0.0, 0.0);
                node2.Geometry = new Point(100.0, 0.0);
                network.Nodes.Add(node1);
                network.Nodes.Add(node2);
                var branch = new Channel("branch1", node1, node2)
                {
                    Geometry = new LineString(new[]
                    {
                        node1.Geometry.Coordinate,
                        node2.Geometry.Coordinate
                    })
                };
                network.Branches.Add(branch);
                var compositeBranchStructure = new CompositeBranchStructure
                {
                    Network = network,
                    Geometry = new Point(5, 0),
                    Chainage = 5
                };
                NetworkHelper.AddBranchFeatureToBranch(compositeBranchStructure, branch, compositeBranchStructure.Chainage);
                HydroNetworkHelper.AddStructureToComposite(compositeBranchStructure, weir);

                model.Network = network;
            
                int start =  GetValueOfDirtyCounter(model);

                //Action
                weir.CrestLevel += 1.0;

                //Check test result
                int current =  GetValueOfDirtyCounter(model);
                Assert.Greater(current, start, "No update of the dirty counter");
            }

        }
        
        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestImportSimpleModelWith_SourceAndSink_Tracer_Morphology_CorrectlyUpdatesSourceAndSinkComponents()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"SimpleModel_SourceAndSink_Tracer_Morphology\SimpleModel.mdu"));
            var sourceAndSink = model.SourcesAndSinks.FirstOrDefault();

            Assert.NotNull(sourceAndSink);
            foreach (var sedimentFraction in model.SedimentFractions)
            {
                Assert.True(sourceAndSink.Function.Components.Any(c => c.Name == sedimentFraction.Name));
            }

            var tracerBoundaryConditionsTracerNames = model.BoundaryConditions
                .OfType<FlowBoundaryCondition>()
                .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                .Select(tbc => tbc.TracerName)
                .Distinct();

            foreach (var tracerName in model.TracerDefinitions.Where(t => tracerBoundaryConditionsTracerNames.Contains(t)))
            {
                Assert.True(sourceAndSink.Function.Components.Any(c => c.Name == tracerName));
            }

            foreach (var tracerName in model.TracerDefinitions.Where(t => !tracerBoundaryConditionsTracerNames.Contains(t)))
            {
                Assert.False(sourceAndSink.Function.Components.Any(c => c.Name == tracerName));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestAddingSourceAndSinkCorrectlyUpdatesSedimentFractionAndTracerNamesForSourceAndSink()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"SimpleModel_SourceAndSink_Tracer_Morphology\SimpleModel.mdu"));
            var sourceAndSink = new SourceAndSink();

            Assert.That(sourceAndSink.SedimentFractionNames.Count, Is.EqualTo(0));
            Assert.That(sourceAndSink.TracerNames.Count, Is.EqualTo(0));

            model.SourcesAndSinks.Add(sourceAndSink);

            foreach (var sedimentFraction in model.SedimentFractions)
            {
                Assert.True(sourceAndSink.SedimentFractionNames.Contains(sedimentFraction.Name));
            }

            var tracerBoundaryConditionsTracerNames = model.BoundaryConditions
                .OfType<FlowBoundaryCondition>()
                .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                .Select(tbc => tbc.TracerName)
                .Distinct();

            foreach (var tracerName in model.TracerDefinitions.Where(t => tracerBoundaryConditionsTracerNames.Contains(t)))
            {
                Assert.True(sourceAndSink.TracerNames.Contains(tracerName));
            }

            foreach (var tracerName in model.TracerDefinitions.Where(t => !tracerBoundaryConditionsTracerNames.Contains(t)))
            {
                Assert.False(sourceAndSink.TracerNames.Contains(tracerName));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestRemovingTracerBoundaryCondition_OnlyRemovesTracerNameFromSourceAndSink_IfNoOtherTracerBoundaryConditionsExistsForSameTracer()
        {
            var model = new WaterFlowFMModel();
            var sourceAndSink = new SourceAndSink();

            Assert.That(sourceAndSink.SedimentFractionNames.Count, Is.EqualTo(0));
            Assert.That(sourceAndSink.TracerNames.Count, Is.EqualTo(0));

            model.SourcesAndSinks.Add(sourceAndSink);

            var tracer01 = "Tracer01";
            var tracer02 = "Tracer02";
            model.TracerDefinitions.AddRange(new List<string> { tracer01, tracer02 });

            var boundary01 = new Feature2D { Name = "Boundary01" };
            var set01 = new BoundaryConditionSet();
            model.BoundaryConditionSets.Add(set01);

            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary01,
                TracerName = tracer01
            });

            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary01,
                TracerName = tracer02
            });

            var boundary02 = new Feature2D { Name = "Boundary02" };
            var set02 = new BoundaryConditionSet();
            model.BoundaryConditionSets.Add(set02);
            set02.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary02,
                TracerName = tracer01
            });

            Assert.That(sourceAndSink.TracerNames.Count, Is.EqualTo(2));
            Assert.That(sourceAndSink.TracerNames[0], Is.EqualTo(tracer01));
            Assert.That(sourceAndSink.TracerNames[1], Is.EqualTo(tracer02));

            set01.BoundaryConditions.Clear();

            Assert.That(sourceAndSink.TracerNames.Count, Is.EqualTo(1));
            Assert.That(sourceAndSink.TracerNames[0], Is.EqualTo(tracer01));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestRemovingBoundaryConditionSet_OnlyRemovesTracerNameFromSourceAndSink_IfNoOtherTracerBoundaryConditionsExistsForSameTracer()
        {
            var model = new WaterFlowFMModel();
            var sourceAndSink = new SourceAndSink();

            Assert.That(sourceAndSink.SedimentFractionNames.Count, Is.EqualTo(0));
            Assert.That(sourceAndSink.TracerNames.Count, Is.EqualTo(0));

            model.SourcesAndSinks.Add(sourceAndSink);

            var tracer01 = "Tracer01";
            var tracer02 = "Tracer02";
            model.TracerDefinitions.AddRange(new List<string> { tracer01, tracer02 });
            
            var boundary01 = new Feature2D { Name = "Boundary01" };
            var set01 = new BoundaryConditionSet();
            model.BoundaryConditionSets.Add(set01);

            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary01,
                TracerName = tracer01
            });

            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary01,
                TracerName = tracer02
            });

            var boundary02 = new Feature2D { Name = "Boundary02" };
            var set02 = new BoundaryConditionSet();
            model.BoundaryConditionSets.Add(set02);
            set02.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary02,
                TracerName = tracer01
            });

            Assert.That(sourceAndSink.TracerNames.Count, Is.EqualTo(2));
            Assert.That(sourceAndSink.TracerNames[0], Is.EqualTo(tracer01));
            Assert.That(sourceAndSink.TracerNames[1], Is.EqualTo(tracer02));

            model.BoundaryConditionSets.Remove(set01);
            
            Assert.That(sourceAndSink.TracerNames.Count, Is.EqualTo(1));
            Assert.That(sourceAndSink.TracerNames[0], Is.EqualTo(tracer01));
        }

        [Test]
        public void BoundaryConditionSetShouldBubbleEvents()
        {
            var model = new WaterFlowFMModel();
            var set = new BoundaryConditionSet();

            model.BoundaryConditionSets.Add(set);
            
            var count = 0;
            model.CollectionChanged += (sender, args) => count++;

            set.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty));

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void CheckWeirFormulaPropertyChangeEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();

            var weir = new Weir2D
            {
                Name = "weir01",
                WeirFormula = new SimpleWeirFormula()
            };

            var collectionChangedCount = 0;
            ((INotifyCollectionChanged) model).CollectionChanged += (s, e) =>
            {
                if (e.GetRemovedOrAddedItem() != weir) return;
                collectionChangedCount++;
            };

            var weirFormulaChangeCount = 0;
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(IWeir.WeirFormula)) return;
                weirFormulaChangeCount++;
            };
            // add weir to model
            model.Area.Weirs.Add(weir);
            Assert.That(collectionChangedCount, Is.EqualTo(1));
            
            // change weirformula
            weir.WeirFormula = new GeneralStructureWeirFormula();
            Assert.That(weirFormulaChangeCount, Is.EqualTo(1));
        }

        [Test]
        public void CheckDataItemsAfterChangeOfWeirFormula()
        {
            var model = new WaterFlowFMModel();

            var weir = new Weir2D
            {
                Name = "weir01",
                WeirFormula = new SimpleWeirFormula()
            };
            model.Area.Weirs.Add(weir);
            
            var dataItems = model.GetChildDataItems(weir).ToList();
            
            Assert.That(dataItems.Count, Is.EqualTo(1));

            Assert.That(dataItems[0].Name, Is.EqualTo(weir.Name));
            Assert.That(dataItems[0].Tag, Is.EqualTo("CrestLevel"));
            Assert.That(dataItems[0].Role, Is.EqualTo(DataItemRole.Input | DataItemRole.Output));

            var valueConverter = (WaterFlowFMFeatureValueConverter)dataItems[0].ValueConverter;
            Assert.That(valueConverter.Location, Is.EqualTo(weir));
            Assert.That(valueConverter.Model, Is.EqualTo(model));
            Assert.That(valueConverter.ParameterName, Is.EqualTo("CrestLevel"));

            // change weir formula
            weir.WeirFormula = new GeneralStructureWeirFormula();
            dataItems = model.GetChildDataItems(weir).ToList();
            Assert.That(dataItems.Count, Is.EqualTo(5));

            var generalStructureDataItems = (TypeUtils.CallPrivateStaticMethod(typeof(WaterFlowFMModelDataSet), "CreateGeneralStructuresNames") as Dictionary<string, string>)?.Values.ToList();


            Assert.That(generalStructureDataItems.Count == dataItems.Count);

            for (var i = 0; i < dataItems.Count; ++i)
            {
                Assert.That(dataItems[i].Name, Is.EqualTo(weir.Name));
                Assert.That(dataItems[i].Tag, Is.EqualTo(generalStructureDataItems[i]));
                Assert.That(dataItems[i].Role, Is.EqualTo(DataItemRole.Input | DataItemRole.Output));

                valueConverter = (WaterFlowFMFeatureValueConverter)dataItems[i].ValueConverter;
                Assert.That(valueConverter.Location, Is.EqualTo(weir));
                Assert.That(valueConverter.Model, Is.EqualTo(model));
                Assert.That(valueConverter.ParameterName, Is.EqualTo(generalStructureDataItems[i]));
            }
        }

        [Test]
        public void CheckSedimentFormulaPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel {ModelDefinition = {UseMorphologySediment = true}};
            var sedFrac = new SedimentFraction
            {
                Name = "testFrac",
                CurrentSedimentType = SedimentFractionHelper.GetSedimentationTypes()[1],
                CurrentFormulaType = SedimentFractionHelper.GetSedimentationFormulas()[0]
            };
            model.SedimentFractions.Add(sedFrac);

            var modelCount = 0;
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != "IsSpatiallyVarying") return;
                modelCount++;
            };

            var sedFracCount = 0;
            ((INotifyPropertyChanged)sedFrac).PropertyChanged += (s, e) => sedFracCount++;

            var prop = sedFrac.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().First();
            prop.IsSpatiallyVarying = true;

            Assert.That(sedFracCount, Is.EqualTo(1));
            Assert.That(modelCount, Is.EqualTo(1)); // IsSpatiallyVarying
        }

        [Test]
        public void CheckSedimentPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction { Name = "testFrac" };
            model.SedimentFractions.Add(sedFrac);

            var modelCount = 0;
            ((INotifyPropertyChanged)model).PropertyChanged += (s, e) => modelCount++;
            var sedFracCount = 0;
            ((INotifyPropertyChanged)sedFrac).PropertyChanged += (s, e) => sedFracCount++;
            
            var prop = sedFrac.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().First();
            prop.IsSpatiallyVarying = true;

            Assert.That(sedFracCount, Is.EqualTo(1));

            // TODO: Set the assertion value to 3 when initial condition is supported in ext-files (DELFT3DFM-996)
            Assert.That(modelCount, Is.EqualTo(1)); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckFileBasedStatesofFMModel()
        {
            var model1 = new WaterFlowFMModel();
            Assert.That(model1.Name, Is.EqualTo("FlowFM"));
            Assert.That(model1.MduFilePath, Is.EqualTo(null));

            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model2 = new WaterFlowFMModel(mduPath);
            Assert.That(model2.Name, Is.EqualTo("bendprof"));
            Assert.That(Path.GetFileName(model2.MduFilePath), Is.EqualTo("bendprof.mdu"));
        }

        [Test]
        public void CreateNewModelCheckStuffIsEmptyButNotNull()
        {
            var model = new WaterFlowFMModel(); // empty model
            Assert.IsTrue(model.Grid.IsEmpty);
            Assert.IsNotNull(model.Bathymetry);
            Assert.That(model.Bathymetry.ToPointCloud().PointValues.Count, Is.EqualTo(0));
        }

        [Test]
        public void AddInitialSalinityTest()
        {
            // this test checks for SpatialDataLayersChanged() in WaterFlowFMModel.
            var model = new WaterFlowFMModel();

            Assert.That(model.InitialSalinity.Coverages.Count, Is.EqualTo(1));
            var originalDataItem = model.GetDataItemByValue(model.InitialSalinity.Coverages[0]);
            var originalName = originalDataItem.Name;

            model.InitialSalinity.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.TopBottom);

            Assert.That(model.InitialSalinity.Coverages.Count, Is.EqualTo(2));
            Assert.IsNotNull(model.GetDataItemByValue(model.InitialSalinity.Coverages[1]));
                // check if a data item was created

            Assert.AreNotEqual(originalName, model.GetDataItemByValue(model.InitialSalinity.Coverages[0]).Name);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TransformCoordinateSystemTest()
        {
            string mduPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu");
            var localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            var factory = Map.CoordinateSystemFactory;
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(localMduFilePath));
            model.CoordinateSystem = factory.CreateFromEPSG(28992);

            var newCoordinateSystem = factory.CreateFromEPSG(4326);
            var transformation = factory.CreateTransformation(model.CoordinateSystem, newCoordinateSystem);
            model.TransformCoordinates(transformation);

            Assert.That(newCoordinateSystem, Is.EqualTo(model.CoordinateSystem));
            Assert.That(newCoordinateSystem, Is.EqualTo(model.Roughness.CoordinateSystem));

            var roughnessDataItem = model.GetDataItemByValue(model.Roughness);
            var valueConverter = (SpatialOperationSetValueConverter) roughnessDataItem.ValueConverter;

            var spatialOperationSet = valueConverter.SpatialOperationSet;
            Assert.That(spatialOperationSet.CoordinateSystem, Is.EqualTo(model.CoordinateSystem));
            Assert.That(spatialOperationSet.Operations.Last().CoordinateSystem, Is.EqualTo(model.CoordinateSystem));
        }

        [Test]
        public void HydFileNameShouldBeBasedOnMduFileName()
        {
            var model = new WaterFlowFMModel {WorkingDirectoryPathFunc = ()=> @"C:\TestWorkDir"};

            TypeUtils.SetPrivatePropertyValue(model, nameof(model.MduFilePath), "Test.mdu");

            Assert.That(model.HydFilePath, Is.EqualTo($@"C:\TestWorkDir\{model.Name}\DFM_DELWAQ_Test\Test.hyd"));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestDiaFileIsRetrievedAfterModelRun()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            ActivityRunner.RunActivity(model);

            var diaFileDataItem = model.DataItems.FirstOrDefault(di => di.Tag == WaterFlowFMModelDataSet.DiaFileDataItemTag);
            Assert.NotNull(diaFileDataItem, "DiaFile not retrieved after model run, check WaterFlowFMModel.DiaFileDataItemTag");
            Assert.NotNull(diaFileDataItem.Value, "DiaFile not retrieved after model run, check WaterFlowFMModel.DiaFileDataItemTag");
        }

        [Test]
        public void WhenInstantiatingAnFmModel_ThenTheModelHasDefaultRoughnessSections()
        {
            var fmModel = new WaterFlowFMModel();
            Assert.IsNotNull(fmModel.RoughnessSections, "Roughness sections of the FM model were not instantiated.");
            Assert.That(fmModel.RoughnessSections.Count(rs => rs.Name == "Main"), Is.EqualTo(1));
        }

        [Test]
        public void WhenInstantiatingAnFmModel_ThenTheModelHasSewerRoughnessSectionWithDefaultValues()
        {
            var fmModel = new WaterFlowFMModel();
            var roughnessSections = fmModel.RoughnessSections;

            Assert.IsNotNull(roughnessSections, "Roughness sections of the FM model were not instantiated.");
            Assert.That(roughnessSections.Count(rs => rs.Name == RoughnessDataSet.SewerSectionTypeName), Is.EqualTo(1));
            Assert.That(roughnessSections.ElementAt(1).Name, Is.EqualTo(RoughnessDataSet.SewerSectionTypeName));

            var sewerRoughnessSection = roughnessSections.ElementAt(1);
            Assert.That(sewerRoughnessSection.GetDefaultRoughnessValue(), Is.EqualTo(0.2));
            Assert.That(sewerRoughnessSection.GetDefaultRoughnessType(), Is.EqualTo(RoughnessType.WhiteColebrook));
        }

        [Test]
        public void GivenLegacyMduFileWithout1DNetworkDefined_WhenInstantiatingWithMduPath_ThenTheModelHasDefaultRoughnessSections()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var fmModel = new WaterFlowFMModel(mduPath);
            var roughnessSections = fmModel.RoughnessSections;

            Assert.IsNotNull(roughnessSections, "Roughness sections of the FM model were not instantiated.");
            Assert.That(roughnessSections.Count(rs => rs.Name == RoughnessDataSet.MainSectionTypeName), Is.EqualTo(1));
            Assert.That(roughnessSections.Count(rs => rs.Name == RoughnessDataSet.SewerSectionTypeName), Is.EqualTo(1));
            Assert.That(roughnessSections.ElementAt(1).Name, Is.EqualTo(RoughnessDataSet.SewerSectionTypeName));
        }

        [Test]
        public void GivenFmModelWithNetwork_WhenAddingNewCrossSectionTypeToNetwork_ThenAnExtraDataItemIsAddedToTheModel()
        {
            const string crossSectionTypeName = "myNewCrossSectionType";

            var fmModel = new WaterFlowFMModel();
            var newCrossSectionSectionType = new CrossSectionSectionType
            {
                Name = crossSectionTypeName
            };

            fmModel.Network.CrossSectionSectionTypes.Add(newCrossSectionSectionType);

            var roughnessSections = fmModel.RoughnessSections;
            Assert.That(roughnessSections.Count, Is.EqualTo(3));
            Assert.That(roughnessSections.Count(rs => rs.Name == crossSectionTypeName), Is.EqualTo(1));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void SetCoordinateSystemOnModelAndExportAdjustsNetFile()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            var tempDir = Path.GetTempFileName();
            File.Delete(tempDir);
            Directory.CreateDirectory(tempDir);

            model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wgs84
            model.ExportTo(Path.Combine(tempDir, @"cs\cs.mdu"));

            Assert.That(NetFile.ReadCoordinateSystem(model.NetFilePath).AuthorityCode, Is.EqualTo(4326));

            model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); //other number
            model.ExportTo(Path.Combine(tempDir, @"cs2\cs2.mdu"));

            Assert.That(NetFile.ReadCoordinateSystem(model.NetFilePath).AuthorityCode, Is.EqualTo(28992));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void CheckIfBcmFileIsReferencedInMorFileAfterRunningAnImportedMduFile()
        {
            //arrange
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var tempDir = FileUtils.CreateTempDirectory();
            var model = new WaterFlowFMModel(mduPath);

            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction
            {
                Name = "testFrac",
                CurrentSedimentType = SedimentFractionHelper.GetSedimentationTypes()[1],
                CurrentFormulaType = SedimentFractionHelper.GetSedimentationFormulas()[0]
            };

            model.SedimentFractions.Add(sedFrac);

            var tracer01 = "Tracer01";
            var tracer02 = "Tracer02";
            model.TracerDefinitions.AddRange(new List<string> { tracer01, tracer02 });

            var feature = new Feature2D
            {
                Name = "Boundary1",
                Geometry =
                    new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
            };

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[] { model.StartTime, model.StopTime });
            flowBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            flowBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var set01 = new BoundaryConditionSet { Feature = feature };
            model.BoundaryConditionSets.Add(set01);

            var boundary = new Feature2D()
            {
                Name = "TracerBoundary1",
                Geometry =
                    new LineString(new[] { new Coordinate(0, 0), new Coordinate(1, 0) })
            };
            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.AstroComponents)
            {
                Feature = boundary,
                TracerName = tracer01
            });
            var exportPath = Path.Combine(tempDir, "export");
            var mduExportPath = Path.Combine(exportPath, "cs.mdu");
            model.ExportTo(mduExportPath);

            var modelAfterImport = new WaterFlowFMModel(mduExportPath);
            ActivityRunner.RunActivity(modelAfterImport);
            var mduFilePathAfterExport = modelAfterImport.MduFilePath;

            File.Exists(Path.Combine(mduFilePathAfterExport, "cs.mdu"));
            File.Exists(Path.Combine(mduFilePathAfterExport, "cs.mor"));
            File.Exists(Path.Combine(mduFilePathAfterExport, "cs.sed"));

            var morFilePath = Path.Combine(exportPath, "cs.mor");
            File.Exists(morFilePath);

            //act
            var lines = File.ReadLines(morFilePath);
            var countedLines = lines.Count(l => l.Replace(" ", "").Contains("BcFil=bendprof.bcm"));
         
            //assert
            Assert.AreEqual(countedLines, 1);

        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckStartTime()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            Assert.That(model.StartTime, Is.EqualTo(new DateTime(1992, 08, 31)));

            var newTime = new DateTime(2000, 1, 2, 11, 15, 5, 2); //time with milliseconds
            model.StartTime = newTime;
            Assert.That(model.StartTime, Is.EqualTo(newTime));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckCoordinateSystemBendProf()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            Assert.IsNull(model.CoordinateSystem);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckCoordinateSystemIvoorkust()
        {
            var mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(mduPath))
            {
                Assert.That(model.CoordinateSystem.Name, Is.EqualTo("WGS 84"));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportIvoorkustModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            using (var model = new WaterFlowFMModel(mduPath))
            {
                model.Initialize();

                Assert.That(model.Status, Is.EqualTo(ActivityStatus.Initialized));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportHarlingen3DModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"harlingen_model_3d\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            Assert.That(model.DepthLayerDefinition.NumLayers, Is.EqualTo(10), "depth layers");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ExportTwiceCheckNetFileIsCopiedCorrectly()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            var tempPath1 = Path.GetTempFileName();
            File.Delete(tempPath1);
            Directory.CreateDirectory(tempPath1);

            model.ExportTo(Path.Combine(tempPath1, "test.mdu"), false);

            // delete the first export location
            FileUtils.DeleteIfExists(tempPath1);

            var tempPath2 = Path.GetTempFileName();
            File.Delete(tempPath2);
            Directory.CreateDirectory(tempPath2);

            // export to second export location
            model.ExportTo(Path.Combine(tempPath2, "test.mdu"), false);

            Assert.IsTrue(File.Exists(Path.Combine(tempPath2, "bend1_net.nc")));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void LoadingEmptyGridNetFileShouldNotLockIt()
        {
            string testDataDir = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input");

            using (var tempDir = new TemporaryDirectory())
            {
                string tempTestDataDir = tempDir.CopyDirectoryToTempDirectory(testDataDir);
                string mduPath = Path.Combine(tempTestDataDir, "bendprof.mdu");
                
                var model = new WaterFlowFMModel(mduPath);
                
                // make grid file corrupt
                File.WriteAllText(model.NetFilePath, "");

                // attempt to reload grid
                model.ReloadGrid();

                // make sure we can still delete the file (not locked by mistake)
                File.Delete(model.NetFilePath);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportHarlingenAndCheckTimeSeries()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"harlingen\har.mdu"));

            var boundaryCondition =
                model.BoundaryConditions.First(
                    bc => bc is FlowBoundaryCondition && ((Feature2D) bc.Feature).Name == "071_02");

            var refDate = model.ModelDefinition.GetReferenceDateAsDateTime();

            var function = boundaryCondition.GetDataAtPoint(0);

            var times = function.Arguments.OfType<IVariable<DateTime>>().First();

            var bcStartTime = times.MinValue;

            Assert.That(bcStartTime, Is.EqualTo(refDate));

            const double minutes = 4.7520000e+04;

            var bcTimeRange = new TimeSpan(0, 0, (int) minutes, 0);

            var bcStopTime = times.MaxValue;

            Assert.That(bcStopTime, Is.EqualTo(refDate + bcTimeRange));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadGridShouldNotThrowAlotOfEvents()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            int count = 0;
            ((INotifyPropertyChanged) model).PropertyChanged += (s, e) => count++;

            model.ReloadGrid();

            Assert.Less(count, model.Grid.Vertices.Count, "expected few events");

            // if it throws many events it can cause performance problems
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void LoadManyRoughnessPolygonsForVenice()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"venice_pilot_22ott2013\n_e04e.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);

            Assert.IsTrue(model.ModelDefinition.SpatialOperations.Count > 0);

            var operation =
                (SetValueOperation)
                model.ModelDefinition.GetSpatialOperations(WaterFlowFMModelDefinition.RoughnessDataItemName)[0];
            Assert.IsTrue(operation.Mask.Provider.Features.Count > 1);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportSpatialOperationsTest()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));

            var valueConverter = model.GetDataItemByValue(model.Roughness).ValueConverter;
            var spatialOperationValueConverter = valueConverter as SpatialOperationSetValueConverter;

            Assert.IsNotNull(spatialOperationValueConverter);

            Assert.That(spatialOperationValueConverter.SpatialOperationSet.Operations.Count, Is.EqualTo(2));
            Assert.IsTrue(spatialOperationValueConverter.SpatialOperationSet.Operations[1] is InterpolateOperation);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadBathymetryTest()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));
            var originalGrid = model.Grid;
            var bathymetryDataItem = model.GetDataItemByValue(model.Bathymetry);
            var spatialOperationValueConverter =
                SpatialOperationValueConverterFactory.GetOrCreateSpatialOperationValueConverter(bathymetryDataItem,
                    model.Bathymetry.Name);

            Assert.IsNotNull(spatialOperationValueConverter);

            var eraseOperation = new EraseOperation();
            Assert.IsNotNull(spatialOperationValueConverter.SpatialOperationSet.AddOperation(eraseOperation));

            model.ReloadGrid(true);

            Assert.IsTrue(spatialOperationValueConverter.SpatialOperationSet.Dirty);

            spatialOperationValueConverter.SpatialOperationSet.Execute();
            var cov =
                spatialOperationValueConverter.SpatialOperationSet.Output.Provider.Features[0] as
                    UnstructuredGridCoverage;

            Assert.IsTrue(originalGrid == model.Grid);
            Assert.IsTrue(cov.Grid == model.Grid);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadGridShouldConstructEdges()
        {
            var model = new WaterFlowFMModel(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));
            new FlowFMNetFileImporter().ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"), model);
            Assert.That(model.Grid.Vertices.Count, Is.EqualTo(12845));
            Assert.That(model.Grid.Cells.Count, Is.EqualTo(16597));
            Assert.That(model.Grid.Edges.Count, Is.EqualTo(29441));
        }

        [Test]
        public void ReloadGridShouldSetNoDataValueForBathemetry()
        {
            var model = new WaterFlowFMModel();
            Assert.That(model.Grid.Cells.Count, Is.EqualTo(0));

            var testFile = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFile));
            var localCopyOfTestFile = TestHelper.CreateLocalCopy(testFile);

            try
            {
                Assert.That(model.Bathymetry.Components[0].NoDataValue, Is.EqualTo(-999.0).Within(0.01));
                TypeUtils.SetPrivatePropertyValue(model, "MduFilePath", @".\");
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).Value = localCopyOfTestFile;
                model.ReloadGrid(false);
                Assert.That(model.Grid.Cells.Count, Is.GreaterThan(0));

                Assert.That(model.Bathymetry.Components[0].NoDataValue, Is.EqualTo(-999.0).Within(0.01));
            }
            finally
            {
                FileUtils.DeleteIfExists(localCopyOfTestFile);
            }

        }

        [TestCase(
            new[]
            {
                UGridFileHelper.BedLevelLocation.Faces,
                UGridFileHelper.BedLevelLocation.NodesMeanLev,
                UGridFileHelper.BedLevelLocation.Faces
            }, 
            new[]
            {
                typeof(UnstructuredGridCellCoverage),
                typeof(UnstructuredGridVertexCoverage),
                typeof(UnstructuredGridCellCoverage)
            }
        )]
        [TestCase(
            new[]
            {
                UGridFileHelper.BedLevelLocation.NodesMaxLev,
                UGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes,
                UGridFileHelper.BedLevelLocation.NodesMinLev
            }, 
            new[]
            {
                typeof(UnstructuredGridVertexCoverage),
                typeof(UnstructuredGridCellCoverage),
                typeof(UnstructuredGridVertexCoverage)
            }
        )]
        [TestCase(
            new[]
            {
                UGridFileHelper.BedLevelLocation.CellEdges
            }, 
            new[]
            {
                // UnstructuredGridEdgeCoverage not currently supported
                // returns UnstructuredGridVertexCoverage instead
                typeof(UnstructuredGridVertexCoverage) 
            }
        )]

        public void TestUpdateBathymetryCoverage(UGridFileHelper.BedLevelLocation[] bedLevelLocations, Type[] coverageTypes)
        {
            // if this is false, the test cases are not correct
            Assert.That(coverageTypes.Length, Is.EqualTo(bedLevelLocations.Length));

            var fmModel = new WaterFlowFMModel();

            for (var i = 0; i < bedLevelLocations.Length; i++)
            {
                TypeUtils.CallPrivateMethod(fmModel, "UpdateBathymetryCoverage", bedLevelLocations[i]);
                Assert.That(fmModel.Bathymetry.GetType(), Is.EqualTo(coverageTypes[i]));
            }
        }

        [Test]
        public void FmModelGetVarGridPropertyNameShouldReturnGrid()
        {
            var model = new WaterFlowFMModel();
            var grids = model.GetVar(WaterFlowFMModel.GridPropertyName) as UnstructuredGrid[];
            Assert.IsNotNull(grids);
            Assert.IsNotNull(grids[0]);
            Assert.IsTrue(grids[0].IsEmpty);
        }

        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [Test]
        public void FmModelGetVarCellsToFeaturesNameShouldReturnEmptyTimeseries()
        {
            var model = new WaterFlowFMModel();
            TypeUtils.SetField(model, "outputMapFileStore", new FMMapFileFunctionStore());
            var timeSeries = model.GetVar(WaterFlowFMModel.CellsToFeaturesName) as ITimeSeries[];

            Assert.IsNotNull(timeSeries,
                "Time series was not expected to be null");
            Assert.That(timeSeries.Length, Is.EqualTo(0),
                "Time series was expected to be empty.");
        }
        
        [Test]
        public void FmModelSetVarDisableFlowNodeRenumbering()
        {
            var model = new WaterFlowFMModel();
            Assert.IsFalse(model.DisableFlowNodeRenumbering);
            model.SetVar(new[] {true}, WaterFlowFMModel.DisableFlowNodeRenumberingPropertyName, null, null);
            Assert.IsTrue(model.DisableFlowNodeRenumbering);
        }

        [Test]
        public void WriteSnappedFeaturesTest()
        {
            var model = new WaterFlowFMModel();

            /* Default is false */
            Assert.IsFalse(model.WriteSnappedFeatures);
            Assert.That(model.ModelDefinition.WriteSnappedFeatures, Is.EqualTo(model.WriteSnappedFeatures));

            /* Value is the same in the model definition */
            model.ModelDefinition.WriteSnappedFeatures = true;
            Assert.IsTrue(model.WriteSnappedFeatures);
            Assert.That(model.ModelDefinition.WriteSnappedFeatures, Is.EqualTo(model.WriteSnappedFeatures));
        }

        [Test]
        public void GivenFmModel_WhenAddingAnAreaFeatureWithGroupNameEqualToPathThatIsPointingToASubFolderOfMduFolder_ThenGroupNameIsAlwaysRelative()
        {
            // Make local copy of project
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(baseFolderPath, "FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);
            
            fmModel.Area.DryPoints.Add(new GroupablePointFeature
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"SubFolder/MyDryPoints_dry.xyz")
            });
            fmModel.Area.LandBoundaries.Add(new DelftTools.Hydro.LandBoundary2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"SubFolder/MyLandBoundaries.ldb")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.DryPoints.FirstOrDefault().GroupName, Is.EqualTo(@"SubFolder/MyDryPoints_dry.xyz"));
            Assert.That(fmModel.Area.LandBoundaries.FirstOrDefault().GroupName, Is.EqualTo(@"SubFolder/MyLandBoundaries.ldb"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAStructureWithAreaFeatureGroupNameToPathThatIsPointingToASubFolderOfMduFolder_ThenGroupNameIsPointingToItsReferencingStructureFile()
        {
            // Make local copy of project
            const string testOutputFolder = @"TestOutput";
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(testOutputFolder, baseFolderPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);

            // Import dry points
            fmModel.Area.Gates.Add(new Gate2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), testOutputFolder, baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")
            });
            fmModel.Area.Pumps.Add(new Pump2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), testOutputFolder, baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")
            });
            fmModel.Area.Weirs.Add(new Weir2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), testOutputFolder, baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.Gates.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
            Assert.That(fmModel.Area.Pumps.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
            Assert.That(fmModel.Area.Weirs.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAStructureWithAreaFeatureGroupNameEqualToPathThatIsNotReferencedByAStructureFile_ThenGroupNameIsEqualToDefaultStructuresFileNameInTheSameFolder()
        {
            // Make local copy of project
            const string testOutputFolder = @"TestOutput";
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(testOutputFolder, baseFolderPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);

            // Import dry points
            fmModel.Area.Gates.Add(new Gate2D
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), testOutputFolder, baseFolderPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/nonReferencedGates.pli")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.Gates.FirstOrDefault().GroupName, Is.EqualTo("FeatureFiles/" + fmModel.Name + "_structures.ini"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAnAreaFeatureWithGroupNameToPathThatIsPointingToNotASubFolderOfMduFolder_ThenGroupNameIsEqualToFileName()
        {
            // Make local copy of project
            const string baseFolderPath = @"HydroAreaCollection/MduFileProjects";
            var filePath = Path.Combine(baseFolderPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");
            var mduFilePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(baseFolderPath));

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel(mduFilePath);

            // Import dry points
            fmModel.Area.DryAreas.Add(new GroupableFeature2DPolygon()
            {
                GroupName = Path.Combine(Directory.GetCurrentDirectory(), baseFolderPath, @"MyDryAreas_dry.pol")
            });

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.DryAreas.FirstOrDefault().GroupName, Is.EqualTo(@"MyDryAreas_dry.pol"));
            
        }

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenValidFmModel_WhenModelHasRun_ThenProgressTextHasBeenReset()
        {
            var originalDir = TestHelper.GetTestFilePath("flow1d2dLinks");
            var testDir = FileUtils.CreateTempDirectory();
            var mduFilePath = Path.Combine(testDir, "FlowFM.mdu");
            FileUtils.CopyDirectory(originalDir, testDir);

            var messageList = new List<string>
            {
                "Initializing",
                "0,00%",
                "(1,39%)",
                "(2,78%)",
                "(4,17%)",
                "(5,56%)",
                "(6,94%)",
                "(8,33%)",
                "(9,72%)",
                "(11,11%)",
                "(12,50%)",
                "(13,89%)",
                "(15,28%)",
                "(16,67%)",
                "(18,06%)",
                "(19,44%)",
                "(20,83%)",
                "(22,22%)",
                "(23,61%)",
                "(25,00%)",
                "(26,39%)",
                "(27,78%)",
                "(29,17%)",
                "(30,56%)",
                "(31,94%)",
                "(33,33%)",
                "(34,72%)",
                "(36,11%)",
                "(37,50%)",
                "(38,89%)",
                "(40,28%)",
                "(41,67%)",
                "(43,06%)",
                "(44,44%)",
                "(45,83%)",
                "(47,22%)",
                "(48,61%)",
                "(50,00%)",
                "(51,39%)",
                "(52,78%)",
                "(54,17%)",
                "(55,56%)",
                "(56,94%)",
                "(58,33%)",
                "(59,72%)",
                "(61,11%)",
                "(62,50%)",
                "(63,89%)",
                "(65,28%)",
                "(66,67%)",
                "(68,06%)",
                "(69,44%)",
                "(70,83%)",
                "(72,22%)",
                "(73,61%)",
                "(75,00%)",
                "(76,39%)",
                "(77,78%)",
                "(79,17%)",
                "(80,56%)",
                "(81,94%)",
                "(83,33%)",
                "(84,72%)",
                "(86,11%)",
                "(87,50%)",
                "(88,89%)",
                "(90,28%)",
                "(91,67%)",
                "(93,06%)",
                "(94,44%)",
                "(95,83%)",
                "(97,22%)",
                "(98,61%)",
                "(100,00%)",
                "Reading dia file",
                "Reading map file",
                "Reading his file",
                "00:00:00 (100,00%)"
            };

            try
            {
                var counter = 0;
                var fmModel = new WaterFlowFMModel(mduFilePath){WorkingDirectoryPathFunc = ()=> TestHelper.GetTestWorkingDirectory(TestHelper.GetCurrentMethodName())};
                fmModel.ReferenceTime = fmModel.StartTime;
                fmModel.ProgressChanged += (sender, args) =>
                {
                    Assert.IsTrue(fmModel.ProgressText.EndsWith(messageList[counter]), $"\"{fmModel.ProgressText}\" expected to end on {messageList[counter]}");
                    counter++;
                };
                ActivityRunner.RunActivity(fmModel);
                counter = 0;
                ActivityRunner.RunActivity(fmModel);
            }
            finally
            {
                FileUtils.DeleteIfExists(testDir);
            }
        }
        
        [Test]
        public void GivenModelForImporting_WhenThereAreFixedWeirs_ThenTheseFixedWeirsShouldBeCorrectlyImported()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);
            
            try
            {

                var model = new WaterFlowFMModel(mduFilePath);

                var modelvalue = model.FixedWeirsProperties[0].DataColumns[0].ValueList[0];
                Assert.That(modelvalue, Is.EqualTo(1.2));
                modelvalue = model.FixedWeirsProperties[0].DataColumns[0].ValueList[1];
                Assert.That(modelvalue, Is.EqualTo(6.4));
                modelvalue = model.FixedWeirsProperties[0].DataColumns[1].ValueList[0];
                Assert.That(modelvalue, Is.EqualTo(3.5));

                //To do test write function also

            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Test]
        public void CreateFixedWeirAndChangeSchemeAndNumberOfCoordinates()
        {
            var lineGeomery = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var fixedWeir = new DelftTools.Hydro.Structures.FixedWeir { Geometry = lineGeomery };

            var fmModel = new WaterFlowFMModel();

            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("8");
            fmModel.Area.FixedWeirs.Add(fixedWeir);

            var allData = fmModel.FixedWeirsProperties;
           
            Assert.That(allData.Count, Is.EqualTo(1));

            var modelFeatureCoordinateData = allData.First();

            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(3));
            Assert.That(modelFeatureCoordinateData.DataColumns.First().ValueList.Count, Is.EqualTo(4));

            fixedWeir.Geometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0),
                new Coordinate(0, 100),
            });

            allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(1));

            modelFeatureCoordinateData = allData.First();

            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(3));
            Assert.That(modelFeatureCoordinateData.DataColumns.First().ValueList.Count, Is.EqualTo(5));

            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("9");

            allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(1));

            modelFeatureCoordinateData = allData.First();

            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(7));

            foreach (var dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(5));
                Assert.That(dataColumn.IsActive, Is.True);
            }

            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("6");

            allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(1));
            modelFeatureCoordinateData = allData.First();
            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(7));

            foreach (var dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(5));

                if (dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLengthColumnName ||
                    dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludUpColumnName ||
                    dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludDownColumnName ||
                    dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.VegetationCoefficientColumnName)
                    Assert.That(dataColumn.IsActive, Is.False);
                else
                    Assert.That(dataColumn.IsActive, Is.True);
            }

            fixedWeir.Geometry = lineGeomery;

            allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(1));
            modelFeatureCoordinateData = allData.First();

            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(7));
            foreach (var dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(4));
            }

            fmModel.Area.FixedWeirs.Remove(fixedWeir);

            allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(0));

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenAnFMModel_WhenCloningThisModel_ThenTheNewFixedWeirPropertiesShouldBeLinkedToTheNewFixedWeirs()
        {
            var mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM.mdu"); //model with two fixed weirs and every fixed weir has two coordinates.
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            var mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);

            try
            {
                var fmModel = new WaterFlowFMModel(mduFilePath);
                var clonedFmModel = fmModel.DeepClone() as WaterFlowFMModel;

                Assert.NotNull(clonedFmModel);

                Assert.That(fmModel.FixedWeirsProperties[0].Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties[0].Feature));
                Assert.That(fmModel.FixedWeirsProperties[1].Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties[1].Feature));
                Assert.That(fmModel.FixedWeirsProperties[0].Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties[1].Feature));
                Assert.That(fmModel.FixedWeirsProperties[1].Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties[0].Feature));

                Assert.That(fmModel.FixedWeirsProperties[0].Feature, Is.SameAs(fmModel.Area.FixedWeirs[0]));
                Assert.That(fmModel.FixedWeirsProperties[1].Feature, Is.SameAs(fmModel.Area.FixedWeirs[1]));
                Assert.That(clonedFmModel.FixedWeirsProperties[0].Feature, Is.SameAs(clonedFmModel.Area.FixedWeirs[0]));
                Assert.That(clonedFmModel.FixedWeirsProperties[1].Feature, Is.SameAs(clonedFmModel.Area.FixedWeirs[1]));

                var lineGeomery = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 10),
                    new Coordinate(10, 0),
                    new Coordinate(0, 0)
                });

                fmModel.Area.FixedWeirs[0].Geometry = lineGeomery;

                Assert.That(fmModel.FixedWeirsProperties[0].DataColumns[0].ValueList.Count, Is.EqualTo(4));
                Assert.That(clonedFmModel.FixedWeirsProperties[0].DataColumns[0].ValueList.Count, Is.EqualTo(2));
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }

        }

        [Test]
        public void Given_EmptyFmModel_When_ChangingMeteoTimeSeriesValue_Then_ModelShouldHaveThisChange()
        {
            var model = new WaterFlowFMModel();
            Assert.IsNotNull(model.FmMeteoFields);

            var meteoField = CreateMeteoField();
            model.ModelDefinition.FmMeteoFields.Add(meteoField);

            Assert.That(model.FmMeteoFields.Count, Is.GreaterThan(0));
            Assert.That(model.FmMeteoFields[0].Data.Components[0].Values[0], Is.EqualTo(1).Within(0.1));
        }

        private static FmMeteoField CreateMeteoField()
        {
            var meteoField = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
            var dateTimeNow = DateTime.Now;
            meteoField.Data.Arguments[0].SetValues(new[] { dateTimeNow, dateTimeNow.AddHours(1), dateTimeNow.AddHours(2) });
            meteoField.Data.Components[0].SetValues(new[] { 1.0, 5.0, 10.0 });

            return meteoField;
        }
        
        [Test]
        public void Synchronize_Outlet_and_Boundary_Data()
        {
            //setup testcase

            var fmModel = new WaterFlowFMModel();
            var manhole = new Manhole("tm");
            var outlet = new OutletCompartment("outlet") { SurfaceLevel = 0.0, Geometry = new Point(0, 0) };
            manhole.Compartments.Add(outlet);
            
            fmModel.Network.Nodes.Add(manhole);

            var boundary = fmModel.BoundaryConditions1D.FirstOrDefault(b => b.Node.Name == manhole.Name); //data on manhole of compartment, yep ...
            Assert.IsNotNull(boundary);

            //set data in outlet 

            outlet.SurfaceWaterLevel = 1234.567;

            Assert.AreEqual(outlet.SurfaceWaterLevel,boundary.WaterLevel);
        }

        [Test] // Test related to marking model dirty
        public void ChannelFrictionDefinitions_ChangeProperty_BubblesPropertyChanged()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel());
            var waterFlowFmModel = new WaterFlowFMModel
            {
                ChannelFrictionDefinitions =
                {
                    channelFrictionDefinition
                }
            };

            var counter = 0;
            ((INotifyPropertyChanged) waterFlowFmModel).PropertyChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelFrictionDefinition) && args.PropertyName == nameof(ChannelFrictionDefinition.SpecificationType))
                {
                    counter++;
                }
            };

            // Call
            channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.ConstantChannelFrictionDefinition;

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test] // Test related to marking model dirty
        public void ChannelFrictionDefinitions_ChangeCollection_BubblesCollectionChanged()
        {
            // Setup
            var channelFrictionDefinition = new ChannelFrictionDefinition(new Channel())
            {
                SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition,
                SpatialChannelFrictionDefinition =
                {
                    FunctionType = RoughnessFunction.Constant
                }
            };

            var waterFlowFmModel = new WaterFlowFMModel
            {
                ChannelFrictionDefinitions =
                {
                    channelFrictionDefinition
                }
            };

            var counter = 0;
            ((INotifyCollectionChanged) waterFlowFmModel).CollectionChanged += (sender, args) =>
            {
                if (ReferenceEquals(sender, channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions))
                {
                    counter++;
                }
            };

            // Call
            channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.Add(new ConstantSpatialChannelFrictionDefinition());

            // Assert
            Assert.AreEqual(1, counter);
        }

        [Test]
        public void GivenFMModelWith1DChannelNetworkWithBranchWithBoundaryCondition1DAtNode2_WhenAddingAndConnectionANewBranchTargetNodeToNode2_ThenBoundaryCondition1DAtNode2WillNotBeRemoved()
        {
            // Setup
            var model = new WaterFlowFMModel();
            HydroNetworkHelper.AddSnakeHydroNetwork(model.Network,new [] {new Point(0,0),new Point(100,0)});
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(2));
            model.BoundaryConditions1D[1].DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            
            //Call
            var channel = new Channel();
            model.Network.Branches.Add(channel);
            var sourceNode = new HydroNode()
            {
                Name = "Node3",
                Geometry = new Point(100, 100),
            };
            model.Network.Nodes.Add(sourceNode);
            channel.Source = sourceNode;
            channel.Target = model.Network.Nodes[1];

            // Assert
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(3));
            Assert.That(model.BoundaryConditions1D[1].DataType, Is.EqualTo(Model1DBoundaryNodeDataType.WaterLevelConstant));
        }

        [Test]
        public void GivenFMModelWith1DChannelNetworkWithBranchWithBoundaryCondition1DAtNode2_WhenAddingAndConnectionANewBranchTargetNodeToNode2AndOneBranchWithSourceFromNode2_ThenBoundaryCondition1DAtNode2WillBeRemoved()
        {
            // Setup
            var model = new WaterFlowFMModel();
            HydroNetworkHelper.AddSnakeHydroNetwork(model.Network,new [] {new Point(0,0),new Point(100,0)});
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(2));
            model.BoundaryConditions1D[1].DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            
            //Call2
            var channelToNode2 = new Channel();
            model.Network.Branches.Add(channelToNode2);
            var sourceNode = new HydroNode()
            {
                Name = "Node3",
                Geometry = new Point(100, 100),
            };
            model.Network.Nodes.Add(sourceNode);
            channelToNode2.Source = sourceNode;
            channelToNode2.Target = model.Network.Nodes[1];

            //Assert1
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(3));
            Assert.That(model.BoundaryConditions1D[1].DataType, Is.EqualTo(Model1DBoundaryNodeDataType.WaterLevelConstant));

            //Call2
            var channelFromNode2 = new Channel();
            model.Network.Branches.Add(channelFromNode2);
            var targetNode = new HydroNode()
            {
                Name = "Node4",
                Geometry = new Point(0, 100),
            };
            model.Network.Nodes.Add(targetNode);
            channelFromNode2.Source = model.Network.Nodes[1];
            channelFromNode2.Target = targetNode; 

            // Assert2
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(4));
            Assert.That(model.BoundaryConditions1D[1].DataType, Is.EqualTo(Model1DBoundaryNodeDataType.None));
        }

        [Test]
        public void GivenFMModelWith1DChannelNetworkWithBranchWithBoundaryCondition1DAtNode2_WhenAddingAndConnectionANewBranchSourceNodeToNode2_ThenBoundaryCondition1DAtNode2WillBeRemoved()
        {
            // Setup
            var model = new WaterFlowFMModel();
            HydroNetworkHelper.AddSnakeHydroNetwork(model.Network,new [] {new Point(0,0),new Point(100,0)});
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(2));
            model.BoundaryConditions1D[1].DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            
            //Call
            var channel = new Channel();
            model.Network.Branches.Add(channel);
            var targetNode = new HydroNode()
            {
                Name = "Node3",
                Geometry = new Point(100, 100),
            };
            model.Network.Nodes.Add(targetNode);
            channel.Source = model.Network.Nodes[1]; 
            channel.Target = targetNode;

            // Assert
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(3));
            Assert.That(model.BoundaryConditions1D[1].DataType, Is.EqualTo(Model1DBoundaryNodeDataType.None));
        }

        [Test]
        public void GivenFMModelWithNetworkWithBranchWithBoundaryCondition1DAtNode_WhenAddingBranchConnectionWithTargetNodeToNode2AndOneBranchWithSourceFromNode2_ThenBoundaryCondition1DAtNode2WillBeRemoved()
        {
            // Setup
            var channelToNode2 = new Channel();
            var sourceNode = new HydroNode();
            var channelFromNode2 = new Channel();
            var targetNode = new HydroNode();

            var eventedListOfSourceNodeIncomingBranches = new EventedList<IBranch>();
            var eventedListOfSourceNodeOutgoingBranches = new EventedList<IBranch>();
            var eventedListOfTargetNodeIncomingBranches = new EventedList<IBranch>();
            var eventedListOfTargetNodeOutgoingBranches = new EventedList<IBranch>();
            var eventedListOfEmptyLinks = new EventedList<HydroLink>();

            var model = new WaterFlowFMModel();
            HydroNetworkHelper.AddSnakeHydroNetwork(model.Network, new[] { new Point(0, 0), new Point(100, 0) });
            channelToNode2.Network = model.Network;
            channelToNode2.Length = 100;

            channelFromNode2.Network = model.Network;
            channelFromNode2.Length = 100;

            sourceNode.Network = model.Network;
            sourceNode.Name = "Node3";
            sourceNode.IncomingBranches = eventedListOfSourceNodeIncomingBranches;
            sourceNode.OutgoingBranches = eventedListOfSourceNodeOutgoingBranches;
            sourceNode.Links = eventedListOfEmptyLinks;

            targetNode.Network = model.Network;
            targetNode.Name = "Node4";
            targetNode.IncomingBranches = eventedListOfTargetNodeIncomingBranches;
            targetNode.OutgoingBranches = eventedListOfTargetNodeOutgoingBranches;
            targetNode.Links = eventedListOfEmptyLinks;

            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(2));
            model.BoundaryConditions1D[1].DataType = Model1DBoundaryNodeDataType.WaterLevelConstant;
            
            //Call1
            model.Network.Branches.Add(channelToNode2);
            model.Network.Nodes.Add(sourceNode);
            channelToNode2.Source = sourceNode;
            channelToNode2.Target = model.Network.Nodes[1];

            //Assert1
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(3));
            Assert.That(model.BoundaryConditions1D[1].DataType, Is.EqualTo(Model1DBoundaryNodeDataType.WaterLevelConstant));

            //Call2
            model.Network.Branches.Add(channelFromNode2);
            model.Network.Nodes.Add(targetNode);
            channelFromNode2.Source = model.Network.Nodes[1];
            channelFromNode2.Target = targetNode;

            // Assert2
            Assert.That(model.BoundaryConditions1D.Count, Is.EqualTo(4));
            Assert.That(model.BoundaryConditions1D[1].DataType, Is.EqualTo(Model1DBoundaryNodeDataType.None));
        }

        [Test]
        public void GivenWaterFlowFMModel_WhenChangingRandomModelProperty_ThenAlwaysTriggersWaterFlowFMModelOnPropertyChanged()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                WaterFlowFMModelDefinition modelDefinition = model.ModelDefinition;
                
                int propertyChangedCounter = 0;
                model.PropertyChanged += (sender, args) =>
                {
                    propertyChangedCounter++;
                };

                foreach (WaterFlowFMProperty property in modelDefinition.Properties)
                {
                    string propertyName = property.PropertyDefinition.MduPropertyName;
                    string defaultValue = property.PropertyDefinition.DefaultValueAsString;
                    
                    // Call
                    modelDefinition.SetModelProperty(propertyName, defaultValue);
                    
                    // Assert
                    Assert.That(propertyChangedCounter, Is.GreaterThan(0)); // Apparently, the event can be fired multiple times
                    propertyChangedCounter = 0; // Reset counter
                }
            }
        }

        [Test]
        [TestCase("")]
        [TestCase("a/b")]
        [TestCase("a/b/c/d")]
        [TestCase("randomString")]
        public void GetDataItemsByItemString_InvalidItemString_ThrowsArgumentException(string invalidItemString)
        {
            // Setup
            const string randomItemString = "a/random/item string";
            
            var model = new WaterFlowFMModel();

            // Call
            TestDelegate call = () => model.GetDataItemsByItemString(invalidItemString, randomItemString);

            // Assert
            Assert.That(call, Throws.ArgumentException
                                    .With.Message.EqualTo($"{invalidItemString} should contain a category, feature name and a parameter name."));
        }
        
        [Test]
        public void GetDataItemsByItemString_UnknownFeature_ThrowsArgumentException()
        {
            // Setup
            const string randomItemString = "a/random/item string";
            
            string unknownFeatureName = "unknownPump";
            string unknownFeatureItemString = $"{KnownFeatureCategories.Pumps}/{unknownFeatureName}/capacity";

            var model = new WaterFlowFMModel();

            // Call
            TestDelegate call = () => model.GetDataItemsByItemString(unknownFeatureItemString, randomItemString);

            // Assert
            Assert.That(call, Throws.ArgumentException
                                    .With.Message.EqualTo($"feature {unknownFeatureName} in {unknownFeatureItemString} cannot be found in the FM model."));
        }
        
        [Test]
        public void GetDataItemsByItemString_UnknownParameterNames_ReturnsNull()
        {
            // Setup
            const string unknownParameterName = "unknownParameter1";
            const string unknownParameterName2 = "unknownParameter2";
            
            var model = new WaterFlowFMModel();

            const string pumpName = "testPump";
            var pump = new Pump2D(pumpName);
            model.Area.Pumps.Add(pump);
            
            // Call
            string unknownParameterItemString = $"{KnownFeatureCategories.Pumps}/{pumpName}/{unknownParameterName}";
            string unknownParameterItemString2 = $"{KnownFeatureCategories.Pumps}/{pumpName}/{unknownParameterName2}";
            IEnumerable<IDataItem> dataItems = model.GetDataItemsByItemString(unknownParameterItemString, unknownParameterItemString2);

            // Assert
            Assert.That(dataItems, Is.Null);
        }

        [Test]
        public void GetDataItemsByItemString_ValidParameterName_ReturnsExpectedDataItem()
        {
            // Setup
            var model = new WaterFlowFMModel();
            
            const string pumpName = "testPump";
            var pump = new Pump2D(pumpName);
            model.Area.Pumps.Add(pump);
            
            // Call
            var itemString = $"{KnownFeatureCategories.Pumps}/{pumpName}/capacity";
            string unknownParameterItemString = $"{KnownFeatureCategories.Pumps}/{pumpName}/unknownParameterName";
            IEnumerable<IDataItem> dataItems = model.GetDataItemsByItemString(itemString, unknownParameterItemString);

            // Assert
            Assert.That(dataItems.Count(), Is.EqualTo(1));
            IDataItem dataItem = dataItems.First();
            
            Assert.That(dataItem.Name, Is.EqualTo(pumpName));
        }
        
        [Test]
        public void GetDataItemsByItemString_ValidParameterName2_ReturnsExpectedDataItem()
        {
            // Setup
            var model = new WaterFlowFMModel();
            
            const string pumpName = "testPump";
            var pump = new Pump2D(pumpName);
            model.Area.Pumps.Add(pump);
            
            // Call
            string unknownParameterItemString = $"{KnownFeatureCategories.Pumps}/{pumpName}/unknownParameterName";
            var itemString = $"{KnownFeatureCategories.Pumps}/{pumpName}/capacity";
            IEnumerable<IDataItem> dataItems = model.GetDataItemsByItemString(unknownParameterItemString, itemString);

            // Assert
            Assert.That(dataItems.Count(), Is.EqualTo(1));
            IDataItem dataItem = dataItems.First();
            
            Assert.That(dataItem.Name, Is.EqualTo(pumpName));
        }
        
        [Test]
        public void GetDirectChildren_ReturnsCorrectObjects()
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                // Call
                object[] objects = model.GetDirectChildren().ToArray();
                
                // Assert
                Assert.That(objects, Contains.Item(model.InitialWaterLevel));
                Assert.That(objects, Contains.Item(model.InitialTemperature));
                Assert.That(objects, Contains.Item(model.InitialSalinity));
                Assert.That(objects, Contains.Item(model.Roughness));
                Assert.That(objects, Contains.Item(model.Viscosity));
                Assert.That(objects, Contains.Item(model.Diffusivity));
                Assert.That(objects, Contains.Item(model.Infiltration));
            }
        }

        [Test]
        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        [TestCase(3, false)]
        [TestCase(4, false)]
        public void UseInfiltration_ReturnsCorrectResult(int infiltrationModel, bool expResult)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.ModelDefinition.GetModelProperty("infiltrationmodel").SetValueAsString(infiltrationModel.ToString());

                // Call
                bool result = model.UseInfiltration;
                
                // Assert
                Assert.That(result, Is.EqualTo(expResult));
            }
        }
        
        [Test]
        [TestCaseSource(nameof(AddLateralSourceToNetworkBranchCases))]
        [Category(TestCategory.Integration)]
        public void AddLateralSourceToNetworkBranch_AddsCorrectLateralSourcesDataToModel(IPipe pipe1, IPipe pipe2, IPipe lateralSourcePipe, double chainage, ICompartment expCompartment)
        {
            using (var model = new WaterFlowFMModel())
            {
                // Setup
                var network = new HydroNetwork();
                network.Branches.Add(pipe1);
                network.Branches.Add(pipe2);

                model.Network = network;

                var lateralSource = new LateralSource
                {
                    Branch = lateralSourcePipe,
                    Chainage = chainage,
                };

                // Call
                lateralSourcePipe.BranchFeatures.Add(lateralSource);

                // Assert
                Model1DLateralSourceData lateralSourceData = model.LateralSourcesData.Single();
                Assert.That(lateralSourceData.Compartment, Is.SameAs(expCompartment));
                Assert.That(lateralSourceData.Feature, Is.SameAs(lateralSource));
                Assert.That(lateralSourceData.UseTemperature, Is.False);
                Assert.That(lateralSourceData.UseSalt, Is.False);
            }
        }

        private static IEnumerable<TestCaseData> AddLateralSourceToNetworkBranchCases()
        {
            IPipe pipeA1 = CreatePipe(100, 0, 200, 0);
            IPipe pipeA2 = CreatePipe(200, 0, 300, 0);
            yield return new TestCaseData(pipeA1, pipeA2, pipeA1, 0, pipeA1.SourceCompartment);

            IPipe pipeB1 = CreatePipe(100, 0, 200, 0);
            IPipe pipeB2 = CreatePipe(200, 0, 300, 0);
            yield return new TestCaseData(pipeB1, pipeB2, pipeB1, 100, pipeB1.TargetCompartment);

            IPipe pipeC1 = CreatePipe(100, 0, 200, 0);
            IPipe pipeC2 = CreatePipe(200, 0, 300, 0);
            yield return new TestCaseData(pipeC1, pipeC2, pipeC2, 0, pipeC2.SourceCompartment);

            IPipe pipeD1 = CreatePipe(100, 0, 200, 0);
            IPipe pipeD2 = CreatePipe(200, 0, 300, 0);
            yield return new TestCaseData(pipeD1, pipeD2, pipeD2, 100, pipeD2.TargetCompartment);

            IPipe pipeE1 = CreatePipe(100, 0, 200, 0);
            IPipe pipeE2 = CreatePipe(200, 0, 300, 0);
            yield return new TestCaseData(pipeE1, pipeE2, pipeE1, 50, null);

            IPipe pipeF1 = CreatePipe(100, 0, 200, 0);
            IPipe pipeF2 = CreatePipe(200, 0, 300, 0);
            yield return new TestCaseData(pipeF1, pipeF2, pipeF2, 50, null);
        }

        private static IPipe CreatePipe(double x1, double y1, double x2, double y2)
        {
            var c1 = new Coordinate(x1, y1);
            var c2 = new Coordinate(x2, y2);

            var geometry = new LineString(new[]
            {
                c1,
                c2
            });

            double length = Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            var pipe = new Pipe
            {
                Length = length,
                Geometry = geometry,
                SourceCompartment = Substitute.For<ICompartment>(),
                TargetCompartment = Substitute.For<ICompartment>()
            };

            return pipe;
        }
        
        [Test]
        [TestCaseSource(nameof(CreateRetrieveChildDataItemLocationTestCases))]
        public IEnumerable<IFeature> GivenAModel_WhenRetrievingChildDataItemLocations_ReturnsItemsWithSpecifiedRole(
            WaterFlowFMModel model, DataItemRole role)
        {
            return model.GetChildDataItemLocations(role);
        }

        private static IEnumerable<TestCaseData> CreateRetrieveChildDataItemLocationTestCases()
        {
            var model = new WaterFlowFMModel();

            var pump = new Pump2D();
            var weir = new Weir2D();
            var observationPoint = new GroupableFeature2DPoint();
            var observationCrossSection = new ObservationCrossSection2D();

            model.Area.Pumps.Add(pump);
            model.Area.Weirs.Add(weir);
            model.Area.ObservationPoints.Add(observationPoint);
            model.Area.ObservationCrossSections.Add(observationCrossSection);

            var empty = new IFeature[] {};
            var inputs = new IFeature[] { pump, weir };
            var outputs = new IFeature[] { pump, weir, observationPoint, observationCrossSection };

            yield return GenerateTestCaseData(DataItemRole.None, empty);
            yield return GenerateTestCaseData(DataItemRole.Input, inputs);
            yield return GenerateTestCaseData(DataItemRole.Output, outputs);
            yield break;

            TestCaseData GenerateTestCaseData(DataItemRole role, IEnumerable<IFeature> expected)
            {
                return new TestCaseData(model, role)
                       .Returns(expected)
                       .SetName(role.ToString());
            }
        }

        [Test]
        public void GivenAModel_WhenRetrievingChildDataItemsForPump_ReturnsDataItems()
        {
            var model = new WaterFlowFMModel();
            var pump = new Pump2D();

            model.Area.Pumps.Add(pump);

            IReadOnlyList<IDataItem> items = model.GetChildDataItems(pump).ToArray();

            Assert.That(items, Is.All.Matches<IDataItem>(x => ReferenceEquals(x.ComposedValue, pump)));
            Assert.That(items, Is.All.Matches<IDataItem>(x => x.Role.HasFlag(DataItemRole.Input) && x.Role.HasFlag(DataItemRole.Output)));
            Assert.That(items.Select(x => x.Tag), Is.EqualTo(new[] { "capacity" }));
        }

        [Test]
        public void GivenAModel_WhenRetrievingChildDataItemsForSimpleWeir_ReturnsDataItems()
        {
            var model = new WaterFlowFMModel();
            var weir = new Weir2D { WeirFormula = new SimpleWeirFormula() };

            model.Area.Weirs.Add(weir);

            IReadOnlyList<IDataItem> items = model.GetChildDataItems(weir).ToArray();

            Assert.That(items, Is.All.Matches<IDataItem>(x => ReferenceEquals(x.ComposedValue, weir)));
            Assert.That(items, Is.All.Matches<IDataItem>(x => x.Role.HasFlag(DataItemRole.Input) && x.Role.HasFlag(DataItemRole.Output)));
            Assert.That(items.Select(x => x.Tag), Is.EqualTo(new[] { "CrestLevel" }));
        }

        [Test]
        public void GivenAModel_WhenRetrievingChildDataItemsForGeneralStructureWeir_ReturnsDataItems()
        {
            var model = new WaterFlowFMModel();
            var weir = new Weir2D { WeirFormula = new GeneralStructureWeirFormula() };

            model.Area.Weirs.Add(weir);

            IReadOnlyList<IDataItem> items = model.GetChildDataItems(weir).ToArray();

            Assert.That(items, Is.All.Matches<IDataItem>(x => ReferenceEquals(x.ComposedValue, weir)));
            Assert.That(items, Is.All.Matches<IDataItem>(x => x.Role.HasFlag(DataItemRole.Input) && x.Role.HasFlag(DataItemRole.Output)));
            Assert.That(items.Select(x => x.Tag), Is.EqualTo(new[] { "CrestLevel", "GateHeight", "GateLowerEdgeLevel", "GateOpeningWidth", "GateOpeningHorizontalDirection" }));
        }

        [Test]
        public void GivenAModel_WhenRetrievingChildDataItemsForGeneralGatedWeir_ReturnsDataItems()
        {
            var model = new WaterFlowFMModel();
            var weir = new Weir2D { WeirFormula  = new GatedWeirFormula() };

            model.Area.Weirs.Add(weir);

            IReadOnlyList<IDataItem> items = model.GetChildDataItems(weir).ToArray();

            Assert.That(items, Is.All.Matches<IDataItem>(x => ReferenceEquals(x.ComposedValue, weir)));
            Assert.That(items, Is.All.Matches<IDataItem>(x => x.Role.HasFlag(DataItemRole.Input) && x.Role.HasFlag(DataItemRole.Output)));
            Assert.That(items.Select(x => x.Tag), Is.EqualTo(new[] { "CrestLevel", "GateHeight", "GateLowerEdgeLevel", "GateOpeningWidth", "GateOpeningHorizontalDirection" }));
        }

        [Test]
        public void GivenAModel_WhenRetrievingChildDataItemsForObservationPoint_ReturnsDataItems()
        {
            var model = new WaterFlowFMModel();
            var observationPoint = new GroupableFeature2DPoint();

            model.ModelDefinition.GetModelProperty(KnownProperties.UseSalinity).Value = true;
            model.Area.ObservationPoints.Add(observationPoint);

            IReadOnlyList<IDataItem> items = model.GetChildDataItems(observationPoint).ToArray();

            Assert.That(items, Is.All.Matches<IDataItem>(x => ReferenceEquals(x.ComposedValue, observationPoint)));
            Assert.That(items, Is.All.Matches<IDataItem>(x => x.Role.HasFlag(DataItemRole.Output)));
            Assert.That(items.Select(x => x.Tag), Is.EqualTo(new[] { "water_level", "salinity", "water_depth" }));
        }

        [Test]
        public void GivenAModel_WhenRetrievingChildDataItemsForObservationCrossSection_ReturnsDataItems()
        {
            var model = new WaterFlowFMModel();
            var observationCrossSection = new ObservationCrossSection2D();

            model.Area.ObservationCrossSections.Add(observationCrossSection);

            IReadOnlyList<IDataItem> items = model.GetChildDataItems(observationCrossSection).ToArray();

            Assert.That(items, Is.All.Matches<IDataItem>(x => ReferenceEquals(x.ComposedValue, observationCrossSection)));
            Assert.That(items, Is.All.Matches<IDataItem>(x => x.Role.HasFlag(DataItemRole.Output)));
            Assert.That(items.Select(x => x.Tag), Is.EqualTo(new[] { "discharge", "velocity", "water_level", "water_depth" }));
        }
    }

}
