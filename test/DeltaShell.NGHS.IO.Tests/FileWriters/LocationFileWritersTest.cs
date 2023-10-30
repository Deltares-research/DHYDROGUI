using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.TestUtils;
using log4net.Core;
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

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.CrossSectionLocations);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, iniSections.Count(op => op.Name == CrossSectionRegion.IniHeader));

            var content = iniSections.Where(c => c.Name == CrossSectionRegion.IniHeader).ToList().First();

            var idProperty = content.Properties.First(p => p.Key == LocationRegion.Id.Key);
            Assert.AreEqual(expectedId, idProperty.Value);

            var branchIdProperty = content.Properties.First(p => p.Key == LocationRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, branchIdProperty.Value);

            var chainageProperty = content.Properties.First(p => p.Key == LocationRegion.Chainage.Key);
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

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.ObservationPoints);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, iniSections.Count(op => op.Name == ObservationPointRegion.IniHeader));

            var content = iniSections.Where(c => c.Name == ObservationPointRegion.IniHeader).ToList().First();

            var idProperty = content.Properties.First(p => p.Key == LocationRegion.ObsId.Key);
            Assert.AreEqual(expectedId.ToString(), idProperty.Value);

            var branchIdProperty = content.Properties.First(p => p.Key == LocationRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, branchIdProperty.Value);

            var chainageProperty = content.Properties.First(p => p.Key == LocationRegion.Chainage.Key);
            Assert.AreEqual(expectedChainage.ToString(LocationRegion.Chainage.Format, CultureInfo.InvariantCulture), chainageProperty.Value);
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

            var iniReader = new IniReader();
            var iniSections = iniReader.ReadIniFile(FileWriterTestHelper.ModelFileNames.LateralDischarge);

            Assert.AreEqual(1, iniSections.Count(g => g.Name == GeneralRegion.IniHeader));
            Assert.AreEqual(3, iniSections.Count(l => l.Name == BoundaryRegion.LateralDischargeHeader));

            var content = iniSections.Where(c => c.Name == BoundaryRegion.LateralDischargeHeader).ToList().First();

            var idProperty = content.Properties.First(p => p.Key == LocationRegion.Id.Key);
            Assert.AreEqual(expectedId.ToString(), idProperty.Value);

            var branchIdProperty = content.Properties.First(p => p.Key == LocationRegion.BranchId.Key);
            Assert.AreEqual(branch.Name, branchIdProperty.Value);

            var chainageProperty = content.Properties.First(p => p.Key == LocationRegion.Chainage.Key);
            Assert.AreEqual(expectedChainage.ToString(LocationRegion.Chainage.Format, CultureInfo.InvariantCulture), chainageProperty.Value);

            var lengthProperty = content.Properties.First(p => p.Key == LateralSourceLocationRegion.Length.Key);
            Assert.AreEqual(expectedDiffuseLength.ToString(LateralSourceLocationRegion.Length.Format,CultureInfo.InvariantCulture), lengthProperty.Value);
        }

        [Test]
        [TestCaseSource(nameof(GetTargetFileNullOrWhiteSpaceCases_WriteFileCrossSectionLocations))]
        public void WriteFileCrossSectionLocations_TargetFileNullOrWhiteSpace_ThrowsArgumentException(
            string targetFile, 
            IEnumerable<ICrossSection> crossSections)
        {
            // Call
            void Call() => LocationFileWriter.WriteFileCrossSectionLocations(targetFile, crossSections);
            
            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        private static IEnumerable<TestCaseData> GetTargetFileNullOrWhiteSpaceCases_WriteFileCrossSectionLocations()
        {
            yield return new TestCaseData(null, Enumerable.Empty<ICrossSection>());
            yield return new TestCaseData(string.Empty, Enumerable.Empty<ICrossSection>());
            yield return new TestCaseData("   ", Enumerable.Empty<ICrossSection>());
        }

        [Test]
        public void WriteFileCrossSectionLocations_CrossSectionsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => LocationFileWriter.WriteFileCrossSectionLocations("random string", null);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void WriteFileCrossSectionLocations_AddsInfoMessageToLogIndicatingWhichFileIsBeingWritten()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string filePath = Path.Combine(temp.Path, "crsloc.ini");

                // Call
                void Call() => LocationFileWriter.WriteFileCrossSectionLocations(filePath, Enumerable.Empty<ICrossSection>());
                IEnumerable<string> infoMessages = TestHelper.GetAllRenderedMessages(Call, Level.Info);

                // Assert
                var expectedMessage = $"Writing locations to {filePath}.";
                Assert.That(infoMessages.Any(m => m.Equals(expectedMessage)));
            }
        }

        [Test]
        [TestCaseSource(nameof(GetTargetFileNullOrWhiteSpaceCases_WriteFileLateralDischargeLocations))]
        public void WriteFileLateralDischargeLocations_TargetFileNullOrWhiteSpace_ThrowsArgumentNullException(
            string targetFile, 
            IEnumerable<ILateralSource> lateralSources)
        {
            // Call
            void Call() => LocationFileWriter.WriteFileLateralDischargeLocations(targetFile, lateralSources);
            
            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        private static IEnumerable<TestCaseData> GetTargetFileNullOrWhiteSpaceCases_WriteFileLateralDischargeLocations()
        {
            yield return new TestCaseData(null, Enumerable.Empty<ILateralSource>());
            yield return new TestCaseData(string.Empty, Enumerable.Empty<ILateralSource>());
            yield return new TestCaseData("   ", Enumerable.Empty<ILateralSource>());
        }

        [Test]
        public void WriteFileLateralDischargeLocations_LateralSourcesNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => LocationFileWriter.WriteFileLateralDischargeLocations("random string", null);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void WriteFileLateralDischargeLocations_AddsInfoMessageToLogIndicatingWhichFileIsBeingWritten()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string filePath = Path.Combine(temp.Path, "laterals.ini");

                // Call
                void Call() => LocationFileWriter.WriteFileLateralDischargeLocations(filePath, Enumerable.Empty<ILateralSource>());
                IEnumerable<string> infoMessages = TestHelper.GetAllRenderedMessages(Call, Level.Info);

                // Assert
                var expectedMessage = $"Writing locations to {filePath}.";
                Assert.That(infoMessages.Any(m => m.Equals(expectedMessage)));
            }
        }

        [Test]
        [TestCaseSource(nameof(GetTargetPathNullOrWhiteSpaceCases_WriteFileObservationPointLocations))]
        public void WriteFileObservationPointLocations_TargetPathNullOrWhiteSpace_ThrowsArgumentNullException(
            string targetFile, 
            IEnumerable<IObservationPoint> observationPoints)
        {
            // Call
            void Call() => LocationFileWriter.WriteFileObservationPointLocations(targetFile, observationPoints);
            
            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        private static IEnumerable<TestCaseData> GetTargetPathNullOrWhiteSpaceCases_WriteFileObservationPointLocations()
        {
            yield return new TestCaseData(null, Enumerable.Empty<IObservationPoint>());
            yield return new TestCaseData(string.Empty, Enumerable.Empty<IObservationPoint>());
            yield return new TestCaseData("   ", Enumerable.Empty<IObservationPoint>());
        }

        [Test]
        public void WriteFileObservationPointLocations_ObservationPointsNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => LocationFileWriter.WriteFileObservationPointLocations("random string", null);
            
            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void WriteFileObservationPointLocations_AddsInfoMessageToLogIndicatingWhichFileIsBeingWritten()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string filePath = Path.Combine(temp.Path, "obspoints.ini");

                // Call
                void Call() => LocationFileWriter.WriteFileObservationPointLocations(filePath, Enumerable.Empty<IObservationPoint>());
                IEnumerable<string> infoMessages = TestHelper.GetAllRenderedMessages(Call, Level.Info);

                // Assert
                var expectedMessage = $"Writing locations to {filePath}.";
                Assert.That(infoMessages.Any(m => m.Equals(expectedMessage)));
            }
        }
    }
}