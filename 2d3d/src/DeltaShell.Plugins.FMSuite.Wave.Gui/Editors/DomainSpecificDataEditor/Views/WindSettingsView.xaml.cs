using System.Windows;
using System.Windows.Controls;
using DelftTools.Controls;
using DelftTools.Controls.Wpf.Services;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

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

        private WindSettingsViewModel ViewModel => (WindSettingsViewModel)DataContext;

        private void OnClick_OpenXComponentFileButton(object sender, RoutedEventArgs e)
        {
            if (TryGetFileSelection("uniform x series (*.wnd;*.amu)|*.wnd;*.amu",
                                    out string filePath))
            {
                ViewModel.XComponentFilePath = filePath;
            }
        }

        private void OnClick_OpenYComponentFileButton(object sender, RoutedEventArgs e)
        {
            if (TryGetFileSelection("uniform y series (*.wnd;*.amv)|*.wnd;*.amv",
                                    out string filePath))
            {
                ViewModel.YComponentFilePath = filePath;
            }
        }

        private void OnClick_OpenSpiderWebFileButton(object sender, RoutedEventArgs e)
        {
            if (TryGetFileSelection("spider web (*.spw)|*.spw",
                                    out string filePath))
            {
                ViewModel.SpiderWebFilePath = filePath;
            }
        }

        private void OnClick_OpenWindVelocityFileButton(object sender, RoutedEventArgs e)
        {
            if (TryGetFileSelection("uniform xy series (*.wnd)|*.wnd",
                                    out string filePath))
            {
                ViewModel.WindVelocityFilePath = filePath;
            }
        }

        private static bool TryGetFileSelection(string fileFilter, out string selectedFilePath)
        {
            var fileDialogService = new FileDialogService();
            var fileDialogOptions = new FileDialogOptions { FileFilter = fileFilter };
            
            selectedFilePath = fileDialogService.ShowOpenFileDialog(fileDialogOptions);
            return selectedFilePath != null;
        }
    }
}