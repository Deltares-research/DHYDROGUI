using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
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

            buttonCommands.Add(ButtonAddBoundary, new MapToolCommand(FlowFMMapViewDecorator.BoundaryToolName) { LayerType = typeof(HydroAreaLayer) });
            buttonCommands.Add(ButtonAddSourceSink, new MapToolCommand(FlowFMMapViewDecorator.SourceAndSinkToolName) { LayerType = typeof(HydroAreaLayer) });
            buttonCommands.Add(ButtonAddSource, new MapToolCommand(FlowFMMapViewDecorator.SourceToolName) { LayerType = typeof(HydroAreaLayer) });
            buttonCommands.Add(ButtonAddLateralPolygon, new MapToolCommand(FlowFMMapViewDecorator.LateralToolName) { LayerType = typeof(HydroAreaLayer) });
            buttonCommands.Add(ButtonAddLateralPoint, new MapToolCommand(FlowFMMapViewDecorator.LateralPointToolName) { LayerType = typeof(HydroAreaLayer) });
            buttonCommands.Add(ButtonReverseLine, new MapToolCommand(FlowFMMapViewDecorator.Reverse2DLineToolName) { LayerType = typeof(HydroAreaLayer) });

            ButtonReverseLine.ToolTip = new ScreenTip
            {
                Title = "Reverse line(s)",
                Text = "Reverses the selected poly-line features.",
                DisableReason = "Required to have exclusively 2D/3D oriented polyline features selected.",
                MaxWidth = 250
            };
        }

        public IEnumerable<ICommand> Commands => buttonCommands.Values;

        public void ValidateItems()
        {
            MapView mapView = FlowFMGuiPlugin.ActiveMapView;
            bool visible = mapView != null && mapView.Map != null && mapView.Map.GetAllLayers(true).OfType<ModelGroupLayer>().Any(l => l.Model is WaterFlowFMModel);

            foreach (KeyValuePair<ButtonBase, ICommand> buttonCommandPair in buttonCommands)
            {
                ButtonBase button = buttonCommandPair.Key;
                ICommand command = buttonCommandPair.Value;
                button.IsEnabled = command.Enabled;

                button.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

                if (button is IToggleButton toggleButton)
                {
                    toggleButton.IsChecked = command.Checked;
                }
            }
        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            if (tabName != "tabRegion")
            {
                return false;
            }

            // return true if any button is enabled on the tab
            return buttonCommands.Keys.Any(b => b.IsEnabled);
        }

        public object GetRibbonControl()
        {
            return RibbonControl;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            buttonCommands[(ButtonBase)sender].Execute();
            ValidateItems();
        }
    }
}