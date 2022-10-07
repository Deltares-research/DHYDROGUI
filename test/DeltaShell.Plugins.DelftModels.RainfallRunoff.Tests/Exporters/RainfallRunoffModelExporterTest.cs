using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Exporters;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Exporters
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class RainfallRunoffModelExporterTest
    {
        private RainfallRunoffModelExporter exporter;
        private RainfallRunoffModel rainfallRunoffModel;

        [SetUp]
        public void Setup()
        {
            exporter = new RainfallRunoffModelExporter(new BasinGeometryShapeFileSerializer(), Substitute.For<IEvaporationExporter>());
            rainfallRunoffModel = new RainfallRunoffModel();
        }

        [Test]
        [TestCaseSource(nameof(ConstructorArgNullCases))]
        public void Constructor_ArgNull_ThrowsArgumentNullException(IBasinGeometrySerializer serializer, 
                                                                    IEvaporationExporter evaporationExporter, 
                                                                    string expParamName)
        {
            // Call
            void Call() => new RainfallRunoffModelExporter(serializer, evaporationExporter);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName)).EqualTo(expParamName));
        }

        [Test]
        public void ExportRainfallRunoffModel()
        {
            var tempExportDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            bool exportSuccessful = exporter.Export(rainfallRunoffModel, tempExportDir);
            Assert.IsTrue(exportSuccessful);
            const string sobekBFnm = "Sobek_3b.fnm";
            Assert.IsTrue(File.Exists(Path.Combine(tempExportDir, sobekBFnm)));
            var linesWithFiles = File.ReadAllLines(System.IO.Path.Combine(tempExportDir, sobekBFnm));
            IList<string> inputFilenames = new List<string>();
            inputFilenames.Add(sobekBFnm);
            IList<string> generatedInputFiles = Directory.GetFiles(tempExportDir)
                .Select(s => Path.GetFileName(s).ToLower())
                .ToList();
            //var fileCount = 1;
            foreach (var linesWithFile in linesWithFiles)
            {
                if (linesWithFile.StartsWith("*") || !linesWithFile.EndsWith("I") ||
                    string.IsNullOrWhiteSpace(linesWithFile)) continue;
                var splittedLinesWithFile = linesWithFile.Split(' ');
                var fileName = splittedLinesWithFile.FirstOrDefault();
                if (fileName == null) continue;
                fileName = fileName.Replace("'", "").ToLower(); // remove the single quotes
                Assert.IsTrue(File.Exists(Path.Combine(tempExportDir, sobekBFnm)));
                //fileCount++;
                inputFilenames.Add(fileName);
                generatedInputFiles.Remove(fileName);
            }
            Console.WriteLine(string.Join(", ", generatedInputFiles));
        }

        [Test]
        public void GivenRainfallRunoffModelExporterWhenExportingWithItemNotEqualToRainfallRunoffModelThenReturnsFalse()
        {
            var exportResult = exporter.Export("NotARainfallRunoffModel", Arg<string>.Is.Anything);
            Assert.IsFalse(exportResult);
        }

        [Test]
        public void GivenRainfallRunoffModelExporterWhenGettingSourceTypesThenReturnRainfallRunoffModel()
        {
            var sourceTypes = exporter.SourceTypes().AsList();
            Assert.That(sourceTypes.Count, Is.EqualTo(1));
            Assert.That(sourceTypes[0], Is.EqualTo(typeof(RainfallRunoffModel)));
        }

        [Test]
        public void GivenRainfallRunoffModelExporterWhenRequestingFileFilterThenReturnStringThatEndsWithAPoint()
        {
            var filter = exporter.FileFilter;
            Assert.That(filter.EndsWith("*."), Is.True);
        }

        private static IEnumerable<TestCaseData> ConstructorArgNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IEvaporationExporter>(), "serializer");
            yield return new TestCaseData(Substitute.For<IBasinGeometrySerializer>(), null, "evaporationExporter");
        }
    }
}