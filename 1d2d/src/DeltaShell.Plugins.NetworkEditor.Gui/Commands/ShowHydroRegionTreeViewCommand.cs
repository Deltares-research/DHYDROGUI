using DelftTools.Controls;
using DelftTools.Shell.Gui;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    class ShowHydroRegionTreeViewCommand:Command, IGuiCommand
    {
        protected override void OnExecute(params object[] arguments)
        {
            var view = NetworkEditorGuiPlugin.Instance.HydroRegionContents;
            var active = Gui.ToolWindowViews.Contains(view);

            if (active)
            {
                Gui.ToolWindowViews.Remove(view);
            }
            else
            {
                NetworkEditorGuiPlugin.Instance.InitializeHydroRegionTreeView();
            }
        }

        public override bool Checked
        {
            get
            {
                return Gui.ToolWindowViews != null && Gui.ToolWindowViews.Contains(NetworkEditorGuiPlugin.Instance.HydroRegionContents);
            }
        }

        public override bool Enabled
        {
            get { return true; }
        }

        public IGui Gui { get; set; }
    }
}