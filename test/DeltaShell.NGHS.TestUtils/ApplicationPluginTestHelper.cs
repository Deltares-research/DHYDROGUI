using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.IntegrationTestUtils.Builders;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.TestUtils
{
    public static class ApplicationPluginTestHelper
    {
        public static void TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(ApplicationPlugin applicationPlugin)
        {
            //Given
            var pluginsToAdd = new List<IPlugin> { applicationPlugin };
            using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build() )
            {
                app.Run();
                app.ProjectService.CreateProject();

                var compositeActivity = MockRepository.GenerateStub<ICompositeActivity>();

                // When
                ModelInfo modelInfos = applicationPlugin.GetModelInfos().FirstOrDefault();

                // Then
                Assert.NotNull(modelInfos);
                Assert.AreSame(compositeActivity, modelInfos.GetParentProjectItem(compositeActivity));
            }
        }

        public static void TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(ApplicationPlugin applicationPlugin)
        {
            // Given
            var pluginsToAdd = new List<IPlugin> { applicationPlugin };
            using (var app = new DeltaShellApplicationBuilder().WithPlugins(pluginsToAdd).Build())
            {
                app.Run();
                Project project = app.ProjectService.CreateProject();
                
                // When
                ModelInfo modelInfos = applicationPlugin.GetModelInfos().FirstOrDefault();

                // Then
                Assert.NotNull(modelInfos);
                Assert.AreSame(project.RootFolder, modelInfos.GetParentProjectItem(null));
            }
        }
    }
}