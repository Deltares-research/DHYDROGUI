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
            environment.GetVariable("PATH", EnvironmentVariableTarget.Process)
                       .Returns(somePath);

            // Call
            DimrApiDataSet.SetSharedPath(environment);

            // Assert
            string expectedValue = $"{somePath};{DimrApiDataSet.SharedDllPath}";
            environment.Received(1)
                       .SetVariable("PATH", expectedValue, EnvironmentVariableTarget.Process);
        }

        [Test]
        public void SetSharedPath_Contained_DoesNotAddSecondSharedPath()
        {
            // Setup
            string somePath = $"{DimrApiDataSet.SharedDllPath};bin;more/bin;super/more/bin";
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable("PATH", EnvironmentVariableTarget.Process)
                       .Returns(somePath);

            // Call
            DimrApiDataSet.SetSharedPath(environment);

            // Assert
            environment.DidNotReceiveWithAnyArgs()
                       .SetVariable(null, null, EnvironmentVariableTarget.Process);
        }
    }
}