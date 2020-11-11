using System;
using System.Collections.Generic;
using System.Drawing;
using DelftTools.Shell.Core;
using DeltaShell.NGHS.IO.TestUtils;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrRunnerTest
    {
        private static readonly IDimrApiFactory dimrApiFactory = Substitute.For<IDimrApiFactory>();

        [Test]
        public void GivenDimrRunnerThatFailsUpdate_WhenOnExecuteCalled_ThenDimrErrorCodeExceptionThrown()
        {
            var dimrApi = Substitute.For<IDimrApi>();
            dimrApi.Update(Arg.Any<double>()).Returns(-1);
            dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);

            var dimrRunner = new DimrRunner(Substitute.For<IDimrModel>(), dimrApiFactory);

            Assert.That(() => dimrRunner.OnExecute(), Throws.InstanceOf<DimrErrorCodeException>());
        }

        [Test]
        public void GivenDimrRunnerThatFailsInitialize_WhenOnInitializeCalled_ThenDimrErrorCodeExceptionThrown()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                var dimrApi = Substitute.For<IDimrApi>();
                dimrApi.Initialize(Arg.Any<string>()).Returns(-1);
                dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);

                var dimrModel = Substitute.For<IDimrModel>();
                dimrModel.ExporterType.Returns(typeof(TestExporter));
                dimrModel.DimrExportDirectoryPath.Returns(tempDir.Path);
                var dimrRunner = new DimrRunner(dimrModel, dimrApiFactory);

                Assert.That(() => dimrRunner.OnInitialize(), Throws.InstanceOf<DimrErrorCodeException>());
            }
        }

        [Test]
        public void GivenDimrRunnerThatFailsFinish_WhenOnFinishCalled_ThenDimrErrorCodeExceptionThrown()
        {
            var dimrApi = Substitute.For<IDimrApi>();
            dimrApi.Finish().Returns(-1);
            dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);

            var dimrRunner = new DimrRunner(Substitute.For<IDimrModel>(), dimrApiFactory);

            Assert.That(() => dimrRunner.OnFinish(), Throws.InstanceOf<DimrErrorCodeException>());
        }

        private class TestExporter : IFileExporter
        {
            public string Name { get; }
            public string Category { get; }
            public string Description { get; }

            public string FileFilter { get; }
            public Bitmap Icon { get; }

            public bool Export(object item, string path)
            {
                return true;
            }

            public IEnumerable<Type> SourceTypes()
            {
                throw new InvalidOperationException();
            }

            public bool CanExportFor(object item)
            {
                throw new InvalidOperationException();
            }
        }
    }
}