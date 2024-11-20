using System;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public class ListBoxItemRemovedEventArgs: EventArgs
    {
        public object Value { get; set; }

        public int Index { get; set; }

        public ListBoxItemRemovedEventArgs(object value, int index)
        {
            Value = value;
            Index = index;
        }
    }
}