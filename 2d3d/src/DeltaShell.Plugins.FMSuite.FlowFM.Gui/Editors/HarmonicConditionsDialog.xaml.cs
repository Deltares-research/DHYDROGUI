using System.Windows;
using System.Windows.Controls;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    /// <summary>
    /// Interaction logic for HarmonicConditionsDialog.xaml
    /// </summary>
    public partial class HarmonicConditionsDialog
    {
        public HarmonicConditionsDialog(bool correctionsEnabled = false)
        {
            InitializeComponent();
            ViewModel.CorrectionsEnabled = correctionsEnabled;
        }

        private void OKButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void SelectAll(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
            {
                textBox.SelectAll();
            }
        }
    }
}