using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.OutputData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.OutputData
{
    [TestFixture]
    public class WaveOutputDataTest
    {
        [Test]
        public void Constructor_ValidDataSourcePath_ExpectedResult()
        {
            // Call
            var outputData = new WaveOutputData();

            // Assert
            Assert.That(outputData, Is.InstanceOf<IWaveOutputData>());
            Assert.That(outputData.DataSourcePath, Is.Null);
            Assert.That(outputData.IsConnected, Is.False);
        }

        [Test]
        public void ConnectTo_PathNull_ThrowsArgumentNullException()
        {
            
            // Setup
            var outputData = new WaveOutputData();

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
            using (var tempDir = new TemporaryDirectory())
            {
                var outputData = new WaveOutputData();

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
        public void Disconnect_WithConnection_ChangesDataSourcePathToNull()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                var outputData = new WaveOutputData();
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
            var outputData = new WaveOutputData();

            // Call
            outputData.Disconnect();

            // Assert
            Assert.That(outputData.DataSourcePath, Is.Null);
            Assert.That(outputData.IsConnected, Is.False);
        }
    }
}