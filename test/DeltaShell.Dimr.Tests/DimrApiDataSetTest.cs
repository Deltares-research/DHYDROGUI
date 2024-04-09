using DeltaShell.NGHS.Common;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrApiDataSetTest
    {
        [Test]
        public void AddKernelDirToPathToPath_NotContained_PathContainsKernelDir()
        {
            // Setup
            const string somePath = "bin;more/bin;super/more/bin;dev/null";

            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey)
                       .Returns(somePath);

            // Call
            DimrApiDataSet.AddKernelDirToPath(environment);

            // Assert
            var expectedValue = $"{DimrApiDataSet.KernelsLibDirectory};{somePath}";
            environment.Received(1)
                       .SetVariable(EnvironmentConstants.PathKey,
                                    expectedValue);
        }

        [Test]
        public void AddKernelDirToPathToPath_Contained_DoesNotAddDuplicateDir()
        {
            // Setup
            var somePath = $"bin;more/bin;super/more/bin;{DimrApiDataSet.KernelsLibDirectory}";
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey)
                       .Returns(somePath);

            // Call
            DimrApiDataSet.AddKernelDirToPath(environment);

            // Assert
            environment.DidNotReceiveWithAnyArgs()
                       .SetVariable(null, null);
        }

        [TestCase(null)]
        [TestCase("")]
        public void AddKernelDirToPathToPath_NullOrEmpty_SetsPathCorrectly(string returnValue)
        {
            // Setup
            var environment = Substitute.For<IEnvironment>();
            environment.GetVariable(EnvironmentConstants.PathKey)
                       .Returns(returnValue);

            // Call
            DimrApiDataSet.AddKernelDirToPath(environment);

            // Assert
            var expectedValue = $"{DimrApiDataSet.KernelsLibDirectory}";
            environment.Received(1)
                       .SetVariable(EnvironmentConstants.PathKey,
                                    expectedValue);
        }
    }
}