using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Layers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Fluent;
using NetTopologySuite.Extensions.Grids;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Ribbon
{
    /// <summary>
    /// Interaction logic for Ribbon.xaml
    /// </summary>
    public partial class Ribbon : IRibbonCommandHandler
    {
        private readonly IDictionary<ToggleButton, ICommand> buttonCommands = new Dictionary<ToggleButton, ICommand>();

        public Ribbon()
        {
            InitializeComponent();

            mapTab.Group = geospatialContextualGroup;

            buttonCommands.Add(ButtonAddObstacle, new MapToolCommand(WaveMapViewDecorator.ObstacleToolName));
            buttonCommands.Add(ButtonAddBoundary, new MapToolCommand(WaveMapViewDecorator.BoundaryToolName));
            buttonCommands.Add(ButtonAddObsPoint, new MapToolCommand(WaveMapViewDecorator.ObservationPointToolName));
            buttonCommands.Add(ButtonAddObsCrossSection, new MapToolCommand(WaveMapViewDecorator.ObservationCrossSectionToolName));
        }

        public IEnumerable<ICommand> Commands => buttonCommands.Values;

        public void ValidateItems()
        {
            MapView mapView = WaveGuiPlugin.ActiveMapView;

            ModelGroupLayer modelGroupLayer = null;
            if (mapView != null && mapView.Map != null)
            {
                modelGroupLayer =
                    mapView.Map.GetAllLayers(true).OfType<ModelGroupLayer>().FirstOrDefault(l => l.Model is WaveModel);
            }

            bool visible = modelGroupLayer != null;

            foreach (KeyValuePair<ToggleButton, ICommand> buttonCommandPair in buttonCommands)
            {
                ToggleButton button = buttonCommandPair.Key;
                ICommand command = buttonCommandPair.Value;

                if (Equals(button, ButtonAddBoundary) && modelGroupLayer != null)
                {
                    CurvilinearGrid curvilinearGrid = ((WaveModel) modelGroupLayer.Model).OuterDomain.Grid;
                    bool hasGrid = curvilinearGrid != null && !curvilinearGrid.IsEmpty;
                    bool boundaryDefinedByFile = ((WaveModel) modelGroupLayer.Model).BoundaryContainer.DefinitionPerFileUsed;
                    button.IsEnabled = hasGrid && !boundaryDefinedByFile && command.Enabled;
                }
                else
                {
                    button.IsEnabled = command.Enabled;
                }

                button.IsChecked = command.Checked;
                button.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
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
            buttonCommands[(ToggleButton) sender].Execute();
            ValidateItems();
        }
    }
}