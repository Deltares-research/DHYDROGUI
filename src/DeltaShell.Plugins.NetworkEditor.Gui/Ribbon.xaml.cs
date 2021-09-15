using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Forms;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.NetworkEditor.Gui.Layers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using Fluent;

namespace DeltaShell.Plugins.NetworkEditor.Gui
{
    internal static class ToggleButtonExtensions
    {
        public static void SetState(this ToggleButton button, ICommand command, bool visible = true)
        {
            button.IsEnabled = command.Enabled;
            button.IsChecked = command.Checked;
            button.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Interaction logic for Ribbon.xaml
    /// </summary>
    public partial class Ribbon : IRibbonCommandHandler
    {
        private ICommand addThinDam2dCommand = new MapToolCommand(HydroRegionEditorMapTool.ThinDamToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addFixedWeir2dCommand = new MapToolCommand(HydroRegionEditorMapTool.FixedWeirToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addObs2dCommand = new MapToolCommand(HydroRegionEditorMapTool.ObservationPointToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addObsCS2dCommand = new MapToolCommand(HydroRegionEditorMapTool.ObservationCrossSectionToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addPump2dCommand = new MapToolCommand(HydroRegionEditorMapTool.PumpToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addWeir2dCommand = new MapToolCommand(HydroRegionEditorMapTool.WeirToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addLandBoundary2dCommand = new MapToolCommand(HydroRegionEditorMapTool.LandBoundaryToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addDryPoint2dCommand = new MapToolCommand(HydroRegionEditorMapTool.DryPointToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addDryArea2dCommand = new MapToolCommand(HydroRegionEditorMapTool.DryAreaToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addEnclosure2dCommand = new MapToolCommand(HydroRegionEditorMapTool.EnclosureToolName) { LayerType = typeof(HydroAreaLayer) };
        private ICommand addBridgePillarCommand = new MapToolCommand(HydroRegionEditorMapTool.BridgePillarToolName) { LayerType = typeof(HydroAreaLayer) };

        public Ribbon()
        {
            InitializeComponent();
            tabRegion.Group = geospatialContextualGroup;
        }

        public Ribbon(IGui gui) : this()
        {
            Ensure.NotNull(gui, nameof(gui));
            gui.MainWindow.SetActiveRibbonTab("Home");
        }

        public IEnumerable<ICommand> Commands
        {
            get
            {
                yield return addThinDam2dCommand;
                yield return addFixedWeir2dCommand;
                yield return addObs2dCommand;
                yield return addObsCS2dCommand;
                yield return addPump2dCommand;
                yield return addWeir2dCommand;
                yield return addLandBoundary2dCommand;
                yield return addDryPoint2dCommand;
                yield return addDryArea2dCommand;
                yield return addEnclosure2dCommand;
                yield return addBridgePillarCommand;
            }
        }

        public void ValidateItems()
        {
            var showArea2DTools = true;

            // Area2d tools
            ButtonAddNewThinDam2D.SetState(addThinDam2dCommand, showArea2DTools);
            ButtonAddNewFixedWeir2D.SetState(addFixedWeir2dCommand, showArea2DTools);
            ButtonAddNewObsPoint2D.SetState(addObs2dCommand, showArea2DTools);
            ButtonAddNewObsCs2D.SetState(addObsCS2dCommand, showArea2DTools);
            ButtonAddNewPump2D.SetState(addPump2dCommand, showArea2DTools);
            ButtonAddNewWeir2D.SetState(addWeir2dCommand, showArea2DTools);
            ButtonAddNewLandBoundary2D.SetState(addLandBoundary2dCommand, showArea2DTools);
            ButtonAddNewDryPoint2D.SetState(addDryPoint2dCommand, showArea2DTools);
            ButtonAddNewDryArea2D.SetState(addDryArea2dCommand, showArea2DTools);
            ButtonAddNewEnclosure2D.SetState(addEnclosure2dCommand, showArea2DTools);
            ButtonAddBridgePillar.SetState(addBridgePillarCommand, showArea2DTools);
        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            if (tabGroupName == geospatialContextualGroup.Name && tabName == tabRegion.Name)
            {
                return IsActiveViewMapViewWithRegion();
            }

            return false;
        }

        public object GetRibbonControl()
        {
            return RibbonControl;
        }

        private bool IsActiveViewMapViewWithRegion()
        {
            MapView mapView = NetworkEditorGuiPlugin.GetFocusedMapView();

            if (mapView == null || mapView.Map == null)
            {
                return false;
            }

            if (mapView.Map.GetAllLayers(true).Any(l => l is HydroRegionMapLayer))
            {
                return true;
            }

            return false;
        }

        private void ButtonAddNewThinDam_Click(object sender, RoutedEventArgs e)
        {
            addThinDam2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewFixedWeir_Click(object sender, RoutedEventArgs e)
        {
            addFixedWeir2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewObsPoint2D_Click(object sender, RoutedEventArgs e)
        {
            addObs2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewObsCs2D_Click(object sender, RoutedEventArgs e)
        {
            addObsCS2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewPump2D_Click(object sender, RoutedEventArgs e)
        {
            addPump2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewWeir2D_Click(object sender, RoutedEventArgs e)
        {
            addWeir2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewLandBoundary_Click(object sender, RoutedEventArgs e)
        {
            addLandBoundary2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewDryPoint_Click(object sender, RoutedEventArgs e)
        {
            addDryPoint2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewDryArea_Click(object sender, RoutedEventArgs e)
        {
            addDryArea2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewEnclosure_Click(object sender, RoutedEventArgs e)
        {
            addEnclosure2dCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddBridgePillar_Click(object sender, RoutedEventArgs e)
        {
            addBridgePillarCommand.Execute();
            ValidateItems();
        }
    }
}