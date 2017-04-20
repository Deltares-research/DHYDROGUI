using System.Collections.Generic;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.DeveloperTools.Commands;
using DeltaShell.Plugins.DeveloperTools.Commands.IntegratedDemoModels;

namespace DeltaShell.Plugins.DeveloperTools
{
    /// <summary>
    /// Interaction logic for Ribbon.xaml
    /// </summary>
    public partial class Ribbon : IRibbonCommandHandler
    {
        private ICommand createTestNetworkWithMapCommand;
        private ICommand createFlowModel1dDemoNetworkCommand;
        private ICommand createFlowModel1DRectangleNetworkCommand;
        private ICommand importNetworkDataCommand;
        private ICommand nuNLCommand;
        private ICommand collectMemoryCommand;
        private ICommand allocateCommand;
        private ICommand showMemoryUsageCommand;
        private ICommand addWaterQualityModel1DAcceptanceModel1Command;
        private ICommand addWaterQualityModel1DAcceptanceModel2Command;
        
        private ICommand addWaterQualityModel1DIntegratedModelAcceptanceModel1Command;
        private ICommand addIntegratedModelDWAQ_AC2aModelCommand;
        private ICommand addIntegratedModelDWAQ_AC2aRtcVariationModelCommand;
        private ICommand addIntegratedModel1d2d;

        private ICommand addProjectedNetwork;

        public Ribbon()
        {
            InitializeComponent();

            createTestNetworkWithMapCommand = new CreateTestNetworkAndMapCommand();
            createFlowModel1dDemoNetworkCommand = new CreateFlowModel1dDemoNetworkCommand();
            createFlowModel1DRectangleNetworkCommand = new CreateFlowModel1DRectangleNetworkCommand();
            importNetworkDataCommand = new ImportNetworkAndAddToMapCommand();
            nuNLCommand = new NuNLCommand();
            collectMemoryCommand = new CollectMemoryCommand();
            allocateCommand = new AllocateTooMuchMemoryCommand();
            showMemoryUsageCommand = new ShowProcessMemoryUsageCommand();

            addWaterQualityModel1DIntegratedModelAcceptanceModel1Command = new AddWaterQualityModel1DIntegratedModelAcceptanceModel1Command();
            addIntegratedModelDWAQ_AC2aModelCommand = new AddIntegratedModelDWAQ_AC2aModelCommand();
            addIntegratedModelDWAQ_AC2aRtcVariationModelCommand = new AddIntegratedModelDWAQ_AC2aRtcVariationModelCommand();
            addIntegratedModel1d2d = new AddIntegratedModel1d2d();
            addProjectedNetwork = new AddProjectedNetwork();
        }

        public IEnumerable<ICommand> Commands
        {
            get
            {
                yield return createTestNetworkWithMapCommand;
                yield return createFlowModel1dDemoNetworkCommand;
                yield return createFlowModel1DRectangleNetworkCommand;
                yield return importNetworkDataCommand;
                yield return nuNLCommand;
                yield return collectMemoryCommand;
                yield return allocateCommand;
                yield return showMemoryUsageCommand;
                yield return addWaterQualityModel1DAcceptanceModel1Command;
                yield return addWaterQualityModel1DAcceptanceModel2Command;
                
                yield return addWaterQualityModel1DIntegratedModelAcceptanceModel1Command;
                yield return addIntegratedModelDWAQ_AC2aModelCommand;
                yield return addIntegratedModelDWAQ_AC2aRtcVariationModelCommand;
                yield return addIntegratedModel1d2d;
                yield return addProjectedNetwork;
            }
        }

        public void ValidateItems()
        {
        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            return false;
        }

        public object GetRibbonControl() { return RibbonControl; }

        private void ButtonCreateTestNetworkWithMap_Click(object sender, RoutedEventArgs e)
        {
            createTestNetworkWithMapCommand.Execute();
        }

        private void ButtonCreateFlowModel1dDemoNetworkCommand_Click(object sender, RoutedEventArgs e)
        {
            createFlowModel1dDemoNetworkCommand.Execute();
        }

        private void ButtonCreateFlowModel1DRectangleNetworkCommand_Click(object sender, RoutedEventArgs e)
        {
            createFlowModel1DRectangleNetworkCommand.Execute();
        }

        private void ButtonImportNetwork_Click(object sender, RoutedEventArgs e)
        {
            importNetworkDataCommand.Execute();
        }

        private void ButtonNuNL_Click(object sender, RoutedEventArgs e)
        {
            nuNLCommand.Execute();
        }

        private void ButtonCollectMemory_Click(object sender, RoutedEventArgs e)
        {
            collectMemoryCommand.Execute();
        }

        private void ButtonAllocate_Click(object sender, RoutedEventArgs e)
        {
            allocateCommand.Execute();
        }

        private void ButtonShowMemoryUsage_Click(object sender, RoutedEventArgs e)
        {
            showMemoryUsageCommand.Execute();
        }

        private void ButtonAddWaterQualityModel1DAcceptanceModel1Command_Click(object sender, RoutedEventArgs e)
        {
            addWaterQualityModel1DAcceptanceModel1Command.Execute();
        }

        private void ButtonAddWaterQualityModel1DAcceptanceModel2Command_Click(object sender, RoutedEventArgs e)
        {
            addWaterQualityModel1DAcceptanceModel2Command.Execute();
        }

        private void ButtonWaqAcceptenceModel1IntegratedModel_Click(object sender, RoutedEventArgs e)
        {
            addWaterQualityModel1DIntegratedModelAcceptanceModel1Command.Execute();
        }

        private void ButtonDWAQ_AC2a_Click(object sender, RoutedEventArgs e)
        {
            addIntegratedModelDWAQ_AC2aModelCommand.Execute();
        }

        private void ButtonDWAQ_AC2a_rtc_Click(object sender, RoutedEventArgs e)
        {
            addIntegratedModelDWAQ_AC2aRtcVariationModelCommand.Execute();
        }

        private void Button1d2d_Click(object sender, RoutedEventArgs e)
        {
            addIntegratedModel1d2d.Execute();
        }

        private void ButtonAddProjectedNetwork_Click(object sender, RoutedEventArgs e)
        {
            addProjectedNetwork.Execute();
        }
    }
}