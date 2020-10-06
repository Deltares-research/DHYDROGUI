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
        public void Constructor_DataSourcePathNull_ThrowsArgumentNullException()
        {
            void Call() => new WaveOutputData(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("dataSourcePath"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Constructor_ValidDataSourcePath_ExpectedResult()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                // Call
                var outputData = new WaveOutputData(tempDir.Path);

                // Assert
                Assert.That(outputData, Is.InstanceOf<IWaveOutputData>());
                Assert.That(outputData.DataSourcePath, Is.EqualTo(tempDir.Path));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectTo_PathNull_ThrowsArgumentNullException()
        {
            
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                var outputData = new WaveOutputData(tempDir.Path);

                // Call | Assert
                void Call() => outputData.ConnectTo(null);

                var exception = Assert.Throws<System.ArgumentNullException>(Call);
                Assert.That(exception.ParamName, Is.EqualTo("dataSourcePath"));
            }
        }

        [Test]
        public void ConnectTo_ValidPath_ChangesDataSourcePath()
        {
            using (var tempDir = new TemporaryDirectory())
            {
                DirectoryInfo oldPath = Directory.CreateDirectory(Path.Combine(tempDir.Path, "old"));
                DirectoryInfo newPath = Directory.CreateDirectory(Path.Combine(tempDir.Path, "new"));
                var outputData = new WaveOutputData(oldPath.FullName);

                // Call
                outputData.ConnectTo(newPath.FullName);

                // Assert
                Assert.That(outputData.DataSourcePath, Is.EqualTo(newPath.FullName));
            }
        }
    }
}