using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Common.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveModelNodePresenter : ModelNodePresenterBase<WaveModel>
    {
        private const string GeneralFolderName = "General";
        private const string AreaFolderName = "Area";
        private const string TimePointFolderName = "Time Frame";
        // TODO (MWT) Remove this as part of clean up and renome added value below
        private const string BoundaryFolderName = "Boundary Conditions";
        private const string PhysicalProcessesName = "Physical Processes";
        private const string NumericalParametersName = "Numerical Parameters";
        private const string OutputParametersName = "Output Parameters";
        private const string ObstacleNodeName = "Obstacles";
        private const string ObsPointNodeName = "Observation Points";
        private const string ObsCurveNodeName = "Observation Curves";
        private const string SpectralDomainName = "Spectral Domain";

        private const string SpatiallyVariantBoundaryFolderName = "Boundaries";

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

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaveModel model)
        {
            if (firstTimeCreate)
            {
                node.Expand();
                firstTimeCreate = false;
            }

            node.Text = model.Name;
            node.Image = WaveImage;
        }

        public WaveModelNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) {}

        public override DragOperations CanDrag(WaveModel nodeData)
        {
            return DragOperations.Move | DragOperations.Copy;
        }

        public override IEnumerable GetChildNodeObjects(WaveModel model, ITreeNode node)
        {
            yield return new WaveModelTreeShortcut(GeneralFolderName, GeneralIcon, model, GeneralFolderName);
            yield return new WaveModelTreeShortcut(AreaFolderName, AreaImage, model, null, ShortCutType.SettingsTab,
                                                   GetArea2DItems(model));
            yield return new WaveModelTreeShortcut(SpectralDomainName, PhysicalParametersImage, model,
                                                   SpectralDomainName);
            yield return model.OuterDomain;
            yield return new WaveModelTreeShortcut(TimePointFolderName, TimePointImage, model, model.TimePointData,
                                                   ShortCutType.FeatureSet);
            yield return new WaveModelTreeShortcut(BoundaryFolderName, 
                                                   BoundaryConditionsImage, 
                                                   model,
                                                   model.BoundaryConditions, 
                                                   ShortCutType.FeatureSet,
                                                   model.BoundaryConditions);

            yield return new WaveModelTreeShortcut(SpatiallyVariantBoundaryFolderName, 
                                                   BoundaryConditionsImage, 
                                                   model, 
                                                   model.BoundaryContainer.Boundaries, 
                                                   ShortCutType.FeatureSet, 
                                                   model.BoundaryContainer.Boundaries);

            yield return new WaveModelTreeShortcut(PhysicalProcessesName, ProcessesImage, model, PhysicalProcessesName);
            yield return new WaveModelTreeShortcut(NumericalParametersName, NumericsIcon, model,
                                                   NumericalParametersName);
            yield return new WaveModelTreeShortcut(OutputParametersName, OutputParametersIcon, model, "Output");
            yield return new TreeFolder(model, GetOutputItems(model), "Output", FolderImageType.Output);
        }

        private static IEnumerable<object> GetArea2DItems(WaveModel model)
        {
            yield return new WaveModelTreeShortcut(ObstacleNodeName, ObstacleImage, model, model.Obstacles,
                                                   ShortCutType.FeatureSet);
            yield return new WaveModelTreeShortcut(ObsPointNodeName, ObsPointImage, model, model.ObservationPoints,
                                                   ShortCutType.FeatureSet);
            yield return new WaveModelTreeShortcut(ObsCurveNodeName, ObsCurveImage, model,
                                                   model.ObservationCrossSections, ShortCutType.FeatureSet);
        }

        private static IEnumerable<object> GetOutputItems(WaveModel model)
        {
            IDataItem dataItem = model.GetDataItemByTag(WaveModel.SwanLogDataItemTag);
            var swanLog = dataItem.Value as TextDocument;
            if (swanLog != null && !string.IsNullOrEmpty(swanLog.Content))
            {
                yield return dataItem;
            }

            foreach (WaveDomainData domain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
            {
                IDataItem subDataItem = model.GetDataItemByTag(WaveModel.WavmStoreDataItemTag + domain.Name);
                if (subDataItem == null)
                {
                    continue;
                }

                var functionStore = subDataItem.Value as WavmFileFunctionStore;
                if (functionStore != null && functionStore.Functions.Any() && !string.IsNullOrEmpty(functionStore.Path))
                {
                    yield return subDataItem;
                }
            }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menu = base.GetContextMenu(sender, nodeData);

            var waveModel = nodeData as WaveModel;
            if (waveModel == null)
            {
                return menu;
            }

            if (waveModel.IsCoupledToFlow && menu is MenuItemContextMenuStripAdapter menuAdapter)
            {
                DisableRunModelButton(menuAdapter);
            }

            var contextMenu = new ContextMenuStrip();

            if (waveModel.CoordinateSystem != null)
            {
                contextMenu.Items.Add(FMMenuItemHelper.CreateResetCoordinateSystemItem(waveModel));
                contextMenu.Items.Add(FMMenuItemHelper.CreateCoordinateTransformItem(waveModel, Gui));
                contextMenu.Items.Add(new ToolStripSeparator());
            }

            contextMenu.Items.Add(CreateWpfSettingsMenuItem(waveModel));
            contextMenu.Items.Add(CreateValidationMenuItem(waveModel));

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

        private static void DisableRunModelButton(MenuItemContextMenuStripAdapter menuAdapter)
        {
            ToolStripItem runModelButton = menuAdapter.ContextMenuStrip
                                                      .Items
                                                      .OfType<ToolStripItem>()
                                                      .FirstOrDefault(item => item.Name == "buttonModelStart");

            if (runModelButton != null)
            {
                runModelButton.Enabled = false;
            }
        }

        private ClonableToolStripMenuItem CreateWpfSettingsMenuItem(WaveModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = "Settings",
                Tag = model,
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
            var model = (WaveModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(WpfSettingsView));
        }

        private void OnValidateClicked(object sender, EventArgs args)
        {
            var model = (WaveModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }
    }
}