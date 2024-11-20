using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.CommonTools.TextData;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WaveOutputDataTest
    {
        [Test]
        public void Constructor_ValidDataSourcePath_ExpectedResult()
        {
            // Setup 
            var harvester = Substitute.For<IWaveOutputDataHarvester>();
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();

            // Call
            var outputData = new WaveOutputData(harvester, copyHandler);

            // Assert
            Assert.That(outputData, Is.InstanceOf<IWaveOutputData>());
            Assert.That(outputData, Is.InstanceOf<INotifyPropertyChange>());

            Assert.That(outputData.DataSourcePath, Is.Null);
            Assert.That(outputData.IsConnected, Is.False);

            Assert.That(outputData.DiagnosticFiles, Is.Not.Null);
            Assert.That(outputData.DiagnosticFiles, Is.Empty);

            Assert.That(outputData.SpectraFiles, Is.Not.Null);
            Assert.That(outputData.SpectraFiles, Is.Empty);
            
            Assert.That(outputData.SwanFiles, Is.Not.Null);
            Assert.That(outputData.SwanFiles, Is.Empty);

            Assert.That(outputData.WavmFileFunctionStores, Is.Not.Null);
            Assert.That(outputData.WavmFileFunctionStores, Is.Empty);

            Assert.That(outputData.WavhFileFunctionStores, Is.Not.Null);
            Assert.That(outputData.WavhFileFunctionStores, Is.Empty);
        }

        [Test]
        public void Constructor_HarvesterNull_ThrowsArgumentNullException()
        {
            // Setup
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();

            // Call | Assert
            void Call() => new WaveOutputData(null, copyHandler);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("harvester"));
        }

        [Test]
        public void Constructor_CopyHandlerNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = Substitute.For<IWaveOutputDataHarvester>();

            // Call | Assert
            void Call() => new WaveOutputData(harvester, null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("copyHandler"));
        }

        [Test]
        public void ConnectTo_PathNull_ThrowsArgumentNullException()
        {
            // Setup
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var harvester = Substitute.For<IWaveOutputDataHarvester>();

            var outputData = new WaveOutputData(harvester, copyHandler);

            // Call | Assert
            void Call() => outputData.ConnectTo(null, false);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataSourcePath"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(true)]
        [TestCase(false)]
        public void ConnectTo_ValidPath_ChangesDataSourcePath(bool isStoredInWorkingDirectory)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
                var harvester = Substitute.For<IWaveOutputDataHarvester>();

                var outputData = new WaveOutputData(harvester, copyHandler);

                // Call
                outputData.ConnectTo(tempDir.Path, isStoredInWorkingDirectory);

                // Assert
                Assert.That(outputData.DataSourcePath, Is.EqualTo(tempDir.Path));
                Assert.That(outputData.IsConnected, Is.True);
                Assert.That(outputData.IsStoredInWorkingDirectory, Is.EqualTo(isStoredInWorkingDirectory));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectTo_ValidPath_Retrieves()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string dataSourcePath = tempDir.Path;

                var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var logHandler = Substitute.For<ILogHandler>();

                var diagFiles = new List<ReadOnlyTextFileData> { new ReadOnlyTextFileData("name", "content", ReadOnlyTextFileDataType.Default)};

                harvester.HarvestDiagnosticFiles(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                 logHandler)
                         .Returns(diagFiles);

                var spectraFiles = new List<ReadOnlyTextFileData> { new ReadOnlyTextFileData("name", "content", ReadOnlyTextFileDataType.Default) };
                harvester.HarvestSpectraFiles(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                              logHandler)
                         .Returns(spectraFiles);
                
                var swanFiles = new List<ReadOnlyTextFileData> { new ReadOnlyTextFileData("name", "content", ReadOnlyTextFileDataType.Default) };
                harvester.HarvestSwanFiles(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                              logHandler)
                         .Returns(swanFiles);

                var wavmFileFunctionStores = new List<WavmFileFunctionStore> { null };
                harvester.HarvestWavmFileFunctionStores(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                        logHandler)
                         .Returns(wavmFileFunctionStores);

                var wavhFileFunctionStores = new List<WavhFileFunctionStore> { null, null};
                harvester.HarvestWavhFileFunctionStores(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                        logHandler)
                         .Returns(wavhFileFunctionStores);

                var outputData = new WaveOutputData(harvester, copyHandler);

                // Call
                outputData.ConnectTo(dataSourcePath, true, logHandler);

                // Assert
                Assert.That(outputData.DiagnosticFiles, Is.EquivalentTo(diagFiles));
                Assert.That(outputData.SpectraFiles, Is.EquivalentTo(spectraFiles));
                Assert.That(outputData.SwanFiles, Is.EquivalentTo(swanFiles));
                Assert.That(outputData.WavmFileFunctionStores, Is.EquivalentTo(wavmFileFunctionStores));
                Assert.That(outputData.WavhFileFunctionStores, Is.EquivalentTo(wavhFileFunctionStores));
                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectTo_PathDoesNotExist_DisconnectsInstead()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string dataSourcePath = tempDir.Path;

                var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var logHandler = Substitute.For<ILogHandler>();

                var diagFiles = new List<ReadOnlyTextFileData>();

                harvester.HarvestDiagnosticFiles(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                 logHandler)
                         .Returns(diagFiles);

                var spectraFiles = new List<ReadOnlyTextFileData>();
                harvester.HarvestSpectraFiles(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                              logHandler)
                         .Returns(spectraFiles);
                
                var swanFiles = new List<ReadOnlyTextFileData>();
                harvester.HarvestSwanFiles(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                              logHandler)
                         .Returns(swanFiles);

                var wavmFileFunctionStores = new List<WavmFileFunctionStore>();
                harvester.HarvestWavmFileFunctionStores(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                        logHandler)
                         .Returns(wavmFileFunctionStores);

                var wavhFileFunctionStores = new List<WavhFileFunctionStore>();
                harvester.HarvestWavhFileFunctionStores(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                        logHandler)
                         .Returns(wavhFileFunctionStores);

                var outputData = new WaveOutputData(harvester, copyHandler);
                outputData.ConnectTo(dataSourcePath, true, null);

                string nonExistingPath = Path.GetFullPath("some/non/existing/toad/");
                harvester.ClearReceivedCalls();

                // Call
                outputData.ConnectTo(nonExistingPath, true, logHandler);

                // Assert
                Assert.That(harvester.ReceivedCalls(), Is.Empty);

                Assert.That(outputData.DiagnosticFiles, Is.Empty);
                Assert.That(outputData.SpectraFiles, Is.Empty);
                Assert.That(outputData.SwanFiles, Is.Empty);
                Assert.That(outputData.WavmFileFunctionStores, Is.Empty);
                Assert.That(outputData.WavhFileFunctionStores, Is.Empty);

                Assert.That(outputData.IsConnected, Is.False);
                Assert.That(outputData.DataSourcePath, Is.Null);

                logHandler.Received(1).ReportErrorFormat("The directory at {0} does not exist, disconnecting output instead.", 
                                                         nonExistingPath);
            }
        }

        [Test]
        public void SwitchTo_DataTargetPathNull_ThrowsArgumentNullException()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            var harvester = Substitute.For<IWaveOutputDataHarvester>();
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var outputData =  new WaveOutputData(harvester, copyHandler);

            // Call | Assert
            void Call() => outputData.SwitchTo(null, logHandler);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataTargetPath"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void SwitchTo_PathDoesNotExist_DisconnectsInstead()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string dataSourcePath = tempDir.Path;

                var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var logHandler = Substitute.For<ILogHandler>();

                var outputData = new WaveOutputData(harvester, copyHandler);
                outputData.ConnectTo(dataSourcePath, true, null);

                string nonExistingPath = Path.GetFullPath("some/non/existing/toad/");

                // Call
                outputData.SwitchTo(nonExistingPath, logHandler);

                // Assert
                Assert.That(copyHandler.ReceivedCalls(), Is.Empty);

                Assert.That(outputData.DiagnosticFiles, Is.Empty);
                Assert.That(outputData.SpectraFiles, Is.Empty);
                Assert.That(outputData.SwanFiles, Is.Empty);
                Assert.That(outputData.WavmFileFunctionStores, Is.Empty);
                Assert.That(outputData.WavhFileFunctionStores, Is.Empty);

                Assert.That(outputData.IsConnected, Is.False);
                Assert.That(outputData.DataSourcePath, Is.Null);

                logHandler.Received(1).ReportErrorFormat("The directory at {0} does not exist, disconnecting output instead.", 
                                                         nonExistingPath);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(true)]
        [TestCase(false)]
        public void SwitchTo_IsStoredInWorkingDirectory_CopiesDataCorrectly(bool isStoredInOutputDirectory)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string targetDirectoryName = "target";
                string dataSourcePath = tempDir.CreateDirectory("source");
                string dataTargetPath = tempDir.CreateDirectory(targetDirectoryName);

                var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var featureContainer = Substitute.For<IWaveFeatureContainer>();
                var logHandler = Substitute.For<ILogHandler>();

                var outputData = new WaveOutputData(harvester, copyHandler);
                outputData.ConnectTo(dataSourcePath, isStoredInOutputDirectory, null);

                string sourceWavmPath = TestHelper.GetTestFilePath(@"WaveOutputDataHarvesterTest\wavm-Waves.nc");
                string ncWavmPath = tempDir.CopyTestDataFileToTempDirectory(sourceWavmPath);
                var ncWavmStore = new WavmFileFunctionStore(ncWavmPath);
                outputData.WavmFileFunctionStores.Add(ncWavmStore);
                string targetWavmPath = Path.Combine(tempDir.Path, 
                                                     targetDirectoryName, 
                                                     Path.GetFileName(ncWavmPath));
                File.Copy(ncWavmPath, 
                          targetWavmPath);

                string sourceWavhPath = TestHelper.GetTestFilePath(@"WaveOutputDataHarvesterTest\wavh-Waves.nc");
                string ncWavhPath = tempDir.CopyTestDataFileToTempDirectory(sourceWavhPath);
                var ncWavhStore = new WavhFileFunctionStore(ncWavhPath, featureContainer);
                outputData.WavhFileFunctionStores.Add(ncWavhStore);
                string targetWavhPath = Path.Combine(tempDir.Path, 
                                                     targetDirectoryName, 
                                                     Path.GetFileName(ncWavhPath));
                File.Copy(ncWavhPath, 
                          targetWavhPath);

                const string diagFileName = "diag.txt";
                tempDir.CreateFile(Path.Combine(targetDirectoryName, diagFileName));
                var diagFile = new ReadOnlyTextFileData(diagFileName, "", ReadOnlyTextFileDataType.Default);
                outputData.DiagnosticFiles.Add(diagFile);

                const string spectraFileName = "spectra.txt";
                tempDir.CreateFile(Path.Combine(targetDirectoryName, spectraFileName));
                var spectraFile = new ReadOnlyTextFileData(spectraFileName, "", ReadOnlyTextFileDataType.Default);
                outputData.SpectraFiles.Add(spectraFile);
                
                const string swanFileName = "INPUT_1_20060105_000000";
                tempDir.CreateFile(Path.Combine(targetDirectoryName, swanFileName));
                var swanFile = new ReadOnlyTextFileData(swanFileName, "", ReadOnlyTextFileDataType.Default);
                outputData.SwanFiles.Add(swanFile);

                // Call
                outputData.SwitchTo(dataTargetPath, logHandler);

                // Assert
                if (isStoredInOutputDirectory)
                {
                    copyHandler.Received(1).CopyRunDataTo(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                          Arg.Is<DirectoryInfo>(x => x.FullName == dataTargetPath), 
                                                          logHandler);
                }
                else
                {
                    copyHandler.Received(1).CopyOutputDataTo(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                             Arg.Is<DirectoryInfo>(x => x.FullName == dataTargetPath), 
                                                             logHandler);
                }

                Assert.That(outputData.DiagnosticFiles, Has.Member(diagFile));
                Assert.That(outputData.DiagnosticFiles.Count, Is.EqualTo(1));

                Assert.That(outputData.SpectraFiles, Has.Member(spectraFile));
                Assert.That(outputData.SpectraFiles.Count, Is.EqualTo(1));
                
                Assert.That(outputData.SwanFiles, Has.Member(swanFile));
                Assert.That(outputData.SwanFiles.Count, Is.EqualTo(1));

                Assert.That(outputData.WavmFileFunctionStores, Has.Member(ncWavmStore));
                Assert.That(outputData.WavmFileFunctionStores.Count, Is.EqualTo(1));

                Assert.That(outputData.WavhFileFunctionStores, Has.Member(ncWavhStore));
                Assert.That(outputData.WavhFileFunctionStores.Count, Is.EqualTo(1));

                Assert.That(ncWavmStore.Path, Is.EqualTo(targetWavmPath));
                Assert.That(ncWavhStore.Path, Is.EqualTo(targetWavhPath));

                Assert.That(outputData.IsConnected, Is.True);
                Assert.That(outputData.DataSourcePath, Is.EqualTo(dataTargetPath));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [TestCase(true)]
        [TestCase(false)]
        public void SwitchTo_DataRemoved_ClearsOutputData(bool isStoredInOutputDirectory)
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                string dataSourcePath = tempDir.CreateDirectory("source");
                string dataTargetPath = tempDir.CreateDirectory("target");

                var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var featureContainer = Substitute.For<IWaveFeatureContainer>();
                var logHandler = Substitute.For<ILogHandler>();

                var outputData = new WaveOutputData(harvester, copyHandler);
                outputData.ConnectTo(dataSourcePath, isStoredInOutputDirectory, null);

                string sourceWavmPath = TestHelper.GetTestFilePath(@"WaveOutputDataHarvesterTest\wavm-Waves.nc");
                string ncWavmPath = tempDir.CopyTestDataFileToTempDirectory(sourceWavmPath);
                var ncWavmStore = new WavmFileFunctionStore(ncWavmPath);
                outputData.WavmFileFunctionStores.Add(ncWavmStore);

                string sourceWavhPath = TestHelper.GetTestFilePath(@"WaveOutputDataHarvesterTest\wavh-Waves.nc");
                string ncWavhPath = tempDir.CopyTestDataFileToTempDirectory(sourceWavhPath);
                var ncWavhStore = new WavhFileFunctionStore(ncWavhPath, featureContainer);
                outputData.WavhFileFunctionStores.Add(ncWavhStore);

                outputData.DiagnosticFiles.Add(new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default));
                outputData.SpectraFiles.Add(new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default));
                outputData.SwanFiles.Add(new ReadOnlyTextFileData("", "", ReadOnlyTextFileDataType.Default));

                // Call
                outputData.SwitchTo(dataTargetPath, logHandler);

                // Assert
                if (isStoredInOutputDirectory)
                {
                    copyHandler.Received(1).CopyRunDataTo(Arg.Any<DirectoryInfo>(),
                                                          Arg.Any<DirectoryInfo>(), 
                                                          logHandler);
                }
                else
                {
                    copyHandler.Received(1).CopyOutputDataTo(Arg.Any<DirectoryInfo>(),
                                                             Arg.Any<DirectoryInfo>(), 
                                                             logHandler);
                }

                Assert.That(outputData.DiagnosticFiles, Is.Empty);
                Assert.That(outputData.SpectraFiles, Is.Empty);
                Assert.That(outputData.SwanFiles, Is.Empty);
                Assert.That(outputData.WavmFileFunctionStores, Is.Empty);
                Assert.That(outputData.WavhFileFunctionStores, Is.Empty);

                Assert.That(outputData.IsConnected, Is.True);
                Assert.That(outputData.DataSourcePath, Is.EqualTo(dataTargetPath));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Disconnect_WithConnection_ChangesDataSourcePathToNull()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
                var harvester = Substitute.For<IWaveOutputDataHarvester>();

                var outputData = new WaveOutputData(harvester, copyHandler);
                outputData.ConnectTo(tempDir.Path, true);

                // Call
                outputData.Disconnect();

                // Assert
                Assert.That(outputData.DataSourcePath, Is.Null);
                Assert.That(outputData.IsConnected, Is.False);
                Assert.That(outputData.IsStoredInWorkingDirectory, Is.False);
            }
        }

        [Test]
        public void Disconnect_WithoutConnection_ChangesDataSourcePathToNull()
        { 
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var harvester = Substitute.For<IWaveOutputDataHarvester>();

            var outputData = new WaveOutputData(harvester, copyHandler);

            // Call
            outputData.Disconnect();

            // Assert
            Assert.That(outputData.DataSourcePath, Is.Null);
            Assert.That(outputData.IsConnected, Is.False);
        }

        [Test]
        public void Disconnect_ResetsDiagnosticFilesToAnEmptyList()
        {
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var harvester = Substitute.For<IWaveOutputDataHarvester>();

            var outputData = new WaveOutputData(harvester, copyHandler);
            outputData.DiagnosticFiles.Add(new ReadOnlyTextFileData("test", "content", ReadOnlyTextFileDataType.Default));

            IEventedList<ReadOnlyTextFileData> prevDiagnosticFiles = outputData.DiagnosticFiles;

            // Call
            outputData.Disconnect();

            // Assert
            Assert.That(outputData.DiagnosticFiles, Is.SameAs(prevDiagnosticFiles));
            Assert.That(outputData.DiagnosticFiles, Is.Empty);
        }

        [Test]
        public void Disconnect_ResetsSpectraFilesToAnEmptyList()
        {
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var harvester = Substitute.For<IWaveOutputDataHarvester>();

            var outputData = new WaveOutputData(harvester, copyHandler);
            outputData.SpectraFiles.Add(new ReadOnlyTextFileData("test", "content", ReadOnlyTextFileDataType.Default));

            IEventedList<ReadOnlyTextFileData> prevSpectraFiles = outputData.SpectraFiles;

            // Call
            outputData.Disconnect();

            // Assert
            Assert.That(outputData.SpectraFiles, Is.SameAs(prevSpectraFiles));
            Assert.That(outputData.SpectraFiles, Is.Empty);
        }
        
        [Test]
        public void Disconnect_ResetsSwanFilesToAnEmptyList()
        {
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var harvester = Substitute.For<IWaveOutputDataHarvester>();

            var outputData = new WaveOutputData(harvester, copyHandler);
            outputData.SwanFiles.Add(new ReadOnlyTextFileData("test", "content", ReadOnlyTextFileDataType.Default));

            IEventedList<ReadOnlyTextFileData> prevSwanFiles = outputData.SwanFiles;

            // Call
            outputData.Disconnect();

            // Assert
            Assert.That(outputData.SwanFiles, Is.SameAs(prevSwanFiles));
            Assert.That(outputData.SwanFiles, Is.Empty);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Disconnect_ResetsWavmFileFunctionStoreToAnEmptyList()
        {
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var harvester = Substitute.For<IWaveOutputDataHarvester>();

            using (var tempDir = new TemporaryDirectory())
            {
                string sourcePath = TestHelper.GetTestFilePath(@"WaveOutputDataHarvesterTest\wavm-Waves.nc");
                string ncPath = tempDir.CopyTestDataFileToTempDirectory(sourcePath);
                var ncStore = new WavmFileFunctionStore(ncPath);

                var outputData = new WaveOutputData(harvester, copyHandler);
                outputData.WavmFileFunctionStores.Add(ncStore);

                IEventedList<IWavmFileFunctionStore> prevWavmFiles = outputData.WavmFileFunctionStores;

                // Call
                outputData.Disconnect();

                // Assert
                Assert.That(outputData.WavmFileFunctionStores, Is.SameAs(prevWavmFiles));
                Assert.That(outputData.WavmFileFunctionStores, Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Disconnect_ResetsWavhFileFunctionStoreToAnEmptyList()
        {
            var copyHandler = Substitute.For<IWaveOutputDataCopyHandler>();
            var harvester = Substitute.For<IWaveOutputDataHarvester>();
            var featureContainer = Substitute.For<IWaveFeatureContainer>();

            using (var tempDir = new TemporaryDirectory())
            {
                string sourcePath = TestHelper.GetTestFilePath(@"WaveOutputDataHarvesterTest\wavh-Waves.nc");
                string ncPath = tempDir.CopyTestDataFileToTempDirectory(sourcePath);
                var ncStore = new WavhFileFunctionStore(ncPath, featureContainer);

                var outputData = new WaveOutputData(harvester, copyHandler);
                outputData.WavhFileFunctionStores.Add(ncStore);

                IEventedList<IWavhFileFunctionStore> prevWavhFiles = outputData.WavhFileFunctionStores;

                // Call
                outputData.Disconnect();

                // Assert
                Assert.That(outputData.WavhFileFunctionStores, Is.SameAs(prevWavhFiles));
                Assert.That(outputData.WavhFileFunctionStores, Is.Empty);
            }
        }
    }
}