using System;
using System.IO;
using System.Linq;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.ImportExport.Exporters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Exporters
{
    [TestFixture]
    public class FMModelFileExporterTest
    {
        private FMModelFileExporter exporter;

        [SetUp]
        public void Setup()
        {
            exporter = new FMModelFileExporter();
        }

        [Test]
        public void GivenAWaterFlowFMFileExporter_WhenNameIsCalled_ThenTheCorrectNameIsReturned()
        {
            const string expectedVal = "Flow Flexible Mesh model";
            Assert.That(exporter.Name, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAWaterFlowFMFileExporter_WhenCategoryIsCalled_ThenGeneralIsReturned()
        {
            const string expectedVal = "General";
            Assert.That(exporter.Category, Is.EqualTo(expectedVal));
        }

        [Test]
        public void GivenAWaterFlowFMFileExporter_WhenExportIsCalledWithANullItem_ThenAnExceptionIsThrown()
        {
            Assert.That(() => exporter.Export(null, Arg<string>.Is.Anything), Throws.Exception.With.Message.EqualTo("Value cannot be null.\r\nParameter name: item"));
        }

        [Test]
        public void GivenAWaterFlowFMFileExporterAndANotWaterFlowFMModelItem_WhenExportIsCalled_ThenAnExceptionIsThrown()
        {
            Assert.That(() => exporter.Export(new object(), Arg<string>.Is.Anything), Throws.Exception.With.Message.EqualTo("Unexpected object type: System.Object"));
        }

        [Test]
        public void GivenAWaterFlowFMFileExporterAndPathAndExportDirectoryIsNull_WhenExportIsCalled_ThenAnExceptionIsThrown()
        {
            Assert.That(() => exporter.Export(new WaterFlowFMModel(), null), Throws.Exception.With.Message.EqualTo("No export path or directory specified."));
        }

        [Test]
        public void GivenAWaterFlowFMFileExporterAndAWaterFlowFMModelItemAndAValidPath_WhenExportIsCalled_ThenTheModelIsExportedToTheSpecifiedPath()
        {
            string path = Path.Combine(Path.GetTempPath(), "FlowFM.mdu");

            var mocks = new MockRepository();
            var model = SetupModelMockForExport(mocks, path);

            mocks.ReplayAll();

            Assert.That(exporter.Export(model, path), Is.True);

            mocks.VerifyAll();
        }

        [Test]
        public void GivenAWaterFlowFMFileExporterAndAWaterFlowFMModelItemAndAPathPointingToADirectory_WhenExportIsCalled_ThenTheModelIsExportedToAPathWithTheModelNameAndMDUExtension()
        {
            string path = Path.GetTempPath();
            string expectedPath = Path.Combine(path, "FlowFM.mdu");

            var mocks = new MockRepository();
            var model = SetupModelMockForExport(mocks, expectedPath);
            
            mocks.ReplayAll();

            Assert.That(exporter.Export(model, path), Is.True);

            mocks.VerifyAll();
        }
        
        [Test]
        public void GivenAWaterFlowFMFileExporterAndAWaterFlowFMModelItemAndAPathIsNullAndExportDirectoryIsSet_WhenExportIsCalled_ThenTheModelIsExportedToAPathWithTheModelNameAndMDUExtension()
        {
            string path = Path.GetTempPath();
            string expectedPath = Path.Combine(path, "FlowFM.mdu");

            var mocks = new MockRepository();
            var model = SetupModelMockForExport(mocks, expectedPath);

            exporter.ExportDirectory = path;
            
            mocks.ReplayAll();

            Assert.That(exporter.Export(model, null), Is.True);

            mocks.VerifyAll();
        }

        private static WaterFlowFMModel SetupModelMockForExport(MockRepository mocks, string expectedPath)
        {
            //delegate bool mockFunc(string p, bool b0, bool b1, bool b2) = (p, b0, b1, b2) => { return true; };
            Func<string, bool, bool, bool, bool> emptyFunc = (p, b0, b1, b2) => true;

            var model = mocks.PartialMock<WaterFlowFMModel>();
            
            model.Expect(n => n.ExportTo(Arg<string>.Is.Equal(expectedPath),
                                         Arg<bool>.Is.Equal(false),
                                         Arg<bool>.Is.Equal(true),
                                         Arg<bool>.Is.Equal(true)))
                 .Do(emptyFunc)
                 .Return(true)
                 .Repeat.Once();
            
            model.Expect(n => n.ToString())
                 .Return("")
                 .Repeat.Any();

            return model;
        }

        [Test]
        public void GivenAWaterFlowFMFileExporter_WhenSourceTypesIsCalled_ThenWaterFlowFMModelIsReturned()
        {
            Assert.That(exporter.SourceTypes().Count(), Is.EqualTo(1));
            Assert.That(exporter.SourceTypes().Contains(typeof(WaterFlowFMModel)));
        }

        [Test]
        public void GivenAWaterFlowFMFileExporter_WhenCanExportForIsForAnyObjectCalled_ThenTrueIsReturned()
        {
            Assert.That(exporter.CanExportFor(Arg<object>.Is.Anything), Is.True);
        }

        [Test]
        public void GivenAWaterFlowFMFileExporter_WhenFileFilterIsCalled_ThenTheCorrectFileFilterIsReturned()
        {
            const string expectedVal = "Flexible Mesh Model Definition|*.mdu";
            Assert.That(exporter.FileFilter, Is.EqualTo(expectedVal));
        }
    }
}