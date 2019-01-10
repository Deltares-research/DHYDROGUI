using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DelftTools.Utils.Reflection;
using DeltaShell.Core;
using DeltaShell.Plugins.CommonTools;
using DeltaShell.Plugins.Data.NHibernate;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.NetworkEditor;
using DeltaShell.Plugins.SharpMapGis;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Grids;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    public class UnstrucGridOperationApiTests
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
        public void GivenModelWithTrachytopes_WhenGridSnappingIsCalled_ThenTrachytopesShouldBeRemovedFromSmallExport()
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try

            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false, false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(model.NetFilePath));

                model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).SetValueAsString("Y");

                var api = new UnstrucGridOperationApi(model, false);
                var tempMduPath = (string)TypeUtils.GetField<UnstrucGridOperationApi, String>(api, "mduFilePath");

                var mduFileDir = Path.GetDirectoryName(tempMduPath);
                var name = Path.GetFileNameWithoutExtension(tempMduPath);
                var fmModelUsedByApi = new WaterFlowFMModel(Path.Combine(mduFileDir,tempMduPath));
                var trtRouUsedInOriginalFMModel = model.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInOriginalFMModel, Is.EqualTo("Y"));
                var trtRouUsedInFMModelByApi = fmModelUsedByApi.ModelDefinition.GetModelProperty(KnownProperties.TrtRou).GetValueAsString();
                Assert.That(trtRouUsedInFMModelByApi, Is.EqualTo("N"));
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
            
            
        }

        [TestCase("bla_bnd.ext" , KnownProperties.ExtForceFile, TestName = "ExtForceFile")]
        [TestCase("bla_thd.pli", KnownProperties.ThinDamFile, TestName = "ThinDamFile")]
        [TestCase("bla_structures.ini", KnownProperties.StructuresFile, TestName = "StructuresFile")]

        public void GivenModelsWithPropertiesToClear_WhenGridSnappingIsCalled_ThenThesePropertiesShouldBeEmpty(string file, string knownProperties)
        {
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try

            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false, false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile).SetValueAsString(Path.GetFileName(model.NetFilePath));

                model.ModelDefinition.GetModelProperty(knownProperties).SetValueAsString(file);

                var api = new UnstrucGridOperationApi(model, false);
                var tempMduPath = (string)TypeUtils.GetField<UnstrucGridOperationApi, String>(api, "mduFilePath");

                var mduFileDir = Path.GetDirectoryName(tempMduPath);
                var name = Path.GetFileNameWithoutExtension(tempMduPath);
                var fmModelUsedByApi = new WaterFlowFMModel(Path.Combine(mduFileDir, tempMduPath));
                var FileUsedInOriginalFMModel = model.ModelDefinition.GetModelProperty(knownProperties).GetValueAsString();
                Assert.That(FileUsedInOriginalFMModel, Is.EqualTo(file));
                var FileUsedInFMModelByApi = fmModelUsedByApi.ModelDefinition.GetModelProperty(knownProperties).GetValueAsString();
                Assert.IsEmpty(FileUsedInFMModelByApi);
            }
            finally
            {
                FileUtils.DeleteIfExists(netFile);
                FileUtils.DeleteIfExists(tempFolder);
            }
        }

        #region Point Sources
        [Test]
        public void GivenAModelWithPointSources_WhenGridSnappingIsCalled_ThenTheSnappedGeometriesShouldBeReturnedAndNotTheGeometriesOfThePointSources()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Point sources will snap to the cell center.
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try

            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                    false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                    .SetValueAsString(Path.GetFileName(model.NetFilePath));

                var api = new UnstrucGridOperationApi(model, false);

                var sourceGeometry = new Point(new Coordinate(2499, 2499, 0));
                var sourceGeometry2 = new Point(new Coordinate(2599, 2599, 0));
                
                var geometries = new List<IGeometry>();
                geometries.Add(sourceGeometry);
                geometries.Add(sourceGeometry2);
                
                const string featureType = UnstrucGridOperationApi.SourceSink;
                
                var snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();

                Assert.AreEqual(2,snappedGeometries.Count, ErrorMessageMissingSnappedGeometriesPointSource);

                Assert.AreEqual(1,snappedGeometries[0].Coordinates.Length, ErrorMessageAmountOfCoordinatesPointSource);
                // The geometries should be not the same, since the original geometries will be returned in case of errors.
                Assert.AreNotEqual(geometries[0].Coordinates[0].X,snappedGeometries[0].Coordinates[0].X, ErrorMessageEqualGeometriesPointSource);
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
        [Category(TestCategory.Jira)] // issue UNST-2232
        public void GivenAModelWithPointSourcesOutsideTheGrid_WhenGridSnappingIsCalled_ThenOnlyTheGeometriesOfThePointsInsideTheGridWillBeSnapped()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Point sources will snap to the cell center.
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try

            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                    false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                    .SetValueAsString(Path.GetFileName(model.NetFilePath));

                var api = new UnstrucGridOperationApi(model, false);

                var sourceGeometry = new Point(new Coordinate(-2499, -2499, 0));
                var sourceGeometry2 = new Point(new Coordinate(2599, 2599, 0));
                
                var geometries = new List<IGeometry>
                {
                    sourceGeometry,
                    sourceGeometry2,
                };

                const string featureType = UnstrucGridOperationApi.SourceSink;

                var snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();
                
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
        #endregion

        #region Source And Sinks
        [Test]
        public void GivenAModelWithSourcesAndSinks_WhenGridSnappingIsCalled_ThenTheSnappedGeometriesShouldBeReturnedAndNotTheGeometriesOfTheSourcesAndSinks()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Sources And Sinks will snap to the cell centers of the first and last coordinates.
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try

            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                    false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                    .SetValueAsString(Path.GetFileName(model.NetFilePath));

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

                var snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();

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
        [Category(TestCategory.Jira)] // issue UNST-2232
        public void GivenAModelWithSourcesAndSinkPointsOutsideTheGrid_WhenGridSnappingIsCalled_ThenOnlyTheGeometriesOfThePointsInsideTheGridWillBeSnapped()
        {
            //Grid X-axis 0-5000, Y-axis 0-5000, steps of 100, Origin bottom left side. Sources And Sinks will snap to the cell centers of the first and last coordinates.
            var netFile = TestHelper.GetTestFilePath(@"basicGrid\basicGrid_net.nc");
            netFile = TestHelper.CreateLocalCopy(netFile);
            Assert.IsTrue(File.Exists(netFile));
            var tempFolder = FileUtils.CreateTempDirectory();
            try
            {
                var model = new WaterFlowFMModel();
                model.ExportTo(Path.Combine(tempFolder, TestHelper.GetCurrentMethodName() + ".mdu"), true, false,
                    false);
                File.Copy(netFile, model.NetFilePath, true);
                model.ModelDefinition.GetModelProperty(KnownProperties.NetFile)
                    .SetValueAsString(Path.GetFileName(model.NetFilePath));

                var api = new UnstrucGridOperationApi(model, false);
                
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
                    sourceAndSink1Geometry,
                    sourceAndSink2Geometry
                };

                const string featureType = UnstrucGridOperationApi.SourceSink;

                var snappedGeometries = api.GetGridSnappedGeometry(featureType, geometries).ToList();

                Assert.AreEqual(2, snappedGeometries.Count, ErrorMessageMissingSnappedGeometriesSourceAndSink);
                                
                // The geometries should be not the same, since the original geometries will be returned in case of errors.
                Assert.AreEqual(1, snappedGeometries[0].Coordinates.Length, ErrorMessageAmountOfCoordinatesSourceAndSink);
                Assert.AreNotEqual(geometries[0].Coordinates[1].X, snappedGeometries[0].Coordinates[0].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[0].Coordinates[1].Y, snappedGeometries[0].Coordinates[0].Y, ErrorMessageEqualGeometriesSourceAndSink);
                
                Assert.AreEqual(2050, snappedGeometries[0].Coordinates[0].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(2050, snappedGeometries[0].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);

                Assert.AreEqual(1, snappedGeometries[1].Coordinates.Length, ErrorMessageAmountOfCoordinatesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[0].X, snappedGeometries[1].Coordinates[0].X, ErrorMessageEqualGeometriesSourceAndSink);
                Assert.AreNotEqual(geometries[1].Coordinates[0].Y, snappedGeometries[1].Coordinates[0].Y, ErrorMessageEqualGeometriesSourceAndSink);
               
                Assert.AreEqual(1950, snappedGeometries[1].Coordinates[0].X, ErrorMessageDifferentGeometryValuesSourceAndSink);
                Assert.AreEqual(1950, snappedGeometries[1].Coordinates[0].Y, ErrorMessageDifferentGeometryValuesSourceAndSink);
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


