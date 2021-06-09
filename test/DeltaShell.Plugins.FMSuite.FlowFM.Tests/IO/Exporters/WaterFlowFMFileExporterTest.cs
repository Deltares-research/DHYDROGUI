using System;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class WaterFlowFMFileExporterTest
    {
        private WaterFlowFMFileExporter exporter;

        [SetUp]
        public void Setup()
        {
            exporter = new WaterFlowFMFileExporter();
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporterWhenNameIsCalledThenTheCorrectNameIsReturned()
        {
            const string expectedVal = "Flow Flexible Mesh model";
            Assert.That(exporter.Name, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporterWhenCategoryIsCalledThenGeneralIsReturned()
        {
            const string expectedVal = "General";
            Assert.That(exporter.Category, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporterWhenExportIsCalledWithANullItemAndAnyPathThenAnExceptionIsThrown()
        {
            var error = Assert.Throws<Exception>(() => exporter.Export(null, Arg<string>.Is.Anything));
            Assert.AreEqual("Item not set", error.Message);
            ;
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporterAndANotWaterFlowFMModelItemWhenExportIsCalledWithThisItemAndAnyPathThenAnExceptionIsThrown()
        {
            var error = Assert.Throws<Exception>(() => exporter.Export(new object(), Arg<string>.Is.Anything));
            Assert.AreEqual("Unexpected object type: System.Object", error.Message);
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporterWhenSourceTypesIsCalledThenWaterFlowFMModelIsReturned()
        {
            Assert.That(exporter.SourceTypes().Count(), Is.EqualTo(1));
            Assert.That(exporter.SourceTypes().Contains(typeof(WaterFlowFMModel)));
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporterWhenCanExportForIsForAnyObjectCalledThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporterWhenFileFilterIsCalledThenTheCorrectFileFilterIsReturned()
        {
            const string expectedVal = "Flexible Mesh Model Definition|*.mdu";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedVal));
        }
    }
}
