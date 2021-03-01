using System;
using DeltaShell.NGHS.Common;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrApiDataSetTest
    {
        [Test]
        public void SetSharedPath_NotContained_PathContainsSharedPathAtTheEnd()
        {
            // Setup
            const string somePath = "bin;more/bin;super/more/bin;dev/null";

            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey,
                                    EnvironmentVariableTarget.Process)
                       .Returns(somePath);

            // Call
            DimrApiDataSet.SetSharedPath(environment);

            // Assert
            var expectedValue = $"{DimrApiDataSet.SharedDllPath};{somePath}";
            environment.Received(1)
                       .SetVariable(EnvironmentConstants.PathKey,
                                    expectedValue,
                                    EnvironmentVariableTarget.Process);
        }

        [Test]
        public void SetSharedPath_Contained_DoesNotAddSecondSharedPath()
        {
            // Setup
            var somePath = $"bin;more/bin;super/more/bin;{DimrApiDataSet.SharedDllPath}";
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey,
                                    EnvironmentVariableTarget.Process)
                       .Returns(somePath);

            // Call
            DimrApiDataSet.SetSharedPath(environment);

            // Assert
            environment.DidNotReceiveWithAnyArgs()
                       .SetVariable(null, null, EnvironmentVariableTarget.Process);
        }

        [TestCase(null)]
        [TestCase("")]
        public void SetSharedPath_NullOrEmpty_SetsPathCorrectly(string returnValue)
        {
            // Setup
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey,
                                    EnvironmentVariableTarget.Process)
                       .Returns(returnValue);

            // Call
            DimrApiDataSet.SetSharedPath(environment);

            // Assert
            var expectedValue = $"{DimrApiDataSet.SharedDllPath}";
            environment.Received(1)
                       .SetVariable(EnvironmentConstants.PathKey,
                                    expectedValue,
                                    EnvironmentVariableTarget.Process);
        }
    }
}