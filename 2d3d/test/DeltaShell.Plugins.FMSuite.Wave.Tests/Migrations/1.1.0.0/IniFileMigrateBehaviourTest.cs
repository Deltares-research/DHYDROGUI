﻿using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using Deltares.Infrastructure.API.Logging;
using Deltares.Infrastructure.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DeltaShell.Plugins.FMSuite.Wave.Migrations._1._1._0._0;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Migrations._1._1._0._0
{
    [TestFixture]
    public class IniFileMigrateBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedBehaviour()
        {
            // Setup
            var migrator = Substitute.For<IIniFileOperator>();

            // Call
            var behaviour = new IniFileMigrateBehaviour("key", ".", ".", migrator);

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IIniPropertyBehaviour>());
        }

        [Test]
        [TestCaseSource(nameof(Constructor_ParameterNull_Data))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(string key,
                                                                          string relativeDirectory,
                                                                          string goalDirectory,
                                                                          IIniFileOperator migrator,
                                                                          string expectedParameter)
        {
            void Call() => new IniFileMigrateBehaviour(key, relativeDirectory, goalDirectory, migrator);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameter));
        }

        [Test]
        public void Invoke_PropertyNull_ThrowsArgumentNullException()
        {
            var migrator = Substitute.For<IIniFileOperator>();
            var behaviour = new IniFileMigrateBehaviour("key", ".", ".", migrator);

            void Call() => behaviour.Invoke(null, null);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void Invoke_NotAffected_ReturnsUnchangedProperty()
        {
            // Setup
            var migrator = Substitute.For<IIniFileOperator>();
            var logHandler = Substitute.For<ILogHandler>();

            const string key = "key";
            const string value = "value";
            const string comment = "comment";

            var property = new IniProperty(key, value, comment);

            var behaviour = new IniFileMigrateBehaviour("notKey",
                                                             ".",
                                                             ".",
                                                             migrator);

            // Call
            behaviour.Invoke(property, logHandler);

            // Assert
            Assert.That(property.Key, Is.EqualTo(key));
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
            var migrator = Substitute.For<IIniFileOperator>();
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

                const string propertyKey = "key";
                const string propertyComment = "Comment";
                var property = new IniProperty(propertyKey, oldPath, propertyComment);

                var behaviour = new IniFileMigrateBehaviour(propertyKey,
                                                                 Path.Combine(tempDir.Path, subDirectory),
                                                                 goalDir,
                                                                 migrator);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Key, Is.EqualTo(propertyKey));
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
            var migrator = Substitute.For<IIniFileOperator>();
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

                const string propertyKey = "key";
                const string propertyComment = "Comment";
                var property = new IniProperty(propertyKey, absolutePath, propertyComment);

                var behaviour = new IniFileMigrateBehaviour(propertyKey,
                                                                 Path.Combine(tempDir.Path, subDirectory),
                                                                 goalDir,
                                                                 migrator);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Key, Is.EqualTo(propertyKey));
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
            var migrator = Substitute.For<IIniFileOperator>();

            migrator.Invoke(Arg.Do<Stream>(x => x.Dispose()),
                                 Arg.Any<string>(),
                                 Arg.Any<ILogHandler>());

            using (var tempDir = new TemporaryDirectory())
            {
                const string fileName = "itDontMeanAThing.txt";
                const string goalDirName = "dooWahDooWah";

                string oldPath = Path.Combine(tempDir.Path, fileName);
                string goalDir = tempDir.CreateDirectory(goalDirName);

                const string propertyKey = "key";
                const string propertyComment = "Comment";
                var property = new IniProperty(propertyKey, oldPath, propertyComment);

                var behaviour = new IniFileMigrateBehaviour(propertyKey,
                                                                 tempDir.Path,
                                                                 goalDir,
                                                                 migrator);

                // Call 
                behaviour.Invoke(property, logHandler);

                // Assert
                Assert.That(property.Key, Is.EqualTo(propertyKey));
                Assert.That(property.Comment, Is.EqualTo(propertyComment));
                Assert.That(property.Value, Is.EqualTo(oldPath));

                const string expectedString = "The file associated with property {0}, {1} at {2}, does not exist and thus is not migrated.";
                logHandler.Received(1).ReportWarningFormat(expectedString, propertyKey, fileName, oldPath);
            }
        }

        [Test]
        public void Invoke_Affected_PathEmpty_Skipped()
        {
            // Setup
            var logHandler = Substitute.For<ILogHandler>();
            var migrator = Substitute.For<IIniFileOperator>();

            const string key = "key";
            const string value = "";
            const string comment = "comment";

            var property = new IniProperty(key, value, comment);

            var behaviour = new IniFileMigrateBehaviour("key", ".", ".", migrator);

            // Call
            behaviour.Invoke(property, logHandler);

            // Assert
            Assert.That(property.Key, Is.EqualTo(key));
            Assert.That(property.Value, Is.EqualTo(value));
            Assert.That(property.Comment, Is.EqualTo(comment));

            Assert.That(logHandler.ReceivedCalls(), Is.Empty);
        }

        private static IEnumerable<TestCaseData> Constructor_ParameterNull_Data()
        {
            var migrator = Substitute.For<IIniFileOperator>();

            yield return new TestCaseData(null, ".", ".", migrator, "expectedKey");
            yield return new TestCaseData("key", null, ".", migrator, "relativeDirectory");
            yield return new TestCaseData("key", ".", null, migrator, "goalDirectory");
            yield return new TestCaseData("key", ".", ".", null, "migrator");
        }
    }
}