using System.Collections.Generic;
using System.IO;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.WaveOutputData
{
    [TestFixture]
    public class CollectPropertyValueWithDependentsBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var behaviour = new CollectPropertyValueWithDependentsBehaviour("somePropertyName",
                                                              "someRelativeDirectory",
                                                              Substitute.For<IDelftIniFileOperator>());

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IDelftIniPropertyBehaviour>());
        }

        public static IEnumerable<TestCaseData> GetConstructorParameterNullData()
        {
            const string propertyName = "someProperty";
            const string relativeDirectory = "someRelativeDirectory";
            var iniOperator = Substitute.For<IDelftIniFileOperator>();

            yield return new TestCaseData(null, relativeDirectory, iniOperator, "propertyName");
            yield return new TestCaseData(propertyName, null, iniOperator, "relativeDirectory");
            yield return new TestCaseData(propertyName, relativeDirectory, null, "iniFileOperator");
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(string propertyName, 
                                                                          string relativeDirectory,
                                                                          IDelftIniFileOperator iniFileOperator,
                                                                          string expectedParameterName)
        {
            // Call | Assert
            void Call() => new CollectPropertyValueWithDependentsBehaviour(propertyName, 
                                                                           relativeDirectory, 
                                                                           iniFileOperator);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Invoke_PropertyNull_ThrowsArgumentNullException()
        {
            // Setup
            var behaviour = new CollectPropertyValueWithDependentsBehaviour("somePropertyName", 
                                                                            "someRelativeDirectory",
                                                                            Substitute.For<IDelftIniFileOperator>());

            // Call | Assert
            void Call() => behaviour.Invoke(null, Substitute.For<ILogHandler>());

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void Invoke_PropertyMatches_CallsOperatorCorrectly()
        {
            // Setup
            using (var tempDir = new TemporaryDirectory())
            {
                const string propertyName = "propertyName";
                const string propertyValue = "someFile.loc";

                string relativeDirectory = tempDir.Path;
                tempDir.CreateFile(propertyValue);

                var iniFileOperator = Substitute.For<IDelftIniFileOperator>();

                var behaviour = new CollectPropertyValueWithDependentsBehaviour(propertyName,
                                                                                relativeDirectory,
                                                                                iniFileOperator);

                var property = new DelftIniProperty(propertyName, propertyValue, "someComment");
                var logHandler = Substitute.For<ILogHandler>();

                // Call
                behaviour.Invoke(property, logHandler);

                // Assert
                string fullPath = Path.Combine(relativeDirectory, propertyValue);
                iniFileOperator.Received(1).Invoke(Arg.Is<Stream>(x => x != null), 
                                                   fullPath, 
                                                   logHandler);
            }
        }

        [Test]
        public void Invoke_PropertyDoesNotMatch_DoesNotCallOperator()
        {
            // Setup
            const string propertyName = "propertyName";
            const string propertyValue = "someFile.loc";
            const string relativeDirectory = "someDirectory";

            var iniFileOperator = Substitute.For<IDelftIniFileOperator>();

            var behaviour = new CollectPropertyValueWithDependentsBehaviour("someOtherProperty", relativeDirectory, iniFileOperator);

            var property = new DelftIniProperty(propertyName, propertyValue, "someComment");

            // Call
            behaviour.Invoke(property, null);

            // Assert
            iniFileOperator.DidNotReceiveWithAnyArgs().Invoke(null, null, null);
        }
    }
}