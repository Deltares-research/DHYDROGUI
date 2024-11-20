using System.Collections.Generic;
using DelftTools.Shell.Core;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Importers;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Importers
{
    [TestFixture]
    public class WavmFileImporterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            var importer = new WavmFileImporter();

            // Assert
            Assert.That(importer, Is.InstanceOf<IFileImporter>());
            Assert.That(importer.Name, Is.EqualTo("Wave Output (WAVM)"));
            Assert.That(importer.Category, Is.EqualTo("D-Flow FM 2D/3D"));
            Assert.That(importer.Description, Is.Empty);
            Assert.That(importer.Image, Is.Not.Null);
            Assert.That(importer.CanImportOnRootLevel, Is.True);
            Assert.That(importer.OpenViewAfterImport, Is.False);
            
            CollectionAssert.AreEqual(new[]
            {
                typeof(WavmFileFunctionStore)
            }, importer.SupportedItemTypes);
        }
        
        [Test]
        [TestCaseSource(nameof(GetImportTargetObjects))]
        public void CanImportOn_VariousTargetObjects_ReturnsFalse(object targetObject)
        {
            // Setup
            var importer = new WavmFileImporter();

            // Call
            bool canImportOn = importer.CanImportOn(targetObject);

            // Assert
            Assert.That(canImportOn, Is.False);
        }

        private static IEnumerable<TestCaseData> GetImportTargetObjects()
        {
            yield return new TestCaseData(null);
            yield return new TestCaseData(new object());
            yield return new TestCaseData(new WavmFileFunctionStore(string.Empty));
        }
    }
}