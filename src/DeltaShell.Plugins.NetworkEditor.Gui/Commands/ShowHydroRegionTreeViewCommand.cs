using DelftTools.Controls;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    internal class ShowHydroRegionTreeViewCommand : Command, IGuiCommand
    {
        public override bool Checked
        {
            get
            {
                return Gui.ToolWindowViews != null && Gui.ToolWindowViews.Contains(NetworkEditorGuiPlugin.Instance.HydroRegionContents);
            }
        }

        public override bool Enabled
        {
            get
            {
                return true;
            }
        }

        public IGui Gui { get; set; }

        protected override void OnExecute(params object[] arguments)
        {
            IView view = NetworkEditorGuiPlugin.Instance.HydroRegionContents;
            bool active = Gui.ToolWindowViews.Contains(view);

            if (active)
            {
                Gui.ToolWindowViews.Remove(view);
            }
            else
            {
                NetworkEditorGuiPlugin.Instance.InitializeHydroRegionTreeView();
            }
        }
    }
}