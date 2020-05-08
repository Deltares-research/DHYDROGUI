using System;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView
{
    // TODO: make it real mock (use RhinoMock)
    internal class MockGui
    {
        private object selection;

        public event EventHandler SelectionChanged;

        public object Selection
        {
            get
            {
                return selection;
            }
            set
            {
                if (selection == value)
                {
                    return;
                }

                selection = value;

                if (SelectionChanged != null && selection != null)
                {
                    SelectionChanged(selection, new EventArgs());
                }
            }
        }
    }
}