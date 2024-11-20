using System;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders.Location;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.NGHS.IO.TestUtils;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class LocationFileReaderTest
    {
        private IHydroNetwork originalNetwork;

        [SetUp]
        public void SetUp()
        {
            originalNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void TestObservationPointLocationFileReaderGivesExpectedResults()
        {
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);

            // Setup network data
            var branch = originalNetwork.Channels.First();
            branch.BranchFeatures.Add(new ObservationPoint()
            {
                Name = LocationFileReaderTestHelper.OBSERVATIONPOINT1_NAME,
                LongName = LocationFileReaderTestHelper.OBSERVATIONPOINT1_LONGNAME,
                Branch = branch,
                Chainage = LocationFileReaderTestHelper.OBSERVATIONPOINT1_CHAINAGE
            });

            branch.BranchFeatures.Add(new ObservationPoint()
            {
                Name = LocationFileReaderTestHelper.OBSERVATIONPOINT2_NAME,
                LongName = LocationFileReaderTestHelper.OBSERVATIONPOINT2_LONGNAME,
                Branch = branch,
                Chainage = LocationFileReaderTestHelper.OBSERVATIONPOINT2_CHAINAGE
            });

            // Write to file
            LocationFileWriter.WriteFileObservationPointLocations(FileWriterTestHelper.ModelFileNames.ObservationPoints, originalNetwork.ObservationPoints);
            
            // Read to file
            var readNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            
            var modelFilename = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            var modelFileNames = new ModelFileNames(modelFilename);
            LocationFileReader.ReadFileObservationPointLocations(modelFileNames.ObservationPoints, readNetwork);
            
            // Comparison
            var originalObservationPoints = originalNetwork.ObservationPoints.ToArray();
            var readObservationPoints = readNetwork.ObservationPoints.ToArray();
            Assert.AreEqual(originalObservationPoints.Length, readObservationPoints.Length);

            for (var i = 0; i < originalObservationPoints.Length; i++)
            {
                Assert.IsTrue(LocationFileReaderTestHelper.CompareObservationPoints(originalObservationPoints[i], readObservationPoints[i]));
            }
        }
        
        [Test]
        public void TestLateralDischargeLocationFileReaderGivesExpectedResults()
        {
            var targetPath = Path.Combine(Environment.CurrentDirectory, FileWriterTestHelper.RelativeTargetDirectory);

            // Setup network data
            var branch = originalNetwork.Channels.First();
            branch.BranchFeatures.Add(new LateralSource()
            {
                Name = LocationFileReaderTestHelper.LATERALDISCHARGE1_NAME,
                LongName = LocationFileReaderTestHelper.LATERALDISCHARGE1_LONGNAME,
                Branch = branch,
                Chainage = LocationFileReaderTestHelper.LATERALDISCHARGE1_CHAINAGE
            });

            branch.BranchFeatures.Add(new LateralSource()
            {
                Name = LocationFileReaderTestHelper.LATERALDISCHARGE2_NAME,
                LongName = LocationFileReaderTestHelper.LATERALDISCHARGE2_LONGNAME,
                Branch = branch,
                Chainage = LocationFileReaderTestHelper.LATERALDISCHARGE2_CHAINAGE
            });

            // Write to file
            LocationFileWriter.WriteFileLateralDischargeLocations(FileWriterTestHelper.ModelFileNames.LateralDischarge, originalNetwork.LateralSources);

            // Read to file
            var readNetwork = FileWriterTestHelper.SetupSimpleHydroNetworkWith2NodesAnd1Branch();
            var modelFilename = Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename);
            var modelFileNames = new ModelFileNames(modelFilename);
            LocationFileReader.ReadFileLateralDischargeLocations(modelFileNames.LateralDischarge, readNetwork);
            
            // Comparison
            var originalLateralSources = originalNetwork.LateralSources.ToArray();
            var readLateralSources = readNetwork.LateralSources.ToArray();
            Assert.AreEqual(originalLateralSources.Length, readLateralSources.Length);

            for (var i = 0; i < originalLateralSources.Length; i++)
            {
                Assert.IsTrue(LocationFileReaderTestHelper.CompareLateralDischargeLocations(originalLateralSources[i], readLateralSources[i]));
            }
        }
    }
}
