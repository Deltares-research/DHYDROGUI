using System;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class DelftIniFileMigrateBehaviourTest
    {
        private static void VerifyLogHandlerDidNotReceiveAnyReports(ILogHandler logHandler)
        {
            logHandler.DidNotReceiveWithAnyArgs().ReportError(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportErrorFormat(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarning(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportWarningFormat(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportInfo(null);
            logHandler.DidNotReceiveWithAnyArgs().ReportInfoFormat(null);
        }


        [Test]
        public void Constructor_ExpectedBehaviour()
        {
            // Setup
            var migrator = Substitute.For<IDelftIniMigrator>();

            // Call
            var behaviour = new DelftIniFileMigrateBehaviour("key", ".", migrator);

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IMigrationBehaviour>());
        }

        [Test]
        public void Constructor_ExpectedKeyNull_ThrowsArgumentNullException()
        {
            var migrator = Substitute.For<IDelftIniMigrator>();
            void Call() => new DelftIniFileMigrateBehaviour(null, ".", migrator);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("expectedKey"));
        }

        [Test]
        public void Constructor_GoalDirectoryNull_ThrowsArgumentNullException()
        {
            var migrator = Substitute.For<IDelftIniMigrator>();
            void Call() => new DelftIniFileMigrateBehaviour("key", null, migrator);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("goalDirectory"));
        }


        [Test]
        public void MigrateProperty_PropertyNull_ThrowsArgumentNullException()
        {
            var migrator = Substitute.For<IDelftIniMigrator>();
            var behaviour = new DelftIniFileMigrateBehaviour("key", ".", migrator);

            void Call() => behaviour.MigrateProperty(null, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void MigrateProperty_NotAffected_ReturnsUnchangedProperty()
        {
            // Setup
            var migrator = Substitute.For<IDelftIniMigrator>();
            var logHandler = Substitute.For<ILogHandler>();

            const string key = "key";
            const string value = "value";
            const string comment = "comment";

            var property = new DelftIniProperty(key, value, comment);

            var behaviour = new DelftIniFileMigrateBehaviour("notKey", ".", migrator);

            // Call
            behaviour.MigrateProperty(property, logHandler);

            // Assert
            Assert.That(property.Name, Is.EqualTo(key));
            Assert.That(property.Value, Is.EqualTo(value));
            Assert.That(property.Comment, Is.EqualTo(comment));

            VerifyLogHandlerDidNotReceiveAnyReports(logHandler);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void MigrateProperty_Affected_CallsMigrator()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IDelftIniMigrator>();

            using (var tempDir = new TemporaryDirectory())
            {
                const string fileName = "itDontMeanAThing.txt";
                const string fileContent = "ifItAintGotThatSwing";
                const string goalDirName = "dooWahDooWah";

                string oldPath = tempDir.CreateFile(fileName, fileContent);

                string goalDir = tempDir.CreateDirectory(goalDirName);

                string expectedPath = Path.Combine(goalDir, fileName);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, oldPath, propertyComment);

                var behaviour = new DelftIniFileMigrateBehaviour(propertyName, 
                                                                 goalDir, 
                                                                 migrator);

                // Call 
                behaviour.MigrateProperty(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(expectedPath));

                migrator.Received(1).MigrateFile(oldPath, goalDir, Arg.Any<ILogHandler>() );

                VerifyLogHandlerDidNotReceiveAnyReports(logHandler);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void MigrateProperty_Affected_FileNotFound_LogsWarning()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IDelftIniMigrator>();

            using (var tempDir = new TemporaryDirectory())
            {
                const string fileName = "itDontMeanAThing.txt";
                const string goalDirName = "dooWahDooWah";

                string oldPath = Path.Combine(tempDir.Path, fileName);
                string goalDir = tempDir.CreateDirectory(goalDirName);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, oldPath, propertyComment);

                var behaviour = new DelftIniFileMigrateBehaviour(propertyName,
                                                                 goalDir, 
                                                                 migrator);

                // Call 
                behaviour.MigrateProperty(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(string.Empty));

                var expectedString =
                    $"The file associated with property {propertyName}, {oldPath}, does not exist, the property is set to an empty string.";
                logHandler.Received(1).ReportWarning(expectedString);
            }
        }

        [Test]
        public void MigrateProperty_Affected_PathEmpty_Skipped()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IDelftIniMigrator>();

            const string key = "key";
            const string value = "";
            const string comment = "comment";

            var property = new DelftIniProperty(key, value, comment);

            var behaviour = new DelftIniFileMigrateBehaviour("key", ".", migrator);

            // Call
            behaviour.MigrateProperty(property, logHandler);

            // Assert
            Assert.That(property.Name, Is.EqualTo(key));
            Assert.That(property.Value, Is.EqualTo(value));
            Assert.That(property.Comment, Is.EqualTo(comment));

            VerifyLogHandlerDidNotReceiveAnyReports(logHandler);
        }
    }
}