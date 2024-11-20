using System.IO;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Export
{
    [TestFixture]
    public class CrossSectionYZToCsvFileExporterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportCrossSectionYZToCsv()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var branch = network.Branches[1];

            var def1 = CrossSectionDefinitionYZ.CreateDefault();
            var def2 = CrossSectionDefinitionZW.CreateDefault();
            var cs1 = new CrossSection(def1) { Name = "cs1" };
            var cs2 = new CrossSection(def2) { Name = "cs2" };

            NetworkHelper.AddBranchFeatureToBranch(cs1, branch, 50);
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch, 75);

            var exporter = new CrossSectionYZToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);

            Assert.IsTrue(File.Exists(path));
            var file = File.ReadAllLines(path);
            Assert.AreEqual(7, file.Length);
            Assert.IsTrue(file[0].StartsWith("name,branch,chainage"));
            Assert.IsTrue(file[1].StartsWith("cs1," + branch.Name + "," + cs1.Chainage));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportCrossSectionYZWithProxyDefinitionToCsv()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var branch = network.Branches[1];

            var def1 = CrossSectionDefinitionYZ.CreateDefault();
            var def2 = CrossSectionDefinitionZW.CreateDefault();
            var cs1 = new CrossSection(new CrossSectionDefinitionProxy(def1)) { Name = "cs1" };
            var cs2 = new CrossSection(new CrossSectionDefinitionProxy(def2)) { Name = "cs2" };

            NetworkHelper.AddBranchFeatureToBranch(cs1, branch, 50);
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch, 75);

            def2.SummerDike.Active = false;

            var exporter = new CrossSectionYZToCsvFileExporter();
            var path = TestHelper.GetCurrentMethodName() + ".csv";
            exporter.Export(network.CrossSections, path);

            Assert.IsTrue(File.Exists(path));
            var file = File.ReadAllLines(path);
            Assert.AreEqual(7, file.Length);
            Assert.IsTrue(file[0].StartsWith("name,branch,chainage"));
            Assert.IsTrue(file[1].StartsWith("cs1," + branch.Name + "," + cs1.Chainage));
        }
    }
}
