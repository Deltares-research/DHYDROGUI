using DelftTools.Shell.Core.Dao;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    [TestFixture]
    public class WaterFlowFMDataAccessListenerTest
    {
        [Test]
        public void WhenLoadingLegacyProjectAndOutputIsClearedThenLogClearing()
        {
            // Arrange
            var projectRepository = Substitute.For<IProjectRepository>();
            projectRepository.IsLegacyProject(Arg.Any<string>()).ReturnsForAnyArgs(true);
            var waterFlowFMDataAccessListener = new WaterFlowFMDataAccessListener(projectRepository);
            var waterFlowFMModel = new WaterFlowFMModel();
            string[] propertyNames = {};
            object[] state = {};

            //Act & Assert
            void Call() => waterFlowFMDataAccessListener.OnPostLoad(waterFlowFMModel, state, propertyNames);
            TestHelper.AssertLogMessageIsGenerated(Call, Resources.WaterFlowFMDataAccessListener_OnPostLoad_Model_output_is_removed_because_project_has_been_migrated_from_older_project_version);
        }
    }
}