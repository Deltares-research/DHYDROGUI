using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Export
{
    [TestFixture]
    public class CrossSectionXYZToCsvFileExporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportCrossSectionXYZToCsv()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var branch = network.Branches[1];
            const int numProfilePoints = 10;
            const double profileWidth = 200;
            const double chainage = 50;

            var def1 = CrossSectionDefinitionXYZ.CreateDefault();
            var crossPoint = GeometryHelper.LineStringCoordinate(branch.Geometry as LineString, chainage);
            var points =
                Enumerable.Range(0, numProfilePoints).Select(i => ((double) i)/numProfilePoints)
                          .Select(f => new Coordinate(crossPoint.X + profileWidth*(f - 0.5),
                                                      crossPoint.Y + profileWidth*(f - 0.5),
                                                      -Math.Sin(Math.PI*f)));

            def1.Geometry=new LineString(points.ToArray());
            var def2 = CrossSectionDefinitionZW.CreateDefault();
            var cs1 = new CrossSection(def1) { Name = "cs1" };
            var cs2 = new CrossSection(def2) { Name = "cs2" };

            NetworkHelper.AddBranchFeatureToBranch(cs1, branch, 50);
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch, 75);

            var exporter = new CrossSectionXYZToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);

            Assert.IsTrue(File.Exists(path));
            var file = File.ReadAllLines(path);
            Assert.AreEqual(numProfilePoints + 1, file.Length);
            Assert.IsTrue(file[0].StartsWith("name,branch,chainage"));
            Assert.IsTrue(file[1].StartsWith("cs1," + branch.Name + "," + cs1.Chainage));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportCrossSectionXYZWithProxyDefinitionToCsv()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var branch = network.Branches[1];
            const int numProfilePoints = 10;
            const double profileWidth = 200;
            const double chainage = 50;

            var def1 = CrossSectionDefinitionXYZ.CreateDefault();
            var crossPoint = GeometryHelper.LineStringCoordinate(branch.Geometry as LineString, chainage);
            var points =
                Enumerable.Range(0, numProfilePoints).Select(i => ((double)i) / numProfilePoints)
                          .Select(f => new Coordinate(crossPoint.X + profileWidth * (f - 0.5),
                                                      crossPoint.Y + profileWidth * (f - 0.5),
                                                      -Math.Sin(Math.PI * f)));

            def1.Geometry = new LineString(points.ToArray());
            var def2 = CrossSectionDefinitionZW.CreateDefault();
            var cs1 = new CrossSection(new CrossSectionDefinitionProxy(def1)) { Name = "cs1" };
            var cs2 = new CrossSection(new CrossSectionDefinitionProxy(def2)) { Name = "cs2" };

            NetworkHelper.AddBranchFeatureToBranch(cs1, branch, 50);
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch, 75);

            var exporter = new CrossSectionXYZToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);

            Assert.IsTrue(File.Exists(path));
            var file = File.ReadAllLines(path);
            Assert.AreEqual(numProfilePoints + 1, file.Length);
            Assert.IsTrue(file[0].StartsWith("name,branch,chainage"));
            Assert.IsTrue(file[1].StartsWith("cs1," + branch.Name + "," + cs1.Chainage));
        }
    }
}
