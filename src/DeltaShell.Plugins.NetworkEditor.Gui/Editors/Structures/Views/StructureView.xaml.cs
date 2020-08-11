using System;
using System.Windows.Controls;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Editors.Structures.Views
{
    /// <summary>
    /// Interaction logic for StructureView.xaml
    /// </summary>
    public partial class StructureView : UserControl, IDisposable
    {
        public StructureView()
        {
            InitializeComponent();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && DataContext is IDisposable dataContext)
            {
                dataContext.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
