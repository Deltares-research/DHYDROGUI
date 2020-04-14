using System;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.ConditionDefinitions.ForcingTypeDefinedParameters
{
    [TestFixture]
    public class FileBasedParametersTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            const string expectedFilePath = "path/to/file.sp2";

            // Call
            var boundaryConditionParameters = new FileBasedParameters(expectedFilePath);

            // Assert
            Assert.That(boundaryConditionParameters, Is.InstanceOf<IForcingTypeDefinedParameters>());
            Assert.That(boundaryConditionParameters.FilePath, Is.EqualTo(expectedFilePath),
                        "Expected a different FilePath:");
        }

        [Test]
        public void Constructor_FilePathNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new FileBasedParameters(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("filePath"));
        }
    }
}