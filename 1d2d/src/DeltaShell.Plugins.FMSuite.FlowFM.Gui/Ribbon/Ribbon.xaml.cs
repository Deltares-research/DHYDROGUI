using System.Collections.Generic;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui;
using Fluent;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    /// <summary>
    /// Interaction logic for Ribbon.xaml
    /// </summary>
    public partial class Ribbon : IRibbonCommandHandler
    {
        private readonly IDictionary<UIElement, ICommand> buttonCommands = new Dictionary<UIElement, ICommand>();
        
        private readonly ICommand importBedlevelFromFileCommand = new ImportBedlevelFromFileCommand();
        public Ribbon()
        {
            
            InitializeComponent();

            mapTab.Group = geospatialContextualGroup;
            importBedlevelFromFileCommand = new ImportBedlevelFromFileCommand();

            // with this call, the set label command is sent to the sharpmapgisguiplugin as its owner.
            // It's not added to the list of Commands.
            
            SharpMapGisGuiPlugin.Instance.AddSpatialOperationCommand(ButtonAddRasterSamples, importBedlevelFromFileCommand, typeof(ImportBedlevelFromFileCommand), Properties.Resources.add);

            ButtonReverseLine.ToolTip = new ScreenTip
            {
                Title = "Reverse line(s)",
                Text = "Reverses the selected poly-line features.",
                DisableReason = "Required to have exclusively 2D/3D oriented polyline features selected.",
                MaxWidth = 250,
            };
        }

        public object GetRibbonControl()
        {
            return RibbonControl;
        }
        public void ValidateItems()
        {
            ViewModelRegion.RefreshButtons();
            ViewModel1D2D.RefreshButtons();
        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            return false;
        }

        public IEnumerable<ICommand> Commands
        {
            get { return buttonCommands.Values; }
        }
        private void ButtonCreateRasterSamples_Click(object sender, RoutedEventArgs e)
        {
            importBedlevelFromFileCommand.Execute();
            ValidateItems();
        }
    }
}

 