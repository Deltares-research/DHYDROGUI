using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.NGHS.IO.Grid;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class UnstrucGridOperationApiIntegrationTest
    {
        private const string ErrorMessageMissingSnappedGeometriesPointSource =
            "Due to an error during grid snapping of point sources, not every point source is snapped";

        private const string ErrorMessageEqualGeometriesPointSource =
            "Due to an exception, the original geometry of a point source is returned instead of the snapped geometry";

        private const string ErrorMessageDifferentGeometryValuesPointSource = "Expected another value from the kernel for the snapped geometry of a point source";

        private const string ErrorMessageAmountOfCoordinatesPointSource =
            "Due to an error during grid snapping of a point source, the amount of coordinates of the snapped geometry is not correct";

        private const string ErrorMessageMissingSnappedGeometriesSourceAndSink =
            "Due to an error during grid snapping of sources and sinks , not every source and sink is snapped";

        private const string ErrorMessageEqualGeometriesSourceAndSink =
            "Due to an exception, the original geometry of a source and sink is returned instead of the snapped geometry";

        private const string ErrorMessageDifferentGeometryValuesSourceAndSink = "Expected another value from the kernel for the snapped geometry of a source and sink";

        private const string ErrorMessageAmountOfCoordinatesSourceAndSink =
            "Due to an error during grid snapping of a source and sink, the amount of coordinates of the snapped geometry is not correct";

        [Test]
        [Category(TestCategory.Slow)]
        public void GivenModelWithTrachytopes_WhenGridSnappingIsCalled_ThenTrachytopesShouldBeRemovedFromSmallExport()
        {
            string netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            string tempFolder = FileUtils.CreateTempDirectory();

            try
            {
                // Given
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false, false);
                File.Copy(netFile, model.NetFilePath, true);

                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueFromString(Path.GetFileName(model.NetFilePath));
                model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).SetValueFromString("Y");

                var api = new UnstrucGridOperationApi(model, false);
                var tempMduPath = (string) TypeUtils.GetField<UnstrucGridOperationApi, string>(api, "mduFilePath");

                string mduFileDir = Path.GetDirectoryName(tempMduPath);

                var fmModelUsedByApi = new WaterFlowFMModel();
                fmModelUsedByApi.ImportFromMdu(Path.Combine(mduFileDir, tempMduPath));

                string trtRouUsedInOriginalFMModel = model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInOriginalFMModel, Is.EqualTo("Y"));
                string trtRouUsedInFMModelByApi = fmModelUsedByApi.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInFMModelByApi, Is.EqualTo("N"));
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        /// <summary>
        /// GIVEN an FM model with a morphology boundary
        /// WHEN grid snapping is called
        /// THEN morphology should be removed from small export
        /// </summary>
        [Test]
        [Category(TestCategory.Slow)]
        public void GivenAnFMModelWithAMorphologyBoundary_WhenGridSnappingIsCalled_ThenMorphologyShouldBeRemovedFromSmallExport()
        {
            string srcNetFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");

            TestHelper.PerformActionInTemporaryDirectory(tempDir =>
            {
                string mduPath = Path.Combine(tempDir, "morph_test.mdu");
                using (var model = new WaterFlowFMModel())
                {
                    // Given
                    model.ExportTo(mduPath, true, false, false);
                    File.Copy(srcNetFile, model.NetFilePath, true);

                    EnableMorphology(model);
                    AddMorphologyBoundary(model);

                    // When | Then
                    string tempMduPath = null;

                    Assert.DoesNotThrow(() =>
                                        {
                                            using (var api = new UnstrucGridOperationApi(model, false))
                                            {
                                                tempMduPath = TypeUtils.GetField<UnstrucGridOperationApi, string>(api, "mduFilePath");
                                            }
                                        }
                                        , "Expected no exception while constructing UnstrucGRidOperationApi.");

                    Assert.That(tempMduPath, Is.Not.Null,
                                "Expected the API to return a mdu path.");
                    string mduFileDir = Path.GetDirectoryName(tempMduPath);

                    var fmModelUsedByApi = new WaterFlowFMModel();
                    fmModelUsedByApi.ImportFromMdu(Path.Combine(mduFileDir, tempMduPath));

                    Assert.That(fmModelUsedByApi.UseMorSed, Is.False,
                                "Expected the used model not to have morphology.");
                }
            });
        }

        /// <summary>
        /// Enable morphology in the specified model.
        /// </summary>
        /// <param name="model">The model on which morphology is enabled.</param>
        private static void EnableMorphology(WaterFlowFMModel model)
        {
            // Morphology
            model.ModelDefinition.GetModelProperty(GuiProperties.UseMorSed).Value = true;
            var cellsValue = ((int) UnstructuredGridFileHelper.BedLevelLocation.Faces).ToString();
            model.ModelDefinition.GetModelProperty(KnownProperties.BedlevType).SetValueFromString(cellsValue);

            // Sediment
            model.SedimentFractions = new EventedList<ISedimentFraction> {new SedimentFraction {Name = "gloomy_sediment"}};
        }

        /// <summary>
        /// Add a morphology boundary to the specified model.
        /// </summary>
        /// <param name="model">The model to which the boundary is added.</param>
        private static void AddMorphologyBoundary(WaterFlowFMModel model)
        {
            var feature = new Feature2D
            {
                Name = "Boundary2",
                Geometry = new LineString(new[]
                {
                    new Coordinate(1, 0),
                    new Coordinate(0, 1)
                })
            };

            var morphologyBoundaryCondition = new FlowBoundaryCondition(
                FlowBoundaryQuantityType.MorphologyBedLevelPrescribed,
                BoundaryConditionDataType.TimeSeries)
            {
                Feature = feature,
                SedimentFractionNames = new List<string> {"Frick_Freck_and_Frack"}
            };

            morphologyBoundaryCondition.AddPoint(0);
            morphologyBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });
            morphologyBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            morphologyBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var flowBoundaryCondition = new FlowBoundaryCondition(FlowBoundaryQuantityType.WaterLevel,
                                                                  BoundaryConditionDataType.TimeSeries) {Feature = feature};

            flowBoundaryCondition.AddPoint(0);
            flowBoundaryCondition.PointData[0].Arguments[0].SetValues(new[]
            {
                model.StartTime,
                model.StopTime
            });
            flowBoundaryCondition.PointData[0][model.StartTime] = 0.5;
            flowBoundaryCondition.PointData[0][model.StopTime] = 0.6;

            var set = new BoundaryConditionSet {Feature = feature};
            set.BoundaryConditions.Add(flowBoundaryCondition);
            set.BoundaryConditions.Add(morphologyBoundaryCondition);

            model.BoundaryConditionSets.Add(set);
        }

        [TestCase("bla_bnd.ext", KnownProperties.ExtForceFile, TestName = "ExtForceFile")]
        [TestCase("bla_thd.pli", KnownProperties.ThinDamFile, TestName = "ThinDamFile")]
        [TestCase("bla_structures.ini", KnownProperties.StructuresFile, TestName = "StructuresFile")]
        [Category(TestCategory.Slow)]
        public void GivenModelsWithPropertiesToClear_WhenGridSnappingIsCalled_ThenThesePropertiesShouldBeEmpty(string fileName, string modelPropertyName)
        {
            //Given
            using (var model = new WaterFlowFMModel())
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string netFilePath = temporaryDirectory.CopyTestDataFileToTempDirectory(@"basicGrid\basicGrid_net.nc");

                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueFromString(Path.GetFileName(netFilePath));
                model.ModelDefinition.GetModelProperty(modelPropertyName).SetValueFromString(fileName);
                model.ExportTo(Path.Combine(temporaryDirectory.Path, TestHelper.GetCurrentMethodName() + ".mdu"), true, false, false);

                // When
                var api = new UnstrucGridOperationApi(model, false);

                // Then
                string originalFileName = model.ModelDefinition.GetModelProperty(modelPropertyName).GetValueAsString();
                Assert.That(originalFileName, Is.EqualTo(fileName));

                using (var fmModelUsedByApi = new WaterFlowFMModel())
                {
                    fmModelUsedByApi.ImportFromMdu(GetApiMduFilePath(api));

                    string apiFileName = fmModelUsedByApi.ModelDefinition.GetModelProperty(modelPropertyName).GetValueAsString();
                    Assert.IsEmpty(apiFileName);
                }
            }
        }

        private static string GetApiMduFilePath(UnstrucGridOperationApi api)
        {
            string tempMduPath = api.MduFilePath;
            string mduFileDir = Path.GetDirectoryName(tempMduPath);
            return Path.Combine(mduFileDir, tempMduPath);
        }

        #region Point Sources

        [Test]
        public void GivenAModelWithPointSources_WhenGridSnappingIsCalled_ThenTheSnappedGeometriesShouldBeReturnedAndNotTheGeometriesOfThePointSources()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Point sources will snap to the cell center.
            string netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            string tempFolder = FileUtils.CreateTempDirectory();
            try
            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                               false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                     .SetValueFromString(Path.GetFileName(model.NetFilePath));

                var api = new UnstrucGridOperationApi(model, false);

                var sourceGeometry = new Point(new Coordinate(2499, 2499, 0));
                var sourceGeometry2 = new Point(new Coordinate(2599, 2599, 0));

                var geometries = new List<IGeometry>();
                geometries.Add(sourceGeometry);
                geometries.Add(sourceGeometry2);

                const string featureType = UnstrucGridOperationApi.SourceSink;

                List<IGeometry> snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();

                Assert.AreEqual(2, snappedGeometries.Count, ErrorMessageMissingSnappedGeometriesPointSource);

                Assert.AreEqual(1, snappedGeometries[0].Coordinates.Length, ErrorMessageAmountOfCoordinatesPointSource);
                // The geometries should be not the same, since the original geometries will be returned in case of errors.
                Assert.AreNotEqual(geometries[0].Coordinates[0].X, snappedGeometries[0].Coordinates[0].X, ErrorMessageEqualGeometriesPointSource);
                Assert.AreNotEqual(geometries[0].Coordinates[0].Y, snappedGeometries[0].Coordinates[0].Y, ErrorMessageEqualGeometriesPointSource);

                Assert.AreEqual(2450, snappedGeometries[0].Coordinates[0].X, ErrorMessageDifferentGeometryValuesPointSource);
                Assert.AreEqual(2450, snappedGeometries[0].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesPointSource);

                Assert.AreEqual(1, snappedGeometries[1].Coordinates.Length, ErrorMessageAmountOfCoordinatesPointSource);
                Assert.AreNotEqual(geometries[1].Coordinates[0].X, snappedGeometries[1].Coordinates[0].X, ErrorMessageEqualGeometriesPointSource);
                Assert.AreNotEqual(geometries[1].Coordinates[0].Y, snappedGeometries[1].Coordinates[0].Y, ErrorMessageEqualGeometriesPointSource);

                Assert.AreEqual(2550, snappedGeometries[1].Coordinates[0].X, ErrorMessageDifferentGeometryValuesPointSource);
                Assert.AreEqual(2550, snappedGeometries[1].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesPointSource);
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        public void GivenAModelWithPointSourcesOutsideTheGrid_WhenGridSnappingIsCalled_ThenOnlyTheGeometriesOfThePointsInsideTheGridWillBeSnapped()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Point sources will snap to the cell center.
            string netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            string tempFolder = FileUtils.CreateTempDirectory();
            try
            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                               false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                     .SetValueFromString(Path.GetFileName(model.NetFilePath));

                var api = new UnstrucGridOperationApi(model, false);

                var sourceGeometry = new Point(new Coordinate(-2499, -2499, 0));
                var sourceGeometry2 = new Point(new Coordinate(2599, 2599, 0));

                var geometries = new List<IGeometry>
                {
                    sourceGeometry,
                    sourceGeometry2
                };

                const string featureType = UnstrucGridOperationApi.SourceSink;

                List<IGeometry> snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();

                Assert.AreEqual(2, snappedGeometries.Count, ErrorMessageMissingSnappedGeometriesPointSource);

                Assert.AreEqual(0, snappedGeometries[0].Coordinates.Length, ErrorMessageAmountOfCoordinatesPointSource);
                // The geometries should be not the same, since the original geometries will be returned in case of errors.

                Assert.AreEqual(1, snappedGeometries[1].Coordinates.Length, ErrorMessageAmountOfCoordinatesPointSource);
                Assert.AreNotEqual(geometries[1].Coordinates[0].X, snappedGeometries[1].Coordinates[0].X, ErrorMessageEqualGeometriesPointSource);
                Assert.AreNotEqual(geometries[1].Coordinates[0].Y, snappedGeometries[1].Coordinates[0].Y, ErrorMessageEqualGeometriesPointSource);

                Assert.AreEqual(2550, snappedGeometries[1].Coordinates[0].X, ErrorMessageDifferentGeometryValuesPointSource);
                Assert.AreEqual(2550, snappedGeometries[1].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesPointSource);
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        #endregion

        #region Source And Sinks

        [Test]
        public void GivenAModelWithSourcesAndSinks_WhenGridSnappingIsCalled_ThenTheSnappedGeometriesShouldBeReturnedAndNotTheGeometriesOfTheSourcesAndSinks()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Sources And Sinks will snap to the cell centers of the first and last coordinates.
            string netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            string tempFolder = FileUtils.CreateTempDirectory();
            try
            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                               false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                     .SetValueFromString(Path.GetFileName(model.NetFilePath));

                var api = new UnstrucGridOperationApi(model, false);

                //add geometry with three coordinates, so that you can test if only the first and last coordinate is used for grid snapping.
                var sourceAndSink1Geometry = new LineString(new[]
                {
                    new Coordinate(2499, 2499, 0),
                    new Coordinate(2599, 2599, 0),
                    new Coordinate(2699, 2699, 0)
                });

                // Basic source and sink
                var sourceAndSink2Geometry = new LineString(new[]
                {
                    new Coordinate(1999, 1999, 0),
                    new Coordinate(2099, 2099, 0)
                });

                var geometries = new List<IGeometry>();
                geometries.Add(sourceAndSink1Geometry);
                geometries.Add(sourceAndSink2Geometry);

                const string featureType = UnstrucGridOperationApi.SourceSink;

                List<IGeometry> snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();

                Assert.AreEqual(2, snappedGeometries.Count, ErrorMessageMissingSnappedGeometriesSourceAndSink);

                Assert.AreEqual(2, snappedGeometries[0].Coordinates.Length, ErrorMessageAmountOfCoordinatesSourceAndSink);
                // The geometries should be not the same, since the original geometries will be returned in case of errors.
                Assert.AreNotEqual(geometries[0].Coordinates[0].X, snappedGeometries[0].Coordinates[0].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[0].Coordinates[0].Y, snappedGeometries[0].Coordinates[0].Y, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[0].Coordinates[2].X, snappedGeometries[0].Coordinates[1].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[0].Coordinates[2].Y, snappedGeometries[0].Coordinates[1].Y, ErrorMessageEqualGeometriesSourceAndSink);

                Assert.AreEqual(2450, snappedGeometries[0].Coordinates[0].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(2450, snappedGeometries[0].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(2650, snappedGeometries[0].Coordinates[1].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(2650, snappedGeometries[0].Coordinates[1].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);

                Assert.AreEqual(2, snappedGeometries[1].Coordinates.Length, ErrorMessageAmountOfCoordinatesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[0].X, snappedGeometries[1].Coordinates[0].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[0].Y, snappedGeometries[1].Coordinates[0].Y, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[1].X, snappedGeometries[1].Coordinates[1].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[1].Y, snappedGeometries[1].Coordinates[1].Y, ErrorMessageEqualGeometriesSourceAndSink);

                Assert.AreEqual(1950, snappedGeometries[1].Coordinates[0].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(1950, snappedGeometries[1].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(2050, snappedGeometries[1].Coordinates[1].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(2050, snappedGeometries[1].Coordinates[1].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        [Test]
        public void GivenAModelWithSourcesAndSinkPointsOutsideTheGrid_WhenGridSnappingIsCalled_ThenOnlyTheGeometriesOfThePointsInsideTheGridWillBeSnapped()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Sources And Sinks will snap to the cell centers of the first and last coordinates.
            string netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            string tempFolder = FileUtils.CreateTempDirectory();
            try
            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                               false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                     .SetValueFromString(Path.GetFileName(model.NetFilePath));

                var api = new UnstrucGridOperationApi(model, false);

                // Source and sink with source and sink outside grid
                var sourceAndSink0Geometry = new LineString(new[]
                {
                    new Coordinate(-500, -500, 0),
                    new Coordinate(-600, -600, 0)
                });

                // Source and sink with source outside grid
                var sourceAndSink1Geometry = new LineString(new[]
                {
                    new Coordinate(-500, -500, 0),
                    new Coordinate(2099, 2099, 0)
                });

                // Source and sink with sink outside grid
                var sourceAndSink2Geometry = new LineString(new[]
                {
                    new Coordinate(1999, 1999, 0),
                    new Coordinate(-500, -500, 0)
                });

                var geometries = new List<IGeometry>
                {
                    sourceAndSink0Geometry,
                    sourceAndSink1Geometry,
                    sourceAndSink2Geometry
                };

                const string featureType = UnstrucGridOperationApi.SourceSink;

                List<IGeometry> snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();

                Assert.AreEqual(3, snappedGeometries.Count, ErrorMessageMissingSnappedGeometriesSourceAndSink);

                // The geometries should be not the same, since the original geometries will be returned in case of errors.
                Assert.AreEqual(0, snappedGeometries[0].Coordinates.Length, ErrorMessageAmountOfCoordinatesSourceAndSink);

                Assert.AreEqual(1, snappedGeometries[1].Coordinates.Length, ErrorMessageAmountOfCoordinatesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[1].X, snappedGeometries[1].Coordinates[0].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[1].Y, snappedGeometries[1].Coordinates[0].Y, ErrorMessageEqualGeometriesSourceAndSink);

                Assert.AreEqual(2050, snappedGeometries[1].Coordinates[0].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(2050, snappedGeometries[1].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);

                Assert.AreEqual(1, snappedGeometries[1].Coordinates.Length, ErrorMessageAmountOfCoordinatesSourceAndSink);
                Assert.AreNotEqual(geometries[2].Coordinates[0].X, snappedGeometries[2].Coordinates[0].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[2].Coordinates[0].Y, snappedGeometries[2].Coordinates[0].Y, ErrorMessageEqualGeometriesSourceAndSink);

                Assert.AreEqual(1950, snappedGeometries[2].Coordinates[0].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(1950, snappedGeometries[2].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        #endregion
    }
}