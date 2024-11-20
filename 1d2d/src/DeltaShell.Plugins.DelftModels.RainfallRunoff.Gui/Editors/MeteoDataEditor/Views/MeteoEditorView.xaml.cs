using System;
using DelftTools.Controls;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Views
{
    /// <summary>
    /// Interaction logic for MeteoEditorView.xaml
    /// </summary>
    public sealed partial class MeteoEditorView : IView
    {
        public MeteoEditorView()
        {
            InitializeComponent();
        }

        public object Data { get; set; }
        public string Text { get; set; }
        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        public void Dispose()
        {
            (Data as IDisposable)?.Dispose();
            (DataContext as IDisposable)?.Dispose();
        }

        public void EnsureVisible(object item)
        {
            // No specific object requires focus.
        }
    }
}
