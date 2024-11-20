using DelftTools.Shell.Gui;
using DeltaShell.NGHS.TestUtils.Builders;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI
{
    internal static class RainfallRunoffIntegrationTestHelper
    {
        internal static IGui GetRunningGuiWithRRPlugins()
        {
            var deltaShell = new DHYDROGuiBuilder().WithRainfallRunoff().Build();
            deltaShell.Run();

            deltaShell.Application.ProjectService.CreateProject();

            return deltaShell;
        }
    }
}