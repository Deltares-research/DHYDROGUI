using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.GroupableFeatures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.Common.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.ImportersExporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    [TestFixture]
    public class PlizFileImporterExporterTest
    {
        #region Import BridgePillars

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Test_PliFileImporterExporter_ImportBridgePillars()
        {
            string importPath = TestHelper.GetTestFilePath(@"BridgePillarsImport\bridge-1.pliz");
            importPath = TestHelper.CreateLocalCopy(importPath);
            Assert.IsTrue(File.Exists(importPath));

            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                //We need to initialize the application as the PlizFile requires to have the custom delegate
                //methods for the bridge pillars in the Importer/Exporter.

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                
                //Setup new model and pillars.
                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                var importer =
                    app.FileImporters.First(fi => fi is PlizFileImporterExporter<BridgePillar, BridgePillar>) as
                        PlizFileImporterExporter<BridgePillar, BridgePillar>;

                importer.ImportItem(importPath, model.Area.BridgePillars);
                Assert.IsNotNull(model.Area.BridgePillars);
                //check content
                BridgePillar bp = model.Area.BridgePillars.First();
                Assert.IsNotNull(bp);

                var diameterList = new List<double>
                {
                    555,
                    555,
                    555,
                    555
                };
                var coeffList = new List<double>
                {
                    323,
                    323,
                    323,
                    323
                };

                /* Check contents of the Bridge Pillar */
                ModelFeatureCoordinateData<BridgePillar> loadedBpDataModel = model.BridgePillarsDataModel[0];

                Assert.AreEqual(diameterList, loadedBpDataModel.DataColumns[0].ValueList);
                Assert.AreEqual(coeffList, loadedBpDataModel.DataColumns[1].ValueList);
            }
        }

        public static IEnumerable<TestCaseData> GetPlizImportTestData()
        {
            PlizFileImporterExporter<T, T> GetImporter<T>(WaterFlowFMModel fmModel) where T : class, IFeature, INameable, IGroupableFeature, new() =>
                new PlizFileImporterExporter<T, T>
                {
                    EqualityComparer = new GroupableFeatureComparer<T>(),
                    AfterCreateAction = (featureList, feature) =>
                    {
                        feature.UpdateGroupName(fmModel);
                    }
                };

            yield return new TestCaseData((Func<WaterFlowFMModel, PlizFileImporterExporter<BridgePillar, BridgePillar>>)GetImporter<BridgePillar>,
                                          new Func<WaterFlowFMModel, IEventedList<BridgePillar>>(model => model.Area.BridgePillars));
            yield return new TestCaseData((Func<WaterFlowFMModel, PlizFileImporterExporter<FixedWeir, FixedWeir>>)GetImporter<FixedWeir>,
                                          new Func<WaterFlowFMModel, IEventedList<FixedWeir>>(model => model.Area.FixedWeirs));
        }

        [Test]
        [Category(TestCategory.Integration)]
        [TestCaseSource(nameof(GetPlizImportTestData))]
        public void GivenFmModelWithoutPlizFeatures_WhenImportingFeaturesFromAPlizFileWithNonUniqueNames_ThenTheFeaturesWithTheSameNameInTheFileWillBeRenamed<T>(Func<WaterFlowFMModel, PlizFileImporterExporter<T, T>> importerFunc,
                                                                                                                                                                 Func<WaterFlowFMModel, IEventedList<T>> featuresFunc)
            where T : class, IFeature, INameable, IGroupableFeature, new()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string filePath = TestHelper.GetTestFilePath("pli_files/geometries_non_unique_names.pliz");
                string tempFilePath = tempDirectory.CopyTestDataFileToTempDirectory(filePath);
                var fmModel = new WaterFlowFMModel();

                PlizFileImporterExporter<T, T> importer = importerFunc(fmModel);

                // Act and assert
                IEventedList<T> featuresImport = featuresFunc(fmModel);
                TestHelper.AssertAtLeastOneLogMessagesContains(() => importer.ImportItem(tempFilePath, featuresImport), Resources.Feature2DImportExportBase_AddOrReplace_The_list_of_imported_features_did_not_contain_unique_names_Names_were_made_unique_during_import);

                IEventedList<T> featuresResult = featuresFunc(fmModel);
                Assert.AreEqual(10, featuresResult.Count);
                Assert.AreEqual(10, featuresResult.Select(w => w.Name).Distinct().Count(), "All names should be unique");
            }
        }
        #endregion

        #region Basic properties tests

        private PlizFileImporterExporter<TFeat, TFeat> GetImporterExporter<TFeat>()
            where TFeat : class, IFeature, INameable, new()
        {
            var plizIE = new PlizFileImporterExporter<TFeat, TFeat>();
            Assert.IsNotNull(plizIE);
            return plizIE;
        }

        [Test]
        public void Test_PlizFileImporterExporter_Category()
        {
            var expectedText = "Feature geometries";
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            Assert.AreEqual(expectedText, plizIE.Category);
        }

        [Test]
        public void Test_PlizFileImporterExporter_FileFilter()
        {
            var expectedText = "Feature polyline-z files (*.pliz)|*.pliz";
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            Assert.AreEqual(expectedText, plizIE.FileFilter);
        }

        [Test]
        public void Test_PlizFileImporterExporter_Image()
        {
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            Assert.IsNotNull(plizIE);
        }

        [Test]
        public void Test_PlizFileImporterExporter_SourceTypes()
        {
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            Assert.IsTrue(plizIE.SourceTypes().Contains(typeof(BridgePillar)));
            Assert.IsTrue(plizIE.SourceTypes().Contains(typeof(IList<BridgePillar>)));
        }

        [Test]
        public void Test_PlizFileImporterExporter_SupportedItemTypes()
        {
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            Assert.IsTrue(plizIE.SupportedItemTypes.Contains(typeof(IList<BridgePillar>)));
        }

        #endregion

        #region Export

        [Test]
        public void Test_PlizFileImporterExporter_Export_PathNull_Returns_False()
        {
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            Assert.IsFalse(plizIE.Export(null, null));
        }

        [Test]
        public void Test_PlizFileImporterExporter_Export_PathNull_ButhPathsDefined_Exports()
        {
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            var testfilePliz = "testFile.pliz";
            plizIE.Files = new[]
            {
                testfilePliz
            };
            Assert.IsTrue(plizIE.Files.Contains(testfilePliz));

            var pillar = new BridgePillar()
            {
                Name = "BridgePillar2Test",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(20.0, 60.0, 0),
                        new Coordinate(140.0, 8.0, 1.0),
                        new Coordinate(180.0, 4.0, 2.0),
                        new Coordinate(260.0, 0.0, 3.0)
                    })
            };

            //Checking the content of the file is an integration test done later on.
            Assert.IsTrue(plizIE.Export(pillar, null));
        }

        [Test]
        public void Test_PlizFileImporterExporter_Export_ObjectList_Exports()
        {
            PlizFileImporterExporter<BridgePillar, BridgePillar> plizIE = GetImporterExporter<BridgePillar>();
            var testfilePliz = "testFile.pliz";
            plizIE.Files = new[]
            {
                testfilePliz
            };
            Assert.IsTrue(plizIE.Files.Contains(testfilePliz));

            var pillar = new BridgePillar()
            {
                Name = "BridgePillar2Test",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(20.0, 60.0, 0),
                        new Coordinate(140.0, 8.0, 1.0),
                        new Coordinate(180.0, 4.0, 2.0),
                        new Coordinate(260.0, 0.0, 3.0)
                    })
            };

            //Checking the content of the file is an integration test done later on.
            Assert.IsTrue(plizIE.Export(new List<BridgePillar>() { pillar }, null));
        }

        #endregion

        #region Integration tests / Bridge pillars

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Test_PlizFileImporterExporter_ExportBridgePillar()
        {
            string exportPath = TestHelper.GetTestFilePath("bridgePillars\\testBridgePillars.pliz");
            FileUtils.DeleteIfExists(exportPath);

            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                //We need to initialize the application as the PlizFile requires to have the custom delegate
                //methods for the bridgepillars in the Importer/Exporter.

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Run();

                app.CreateNewProject();

                //Setup new model and pillars.
                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                //Create some dummy bridge pillaras
                var pillar = new BridgePillar()
                {
                    Name = "BridgePillarTest",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(0.0, 160.0, 0),
                            new Coordinate(40.0, 80.0, 10.0),
                            new Coordinate(80.0, 40.0, 20.0),
                            new Coordinate(160.0, 0.0, 30.0)
                        })
                };
                model.Area.BridgePillars.Add(pillar);
                Assert.IsTrue(model.Area.BridgePillars.Contains(pillar));

                //Set the BridgePillars values.
                /* Set data model */
                IList<ModelFeatureCoordinateData<BridgePillar>> modelFeatureCoordinateDatas = model.BridgePillarsDataModel;
                modelFeatureCoordinateDatas.Clear();

                var modelFeatureCoordinateData = new ModelFeatureCoordinateData<BridgePillar>() { Feature = pillar };
                modelFeatureCoordinateData.UpdateDataColumns();
                //Diameters
                modelFeatureCoordinateData.DataColumns[0].ValueList = new List<double>
                {
                    1.0,
                    2.5,
                    5.0,
                    10.0
                };
                //DragCoefficient
                modelFeatureCoordinateData.DataColumns[1].ValueList = new List<double>
                {
                    10.0,
                    5.0,
                    2.5,
                    1.0
                };

                modelFeatureCoordinateDatas.Add(modelFeatureCoordinateData);
                MduFile.SetBridgePillarAttributes(model.Area.BridgePillars, modelFeatureCoordinateDatas);
                /* Done only for testing purposes. 
                 * Please do not attempt to do this without the supervision of another adult. */
                TypeUtils.SetPrivatePropertyValue(model, "BridgePillarsDataModel", modelFeatureCoordinateDatas);

                //Export BridgePillars to PLIZ file.
                var exporter = app.FileExporters.First(fi => fi is PlizFileImporterExporter<BridgePillar, BridgePillar>) as PlizFileImporterExporter<BridgePillar, BridgePillar>;

                Assert.IsNotNull(exporter, "PlizFileImporterExporter for BridgePillar was not added to the FMPlugin.");
                Assert.IsTrue(exporter.Export(model.Area.BridgePillars, exportPath));

                //Make sure the content has been generated.
                //The reason why we check the hardcoded lines it's because we are not meant to test the import on this test.
                //Thus we just check exactly what we expect.
                Assert.IsTrue(File.Exists(exportPath));
                IEnumerable<string> textLines = File.ReadLines(exportPath);
                var expectedLines = new List<string>()
                {
                    "BridgePillarTest",
                    "    4    4",
                    "0.000000000000000E+000  1.600000000000000E+002  1.000000000000000E+000  1.000000000000000E+001",
                    "4.000000000000000E+001  8.000000000000000E+001  2.500000000000000E+000  5.000000000000000E+000",
                    "8.000000000000000E+001  4.000000000000000E+001  5.000000000000000E+000  2.500000000000000E+000",
                    "1.600000000000000E+002  0.000000000000000E+000  1.000000000000000E+001  1.000000000000000E+000"
                };

                var idx = 0;
                foreach (string textLine in textLines)
                {
                    string expectedLine = expectedLines[idx];
                    Assert.AreEqual(expectedLine, textLine);
                    idx++;
                }
            }

            FileUtils.DeleteIfExists(exportPath);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void Test_PlizFileImporterExporter_ExportListOfBridgePillars()
        {
            string exportPath = TestHelper.GetTestFilePath("bridgePillars\\testBridgePillars.pliz");
            FileUtils.DeleteIfExists(exportPath);

            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                //We need to initialize the application as the PlizFile requires to have the custom delegate
                //methods for the bridgepillars in the Importer/Exporter.

                app.Plugins.Add(new SharpMapGisApplicationPlugin());
                app.Plugins.Add(new NetworkEditorApplicationPlugin());
                app.Plugins.Add(new FlowFMApplicationPlugin());
                app.Plugins.Add(new CommonToolsApplicationPlugin());
                app.Plugins.Add(new NHibernateDaoApplicationPlugin());
                app.Run();

                app.CreateNewProject();
                
                //Setup new model and pillars.
                var model = new WaterFlowFMModel();
                app.Project.RootFolder.Add(model);

                //Create some dummy bridge pillaras
                var pillar1 = new BridgePillar()
                {
                    Name = "BridgePillarTest",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(0.0, 160.0, 0),
                            new Coordinate(40.0, 80.0, 10.0),
                            new Coordinate(80.0, 40.0, 20.0),
                            new Coordinate(160.0, 0.0, 30.0)
                        })
                };
                var pillar2 = new BridgePillar()
                {
                    Name = "BridgePillar2Test",
                    Geometry =
                        new LineString(new[]
                        {
                            new Coordinate(20.0, 60.0, 0),
                            new Coordinate(140.0, 8.0, 1.0),
                            new Coordinate(180.0, 4.0, 2.0),
                            new Coordinate(260.0, 0.0, 3.0)
                        })
                };
                model.Area.BridgePillars.Add(pillar1);
                model.Area.BridgePillars.Add(pillar2);
                Assert.IsTrue(model.Area.BridgePillars.Contains(pillar1));
                Assert.IsTrue(model.Area.BridgePillars.Contains(pillar2));

                /*Set values to the Bridge Pillar data model*/
                //Set the BridgePillars values.
                IList<ModelFeatureCoordinateData<BridgePillar>> modelFeatureCoordinateDatas = model.BridgePillarsDataModel;
                modelFeatureCoordinateDatas.Clear();

                var mfPillar1 = new ModelFeatureCoordinateData<BridgePillar>() { Feature = pillar1 };
                mfPillar1.UpdateDataColumns();
                //Diameters
                mfPillar1.DataColumns[0].ValueList = new List<double>
                {
                    1.0,
                    2.5,
                    5.0,
                    10.0
                };
                //DragCoefficient
                mfPillar1.DataColumns[1].ValueList = new List<double>
                {
                    10.0,
                    5.0,
                    2.5,
                    1.0
                };

                var mfPillar2 = new ModelFeatureCoordinateData<BridgePillar>() { Feature = pillar2 };
                mfPillar2.UpdateDataColumns();
                //Diameters
                mfPillar2.DataColumns[0].ValueList = new List<double>
                {
                    0,
                    25,
                    50,
                    100
                };
                //DragCoefficient
                mfPillar2.DataColumns[1].ValueList = new List<double>
                {
                    1.0,
                    50,
                    25,
                    10
                };

                modelFeatureCoordinateDatas.Add(mfPillar1);
                modelFeatureCoordinateDatas.Add(mfPillar2);
                MduFile.SetBridgePillarAttributes(model.Area.BridgePillars, modelFeatureCoordinateDatas);
                /* Done only for testing purposes. 
                 * Please do not attempt to do this without the supervision of another adult. */
                TypeUtils.SetPrivatePropertyValue(model, "BridgePillarsDataModel", modelFeatureCoordinateDatas);

                //Export BridgePillars to PLIZ file.
                var exporter =
                    app.FileExporters.First(fi => fi is PlizFileImporterExporter<BridgePillar, BridgePillar>) as
                        PlizFileImporterExporter<BridgePillar, BridgePillar>;
                Assert.IsNotNull(exporter, "PlizFileImporterExporter for BridgePillar was not added to the FMPlugin.");
                Assert.IsTrue(exporter.Export(model.Area.BridgePillars, exportPath));

                //Make sure the content has been generated.
                //The reason why we check the hardcoded lines it's because we are not meant to test the import on this test.
                //Thus we just check exactly what we expect.
                Assert.IsTrue(File.Exists(exportPath));
                IEnumerable<string> textLines = File.ReadLines(exportPath);
                var expectedLines = new List<string>()
                {
                    "BridgePillarTest",
                    "    4    4",
                    "0.000000000000000E+000  1.600000000000000E+002  1.000000000000000E+000  1.000000000000000E+001",
                    "4.000000000000000E+001  8.000000000000000E+001  2.500000000000000E+000  5.000000000000000E+000",
                    "8.000000000000000E+001  4.000000000000000E+001  5.000000000000000E+000  2.500000000000000E+000",
                    "1.600000000000000E+002  0.000000000000000E+000  1.000000000000000E+001  1.000000000000000E+000",
                    "BridgePillar2Test",
                    "    4    4",
                    "2.000000000000000E+001  6.000000000000000E+001  0.000000000000000E+000  1.000000000000000E+000",
                    "1.400000000000000E+002  8.000000000000000E+000  2.500000000000000E+001  5.000000000000000E+001",
                    "1.800000000000000E+002  4.000000000000000E+000  5.000000000000000E+001  2.500000000000000E+001",
                    "2.600000000000000E+002  0.000000000000000E+000  1.000000000000000E+002  1.000000000000000E+001"
                };

                var idx = 0;
                foreach (string textLine in textLines)
                {
                    string expectedLine = expectedLines[idx];
                    Assert.AreEqual(expectedLine, textLine);
                    idx++;
                }
            }

            FileUtils.DeleteIfExists(exportPath);
        }

        #endregion
    }
}