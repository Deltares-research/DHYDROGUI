using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public partial class TracerDefinitionsEditor : UserControl
    {
        public class TracerAddedEventArgs : EventArgs
        {
            public string Name { get; private set; }
            public bool Cancelled { get; set; }
            public string ErrorMessage { get; set; }

            public TracerAddedEventArgs(string name)
            {
                Name = name;
                Cancelled = false;
            }
        }

        public event Action<TracerAddedEventArgs> TracerAdded;
        public event Action<string> TracerRemoved;

        private IList<string> data; 
        public IList<string> Data
        {
            get { return data; }
            set
            {
                data = value;
                UpdateListBox();
            }
        }

        public TracerDefinitionsEditor()
        {
            InitializeComponent();

            itemsListBox.OnItemRemoved += listBox_ItemRemoved;
            itemsListBox.OnItemRemoving += listBox_ItemRemoving;
            newTracerTextBox.TextChanged += newTracer_TextChanged;
        }

        private void newTracer_TextChanged(object sender, EventArgs e)
        {
            errorProvider.SetError(newTracerTextBox, string.Empty);
        }

        private void listBox_ItemRemoving(object sender, ListBoxItemRemovingEventArgs e)
        {
            if (
                MessageBox.Show(
                    "Removing this tracer definition will remove all boundary conditions and initial tracers. All data will be lost. Continue?",
                    "All data will be lost", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.Cancel)
            {
                e.Cancel = true; // cancel the remove operation
            }
        }

        private void listBox_ItemRemoved(object sender, ListBoxItemRemovedEventArgs e)
        {
            if (TracerRemoved != null)
            {
                TracerRemoved((string) e.Value);
            }
        }

        public void UpdateListBox()
        {
            itemsListBox.Items.Clear();
            if (data != null)
            {
                itemsListBox.Items.AddRange(data.ToArray());
            }
        }

        private void addTracerDefinitionButton_Click(object sender, EventArgs clickEventArgs)
        {
            AddTracer();
            newTracerTextBox.Focus();
        }

        private void AddTracer()
        {
            TracerAddedEventArgs e = new TracerAddedEventArgs(newTracerTextBox.Text);
            if (TracerAdded != null)
            {
                TracerAdded(e);
            }

            if (!e.Cancelled)
            {
                errorProvider.SetError(addTracerDefinitionButton, string.Empty);
                newTracerTextBox.Text = string.Empty;
            }
            else
            {
                errorProvider.SetError(addTracerDefinitionButton, e.ErrorMessage);
            }
        }

        private void tracerTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char) Keys.Return)
            {
                e.Handled = true;

                AddTracer();
            }
        }
    }
}
