using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.IntegrationTestUtils;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.TestUtils
{
    public static class ApplicationPluginTestHelper
    {
        public static void TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull(ApplicationPlugin applicationPlugin)
        {
            //Given
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                ApplicationPlugin appPlugin = applicationPlugin;

                appPlugin.Application = app;

                app.Run();
                app.CreateNewProject();

                var compositeActivity = MockRepository.GenerateStub<ICompositeActivity>();

                // When
                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();

                // Then
                Assert.NotNull(modelInfos);
                Assert.AreSame(compositeActivity, modelInfos.GetParentProjectItem(compositeActivity));
            }
        }

        public static void TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull(ApplicationPlugin applicationPlugin)
        {
            // Given
            using (var app = DeltaShellCoreFactory.CreateApplication())
            {
                ApplicationPlugin appPlugin = applicationPlugin;

                appPlugin.Application = app;
                
                app.Run();
                app.CreateNewProject();
                
                // When
                ModelInfo modelInfos = appPlugin.GetModelInfos().FirstOrDefault();

                // Then
                Assert.NotNull(modelInfos);
                Assert.AreSame(app.Project.RootFolder, modelInfos.GetParentProjectItem(null));
            }
        }
    }
}