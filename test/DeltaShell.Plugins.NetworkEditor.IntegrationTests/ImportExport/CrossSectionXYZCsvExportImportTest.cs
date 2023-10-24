using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.ImportExportCsv;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.IntegrationTests.ImportExport
{
    class CrossSectionXYZCsvExportImportTest
    {
        private IHydroNetwork HydroNetwork { get; set; }

        private static LineString CreateVerticalString(Coordinate centroid, double length, int points)
        {
            var coordinates = new List<Coordinate>();
            for (int i = 0; i < points; ++i)
            {
                var x = centroid.X;
                var y = centroid.Y + i*(length/points) - 0.5*length;
                var z = 5*Math.Abs(y - centroid.Y)/length-2.5;
                coordinates.Add(new Coordinate(x, y, z));
            }
            return new LineString(coordinates.ToArray());
        }

        private static LineString CreateHorizontalString(Coordinate centroid, double length, int points)
        {
            var coordinates = new List<Coordinate>();
            for (int i = 0; i < points; ++i)
            {
                var x = centroid.X + i*(length/points) - 0.5*length;
                var y = centroid.Y;
                var z = 5 * Math.Abs(x - centroid.X) / length - 2.5;
                coordinates.Add(new Coordinate(x, y, z));
            }
            return new LineString(coordinates.ToArray());
        }

        [SetUp]
        public void SetUp()
        {
            HydroNetwork = new HydroNetwork { Name = "network" };

            var node1 = new Node { Name = "a", Geometry = new Point(0, 0) };
            var node2 = new Node { Name = "b", Geometry = new Point(100, 0) };
            var node3 = new Node { Name = "c", Geometry = new Point(100, 100) };
            var node4 = new Node { Name = "d", Geometry = new Point(200, 100) };
            HydroNetwork.Nodes.AddRange(new[] { node1, node2, node3, node4 });

            var branch1 = new Branch("ab", node1, node2, 100);
            var branch2 = new Branch("bc", node2, node3, 100);
            var branch3 = new Branch("cd", node3, node4, 100);
            
            var csd1 = CrossSectionDefinitionXYZ.CreateDefault();
            csd1.Geometry = CreateVerticalString(new Coordinate(50, 0, 0), 20, 10);
            var cs1 = new CrossSection(csd1) { Name = "cs1" };
            NetworkHelper.AddBranchFeatureToBranch(cs1, branch1, 50);

            var csd2 = CrossSectionDefinitionXYZ.CreateDefault();
            csd2.Geometry = CreateHorizontalString(new Coordinate(100, 50, 0), 20, 10);
            var cs2 = new CrossSection(csd2) { Name = "cs2" };
            NetworkHelper.AddBranchFeatureToBranch(cs2, branch2, 50);

            var csd3 = CrossSectionDefinitionXYZ.CreateDefault();
            csd3.Geometry = CreateVerticalString(new Coordinate(150, 100, 0), 20, 10);
            var cs3 = new CrossSection(csd3) { Name = "cs3" };
            NetworkHelper.AddBranchFeatureToBranch(cs3, branch3, 50);

            HydroNetwork.Branches.AddRange(new[] { branch1, branch2, branch3 });
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportXYZCrossSections()
        {
            var path = TestHelper.GetCurrentMethodName() + ".csv";

            var exporter = new CrossSectionXYZToCsvFileExporter();

            exporter.Export(HydroNetwork.CrossSections.ToList(), path);

            var nCrossSections = HydroNetwork.CrossSections.Count();

            Assert.AreNotEqual(0, nCrossSections);

            foreach (var cs in HydroNetwork.CrossSections.ToList())
            {
                cs.Branch.BranchFeatures.Remove(cs);
            }

            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());

            // import using invariant culture
            using (CultureUtils.SwitchToInvariantCulture())
            {
                var importer = new CrossSectionXYZFromCsvFileImporter { FilePath = path };
                importer.ImportItem(null, HydroNetwork);
            }
            Assert.AreEqual(nCrossSections, HydroNetwork.CrossSections.Count());
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ExportImportXYZCrossSectionsZWShouldBeSkipped()
        {
            var path = TestHelper.GetCurrentMethodName() + ".csv";

            var exporter = new CrossSectionXYZToCsvFileExporter();

            var nCrossSectionBefore = HydroNetwork.CrossSections.Count();

            var branch1 = HydroNetwork.Branches[0];

            NetworkHelper.AddBranchFeatureToBranch(new CrossSection(CrossSectionDefinitionZW.CreateDefault()), branch1, 10);

            exporter.Export(HydroNetwork.CrossSections.ToList(), path);


            var nCrossSectionsAfter = HydroNetwork.CrossSections.Count();

            Assert.AreNotEqual(0, nCrossSectionsAfter);

            foreach (var cs in HydroNetwork.CrossSections.ToList())
            {
                cs.Branch.BranchFeatures.Remove(cs);
            }

            Assert.AreEqual(0, HydroNetwork.CrossSections.Count());

            var importer = new CrossSectionXYZFromCsvFileImporter { FilePath = path };
            importer.ImportItem(null, HydroNetwork);

            Assert.AreEqual(nCrossSectionBefore, HydroNetwork.CrossSections.Count());
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ExportChangeCultureImportXYZCrossSections()
        {
            using (CultureUtils.SwitchToCulture("nl-NL"))
            {
                var path = TestHelper.GetCurrentMethodName() + ".csv";
                var exporter = new CrossSectionXYZToCsvFileExporter();
                var crossSection = HydroNetwork.CrossSections.FirstOrDefault();
                double chainage = crossSection.Chainage;
                exporter.Export(HydroNetwork.CrossSections.ToList(), path);

                crossSection.Chainage += 0.5;

                var nCrossSections = HydroNetwork.CrossSections.Count();

                Assert.AreNotEqual(0, nCrossSections);

                foreach (var cs in HydroNetwork.CrossSections.ToList())
                {
                    cs.Branch.BranchFeatures.Remove(cs);
                }

                Assert.AreEqual(0, HydroNetwork.CrossSections.Count());

                // import using invariant culture
                using (CultureUtils.SwitchToInvariantCulture())
                {
                    var importer = new CrossSectionXYZFromCsvFileImporter { FilePath = path };
                    importer.ImportItem(null, HydroNetwork);
                }

                Assert.AreEqual(nCrossSections, HydroNetwork.CrossSections.Count());
                crossSection = HydroNetwork.CrossSections.FirstOrDefault();
                Assert.AreEqual(chainage, crossSection.Chainage, 0.001);
            }
        }
    }
}
