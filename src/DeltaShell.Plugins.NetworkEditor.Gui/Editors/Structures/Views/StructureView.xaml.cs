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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && DataContext is IDisposable dataContext)
            {
                dataContext.Dispose();
            }
        }
    }
}