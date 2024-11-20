using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Migrations;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations
{
    [TestFixture]
    public class WavesMigratorTest
    {
        [Test]
        public void Migrate_ProjectPathDoesNotExist_ThrowsFileNotFoundException()
        {
            // Call
            void Call() => WavesMigrator.Migrate("does_not_exist.dsproj", new Version(), new Version());

            // Assert
            var e = Assert.Throws<FileNotFoundException>(Call);
            Assert.That(e.FileName, Is.EqualTo("does_not_exist.dsproj"));
            Assert.That(e.Message, Is.EqualTo("Project file does not exist: does_not_exist.dsproj"));
        }

        [Test]
        public void Migrates_ProjectVersionEqualsCurrentVersion_Returns()
        {
            using (var temp = new TemporaryDirectory())
            {
                string projectFile = temp.CreateFile("project.dsproj");

                // Call
                void Call() => WavesMigrator.Migrate(projectFile, new Version(1, 2, 3, 4), new Version(1, 2, 3, 4));

                // Assert
                Assert.DoesNotThrow(Call);
            }
        }

        [Test]
        public void Migrates_ProjectVersionHigherThanCurrentVersion_ThrowsArgumentException()
        {
            using (var temp = new TemporaryDirectory())
            {
                string projectFile = temp.CreateFile("project.dsproj");

                // Call
                void Call() => WavesMigrator.Migrate(projectFile, new Version(2, 0, 0, 0), new Version(1, 0, 0, 0));

                // Assert
                var e = Assert.Throws<ArgumentException>(Call);
                Assert.That(e.Message, Is.EqualTo("The project version (2.0.0.0) cannot be higher than the current application version (1.0.0.0)."));
            }
        }

        [TestCaseSource(nameof(Migrate_ArgumentNullCases))]
        public void Migrate_VersionNull_ThrowsArgumentNullException(Version projectVersion, Version currentVersion, string expParamName)
        {
            // Call
            void Call() => WavesMigrator.Migrate("the_path.dsproj", projectVersion, currentVersion);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        private static IEnumerable<TestCaseData> Migrate_ArgumentNullCases()
        {
            yield return new TestCaseData(null, new Version(), "projectVersion");
            yield return new TestCaseData(new Version(), null, "currentVersion");
        }

        [TestCase("")]
        [TestCase(null)]
        public void Migrates_ProjectPathNullOrEmpty_ThrowsArgumentException(string projectPath)
        {
            // Call
            void Call() => WavesMigrator.Migrate(projectPath, new Version(), new Version());

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("projectPath"));
        }
    }
}