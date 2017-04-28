using DelftTools.Controls;
using DeltaShell.Plugins.DeveloperTools.Builders;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    /// <summary>
    /// Creates 1D flow model with demo network
    /// </summary>
    public class CreateFlowModel1dDemoNetworkCommand : Command
    {
        protected override void OnExecute(params object[] arguments)
        {
            var model = WaterFlowModel1DBuilder.CreateModelWithDemoNetwork();

            var gui = DeveloperToolsGuiPlugin.Instance.Gui;
            var app = gui.Application;

            if (app.Project == null)
            {
                app.CreateNewProject();
            }

            app.Project.RootFolder.Add(model);
        }

        public override bool Enabled
        {
            get { return true; }
        }
    }
}
