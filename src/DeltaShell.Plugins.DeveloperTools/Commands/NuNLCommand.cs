using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DeveloperTools.Commands
{
    class NuNLCommand:Command
    {
        private bool enabled=true;

        protected override void OnExecute(params object[] arguments)
        {
            IGui gui = DeveloperToolsGuiPlugin.Instance.Gui;
            IApplication app = gui.Application;
            Url url = new Url("NU", "http://www.nu.nl");
            IDataItem dataItem = new DataItem(url);
            gui.Selection = dataItem;
            if (app.Project == null)
            {
                app.CreateNewProject();
            }
            app.Project.RootFolder.Add(dataItem);
            gui.CommandHandler.OpenViewForSelection();
        }

        public override bool Enabled
        {
            get { return enabled; }
        }
    }
}