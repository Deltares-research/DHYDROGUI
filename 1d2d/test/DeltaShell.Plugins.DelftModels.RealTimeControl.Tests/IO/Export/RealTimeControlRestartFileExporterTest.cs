using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class RealTimeControlRestartFileExporterTest
    {
        [Test]
        public void GivenRealTimeControlRestartFileExporter_ThenExpectedPropertiesSet()
        {
            var exporter = new RealTimeControlRestartFileExporter();

            Assert.That(exporter.Name, Is.EqualTo("Restart File"));
            Assert.That(exporter.Category, Is.EqualTo("XML"));
            Assert.That(exporter.Description, Is.Empty);
            Assert.That(exporter.FileFilter, Is.EqualTo("Real Time Control restart files|*.xml"));
            Assert.That(exporter.SourceTypes(), Is.EqualTo(new[]
            {
                typeof(RealTimeControlRestartFile)
            }));
        }

        [Test]
        public void GivenNullItem_WhenExported_ThrowsArgumentNullException()
        {
            void Test() => new RealTimeControlRestartFileExporter().Export(null, string.Empty);

            Assert.That(Test, Throws.InstanceOf<ArgumentNullException>().With.Property(nameof(ArgumentNullException.ParamName)).EqualTo("item"));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void GivenEmptyPath_WhenExported_ThrowsArgumentException(string path)
        {
            void Test() => new RealTimeControlRestartFileExporter().Export(new RealTimeControlRestartFile("file.name", "file content"), path);

            TestHelper.AssertLogMessageIsGenerated(Test, "Path cannot be null or empty.");
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenInvalidItemType_WhenExported_ThrowsArgumentException()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string path = Path.Combine(temporaryDirectory.Path, "test.xml");

                void Test() => new RealTimeControlRestartFileExporter().Export(new object(), path);

                TestHelper.AssertLogMessageIsGenerated(Test, "Cannot export type Object.");
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenDirectoryThatDoesNotExist_WhenExported_ThenReturnsFalse()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string path = Path.Combine(temporaryDirectory.Path, "fake_dir", "test.xml");

                var exportResult = true;

                void Test() => exportResult = new RealTimeControlRestartFileExporter().Export(new RealTimeControlRestartFile("file.name", "content"), path);

                IEnumerable<string> logMessages = TestHelper.GetAllRenderedMessages(Test);
                Assert.That(logMessages.Single().StartsWith("Critical exception: Could not find a part of the path"));

                Assert.That(exportResult, Is.False);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GivenValidData_WhenExported_ThenFileExported()
        {
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string path = Path.Combine(temporaryDirectory.Path, "test.xml");

                var exportResult = false;

                void Test() => exportResult = new RealTimeControlRestartFileExporter().Export(new RealTimeControlRestartFile("file.name", "content"), path);

                IEnumerable<string> logMessages = TestHelper.GetAllRenderedMessages(Test);
                Assert.That(logMessages, Is.Empty);
                Assert.That(exportResult, Is.True);
                Assert.That(File.ReadAllText(path), Is.EqualTo("content"));
            }
        }

        [Test]
        public void GivenEmptyRestartFile_WhenGetCanExport_ThenFalseReturned()
        {
            Assert.That(new RealTimeControlRestartFileExporter().CanExportFor(new RealTimeControlRestartFile()), Is.False);
        }

        [Test]
        public void GivenNotEmptyRestartFile_WhenGetCanExport_ThenTrueReturned()
        {
            Assert.That(new RealTimeControlRestartFileExporter().CanExportFor(new RealTimeControlRestartFile("file name", "file contents")), Is.True);
        }

        [Test]
        public void GivenNotRestartFile_WhenGetCanExport_ThenFalseReturned()
        {
            Assert.That(new RealTimeControlRestartFileExporter().CanExportFor(new object()), Is.False);
        }
    }
}