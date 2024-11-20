using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Properties;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.IO;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using TreeNode = DelftTools.Controls.Swf.TreeViewControls.TreeNode;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.ProjectExplorer
{
    /// <summary>
    /// Water quality model node presenter
    /// </summary>
    public class WaterQualityModelNodePresenter : ModelNodePresenterBase<WaterQualityModel>
    {
        /// <summary>
        /// Creates a water quality model node presenter with
        /// <param name="guiPlugin"/>
        /// </summary>
        public WaterQualityModelNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) { }

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterQualityModel nodeData)
        {
            base.UpdateNode(parentNode, node, nodeData);
            if (!node.IsLoaded)
            {
                node.Expand();
                node.Nodes[0].Expand(); // Expand input folder
            }
        }

        /// <summary>
        /// Gets the child node objects.
        /// </summary>
        /// <param name="parentNodeData">The water quality model.</param>
        /// <param name="node">The node.</param>
        /// <returns>The collection of child node objects.</returns>
        public override IEnumerable GetChildNodeObjects(WaterQualityModel parentNodeData, ITreeNode node)
        {
            yield return new WaterQualityInputTreeFolder(parentNodeData, null, "Input", FolderImageType.Input,
                                                         GuiPlugin);
            yield return new TreeFolder(parentNodeData, GetOutputItems(parentNodeData), "Output",
                                        FolderImageType.Output);
        }

        public override bool CanRenameNode(ITreeNode node)
        {
            return true;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menu = base.GetContextMenu(sender, nodeData);
            var model = nodeData as WaterQualityModel;

            if (model == null)
            {
                return menu;
            }

            IMenuItem contextMenu = GetContextMenu(model, Gui);

            if (menu == null)
            {
                return contextMenu;
            }

            menu.Add(contextMenu);
            return menu;
        }

        protected override void OnPropertyChanged(WaterQualityModel item, ITreeNode node, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(WaterQualityModel.OutputOutOfSync))
            {
                ((TreeNode)node).RefreshChildNodes();
            }
        }

        private static IEnumerable GetOutputItems(WaterQualityModel data)
        {
            foreach (IDataItem outputDataItem in data.DataItems.Where(
                di => di.Role.HasFlag(DataItemRole.Output)))
            {
                yield return data.GetDataItemByValue(outputDataItem.Value);
            }
        }

        private static IMenuItem GetContextMenu(WaterQualityModel model, IGui gui)
        {
            var contextMenuStrip = new ContextMenuStrip();

            ClonableToolStripMenuItem exportInputFileMenuItem =
                CreateMenuItem("Export input file(s)", Resources.blue_document_export,
                               (s, e) => gui.CommandHandler.ExportFrom(model, new InputFileExporter()));
            contextMenuStrip.Items.Add(exportInputFileMenuItem);
            contextMenuStrip.Items.Add(CreateMenuItem(Resources.WaterQualityModelNodePresenter_GetContextMenu_Validate,
                                                      Resources.Validate, (s, e) => ValidateClick(model, gui)));

            return new MenuItemContextMenuStripAdapter(contextMenuStrip);
        }

        private static ClonableToolStripMenuItem CreateMenuItem(string name, Image image,
                                                                Action<object, EventArgs> clickAction)
        {
            var toolStripMenuItem = new ClonableToolStripMenuItem
            {
                Text = name,
                Image = image
            };
            toolStripMenuItem.Click += (o, args) => clickAction(o, args);

            return toolStripMenuItem;
        }

        private static void ValidateClick(WaterQualityModel model, IGui gui)
        {
            gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }

        private class WaterQualityInputTreeFolder : TreeFolder
        {
            private readonly IList<IDataItem> inputItems = new List<IDataItem>();

            public WaterQualityInputTreeFolder(WaterQualityModel waterQualityModel, IEnumerable childItems, string text,
                                               FolderImageType imageType, GuiPlugin guiPlugin)
                : base(waterQualityModel, childItems, text, imageType)
            {
                inputItems.Add(
                    waterQualityModel.GetDataItemByTag(WaterQualityModel.InputFileCommandLineDataItemMetaData.Tag));
                inputItems.Add(
                    waterQualityModel.GetDataItemByTag(WaterQualityModel.InputFileHybridDataItemMetaData.Tag));
                inputItems.Add(
                    waterQualityModel.GetDataItemByTag(WaterQualityModel.SubstanceProcessLibraryDataItemMetaData.Tag));
                inputItems.Add(waterQualityModel.GetDataItemByTag(WaterQualityModel.GridDataItemMetaData.Tag));
                inputItems.Add(waterQualityModel.GetDataItemByTag(WaterQualityModel.BathymetryDataItemMetaData.Tag));

                // Add a function data wrapper data item for the initial conditions
                inputItems.Add(new DataItem(new WaterQualityFunctionDataWrapper(waterQualityModel.InitialConditions),
                                            WaterQualityModel.InitialConditionsDataItemMetaData.Name,
                                            typeof(WaterQualityFunctionDataWrapper), DataItemRole.Input,
                                            WaterQualityModel.InitialConditionsDataItemMetaData.Tag)
                { Owner = waterQualityModel });

                // Add a function data wrapper data item for the process coefficients
                inputItems.Add(new DataItem(new WaterQualityFunctionDataWrapper(waterQualityModel.ProcessCoefficients),
                                            WaterQualityModel.ProcessCoefficientsDataItemMetaData.Name,
                                            typeof(WaterQualityFunctionDataWrapper), DataItemRole.Input,
                                            WaterQualityModel.ProcessCoefficientsDataItemMetaData.Tag)
                { Owner = waterQualityModel });

                var waqGuiPlugin = guiPlugin as WaterQualityModelGuiPlugin;

                if (waqGuiPlugin != null &&
                    waterQualityModel.ProcessCoefficients.Any(
                        pc => waqGuiPlugin.BloomInfo.AllParameters.Any(
                            par => string.Equals(
                                par, pc.Name,
                                StringComparison.InvariantCultureIgnoreCase))))
                {
                    inputItems.Add(
                        new DataItem(new WaterQualityBloomFunctionWrapper(waterQualityModel.ProcessCoefficients),
                                     WaterQualityModel.BloomAlgaeDataItemMetaData.Name,
                                     typeof(WaterQualityBloomFunctionWrapper), DataItemRole.Input,
                                     WaterQualityModel.BloomAlgaeDataItemMetaData.Tag)
                        { Owner = waterQualityModel });
                }

                // Add a function data wrapper data item for dispersion
                inputItems.Add(new DataItem(new WaterQualityFunctionDataWrapper(waterQualityModel.Dispersion),
                                            WaterQualityModel.DispersionDataItemMetaData.Name,
                                            typeof(WaterQualityFunctionDataWrapper), DataItemRole.Input,
                                            WaterQualityModel.DispersionDataItemMetaData.Tag)
                { Owner = waterQualityModel });

                inputItems.Add(new DataItem(waterQualityModel.Boundaries, DataItemRole.Input,
                                            WaterQualityModel.BoundariesDataItemMetaData.Tag)
                { Owner = waterQualityModel });
                inputItems.Add(new DataItem(waterQualityModel.Loads, DataItemRole.Input,
                                            WaterQualityModel.LoadsDataItemMetaData.Tag)
                { Owner = waterQualityModel });
                inputItems.Add(new DataItem(waterQualityModel.ObservationPoints, DataItemRole.Input,
                                            WaterQualityModel.ObservationPointsDataItemMetaData.Tag)
                { Owner = waterQualityModel });
                inputItems.Add(
                    waterQualityModel.GetDataItemByTag(WaterQualityModel.ObservationAreasDataItemMetaData.Tag));
                inputItems.Add(waterQualityModel.GetDataItemByTag(WaterQualityModel.BoundaryDataDataItemMetaData.Tag));
                inputItems.Add(waterQualityModel.GetDataItemByTag(WaterQualityModel.LoadsDataDataItemMetaData.Tag));
            }

            public override IEnumerable ChildItems
            {
                get
                {
                    var waterQualityModel = (WaterQualityModel)Parent;

                    return inputItems.Except(new List<IDataItem>
                    {
                        waterQualityModel.GetDataItemByValue(
                            waterQualityModel.InputFileHybrid)
                    });
                }
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as WaterQualityInputTreeFolder);
            }

            public override int GetHashCode()
            {
                return Parent.GetHashCode();
            }

            private bool Equals(WaterQualityInputTreeFolder other)
            {
                return other != null && other.Parent == Parent;
            }
        }
    }
}