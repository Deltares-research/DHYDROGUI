using System;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class NoDependentsFileMigrateBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedBehaviour()
        {
            // Call
            var behaviour = new NoDependentsFileMigrateBehaviour("someKey", ".", ".");

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IDelftIniPropertyBehaviour>());
        }

        [Test]
        public void Constructor_ExpectedKeyNull_ThrowsArgumentNullException()
        {
            void Call() => new NoDependentsFileMigrateBehaviour(null, ".", ".");

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("expectedKey"));
        }

        [Test]
        public void Constructor_RelativeDirectoryNull_ThrowsArgumentNullException()
        {
            void Call() => new NoDependentsFileMigrateBehaviour("key", null, ".");

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("relativeDirectory"));
        }

        [Test]
        public void Constructor_GoalDirectoryNull_ThrowsArgumentNullException()
        {
            void Call() => new NoDependentsFileMigrateBehaviour("key", ".", null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("goalDirectory"));
        }

        [Test]
        public void Invoke_PropertyNull_ThrowsArgumentNullException()
        {
            var behaviour = new NoDependentsFileMigrateBehaviour("someKey", ".", ".");

            void Call() => behaviour.Invoke(null, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void Invoke_NotAffected_ReturnsUnchangedProperty()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            const string key = "key";
            const string value = "value";
            const string comment = "comment";

            var property = new DelftIniProperty(key, value, comment);

            var behaviour = new NoDependentsFileMigrateBehaviour("notKey", ".", "toad/to/elsewhere/");

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
        public void Invoke_Affected_PropertyRelativePath_ExpectedResults()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                const string subDirectory = "itDonMean";
                const string fileName = "AThing.txt";

                const string fileContent = "ifItAintGotThatSwing";
                const string goalDirName = "dooWahDooWah";

                tempDir.CreateDirectory(subDirectory);
                string oldPath = tempDir.CreateFile(Path.Combine(subDirectory, fileName), fileContent);
                var oldPathInfo = new FileInfo(oldPath);

                string goalDir = tempDir.CreateDirectory(goalDirName);

                string expectedPath = Path.Combine(goalDir, fileName);
                var expectedPathInfo = new FileInfo(expectedPath);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, fileName, propertyComment);

                var behaviour = new NoDependentsFileMigrateBehaviour(propertyName,
                                                                     Path.Combine(tempDir.Path, subDirectory),
                                                                     goalDir);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(fileName));

                Assert.That(oldPathInfo.Exists, Is.False);
                Assert.That(expectedPathInfo.Exists, Is.True);

                string resultContent = File.ReadAllText(expectedPathInfo.FullName);
                Assert.That(resultContent, Is.EqualTo(fileContent));

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_Affected_PropertyAbsolutePathSameRelativeDirectory_ExpectedResults()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                const string subDirectory = "itDonMean";
                const string fileName = "AThing.txt";
                string absolutePath = Path.Combine(tempDir.Path, subDirectory, fileName);

                const string fileContent = "ifItAintGotThatSwing";
                const string goalDirName = "dooWahDooWah";

                tempDir.CreateDirectory(subDirectory);
                string oldPath = tempDir.CreateFile(Path.Combine(subDirectory, fileName), fileContent);
                var oldPathInfo = new FileInfo(oldPath);

                string goalDir = tempDir.CreateDirectory(goalDirName);

                string expectedPath = Path.Combine(goalDir, fileName);
                var expectedPathInfo = new FileInfo(expectedPath);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, absolutePath, propertyComment);

                var behaviour = new NoDependentsFileMigrateBehaviour(propertyName,
                                                                     Path.Combine(tempDir.Path, subDirectory),
                                                                     goalDir);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(fileName));

                Assert.That(oldPathInfo.Exists, Is.False);
                Assert.That(expectedPathInfo.Exists, Is.True);

                string resultContent = File.ReadAllText(expectedPathInfo.FullName);
                Assert.That(resultContent, Is.EqualTo(fileContent));

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_Affected_PropertyAbsolutePathDifferentRelativeDirectory_ExpectedResults()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                const string subDirectory = "itDonMean";
                const string fileName = "AThing.txt";
                string absolutePath = Path.Combine(tempDir.Path, subDirectory, fileName);

                const string fileContent = "ifItAintGotThatSwing";
                const string goalDirName = "dooWahDooWah";

                tempDir.CreateDirectory(subDirectory);
                string oldPath = tempDir.CreateFile(Path.Combine(subDirectory, fileName), fileContent);
                var oldPathInfo = new FileInfo(oldPath);

                string relativeDir = tempDir.CreateDirectory("dooWah");

                string goalDir = tempDir.CreateDirectory(goalDirName);

                string expectedPath = Path.Combine(goalDir, fileName);
                var expectedPathInfo = new FileInfo(expectedPath);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, absolutePath, propertyComment);

                var behaviour = new NoDependentsFileMigrateBehaviour(propertyName,
                                                                     relativeDir,
                                                                     goalDir);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Name, Is.EqualTo(propertyName));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(fileName));

                Assert.That(oldPathInfo.Exists, Is.False);
                Assert.That(expectedPathInfo.Exists, Is.True);

                string resultContent = File.ReadAllText(expectedPathInfo.FullName);
                Assert.That(resultContent, Is.EqualTo(fileContent));

                Assert.That(logHandler.ReceivedCalls(), Is.Empty);
            }
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_Affected_FileNotFound_LogsWarning()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();

            using (var tempDir = new TemporaryDirectory())
            {
                const string fileName = "itDontMeanAThing.txt";
                const string goalDirName = "dooWahDooWah";

                string oldPath = Path.Combine(tempDir.Path, fileName);
                string goalDir = tempDir.CreateDirectory(goalDirName);

                const string propertyName = "key";
                const string propertyComment = "Comment";
                var property = new DelftIniProperty(propertyName, oldPath, propertyComment);

                var behaviour = new NoDependentsFileMigrateBehaviour(propertyName,
                                                                     tempDir.Path,
                                                                     goalDir);

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

            const string key = "key";
            const string value = "";
            const string comment = "comment";

            var property = new DelftIniProperty(key, value, comment);

            var behaviour = new NoDependentsFileMigrateBehaviour("key", ".", ".");

            // Call
            behaviour.Invoke(property, logHandler);

            // Assert
            Assert.That(property.Name, Is.EqualTo(key));
            Assert.That(property.Value, Is.EqualTo(value));
            Assert.That(property.Comment, Is.EqualTo(comment));

            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }
    }
}