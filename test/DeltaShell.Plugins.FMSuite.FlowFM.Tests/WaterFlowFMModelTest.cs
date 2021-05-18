using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core;
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
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.MapTools;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;
using SharpMapTestUtils;
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
        public void CheckDefaultPropertiesOfFMModel()
        {
            var model = new WaterFlowFMModel();

            Assert.That(model.SnapVersion, Is.EqualTo(0));
            Assert.IsTrue(model.ValidateBeforeRun);
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

        [Test]
        public void PropertyChanged()
        {
            var project = new Project();
            var model = new WaterFlowFMModel();

            var model1DBoundaryNodeData = new Model1DBoundaryNodeData();
            model.BoundaryConditions1D.Add(model1DBoundaryNodeData);
            project.RootFolder.Add(model);
/*

            var dataItem = new DataItem{
                Value = model1DBoundaryNodeData,
                ValueType = typeof(Model1DBoundaryNodeData),
                Hidden = true};
            model.BoundaryConditions1DDataItemSet.DataItems.Add(dataItem);*/

            project.PropertyChanged += (sender, args) =>
            {
                Console.WriteLine($"Sender {sender}, Arg {args}");
            };

            model1DBoundaryNodeData.DataType = Model1DBoundaryNodeDataType.FlowConstant;

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
            Assert.That(dataItems[0].Role, Is.EqualTo(DataItemRole.Input));

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
                Assert.That(dataItems[i].Role, Is.EqualTo(DataItemRole.Input));

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
            //Assert.That(3, modelCount); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
            Assert.That(modelCount, Is.EqualTo(1)); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void RunModelCheckIfStatisticsAreWrittenToDiaFile()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var workingDir = string.Empty;
            using (var model = new WaterFlowFMModel(mduPath))
            {

                ActivityRunner.RunActivity(model);
                Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned));
                workingDir = Path.Combine(model.WorkingDirectory, model.DirectoryName);
            }
            var statisticsWritten = false;
            Parallel.ForEach(File.ReadAllLines(Path.Combine(workingDir, "bendprof.dia")),
                (line, loopstate) =>
                {
                    if (line.Contains("** INFO   :"))
                    {
                        statisticsWritten = true;
                        loopstate.Stop();
                    }
                });
            Assert.That(statisticsWritten, Is.True);
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
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void RunModelTwice()
        {
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            ActivityRunner.RunActivity(model);
            var waterLevelFirstRun = (double) model.OutputWaterLevel[model.StopTime, 0];
            Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned));
            Assert.AreEqual(4.0d, waterLevelFirstRun, 0.1);

            ActivityRunner.RunActivity(model);
            var waterLevelSecondRun = (double) model.OutputWaterLevel[model.CurrentTime, 0];
            Assert.That(model.Status, Is.EqualTo(ActivityStatus.Cleaned));
            Assert.AreEqual(waterLevelSecondRun, waterLevelFirstRun, 0.005); // value changes per run (see above)

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
            Assert.That(sewerRoughnessSection.GetDefaultRoughnessValue(), Is.EqualTo(0.003));
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [Category("Quarantine")]
        public void TestWarningGivenIfDiaFileFileNotFound()
        {
            var mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            var outputDirectory = FileUtils.CreateTempDirectory();
            var diaFileName = $"{model.Name}.dia";
            var diaFilePath = Path.Combine(outputDirectory, Path.GetDirectoryName(model.ModelDefinition.RelativeMapFilePath) ?? string.Empty,  diaFileName);
            
            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                TypeUtils.CallPrivateMethod(model, "ReadDiaFile", outputDirectory),
                string.Format(Properties.Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_, diaFileName, diaFilePath)
            );
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
            var model = new WaterFlowFMModel(mduPath);

            Assert.That(model.CoordinateSystem.Name, Is.EqualTo("WGS 84"));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportIvoorkustModel()
        {
            var mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel(mduPath);

            model.Initialize();

            Assert.That(model.Status, Is.EqualTo(ActivityStatus.Initialized));
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
            var mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var model = new WaterFlowFMModel(mduPath);
            var gridFile = model.NetFilePath;

            // make grid file corrupt
            File.WriteAllText(gridFile, "");

            // attempt to reload grid
            model.ReloadGrid();

            // make sure we can still delete the file (not locked by mistake)
            File.Delete(gridFile);
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

            var refDate = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

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

            Assert.IsFalse(originalGrid == model.Grid);
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

        [Test, Category(TestCategory.WorkInProgress)]
        public void Generate1D2DLinksAutomaticallyWhenExistsBoth1D2DGrids()
        {
            var model = new WaterFlowFMModel();
            WaterFlowFMTestHelper.ConfigureDemoNetwork(model.Network);

            var offSet = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)model.Network.Branches[1], offSet);

            Assert.IsFalse(model.NetworkDiscretization == null || !model.NetworkDiscretization.Locations.AllValues.Any());

            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2);
            Assert.IsNotEmpty(model.Links);
            Assert.That(model.Links.Count, Is.Not.EqualTo(0));
        }
        
        [TestCase(LinkGeneratingType.EmbeddedOneToOne)]
        [TestCase(LinkGeneratingType.EmbeddedOneToMany, IgnoreReason = "Buizen hebben iets van een mask nodig want zijn dicht en kunnen niet halverwege uit stromen naar een grid! Tjitske en ArthurvD weten meer")]
        public void Generate1D2DLinksAutomaticallyWhenExistsBoth1DUrban2DGrids(LinkGeneratingType linkType)
        {
            var model = new WaterFlowFMModel();

            WaterFlowFMTestHelper.ConfigureDemoPipeNetwork(model.Network);

            Assert.IsFalse(model.NetworkDiscretization == null || !model.NetworkDiscretization.Locations.AllValues.Any());
            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2, 0, 0);

            // Grid and sewer system with discretization of 4 points
            // 4 +-----------------------------------------+-----------------------------------------+
            //   |                                         |                                         |
            //   |                                         |                                         |
            //   |                                         |                                         |
            //   |                                         |                                         |
            //   |                                         |                                         |
            //   |                                         |                                         |
            //3.5|                                         |  -----0                                 |
            //   |                                        ---/                                       |
            //   |                                   ----/ |                                         |
            // 3 |                               ---/      |                                         |
            //   |                          ----/          |                                         |
            //   |                      ---/               |                                         |
            //   |                    -/                   |                                         |
            //2.5|                  0--                    |                                         |
            //   |                     \--                 |                                         |
            //   |                        \--              |                                         |
            //   |                           \--           |                                         |
            //   |                              \--        |                                         |
            // 2 +---------------------------------\-------+-----------------------------------------+
            //   |                                    \--  |                                         |
            //   |                                       \---                                        |
            //   |                                         | \--                                     |
            //   |                                         |    \--                                  |
            //   |                                         |       \--                               |
            //   |                                         |          \--                            |
            //   |                                         |             \--                         |
            //   |                                         |                \--                      |
            // 1 |                                         |                   \-                    |
            //   |                                         |                 ----0                   |
            //   |                                         |          ------/                        |
            //   |                                         |   ------/                               |
            //   |                                      ------/                                      |
            //   |                               ------/   |                                         |
            //   |                        ------/          |                                         |
            //0.5|                    0---/                |                                         |
            //   |                                         |                                         |
            //   |                                         |                                         |
            // 0 +-----------------------------------------+-----------------------------------------+
            //   0                    1                    2       2.5         3                     4

            Assert.That(model.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(4));
            //create ring around grid and sewer system
            var areaCoordinates = new List<Coordinate>();
            areaCoordinates.Add(new Coordinate(-1, -1));
            areaCoordinates.Add(new Coordinate(-1, 5));
            areaCoordinates.Add(new Coordinate(5, 5));
            areaCoordinates.Add(new Coordinate(5, -1));
            areaCoordinates.Add(new Coordinate(-1, -1));

            var area = new Polygon(new LinearRing(areaCoordinates.ToArray()));

            model.Links.AddRange(MapTool1D2DLinksHelper.Generate1D2DLinks(area, linkType, model.Grid, model.Area.Gullies, model.NetworkDiscretization).ToList());

            ///this will generate 2 links:
            // +-----------------------------------------+-----------------------------------------+
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |  ---0                                   |
            // |                                        ---/                                       |
            // |                   ^               ----/ |                                         |
            // |                   |           ---/      |                                         |
            // |                  /       ----/          |                                         |
            // |                  |   ---/               |                                         |
            // |                 / --/                   |                                         |
            // |                 |0--                    |                                         |
            // |                     \--                 |                                         |
            // |                        \--              |                                         |
            // |                           \--           |                                         |
            // |                              \--        |                                         |
            // +---------------------------------\-------+-----------------------------------------+
            // |                                    \--  |                                         |
            // |                                       \---                                        |
            // |                                         | \--                                     |
            // |                                         |    \--                                  |
            // |                                         |       \--                               |
            // |                                         |          \--                            |
            // |                                         |             \--         ^               |
            // |                                         |                \--     /                |
            // |                                         |                   \-  /                 |
            // |                                         |                 ----0/                  |
            // |                                         |          ------/                        |
            // |                                         |   ------/                               |
            // |                                      ------/                                      |
            // |                               ------/   |                                         |
            // |                        ------/          |                                         |
            // |                   0---/                 |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // +-----------------------------------------+-----------------------------------------+
            Assert.IsNotEmpty(model.Links);
            Assert.That(model.Links.Count, Is.EqualTo(2));
            Assert.That(model.Links[0].DiscretisationPointIndex, Is.EqualTo(1));
            Assert.That(model.Links[0].FaceIndex, Is.EqualTo(1));
            Assert.That(model.Links[1].DiscretisationPointIndex, Is.EqualTo(2));
            Assert.That(model.Links[1].FaceIndex, Is.EqualTo(2));

            //If we now remove pipe1 only 1 links should remain like this:
            //1 link and 3 discretization points
            // +-----------------------------------------+-----------------------------------------+
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |  ---0                                   |
            // |                                        ---/                                       |
            // |                   ^               ----/ |                                         |
            // |                   |           ---/      |                                         |
            // |                  /       ----/          |                                         |
            // |                  |   ---/               |                                         |
            // |                 / --/                   |                                         |
            // |                 |0--                    |                                         |
            // |                     \--                 |                                         |
            // |                        \--              |                                         |
            // |                           \--           |                                         |
            // |                              \--        |                                         |
            // +---------------------------------\-------+-----------------------------------------+
            // |                                    \--  |                                         |
            // |                                       \---                                        |
            // |                                         | \--                                     |
            // |                                         |    \--                                  |
            // |                                         |       \--                               |
            // |                                         |          \--                            |
            // |                                         |             \--                         |
            // |                                         |                \--                      |
            // |                                         |                   \-                    |
            // |                                         |                     0                   |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // +-----------------------------------------+-----------------------------------------+

            model.Network.Branches.Remove(model.Network.Branches[0]);
            Assert.That(model.Links.Count, Is.EqualTo(1));
            Assert.That(model.Links[0].DiscretisationPointIndex, Is.EqualTo(1));
            Assert.That(model.Links[0].FaceIndex, Is.EqualTo(2));
            Assert.That(model.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(3));

            //if we now remove pipe2, NO links should be there and 2 discretization points should be there
            // +-----------------------------------------+-----------------------------------------+
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |  ---0                                   |
            // |                                        ---/                                       |
            // |                                   ----/ |                                         |
            // |                               ---/      |                                         |
            // |                          ----/          |                                         |
            // |                      ---/               |                                         |
            // |                   --/                   |                                         |
            // |                  0                      |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // +-----------------------------------------+-----------------------------------------+
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // |                                         |                                         |
            // +-----------------------------------------+-----------------------------------------+
            model.Network.Branches.Remove(model.Network.Branches[0]);
            Assert.That(model.Links.Count, Is.EqualTo(0));
            Assert.That(model.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(2));
        }
        [TestCase(LinkGeneratingType.EmbeddedOneToOne)]
        [TestCase(LinkGeneratingType.EmbeddedOneToMany, IgnoreReason = "Snap ik echt helemaal niks van...")]
        public void Generate1D2DLinksAutomaticallyWhenExistsBoth1DRural2DGrids(LinkGeneratingType linkType)
        {
            var model = new WaterFlowFMModel();

            WaterFlowFMTestHelper.ConfigureDemoNetwork(model.Network);
            //var offSet = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };//11 points
            //HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)model.Network.Branches[0], offSet);
            //offSet = new double[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150 };//16 points
            //HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization, (IChannel)model.Network.Branches[1], offSet);
            
            //Assert.IsFalse(model.NetworkDiscretization == null || !model.NetworkDiscretization.Locations.AllValues.Any());
            //Assert.That(model.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(27)); // manually generated because debugger, so 1 double point
            //model.NetworkDiscretization.Clear();
            //HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization,true, true, 10d, false, 10d, false, false, true, 10d);
            HydroNetworkHelper.GenerateDiscretization(model.NetworkDiscretization,true, false, 10d, false, 10d, false, false, true, 10d);
            Assert.That(model.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(26));// auto generated, so no double points!

            model.Grid = UnstructuredGridTestHelper.GenerateRegularGrid(100, 100, 2, 2, 0, 0);
            //create ring around grid and rural
            var areaCoordinates = new List<Coordinate>();
            areaCoordinates.Add(new Coordinate(-1, -1));
            areaCoordinates.Add(new Coordinate(-1, 201));
            areaCoordinates.Add(new Coordinate(201, 201));
            areaCoordinates.Add(new Coordinate(201, -1));
            areaCoordinates.Add(new Coordinate(-1, -1));

            var area = new Polygon(new LinearRing(areaCoordinates.ToArray()));

            model.Links.AddRange(MapTool1D2DLinksHelper.Generate1D2DLinks(area, linkType, model.Grid, model.Area.Gullies, model.NetworkDiscretization).ToList());
            
            Assert.IsNotEmpty(model.Links);
            Assert.That(model.Links.Count, Is.EqualTo(24));//no links at start and end points 26-2 = 24
            Assert.That(model.Links[0].DiscretisationPointIndex, Is.EqualTo(1));
            Assert.That(model.Links[0].FaceIndex, Is.EqualTo(4));
            Assert.That(model.Links[1].DiscretisationPointIndex, Is.EqualTo(2));
            Assert.That(model.Links[1].FaceIndex, Is.EqualTo(9));
            
            model.Network.Branches.Remove(model.Network.Branches[0]);
            Assert.That(model.NetworkDiscretization.Locations.Values.Count, Is.EqualTo(15));
            Assert.That(model.Links.Count, Is.EqualTo(14)); //no links at start and end points 16-2 = 14
            Assert.That(model.Links[0].DiscretisationPointIndex, Is.EqualTo(1));
            Assert.That(model.Links[0].FaceIndex, Is.EqualTo(449));
            Assert.That(model.Links[1].DiscretisationPointIndex, Is.EqualTo(2));
            Assert.That(model.Links[1].FaceIndex, Is.EqualTo(949));
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
       [NUnit.Framework.Category(TestCategory.VerySlow)]
        [Category("Quarantine")]
        public void RunModelWithGeneralStructuresAcceptanceTest()
        {
            var filePath = TestHelper.GetTestFilePath(@"GeneralStructures\BasicModel\FlowFM.mdu");
            Assert.IsTrue( File.Exists(filePath) );
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath));

            using (var model = new WaterFlowFMModel(filePath))
            {
                Assert.That(model.Boundaries.Count, Is.EqualTo(2));
                Assert.That(model.Area.Weirs.Count, Is.EqualTo(1));
                Assert.That(model.Area.ObservationPoints.Count, Is.EqualTo(3));

                /* Verify the OP exist and OP1.X < OP2.X < OP2.X and the Y is the same */
                var op1 = model.Area.ObservationPoints[0];
                var op2 = model.Area.ObservationPoints[1];
                var op3 = model.Area.ObservationPoints[2];

                Assert.NotNull(op1);
                Assert.NotNull(op2);
                Assert.NotNull(op3);

                Assert.That(op2.Geometry.Coordinate.Y, Is.EqualTo(op1.Geometry.Coordinate.Y));
                Assert.That(op3.Geometry.Coordinate.Y, Is.EqualTo(op1.Geometry.Coordinate.Y));
                Assert.Less(op1.Geometry.Coordinate.X, op2.Geometry.Coordinate.X);
                Assert.Less(op2.Geometry.Coordinate.X, op3.Geometry.Coordinate.X);

                /* Check the Weir currently is a General Structure */
                var weir2D = model.Area.Weirs.First();
                Assert.IsTrue( weir2D.WeirFormula is GeneralStructureWeirFormula);

                //Storing results for the General Structure
                var op1WLResults = new List<double>();
                var op2WLResults = new List<double>();
                var op3WLResults = new List<double>();
                
                var op1VelResults = new List<double>();
                var op2VelResults = new List<double>();
                var op3VelResults = new List<double>();

                try
                {
                    ActivityRunner.RunActivity(model);
                    /*Water Level*/
                    op1WLResults = GetWaterLevelValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2WLResults = GetWaterLevelValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3WLResults = GetWaterLevelValuesAtPoint(model, op3.Geometry.Coordinate);
                    Assert.IsNotEmpty(op1WLResults);
                    Assert.IsNotEmpty(op2WLResults);
                    Assert.IsNotEmpty(op3WLResults);

                    /*Velocity*/
                    op1VelResults = GetVelocityValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2VelResults = GetVelocityValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3VelResults = GetVelocityValuesAtPoint(model, op3.Geometry.Coordinate);

                    Assert.IsNotEmpty(op1VelResults);
                    Assert.IsNotEmpty(op2VelResults);
                    Assert.IsNotEmpty(op3VelResults);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                #region Check Water Level
                /* OP2TN < OP1TN && OP2TN < OP3TN */
                Assert.That(op2WLResults.Count, Is.EqualTo(op1WLResults.Count));
                Assert.That(op3WLResults.Count, Is.EqualTo(op1WLResults.Count));

                var nResults = op1WLResults.Count;
                var lastResult = nResults - 1;

                /* Ensure the LAST time recorded on the OP2 has some water level. */
                var op2ValidValues = op2WLResults.Where(val => val > 0.0).ToList();
                var op3ValidValues = op3WLResults.Where(val => val > 0.0).ToList();

                Assert.IsNotEmpty(op2ValidValues);
                Assert.IsNotEmpty(op3ValidValues);
                
                /* Check at any time the OP2 or OP3 are less than at the same time at OP1
                  and that OP2 results are always Greater Or Equal than OP3 */
                for (int i = 1; i < nResults; i++)
                {
                    Assert.Less(op2WLResults[i], op1WLResults[i]);
                    Assert.Less(op3WLResults[i], op1WLResults[i]);
                    Assert.GreaterOrEqual(op2WLResults[i], op3WLResults[i]);
                }
                #endregion

                #region Check Velocity

                /* OP2TN > OP1TN && OP2TN > OP3TN */
                Assert.That(op1VelResults.Count, Is.EqualTo(op1WLResults.Count));
                Assert.That(op2VelResults.Count, Is.EqualTo(op1VelResults.Count));
                Assert.That(op3VelResults.Count, Is.EqualTo(op1VelResults.Count));

                Assert.Greater(op2VelResults[lastResult], op1VelResults[lastResult]);
                #endregion
            }

            FileUtils.DeleteIfExists(filePath);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.VerySlow)]
        [Category("Quarantine")]
        public void ResultsFromWeirGeneralStructuresShouldDifferFromSimpleWeirAcceptanceTest()
        {
            var filePath = TestHelper.GetTestFilePath(@"GeneralStructures\BasicModel\FlowFM.mdu");
            Assert.IsTrue(File.Exists(filePath));
            filePath = TestHelper.CreateLocalCopy(filePath);
            Assert.IsTrue(File.Exists(filePath));

            using (var model = new WaterFlowFMModel(filePath))
            {
                Assert.That(model.Boundaries.Count, Is.EqualTo(2));
                Assert.That(model.Area.Weirs.Count, Is.EqualTo(1));
                Assert.That(model.Area.ObservationPoints.Count, Is.EqualTo(3));

                /* Verify the OP exist and OP1.X < OP2.X < OP2.X and the Y is the same */
                var op1 = model.Area.ObservationPoints[0];
                var op2 = model.Area.ObservationPoints[1];
                var op3 = model.Area.ObservationPoints[2];

                Assert.NotNull(op1);
                Assert.NotNull(op2);
                Assert.NotNull(op3);

                Assert.That(op2.Geometry.Coordinate.Y, Is.EqualTo(op1.Geometry.Coordinate.Y));
                Assert.That(op3.Geometry.Coordinate.Y, Is.EqualTo(op2.Geometry.Coordinate.Y));

                Assert.Less(op1.Geometry.Coordinate.X, op2.Geometry.Coordinate.X);
                Assert.Less(op2.Geometry.Coordinate.X, op3.Geometry.Coordinate.X);

                /* Check the Weir currently is a General Structure */
                var weir2D = model.Area.Weirs.First();
                Assert.IsTrue(weir2D.WeirFormula is GeneralStructureWeirFormula);

                //Storing results for the General Structure
                var op1GSResults = new List<double>();
                var op2GSResults = new List<double>();
                var op3GSResults = new List<double>();

               //Storing results for the Simple Weir
                var op1SWResults = new List<double>();
                var op2SWResults = new List<double>();
                var op3SWResults = new List<double>();

                try
                {
                    ActivityRunner.RunActivity(model);
                    /*Water Level*/
                    op1GSResults = GetWaterLevelValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2GSResults = GetWaterLevelValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3GSResults = GetWaterLevelValuesAtPoint(model, op3.Geometry.Coordinate);
                    Assert.IsNotEmpty(op1GSResults);
                    Assert.IsNotEmpty(op2GSResults);
                    Assert.IsNotEmpty(op3GSResults);

                    //Change weir to another kind of formula.
                    weir2D.WeirFormula = new SimpleWeirFormula();
                    Assert.IsFalse(weir2D.WeirFormula is GeneralStructureWeirFormula);

                    ActivityRunner.RunActivity(model);
                    op1SWResults = GetWaterLevelValuesAtPoint(model, op1.Geometry.Coordinate);
                    op2SWResults = GetWaterLevelValuesAtPoint(model, op2.Geometry.Coordinate);
                    op3SWResults = GetWaterLevelValuesAtPoint(model, op3.Geometry.Coordinate);
                    Assert.IsNotEmpty(op1SWResults);
                    Assert.IsNotEmpty(op2SWResults);
                    Assert.IsNotEmpty(op3SWResults);
                }
                catch (Exception e)
                {
                    Assert.Fail("Fail to run model: {0}", e.Message);
                }

                /* Make sure the results are not the same when being a General Structure or Plain Weir*/
                Assert.AreNotEqual(op1GSResults, op1SWResults);
                Assert.AreNotEqual(op2GSResults, op2SWResults);
                Assert.AreNotEqual(op3GSResults, op3SWResults);
            }

            FileUtils.DeleteIfExists(filePath);
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

        /* Clone of function from WaterFlowFMModel */
        private static List<double> GetWaterLevelValuesAtPoint(WaterFlowFMModel model, Coordinate measureLocation)
        {
            var result = new List<double>();
            if (model == null || model.OutputWaterLevel == null || model.OutputWaterLevel.Time == null)
            {
                Assert.Fail("Water level coverage not found.");
            }

            foreach (var time in model.OutputWaterLevel.Time.Values)
                result.Add((double)model.OutputWaterLevel.Evaluate(measureLocation, time));

            return result;
        }
        /* Custom made function to retreive velocity */
        private static List<double> GetVelocityValuesAtPoint(WaterFlowFMModel model, Coordinate measureLocation)
        {
            var result = new List<double>();
            if (model == null || model.OutputMapFileStore == null ||
                model.OutputMapFileStore.CustomVelocityCoverage == null)
            {
                Assert.Fail("Velocity coverage not found.");
            }

            var velocity = model.OutputMapFileStore.CustomVelocityCoverage as UnstructuredGridCellCoverage;
            if (velocity == null || velocity.Time == null)
            {
                Assert.Fail("Velocity coverage not found.");
            }

            foreach (var time in velocity.Time.Values)
                result.Add((double)velocity.Evaluate(measureLocation, time));

            return result;
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
        [NUnit.Framework.Category(TestCategory.Slow)]
        [Category("Quarantine")]
        public void GivenFMModelWithPrecipitationDataWhenDeepCloneThenReadBackMeteoData()
        {
            TestHelper.PerformActionInTemporaryDirectory(s =>
            {
                var model = new WaterFlowFMModel();

                var meteoPrecipitationSeries = FmMeteoField.CreateMeteoPrecipitationSeries(FmMeteoLocationType.Global);
                var dateTimeNow = DateTime.Today;
                meteoPrecipitationSeries.Data.Arguments[0].SetValues(new[]
                    {dateTimeNow, dateTimeNow.AddHours(1), dateTimeNow.AddHours(2)});
                meteoPrecipitationSeries.Data.Components[0].SetValues(new[] {1.0, 5.0, 10.0});

                model.ModelDefinition.FmMeteoFields.Add(meteoPrecipitationSeries);
                var clone = (FmMeteoField) meteoPrecipitationSeries.Clone();
                TypeUtils.SetField(clone, "fmMeteoLocationType", FmMeteoLocationType.Feature);
                TypeUtils.CallPrivateMethod(clone, "UpdateName");
                meteoPrecipitationSeries.Data.Components[0].Values[1] = 3.0;
                Assert.That(clone.Data.Components[0].Values[1], Is.EqualTo(5.0).Within(0.1));
                clone.Data.Components[0].Values[1] = 7.0;
                Assert.That(meteoPrecipitationSeries.Data.Components[0].Values[1], Is.EqualTo(3.0).Within(0.1));

                model.ModelDefinition.FmMeteoFields.Add(clone);
                WaterFlowFMModel otherModel = null;
                TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                {
                    otherModel = (WaterFlowFMModel) model.DeepClone();
                }, "Could not parse locationtype feature into a valid meteo location data");
            
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.Count, Is.EqualTo(1));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault(), Is.EqualTo(meteoPrecipitationSeries));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.IsIndependent, Is.False);
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Arguments[0].Name, Is.EqualTo("Time"));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Arguments[0].Values.Count, Is.EqualTo(3));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Arguments[0].Values[0], Is.EqualTo(meteoPrecipitationSeries.Data.Arguments[0].Values[0]));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Arguments[0].Values[1], Is.EqualTo(meteoPrecipitationSeries.Data.Arguments[0].Values[1]));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Arguments[0].Values[2], Is.EqualTo(meteoPrecipitationSeries.Data.Arguments[0].Values[2]));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].Name, Is.EqualTo(FmMeteoComponent.Precipitation.ToString()));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].InterpolationType, Is.EqualTo(InterpolationType.Linear));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].ExtrapolationType, Is.EqualTo(ExtrapolationType.None));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].Unit.Name.Contains("millimeters per day"));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].Unit.Symbol.Contains("mm day-1"));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].Values.Count, Is.EqualTo(3));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].Values[0], Is.EqualTo(clone.Data.Components[0].Values[0]));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].Values[1], Is.EqualTo(clone.Data.Components[0].Values[1]));
                Assert.That(otherModel.ModelDefinition.FmMeteoFields.FirstOrDefault().Data.Components[0].Values[2], Is.EqualTo(clone.Data.Components[0].Values[2]));
                
            });
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
        public void GivenFMModelWithNetworkWithBranchWithBoundaryCondition1DAtNode_WhenAddingMockedBranchConnectionWithTargetNodeToNode2AndOneMockedBranchWithSourceFromNode2_ThenBoundaryCondition1DAtNode2WillBeRemoved()
        {

            // Setup
            var channelToNode2 = mocks.StrictMultiMock<IBranch>(typeof(IHydroObject), typeof(INotifyPropertyChanged), typeof(DelftTools.Utils.INotifyPropertyChange));
            var sourceNode = mocks.StrictMultiMock<INode>(typeof(DelftTools.Utils.INotifyPropertyChange), typeof(IHydroObject));
            var channelFromNode2 = mocks.StrictMultiMock<IBranch>(typeof(IHydroObject), typeof(INotifyPropertyChanged), typeof(DelftTools.Utils.INotifyPropertyChange));
            var targetNode = mocks.StrictMultiMock<INode>(typeof(DelftTools.Utils.INotifyPropertyChange), typeof(IHydroObject));
            var branchFeature = mocks.StrictMock<IBranchFeature>();

            var eventedListOfSourceNodeIncomingBranches = mocks.StrictMultiMock<IEventedList<IBranch>>(typeof(INotifyCollectionChanged));
            //eventedListOfSourceNodeIncomingBranches.Expect(elb => elb.GetEnumerator()).Return(Enumerable.Empty<IBranch>().GetEnumerator());
            ((INotifyCollectionChanged)eventedListOfSourceNodeIncomingBranches).Expect(l => l.CollectionChanged += Arg<NotifyCollectionChangedEventHandler>.Is.Anything).Repeat.Once();

            var eventedListOfSourceNodeOutgoingBranches = mocks.StrictMultiMock<IEventedList<IBranch>>(typeof(INotifyCollectionChanged));
            //eventedListOfSourceNodeOutgoingBranches.Expect(elb => elb.GetEnumerator()).Return(Enumerable.Repeat(channelToNode2, 1).GetEnumerator());
            ((INotifyCollectionChanged)eventedListOfSourceNodeOutgoingBranches).Expect(l => l.CollectionChanged += Arg<NotifyCollectionChangedEventHandler>.Is.Anything).Repeat.Once();

            var eventedListOfTargetNodeIncomingBranches = mocks.StrictMultiMock<IEventedList<IBranch>>(typeof(INotifyCollectionChanged));
            //eventedListOfTargetNodeIncomingBranches.Expect(elb => elb.GetEnumerator()).Return(Enumerable.Repeat(channelFromNode2, 1).GetEnumerator());
            ((INotifyCollectionChanged)eventedListOfTargetNodeIncomingBranches).Expect(l => l.CollectionChanged += Arg<NotifyCollectionChangedEventHandler>.Is.Anything).Repeat.Once();

            var eventedListOfTargetNodeOutgoingBranches = mocks.StrictMultiMock<IEventedList<IBranch>>(typeof(INotifyCollectionChanged));
            //eventedListOfTargetNodeOutgoingBranches.Expect(elb => elb.GetEnumerator()).Return(Enumerable.Empty<IBranch>().GetEnumerator());
            ((INotifyCollectionChanged)eventedListOfTargetNodeOutgoingBranches).Expect(l => l.CollectionChanged += Arg<NotifyCollectionChangedEventHandler>.Is.Anything).Repeat.Once();

            var eventedListOfEmptyLinks = mocks.StrictMultiMock<IEventedList<HydroLink>>(typeof(INotifyCollectionChanged));
            //eventedListOfEmptyLinks.Expect(elb => elb.GetEnumerator()).Return(Enumerable.Empty<HydroLink>().GetEnumerator());
            ((INotifyCollectionChanged)eventedListOfEmptyLinks).Expect(l => l.CollectionChanged += Arg<NotifyCollectionChangedEventHandler>.Is.Anything).Repeat.Twice();

            var model = new WaterFlowFMModel();
            HydroNetworkHelper.AddSnakeHydroNetwork(model.Network, new[] { new Point(0, 0), new Point(100, 0) });
            channelToNode2.Expect(b => b.Network).Return(model.Network).Repeat.Once();
            //channelToNode2.Expect(b => b.BranchFeatures).Return(Enumerable.Repeat(branchFeature,1) as IEventedList<IBranchFeature>).Repeat.Once();
            channelToNode2.Expect(b => b.Length).Return(100).Repeat.Times(4);
            channelToNode2.Expect(b => b.Source).PropertyBehavior().Repeat.Once();
            channelToNode2.Expect(b => b.Target).Return(model.Network.Nodes[1]).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)channelToNode2).Expect(npc => npc.PropertyChanging += Arg<PropertyChangingEventHandler>.Is.Anything).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)channelToNode2).Expect(npc => npc.PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Once();
            channelToNode2.Expect(b => b.Target = Arg<INode>.Is.Anything)
                .WhenCalled(call =>
                    channelToNode2.Raise(b => ((INotifyPropertyChanged)b).PropertyChanged += null, channelToNode2, new PropertyChangedEventArgs(nameof(IBranch.Target)))).Repeat.Once();

            channelFromNode2.Expect(b => b.Network).Return(model.Network).Repeat.Once();
            //channelFromNode2.Expect(b => b.BranchFeatures).Return(Enumerable.Repeat(branchFeature, 1) as IEventedList<IBranchFeature>).Repeat.Once();
            channelFromNode2.Expect(b => b.Length).Return(100).Repeat.Times(4);
            channelFromNode2.Expect(b => b.Source = Arg<INode>.Is.Anything)
                .WhenCalled(call =>
                {
                    model.Network.Nodes[1].OutgoingBranches.Add(channelFromNode2);
                    channelFromNode2.Raise(b => ((INotifyPropertyChanged) b).PropertyChanged += null,
                            channelFromNode2, new PropertyChangedEventArgs(nameof(IBranch.Source)));
                }).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)channelFromNode2).Expect(npc => npc.PropertyChanging += Arg<PropertyChangingEventHandler>.Is.Anything).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)channelFromNode2).Expect(npc => npc.PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Once();


            channelFromNode2.Expect(b => b.Source).Return(model.Network.Nodes[1]).Repeat.Once();
            channelFromNode2.Expect(b => b.Target).PropertyBehavior().Repeat.Once();
            
            sourceNode.Expect(n => n.Network).Return(model.Network).Repeat.Once();
            sourceNode.Expect(n => n.Name).Return("Node3").Repeat.Once();
            sourceNode.Expect(n => n.IsConnectedToMultipleBranches).Return(false).Repeat.Once();
            sourceNode.Expect(n => n.IncomingBranches).Return(eventedListOfSourceNodeIncomingBranches).Repeat.Once();
            sourceNode.Expect(n => n.OutgoingBranches).Return(eventedListOfSourceNodeOutgoingBranches).Repeat.Once();
            ((IHydroObject)sourceNode).Expect(n => n.Links).Return(eventedListOfEmptyLinks).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)sourceNode).Expect(npc => npc.PropertyChanging += Arg<PropertyChangingEventHandler>.Is.Anything).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)sourceNode).Expect(npc => npc.PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Twice();
            
            targetNode.Expect(n => n.Network).Return(model.Network).Repeat.Once();
            targetNode.Expect(n => n.Name).Return("Node4").Repeat.Once();
            targetNode.Expect(n => n.IsConnectedToMultipleBranches).Return(false).Repeat.Once();
            targetNode.Expect(n => n.IncomingBranches).Return(eventedListOfTargetNodeIncomingBranches).Repeat.Once();
            targetNode.Expect(n => n.OutgoingBranches).Return(eventedListOfTargetNodeOutgoingBranches).Repeat.Once();
            ((IHydroObject)targetNode).Expect(n => n.Links).Return(eventedListOfEmptyLinks).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)targetNode).Expect(npc => npc.PropertyChanging += Arg<PropertyChangingEventHandler>.Is.Anything).Repeat.Once();
            ((DelftTools.Utils.INotifyPropertyChange)targetNode).Expect(npc => npc.PropertyChanged += Arg<PropertyChangedEventHandler>.Is.Anything).Repeat.Twice();

            mocks.ReplayAll();
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
    }

}
