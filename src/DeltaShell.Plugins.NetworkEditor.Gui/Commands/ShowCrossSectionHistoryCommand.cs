using DelftTools.Controls;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Commands
{
    /// <summary>
    /// Enables history data in <see cref="ICrossSectionHistoryCapableView"/>
    /// </summary>
    public class ShowCrossSectionHistoryCommand : Command
    {
        public override bool Enabled
        {
            get
            {
                return CrossSectionHistoryCapableView != null;
            }
        }

        public override bool Checked
        {
            get
            {
                return Enabled && CrossSectionHistoryCapableView.HistoryToolEnabled;
            }
        }

        protected override void OnExecute(params object[] arguments)
        {
            bool activeStatusTool = CrossSectionHistoryCapableView.HistoryToolEnabled;
            CrossSectionHistoryCapableView.HistoryToolEnabled = !activeStatusTool;
        }

        private static ICrossSectionHistoryCapableView CrossSectionHistoryCapableView
        {
            get
            {
                return NetworkEditorGuiPlugin.Instance.Gui != null
                           ? NetworkEditorGuiPlugin.Instance.Gui.DocumentViews.ActiveView as ICrossSectionHistoryCapableView
                           : null;
            }
        }
    }
}