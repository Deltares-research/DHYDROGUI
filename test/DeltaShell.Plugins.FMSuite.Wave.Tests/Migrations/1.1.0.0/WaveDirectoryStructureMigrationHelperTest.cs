using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class WaveDirectoryStructureMigrationHelperTest
    {
        [Test]
        public void Migrate_WaveModelNull_ThrowsArgumentNullException()
        {
            void Call() => WaveDirectoryStructureMigrationHelper.Migrate(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("waveModel"));
        }

        [Test]
        public void GetTemporaryMigrationDirectoryName_SrcDirectoryNull_ThrowsArgumentNullException()
        {
            void Call() => WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(null);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("srcDirectory"));
        }

        [Test]
        public void GetTemporaryMigrationDirectoryName_SrcDirectoryParentNull_ThrowsArgumentException()
        {
            var dirInfo = new DirectoryInfo("C:\\");
            void Call() => WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(dirInfo);

            Assert.Throws<System.ArgumentException>(Call);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetTemporaryMigrationDirectoryName_NoNameClashes_ExpectedNameIsReturned()
        {
            // Setup
            const string srcDirName = "srcDirName";

            using (var tempDir = new TemporaryDirectory())
            {
                var parentDirInfo = new DirectoryInfo(tempDir.Path);
                DirectoryInfo srcDirInfo = parentDirInfo.CreateSubdirectory(srcDirName);

                // Call
                string migrationDirectoryName =
                    WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(srcDirInfo);

                // Assert
                Assert.That(migrationDirectoryName, Is.EqualTo(srcDirName + "_tmp.1"));
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void GetTemporaryMigrationDirectoryName_NameClashes_ExpectedNameIsReturned()
        {
            // Setup
            const string srcDirName = "srcDirName";

            using (var tempDir = new TemporaryDirectory())
            {
                var parentDirInfo = new DirectoryInfo(tempDir.Path);
                DirectoryInfo srcDirInfo = parentDirInfo.CreateSubdirectory(srcDirName);
                parentDirInfo.CreateSubdirectory(srcDirName + "_tmp.1");
                parentDirInfo.CreateSubdirectory(srcDirName + "_tmp.2");
                parentDirInfo.CreateSubdirectory(srcDirName + "_tmp.3");

                // Call
                string migrationDirectoryName =
                    WaveDirectoryStructureMigrationHelper.GetTemporaryMigrationDirectoryName(srcDirInfo);

                // Assert
                Assert.That(migrationDirectoryName, Is.EqualTo(srcDirName + "_tmp.4"));
            }
        }
    }
}