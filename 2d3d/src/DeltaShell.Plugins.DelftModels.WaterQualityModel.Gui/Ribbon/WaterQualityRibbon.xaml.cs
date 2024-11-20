using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DelftTools.Controls;
using DelftTools.Shell.Gui.Forms;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.ObservationAreas;
using DeltaShell.Plugins.SharpMapGis.Gui;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands;
using DeltaShell.Plugins.SharpMapGis.Gui.Commands.SpatialOperations;
using Fluent;
using GisResources = DeltaShell.Plugins.SharpMapGis.Gui.Properties.Resources;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Ribbon
{
    /// <summary>
    /// Interaction logic for WaterQualityRibbon.xaml
    /// </summary>
    public partial class WaterQualityRibbon : IRibbonCommandHandler
    {
        private readonly IDictionary<UIElement, ICommand> buttonCommands = new Dictionary<UIElement, ICommand>();

        private readonly ICommand setLabelCommand;
        private readonly ICommand overwriteLabelCommand;

        private readonly IList<ICommand> spatialOperationCommands;

        public WaterQualityRibbon()
        {
            spatialOperationCommands = new List<ICommand>();
            InitializeComponent();

            mapTab.Group = geospatialContextualGroup;

            buttonCommands.Add(ButtonAddObservationPoint,
                               new MapToolCommand(WaterQualityModelGuiPlugin.AddObservationPointMapToolName));
            buttonCommands.Add(ButtonAddLoad,
                               new MapToolCommand(WaterQualityModelGuiPlugin.AddWaterQualityLoadMapToolName));
            buttonCommands.Add(ButtonFindGridCell,
                               new MapToolCommand(WaterQualityModelGuiPlugin.FindGridCellMapToolName));

            setLabelCommand = new SetLabelCommand();
            spatialOperationCommands.Add(setLabelCommand);
            // with this call, the set label command is sent to the sharpmapgisguiplugin as its owner.
            // It's not added to the list of Commands.
            SharpMapGisGuiPlugin.Instance.AddSpatialOperationCommand(ButtonSetLabel, setLabelCommand,
                                                                     typeof(SetLabelOperation),
                                                                     Properties.Resources.price_tag);

            overwriteLabelCommand = new OverwriteLabelCommand();
            spatialOperationCommands.Add(overwriteLabelCommand);
            SharpMapGisGuiPlugin.Instance.AddSpatialOperationCommand(ButtonOverwriteLabel, overwriteLabelCommand,
                                                                     typeof(OverwriteLabelOperation),
                                                                     GisResources.marker);
        }

        public IEnumerable<ICommand> Commands => buttonCommands.Values;

        public bool RibbonContainsSpatialOperationCommand(SpatialOperationCommandBase operationCommand)
        {
            return !spatialOperationCommands.Contains(operationCommand);
        }

        public object GetRibbonControl()
        {
            return RibbonControl;
        }

        public void ValidateItems()
        {
            foreach (KeyValuePair<UIElement, ICommand> buttonCommandPair in buttonCommands)
            {
                UIElement button = buttonCommandPair.Key;
                ICommand command = buttonCommandPair.Value;

                button.IsEnabled = command.Enabled;

                var toggleButton = button as ToggleButton;
                if (toggleButton != null)
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

        private void OnClick(object sender, RoutedEventArgs e)
        {
            buttonCommands[(UIElement) sender].Execute();
            ValidateItems();
        }

        private void ButtonSetLabel_Click(object sender, RoutedEventArgs e)
        {
            setLabelCommand.Execute();
            ValidateItems();
        }

        private void ButtonOverwriteLabel_Click(object sender, RoutedEventArgs e)
        {
            overwriteLabelCommand.Execute();
            ValidateItems();
        }
    }
}