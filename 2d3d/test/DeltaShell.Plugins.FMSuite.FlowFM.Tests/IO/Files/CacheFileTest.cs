using System;
using System.IO;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Files.Helpers.CopyHandlers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO.Files
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class CacheFileTest
    {
        private TemporaryDirectory temporaryDirectory;
        private WaterFlowFMModel model;

        [SetUp]
        public void ContinuousSetUp()
        {
            model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value = true;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            temporaryDirectory = new TemporaryDirectory();

            model = new WaterFlowFMModel();

            string mduFilePath = Path.Combine(temporaryDirectory.Path, "CacheMeOutside.cache");
            model.ExportTo(mduFilePath, true, false, false);
            GenerateDummyCacheFile(mduFilePath);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            ((IDisposable) temporaryDirectory).Dispose();
            model.Dispose();
        }

        [Test]
        public void Constructor_ModelNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new CacheFile(null, Substitute.For<ICopyHandler>());

            // Assert
            var exception =
                Assert.Throws<ArgumentNullException>(Call, "Expected an ArgumentNullException to be thrown:");
            Assert.That(exception.ParamName, Is.EqualTo("model"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void Constructor_CopyHandlerNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new CacheFile(model, null);

            // Assert
            var exception =
                Assert.Throws<ArgumentNullException>(Call, "Expected an ArgumentNullException to be thrown:");
            Assert.That(exception.ParamName, Is.EqualTo("copyHandler"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void Export_ExportMduPathNull_ThrowsArgumentNullException()
        {
            // Setup
            var cacheFile = new CacheFile(model, Substitute.For<ICopyHandler>());

            // Call
            void Call() => cacheFile.Export(null);

            // Assert
            var exception =
                Assert.Throws<ArgumentNullException>(Call, "Expected an ArgumentNullException to be thrown:");
            Assert.That(exception.ParamName, Is.EqualTo("exportMduPath"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void GivenACacheFileWithUsingCachingTurnedOff_WhenExportIsCalled_ThenNothingIsExported()
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            var cacheFile = new CacheFile(model, copyHandler);

            model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value = false;

            // When 
            cacheFile.Export("some/Export/Path.mdu");

            // Then
            copyHandler.DidNotReceiveWithAnyArgs().Copy(null, null);
        }

        [Test]
        public void GivenACacheFileWithoutAFile_WhenExportIsCalled_ThenNothingIsExported()
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            var cacheFile = new CacheFile(model, copyHandler);

            model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value = true;
            cacheFile.UpdatePathToMduLocation(Path.Combine(temporaryDirectory.Path, "nonExistent.mdu"));

            // When 
            cacheFile.Export("some/Export/Path.mdu");

            // Then
            copyHandler.DidNotReceiveWithAnyArgs().Copy(null, null);
        }

        [Test]
        public void GivenACacheFileAndAnExportPathEqualToTheCurrentPath_WhenExportIsCalled_ThenNothingIsExported()
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            var cacheFile = new CacheFile(model, copyHandler);

            string mduPath = Path.ChangeExtension(cacheFile.Path, FileConstants.CachingFileExtension);

            // When
            cacheFile.Export(mduPath);

            // Then
            copyHandler.DidNotReceiveWithAnyArgs().Copy(null, null);
        }

        [Test]
        public void GivenACacheFileAndAValidPath_WhenExportIsCalled_ThenTheFileIsExported()
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            var cacheFile = new CacheFile(model, copyHandler);

            string expectedTargetPath = Path.Combine(temporaryDirectory.Path, FileConstants.CachingFileExtension);
            string mduPath = Path.ChangeExtension(expectedTargetPath, FileConstants.CachingFileExtension);

            // When
            cacheFile.Export(mduPath);

            // Then
            copyHandler.Received(1).Copy(
                Arg.Is<string>(srcPath => IsSamePath(srcPath, cacheFile.Path)),
                Arg.Is<string>(targetPath => IsSamePath(targetPath, expectedTargetPath)));
        }

        [Test]
        public void GivenACacheFile_WhenExportIsCalledAndAnErrorIsThrown_ThenAnExceptionIsLogged()
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            var cacheFile = new CacheFile(model, copyHandler);

            string expectedTargetPath = Path.Combine(temporaryDirectory.Path, FileConstants.CachingFileExtension);
            string mduPath = Path.ChangeExtension(expectedTargetPath, FileConstants.CachingFileExtension);

            copyHandler.WhenForAnyArgs(x => x.Copy(null, null))
                       .Do(_ =>
                               throw new FileCopyException("message", new PathTooLongException()));

            var logHandler = Substitute.For<ILogHandler>();

            // When
            cacheFile.Export(mduPath, logHandler);

            // Then
            logHandler.Received(1)
                      .ReportWarningFormat(Arg.Is<string>(x => string.Equals(x, Resources.CacheFile_CopyInternally_Could_not_copy__0__to__1__due_to___2_)),
                                           Arg.Is<string>(srcPath => IsSamePath(srcPath, cacheFile.Path)),
                                           Arg.Is<string>(targetPath => IsSamePath(targetPath, expectedTargetPath)),
                                           Arg.Is<string>(s => string.Equals(s, "message")));
            logHandler.Received(1).LogReport();
        }

        [Test]
        [TestCase(null, null)]
        [TestCase("somePath.mdu", "somePath.cache")]
        public void UpdatePathToMduLocation_SetsCorrectPath(string newMduFilePath,
                                                            string expectedPath)
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            var cacheFile = new CacheFile(model, copyHandler);

            model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value = false;

            // When 
            cacheFile.UpdatePathToMduLocation(newMduFilePath);

            // Then
            Assert.That(cacheFile.Path, Is.EqualTo(expectedPath));
        }

        [Test]
        public void GivenACacheFileWithFile_WhenExistsIsCalled_ThenTrueIsReturned()
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            // Default cacheFile of this test has a file set.
            var cacheFile = new CacheFile(model, copyHandler);

            // When | Then
            Assert.That(cacheFile.Exists, Is.True);
        }

        [Test]
        public void GivenACacheFileWithoutAFile_WhenExistsIsCalled_ThenFalseIsReturned()
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            // Default cacheFile of this test has a file set.
            var cacheFile = new CacheFile(model, copyHandler);
            cacheFile.UpdatePathToMduLocation(Path.Combine(temporaryDirectory.Path, "someOther.mdu"));

            // When | Then
            Assert.That(cacheFile.Exists, Is.False);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GivenAModelWithAnUseCachingValue_WhenUseCachingIsRetrieved_ThenTheCorrectValueIsReturned(bool expectedValue)
        {
            // Given
            var copyHandler = Substitute.For<ICopyHandler>();
            var cacheFile = new CacheFile(model, copyHandler);

            model.ModelDefinition.GetModelProperty(KnownProperties.UseCaching).Value = expectedValue;

            // When | Then
            Assert.That(cacheFile.UseCaching, Is.EqualTo(expectedValue),
                        "Expected a different value for UseCaching:");
        }

        private static void GenerateDummyCacheFile(string mduFile)
        {
            string cachePath = Path.ChangeExtension(mduFile, FileConstants.CachingFileExtension);
            using (File.Create(cachePath)) {}
        }

        private static bool IsSamePath(string actual, string expected)
        {
            return string.Equals(Path.GetFullPath(actual),
                                 Path.GetFullPath(expected),
                                 StringComparison.InvariantCultureIgnoreCase);
        }
    }
}