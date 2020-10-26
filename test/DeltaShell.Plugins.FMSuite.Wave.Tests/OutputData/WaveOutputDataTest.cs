using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.TestUtils;
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

            // Call
            var outputData = new WaveOutputData(harvester);

            // Assert
            Assert.That(outputData, Is.InstanceOf<IWaveOutputData>());
            Assert.That(outputData, Is.InstanceOf<INotifyPropertyChange>());
            Assert.That(outputData.DataSourcePath, Is.Null);
            Assert.That(outputData.IsConnected, Is.False);
            Assert.That(outputData.DiagnosticFiles, Is.Not.Null);
            Assert.That(outputData.DiagnosticFiles, Is.Empty);
        }

        [Test]
        public void Constructor_HarvesterNull_ThrowsArgumentNullException()
        {
            // Call | Assert
            void Call() => new WaveOutputData(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("harvester"));
        }

        [Test]
        public void ConnectTo_PathNull_ThrowsArgumentNullException()
        {
            // Setup
            var harvester = Substitute.For<IWaveOutputDataHarvester>();
            var outputData = new WaveOutputData(harvester);

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
                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var outputData = new WaveOutputData(harvester);

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

                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var logHandler = Substitute.For<ILogHandler>();

                var diagFiles = new List<ReadOnlyTextFileData>();

                harvester.HarvestDiagnosticFiles(Arg.Is<DirectoryInfo>(x => x.FullName == dataSourcePath),
                                                 logHandler)
                         .Returns(diagFiles);

                var outputData = new WaveOutputData(harvester);

                // Call
                outputData.ConnectTo(dataSourcePath, true, logHandler);

                // Assert
                Assert.That(outputData.DiagnosticFiles, Is.SameAs(diagFiles));
                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Disconnect_WithConnection_ChangesDataSourcePathToNull()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                var harvester = Substitute.For<IWaveOutputDataHarvester>();
                var outputData = new WaveOutputData(harvester);
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
            var harvester = Substitute.For<IWaveOutputDataHarvester>();
            var outputData = new WaveOutputData(harvester);

            // Call
            outputData.Disconnect();

            // Assert
            Assert.That(outputData.DataSourcePath, Is.Null);
            Assert.That(outputData.IsConnected, Is.False);
        }

        [Test]
        public void Disconnect_ResetsDiagnosticFilesToNewEmptyList()
        {
            var harvester = Substitute.For<IWaveOutputDataHarvester>();
            var outputData = new WaveOutputData(harvester);
            IReadOnlyList<ReadOnlyTextFileData> prevDiagnosticFiles = outputData.DiagnosticFiles;

            // Call
            outputData.Disconnect();

            // Assert
            Assert.That(outputData.DiagnosticFiles, Is.Not.SameAs(prevDiagnosticFiles));
            Assert.That(outputData.DiagnosticFiles, Is.Not.Null);
            Assert.That(outputData.DiagnosticFiles, Is.Empty);
        }
    }
}