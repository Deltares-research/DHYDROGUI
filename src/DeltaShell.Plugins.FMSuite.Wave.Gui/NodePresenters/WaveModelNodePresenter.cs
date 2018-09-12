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
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.IO;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters
{
    public class WaveModelNodePresenter : ModelNodePresenterBase<WaveModel>
    {
        private const string GeneralFolderName = "General";
        private const string AreaFolderName = "Area";
        private const string TimePointFolderName = "Time Frame";
        private const string BoundaryFolderName = "Boundary Conditions";
        private const string ProcessesName = "Processes";
        private const string PhysicalParametersName = "Physical Parameters";
        private const string NumericalParametersName = "Numerical Parameters";
        private const string OutputParametersName = "Output Parameters";
        private const string ObstacleNodeName = "Obstacles";
        private const string ObsPointNodeName = "Observation Points";
        private const string ObsCurveNodeName = "Observation Curves";
        private const string SpectralDomainName = "Spectral Domain";

        private static readonly Bitmap AreaImage = Common.Gui.Properties.Resources.area2d;
        private static readonly Bitmap TimePointImage = Common.Gui.Properties.Resources.timers;
        private static readonly Bitmap BoundaryConditionsImage = Common.Gui.Properties.Resources.boundary_folder;
        private static readonly Bitmap ObstacleImage = Resources.wall_brick;
        private static readonly Bitmap ObsPointImage = Common.Gui.Properties.Resources.Observation;
        private static readonly Bitmap ObsCurveImage = Common.Gui.Properties.Resources.ObservationCS;
        private static readonly Bitmap ProcessesImage = Common.Gui.Properties.Resources.processes;
        private static readonly Bitmap NumericsIcon = Common.Gui.Properties.Resources.folder_wrench;
        private static readonly Bitmap OutputParametersIcon = Common.Gui.Properties.Resources.output_param;
        private static readonly Bitmap GeneralIcon = Common.Gui.Properties.Resources.settings;
        private static readonly Bitmap PhysicalParametersImage = Common.Gui.Properties.Resources.folder_wrench;

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

        public WaveModelNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

        public override DragOperations CanDrag(WaveModel nodeData)
        {
            return DragOperations.Move | DragOperations.Copy;
        }

        public override IEnumerable GetChildNodeObjects(WaveModel model, ITreeNode node)
        {
            yield return new WaveTreeShortcut(GeneralFolderName, GeneralIcon, model);
            yield return new WaveTreeShortcut(AreaFolderName, AreaImage, model, null, GetArea2DItems(model));
            yield return new WaveTreeShortcut(SpectralDomainName, PhysicalParametersImage, model);
            yield return model.OuterDomain;
            yield return new WaveTreeShortcut(TimePointFolderName, TimePointImage, model, model.TimePointData);
            yield return new WaveTreeShortcut(ProcessesName, ProcessesImage, model);
            yield return new WaveTreeShortcut(BoundaryFolderName, BoundaryConditionsImage, model, model.BoundaryConditions, model.BoundaryConditions);
            yield return new WaveTreeShortcut(PhysicalParametersName, PhysicalParametersImage, model);
            yield return new WaveTreeShortcut(NumericalParametersName, NumericsIcon, model);
            yield return new WaveTreeShortcut(OutputParametersName, OutputParametersIcon, model) {TabText = "Output"};
            yield return new TreeFolder(model, GetOutputItems(model), "Output", FolderImageType.Output);
        }

        private IEnumerable<object> GetArea2DItems(WaveModel model)
        {
            yield return new WaveTreeShortcut(ObstacleNodeName, ObstacleImage, model, model.Obstacles);
            yield return new WaveTreeShortcut(ObsPointNodeName, ObsPointImage, model, model.ObservationPoints);
            yield return new WaveTreeShortcut(ObsCurveNodeName, ObsCurveImage, model, model.ObservationCrossSections);
        }

        private static IEnumerable<object> GetOutputItems(WaveModel model)
        {
            var dataItem = model.GetDataItemByTag(WaveModel.SwanLogDataItemTag);
            var swanLog = dataItem.Value as TextDocument;
            if (swanLog != null && !string.IsNullOrEmpty(swanLog.Content))
            {
                yield return dataItem;
            }
            foreach (var domain in WaveDomainHelper.GetAllDomains(model.OuterDomain))
            {
                var subDataItem = model.GetDataItemByTag(WaveModel.WavmStoreDataItemTag + domain.Name);
                if (subDataItem == null) continue;
                var functionStore = subDataItem.Value as WavmFileFunctionStore;
                if (functionStore != null && functionStore.Functions.Any() && !string.IsNullOrEmpty(functionStore.Path))
                    yield return subDataItem;
            }
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menu = base.GetContextMenu(sender, nodeData);

            var model = nodeData as WaveModel;

            if (model != null)
            {
                var contextMenu = new ContextMenuStrip();
                contextMenu.Items.Add(CreateWpfSettingsMenuItem(model));
                if (model.CoordinateSystem != null)
                {
                    contextMenu.Items.Add(FMMenuItemHelper.CreateResetCoordinateSystemItem(model));
                    contextMenu.Items.Add(FMMenuItemHelper.CreateCoordinateTransformItem(model, Gui));
                }
                contextMenu.Items.Add(CreateValidationMenuItem(model));

                var waveMenu = new MenuItemContextMenuStripAdapter(contextMenu);

                if (menu != null)
                    menu.Add(waveMenu);
                else
                    return waveMenu;
            }
            return menu;
        }

        private ClonableToolStripMenuItem CreateWpfSettingsMenuItem(WaveModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = "Wave Settings",
                Tag = model,
            };
            item.Click += OnSettingsClicked;
            return item;
        }

        private ClonableToolStripMenuItem CreateValidationMenuItem(WaveModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.WaveModelNodePresenter_CreateValidationMenuItem_Validate___, 
                Tag = model, 
                Image = Common.Gui.Properties.Resources.validation
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