using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
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
            using (var tempDir = new TemporaryDirectory())
            {
                var dimrApi = Substitute.For<IDimrApi>();
                dimrApi.Update(Arg.Any<double>()).Returns(-1);
                dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);

                IDimrModel dimrModel = CreateDimrModel(tempDir);
                var dimrRunner = new DimrRunner(dimrModel, dimrApiFactory);
                
                dimrRunner.OnInitialize();
                Assert.That(() => dimrRunner.OnExecute(), Throws.InstanceOf<DimrErrorCodeException>());
            }
        }

        [Test]
        public void GivenDimrRunnerThatFailsInitialize_WhenOnInitializeCalled_ThenDimrErrorCodeExceptionThrown()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                var dimrApi = Substitute.For<IDimrApi>();
                dimrApi.Initialize(Arg.Any<string>()).Returns(-1);
                dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);

                IDimrModel dimrModel = CreateDimrModel(tempDir);
                var dimrRunner = new DimrRunner(dimrModel, dimrApiFactory);

                Assert.That(() => dimrRunner.OnInitialize(), Throws.InstanceOf<DimrErrorCodeException>());
            }
        }

        [Test]
        public void GivenDimrRunnerThatFailsFinish_WhenOnFinishCalled_ThenDimrErrorCodeExceptionThrown()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                var dimrApi = Substitute.For<IDimrApi>();
                dimrApi.Finish().Returns(-1);
                dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);

                IDimrModel dimrModel = CreateDimrModel(tempDir);
                var dimrRunner = new DimrRunner(dimrModel, dimrApiFactory);

                dimrRunner.OnInitialize();
                Assert.That(() => dimrRunner.OnFinish(), Throws.InstanceOf<DimrErrorCodeException>());
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void OnInitialize_WhenThereIsAFileExceptionForCleaningTheWorkingDirectory_ShouldNotRemoveThisException()
        {
            // Arrange
            using (var tempDir = new TemporaryDirectory())
            {
                var dimrApi = Substitute.For<IDimrApi>();
                dimrApi.Finish().Returns(-1);
                dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);

                IDimrModel dimrModel = CreateDimrModel(tempDir);
                string exceptionFilePath = Path.Combine(tempDir.Path, "test.txt");
                File.WriteAllText(exceptionFilePath, "test");
                string shouldBeRemovedFilePath = Path.Combine(tempDir.Path, "test2.txt");
                File.WriteAllText(shouldBeRemovedFilePath, "test");
                dimrModel.IgnoredFilePathsWhenCleaningWorkingDirectory.Returns(new HashSet<string> {exceptionFilePath});
                dimrModel.Validate().Returns(new ValidationReport("", new List<ValidationIssue>()));
                var dimrRunner = new DimrRunner(dimrModel, dimrApiFactory);

                // Act
                dimrRunner.OnInitialize();

                // Assert
                Assert.IsFalse(File.Exists(shouldBeRemovedFilePath));
                Assert.IsTrue(File.Exists(exceptionFilePath));
            }
        }
        
        private static IDimrModel CreateDimrModel(TemporaryDirectory tempDir)
        {
            var dimrModel = Substitute.For<IDimrModel>();
            dimrModel.ExporterType.Returns(typeof(TestExporter));
            dimrModel.DimrExportDirectoryPath.Returns(tempDir.Path);
            return dimrModel;
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