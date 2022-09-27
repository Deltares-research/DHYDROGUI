using System.Globalization;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters
{
    [TestFixture]
    public class LocationFileWritersTest
    {
        private IHydroNetwork network;

        [SetUp]
        public void SetUp()
        {
            network = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
        }

        [Test]
        public void TestCrossSectionLocationFileWriterGivesExpectedResults()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            var expectedId = "CrossSection_1D_1";
            var expectedChainage = 20.0;

            FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.YZ, expectedChainage, 1.5, true);
            FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.GeometryBased, 80.0);
            FileWriterTestHelper.AddCrossSection(branch, CrossSectionType.ZW, 30.0, 2.5, true);

            LocationFileWriter.WriteFileCrossSectionLocations(FileWriterTestHelper.ModelFileNames.CrossSectionLocations, network.CrossSections);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionLocations);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, categories.Count(op => op.Name == CrossSectionRegion.IniHeader));

            var content = categories.Where(c => c.Name == CrossSectionRegion.IniHeader).ToList().First();

            var idProperty = content.Properties.First(p => p.Name == LocationRegion.Id.Key);
            Assert.AreEqual(expectedId, idProperty.Value);

            var branchIdProperty = content.Properties.First(p => p.Name == LocationRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, branchIdProperty.Value);

            var chainageProperty = content.Properties.First(p => p.Name == LocationRegion.Chainage.Key);
            Assert.AreEqual(expectedChainage.ToString(LocationRegion.Chainage.Format, CultureInfo.InvariantCulture), chainageProperty.Value);
        }
        
        [Test]
        public void TestObservationPointFileWriterGivesExpectedResults()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            var expectedId = 1;
            var expectedName = "observationPoint1";
            var expectedChainage = 20.0;

            FileWriterTestHelper.AddObservationPoint(branch, expectedId, expectedName, expectedChainage);
            FileWriterTestHelper.AddObservationPoint(branch, 2, "observationPoint2", 40.0);
            FileWriterTestHelper.AddObservationPoint(branch, 3, "observationPoint3", 60.0);

            LocationFileWriter.WriteFileObservationPointLocations(FileWriterTestHelper.ModelFileNames.ObservationPoints, network.ObservationPoints);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.ObservationPoints);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, categories.Count(op => op.Name == ObservationPointRegion.IniHeader));

            var content = categories.Where(c => c.Name == ObservationPointRegion.IniHeader).ToList().First();

            var idProperty = content.Properties.First(p => p.Name == LocationRegion.ObsId.Key);
            Assert.AreEqual(expectedId.ToString(), idProperty.Value);

            var branchIdProperty = content.Properties.First(p => p.Name == LocationRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, branchIdProperty.Value);

            var chainageProperty = content.Properties.First(p => p.Name == LocationRegion.Chainage.Key);
            Assert.AreEqual(expectedChainage.ToString(LocationRegion.Chainage.Format, CultureInfo.InvariantCulture), chainageProperty.Value);

            //var nameProperty = content.Properties.First(p => p.Name == LocationRegion.Name.Key);
            //Assert.AreEqual(expectedName, nameProperty.Value);

        }
        
        [Test]
        public void TestLateralDischargeLocationsFileWriterGivesExpectedResults()
        {
            var branch = network.Branches.FirstOrDefault();
            Assert.NotNull(branch, "No branched added to the network");

            long expectedId = 2;
            string expectedName = "lateralDischarge1";
            double expectedChainage = 0.4;
            double expectedDiffuseLength = 0.7d;

            branch.BranchFeatures.Add(new LateralSource()
            {
                Name = expectedId.ToString(),
                LongName = expectedName,
                Chainage = expectedChainage,
                Length = expectedDiffuseLength
            });

            branch.BranchFeatures.Add(new LateralSource());
            branch.BranchFeatures.Add(new LateralSource());

            LocationFileWriter.WriteFileLateralDischargeLocations(FileWriterTestHelper.ModelFileNames.LateralDischarge, network.LateralSources);

            var delftIniReader = new DelftIniReader();
            var categories = delftIniReader.ReadDelftIniFile(FileWriterTestHelper.ModelFileNames.LateralDischarge);

            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, categories.Count(l => l.Name == BoundaryRegion.LateralDischargeHeader));

            var content = categories.Where(c => c.Name == BoundaryRegion.LateralDischargeHeader).ToList().First();

            var idProperty = content.Properties.First(p => p.Name == LocationRegion.Id.Key);
            Assert.AreEqual(expectedId.ToString(), idProperty.Value);

            var branchIdProperty = content.Properties.First(p => p.Name == LocationRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, branchIdProperty.Value);

            var chainageProperty = content.Properties.First(p => p.Name == LocationRegion.Chainage.Key);
            Assert.AreEqual(expectedChainage.ToString(LocationRegion.Chainage.Format, CultureInfo.InvariantCulture), chainageProperty.Value);

            var lengthProperty = content.Properties.First(p => p.Name == LateralSourceLocationRegion.Length.Key);
            Assert.AreEqual(expectedDiffuseLength.ToString(LateralSourceLocationRegion.Length.Format,CultureInfo.InvariantCulture), lengthProperty.Value);
        }
    }
}