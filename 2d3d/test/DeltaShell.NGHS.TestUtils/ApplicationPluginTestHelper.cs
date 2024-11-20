using System;
using System.Linq;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.NGHS.TestUtils.Builders;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.TestUtils
{
    public static class ApplicationPluginTestHelper
    {
        public static void TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNotNull<TApplicationPlugin>(
            Func<DHYDROApplicationBuilder, DHYDROApplicationBuilder> buildApplication) where TApplicationPlugin : ApplicationPlugin
        {
            //Given
            using (IApplication app = buildApplication(new DHYDROApplicationBuilder()).Build())
            {
                app.Run();
                TApplicationPlugin applicationPlugin = app.Plugins.OfType<TApplicationPlugin>().Single();
                app.ProjectService.CreateProject();

                var compositeActivity = MockRepository.GenerateStub<ICompositeActivity>();

                // When
                ModelInfo modelInfos = applicationPlugin.GetModelInfos().FirstOrDefault();

                // Then
                Assert.NotNull(modelInfos);
                Assert.AreSame(compositeActivity, modelInfos.GetParentProjectItem(compositeActivity));
            }
        }

        public static void TestForGetParentProjectItemDelegateSetByApplicationPlugins_WhenApplicationPluginHelperReturnsNull<TApplicationPlugin>(
            Func<DHYDROApplicationBuilder, DHYDROApplicationBuilder> buildApplication) where TApplicationPlugin : ApplicationPlugin
        {
            // Given
            using (IApplication app = buildApplication(new DHYDROApplicationBuilder()).Build())
            {
                app.Run();
                TApplicationPlugin applicationPlugin = app.Plugins.OfType<TApplicationPlugin>().Single();
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