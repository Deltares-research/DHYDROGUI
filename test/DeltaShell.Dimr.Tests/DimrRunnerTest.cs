using System.Collections.Generic;
using System.IO;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Services;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrRunnerTest
    {
        private IDimrApi dimrApi;
        private IDimrModel dimrModel;
        private IDimrApiFactory dimrApiFactory;
        private IFileExportService fileExportService;

        [SetUp]
        public void SetUp()
        {
            dimrApi = Substitute.For<IDimrApi>();
            dimrModel = Substitute.For<IDimrModel>();
            dimrApiFactory = Substitute.For<IDimrApiFactory>();
            fileExportService = Substitute.For<IFileExportService>();
            
            dimrApiFactory.CreateNew(Arg.Any<bool>()).Returns(dimrApi);
        }

        [Test]
        public void Constructor_ModelIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new DimrRunner(null, dimrApiFactory, fileExportService), Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_DimrApiFactoryIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new DimrRunner(dimrModel, null, fileExportService), Throws.ArgumentNullException);
        }
        
        [Test]
        public void Constructor_FileExportServiceIsNull_ThrowsArgumentNullException()
        {
            Assert.That(() => new DimrRunner(dimrModel, dimrApiFactory, null), Throws.ArgumentNullException);
        }

        [Test]
        public void FileExportService_SetToNull_ThrowsArgumentNullException()
        {
            DimrRunner dimrRunner = CreateDimrRunner();
            
            Assert.That(() => dimrRunner.FileExportService = null, Throws.ArgumentNullException);
        }
        
        [Test]
        public void GivenDimrRunnerWithNoFileExporterFound_WhenOnInitializeCalled_ThenInvalidOperationExceptionThrown()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                SetupDimrExportDir(tempDir);

                DimrRunner dimrRunner = CreateDimrRunner();
                
                Assert.That(() => dimrRunner.OnInitialize(), Throws.InvalidOperationException);
            }
        }
        
        [Test]
        public void GivenDimrRunnerThatFailsUpdate_WhenOnExecuteCalled_ThenDimrErrorCodeExceptionThrown()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                dimrApi.Update(Arg.Any<double>()).Returns(-1);

                SetupDimrExportDir(tempDir);
                SetupFileExportService(CreateFileExporter());
                
                DimrRunner dimrRunner = CreateDimrRunner();
                dimrRunner.OnInitialize();
                
                Assert.That(() => dimrRunner.OnExecute(), Throws.InstanceOf<DimrErrorCodeException>());
            }
        }

        [Test]
        public void GivenDimrRunnerThatFailsInitialize_WhenOnInitializeCalled_ThenDimrErrorCodeExceptionThrown()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                dimrApi.Initialize(Arg.Any<string>()).Returns(-1);

                SetupDimrExportDir(tempDir);
                SetupFileExportService(CreateFileExporter());
                
                DimrRunner dimrRunner = CreateDimrRunner();

                Assert.That(() => dimrRunner.OnInitialize(), Throws.InstanceOf<DimrErrorCodeException>());
            }
        }

        [Test]
        public void GivenDimrRunnerThatFailsFinish_WhenOnFinishCalled_ThenDimrErrorCodeExceptionThrown()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                dimrApi.Finish().Returns(-1);

                SetupDimrExportDir(tempDir);
                SetupFileExportService(CreateFileExporter());
                
                DimrRunner dimrRunner = CreateDimrRunner();
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
                dimrApi.Finish().Returns(-1);

                SetupDimrExportDir(tempDir);
                SetupFileExportService(CreateFileExporter());

                string exceptionFilePath = Path.Combine(tempDir.Path, "test.txt");
                File.WriteAllText(exceptionFilePath, "test");
                
                string shouldBeRemovedFilePath = Path.Combine(tempDir.Path, "test2.txt");
                File.WriteAllText(shouldBeRemovedFilePath, "test");
                
                dimrModel.IgnoredFilePathsWhenCleaningWorkingDirectory.Returns(new HashSet<string> {exceptionFilePath});
                dimrModel.Validate().Returns(new ValidationReport("", new List<ValidationIssue>()));
                
                DimrRunner dimrRunner = CreateDimrRunner();

                // Act
                dimrRunner.OnInitialize();

                // Assert
                Assert.IsFalse(File.Exists(shouldBeRemovedFilePath));
                Assert.IsTrue(File.Exists(exceptionFilePath));
            }
        }

        private DimrRunner CreateDimrRunner()
        {
            return new DimrRunner(dimrModel, dimrApiFactory, fileExportService);
        }
        
        private IDimrModelFileExporter CreateFileExporter()
        {
            var fileExporter = Substitute.For<IDimrModelFileExporter>();
            fileExporter.Export(Arg.Any<object>(), Arg.Any<string>()).Returns(true);
            return fileExporter;
        }
        
        private void SetupDimrExportDir(TemporaryDirectory tempDir)
        {
            dimrModel.DimrExportDirectoryPath.Returns(tempDir.Path);
        }

        private void SetupFileExportService(IFileExporter fileExporter)
        {
            fileExportService.GetFileExportersFor(Arg.Any<IDimrModel>()).Returns(new[] { fileExporter });
        }
    }
}