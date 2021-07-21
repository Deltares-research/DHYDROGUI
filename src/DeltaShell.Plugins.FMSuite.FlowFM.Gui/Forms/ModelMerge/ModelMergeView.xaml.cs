using System;
using System.Drawing;
using System.Windows;
using DelftTools.Controls;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Forms.ModelMerge
{
    public partial class ModelMergeView : Window, IDialog, IView
    {
        public ModelMergeView()
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
            if (disposing)
            {
                Image?.Dispose();
                ViewModel.Dispose();
            }
        }

        public void EnsureVisible(object item) { }

        public object Data { get; set; }
        public string Text { get; set; }
        public Image Image { get; set; }
        public ViewInfo ViewInfo { get; set; }

        public WaterFlowFMModel OriginalModel
        {
            get => ((ModelMergeViewModel) DataContext)?.OriginalModel;
            set => ((ModelMergeViewModel) DataContext).OriginalModel = value;
        }

        public DelftDialogResult ShowModal()
        {
            return ShowModal(null);
        }

        public DelftDialogResult ShowModal(object owner)
        {
            ShowDialog();
            return DialogResult.HasValue && DialogResult.Value
                       ? DelftDialogResult.OK
                       : DelftDialogResult.Cancel;
        }

        private void OnCloseButtonClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}