using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class StructuresListExporterTest
    {
        private StructuresListExporter exporter;

        [SetUp]
        public void Setup()
        {
            exporter = new StructuresListExporter(StructuresListType.Gates);
        }

        [Test]
        public void GivenAStructuresListExporter_WhenCategoryIsCalled_ThenGeneralIsReturned()
        {
            Assert.That(exporter.Category, Is.EqualTo("General"));
        }

        [Test]
        public void GivenAStructuresListExporter_WhenExportIsCalledWithAnyItemAndANullPath_ThenAnErrorIsLoggedAndFalseIsReturned()
        {
            Assert.That(exporter.Export(Arg<object>.Is.Anything, null), Is.False);

            const string expectedLogMessage = "No file was presented to import from.";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(Arg<object>.Is.Anything, null), expectedLogMessage);
        }

        [Test]
        public void GivenAStructuresListExporterAndAValidPath_WhenExportIsCalledWithANullItem_ThenAnErrorIsLoggedAndFalseIsReturned()
        {
            var path = "";
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (File.Exists(path));

            Assert.That(exporter.Export(null, path), Is.False);

            const string expectedLogMessage = "No target was presented to import to (requires a Flexible Mesh Water Flow model or Area.";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(null, path), expectedLogMessage);
        }

        [Test]
        public void GivenAStructuresListExporterAValidItemAndAValidPath_WhenExportIsCalled_ThenAMessageIsLoggedAndTrueIsReturned()
        {
            var path = "";
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (File.Exists(path));

            var item = new List<IStructure>();

            var fmModel = new WaterFlowFMModel();
            var exporter = new StructuresListExporter(StructuresListType.Gates) {GetModelForList = input => fmModel};

            try
            {
                Assert.That(exporter.Export(item, path), Is.True);

                const string expectedMessage = "Written 0 Gates to structures file.";
                TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(item, path), expectedMessage);
            }
            finally
            {
                // Clean up garbage because of file writing
                FileUtils.DeleteIfExists(path);
            }
        }

        [Test]
        public void GivenAStructuresListExporterAndAnExceptionToOccur_WhenExportIsCalled_ThenAnErrorIsLoggedAndFalseIsReturned()
        {
            // This would be significantly easier if structuresFile was properly stored as a dependency and therefore mockable
            var drive = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            // assumption there exists a letter for which there exists no drive - construct drive that not exists in order to prove an exception
            var path = "";
            for (var i = 0; i < drive.Length; i++)
            {
                path = $"{drive[i]}:/";
                if (!Directory.Exists(path))
                {
                    break;
                }
            }

            var item = new List<IStructure>();

            var fmModel = new WaterFlowFMModel();
            var exporter = new StructuresListExporter(StructuresListType.Gates) {GetModelForList = input => fmModel};

            Assert.That(exporter.Export(item, path), Is.False);

            const string expectedMessage = "An error occurred while exporting structures, export stopped";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(item, path), expectedMessage);
        }

        [Test]
        public void GivenAStructuresListExporter_WhenFileFilterIsCalled_ThenTheCorrectFileFilterIsReturned()
        {
            const string expectedValue = "Structures file|*.ini";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GivenAStructuresListExporter_WhenCanExportIsCalledWithAnyObject_ThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }

        [TestCase(StructuresListType.Pumps, "Pumps to structures file")]
        [TestCase(StructuresListType.Weirs, "Weirs to structures file")]
        [TestCase(StructuresListType.Gates, "Gates to structures file")]
        public void GivenAStructuresListExporterWithAType_WhenNameIsCalled_ThenTheNameOfStructureTypeIsGiven(StructuresListType t, string expectedName)
        {
            var exporter = new StructuresListExporter(t);
            Assert.That(exporter.Name, Is.EqualTo(expectedName));
        }

        [TestCase(StructuresListType.Pumps, typeof(IList<IPump>), typeof(IEventedList<IPump>))]
        [TestCase(StructuresListType.Weirs, typeof(IList<IStructure>), typeof(IEventedList<IStructure>))]
        public void
            GivenStructuresListExporterOfTheSpecifiedTypeWhenSourceTypesIsCalledThenTheCorrectSourceTypesAreReturned(
                StructuresListType t, Type listClassType, Type eventedListClassType)
        {
            var exporter = new StructuresListExporter(t);

            Assert.That(exporter.SourceTypes().Count(), Is.EqualTo(2));
            Assert.That(exporter.SourceTypes().Contains(listClassType));
            Assert.That(exporter.SourceTypes().Contains(eventedListClassType));
        }
    }
}