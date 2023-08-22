using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class SamplesExporterTest
    {
        [Test]
        public void Export_ItemNull_ThrowsException()
        {
            // Setup
            var exporter = new SamplesExporter();

            // Call
            void Call() => exporter.Export(null, "randomPath.xyz");

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Export_ItemNotSampleSObject_ThrowsException()
        {
            // Setup
            var exporter = new SamplesExporter();
            var item = new object();

            // Call
            void Call() => exporter.Export(item, "randomPath.xyz");

            // Assert
            Assert.That(Call, Throws.ArgumentException);
        }

        [Test]
        public void Export_WritesSamplesToFile()
        {
            // Setup
            var exporter = new SamplesExporter();
            Samples samples = GetSamples();

            using (var tempDir = new TemporaryDirectory())
            {
                const string fileName = "samples.xyz";
                string filePath = Path.Combine(tempDir.Path, fileName);

                // Call
                exporter.Export(samples, filePath);

                // Assert
                string[] fileContent = File.ReadAllLines(filePath);
                AssertThatFileContentIsAsExpected(fileContent);
            }
        }

        private static Samples GetSamples()
        {
            var samples = new Samples("randomName");
            var pointValues = new[]
            {
                new PointValue
                {
                    X = 1,
                    Y = 2,
                    Value = 3
                },
                new PointValue
                {
                    X = 4,
                    Y = 5,
                    Value = 6
                }
            };

            samples.SetPointValues(pointValues);

            return samples;
        }

        private static void AssertThatFileContentIsAsExpected(string[] fileContent)
        {
            Assert.That(fileContent[0], Is.EqualTo("                       1                       2    3"));
            Assert.That(fileContent[1], Is.EqualTo("                       4                       5    6"));
        }
    }
}