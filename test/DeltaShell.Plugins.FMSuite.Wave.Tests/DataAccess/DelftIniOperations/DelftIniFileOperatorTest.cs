using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeltaShell.NGHS.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.DelftIniOperations
{
    [TestFixture]
    public class DelftIniFileOperatorTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var postOperations = new List<IDelftIniPostOperationBehaviour>();

            // Call
            var iniFileOperator = new DelftIniFileOperator(migrationBehaviourMapping, iniReader, postOperations);

            // Assert
            Assert.That(iniFileOperator, Is.InstanceOf<IDelftIniFileOperator>());
        }

        [Test]
        [TestCaseSource(nameof(Constructor_ParameterNull_Data))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> migrationBehaviour,
                                                                          IDelftIniReader iniReader,
                                                                          IList<IDelftIniPostOperationBehaviour> postOperationBehaviours,
                                                                          string expectedParameterName)
        {
            void Call() => new DelftIniFileOperator(migrationBehaviour, iniReader, postOperationBehaviours);

            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Invoke_SourceFileStreamNull_ThrowsArgumentNullException()
        {
            // Setup
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var iniFileOperator = new DelftIniFileOperator(migrationBehaviourMapping, iniReader, new List<IDelftIniPostOperationBehaviour>());

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
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var logHandler = Substitute.For<ILogHandler>();

            var iniFileOperator = new DelftIniFileOperator(migrationBehaviourMapping, iniReader, new List<IDelftIniPostOperationBehaviour>());

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
            IDelftIniPropertyBehaviour[] propertyBehaviours =
                Enumerable.Range(0, 5)
                          .Select(_ => Substitute.For<IDelftIniPropertyBehaviour>())
                          .ToArray();

            section.AddMultipleProperties(properties);

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> behaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>
                {
                    {
                        sectionName, new Dictionary<string, IDelftIniPropertyBehaviour>
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
                Substitute.For<IDelftIniPostOperationBehaviour>(),
                Substitute.For<IDelftIniPostOperationBehaviour>(),
                Substitute.For<IDelftIniPostOperationBehaviour>(),
            };

            var iniReader = Substitute.For<IDelftIniReader>();
            iniReader.ReadDelftIniFile(sourceFile, sourcePath).Returns(iniData);

            var iniFileOperator = new DelftIniFileOperator(behaviourMapping, iniReader, postOperations);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            iniFileOperator.Invoke(sourceFile, sourcePath, logHandler);

            // Assert
            iniReader.Received(1).ReadDelftIniFile(sourceFile, sourcePath);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);

            for (var i = 0; i < 5; i++)
            {
                propertyBehaviours[i].Received(1).Invoke(properties[i], logHandler);
                propertyBehaviours[i].DidNotReceive().Invoke(Arg.Is<IniProperty>(x => !x.Key.Equals(properties[i].Key)),
                                                             logHandler);
            }

            foreach (IDelftIniPostOperationBehaviour postOperation in postOperations)
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

            var propertyBehaviour = Substitute.For<IDelftIniPropertyBehaviour>();

            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> propertyBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>
                {
                    {
                        sectionName, new Dictionary<string, IDelftIniPropertyBehaviour>
                        {
                            {"notSomeName", propertyBehaviour},
                        }
                    },
                };

            var iniReader = Substitute.For<IDelftIniReader>();
            iniReader.ReadDelftIniFile(sourceFile, sourcePath).Returns(iniData);

            var postOperations = new[]
            {
                Substitute.For<IDelftIniPostOperationBehaviour>(),
                Substitute.For<IDelftIniPostOperationBehaviour>(),
                Substitute.For<IDelftIniPostOperationBehaviour>(),
            };

            var iniFileOperator = new DelftIniFileOperator(propertyBehaviourMapping, iniReader, postOperations);
            var logHandler = Substitute.For<ILogHandler>();

            // Call
            iniFileOperator.Invoke(sourceFile, sourcePath, logHandler);

            // Assert
            iniReader.Received(1).ReadDelftIniFile(sourceFile, sourcePath);
            Assert.That(logHandler.ReceivedCalls(), Is.Empty);

            propertyBehaviour.DidNotReceiveWithAnyArgs().Invoke(null, null);
            Assert.That(property.Key, Is.EqualTo(propertyKey));
            Assert.That(property.Value, Is.EqualTo(propertyValue));
            Assert.That(property.Comment, Is.EqualTo(propertyComment));
        }

        private static IEnumerable<TestCaseData> Constructor_ParameterNull_Data()
        {
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>> migrationBehaviourMapping =
                new Dictionary<string, IReadOnlyDictionary<string, IDelftIniPropertyBehaviour>>();
            var iniReader = Substitute.For<IDelftIniReader>();
            var postOperations = new List<IDelftIniPostOperationBehaviour>();

            yield return new TestCaseData(null, iniReader, postOperations, "categoryPropertyBehaviourMapping");
            yield return new TestCaseData(migrationBehaviourMapping, null, postOperations, "iniReader");
            yield return new TestCaseData(migrationBehaviourMapping, iniReader, null, "postOperations");
        }
    }
}