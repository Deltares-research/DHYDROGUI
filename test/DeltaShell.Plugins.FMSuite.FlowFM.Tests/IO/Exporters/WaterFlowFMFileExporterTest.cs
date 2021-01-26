using System;
using System.IO;
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
        public void GivenAWaterFlowFMFileExporterAndAWaterFlowFMModelItemAndAValidPathWhenExportIsCalledWithThisItemAndPathThenTheModelIsExportedToTheSpecifiedPath()
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
        public void GivenANWaterFlowFMFileExporterAndAWaterFlowFMModelItemAndAPathPointingToADirectoryWhenExportIsCalledWithThisItemAndPathThenTheModelIsExportedToAPathWithTheModelNameAndMDUExtension()
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
