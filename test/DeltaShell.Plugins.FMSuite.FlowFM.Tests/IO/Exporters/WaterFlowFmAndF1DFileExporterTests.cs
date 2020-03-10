using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.Exporters.Tests
{
    [TestFixture]
    public class WaterFlowFmAndF1DFileExporterTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Item to export Flow Flexible Mesh model with 1D Network is not set")]
        public void GivenAnythingButWaterFlowFmModelToExportWhenExportThanExpectExceptionTest()
        {
            var exporter = new WaterFlowFmAndF1DFileExporter();
            exporter.Export(null, string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot export to unknown location, path is null or empty")]
        public void GivenAnythingButWaterFlowFmModelOrNullAndPathIsNullToExportWhenExportThanExpectExceptionTest()
        {
            var exporter = new WaterFlowFmAndF1DFileExporter();
            exporter.Export(new object(), null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Cannot export to unknown location, path is null or empty")]
        public void GivenAnythingButWaterFlowFmModelOrNullAndPathIsNotSetToExportWhenExportThanExpectExceptionTest()
        {
            var exporter = new WaterFlowFmAndF1DFileExporter();
            exporter.Export(new object(), string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Unexpected object type: ", MatchType = MessageMatch.Contains)]
        public void GivenAnythingButWaterFlowFmModelOrNullAndPathIsSetToExportWhenExportThanExpectExceptionTest()
        {
            var exporter = new WaterFlowFmAndF1DFileExporter();
            exporter.Export(new object(), ".");
        }

        [Test]
        public void GivenEmptyWaterFlowFmModelToExportWhenExportThanExpectExportToSuccesful()
        {
            var exporter = new WaterFlowFmAndF1DFileExporter();
            Assert.That(exporter.Export(new WaterFlowFMModel(), "."), Is.True);//exporting nothing should result to true?
        }
        [Test]
        public void SourceTypesTest()
        {
            var exporter = new WaterFlowFmAndF1DFileExporter();
            Assert.That(exporter.SourceTypes().ToList(), Contains.Item(typeof(WaterFlowFMModel)));
        }

        [Test]
        public void CanExportForTest()
        {
            var exporter = new WaterFlowFmAndF1DFileExporter();
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }
    }
}