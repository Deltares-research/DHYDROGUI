using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.Helpers.WaveOutputData;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.IniOperations.PostBehaviours;
using DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.IniOperations.PostBehaviours;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.Helpers.WaveOutputData
{
    [TestFixture]
    public class CollectIniFileNamePostOperationBehaviourTest : IniPostOperationBehaviourTestFixture
    {
        protected override IniPostOperationBehaviour ConstructPostBehaviour() =>
            new CollectIniFileNamePostOperationBehaviour(new HashSet<string>(), "");

        [Test]
        public void Constructor_ExpectedResults()
        {
            var behaviour = new CollectIniFileNamePostOperationBehaviour(new HashSet<string>(), "");

            Assert.That(behaviour, Is.InstanceOf<IIniPostOperationBehaviour>());
        }

        public static IEnumerable<TestCaseData> GetConstructorParameterNullData()
        {
            yield return new TestCaseData(null, "someRelativeDirectory", "hashSet");
            yield return new TestCaseData(new HashSet<string>(), null, "relativeDirectory");
        }

        [Test]
        [TestCaseSource(nameof(GetConstructorParameterNullData))]
        public void Constructor_ParameterNull_ThrowsArgumentNullException(HashSet<string> hashSet, 
                                                                          string relativeDirectory,
                                                                          string expectedParameterName)
        {
            // Call | Assert
            void Call() => new CollectIniFileNamePostOperationBehaviour(hashSet, relativeDirectory);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Invoke_AddsSourceFilePathToHashSet()
        {
            // Setup
            var hashSet = new HashSet<string>();
            const string relativeDirectory = "someRelativeDirectory";
            const string fileName = "fileName.ini";

            var behaviour = new CollectIniFileNamePostOperationBehaviour(hashSet, relativeDirectory);

            // Call
            behaviour.Invoke(Stream.Null, fileName, new IniData(), null);

            // Assert
            string expectedPath = Path.Combine(relativeDirectory, fileName);
            Assert.That(hashSet, Has.Member(expectedPath));
        }
    }
}