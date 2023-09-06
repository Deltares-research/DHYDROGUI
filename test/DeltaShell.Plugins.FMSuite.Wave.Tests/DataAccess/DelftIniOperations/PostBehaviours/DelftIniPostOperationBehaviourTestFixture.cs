using System.Collections.Generic;
using System.IO;
using DeltaShell.NGHS.IO.Ini;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess.DelftIniOperations.PostBehaviours;
using DHYDRO.Common.Logging;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess.DelftIniOperations.PostBehaviours
{
    [TestFixture]
    public abstract class DelftIniPostOperationBehaviourTestFixture
    {
        protected abstract DelftIniPostOperationBehaviour ConstructPostBehaviour();

        public static IEnumerable<TestCaseData> GetInvokeNullData()
        {
            var stream = new MemoryStream();
            const string filePath = "some/toad/to/a/file.ini";
            var iniData = new IniData();

            yield return new TestCaseData(null, filePath, iniData, "sourceFileStream");
            yield return new TestCaseData(stream, null, iniData, "sourceFilePath");
            yield return new TestCaseData(stream, filePath, null, "iniData");
        }

        [Test]
        [TestCaseSource(nameof(GetInvokeNullData))]
        public void Invoke_ParameterNull_ThrowsArgumentNullException(Stream sourceFileStream, 
                                                                     string sourceFilePath, 
                                                                     IniData iniData,
                                                                     string expectedParameterName)
        {
            // Setup 
            DelftIniPostOperationBehaviour behaviour = ConstructPostBehaviour();
            var logHandler = Substitute.For<ILogHandler>();

            // Call | Assert
            void Call() => behaviour.Invoke(sourceFileStream, sourceFilePath, iniData, logHandler);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParameterName));
        }

        [Test]
        public void Invoke_SourceFilePathInvalidFilePath_ThrowsArgumentException()
        {
            // Setup
            var stream = new MemoryStream();
            const string filePath = "";
            var iniData = new IniData();
            var logHandler = Substitute.For<ILogHandler>();

            DelftIniPostOperationBehaviour behaviour = ConstructPostBehaviour();

            // Call | Assert
            void Call() => behaviour.Invoke(stream, filePath, iniData, logHandler);

            Assert.Throws<System.ArgumentException>(Call);
        }
    }
}