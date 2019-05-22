using System;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
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
        public void GivenAnWaterFlowFMFileExporter_WhenNameIsCalled_ThenTheCorrectNameIsReturned()
        {
            const string expectedVal = "Flow Flexible Mesh model";
            Assert.That(exporter.Name, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporter_WhenCategoryIsCalled_ThenGeneralIsReturned()
        {
            const string expectedVal = "General";
            Assert.That(exporter.Category, Is.EqualTo(expectedVal));
        }

        [Test]
        [ExpectedException(typeof(Exception),ExpectedMessage = "Item not set")]
        public void GivenAnWaterFlowFMFileExporter_WhenExportIsCalledWithANullItem_ThenAnExceptionIsThrown()
        {
            exporter.Export(null, Arg<string>.Is.Anything);
        }

        [Test]
        [ExpectedException(typeof(Exception), ExpectedMessage = "Unexpected object type: System.Object")]
        public void GivenAnWaterFlowFMFileExporterAndANotWaterFlowFMModelItem_WhenExportIsCalled_ThenAnExceptionIsThrown()
        {
            exporter.Export(new object(), Arg<string>.Is.Anything);
        }

        [Test]
        public void GivenAWaterFlowFMFileExporterAndAWaterFlowFMModelItemAndAValidPath_WhenExportIsCalled_ThenTheModelIsExportedToTheSpecifiedPath()
        {
            var path = Path.Combine(Path.GetTempPath(), "FlowFM.mdu");

            var mocks = new MockRepository();

            //delegate bool mockFunc(string p, bool b0, bool b1, bool b2) = (p, b0, b1, b2) => { return true; };
            Func<string, bool, bool, bool, bool> emptyFunc = (p, b0, b1, b2) => true;

            var model = mocks.PartialMock<WaterFlowFMModel>();
            model
                .Expect(n => n.ExportTo(Arg<string>.Is.Equal(path),
                                        Arg<bool>.Is.Equal(false), 
                                        Arg<bool>.Is.Equal(true), 
                                        Arg<bool>.Is.Equal(true)))
                .Do(emptyFunc)
                .Return(true)
                .Repeat.Once();
            model.Expect(n => n.ToString()).Return("").Repeat.Any();

            mocks.ReplayAll();

            Assert.That(exporter.Export(model, path), Is.True);

            mocks.VerifyAll();
        }

        [Test]
        public void GivenANWaterFlowFMFileExporterAndAWaterFlowFMModelItemAndAPathPointingToADirectory_WhenExportIsCalled_ThenTheModelIsExportedToAPathWithTheModelNameAndMDUExtension()
        {
            var path = Path.GetTempPath();
            var expectedPath = Path.Combine(path, "FlowFM.mdu");

            var mocks = new MockRepository();

            //delegate bool mockFunc(string p, bool b0, bool b1, bool b2) = (p, b0, b1, b2) => { return true; };
            Func<string, bool, bool, bool, bool> emptyFunc = (p, b0, b1, b2) => true;

            var model = mocks.PartialMock<WaterFlowFMModel>();
            model
                .Expect(n => n.ExportTo(Arg<string>.Is.Equal(expectedPath),
                                        Arg<bool>.Is.Equal(false), 
                                        Arg<bool>.Is.Equal(true), 
                                        Arg<bool>.Is.Equal(true)))
                .Do(emptyFunc)
                .Return(true)
                .Repeat.Once();
            model.Expect(n => n.ToString()).Return("").Repeat.Any();

            mocks.ReplayAll();

            Assert.That(exporter.Export(model, path), Is.True);

            mocks.VerifyAll();
            

        }

        [Test]
        public void GivenAnWaterFlowFMFileExporter_WhenSourceTypesIsCalled_ThenWaterFlowFMModelIsReturned()
        {
            Assert.That(exporter.SourceTypes().Count(), Is.EqualTo(1));
            Assert.That(exporter.SourceTypes().Contains(typeof(WaterFlowFMModel)));
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporter_WhenCanExportForIsForAnyObjectCalled_ThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }

        [Test]
        public void GivenAnWaterFlowFMFileExporter_WhenFileFilterIsCalled_ThenTheCorrectFileFilterIsReturned()
        {
            const string expectedVal = "Flexible Mesh Model Definition|*.mdu";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedVal));
        }
    }
}
