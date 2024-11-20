using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
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
            this.exporter = new StructuresListExporter(StructuresListType.Gates);
        }

        [TestCase(StructuresListType.Pumps, "Pumps to structures file")]
        [TestCase(StructuresListType.Weirs, "Weirs to structures file")]
        [TestCase(StructuresListType.Gates, "Gates to structures file")]
        public void GivenAStructuresListExporterWithATypeWhenNameIsCalledThenTheNameOfStructureTypeIsGiven(StructuresListType t, string expectedName)
        {
            var exporter = new StructuresListExporter(t);
            Assert.That(exporter.Name, Is.EqualTo(expectedName));
        }

        [Test]
        public void GivenAStructuresListExporterWhenCategoryIsCalledThenGeneralIsReturned()
        {
            Assert.That(exporter.Category, Is.EqualTo("General"));
        }


        [TestCase(StructuresListType.Pumps, typeof(IList<IPump>), typeof(IEventedList<IPump>))]
        [TestCase(StructuresListType.Weirs, typeof(IList<IWeir>), typeof(IEventedList<IWeir>))]
        [TestCase(StructuresListType.Gates, typeof(IList<IGate>), typeof(IEventedList<IGate>))]
        public void
            GivenStructuresListExporterOfTheSpecifiedTypeWhenSourceTypesIsCalledThenTheCorrectSourceTypesAreReturned(
                StructuresListType t, System.Type listClassType, System.Type eventedListClassType)
        {
            var exporter = new StructuresListExporter(t);

            Assert.That(exporter.SourceTypes().Count(), Is.EqualTo(2));
            Assert.That(exporter.SourceTypes().Contains(listClassType));
            Assert.That(exporter.SourceTypes().Contains(eventedListClassType));
        }

        [Test]
        public void GivenAStructuresListExporterWhenExportIsCalledWithAnyItemAndANullPathThenAnErrorIsLoggedAndFalseIsReturned()
        {
            Assert.That(exporter.Export(Arg<object>.Is.Anything, null), Is.False);

            const string expectedLogMessage = "No file was presented to import from.";
            TestHelper.AssertAtLeastOneLogMessagesContains(()=> exporter.Export(Arg<object>.Is.Anything, null), expectedLogMessage);
        }

        [Test]
        public void GivenAStructuresListExporterAndAValidPathWhenExportIsCalledWithANullItemAndThisPathThenAnErrorIsLoggedAndFalseIsReturned()
        {
            var path = "";
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (File.Exists(path));
            
            Assert.That(exporter.Export(null, path), Is.False);

            const string expectedLogMessage = "No target was presented to import to (requires a Flexible Mesh Water Flow model or Area.";
            TestHelper.AssertAtLeastOneLogMessagesContains(()=> exporter.Export(null, path), expectedLogMessage);
        }

        [Test]
        public void GivenAStructuresListExporterAValidItemAndAValidPathWhenExportIsCalledWithThisItemAndThisPathThenAMessageIsLoggedAndTrueIsReturned()
        {
            var path = "";
            do
            {
                path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            } while (File.Exists(path));

            var item = new List<IStructure1D>();

            var fmModel = new WaterFlowFMModel();
            var exporter = new StructuresListExporter(StructuresListType.Gates)
            {
                GetModelForList = input => fmModel
            };

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
        public void GivenAStructuresListExporterAndAnExceptionToOccurWhenExportIsCalledWithThisItemThenAnErrorIsLoggedAndFalseIsReturned()
        {
            // This would be significantly easier if structuresFile was properly stored as a dependency and therefore mockable
            var drive = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            // assumption there exists a letter for which there exists no drive - construct drive that not exists in order to prove an exception
            var path = "";
            for (int i = 0; i < drive.Length; i++)
            {
                path = $"{drive[i]}:/";
                if (!Directory.Exists(path))
                    break;
            }

            var item = new List<IStructure1D>();

            var fmModel = new WaterFlowFMModel();
            var exporter = new StructuresListExporter(StructuresListType.Gates)
            {
                GetModelForList = input => fmModel
            };

            Assert.That(exporter.Export(item, path), Is.False);

            const string expectedMessage = "An error occurred while exporting structures, export stopped";
            TestHelper.AssertAtLeastOneLogMessagesContains(() => exporter.Export(item, path), expectedMessage);
        }


        [Test]
        public void GivenAStructuresListExporterWhenFileFilterIsCalledThenTheCorrectFileFilterIsReturned()
        {
            const string expectedValue = "Structures file|*.ini";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedValue));
        }


        [Test]
        public void GivenAStructuresListExporterWhenCanExportIsCalledWithAnyObjectThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }

    }
}
