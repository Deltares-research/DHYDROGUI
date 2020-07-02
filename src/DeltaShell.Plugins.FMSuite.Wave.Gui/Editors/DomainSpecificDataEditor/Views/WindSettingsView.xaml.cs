using System.Windows;
using System.Windows.Controls;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using Microsoft.Win32;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Views
{
    /// <summary>
    /// Interaction logic for WindSettingsView.xaml
    /// </summary>
    public partial class WindSettingsView : UserControl
    {
        public WindSettingsView()
        {
            InitializeComponent();
        }

        private WindSettingsViewModel ViewModel => (WindSettingsViewModel) DataContext;

        private void OnClick_OpenXComponentFileButton(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "uniform x series (*.wnd;*.amu)|*.wnd;*.amu",
                Title = "Select X component file"
            };

            if (fileDialog.ShowDialog() == true)
            {
                string selectedFilePath = fileDialog.FileName;
                ViewModel.XComponentFilePath = selectedFilePath;
            }
        }

        private void OnClick_OpenYComponentFileButton(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "uniform y series (*.wnd;*.amv)|*.wnd;*.amv",
                Title = "Select Y component file"
            };

            if (fileDialog.ShowDialog() == true)
            {
                string selectedFilePath = fileDialog.FileName;
                ViewModel.YComponentFilePath = selectedFilePath;
            }
        }

        private void OnClick_OpenSpiderWebFileButton(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "spider web (*.spw)|*.spw",
                Title = "Select spider web file"
            };

            if (fileDialog.ShowDialog() == true)
            {
                string selectedFilePath = fileDialog.FileName;
                ViewModel.SpiderWebFilePath = selectedFilePath;
            }
        }

        private void OnClick_OpenWindVelocityFileButton(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "uniform xy series (*.wnd)|*.wnd",
                Title = "Select wind velocity file"
            };

            if (fileDialog.ShowDialog() == true)
            {
                string selectedFilePath = fileDialog.FileName;
                ViewModel.WindVelocityFilePath = selectedFilePath;
            }
        }
    }
}