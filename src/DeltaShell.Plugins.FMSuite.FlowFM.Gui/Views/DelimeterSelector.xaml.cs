using DelftTools.Controls;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Views
{
    /// <summary>
    /// Interaction logic for DelimeterSelector.xaml
    /// </summary>
    public partial class DelimeterSelector : IView
    {
        public DelimeterSelector()
        {
            InitializeComponent();
            if (ViewModel.CloseAction == null)
                ViewModel.CloseAction = result =>
                {
                    DialogResult = result;
                    Close();
                };
        }

        public object Data
        {
            get { return ViewModel.SelectedDelimeter; }
            set
            {
                ViewModel.SelectedDelimeter = (char) value;
                ViewModel.OnSetOptionChecked.Execute(null);
            }
        }

        public string Text { get; set; }
        public Image Image { get; set; }
        public void EnsureVisible(object item)
        {

        }

        public bool Visible { get { return true; } }
        public ViewInfo ViewInfo { get; set; }
        public void Dispose()
        {

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
    }
}
