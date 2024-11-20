using System.Windows.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    /// <summary>
    /// Interaction logic for SedimentFractionsEditor.xaml
    /// </summary>
    public partial class SedimentFractionsEditor : UserControl
    {
        private readonly SedimentFractionsEditorViewModel viewModel = new SedimentFractionsEditorViewModel();

        public SedimentFractionsEditor(IEventedList<ISedimentFraction> sedimentFractions, IEventedList<ISedimentProperty> sedimentOverallProperties)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.ObjectModelSedimentFractions = sedimentFractions;
            viewModel.ObjectModelSedimentOverallProperties = sedimentOverallProperties;
        }
    }
}