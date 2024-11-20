using System.ComponentModel;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public class ListBoxItemRemovingEventArgs : CancelEventArgs
    {
        public int Index { get; private set; }
        public object Item { get; private set; }

        public ListBoxItemRemovingEventArgs(object removedItem, int removedIndex)
        {
            Index = removedIndex;
            Item = removedItem;
        }
    }
}