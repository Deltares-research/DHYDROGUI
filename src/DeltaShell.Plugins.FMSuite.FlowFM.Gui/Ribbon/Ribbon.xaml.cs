using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Common.Layers;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using Fluent;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Ribbon
{
    /// <summary>
    /// Interaction logic for Ribbon.xaml
    /// </summary>
    public partial class Ribbon : IRibbonCommandHandler
    {
        private readonly IDictionary<ButtonBase, ICommand> buttonCommands = new Dictionary<ButtonBase, ICommand>();

        public Ribbon()
        {
            InitializeComponent();

            mapTab.Group = geospatialContextualGroup;

            buttonCommands.Add(ButtonAddBoundary, new MapToolCommand(FlowFMMapViewDecorator.BoundaryToolName) { LayerType = typeof(AreaLayer) });
            buttonCommands.Add(ButtonAddSourceSink, new MapToolCommand(FlowFMMapViewDecorator.SourceAndSinkToolName) { LayerType = typeof(AreaLayer) });
            buttonCommands.Add(ButtonAddSource, new MapToolCommand(FlowFMMapViewDecorator.SourceToolName) { LayerType = typeof(AreaLayer) });
            buttonCommands.Add(ButtonReverseLine, new MapToolCommand(FlowFMMapViewDecorator.Reverse2DLineToolName) { LayerType = typeof(AreaLayer), ToolAction = ToolAction.Execute});
            buttonCommands.Add(ButtonGenerateEmbankments, new MapToolCommand(FlowFMMapViewDecorator.GenerateEmbankmentsToolName) { LayerType = typeof(AreaLayer), ToolAction = ToolAction.Execute});
            buttonCommands.Add(ButtonMergeEmbankments, new MapToolCommand(FlowFMMapViewDecorator.MergeEmbankmentsToolName) { LayerType = typeof(AreaLayer), ToolAction = ToolAction.Execute });
            buttonCommands.Add(ButtonGridWizard, new MapToolCommand(FlowFMMapViewDecorator.GridWizardToolName) { LayerType = typeof(AreaLayer) });

            ButtonReverseLine.ToolTip = new ScreenTip
                {
                    Title = "Reverse line(s)",
                    Text = "Reverses the selected poly-line features.",
                    DisableReason = "Required to have exclusively 2D/3D oriented polyline features selected.",
                    MaxWidth = 250,
                };
        }

        public IEnumerable<ICommand> Commands
        {
            get { return buttonCommands.Values; }
        }

        public void ValidateItems()
        {
            var mapView = FlowFMGuiPlugin.ActiveMapView;
            var visible = mapView != null && mapView.Map != null && mapView.Map.GetAllLayers(true).OfType<ModelGroupLayer>().Any(l => l.Model is WaterFlowFMModel);

            foreach (var buttonCommandPair in buttonCommands)
            {
                var button = buttonCommandPair.Key;
                var command = buttonCommandPair.Value;
                button.IsEnabled = command.Enabled;
                
                button.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

                var toggleButton = button as IToggleButton;
                if (toggleButton != null)
                    toggleButton.IsChecked = command.Checked;
            }
        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            if (tabName != "tabRegion")
                return false;

            // return true if any button is enabled on the tab
            return buttonCommands.Keys.Any(b => b.IsEnabled);
        }

        public object GetRibbonControl() { return RibbonControl; }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            buttonCommands[(ButtonBase)sender].Execute();
            ValidateItems();
        }
    }
}
