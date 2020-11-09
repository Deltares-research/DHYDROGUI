using System;
using System.IO;
using DeltaShell.Plugins.FMSuite.Wave.Migrations;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations
{
    [TestFixture]
    public class WavesMigratorTest
    {
        [Test]
        public void Migrate_VersionNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WavesMigrator.Migrate("the_path.dsproj", null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("projectVersion"));
        }

        [Test]
        public void Migrate_ProjectPathDoesNotExist_ThrowsFileNotFoundException()
        {
            // Call
            void Call() => WavesMigrator.Migrate("does_not_exist.dsproj", new Version());

            // Assert
            var e = Assert.Throws<FileNotFoundException>(Call);
            Assert.That(e.FileName, Is.EqualTo("does_not_exist.dsproj"));
            Assert.That(e.Message, Is.EqualTo("Project file does not exist: does_not_exist.dsproj"));
        }

        [TestCase("")]
        [TestCase(null)]
        public void Migrates_ProjectPathNullOrEmpty_ThrowsArgumentException(string projectPath)
        {
            // Call
            void Call() => WavesMigrator.Migrate(projectPath, new Version());

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("projectPath"));
        }
    }
}