using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Api;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Api
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class RemoteFlexibleMeshModelApiTest
    {
        [Test]
        [TestCaseSource(nameof(GetDiagnosticsFilePaths))]
        public void Initialize_WithDiagnosticsFileIncludedInFolder_ThrowsException_ThenThrowsInvalidOperationException(Func<string, string> getDiaFilePath)
        {
            // Setup
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string diaFilePath = getDiaFilePath(temporaryDirectory.Path);
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(diaFilePath));
                const string fileContent = "** ERROR  :     this is an error!";
                File.WriteAllText(diaFilePath, fileContent);

                string filePath = Path.Combine(temporaryDirectory.Path, "someFile");
                var exception = new Exception(filePath);
                var api = Substitute.For<IFlexibleMeshModelApi>();
                api.Initialize(Arg.Any<string>()).Returns(x => throw exception);

                var remoteApi = new RemoteFlexibleMeshModelApi(api);

                // Call
                void Call() => remoteApi.Initialize(filePath);

                // Assert
                var thrownException = Assert.Throws<InvalidOperationException>(Call);
                Assert.That(thrownException.InnerException, Is.SameAs(exception),
                            "The inner exception of the thrown exception was not the same as the one thrown by the inner api.");

                string expectedExceptionMessage = "The kernel reported the following error(s):"
                                                  + Environment.NewLine
                                                  + fileContent
                                                  + Environment.NewLine
                                                  + $"(Errors extracted from diagnostics file {diaFilePath})";

                Assert.That(thrownException.Message, Is.EqualTo(expectedExceptionMessage), "Thrown exception message was not as expected.");
            }
        }

        [Test]
        public void Initialize_WithoutDiagnosticsFile_ThrowsException_ThenThrowsFileNotFoundException()
        {
            // Setup
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                var api = Substitute.For<IFlexibleMeshModelApi>();
                var remoteApi = new RemoteFlexibleMeshModelApi(api);
                api.Initialize(Arg.Any<string>()).Returns(x => throw new Exception());

                // Call
                void Call() => remoteApi.Initialize(Path.Combine(temporaryDirectory.Path, "NonExistingFilePath"));

                // Assert
                var thrownException = Assert.Throws<FileNotFoundException>(Call);
                Assert.That(thrownException.Message, Is.EqualTo($"Could not detect diagnostics file in {temporaryDirectory.Path}"));
            }
        }

        [Test]
        public void Initialize_WithLockedFile_ThrowsException_ThenFileFormatExceptionIsThrown()
        {
            // Setup
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string diaFilePath = Path.Combine(temporaryDirectory.Path, "myFile.dia");
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(diaFilePath));
                const string fileContent = "** ERROR  :     this is an error!";
                File.WriteAllText(diaFilePath, fileContent);

                var api = Substitute.For<IFlexibleMeshModelApi>();
                var remoteApi = new RemoteFlexibleMeshModelApi(api);
                api.Initialize(Arg.Any<string>()).Returns(x => throw new Exception());

                // Locking the diagnostics file that is being read. This will lead to an exception when Initializing.
                using (new FileStream(diaFilePath, FileMode.Open))
                {
                    // Call
                    void Call() => remoteApi.Initialize(diaFilePath);

                    // Assert
                    var thrownException = Assert.Throws<FileFormatException>(Call);
                    string expectedExceptionMessage = $"Unable to read diagnostics file {diaFilePath}: " +
                                                      $"The process cannot access the file '{diaFilePath}' because it is being used by another process.";
                    Assert.That(thrownException.Message, Is.EqualTo(expectedExceptionMessage));
                }
            }
        }

        [Test]
        public void Initialize_WithDiagnosticsFileWithoutErrors_ThrowsException_ThenInvalidOperationExceptionIsThrown()
        {
            // Setup
            using (var temporaryDirectory = new TemporaryDirectory())
            {
                string diaFilePath = Path.Combine(temporaryDirectory.Path, "myFile.dia");
                FileUtils.CreateDirectoryIfNotExists(Path.GetDirectoryName(diaFilePath));
                const string fileContent = "** INFO  :     this is an info message!";
                File.WriteAllText(diaFilePath, fileContent);

                var api = Substitute.For<IFlexibleMeshModelApi>();
                var remoteApi = new RemoteFlexibleMeshModelApi(api);
                api.Initialize(Arg.Any<string>()).Returns(x => throw new Exception());

                // Call
                void Call() => remoteApi.Initialize(diaFilePath);

                // Assert
                var thrownException = Assert.Throws<InvalidOperationException>(Call);
                var expectedExceptionMessage = $"No errors were reported in the diagnostics file {diaFilePath}";
                Assert.That(thrownException.Message, Is.EqualTo(expectedExceptionMessage));
            }
        }

        private static IEnumerable<Func<string, string>> GetDiagnosticsFilePaths()
        {
            yield return directoryPath => Path.Combine(directoryPath, "subFolder", "myFile.dia");
            yield return directoryPath => Path.Combine(directoryPath, "myFile.dia");
        }
    }
}