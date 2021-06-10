using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveModelNodePresenter : ModelNodePresenterBase<WaveModel>
    {
        private const string GeneralFolderName = "General";
        private const string AreaFolderName = "Area";
        private const string TimePointFolderName = "Time Frame";
        private const string BoundaryFolderName = "Boundaries";
        private const string PhysicalProcessesName = "Physical Processes";
        private const string NumericalParametersName = "Numerical Parameters";
        private const string OutputParametersName = "Output Parameters";
        private const string ObstacleNodeName = "Obstacles";
        private const string ObsPointNodeName = "Observation Points";
        private const string ObsCurveNodeName = "Observation Curves";
        private const string SpectralDomainName = "Spectral Domain";

        private static readonly Bitmap AreaImage = Resources.area2d;
        private static readonly Bitmap TimePointImage = Resources.timers;
        private static readonly Bitmap BoundaryConditionsImage = Resources.boundary_folder;
        private static readonly Bitmap ObstacleImage = Properties.Resources.wall_brick;
        private static readonly Bitmap ObsPointImage = Resources.Observation;
        private static readonly Bitmap ObsCurveImage = Resources.ObservationCS;
        private static readonly Bitmap ProcessesImage = Resources.processes;
        private static readonly Bitmap NumericsIcon = Resources.folder_wrench;
        private static readonly Bitmap OutputParametersIcon = Resources.output_param;
        private static readonly Bitmap GeneralIcon = Resources.settings;
        private static readonly Bitmap PhysicalParametersImage = Resources.folder_wrench;

        private static readonly Bitmap WaveImage = Wave.Properties.Resources.wave;

        private bool firstTimeCreate = true;

        public WaveModelNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) { }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaveModel nodeData)
        {
            if (firstTimeCreate)
            {
                node.Expand();
                firstTimeCreate = false;
            }

            node.Text = nodeData.Name;
            node.Image = WaveImage;
        }

        public override DragOperations CanDrag(WaveModel nodeData)
        {
            return DragOperations.Move | DragOperations.Copy;
        }

        public override IEnumerable GetChildNodeObjects(WaveModel parentNodeData, ITreeNode node)
        {
            yield return new WaveModelTreeShortcut(GeneralFolderName, GeneralIcon, parentNodeData, GeneralFolderName);
            yield return new WaveModelTreeShortcut(AreaFolderName, AreaImage, parentNodeData, AreaFolderName, ShortCutType.SettingsTab,
                                                   GetArea2DItems(parentNodeData));
            yield return new WaveModelTreeShortcut(SpectralDomainName, PhysicalParametersImage, parentNodeData,
                                                   SpectralDomainName);
            yield return parentNodeData.OuterDomain;
            yield return new WaveModelTreeShortcut(TimePointFolderName,
                                                   TimePointImage,
                                                   parentNodeData,
                                                   parentNodeData.TimeFrameData,
                                                   ShortCutType.FeatureSet);

            yield return new WaveModelTreeShortcut(BoundaryFolderName,
                                                   BoundaryConditionsImage,
                                                   parentNodeData,
                                                   parentNodeData.BoundaryContainer.Boundaries,
                                                   ShortCutType.FeatureSet,
                                                   parentNodeData.BoundaryContainer.Boundaries);

            yield return new WaveModelTreeShortcut(PhysicalProcessesName, ProcessesImage, parentNodeData, PhysicalProcessesName);
            yield return new WaveModelTreeShortcut(NumericalParametersName, NumericsIcon, parentNodeData,
                                                   NumericalParametersName);
            yield return new WaveModelTreeShortcut(OutputParametersName, OutputParametersIcon, parentNodeData, OutputParametersName);
            yield return parentNodeData.WaveOutputData;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menu = base.GetContextMenu(sender, nodeData);

            var model = nodeData as WaveModel;
            if (model == null)
            {
                return menu;
            }

            var contextMenu = new ContextMenuStrip();

            if (model.CoordinateSystem != null)
            {
                contextMenu.Items.Add(FMMenuItemHelper.CreateResetCoordinateSystemItem(model));
                contextMenu.Items.Add(FMMenuItemHelper.CreateCoordinateTransformItem(model, Gui));
                contextMenu.Items.Add(new ToolStripSeparator());
            }

            contextMenu.Items.Add(CreateWpfSettingsMenuItem(model));
            contextMenu.Items.Add(CreateValidationMenuItem(model));

            var waveMenu = new MenuItemContextMenuStripAdapter(contextMenu);
            if (menu == null)
            {
                return waveMenu;
            }

            menu.Add(waveMenu);

            // remove properties because there is already a settings option
            ToolStripItem propertiesItem = (menu as MenuItemContextMenuStripAdapter)?.ContextMenuStrip?.Items?
                                                                                    .OfType<ToolStripItem>()
                                                                                    .FirstOrDefault(
                                                                                        i =>
                                                                                            i.Name ==
                                                                                            "buttonModelProperties");

            if (propertiesItem != null)
            {
                propertiesItem.Visible = false;
            }

            return menu;
        }

        private static IEnumerable<object> GetArea2DItems(WaveModel model)
        {
            yield return new WaveModelTreeShortcut(ObstacleNodeName, ObstacleImage, model, model.FeatureContainer.Obstacles,
                                                   ShortCutType.FeatureSet);
            yield return new WaveModelTreeShortcut(ObsPointNodeName, ObsPointImage, model, model.FeatureContainer.ObservationPoints,
                                                   ShortCutType.FeatureSet);
            yield return new WaveModelTreeShortcut(ObsCurveNodeName, ObsCurveImage, model,
                                                   model.FeatureContainer.ObservationCrossSections, ShortCutType.FeatureSet);
        }

        private ClonableToolStripMenuItem CreateWpfSettingsMenuItem(WaveModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = "Settings",
                Tag = model
            };
            item.Click += OnSettingsClicked;
            return item;
        }

        private ClonableToolStripMenuItem CreateValidationMenuItem(WaveModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Properties.Resources.WaveModelNodePresenter_CreateValidationMenuItem_Validate___,
                Tag = model,
                Image = Resources.validation
            };
            item.Click += OnValidateClicked;
            return item;
        }

        private void OnSettingsClicked(object sender, EventArgs args)
        {
            var model = (WaveModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(WpfSettingsView));
        }

        private void OnValidateClicked(object sender, EventArgs args)
        {
            var model = (WaveModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }
    }
}