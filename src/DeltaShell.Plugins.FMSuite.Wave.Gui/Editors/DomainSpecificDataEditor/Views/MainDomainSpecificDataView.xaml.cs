using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Views
{
    /// <summary>
    /// Interaction logic for DomainSpecificDataEditor.xaml
    /// </summary>
    public partial class DomainSpecificDataEditor 
    {

        public DomainSpecificDataEditor(MainDomainSpecificDataViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
