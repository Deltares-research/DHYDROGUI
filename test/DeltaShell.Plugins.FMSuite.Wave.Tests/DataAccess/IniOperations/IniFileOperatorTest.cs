using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.IniOperations
{
    [TestFixture]
    public class IniFileOperatorTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IIniReader>();
            var postOperations = new List<IIniPostOperationBehaviour>();

            // Call
            var iniFileOperator = new IniFileOperator(migrationBehaviourMapping, iniReader, postOperations);

            // Assert
            Assert.That(iniFileOperator, Is.InstanceOf<IIniFileOperator>());
        }

        [Test]
        [TestCaseSource(nameof(Constructor_ParameterNull_Data))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> migrationBehaviour,
                                                                          IIniReader iniReader,
                                                                          IList<IIniPostOperationBehaviour> postOperationBehaviours,
                                                                          string expectedParameterName)
        {
            void Call() => new IniFileOperator(migrationBehaviour, iniReader, postOperationBehaviours);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Invoke_SourceFileStreamNull_ThrowsArgumentNullException()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var iniFileOperator = new IniFileOperator(migrationBehaviourMapping, iniReader, new List<IIniPostOperationBehaviour>());

            // Call | Assert
            void Call() => iniFileOperator.Invoke(null,
                                                  "./imaginary/toad/to/src.ini",
                                                  logHandler);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("sourceFileStream"));
        }

        [Test]
        public void Invoke_SourceFilePath_ThrowsArgumentNullException()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var iniFileOperator = new IniFileOperator(migrationBehaviourMapping, iniReader, new List<IIniPostOperationBehaviour>());

            // Call | Assert
            void Call() => iniFileOperator.Invoke(new MemoryStream(),
                                                  null,
                                                  logHandler);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("sourceFilePath"));
        }

        [Test]
        public void Invoke_CallsBehavioursDefinedInMappingCorrectly()
        {
            // Setup
            // Paths
            var sourceFile = new MemoryStream();
            const string sourcePath = "./fromHere/soooooooooourceyFile.ini";

            // Ini properties
            const string sectionName = "sectionName";
            var section = new IniSection(sectionName);

            IniProperty[] properties =
                Enumerable.Range(0, 5)
                          .Select(i => new IniProperty($"someName_{i}", $"someValue_{i}", $"(someComment_{i}"))
                          .ToArray();
            IIniPropertyBehaviour[] propertyBehaviours =
                Enumerable.Range(0, 5)
                          .Select(_ => Substitute.For<IIniPropertyBehaviour>())
                          .ToArray();

            section.AddMultipleProperties(properties);

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> behaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>
                {
                    {
                        sectionName, new Dictionary<string, IIniPropertyBehaviour>
                        {
                            {properties[0].Key, propertyBehaviours[0]},
                            {properties[1].Key, propertyBehaviours[1]},
                            {properties[2].Key, propertyBehaviours[2]},
                            {properties[3].Key, propertyBehaviours[3]},
                            {properties[4].Key, propertyBehaviours[4]},
                        }
                    },
                };

            var iniData = new IniData();
            iniData.AddSection(section);

            var postOperations = new[]
            {
                Substitute.For<IIniPostOperationBehaviour>(),
                Substitute.For<IIniPostOperationBehaviour>(),
                Substitute.For<IIniPostOperationBehaviour>(),
            };

            var iniReader = Substitute.For<IIniReader>();
            iniReader.ReadIniFile(sourceFile, sourcePath).Returns(iniData);

            var iniFileOperator = new IniFileOperator(behaviourMapping, iniReader, postOperations);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            iniFileOperator.Invoke(sourceFile, sourcePath, logHandler);

            // Assert
            iniReader.Received(1).ReadIniFile(sourceFile, sourcePath);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);

            for (var i = 0; i < 5; i++)
            {
                propertyBehaviours[i].Received(1).Invoke(properties[i], logHandler);
                propertyBehaviours[i].DidNotReceive().Invoke(Arg.Is<IniProperty>(x => !x.Key.Equals(properties[i].Key)),
                                                             logHandler);
            }

            foreach (IIniPostOperationBehaviour postOperation in postOperations)
            {
                postOperation.Received(1).Invoke(sourceFile, sourcePath, iniData, logHandler);
            }
        }

        [Test]
        public void Invoke_SkipsPropertiesNotDefinedInMapping()
        {
            // Setup
            var sourceFile = new MemoryStream();

            const string sourcePath = "./fromHere/soooooooooourceyFile.ini";

            // Ini properties
            const string sectionName = "sectionName";
            var section = new IniSection(sectionName);

            const string propertyKey = "someKey";
            const string propertyValue = "someValue";
            const string propertyComment = "someComment";

            var property = new IniProperty(propertyKey, propertyValue, propertyComment);
            section.AddProperty(property);

            var iniData = new IniData();
            iniData.AddSection(section);

            var propertyBehaviour = Substitute.For<IIniPropertyBehaviour>();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> propertyBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>
                {
                    {
                        sectionName, new Dictionary<string, IIniPropertyBehaviour>
                        {
                            {"notSomeName", propertyBehaviour},
                        }
                    },
                };

            var iniReader = Substitute.For<IIniReader>();
            iniReader.ReadIniFile(sourceFile, sourcePath).Returns(iniData);

            var postOperations = new[]
            {
                Substitute.For<IIniPostOperationBehaviour>(),
                Substitute.For<IIniPostOperationBehaviour>(),
                Substitute.For<IIniPostOperationBehaviour>(),
            };

            var iniFileOperator = new IniFileOperator(propertyBehaviourMapping, iniReader, postOperations);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            iniFileOperator.Invoke(sourceFile, sourcePath, logHandler);

            // Assert
            iniReader.Received(1).ReadIniFile(sourceFile, sourcePath);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);

            propertyBehaviour.DidNotReceiveWithAnyArgs().Invoke(null, null);
            Assert.That(property.Key, Is.EqualTo(propertyKey));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.Comment, Is.EqualTo(propertyComment));
        }

        private static IEnumerable<TestCaseData> Constructor_ParameterNull_Data()
        {
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IIniReader>();
            var postOperations = new List<IIniPostOperationBehaviour>();

            yield return new TestCaseData(null, iniReader, postOperations, "categoryPropertyBehaviourMapping");
            yield return new TestCaseData(migrationBehaviourMapping, null, postOperations, "iniReader");
            yield return new TestCaseData(migrationBehaviourMapping, iniReader, null, "postOperations");
        }
    }
}