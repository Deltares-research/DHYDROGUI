using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Views
{
    /// <summary>
    /// Interaction logic for MainDomainSpecificDataView.xaml
    /// </summary>
    public partial class MainDomainSpecificDataView 
    {
        public MainDomainSpecificDataView(MainDomainSpecificDataViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
