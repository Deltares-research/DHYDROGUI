using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Hydro;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.NetworkEditor.Gui.Commands;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.MapTools;
using DeltaShell.Plugins.NetworkEditor.MapLayers;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using Fluent;
using SharpMap.Layers;
using MessageBox = System.Windows.MessageBox;

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
        private ICommand showHydroRegionContentsCommand = new ShowHydroRegionTreeViewCommand();
        private ICommand addNewBranchCommand = new AddNewBranchCommand();
        private ICommand addNewBranchScribbleCommand = new AddNewBranchUsingScribbleModeCommand();
        private ICommand insertNewNodeCommand = new InsertNewNodeCommand();
        private ICommand addNewPipeCommand = new AddNewPipeCommand();
        private ICommand addNewSewerConnectionCommand = new AddNewSewerConnectionCommand();
        private ICommand splitPipeCommand = new SplitPipeCommand();
        private ICommand addNewCrossSectionYZCommand = new AddNewCrossSectionYZCommand();
        private ICommand addNewCrossSectionZWCommand = new AddNewCrossSectionZWCommand();
        private ICommand addNewCrossSectionXYZCommand= new AddNewCrossSectionXYZCommand();
        private ICommand addNewCrossSectionStandardCommand = new AddNewCrossSectionStandardCommand();
        private ICommand addNewCrossSectionDefaultCommand = new AddNewDefaultCrossSectionCommand();
        private ICommand addNewCrossSectionInterpolatedCommand = new AddInterpolatedCrossSectionCommand();
        private ICommand addNewPumpCommand = new AddNewPumpCommand();
        private ICommand addNewWeirCommand = new AddNewWeirCommand();
        private ICommand addNewOrificeCommand = new AddNewOrificeCommand();
        private ICommand addNewCulvertCommand = new AddNewCulvertCommand();
        private ICommand addNewBridgeCommand = new AddNewBridgeCommand();
        private ICommand addNewLateralSourceCommand = new AddNewLateralSourceCommand();
        private ICommand addNewRetentionCommand = new AddNewRetentionCommand();
        private ICommand addNewObservationPointCommand = new AddNewObservationPointCommand();
        private ICommand addNewRouteCommand = new AddNewNetworkRouteCommand();
        private ICommand showCrossSectionHistoryCommand = new ShowCrossSectionHistoryCommand();
        private ICommand removeRouteCommand = new RemoveSelectedRouteCommand();
        private ICommand addNewCatchmentPavedCommand = new AddNewCatchmentCommand.AddNewPavedCommand();
        private ICommand addNewCatchmentUnpavedCommand = new AddNewCatchmentCommand.AddNewUnpavedCommand();
        private ICommand addNewCatchmentOpenWaterCommand = new AddNewCatchmentCommand.AddNewOpenWaterCommand();
        private ICommand addNewCatchmentGreenHouseCommand = new AddNewCatchmentCommand.AddNewGreenHouseCommand();
        private ICommand addNewCatchmentSacramentoCommand = new AddNewCatchmentCommand.AddNewSacramentoCommand();
        private ICommand addNewCatchmentHbvCommand = new AddNewCatchmentCommand.AddNewHbvCommand();
        private ICommand addNewWasteWaterTreatmentPlantCommand = new AddNewWasteWaterTreatmentPlantCommand();
        private ICommand addNewRunoffBoundaryCommand = new AddNewRunoffBoundaryCommand();
        private ICommand addNewLinkCommand = new AddNewLinkCommand();
        private ICommand addNewNetworkLocationCommand = new AddNewNetworkLocationCommand();
        private ICommand showSideViewCommand = new ShowSideViewCommand();
        private ICommand openCaseAnalysisCommand = new OpenCaseAnalysisViewCommand();

        private ICommand addThinDam2dCommand = new MapToolCommand(HydroRegionEditorMapTool.ThinDamToolName){LayerType = typeof(AreaLayer)};
        private ICommand addFixedWeir2dCommand = new MapToolCommand(HydroRegionEditorMapTool.FixedWeirToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addObs2dCommand = new MapToolCommand(HydroRegionEditorMapTool.ObservationPointToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addObsCS2dCommand = new MapToolCommand(HydroRegionEditorMapTool.ObservationCrossSectionToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addPump2dCommand = new MapToolCommand(HydroRegionEditorMapTool.PumpToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addWeir2dCommand = new MapToolCommand(HydroRegionEditorMapTool.WeirToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addGate2dCommand = new MapToolCommand(HydroRegionEditorMapTool.GateToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addLandBoundary2dCommand = new MapToolCommand(HydroRegionEditorMapTool.LandBoundaryToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addDryPoint2dCommand = new MapToolCommand(HydroRegionEditorMapTool.DryPointToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addDryArea2dCommand = new MapToolCommand(HydroRegionEditorMapTool.DryAreaToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addNewLeveeBreachCommand = new MapToolCommand(HydroRegionEditorMapTool.LeveeBreachToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addNewEmbankmentCommand = new MapToolCommand(HydroRegionEditorMapTool.EmbankmentToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addRoofAreaCommand = new MapToolCommand(HydroRegionEditorMapTool.RoofAreaToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addGullyCommand = new MapToolCommand(HydroRegionEditorMapTool.GullyToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addEnclosure2dCommand = new MapToolCommand(HydroRegionEditorMapTool.EnclosureToolName) { LayerType = typeof(AreaLayer) };
        private ICommand addBridgePillarCommand = new MapToolCommand(HydroRegionEditorMapTool.BridgePillarToolName) {LayerType = typeof(AreaLayer)};

        private readonly IRouteSelectionFinder routeSelectionFinder = new RouteSelectionFinder();

        public Ribbon()
        {
            InitializeComponent();
            tabRegion.Group = geospatialContextualGroup;
            tabCrossSectionTools.Group = crossSectionContextualGroup;
        }

        public IEnumerable<ICommand> Commands
        {
            get
            {
                yield return showHydroRegionContentsCommand;
                yield return addNewBranchCommand;
                yield return addNewPipeCommand;
                yield return addNewSewerConnectionCommand;
                yield return addNewBranchScribbleCommand;
                yield return insertNewNodeCommand;
                yield return addNewCrossSectionYZCommand;
                yield return addNewCrossSectionZWCommand;
                yield return addNewCrossSectionXYZCommand;
                yield return addNewCrossSectionStandardCommand;
                yield return addNewCrossSectionDefaultCommand;
                yield return addNewCrossSectionInterpolatedCommand;
                yield return addNewPumpCommand;
                yield return addNewWeirCommand;
                yield return addNewOrificeCommand;
                yield return addNewCulvertCommand;
                yield return addNewBridgeCommand;
                yield return addNewLateralSourceCommand;
                yield return addNewRetentionCommand;
                yield return addNewObservationPointCommand;
                yield return addNewRouteCommand;
                yield return showCrossSectionHistoryCommand;
                yield return removeRouteCommand;
                yield return addNewCatchmentPavedCommand;
                yield return addNewCatchmentUnpavedCommand;
                yield return addNewCatchmentOpenWaterCommand;
                yield return addNewCatchmentGreenHouseCommand;
                yield return addNewCatchmentSacramentoCommand;
                yield return addNewCatchmentHbvCommand;
                yield return addNewWasteWaterTreatmentPlantCommand;
                yield return addNewRunoffBoundaryCommand;
                yield return addThinDam2dCommand;
                yield return addFixedWeir2dCommand;
                yield return addObs2dCommand;
                yield return addObsCS2dCommand;
                yield return addPump2dCommand;
                yield return addWeir2dCommand;
                yield return addGate2dCommand;
                yield return addLandBoundary2dCommand;
                yield return addDryPoint2dCommand;
                yield return addDryArea2dCommand;
                yield return addNewLeveeBreachCommand;
                yield return addRoofAreaCommand;
                yield return addGullyCommand;
                yield return addNewLinkCommand;
                yield return addNewNetworkLocationCommand;
                yield return showSideViewCommand;
                yield return openCaseAnalysisCommand;
                yield return addNewEmbankmentCommand;
                yield return addEnclosure2dCommand;
                yield return addBridgePillarCommand;
            }
        }

        public void ValidateItems()
        {
            var mapview = NetworkEditorGuiPlugin.GetFocusedMapView();
            var regions = (mapview != null && mapview.Map != null
                ? mapview.Map.GetAllLayers(true).OfType<HydroRegionMapLayer>().Select(l => l.Region)
                : Enumerable.Empty<IHydroRegion>()).ToList();

            var showNetworkTools = regions.OfType<IHydroNetwork>().Any();
            var showBasinTools = regions.OfType<IDrainageBasin>().Any();
            var showArea2DTools = true;

            ButtonShowHydroRegionContents.SetState(showHydroRegionContentsCommand);

            // branch tools
            ButtonAddNewBranch.SetState(addNewBranchCommand, showNetworkTools);
            ButtonAddNewBranchScribble.SetState(addNewBranchScribbleCommand, showNetworkTools);
            ButtonInsertNewNode.SetState(insertNewNodeCommand, showNetworkTools);

            // sewer network tools
            ButtonAddNewPipe.SetState(addNewPipeCommand, showNetworkTools);
            ButtonInsertManhole.SetState(splitPipeCommand, showNetworkTools);
            ButtonAddNewSewerConnection.SetState(addNewSewerConnectionCommand, showNetworkTools);

            // crossSection tools
            ButtonAddNewCrossSectionYZ.SetState(addNewCrossSectionYZCommand, showNetworkTools);
            ButtonAddNewCrossSectionZW.SetState(addNewCrossSectionZWCommand, showNetworkTools);
            ButtonAddNewCrossSectionXYZ.SetState(addNewCrossSectionXYZCommand, showNetworkTools);
            ButtonAddNewCrossSectionStandard.SetState(addNewCrossSectionStandardCommand, showNetworkTools);
            ButtonAddNewCrossSectionDefault.SetState(addNewCrossSectionDefaultCommand, showNetworkTools);
            ButtonAddNewCrossSectionInterpolated.SetState(addNewCrossSectionInterpolatedCommand, showNetworkTools);

            // structure tools
            ButtonAddNewPump.SetState(addNewPumpCommand, showNetworkTools);
            ButtonAddNewWeir.SetState(addNewWeirCommand, showNetworkTools);
            ButtonAddNewOrifice.SetState(addNewOrificeCommand, showNetworkTools);
            ButtonAddNewCulvert.SetState(addNewCulvertCommand, showNetworkTools);
            ButtonAddNewBridge.SetState(addNewBridgeCommand, showNetworkTools);
            ButtonAddNewLateralSource.SetState(addNewLateralSourceCommand, showNetworkTools);
            ButtonAddNewRetention.SetState(addNewRetentionCommand, showNetworkTools);
            ButtonAddNewObservationPoint.SetState(addNewObservationPointCommand, showNetworkTools);
            
            ButtonAddNewRoute.SetState(addNewRouteCommand, showNetworkTools);
            ButtonShowCrossSectionHistory.SetState(showCrossSectionHistoryCommand);
            
            ButtonRemoveRoute.IsEnabled = removeRouteCommand.Enabled;
            ButtonRemoveRoute.SetState(removeRouteCommand, showNetworkTools);
            
            // catchment tools
            ButtonAddNewCatchmentPaved.SetState(addNewCatchmentPavedCommand, showBasinTools);
            ButtonAddNewCatchmentUnpaved.SetState(addNewCatchmentUnpavedCommand, showBasinTools);
            ButtonAddNewCatchmentOpenWater.SetState(addNewCatchmentOpenWaterCommand, showBasinTools);
            ButtonAddNewCatchmentGreenHouse.SetState(addNewCatchmentGreenHouseCommand, showBasinTools);
            ButtonAddNewCatchmentSacramento.SetState(addNewCatchmentSacramentoCommand, showBasinTools);
            ButtonAddNewCatchmentHbv.SetState(addNewCatchmentHbvCommand, showBasinTools);
            
            ButtonAddNewWasteWaterTreatmentPlant.SetState(addNewWasteWaterTreatmentPlantCommand, showBasinTools);
            ButtonAddNewRunoffBoundary.SetState(addNewRunoffBoundaryCommand, showBasinTools);

            // Area2d tools
            ButtonAddNewThinDam2D.SetState(addThinDam2dCommand, showArea2DTools);
            ButtonAddNewFixedWeir2D.SetState(addFixedWeir2dCommand, showArea2DTools);
            ButtonAddNewObsPoint2D.SetState(addObs2dCommand, showArea2DTools);
            ButtonAddNewObsCs2D.SetState(addObsCS2dCommand, showArea2DTools);
            ButtonAddNewPump2D.SetState(addPump2dCommand, showArea2DTools);
            ButtonAddNewWeir2D.SetState(addWeir2dCommand, showArea2DTools);
            ButtonAddLeveeBreach.SetState(addNewLeveeBreachCommand, showArea2DTools);
            ButtonAddNewGate2D.SetState(addGate2dCommand, showArea2DTools);
            ButtonAddNewLandBoundary2D.SetState(addLandBoundary2dCommand, showArea2DTools);
            ButtonAddNewDryPoint2D.SetState(addDryPoint2dCommand, showArea2DTools);
            ButtonAddNewDryArea2D.SetState(addDryArea2dCommand, showArea2DTools);
            ButtonAddNewEmbankment2D.SetState(addNewEmbankmentCommand, showArea2DTools);
            ButtonAddNewRoofArea.SetState(addRoofAreaCommand, showArea2DTools);
            ButtonAddNewGully.SetState(addGullyCommand, showArea2DTools);
            ButtonAddNewEnclosure2D.SetState(addEnclosure2dCommand, showArea2DTools);
            ButtonAddBridgePillar.SetState(addBridgePillarCommand, showArea2DTools);

            ButtonAddNewLink.SetState(addNewLinkCommand, regions.Count > 0);
            
            ButtonShowSideView.IsEnabled = showSideViewCommand.Enabled;
            ButtonShowSideView.Visibility = showNetworkTools ? Visibility.Visible : Visibility.Collapsed;

            ButtonOpenCaseAnalysis.IsEnabled = openCaseAnalysisCommand.Enabled;
            ButtonShowSideView.Visibility = showNetworkTools ? Visibility.Visible : Visibility.Collapsed;

            SetCoverageComboBox();

            // Depends on SetCoverageComboBox
            ButtonAddNewNetworkLocation.SetState(addNewNetworkLocationCommand);
            NetworkCoverageEditPanel.Visibility = showNetworkTools ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SetCoverageComboBox()
        {
            var mapview = NetworkEditorGuiPlugin.GetFocusedMapView();
            if (mapview == null || mapview.Map == null)
            {
                ComboBoxSelectNetworkCoverage.SelectionChanged -= ComboBoxSelectNetworkCoverageSelectionChanged;
                ComboBoxSelectNetworkCoverage.Items.Clear();
                ComboBoxSelectNetworkCoverage.SelectionChanged += ComboBoxSelectNetworkCoverageSelectionChanged;
                return;
            }

            var coverageLayers = mapview.Map.GetAllVisibleLayers(true).OfType<INetworkCoverageGroupLayer>().Where(c => c.Coverage.IsEditable).ToList();
            var oldCoverageGroupLayers = ComboBoxSelectNetworkCoverage.Items.Cast<INetworkCoverageGroupLayer>().ToList();
            
            var tool = mapview.MapControl.GetToolByType<HydroRegionEditorMapTool>();
            var currentSelection = tool == null
                ? ComboBoxSelectNetworkCoverage.SelectedItem as INetworkCoverageGroupLayer
                : tool.ActiveNetworkCoverageGroupLayer;

            if (oldCoverageGroupLayers.Count == coverageLayers.Count && 
                oldCoverageGroupLayers.Union(coverageLayers).Count() == ComboBoxSelectNetworkCoverage.Items.Count &&
                ComboBoxSelectNetworkCoverage.SelectedItem as INetworkCoverageGroupLayer == currentSelection)
            {
                return;
            }
            
            ComboBoxSelectNetworkCoverage.SelectionChanged -= ComboBoxSelectNetworkCoverageSelectionChanged;

            ComboBoxSelectNetworkCoverage.Items.Clear();
            foreach (var layer in coverageLayers)
            {
                ComboBoxSelectNetworkCoverage.Items.Add(layer);
            }

            if (ComboBoxSelectNetworkCoverage.Items.Contains(currentSelection))
            {
                ComboBoxSelectNetworkCoverage.SelectedItem = currentSelection;
            }
            else if (ComboBoxSelectNetworkCoverage.Items.Count != 0)
            {
                ComboBoxSelectNetworkCoverage.SelectedIndex = ComboBoxSelectNetworkCoverage.Items.Count - 1;
            }

            UpdateAddNetworkLocationHeaderTooltip();

            ComboBoxSelectNetworkCoverage.SelectionChanged += ComboBoxSelectNetworkCoverageSelectionChanged;

        }

        public bool IsContextualTabVisible(string tabGroupName, string tabName)
        {
            if (tabGroupName == geospatialContextualGroup.Name && tabName == tabRegion.Name)
            {
                return IsActiveViewMapViewWithRegion();
            }

            if (tabGroupName == crossSectionContextualGroup.Name && tabName == tabCrossSectionTools.Name)
            {
                return IsCrossSectionViewActive();
            }

            return false;
        }

        private static bool IsCrossSectionViewActive()
        {
            return NetworkEditorGuiPlugin.Instance != null &&
                   NetworkEditorGuiPlugin.Instance.Gui.DocumentViews.ActiveView is ICrossSectionHistoryCapableView;
        }

        private bool IsActiveViewMapViewWithRegion()
        {
            var mapView = NetworkEditorGuiPlugin.GetFocusedMapView();

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

        public object GetRibbonControl() { return RibbonControl; }


        private void ButtonShowHydroRegionContents_Click(object sender, RoutedEventArgs e)
        {
            showHydroRegionContentsCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewBranch_Click(object sender, RoutedEventArgs e)
        {
            addNewBranchCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewBranchScribble_Click(object sender, RoutedEventArgs e)
        {
            addNewBranchScribbleCommand.Execute();
            ValidateItems();
        }

        private void ButtonInsertNewNode_Click(object sender, RoutedEventArgs e)
        {
            insertNewNodeCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewPipe_Click(object sender, RoutedEventArgs e)
        {
            addNewPipeCommand.Execute();
            ValidateItems();
        }

        private void ButtonSplitPipe_Click(object sender, RoutedEventArgs e)
        {
            splitPipeCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCrossSectionYZ_Click(object sender, RoutedEventArgs e)
        {
            addNewCrossSectionYZCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCrossSectionZW_Click(object sender, RoutedEventArgs e)
        {
            addNewCrossSectionZWCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCrossSectionXYZ_Click(object sender, RoutedEventArgs e)
        {
            addNewCrossSectionXYZCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCrossSectionStandard_Click(object sender, RoutedEventArgs e)
        {
            addNewCrossSectionStandardCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCrossSectionDefault_Click(object sender, RoutedEventArgs e)
        {
            addNewCrossSectionDefaultCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCrossSectionInterpolated_Click(object sender, RoutedEventArgs e)
        {
            addNewCrossSectionInterpolatedCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewPump_Click(object sender, RoutedEventArgs e)
        {
            addNewPumpCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewWeir_Click(object sender, RoutedEventArgs e)
        {
            addNewWeirCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewOrifice_Click(object sender, RoutedEventArgs e)
        {
            addNewOrificeCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCulvert_Click(object sender, RoutedEventArgs e)
        {
            addNewCulvertCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewBridge_Click(object sender, RoutedEventArgs e)
        {
            addNewBridgeCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewLateralSource_Click(object sender, RoutedEventArgs e)
        {
            addNewLateralSourceCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewRetention_Click(object sender, RoutedEventArgs e)
        {
            addNewRetentionCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewObservationPoint_Click(object sender, RoutedEventArgs e)
        {
            addNewObservationPointCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewRoute_Click(object sender, RoutedEventArgs e)
        {
            addNewRouteCommand.Execute();
            ValidateItems();
        }

        private void ButtonShowCrossSectionHistory_Click(object sender, RoutedEventArgs e)
        {
            showCrossSectionHistoryCommand.Execute();
            ValidateItems();
        }

        private void ButtonRemoveRoute_Click(object sender, RoutedEventArgs e)
        {
            string message = string.Format(Properties.Resources.Ribbon_RemoveRoute_Are_you_sure_you_want_to_delete_the_following_item___0_,
                                           routeSelectionFinder.GetSelectedRoute(NetworkEditorGuiPlugin.Instance.Gui));
  
            if(MessageBox.Show(message, Properties.Resources.Ribbon_RemoveRoute_Confirm,
                               MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                removeRouteCommand.Execute();
            }
            ValidateItems();
        }

        private void ButtonAddNewCatchmentPaved_Click(object sender, RoutedEventArgs e)
        {
            addNewCatchmentPavedCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCatchmentUnpaved_Click(object sender, RoutedEventArgs e)
        {
            addNewCatchmentUnpavedCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCatchmentOpenWater_Click(object sender, RoutedEventArgs e)
        {
            addNewCatchmentOpenWaterCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCatchmentGreenHouse_Click(object sender, RoutedEventArgs e)
        {
            addNewCatchmentGreenHouseCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCatchmentSacramento_Click(object sender, RoutedEventArgs e)
        {
            addNewCatchmentSacramentoCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewCatchmentHbv_Click(object sender, RoutedEventArgs e)
        {
            addNewCatchmentHbvCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewWasteWaterTreatmentPlant_Click(object sender, RoutedEventArgs e)
        {
            addNewWasteWaterTreatmentPlantCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewRunoffBoundary_Click(object sender, RoutedEventArgs e)
        {
            addNewRunoffBoundaryCommand.Execute();
            ValidateItems();
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

        private void ButtonAddNewGate2D_Click(object sender, RoutedEventArgs e)
        {
            addGate2dCommand.Execute();
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

        /// <summary>
        /// Handles the Click event of the ButtonAddNewLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void ButtonAddNewLink_Click(object sender, RoutedEventArgs e)
        {
            addNewLinkCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewNetworkLocation_Click(object sender, RoutedEventArgs e)
        {
            addNewNetworkLocationCommand.Execute();
            ValidateItems();
        }

        private void ButtonShowSideView_Click(object sender, RoutedEventArgs e)
        {
            showSideViewCommand.Execute();
            ValidateItems();
        }

        private void ButtonOpenCaseAnalysis_Click(object sender, RoutedEventArgs e)
        {
            openCaseAnalysisCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddLeveeBeach_Click(object sender, RoutedEventArgs e)
        {
            addNewLeveeBreachCommand.Execute();
            ValidateItems();
        }
        private void ButtonAddNewEmbankment_Click(object sender, RoutedEventArgs e)
        {
            addNewEmbankmentCommand.Execute();
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

        private void ButtonAddNewRoofArea_Click(object sender, RoutedEventArgs e)
        {
            addRoofAreaCommand.Execute();
            ValidateItems();
        }

        private void ButtonAddNewGully_Click(object sender, RoutedEventArgs e)
        {
            addGullyCommand.Execute();
            ValidateItems();
        }

        private void UpdateAddNetworkLocationHeaderTooltip()
        {
            var mapView = NetworkEditorGuiPlugin.GetFocusedMapView();
            if (mapView == null) return;

            var tool = mapView.MapControl.GetToolByType<HydroRegionEditorMapTool>();
            if (tool == null) return;

            var activeNetworkCoverageGroupLayer = ComboBoxSelectNetworkCoverage.SelectedItem as INetworkCoverageGroupLayer;
            tool.ActiveNetworkCoverageGroupLayer = activeNetworkCoverageGroupLayer;

            ButtonAddNewNetworkLocation.Header = activeNetworkCoverageGroupLayer != null
                ? string.Format("Add {0} location", activeNetworkCoverageGroupLayer.Name)
                : "Add Network Location";
            ButtonAddNewNetworkLocation.ToolTip = activeNetworkCoverageGroupLayer != null
                ? string.Format("Add location to {0} data", activeNetworkCoverageGroupLayer.Name)
                : "Add location to network data";
        }

        private void ComboBoxSelectNetworkCoverageSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateAddNetworkLocationHeaderTooltip();
            
            // activate addNewNetworkLocation tool
            if(!addNewNetworkLocationCommand.Enabled)
            {
                addNewNetworkLocationCommand.Execute();
            }
            ValidateItems();
        }

        private void ButtonAddNewSewerConnection_Click(object sender, RoutedEventArgs e)
        {
            addNewSewerConnectionCommand.Execute();
            ValidateItems();
        }
    }
}
