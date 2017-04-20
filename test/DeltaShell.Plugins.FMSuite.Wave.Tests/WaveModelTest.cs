using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using NetTopologySuite.Extensions.Grids;
using NUnit.Framework;
using SharpMap.Extensions.CoordinateSystems;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests
{
    [TestFixture]
    public class WaveModelTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadTestModelFromFile()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw");
            Assert.IsTrue(File.Exists(mdwPath));

            var waveModel = new WaveModel(mdwPath);

            var grid = waveModel.OuterDomain.Grid;
            Assert.IsTrue(grid != null);
            Assert.AreEqual(121, grid.Arguments[0].Values.Count); // N or y-direction
            Assert.AreEqual(236, grid.Arguments[1].Values.Count); // M or x-direction
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadTestModelWithFlowCouplingFromFile()
        {
            var mdwPath = TestHelper.GetTestFilePath(@"flow_coupled/wave.mdw");
            Assert.IsTrue(File.Exists(mdwPath));

            var waveModel = new WaveModel(mdwPath);

            Assert.AreEqual(false, waveModel.OuterDomain.HydroFromFlowData.UseDefaultHydroFromFlowSettings);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.WaterLevelUsage);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.BedLevelUsage);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.WindUsage);
            Assert.AreEqual(UsageFromFlowType.UseAndExtend, waveModel.OuterDomain.HydroFromFlowData.VelocityUsage);
            Assert.AreEqual(VelocityComputationType.DepthAveraged,
                waveModel.OuterDomain.HydroFromFlowData.VelocityUsageType); // not specified, hence default
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void TransformModelCoordinateSystemUpdatesGridCoordinateSystem()
        {
            const int wgs84CS = 4326;
            const int rd = 28992;
            
            var localMdwPath = WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"flow_coupled/wave.mdw"));
            var waveModel = new WaveModel(localMdwPath)
            {
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(wgs84CS)
            };
            
            var src = new OgrCoordinateSystemFactory().CreateFromEPSG(wgs84CS);
            var target = new OgrCoordinateSystemFactory().CreateFromEPSG(rd);
            var transformation = new OgrCoordinateSystemFactory().CreateTransformation(src, target);

            var grid = waveModel.OuterDomain.Grid;
            Assert.IsTrue(grid.CoordinateSystem.IsGeographic);

            string coordinateSystemType;
            Assert.IsTrue(grid.Attributes.TryGetValue(CurvilinearGrid.CoordinateSystemKey, out coordinateSystemType));
            Assert.AreEqual("Spherical", coordinateSystemType);

            waveModel.TransformCoordinates(transformation);

            Assert.IsFalse(grid.CoordinateSystem.IsGeographic);
            Assert.IsTrue(grid.Attributes.TryGetValue(CurvilinearGrid.CoordinateSystemKey, out coordinateSystemType));
            Assert.AreEqual("Cartesian", coordinateSystemType);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void TransformModelCoordinateSystem()
        {
            const int nad27_utm16N = 26916;
            const int pseudo_webm = 3857;

            var localMdwPath =
                WaveTestHelper.CreateLocalCopy(TestHelper.GetTestFilePath(@"wave_timespacevarbnd/tst.mdw"));
            var waveModel = new WaveModel(localMdwPath)
            {
                CoordinateSystem = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N)
            };

            var src = new OgrCoordinateSystemFactory().CreateFromEPSG(nad27_utm16N);
            var target = new OgrCoordinateSystemFactory().CreateFromEPSG(pseudo_webm);
            var fromUTM16ToWebMercator = new OgrCoordinateSystemFactory().CreateTransformation(src, target);
            var fromWebMercatorToUTM16 = new OgrCoordinateSystemFactory().CreateTransformation(target, src);

            var coordinates =
                WaveModelCoordinateConversion.GetAllModelFeatures(waveModel)
                    .SelectMany(f => f.Geometry.Coordinates)
                    .ToList();

            waveModel.TransformCoordinates(fromUTM16ToWebMercator);
            waveModel.TransformCoordinates(fromWebMercatorToUTM16);

            var coordinatesAfter =
                WaveModelCoordinateConversion.GetAllModelFeatures(waveModel)
                    .SelectMany(f => f.Geometry.Coordinates)
                    .ToList();

            Assert.AreEqual(coordinatesAfter.Count, coordinates.Count);
            for (int i = 0; i < coordinates.Count; ++i)
            {
                Assert.AreEqual(coordinatesAfter[i].X, coordinates[i].X, 1e-05);
                Assert.AreEqual(coordinatesAfter[i].Y, coordinates[i].Y, 1e-05);
            }
        }
    }

}