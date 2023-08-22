using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Importers
{
    [TestFixture]
    public class SamplesImporterTest
    {
        [Test]
        [TestCaseSource(nameof(GetNullOrWhiteSpaceTestCases))]
        public void ImportItem_FilePathNullOrWhiteSpace_ThrowsException(string filePath)
        {
            // Setup
            var importer = new SamplesImporter();
            var samples = new Samples("randomName");

            // Call
            void Call() => importer.ImportItem(filePath, samples);

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void ImportItem_TargetNotSamples_ThrowsException()
        {
            // Setup
            var importer = new SamplesImporter();
            var target = new object();

            // Call
            void Call() => importer.ImportItem("randomPath.txt", target);

            // Assert
            const string expectedMessage = "target is not a GeoAPI.Extensions.Coverages.IPointCloud.";
            Assert.That(Call, Throws.ArgumentException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void ImportItem_ReturnsUpdatedSamples()
        {
            // Setup
            string fileContent = GetSamplesFileContent();
            var importer = new SamplesImporter();
            var samples = new Samples("randomName");

            using (var tempDir = new TemporaryDirectory())
            {
                const string fileName = "samples.xyz";
                string samplesFilePath = tempDir.CreateFile(fileName, fileContent);

                // Call
                object importedObject = importer.ImportItem(samplesFilePath, samples);

                // Assert
                Assert.That(importedObject, Is.TypeOf<Samples>());
                Assert.That(importedObject, Is.SameAs(samples));

                var importedSamples = (Samples)importedObject;
                Assert.That(importedSamples.SourceFileName, Is.EqualTo(fileName));

                AssertThatPointCloudValuesAreAsExpected(importedSamples.PointValues);
            }
        }

        private static IEnumerable<TestCaseData> GetNullOrWhiteSpaceTestCases()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData("");
            yield return new TestCaseData("   ");
            yield return new TestCaseData(Environment.NewLine);
        }

        private static string GetSamplesFileContent()
        {
            return "1 2 3" + Environment.NewLine +
                   "4 5 6";
        }

        private static void AssertThatPointCloudValuesAreAsExpected(IEnumerable<IPointValue> pointValuesToAssert)
        {
            List<IPointValue> pointValues = pointValuesToAssert.ToList();

            Assert.That(pointValues.Count, Is.EqualTo(2));
            AssertThatPointValueIsAsExpected(pointValues[0], 1, 2, 3);
            AssertThatPointValueIsAsExpected(pointValues[1], 4, 5, 6);
        }

        private static void AssertThatPointValueIsAsExpected(IPointValue pointValue, double x, double y, double value)
        {
            Assert.That(pointValue.X, Is.EqualTo(x));
            Assert.That(pointValue.Y, Is.EqualTo(y));
            Assert.That(pointValue.Value, Is.EqualTo(value));
        }
    }
}