using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.DelftIniObjects;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.WaveOutputData
{
    [TestFixture]
    public class CollectPropertyValueBehaviourTest
    {
        [Test]
        public void Constructor_ExpectedResults()
        {
            // Call
            var behaviour = new CollectPropertyValueBehaviour("somePropertyName",
                                                              new HashSet<string>(), 
                                                              "someRelativeDirectory");

            // Assert
            Assert.That(behaviour, Is.InstanceOf<IDelftIniPropertyBehaviour>());
        }

        public static IEnumerable<TestCaseData> GetConstructorParameterNullData()
        {
            const string propertyName = "someProperty";
            var hashSet = new HashSet<string>();
            const string relativeDirectory = "someRelativeDirectory";

            yield return new TestCaseData(null, hashSet, relativeDirectory, "propertyName");
            yield return new TestCaseData(propertyName, null, relativeDirectory, "hashSet");
            yield return new TestCaseData(propertyName, hashSet, null, "relativeDirectory");
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(string propertyName, 
                                                                          HashSet<string> hashSet,
                                                                          string relativeDirectory,
                                                                          string expectedParameterName)
        {
            // Call | Assert
            void Call() => new CollectPropertyValueBehaviour(propertyName, hashSet, relativeDirectory);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Invoke_PropertyNull_ThrowsArgumentNullException()
        {
            // Setup
            var behaviour = new CollectPropertyValueBehaviour("somePropertyName",
                                                              new HashSet<string>(), 
                                                              "someRelativeDirectory");

            // Call | Assert
            void Call() => behaviour.Invoke(null, Substitute.For<ILogHandler>());

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("property"));
        }

        [Test]
        public void Invoke_PropertyMatches_AddsElementToHashSet()
        {
            // Setup
            var hashSet = new HashSet<string>();
            const string propertyName = "propertyName";
            const string relativeDirectory = "relativePath";
            const string propertyValue = "someFile.loc";

            var behaviour = new CollectPropertyValueBehaviour(propertyName, hashSet, relativeDirectory);

            var property = new DelftIniProperty(propertyName, propertyValue, "someComment");

            // Call
            behaviour.Invoke(property, null);

            // Assert
            string expectedValue = Path.Combine(relativeDirectory, propertyValue);
            Assert.That(hashSet, Has.Member(expectedValue));
        }

        [Test]
        public void Invoke_PropertyDoesNotMatch_AddsElementToHashSet()
        {
            // Setup
            var hashSet = new HashSet<string>();
            const string propertyName = "propertyName";
            const string relativeDirectory = "relativePath";
            const string propertyValue = "someFile.loc";

            var behaviour = new CollectPropertyValueBehaviour("someOtherProperty", hashSet, relativeDirectory);

            var property = new DelftIniProperty(propertyName, propertyValue, "someComment");

            // Call
            behaviour.Invoke(property, null);

            // Assert
            string expectedValue = Path.Combine(relativeDirectory, propertyValue);
            Assert.That(hashSet, Has.No.Member(expectedValue));
        }
    }
}