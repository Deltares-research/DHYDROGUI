using System.Collections;
using System.Drawing;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    /// <summary>
    /// OutputTreeFolder for showing all output files. Different from <see cref="DelftTools.Shell.Gui.Swf.TreeFolder"/>,
    /// so that it is clear that the object is used for output without searching if it contains output data items. RTC
    /// doesn't contain these data items and therefore the <see cref="ProjectExplorer.NodePresenters.TreeFolderNodePresenter"/>
    /// is not able to show exclamations if the output is out of sync.
    /// </summary>
    public sealed class OutputTreeFolder 
    {
        public OutputTreeFolder(IModel parent, IEnumerable childItems, string text)
        {
            ChildItems = childItems;
            Text = text;
            Parent = parent;
        }

        public Image Image => Resources.folder_output;

        public string Text { get; }

        public IEnumerable ChildItems { get; }
        
        public IModel Parent { get;}
    }
}