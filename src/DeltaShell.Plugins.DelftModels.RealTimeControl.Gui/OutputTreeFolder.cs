using System.Drawing;
using System.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    public class OutputTreeFolder 
    {
        public OutputTreeFolder(object parent, IEnumerable childItems, string text)
        {
            ChildItems = childItems;
            Text = text;
            Parent = parent;
        }

        public Image Image => Resources.folder_output;

        public string Text { get; }

        public virtual IEnumerable ChildItems { get; }
        
        public object Parent { get;}

    }
}