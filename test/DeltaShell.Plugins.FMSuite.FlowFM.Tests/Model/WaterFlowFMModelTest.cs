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
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.KnownStructureProperties;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.Common.IO.RestartFiles;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Coverages;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FunctionStores;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using DeltaShell.Plugins.SharpMapGis.ImportExport;
using DeltaShell.Plugins.SharpMapGis.SpatialOperations;
using GeoAPI.CoordinateSystems.Transformations;
using GeoAPI.Extensions.CoordinateSystems;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;
using SharpMap;
using SharpMap.Extensions.CoordinateSystems;
using SharpMap.SpatialOperations;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Model
{
    [TestFixture]
    public partial class WaterFlowFMModelTest
    {
        [Test]
        public void CheckDefaultPropertiesOfFMModel()
        {
            var model = new WaterFlowFMModel();

            Assert.AreEqual(0, model.SnapVersion);
            Assert.IsTrue(model.ValidateBeforeRun);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestImportSimpleModelWith_SourceAndSink_Tracer_Morphology_CorrectlyUpdatesSourceAndSinkComponents()
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"SimpleModel_SourceAndSink_Tracer_Morphology\SimpleModel.mdu"));

            SourceAndSink sourceAndSink = model.SourcesAndSinks.FirstOrDefault();

            Assert.NotNull(sourceAndSink);
            foreach (ISedimentFraction sedimentFraction in model.SedimentFractions)
            {
                Assert.True(sourceAndSink.Function.Components.Any(c => c.Name == sedimentFraction.Name));
            }

            IEnumerable<string> tracerBoundaryConditionsTracerNames = model.BoundaryConditions
                                                                           .OfType<FlowBoundaryCondition>()
                                                                           .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                                                                           .Select(tbc => tbc.TracerName)
                                                                           .Distinct();

            foreach (string tracerName in model.TracerDefinitions.Where(t => tracerBoundaryConditionsTracerNames.Contains(t)))
            {
                Assert.True(sourceAndSink.Function.Components.Any(c => c.Name == tracerName));
            }

            foreach (string tracerName in model.TracerDefinitions.Where(t => !tracerBoundaryConditionsTracerNames.Contains(t)))
            {
                Assert.False(sourceAndSink.Function.Components.Any(c => c.Name == tracerName));
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestAddingSourceAndSinkCorrectlyUpdatesSedimentFractionAndTracerNamesForSourceAndSink()
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"SimpleModel_SourceAndSink_Tracer_Morphology\SimpleModel.mdu"));

            var sourceAndSink = new SourceAndSink();

            Assert.AreEqual(0, sourceAndSink.SedimentFractionNames.Count);
            Assert.AreEqual(0, sourceAndSink.TracerNames.Count);

            model.SourcesAndSinks.Add(sourceAndSink);

            foreach (ISedimentFraction sedimentFraction in model.SedimentFractions)
            {
                Assert.True(sourceAndSink.SedimentFractionNames.Contains(sedimentFraction.Name));
            }

            IEnumerable<string> tracerBoundaryConditionsTracerNames = model.BoundaryConditions
                                                                           .OfType<FlowBoundaryCondition>()
                                                                           .Where(fbc => fbc.FlowQuantity == FlowBoundaryQuantityType.Tracer)
                                                                           .Select(tbc => tbc.TracerName)
                                                                           .Distinct();

            foreach (string tracerName in model.TracerDefinitions.Where(t => tracerBoundaryConditionsTracerNames.Contains(t)))
            {
                Assert.True(sourceAndSink.TracerNames.Contains(tracerName));
            }

            foreach (string tracerName in model.TracerDefinitions.Where(t => !tracerBoundaryConditionsTracerNames.Contains(t)))
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

            Assert.AreEqual(0, sourceAndSink.SedimentFractionNames.Count);
            Assert.AreEqual(0, sourceAndSink.TracerNames.Count);

            model.SourcesAndSinks.Add(sourceAndSink);

            var tracer01 = "Tracer01";
            var tracer02 = "Tracer02";
            model.TracerDefinitions.AddRange(new List<string>
            {
                tracer01,
                tracer02
            });

            var boundary01 = new Feature2D() {Name = "Boundary01"};
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

            var boundary02 = new Feature2D() {Name = "Boundary02"};
            var set02 = new BoundaryConditionSet();
            model.BoundaryConditionSets.Add(set02);
            set02.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary02,
                TracerName = tracer01
            });

            Assert.AreEqual(2, sourceAndSink.TracerNames.Count);
            Assert.AreEqual(tracer01, sourceAndSink.TracerNames[0]);
            Assert.AreEqual(tracer02, sourceAndSink.TracerNames[1]);

            set01.BoundaryConditions.Clear();

            Assert.AreEqual(1, sourceAndSink.TracerNames.Count);
            Assert.AreEqual(tracer01, sourceAndSink.TracerNames[0]);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestRemovingBoundaryConditionSet_OnlyRemovesTracerNameFromSourceAndSink_IfNoOtherTracerBoundaryConditionsExistsForSameTracer()
        {
            var model = new WaterFlowFMModel();
            var sourceAndSink = new SourceAndSink();

            Assert.AreEqual(0, sourceAndSink.SedimentFractionNames.Count);
            Assert.AreEqual(0, sourceAndSink.TracerNames.Count);

            model.SourcesAndSinks.Add(sourceAndSink);

            var tracer01 = "Tracer01";
            var tracer02 = "Tracer02";
            model.TracerDefinitions.AddRange(new List<string>
            {
                tracer01,
                tracer02
            });

            var boundary01 = new Feature2D() {Name = "Boundary01"};
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

            var boundary02 = new Feature2D() {Name = "Boundary02"};
            var set02 = new BoundaryConditionSet();
            model.BoundaryConditionSets.Add(set02);
            set02.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.Tracer, BoundaryConditionDataType.Empty)
            {
                Feature = boundary02,
                TracerName = tracer01
            });

            Assert.AreEqual(2, sourceAndSink.TracerNames.Count);
            Assert.AreEqual(tracer01, sourceAndSink.TracerNames[0]);
            Assert.AreEqual(tracer02, sourceAndSink.TracerNames[1]);

            model.BoundaryConditionSets.Remove(set01);

            Assert.AreEqual(1, sourceAndSink.TracerNames.Count);
            Assert.AreEqual(tracer01, sourceAndSink.TracerNames[0]);
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

            Assert.AreEqual(1, count);
        }

        [Test]
        public void CheckWeirFormulaPropertyChangeEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();

            var weir = new Weir2D()
            {
                Name = "weir01",
                WeirFormula = new SimpleWeirFormula()
            };

            var collectionChangedCount = 0;
            ((INotifyCollectionChanged) model).CollectionChanged += (s, e) =>
            {
                if (e.GetRemovedOrAddedItem() != weir)
                {
                    return;
                }

                collectionChangedCount++;
            };

            var weirFormulaChangeCount = 0;
            ((INotifyPropertyChanged) model).PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(Weir.WeirFormula))
                {
                    return;
                }

                weirFormulaChangeCount++;
            };
            // add weir to model
            model.Area.Weirs.Add(weir);
            Assert.AreEqual(1, collectionChangedCount);

            // change weirformula
            weir.WeirFormula = new GeneralStructureWeirFormula();
            Assert.AreEqual(1, weirFormulaChangeCount);
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

            List<IDataItem> dataItems = model.GetChildDataItems(weir).ToList();

            Assert.AreEqual(1, dataItems.Count);

            Assert.AreEqual(weir.Name, dataItems[0].Name);
            Assert.AreEqual(KnownStructureProperties.CrestLevel, dataItems[0].Tag);
            Assert.AreEqual(DataItemRole.Input, dataItems[0].Role);
            Assert.AreEqual(weir, ((WaterFlowFMFeatureValueConverter) dataItems[0].ValueConverter).Location);
            Assert.AreEqual(model, ((WaterFlowFMFeatureValueConverter) dataItems[0].ValueConverter).Model);
            Assert.AreEqual(KnownStructureProperties.CrestLevel, ((WaterFlowFMFeatureValueConverter) dataItems[0].ValueConverter).ParameterName);

            // change weir formula
            weir.WeirFormula = new GeneralStructureWeirFormula();
            dataItems = model.GetChildDataItems(weir).ToList();
            Assert.AreEqual(3, dataItems.Count);

            var generalStructureDataItems = new List<string>
            {
                KnownGeneralStructureProperties.CrestLevel.GetDescription(),
                KnownGeneralStructureProperties.GateLowerEdgeLevel.GetDescription(),
                KnownGeneralStructureProperties.GateOpeningWidth.GetDescription()
            };

            for (var i = 0; i < dataItems.Count; ++i)
            {
                Assert.AreEqual(weir.Name, dataItems[i].Name);
                Assert.AreEqual(generalStructureDataItems[i], dataItems[i].Tag);
                Assert.AreEqual(DataItemRole.Input, dataItems[i].Role);
                Assert.AreEqual(weir, ((WaterFlowFMFeatureValueConverter) dataItems[i].ValueConverter).Location);
                Assert.AreEqual(model, ((WaterFlowFMFeatureValueConverter) dataItems[i].ValueConverter).Model);
                Assert.AreEqual(generalStructureDataItems[i], ((WaterFlowFMFeatureValueConverter) dataItems[i].ValueConverter).ParameterName);
            }
        }

        [Test]
        public void CheckSedimentFormulaPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction
            {
                Name = "testFrac",
                CurrentSedimentType = SedimentFractionHelper.GetSedimentationTypes()[1],
                CurrentFormulaType = SedimentFractionHelper.GetSedimentationFormulas()[0]
            };
            model.SedimentFractions.Add(sedFrac);

            var modelCount = 0;
            ((INotifyPropertyChanged) model).PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != "IsSpatiallyVarying")
                {
                    return;
                }

                modelCount++;
            };

            var sedFracCount = 0;
            ((INotifyPropertyChanged) sedFrac).PropertyChanged += (s, e) => sedFracCount++;

            ISpatiallyVaryingSedimentProperty prop = sedFrac.CurrentFormulaType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().First();
            prop.IsSpatiallyVarying = true;

            Assert.AreEqual(1, sedFracCount);
            Assert.AreEqual(1, modelCount); // IsSpatiallyVarying
        }

        [Test]
        public void CheckSedimentPropertyEventPropagatesToModel()
        {
            var model = new WaterFlowFMModel();
            model.ModelDefinition.UseMorphologySediment = true;
            var sedFrac = new SedimentFraction {Name = "testFrac"};
            model.SedimentFractions.Add(sedFrac);

            var modelCount = 0;
            ((INotifyPropertyChanged) model).PropertyChanged += (s, e) => modelCount++;
            var sedFracCount = 0;
            ((INotifyPropertyChanged) sedFrac).PropertyChanged += (s, e) => sedFracCount++;

            ISpatiallyVaryingSedimentProperty prop = sedFrac.CurrentSedimentType.Properties.OfType<ISpatiallyVaryingSedimentProperty>().First();
            prop.IsSpatiallyVarying = true;

            Assert.AreEqual(1, sedFracCount);

            // TODO: Set the assertion value to 3 when initial condition is supported in ext-files (DELFT3DFM-996)
            //Assert.AreEqual(3, modelCount); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
            Assert.AreEqual(1, modelCount); /* IsSpatiallyVarying + 2 changes id AddOrRenameDataItem */
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.VerySlow)]
        public void RunModelCheckIfStatisticsAreWrittenToDiaFile()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            var workingDir = string.Empty;
            var workingOutputDir = string.Empty;

            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(mduPath);

                ActivityRunner.RunActivity(model);
                Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
                workingDir = Path.Combine(model.WorkingDirectoryPath, model.DirectoryName);
                workingOutputDir = Path.Combine(workingDir, "output");
            }

            var statisticsWritten = false;
            Parallel.ForEach(File.ReadAllLines(Path.Combine(workingOutputDir, "bendprof.dia")),
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
        public void WhenInstantiatingWaterFlowFMModelWithDefaultConstructor_ThenDefaultStateIsExpected()
        {
            var fmModel = new WaterFlowFMModel();
            Assert.That(fmModel.Name, Is.EqualTo("FlowFM"));
            Assert.IsNull(fmModel.MduFilePath);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckFileBasedStatesOfFMModel()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduPath);

            Assert.That(fmModel.Name, Is.EqualTo("bendprof"));
            Assert.That(Path.GetFileName(fmModel.MduFilePath), Is.EqualTo("bendprof.mdu"));
        }

        [Test]
        public void CreateNewModelCheckStuffIsEmptyButNotNull()
        {
            var model = new WaterFlowFMModel(); // empty model
            Assert.IsTrue(model.Grid.IsEmpty);
            Assert.IsNotNull(model.Bathymetry);
            Assert.AreEqual(0, model.Bathymetry.ToPointCloud().PointValues.Count);
        }

        [Test]
        public void AddInitialSalinityTest()
        {
            // this test checks for SpatialDataLayersChanged() in WaterFlowFMModel.
            var model = new WaterFlowFMModel();

            Assert.AreEqual(1, model.InitialSalinity.Coverages.Count);
            IDataItem originalDataItem = model.GetDataItemByValue(model.InitialSalinity.Coverages[0]);
            string originalName = originalDataItem.Name;

            model.InitialSalinity.VerticalProfile = new VerticalProfileDefinition(VerticalProfileType.TopBottom);

            Assert.AreEqual(2, model.InitialSalinity.Coverages.Count);
            Assert.IsNotNull(model.GetDataItemByValue(model.InitialSalinity.Coverages[1]));
            // check if a data item was created

            Assert.AreNotEqual(originalName, model.GetDataItemByValue(model.InitialSalinity.Coverages[0]).Name);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TransformCoordinateSystemTest()
        {
            string mduPath = TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu");
            string localMduFilePath = TestHelper.CreateLocalCopy(mduPath);

            Map.CoordinateSystemFactory = new OgrCoordinateSystemFactory();
            ICoordinateSystemFactory factory = Map.CoordinateSystemFactory;

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(localMduFilePath));

            model.CoordinateSystem = factory.CreateFromEPSG(28992);

            ICoordinateSystem newCoordinateSystem = factory.CreateFromEPSG(4326);
            ICoordinateTransformation transformation = factory.CreateTransformation(model.CoordinateSystem, newCoordinateSystem);
            model.TransformCoordinates(transformation);

            Assert.AreEqual(model.CoordinateSystem, newCoordinateSystem);
            Assert.AreEqual(model.Roughness.CoordinateSystem, newCoordinateSystem);

            IDataItem roughnessDataItem = model.GetDataItemByValue(model.Roughness);
            var valueConverter = (SpatialOperationSetValueConverter) roughnessDataItem.ValueConverter;

            Assert.AreEqual(model.CoordinateSystem, valueConverter.SpatialOperationSet.CoordinateSystem);
            Assert.AreEqual(model.CoordinateSystem,
                            valueConverter.SpatialOperationSet.Operations.Last().CoordinateSystem);
        }

        [Test]
        public void GivenAWaterFlowFMModel_WhenHydFilePathIsCalled_ThenHydFileNameShouldBeBasedOnMduFileName()
        {
            // Given
            const string mduFileName = "mdu_file_name";
            var model = new WaterFlowFMModel {DelwaqOutputDirectoryPath = "dir"};
            TypeUtils.SetPrivatePropertyValue(model, nameof(model.MduFilePath), $"{mduFileName}.mdu");

            // When
            string hydFileName = Path.GetFileNameWithoutExtension(model.HydFilePath);

            // Then
            Assert.AreEqual(hydFileName, mduFileName,
                            "Name of the hyd file should be the same as the mdu file name.");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void RunModelTwice()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            ActivityRunner.RunActivity(model);
            var waterLevelFirstRun = (double) model.OutputWaterLevel[model.StopTime, 0];
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            Assert.AreEqual(4.0d, waterLevelFirstRun, 0.1);

            ActivityRunner.RunActivity(model);
            var waterLevelSecondRun = (double) model.OutputWaterLevel[model.CurrentTime, 0];
            Assert.AreEqual(ActivityStatus.Cleaned, model.Status);
            Assert.AreEqual(waterLevelSecondRun, waterLevelFirstRun, 0.005); // value changes per run (see above)
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void TestDiaFileIsRetrievedAfterModelRun()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            ActivityRunner.RunActivity(model);

            IDataItem diaFileDataItem = model.DataItems.FirstOrDefault(di => di.Tag == WaterFlowFMModel.DiaFileDataItemTag);
            Assert.NotNull(diaFileDataItem, "DiaFile not retrieved after model run, check WaterFlowFMModel.DiaFileDataItemTag");
            Assert.NotNull(diaFileDataItem.Value, "DiaFile not retrieved after model run, check WaterFlowFMModel.DiaFileDataItemTag");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void TestWarningGivenIfDiaFileFileNotFound()
        {
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            string outputDirectory = FileUtils.CreateTempDirectory();
            string diaFileName = string.Format("{0}.dia", model.Name);
            string diaFilePath = Path.Combine(outputDirectory, diaFileName);

            TestHelper.AssertAtLeastOneLogMessagesContains(() =>
                                                               TypeUtils.CallPrivateMethod(model, "ReadDiaFile", new[]
                                                               {
                                                                   outputDirectory
                                                               }),
                                                           string.Format(Resources.WaterFlowFMModel_ReadDiaFile_Could_not_find_log_file___0__at_expected_path___1_, diaFileName, diaFilePath)
            );
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void SetCoordinateSystemOnModelAndExportAdjustsNetFile()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            string tempDir = Path.GetTempFileName();
            File.Delete(tempDir);
            Directory.CreateDirectory(tempDir);

            model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(4326); //wgs84
            model.ExportTo(Path.Combine(tempDir, "cs\\cs.mdu"));

            Assert.AreEqual(4326, NetFile.ReadCoordinateSystem(model.NetFilePath).AuthorityCode);

            model.CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(28992); //other number
            model.ExportTo(Path.Combine(tempDir, "cs2\\cs2.mdu"));

            Assert.AreEqual(28992, NetFile.ReadCoordinateSystem(model.NetFilePath).AuthorityCode);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckIfBcmFileIsReferencedInMorFileAfterRunningAnImportedMduFile()
        {
            //arrange
            string mduPath = TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);
            string tempDir = FileUtils.CreateTempDirectory();

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

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
            model.TracerDefinitions.AddRange(new List<string>
            {
                tracer01,
                tracer02
            });

            var feature = new Feature2D
            {
                Name = "Boundary1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0)
                    })
            };

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.Discharge,
                                                                  BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature
            };

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });
            flowBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            flowBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var set01 = new BoundaryConditionSet {Feature = feature};
            model.BoundaryConditionSets.Add(set01);

            var boundary = new Feature2D()
            {
                Name = "TracerBoundary1",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 0),
                        new Coordinate(1, 0)
                    })
            };
            set01.BoundaryConditions.Add(new FlowBoundaryCondition(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, BoundaryConditionDataType.AstroComponents)
            {
                Feature = boundary,
                TracerName = tracer01
            });
            string exportPath = Path.Combine(tempDir, "export");
            string mduExportPath = Path.Combine(exportPath, "cs.mdu");
            model.ExportTo(mduExportPath);

            var modelAfterImport = new WaterFlowFMModel();
            modelAfterImport.ImportFromMdu(mduExportPath);

            ActivityRunner.RunActivity(modelAfterImport);
            string mduFilePathAfterExport = modelAfterImport.MduFilePath;

            File.Exists(Path.Combine(mduFilePathAfterExport, "cs.mdu"));
            File.Exists(Path.Combine(mduFilePathAfterExport, "cs.mor"));
            File.Exists(Path.Combine(mduFilePathAfterExport, "cs.sed"));

            string morFilePath = Path.Combine(exportPath, "cs.mor");
            File.Exists(morFilePath);

            //act
            IEnumerable<string> lines = File.ReadLines(morFilePath);
            int countedLines = lines.Count(l => l.Replace(" ", "").Contains("BcFil=bendprof.bcm"));

            //assert
            Assert.AreEqual(countedLines, 1);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckStartTime()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            Assert.AreEqual(new DateTime(1992, 08, 31), model.StartTime);

            var newTime = new DateTime(2000, 1, 2, 11, 15, 5, 2); //time with milliseconds
            model.StartTime = newTime;
            Assert.AreEqual(newTime, model.StartTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckCoordinateSystemBendProf()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            Assert.AreEqual(null, model.CoordinateSystem);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void CheckCoordinateSystemIvoorkust()
        {
            string mduPath = TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            Assert.AreEqual("WGS 84", model.CoordinateSystem.Name);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportIvoorkustModel()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"mdu_ivoorkust\ivk.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            model.Initialize();

            Assert.AreEqual(ActivityStatus.Initialized, model.Status);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ImportHarlingen3DModel()
        {
            string mduPath = TestHelper.GetTestFilePath(@"harlingen_model_3d\har.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            Assert.AreEqual(10, model.DepthLayerDefinition.NumLayers, "depth layers");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ExportTwiceCheckNetFileIsCopiedCorrectly()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            string tempPath1 = Path.GetTempFileName();
            File.Delete(tempPath1);
            Directory.CreateDirectory(tempPath1);

            model.ExportTo(Path.Combine(tempPath1, "test.mdu"), false);

            // delete the first export location
            FileUtils.DeleteIfExists(tempPath1);

            string tempPath2 = Path.GetTempFileName();
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
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            string gridFile = model.NetFilePath;

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
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"harlingen\har.mdu"));

            IBoundaryCondition boundaryCondition =
                model.BoundaryConditions.First(
                    bc => bc is FlowBoundaryCondition && ((Feature2D) bc.Feature).Name == "071_02");

            var refDate = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value;

            IFunction function = boundaryCondition.GetDataAtPoint(0);

            IVariable<DateTime> times = function.Arguments.OfType<IVariable<DateTime>>().First();

            DateTime bcStartTime = times.MinValue;

            Assert.AreEqual(refDate, bcStartTime);

            const double minutes = 4.7520000e+04;

            var bcTimeRange = new TimeSpan(0, 0, (int) minutes, 0);

            DateTime bcStopTime = times.MaxValue;

            Assert.AreEqual(refDate + bcTimeRange, bcStopTime);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadGridShouldNotThrowAlotOfEvents()
        {
            string mduPath =
                TestHelper.GetTestFilePath(@"data\f04_bottomfriction\c016_2DConveyance_bend\input\bendprof.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            var count = 0;
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
            string mduPath =
                TestHelper.GetTestFilePath(@"venice_pilot_22ott2013\n_e04e.mdu");
            mduPath = TestHelper.CreateLocalCopy(mduPath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

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
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));

            IValueConverter valueConverter = model.GetDataItemByValue(model.Roughness).ValueConverter;
            var spatialOperationValueConverter = valueConverter as SpatialOperationSetValueConverter;

            Assert.IsNotNull(spatialOperationValueConverter);

            Assert.AreEqual(2, spatialOperationValueConverter.SpatialOperationSet.Operations.Count);
            Assert.IsTrue(spatialOperationValueConverter.SpatialOperationSet.Operations[1] is InterpolateOperation);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadBathymetryTest()
        {
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));

            UnstructuredGrid originalGrid = model.Grid;
            IDataItem bathymetryDataItem = model.GetDataItemByValue(model.Bathymetry);
            SpatialOperationSetValueConverter spatialOperationValueConverter =
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
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(TestHelper.GetTestFilePath(@"chezy_samples\chezy.mdu"));

            new FlowFMNetFileImporter().ImportItem(TestHelper.GetTestFilePath(@"harlingen\fm_003_net.nc"), model);
            Assert.AreEqual(12845, model.Grid.Vertices.Count);
            Assert.AreEqual(16597, model.Grid.Cells.Count);
            Assert.AreEqual(29441, model.Grid.Edges.Count);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void ReloadGridShouldSetNoDataValueForBathymetry()
        {
            var model = new WaterFlowFMModel();
            Assert.That(model.Grid.Cells.Count, Is.EqualTo(0));

            string testFile = TestHelper.GetTestFilePath(@"ugrid\Custom_Ugrid.nc");
            Assert.IsTrue(File.Exists(testFile));

            string localCopyOfTestFile = TestHelper.CreateLocalCopy(testFile);

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

        [Test]
        public void FmModelGetVarGridPropertyNameShouldReturnGrid()
        {
            var model = new WaterFlowFMModel();
            var grids = model.GetVar(WaterFlowFMModel.GridPropertyName) as UnstructuredGrid[];
            Assert.IsNotNull(grids);
            Assert.IsNotNull(grids[0]);
            Assert.IsTrue(grids[0].IsEmpty);
        }

        [Test]
        public void FmModelGetVarCellsToFeaturesNameShouldReturnEmptyTimeseries()
        {
            var model = Substitute.ForPartsOf<WaterFlowFMModel>();
            model.OutputMapFileStore.Returns(new FMMapFileFunctionStore());
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
            model.SetVar(new[]
            {
                true
            }, WaterFlowFMModel.DisableFlowNodeRenumberingPropertyName, null, null);
            Assert.IsTrue(model.DisableFlowNodeRenumbering);
        }

        [Test]
        public void WriteSnappedFeaturesTest()
        {
            var model = new WaterFlowFMModel();

            /* Default is false */
            Assert.IsFalse(model.WriteSnappedFeatures);
            Assert.AreEqual(model.WriteSnappedFeatures, model.ModelDefinition.WriteSnappedFeatures);

            /* Value is the same in the model definition */
            model.ModelDefinition.WriteSnappedFeatures = true;
            Assert.IsTrue(model.WriteSnappedFeatures);
            Assert.AreEqual(model.WriteSnappedFeatures, model.ModelDefinition.WriteSnappedFeatures);
        }

        [Test]
        public void GivenFmModel_WhenAddingAnAreaFeatureWithGroupNameEqualToPathThatIsPointingToASubFolderOfMduFolder_ThenGroupNameIsAlwaysRelative()
        {
            // Make local copy of project
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection/MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, "FlowFM.mdu");

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduFilePath);

            // Import dry points

            fmModel.Area.DryPoints.Add(new GroupablePointFeature {GroupName = Path.Combine(localPath, @"SubFolder/MyDryPoints_dry.xyz")});
            fmModel.Area.LandBoundaries.Add(new LandBoundary2D {GroupName = Path.Combine(localPath, @"SubFolder/MyLandBoundaries.ldb")});

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.DryPoints.FirstOrDefault().GroupName, Is.EqualTo(@"SubFolder/MyDryPoints_dry.xyz"));
            Assert.That(fmModel.Area.LandBoundaries.FirstOrDefault().GroupName, Is.EqualTo(@"SubFolder/MyLandBoundaries.ldb"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAStructureWithAreaFeatureGroupNameToPathThatIsPointingToASubFolderOfMduFolder_ThenGroupNameIsPointingToItsReferencingStructureFile()
        {
            // Make local copy of project
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection/MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduFilePath);

            // Import dry points
            fmModel.Area.Pumps.Add(new Pump2D {GroupName = Path.Combine(localPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")});
            fmModel.Area.Weirs.Add(new Weir2D {GroupName = Path.Combine(localPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/gate01.pli")});

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.Pumps.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
            Assert.That(fmModel.Area.Weirs.FirstOrDefault().GroupName, Is.EqualTo(@"FeatureFiles/FlowFM_structures.ini"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAStructureWithAreaFeatureGroupNameEqualToPathThatIsNotReferencedByAStructureFile_ThenGroupNameIsEqualToDefaultStructuresFileNameInTheSameFolder()
        {
            // Make local copy of project
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection/MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduFilePath);

            // Import dry points
            fmModel.Area.Weirs.Add(new Weir2D {GroupName = Path.Combine(localPath, @"MduFileWithoutFeatureFileReferences/FeatureFiles/nonReferencedGates.pli")});

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.Weirs.FirstOrDefault().GroupName, Is.EqualTo("FeatureFiles/" + fmModel.Name + "_structures.ini"));
        }

        [Test]
        public void GivenFmModel_WhenAddingAnAreaFeatureWithGroupNameToPathThatIsPointingToNotASubFolderOfMduFolder_ThenGroupNameIsEqualToFileName()
        {
            // Make local copy of project
            string localPath = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"HydroAreaCollection/MduFileProjects"));
            string mduFilePath = Path.Combine(localPath, "MduFileWithoutFeatureFileReferences/FlowFM.mdu");

            // Make FM model from Mdu file
            var fmModel = new WaterFlowFMModel();
            fmModel.ImportFromMdu(mduFilePath);

            // Import dry points
            fmModel.Area.DryAreas.Add(new GroupableFeature2DPolygon() {GroupName = Path.Combine(localPath, @"MyDryAreas_dry.pol")});

            // Check that group name gives a relative path from the mdu folder
            Assert.That(fmModel.Area.DryAreas.FirstOrDefault().GroupName, Is.EqualTo(@"MyDryAreas_dry.pol"));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenValidFmModel_WhenModelHasRun_ThenProgressTextHasBeenReset()
        {
            string originalDir = TestHelper.GetTestFilePath("small");
            string testDir = FileUtils.CreateTempDirectory();
            string mduFilePath = Path.Combine(testDir, "input", "FlowFM.mdu");
            FileUtils.CopyDirectory(originalDir, testDir);

            var messageList = new List<string>
            {
                "Exporting to mdu file",
                "Initializing",
                "0,00 %",
                "00:17:59 (1,39 %)",
                "00:10:12 (2,78 %)",
                "00:07:13 (4,17 %)",
                "00:05:20 (5,56 %)",
                "00:04:14 (6,94 %)",
                "00:03:32 (8,33 %)",
                "00:03:00 (9,72 %)",
                "00:02:35 (11,11 %)",
                "00:02:16 (12,50 %)",
                "00:02:01 (13,89 %)",
                "00:01:48 (15,28 %)",
                "00:01:38 (16,67 %)",
                "00:01:29 (18,06 %)",
                "00:01:22 (19,44 %)",
                "00:01:15 (20,83 %)",
                "00:01:09 (22,22 %)",
                "00:01:04 (23,61 %)",
                "00:01:00 (25,00 %)",
                "00:00:56 (26,39 %)",
                "00:00:53 (27,78 %)",
                "00:00:50 (29,17 %)",
                "00:00:46 (30,56 %)",
                "00:00:44 (31,94 %)",
                "00:00:41 (33,33 %)",
                "00:00:39 (34,72 %)",
                "00:00:37 (36,11 %)",
                "00:00:35 (37,50 %)",
                "00:00:33 (38,89 %)",
                "00:00:31 (40,28 %)",
                "00:00:30 (41,67 %)",
                "00:00:28 (43,06 %)",
                "00:00:27 (44,44 %)",
                "00:00:25 (45,83 %)",
                "00:00:24 (47,22 %)",
                "00:00:23 (48,61 %)",
                "00:00:21 (50,00 %)",
                "00:00:20 (51,39 %)",
                "00:00:19 (52,78 %)",
                "00:00:18 (54,17 %)",
                "00:00:17 (55,56 %)",
                "00:00:16 (56,94 %)",
                "00:00:16 (58,33 %)",
                "00:00:15 (59,72 %)",
                "00:00:14 (61,11 %)",
                "00:00:13 (62,50 %)",
                "00:00:12 (63,89 %)",
                "00:00:12 (65,28 %)",
                "00:00:11 (66,67 %)",
                "00:00:10 (68,06 %)",
                "00:00:10 (69,44 %)",
                "00:00:09 (70,83 %)",
                "00:00:08 (72,22 %)",
                "00:00:08 (73,61 %)",
                "00:00:07 (75,00 %)",
                "00:00:07 (76,39 %)",
                "00:00:06 (77,78 %)",
                "00:00:06 (79,17 %)",
                "00:00:05 (80,56 %)",
                "00:00:05 (81,94 %)",
                "00:00:04 (83,33 %)",
                "00:00:04 (84,72 %)",
                "00:00:03 (86,11 %)",
                "00:00:03 (87,50 %)",
                "00:00:03 (88,89 %)",
                "00:00:02 (90,28 %)",
                "00:00:02 (91,67 %)",
                "00:00:01 (93,06 %)",
                "00:00:01 (94,44 %)",
                "00:00:01 (95,83 %)",
                "00:00:00 (97,22 %)",
                "00:00:00 (98,61 %)",
                "00:00:00 (100,00 %)",
                "Reading map file",
                "Reading his file",
                "Reading dia file",
                "00:00:00 (100,00 %)"
            };

            try
            {
                var counter = 0;

                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(mduFilePath);

                fmModel.ReferenceTime = fmModel.StartTime;
                fmModel.ProgressChanged += (sender, args) =>
                {
                    Assert.AreEqual(fmModel.ProgressText, messageList[counter],
                                    "Progress text when running FM model is different than expected.");
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
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void GivenModelForImporting_WhenThereAreFixedWeirs_ThenTheseFixedWeirsShouldBeCorrectlyImported()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);

            try
            {
                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduFilePath);

                ModelFeatureCoordinateData<FixedWeir> featureCoordinateData = model.FixedWeirsProperties.ElementAt(0);

                object modelValue = featureCoordinateData.DataColumns[0].ValueList[0];
                Assert.AreEqual(1.2, modelValue);
                modelValue = featureCoordinateData.DataColumns[0].ValueList[1];
                Assert.AreEqual(6.4, modelValue);
                modelValue = featureCoordinateData.DataColumns[1].ValueList[0];
                Assert.AreEqual(3.5, modelValue);

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
            var lineGeometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0)
            });

            var fixedWeir = new FixedWeir {Geometry = lineGeometry};

            var fmModel = new WaterFlowFMModel();

            fmModel.ModelDefinition.GetModelProperty(KnownProperties.FixedWeirScheme).SetValueAsString("8");
            fmModel.Area.FixedWeirs.Add(fixedWeir);

            IEnumerable<ModelFeatureCoordinateData<FixedWeir>> allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(1));

            ModelFeatureCoordinateData<FixedWeir> modelFeatureCoordinateData = allData.First();

            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(3));
            Assert.That(modelFeatureCoordinateData.DataColumns.First().ValueList.Count, Is.EqualTo(4));

            fixedWeir.Geometry = new LineString(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 10),
                new Coordinate(10, 0),
                new Coordinate(0, 0),
                new Coordinate(0, 100)
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

            foreach (IDataColumn dataColumn in modelFeatureCoordinateData.DataColumns)
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

            foreach (IDataColumn dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(5));

                if (dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.CrestLengthColumnName ||
                    dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludUpColumnName ||
                    dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.TaludDownColumnName ||
                    dataColumn.Name == FixedWeirFmModelFeatureCoordinateDataSyncExtensions.VegetationCoefficientColumnName)
                {
                    Assert.That(dataColumn.IsActive, Is.False);
                }
                else
                {
                    Assert.That(dataColumn.IsActive, Is.True);
                }
            }

            fixedWeir.Geometry = lineGeometry;

            allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(1));
            modelFeatureCoordinateData = allData.First();

            Assert.That(modelFeatureCoordinateData.Feature, Is.EqualTo(fixedWeir));
            Assert.That(modelFeatureCoordinateData.DataColumns.Count, Is.EqualTo(7));
            foreach (IDataColumn dataColumn in modelFeatureCoordinateData.DataColumns)
            {
                Assert.That(dataColumn.ValueList.Count, Is.EqualTo(4));
            }

            fmModel.Area.FixedWeirs.Remove(fixedWeir);

            allData = fmModel.FixedWeirsProperties;

            Assert.That(allData.Count, Is.EqualTo(0));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void GivenAnFMModel_WhenCloningThisModel_ThenTheNewFixedWeirPropertiesShouldBeLinkedToTheNewFixedWeirs()
        {
            string mduFilePath = TestHelper.GetTestFilePath(@"HydroAreaCollection\FlowFMFixedWeirs\FlowFM.mdu"); //model with two fixed weirs and every fixed weir has two coordinates.
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);
            string mduDir = Path.GetDirectoryName(mduFilePath);
            Assert.NotNull(mduDir);

            try
            {
                var fmModel = new WaterFlowFMModel();
                fmModel.ImportFromMdu(mduFilePath);

                var clonedFmModel = fmModel.DeepClone() as WaterFlowFMModel;

                Assert.NotNull(clonedFmModel);

                Assert.That(fmModel.FixedWeirsProperties.ElementAt(0).Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties.ElementAt(0).Feature));
                Assert.That(fmModel.FixedWeirsProperties.ElementAt(1).Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties.ElementAt(1).Feature));
                Assert.That(fmModel.FixedWeirsProperties.ElementAt(0).Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties.ElementAt(1).Feature));
                Assert.That(fmModel.FixedWeirsProperties.ElementAt(1).Feature, Is.Not.SameAs(clonedFmModel.FixedWeirsProperties.ElementAt(0).Feature));

                Assert.That(fmModel.FixedWeirsProperties.ElementAt(0).Feature, Is.SameAs(fmModel.Area.FixedWeirs[0]));
                Assert.That(fmModel.FixedWeirsProperties.ElementAt(1).Feature, Is.SameAs(fmModel.Area.FixedWeirs[1]));
                Assert.That(clonedFmModel.FixedWeirsProperties.ElementAt(0).Feature, Is.SameAs(clonedFmModel.Area.FixedWeirs[0]));
                Assert.That(clonedFmModel.FixedWeirsProperties.ElementAt(1).Feature, Is.SameAs(clonedFmModel.Area.FixedWeirs[1]));

                var lineGeometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(10, 10),
                    new Coordinate(10, 0),
                    new Coordinate(0, 0)
                });

                fmModel.Area.FixedWeirs[0].Geometry = lineGeometry;

                Assert.AreEqual(4, fmModel.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList.Count);
                Assert.AreEqual(2, clonedFmModel.FixedWeirsProperties.ElementAt(0).DataColumns[0].ValueList.Count);
            }
            finally
            {
                FileUtils.DeleteIfExists(mduDir);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.DataAccess)]
        public void GivenAnFmModelWithAClassMapFunctionStore_WhenGetDirectChilderenIsCalled_ExpectedChildrenObjectsAreReturned()
        {
            // Given
            string testDirectoryPath = TestHelper.GetTestFilePath("output_classmapfiles");
            string outputDirectoryPath = Path.Combine(testDirectoryPath, "output");
            string filePath = Path.Combine(outputDirectoryPath, "FlowFM_clm.nc");
            Assert.IsTrue(File.Exists(filePath));

            var model = new WaterFlowFMModel();
            model.ConnectOutput(outputDirectoryPath);
            FMClassMapFileFunctionStore outputClassMapFileStore = model.OutputClassMapFileStore;
            Assert.NotNull(outputClassMapFileStore);
            Assert.AreEqual(filePath, outputClassMapFileStore.Path);

            // When
            object[] directChildren = model.GetDirectChildren().ToArray();

            // Then
            Assert.IsTrue(outputClassMapFileStore.Functions.All(f => directChildren.Contains(f)));
        }

        [Test]
        public void GivenAnFmModel_WhenClassMapSavePathPropertyIsCalled_ExpectedStringIsReturned()
        {
            // Given
            var model = new WaterFlowFMModel();
            var modelName = "some_model_name";
            model.Name = modelName;

            // When
            string resultedPath = model.ClassMapSavePath;

            // Then
            string expectedPath = modelName + "_clm.nc";
            Assert.AreEqual(expectedPath, resultedPath);
        }

        [Test]
        public void GivenAnFmModelWithAnMduFilePathAndModelDefinitionWithEqualName_WhenClassMapSavePathPropertyIsCalled_ExpectedStringIsReturned()
        {
            // Given
            const string modelName = "some_model_name";

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(Path.Combine("directory", modelName + ".mdu"));

            model.Name = modelName;
            model.ModelDefinition.ModelName = modelName;

            // When
            string resultedPath = model.ClassMapSavePath;

            // Then
            string expectedPath = Path.Combine(model.PersistentOutputDirectoryPath, modelName + FileConstants.ClassMapFileExtension);
            Assert.AreEqual(expectedPath, resultedPath);
        }

        [Test]
        public void GivenAnFmModelWithoutAMduFilePathAndModelDefinitionWithEqualName_WhenClassMapSavePathPropertyIsCalled_ThenNullIsReturned()
        {
            // Given
            var model = new WaterFlowFMModel();
            const string modelName = "some_model_name";
            model.Name = modelName;
            model.ModelDefinition.ModelName = modelName;

            // When
            string resultedPath = model.ClassMapSavePath;

            // Then         
            Assert.AreEqual(null, resultedPath);
        }

        [Test]
        public void GivenAModelWithOutput_WhenOpeningIt_ThenCurrentOutputDirectoryIsInPersistentFolder()
        {
            //Creation of a path of non-existing model file 
            string mduPath = TestHelper.GetTestFilePath(@"notexistingmodel\input\notexistingmodel.mdu");

            //Load model 
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            object currentOutputDirectory = TypeUtils.GetField(model, "currentOutputDirectoryPath");

            string expectedPath = Path.Combine(TestHelper.GetTestDataDirectory(), @"notexistingmodel\output");
            Assert.AreEqual(expectedPath, currentOutputDirectory);
        }

        [Test]
        public void GivenAModel_WhenARunIsDone_ThenCurrentOutputDirectoryIsInWorkingDirectory()
        {
            //Creation of a path of non-existing model file 
            string mduPath = TestHelper.GetTestFilePath(@"notexistingmodel\input\notexistingmodel.mdu");

            //Load model and "run"
            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduPath);

            TypeUtils.CallPrivateMethod(model, "OnFinish");

            object currentOutputDirectory = TypeUtils.GetField(model, "currentOutputDirectoryPath");

            string expectedPath = model.WorkingOutputDirectoryPath;
            Assert.AreEqual(expectedPath, currentOutputDirectory);
        }

        [Test]
        public void GivenAModel_WhenARunIsDoneAndASave_ThenCurrentOutputDirectoryIsInPersistentFolder()
        {
            string tempFolder = FileUtils.CreateTempDirectory();

            try
            {
                //Creation of a path of non-existing model file 
                string mduPath = TestHelper.GetTestFilePath(@"notexistingmodel\input\notexistingmodel.mdu");
                var mduFile2 = "notexistingmodel2.mdu";

                //Load model and save
                var model = new WaterFlowFMModel();
                model.ImportFromMdu(mduPath);

                //Run, so that the CurrentOutputDirectory is set to WorkingDirectoryPath
                TypeUtils.CallPrivateMethod(model, "OnFinish");

                object currentOutputDirectory = TypeUtils.GetField(model, "currentOutputDirectoryPath");

                string expectedPath = Path.Combine(model.WorkingDirectoryPath, model.DirectoryName, "output");
                Assert.AreEqual(expectedPath, currentOutputDirectory);

                //Save, so that CurrentOutputDirectory is set to the persistent folder
                model.ExportTo(Path.Combine(tempFolder, "notexistingmodel", "input", mduFile2));

                currentOutputDirectory = TypeUtils.GetField(model, "currentOutputDirectoryPath");

                expectedPath = Path.Combine(tempFolder, @"notexistingmodel\output");
                Assert.AreEqual(expectedPath, currentOutputDirectory);
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        [NUnit.Framework.Category(TestCategory.Slow)]
        public void GivenASavedModelWithOutput_WhenSavingItToAnotherLocation_ThenTheNewLocationShouldBeCleanedFirstBeforeGettingTheOutput()
        {
            string tempFolder = FileUtils.CreateTempDirectory();
            FileUtils.CreateDirectoryIfNotExists(Path.Combine(tempFolder, "harlingen", "input"));
            FileUtils.CreateDirectoryIfNotExists(Path.Combine(tempFolder, "harlingen", "output"));

            try
            {
                string sourceOutput = TestHelper.GetTestFilePath(@"harlingen\output");
                string sourceMdu = Path.Combine(TestHelper.GetTestDataDirectory(), "harlingen", "har.mdu");
                string existingOutput = Path.Combine(TestHelper.GetTestDataDirectory(), "harlingen", "001_map.nc");

                string targetMdu = Path.Combine(tempFolder, "harlingen", "input", "har.mdu");
                string targetOutput = Path.Combine(tempFolder, "harlingen", "output");
                string targetSnappedOutput = Path.Combine(targetOutput, "snapped");
                FileUtils.CopyFile(sourceMdu, targetMdu);
                FileUtils.CopyFile(existingOutput, Path.Combine(tempFolder, "harlingen", "output", "001_map.nc"));

                //Create WaterFlowFMModel from target MDU, so that the outputDirectory is set correctly.
                var model = new WaterFlowFMModel();
                model.ImportFromMdu(targetMdu);

                //Put random file and directory in targetfolder, so that you can check the clean up after a save.
                Directory.CreateDirectory(Path.Combine(targetOutput, "blarg"));
                using (File.Create(Path.Combine(targetOutput, "blarg.txt"))) {}

                //Check creation of random file and directory 
                Assert.That(Directory.Exists(Path.Combine(targetOutput, "blarg")));
                Assert.That(File.Exists(Path.Combine(targetOutput, "blarg.txt")));

                TypeUtils.SetField(model, "currentOutputDirectoryPath", sourceOutput);

                TypeUtils.CallPrivateMethod(model, "SaveOutput");

                //Check that save location is cleaned before copying all the output files
                Assert.That(!File.Exists(Path.Combine(targetOutput, "blarg.txt")));
                Assert.That(!Directory.Exists(Path.Combine(targetOutput, "blarg")));

                //Check if all the output files are copied
                Assert.That(File.Exists(Path.Combine(targetOutput, "001_his.nc")));
                Assert.That(File.Exists(Path.Combine(targetOutput, "001_map.nc")));
                Assert.That(File.Exists(Path.Combine(targetOutput, "har_20080119_120000_rst.nc")));
                Assert.That(File.Exists(Path.Combine(targetOutput, "har_20080119_121000_rst.nc")));
                Assert.That(File.Exists(Path.Combine(targetOutput, "har_timings.txt")));

                Assert.That(Directory.Exists(targetSnappedOutput));

                Assert.That(File.Exists(Path.Combine(targetOutput, "snapped", "har_snapped_crs.dbf")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_crs.shp")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_crs.shx")));

                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_fxw.dbf")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_fxw.shp")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_fxw.shx")));

                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_obs.dbf")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_obs.shp")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_obs.shx")));

                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_thd.dbf")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_thd.shp")));
                Assert.That(File.Exists(Path.Combine(targetSnappedOutput, "har_snapped_thd.shx")));

                //Test the "OnClearOutput" method, so at the end the new location is completely empty
                TypeUtils.SetField(model, "currentOutputDirectoryPath", targetOutput);

                TypeUtils.CallPrivateMethod(model, "OnClearOutput");
                Assert.That(!FileUtils.IsDirectoryEmpty(targetOutput), "Directory should not be cleared after calling OnClearOutput method");
            }
            finally
            {
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        [TestCase(typeof(SimpleWeirFormula), KnownFeatureCategories.Weirs)]
        [TestCase(typeof(GatedWeirFormula), KnownFeatureCategories.Gates)]
        [TestCase(typeof(GeneralStructureWeirFormula), KnownFeatureCategories.GeneralStructures)]
        public void GivenAWeirFeature_WhenGettingFeatureCategory_ThenTheCorrectStringIsReturned(Type weirType, string expectedString)
        {
            // Given
            var feature = new Weir("myStructure") {WeirFormula = (IWeirFormula) Activator.CreateInstance(weirType, false)};
            var model = new WaterFlowFMModel();

            // When
            string returnedString = model.GetFeatureCategory(feature);

            // Then
            Assert.That(returnedString, Is.EqualTo(expectedString));
        }

        [Test]
        [TestCase(typeof(Pump), KnownFeatureCategories.Pumps)]
        [TestCase(typeof(Feature), null)]
        public void GivenAFeature_WhenGettingFeatureCategory_ThenTheCorrectStringOrNullIsReturned(Type type, string expectedString)
        {
            // Given
            var feature = (IFeature) Activator.CreateInstance(type, false);
            var model = new WaterFlowFMModel();

            // When
            string returnedString = model.GetFeatureCategory(feature);

            // Then
            Assert.That(returnedString, Is.EqualTo(expectedString));
        }

        [Test]
        public void GivenAnObservationPointThatContainsTheCurrentFeature_WhenGettingFeatureCategory_ThenTheCorrectStringIsReturned()
        {
            // Given
            var feature = new GroupableFeature2DPoint();
            var model = new WaterFlowFMModel();
            model.Area.ObservationPoints.Add(feature);

            // When
            string returnedString = model.GetFeatureCategory(feature);

            // Then
            Assert.That(returnedString, Is.EqualTo(KnownFeatureCategories.ObservationPoints));
        }

        [Test]
        public void GivenAnObservationCrossSectionThatContainsTheCurrentFeature_WhenGettingFeatureCategory_ThenTheCorrectStringIsReturned()
        {
            // Given
            var feature = new ObservationCrossSection2D();
            var model = new WaterFlowFMModel();
            model.Area.ObservationCrossSections.Add(feature);

            // When
            string returnedString = model.GetFeatureCategory(feature);

            // Then
            Assert.That(returnedString, Is.EqualTo(KnownFeatureCategories.ObservationCrossSections));
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void
            GivenAWaterFlowFMModelWithALinkedStructureToRTC_WhenChangingTheWeirFormula_ThenTheLinkShouldBeBrokenAndAWarningShouldBeGiven()
        {
            // Given
            WaterFlowFMModel model = CreateFMModelWithStructureLinkedToRTC(out DataItem rtcDataItem, out IDataItem dataItemWaterFlowFmModel);

            string expectedMessage = string.Format(
                Resources
                    .WaterFlowFMModel_ChangingWeirFormulaWhenAlsoUsedInRTC_Structure_component__0__has_been_removed_from_RTC_Control_Group__1__due_to_type_change,
                dataItemWaterFlowFmModel.Name + "_" + dataItemWaterFlowFmModel.Tag,
                dataItemWaterFlowFmModel.LinkedTo.Parent.ToString());

            // When and Then
            Weir2D feature = model.Area.Weirs.FirstOrDefault();
            Assert.NotNull(feature);

            TestHelper.AssertAtLeastOneLogMessagesContains(() => feature.WeirFormula = new GatedWeirFormula(),
                                                           expectedMessage);

            Assert.IsNull(dataItemWaterFlowFmModel.LinkedTo, "The DataItem of the structure is still linked after changing the weir formula");
            Assert.AreEqual(0, rtcDataItem.LinkedBy.Count, "The DataItem of the RTC component is still linked after changing the weir formula");
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void
            GivenAWaterFlowFMModelWithALinkedStructureToRTC_WhenDeletingTheStructure_ThenTheLinkShouldBeBroken()
        {
            // Given
            WaterFlowFMModel model = CreateFMModelWithStructureLinkedToRTC(out DataItem rtcDataItem, out IDataItem dataItemWaterFlowFmModel);

            // When
            model.Area.Weirs.Clear();

            dataItemWaterFlowFmModel = model.AllDataItems.FirstOrDefault(di => di.ComposedValue is Weir2D);
            Assert.IsNull(dataItemWaterFlowFmModel, "The DataItem for the weir is not removed after removing the weir");
            Assert.AreEqual(0, rtcDataItem.LinkedBy.Count, "The DataItem of the RTC component is still linked after removing the weir");
        }

        [Test]
        public void GivenAnMduWithoutOrZeroValueForPropertyPathsRelativeToParent_WhenImportAndExportThisModel_ThenThisPropertyShouldChangedToOneDuringAnExport()
        {
            // Given
            string mduFilePath = TestHelper.GetTestFilePath(@"small\small.mdu");
            mduFilePath = TestHelper.CreateLocalCopy(mduFilePath);

            var model = new WaterFlowFMModel();
            model.ImportFromMdu(mduFilePath);

            string pathsRelativeToParent = model.ModelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).GetValueAsString();
            Assert.AreEqual("0", pathsRelativeToParent, "The property for PathsRelativeToParent is {0} instead of 0. This is incorrect, because it was not written in the Mdu file", pathsRelativeToParent);

            string saveDirectory = Path.Combine(Path.GetDirectoryName(mduFilePath), "..", "small_saved");

            FileUtils.DeleteIfExists(saveDirectory);
            Directory.CreateDirectory(saveDirectory);

            string targetMduFilePath = Path.Combine(saveDirectory, "small.mdu");

            // When
            model.ExportTo(targetMduFilePath);

            pathsRelativeToParent = model.ModelDefinition.GetModelProperty(KnownProperties.PathsRelativeToParent).GetValueAsString();
            // Then
            Assert.AreEqual("1", pathsRelativeToParent, "The property for PathsRelativeToParent is {0} instead of 1. This is incorrect, because it should change to 1 during an export", pathsRelativeToParent);
        }

        [Test]
        [NUnit.Framework.Category(TestCategory.Integration)]
        public void GivenAModelWithADataItem_WhenAddingNewTracerWithSameName_ThenValueOfThisDataItemIsSetAndNoExtraDataItemIsCreated()
        {
            // Given
            const string tracerName = "tracer";
            var dataItem = new DataItem(null, tracerName, typeof(UnstructuredGridCellCoverage), DataItemRole.Input, "");
            using (var model = new WaterFlowFMModel())
            {
                model.DataItems.Add(dataItem);
                int dataItemCountBefore = model.DataItems.Count;

                // Pre-condition
                Assert.That(dataItem.Value, Is.Null);

                // When
                model.TracerDefinitions.Add(tracerName);

                // Then
                Assert.That(dataItem.Value, Is.SameAs(model.InitialTracers.Single()),
                            "Value of data item was not as expected.");
                Assert.That(model.DataItems.Count, Is.EqualTo(dataItemCountBefore),
                            "No data items should have been added.");
            }
        }

        [TestCase(
            new[]
            {
                UnstructuredGridFileHelper.BedLevelLocation.Faces,
                UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev,
                UnstructuredGridFileHelper.BedLevelLocation.Faces
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
                UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev,
                UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes,
                UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev
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
                UnstructuredGridFileHelper.BedLevelLocation.CellEdges
            },
            new[]
            {
                // UnstructuredGridEdgeCoverage not currently supported
                // returns UnstructuredGridVertexCoverage instead
                typeof(UnstructuredGridVertexCoverage)
            }
        )]
        public void TestUpdateBathymetryCoverage(UnstructuredGridFileHelper.BedLevelLocation[] bedLevelLocations, Type[] coverageTypes)
        {
            // if this is false, the test cases are not correct
            Assert.AreEqual(bedLevelLocations.Length, coverageTypes.Length);

            var fmModel = new WaterFlowFMModel();

            for (var i = 0; i < bedLevelLocations.Length; i++)
            {
                TypeUtils.CallPrivateMethod(fmModel, "UpdateBathymetryCoverage", bedLevelLocations[i]);
                Assert.AreEqual(coverageTypes[i], fmModel.Bathymetry.GetType());
            }
        }

        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.Faces, typeof(UnstructuredGridCellCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.CellEdges, typeof(UnstructuredGridVertexCoverage))] // UnstructuredGridEdgeCoverages not yet supported
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.NodesMeanLev, typeof(UnstructuredGridVertexCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.NodesMinLev, typeof(UnstructuredGridVertexCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.NodesMaxLev, typeof(UnstructuredGridVertexCoverage))]
        [TestCase(UnstructuredGridFileHelper.BedLevelLocation.FacesMeanLevFromNodes, typeof(UnstructuredGridCellCoverage))]
        public void TestInitializeUnstructuredGridCoveragesSetsCorrectBathymetryCoverageType(UnstructuredGridFileHelper.BedLevelLocation bedLevelLocation, Type coverageType)
        {
            // setup
            var fmModel = new WaterFlowFMModel();

            WaterFlowFMProperty bedLevelTypeProperty = fmModel.ModelDefinition.Properties.FirstOrDefault(p => p.PropertyDefinition.MduPropertyName.ToLower() == KnownProperties.BedlevType);
            Assert.NotNull(bedLevelTypeProperty);

            bedLevelTypeProperty.SetValueAsString(((int) bedLevelLocation).ToString());

            // execution
            TypeUtils.CallPrivateMethod(fmModel, "InitializeUnstructuredGridCoverages");

            // check result
            Assert.AreEqual(coverageType, fmModel.Bathymetry.GetType());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GivenAnFmModelWithAWriteClassMapFileProperty_WhenWriteClassMapFileIsCalled_ThenCorrectValueIsReturned(bool expectedValue)
        {
            // Given
            var model = new WaterFlowFMModel();
            model.ModelDefinition.GetModelProperty(GuiProperties.WriteClassMapFile).Value = expectedValue;

            // When
            bool resultedValue = model.WriteClassMapFile;

            // Then
            Assert.AreEqual(expectedValue, resultedValue);
        }

        [TestCase("directory_path", "directory_path\\FlowFM.hyd")]
        [TestCase(null, "")]
        public void GivenAWaterFlowFMModel_WhenHydFilePathIsCalled_ThenCorrectPathIsReturned(string delwaqOutputDirectoryPath, string expectedPath)
        {
            // Given
            var model = new WaterFlowFMModel();
            model.ImportFromMdu("input\\FlowFM.mdu");

            model.DelwaqOutputDirectoryPath = delwaqOutputDirectoryPath;

            // When
            string result = model.HydFilePath;

            // Then
            Assert.AreEqual(expectedPath, result, "Hyd file path was not as expected");
        }

        [TestCase("C:\\project\\modelA\\modelA.mdu", "C:\\project\\modelB\\input\\modelB.mdu")]
        [TestCase("C:\\modelA\\project\\modelA\\input\\modelA.mdu", "C:\\modelA\\project\\modelB\\input\\modelB.mdu")]
        public void GetMduSavePath_WhenModelIsRenamedButFilesAndFolderStillHaveOldNames_ThenCorrectPathIsReturned(string mduFilePath, string expectedMduSavePath)
        {
            // Setup
            using (var model = new WaterFlowFMModel())
            {
                model.ImportFromMdu(mduFilePath);

                model.Name = "modelB";

                // Precondition
                Assert.That(model.MduFilePath, Is.EqualTo(mduFilePath), "Precondition failed.");

                // Call
                string mduSavePath = model.MduSavePath;

                // Assert
                Assert.That(mduSavePath, Is.EqualTo(expectedMduSavePath),
                            $"After renaming the model, the {nameof(model.MduSavePath)} should return the correct path.");
            }
        }

        [Test]
        public void Constructor_CorrectRestartData()
        {
            // Call
            var model = new WaterFlowFMModel();

            // Assert
            Assert.That(model.UseRestart, Is.False);
            Assert.That(model.WriteRestart, Is.False);
            Assert.That(model.RestartInput, Is.Not.Null);
            Assert.That(model.RestartInput.Path, Is.Null);
            Assert.That(model.RestartOutput, Is.Not.Null);
            Assert.That(model.RestartOutput, Is.Empty);
        }

        [Test]
        public void SetRestartInput_Null_ThrowsArgumentNullException()
        {
            // Setup
            var model = new WaterFlowFMModel();

            // Call
            void Call() => model.RestartInput = null;

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("value"));
        }

        [Test]
        public void SetRestartInput_SetsCorrectly()
        {
            // Setup
            var model = new WaterFlowFMModel();
            var restartFile = new RestartFile();

            // Call
            model.RestartInput = restartFile;

            // Assert
            Assert.That(model.RestartInput, Is.SameAs(restartFile));
        }

        [TestCase(null, false)]
        [TestCase("path/to/the.file", true)]
        public void GetUseRestart_ReturnsCorrectResult(string filePath, bool expected)
        {
            // Setup
            var model = new WaterFlowFMModel {RestartInput = {Path = filePath}};

            // Call
            bool result = model.UseRestart;

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        private static WaterFlowFMModel CreateFMModelWithStructureLinkedToRTC(out DataItem rtcDataItem, out IDataItem dataItemWaterFlowFmModel)
        {
            var feature = new Weir2D() {WeirFormula = new SimpleWeirFormula()};

            var model = new WaterFlowFMModel();

            rtcDataItem = new DataItem() {Parent = new DataItem() {Name = "Control Group 1"}};

            model.Area.Weirs.Add(feature);

            dataItemWaterFlowFmModel = model.AllDataItems.FirstOrDefault(di => di.ComposedValue is Weir2D);

            Assert.IsNotNull(dataItemWaterFlowFmModel, "The DataItem for the weir is not created");

            Assert.IsNull(dataItemWaterFlowFmModel.LinkedTo, "The DataItem of the structure is already linked before linking");
            Assert.AreEqual(0, rtcDataItem.LinkedBy.Count,
                            "The DataItem of the RTC component is already linked before linking");

            dataItemWaterFlowFmModel.LinkTo(rtcDataItem);

            Assert.IsNotNull(dataItemWaterFlowFmModel.LinkedTo, "The DataItem of the structure is not linked after linking");
            Assert.AreEqual(1, rtcDataItem.LinkedBy.Count, "The DataItem of the RTC component is not linked after linking");

            Assert.AreSame(dataItemWaterFlowFmModel.LinkedTo, rtcDataItem,
                           "Something else is linked to the DataItem of the structure instead of the RTC component");

            return model;
        }
    }
}