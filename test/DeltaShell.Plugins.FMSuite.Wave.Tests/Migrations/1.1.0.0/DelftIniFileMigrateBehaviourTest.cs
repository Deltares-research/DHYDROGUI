using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.Common.Logging;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class DelftIniFileMigrateBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedBehaviour()
        {
            // Setup
            var migrator = Substitute.For<IDelftIniFileOperator>();

            // Call
            var behaviour = new DelftIniFileMigrateBehaviour("key", ".", ".", migrator);

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IDelftIniPropertyBehaviour>());
        }

        [Test]
        [TestCaseSource(nameof(Constructor_ParameterNull_Data))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(string key,
                                                                          string relativeDirectory,
                                                                          string goalDirectory,
                                                                          IDelftIniFileOperator migrator,
                                                                          string expectedParameter)
        {
            void Call() => new DelftIniFileMigrateBehaviour(key, relativeDirectory, goalDirectory, migrator);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameter));
        }

        [Test]
        public void Invoke_PropertyNull_ThrowsArgumentNullException()
        {
            var migrator = Substitute.For<IDelftIniFileOperator>();
            var behaviour = new DelftIniFileMigrateBehaviour("key", ".", ".", migrator);

            void Call() => behaviour.Invoke(null, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void Invoke_NotAffected_ReturnsUnchangedProperty()
        {
            // Setup
            var migrator = Substitute.For<IDelftIniFileOperator>();
            var logHandler = Substitute.For<ILogHandler>();

            const string key = "key";
            const string value = "value";
            const string comment = "comment";

            var property = new DelftIniProperty(key, value, comment);

            var behaviour = new DelftIniFileMigrateBehaviour("notKey",
                                                             ".",
                                                             ".",
                                                             migrator);

            // Call
            behaviour.Invoke(property, logHandler);

            // Assert
            Assert.That(property.Name, Is.EqualTo(key));
            Assert.That(property.Value, Is.EqualTo(value));
            Assert.That(property.Comment, Is.EqualTo(comment));

            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_Affected_PropertyRelativePath_CallsMigrator()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IDelftIniFileOperator>();
            migrator.Invoke(Arg.Do<Stream>(x => x.Dispose()),
                                 Arg.Any<string>(),
                                 Arg.Any<ILogHandler>());

            using (var tempDir = new TemporaryDirectory())
            {
                const string subDirectory = "itDonMean";
                const string fileName = "AThing.txt";

                const string fileContent = "ifItAintGotThatSwing";
                const string goalDirName = "dooWahDooWah";

                tempDir.CreateDirectory(subDirectory);
                string oldPath = tempDir.CreateFile(Path.Combine(subDirectory, fileName), fileContent);

                string goalDir = tempDir.CreateDirectory(goalDirName);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, oldPath, propertyComment);

                var behaviour = new DelftIniFileMigrateBehaviour(propertyName,
                                                                 Path.Combine(tempDir.Path, subDirectory),
                                                                 goalDir,
                                                                 migrator);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(fileName));

                migrator.Received(1).Invoke(Arg.Any<Stream>(),
                                                 oldPath,
                                                 Arg.Any<ILogHandler>());

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_Affected_PropertyAbsolutePath_CallsMigrator()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IDelftIniFileOperator>();
            migrator.Invoke(Arg.Do<Stream>(x => x.Dispose()),
                                 Arg.Any<string>(),
                                 Arg.Any<ILogHandler>());

            using (var tempDir = new TemporaryDirectory())
            {
                const string subDirectory = "itDonMean";
                const string fileName = "AThing.txt";
                string absolutePath = Path.Combine(tempDir.Path, subDirectory, fileName);

                const string fileContent = "ifItAintGotThatSwing";
                const string goalDirName = "dooWahDooWah";

                tempDir.CreateDirectory(subDirectory);
                tempDir.CreateFile(Path.Combine(subDirectory, fileName), fileContent);

                string goalDir = tempDir.CreateDirectory(goalDirName);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, absolutePath, propertyComment);

                var behaviour = new DelftIniFileMigrateBehaviour(propertyName,
                                                                 Path.Combine(tempDir.Path, subDirectory),
                                                                 goalDir,
                                                                 migrator);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(fileName));

                migrator.Received(1).Invoke(Arg.Any<Stream>(),
                                                 absolutePath,
                                                 Arg.Any<ILogHandler>());

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_Affected_FileNotFound_LogsWarning()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IDelftIniFileOperator>();

            migrator.Invoke(Arg.Do<Stream>(x => x.Dispose()),
                                 Arg.Any<string>(),
                                 Arg.Any<ILogHandler>());

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
                                                                 tempDir.Path,
                                                                 goalDir,
                                                                 migrator);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(oldPath));

                const string expectedString = "The file associated with property {0}, {1} at {2}, does not exist and thus is not migrated.";
                logHandler.Received(1).ReportWarningFormat(expectedString, propertyName, fileName, oldPath);
            }
        }

        [Test]
        public void Invoke_Affected_PathEmpty_Skipped()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IDelftIniFileOperator>();

            const string key = "key";
            const string value = "";
            const string comment = "comment";

            var property = new DelftIniProperty(key, value, comment);

            var behaviour = new DelftIniFileMigrateBehaviour("key", ".", ".", migrator);

            // Call
            behaviour.Invoke(property, logHandler);

            // Assert
            Assert.That(property.Name, Is.EqualTo(key));
            Assert.That(property.Value, Is.EqualTo(value));
            Assert.That(property.Comment, Is.EqualTo(comment));

            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }

        private static IEnumerable<TestCaseData> Constructor_ParameterNull_Data()
        {
            var migrator = Substitute.For<IDelftIniFileOperator>();

            yield return new TestCaseData(null, ".", ".", migrator, "expectedKey");
            yield return new TestCaseData("key", null, ".", migrator, "relativeDirectory");
            yield return new TestCaseData("key", ".", null, migrator, "goalDirectory");
            yield return new TestCaseData("key", ".", ".", null, "migrator");
        }
    }
}