using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DeltaShell.Dimr;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Coverages;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class WaterFlowFMModelNodePresenter : ModelNodePresenterBase<WaterFlowFMModel>
    {
        public static readonly Bitmap UnstrucModelIcon = new Bitmap(Resources.unstrucmodel, 16, 16);
        public static readonly Bitmap ThinDamIcon = new Bitmap(Resources.thindam, 16, 16);
        public static readonly Bitmap FixedWeirIcon = new Bitmap(Resources.fixedweir, 16, 16);
        public static readonly Bitmap LandBoundaryIcon = new Bitmap(Resources.landboundary, 16, 16);
        public static readonly Bitmap DryPointIcon = new Bitmap(Resources.dry_point, 16, 16);
        public static readonly Bitmap ObsIcon = new Bitmap(Common.Gui.Properties.Resources.Observation, 16, 16);
        public static readonly Bitmap ObsCSIcon = new Bitmap(Common.Gui.Properties.Resources.ObservationCS, 16, 16);
        public static readonly Bitmap UnstrucIcon = new Bitmap(Resources.unstruc, 16, 16);
        private static readonly Bitmap ProcessesIcon = new Bitmap(Common.Gui.Properties.Resources.processes, 16, 16);
        private static readonly Bitmap TimeFrameIcon = new Bitmap(Common.Gui.Properties.Resources.timers, 16, 16);
        private static readonly Bitmap InitialConditionsIcon = new Bitmap(Resources.initial_folder, 16, 16);
        private static readonly Bitmap BoundaryConditionIcon = new Bitmap(Common.Gui.Properties.Resources.boundary_folder, 16, 16);
        private static readonly Bitmap SourceSinkIcon = new Bitmap(Resources.SourceSinkFolder, 16, 16);
        private static readonly Bitmap LateralsFolderIcon = new Bitmap(Resources.LateralsFolder, 16, 16);
        private static readonly Bitmap PhysParamIcon = new Bitmap(Common.Gui.Properties.Resources.folder_wrench, 16, 16);
        private static readonly Bitmap NumParamIcon = new Bitmap(Common.Gui.Properties.Resources.settings, 16, 16);
        private static readonly Bitmap OutParamIcon = new Bitmap(Common.Gui.Properties.Resources.output_param, 16, 16);

        private readonly IList<DataItem> DataItems = new List<DataItem>();

        // boolean is used only the first time to expand the node after creation.
        private bool firstTimeCreate = true;

        public WaterFlowFMModelNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin) {}

        public override void UpdateNode(ITreeNode parentNode, ITreeNode node, WaterFlowFMModel nodeData)
        {
            if (firstTimeCreate)
            {
                node.Expand();
                firstTimeCreate = false;
            }

            node.Text = nodeData.Name;
            node.Image = UnstrucModelIcon;
        }

        public override DragOperations CanDrag(WaterFlowFMModel nodeData)
        {
            return DragOperations.Move | DragOperations.Copy;
        }

        public override IEnumerable GetChildNodeObjects(WaterFlowFMModel parentNodeData, ITreeNode node)
        {
            foreach (object input in GetInputItems(parentNodeData))
            {
                yield return input;
            }

            yield return new TreeFolder(parentNodeData, GetOutputItems(parentNodeData), "Output", FolderImageType.Output);
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            IMenuItem menu = base.GetContextMenu(sender, nodeData);

            var model = nodeData as WaterFlowFMModel;
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

            contextMenu.Items.Add(CreateFileStructureItem(model));
            contextMenu.Items.Add(CreateWpfSettingsMenuItem(model));
            contextMenu.Items.Add(CreateValidationMenuItem(model));

            var flowMenu = new MenuItemContextMenuStripAdapter(contextMenu);
            if (menu == null)
            {
                return flowMenu;
            }

            menu.Add(flowMenu);

            // remove properties because there is already a settings option
            ToolStripItem propertiesItem = (menu as MenuItemContextMenuStripAdapter)?.ContextMenuStrip?.Items?
                                                                                    .OfType<ToolStripItem>()
                                                                                    .FirstOrDefault(i => i.Name == "buttonModelProperties");

            if (propertiesItem != null)
            {
                propertiesItem.Visible = false;
            }

            return menu;
        }

        protected override void OnPropertyChanged(WaterFlowFMModel item, ITreeNode node, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(item, node, e);

            if (e.PropertyName == nameof(WaterFlowFMModel.InitialCoverageSetChanged) ||
                e.PropertyName == nameof(WaterFlowFMModel.RestartInput))
            {
                TreeView.RefreshChildNodes(node);
            }
        }

        private IEnumerable GetInputItems(WaterFlowFMModel model)
        {
            // some of these are shortcuts because they have not specific data object
            // others are wrapped in shortcuts because they are not IProjectItem

            yield return new FmModelTreeShortcut("General", NumParamIcon, model, "General");
            yield return model.GetDataItemByValue(model.Area);
            yield return new FmModelTreeShortcut("Grid", UnstrucIcon, model, model.Grid, ShortCutType.Grid);
            yield return new FmModelTreeShortcut("Bed Level", Resources.unstrucWater, model, model.SpatialData.Bathymetry, ShortCutType.SpatialCoverage);
            yield return new FmModelTreeShortcut("Time Frame", TimeFrameIcon, model, "Time Frame");
            yield return new FmModelTreeShortcut("Processes", ProcessesIcon, model, "Processes");
            yield return new FmModelTreeShortcut("Initial Conditions", InitialConditionsIcon, model, "Initial Conditions", childObjects: GetInitialConditionsItems(model));
            yield return new FmModelTreeShortcut("Boundary Conditions", BoundaryConditionIcon, model, model.BoundaryConditionSets, ShortCutType.FeatureSet, model.BoundaryConditionSets);
            yield return new FmModelTreeShortcut("Physical Parameters", PhysParamIcon, model, "Physical Parameters", childObjects: GetPhysicalSubItems(model));
            yield return new FmModelTreeShortcut("Sources and Sinks", SourceSinkIcon, model, model.SourcesAndSinks, ShortCutType.FeatureSet, model.SourcesAndSinks);
            yield return new FmModelTreeShortcut("Laterals", LateralsFolderIcon, model, model.Laterals, ShortCutType.FeatureSet, model.Laterals);
            yield return new FmModelTreeShortcut("Numerical Parameters", NumParamIcon, model, "Numerical Parameters");
            yield return new FmModelTreeShortcut("Output Parameters", OutParamIcon, model, "Output Parameters");
        }

        private static IEnumerable<object> GetInitialConditionsItems(WaterFlowFMModel model)
        {
            yield return model.RestartInput;

            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, Resources.waterLevel, model, model.SpatialData.InitialWaterLevel, ShortCutType.SpatialCoverage);

            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InitialVelocityXName, Resources.velocity_x, model, model.ModelDefinition.InitialVelocityX, ShortCutType.FeatureSet);
            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InitialVelocityYName, Resources.velocity_y, model, model.ModelDefinition.InitialVelocityY, ShortCutType.FeatureSet);

            if (model.UseSalinity)
            {
                yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InitialSalinityDataItemName, Resources.salt, model, model.SpatialData.InitialSalinity, ShortCutType.SpatialCoverage);
            }

            if (model.HeatFluxModelType != HeatFluxModelType.None)
            {
                yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InitialTemperatureDataItemName, Resources.thermometer, model, model.SpatialData.InitialTemperature, ShortCutType.SpatialCoverage);
            }

            foreach (UnstructuredGridCellCoverage tracer in model.SpatialData.InitialTracers)
            {
                yield return new FmModelTreeShortcut(tracer.Name, Resources.pipette, model, tracer, ShortCutType.SpatialCoverage);
            }

            if (!model.UseMorSed)
            {
                yield break;
            }

            foreach (UnstructuredGridCellCoverage fraction in model.SpatialData.InitialFractions)
            {
                yield return new FmModelTreeShortcut(fraction.Name, Resources.pipette, model, fraction, ShortCutType.SpatialCoverage);
            }
        }

        private static IEnumerable<object> GetPhysicalSubItems(WaterFlowFMModel model)
        {
            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.RoughnessDataItemName, Resources.Roughness, model, model.SpatialData.Roughness, ShortCutType.SpatialCoverage);
            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.ViscosityDataItemName, Resources.tube, model, model.SpatialData.Viscosity, ShortCutType.SpatialCoverage);
            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.DiffusivityDataItemName, Resources.drop, model, model.SpatialData.Diffusivity, ShortCutType.SpatialCoverage);

            if (IsUniformHeatFluxModel(model.ModelDefinition.HeatFluxModel))
            {
                yield return model.ModelDefinition.HeatFluxModel;
            }

            yield return model.WindFields;
        }

        private static bool IsUniformHeatFluxModel(HeatFluxModel heatFluxModel)
        {
            return heatFluxModel.MeteoData != null &&
                   heatFluxModel.GriddedHeatFluxFilePath == null &&
                   heatFluxModel.GridFilePath == null;
        }

        private IEnumerable GetOutputItems(WaterFlowFMModel model)
        {
            yield return new TreeFolder(model, model.RestartOutput, NGHS.Common.Gui.Properties.Resources.RestartFolderName, FolderImageType.Output);

            IDataItem dimrLogDataItem = model.GetDataItems<TextDocument>(DataItemRole.Output).FirstOrDefault(di => di.Tag == DimrRunHelper.dimrRunLogfileDataItemTag);
            if (dimrLogDataItem != null)
            {
                yield return dimrLogDataItem;
            }

            IDataItem diaLogDataItem = model.GetDataItems<TextDocument>(DataItemRole.Output).FirstOrDefault(di => di.Tag == WaterFlowFMModel.DiaFileDataItemTag);
            if (diaLogDataItem != null)
            {
                yield return diaLogDataItem;
            }

            foreach (object p in GetOutputDataItemsCore(model))
            {
                yield return p;
            }
        }

        private IEnumerable GetOutputDataItemsCore(WaterFlowFMModel model)
        {
            if (model.OutputMapFileStore != null)
            {
                foreach (IFunction func in model.OutputMapFileStore.Functions)
                {
                    yield return WrapIntoOutputItem(func, func.Name, model);
                }

                // wrapped in dataitems for central map resolve logic..
            }

            if (model.OutputHisFileStore != null)
            {
                foreach (IFunction func in model.OutputHisFileStore.Functions)
                {
                    yield return WrapIntoOutputItem(func, func.Name, model);
                }

                // wrapped in dataitems for central map resolve logic..
            }

            if (model.OutputClassMapFileStore != null)
            {
                foreach (IFunction func in model.OutputClassMapFileStore.Functions)
                {
                    yield return WrapIntoOutputItem(func, func.Name, model);
                }
            }
        }

        private IDataItem WrapIntoOutputItem(object o, string tag, IDataItemOwner model)
        {
            List<DataItem> existingItems = DataItems.Where(di => Equals(di.Tag, tag) && Equals(di.Owner, model)).ToList();
            DataItem existingItem = existingItems.FirstOrDefault(di => di.ValueType == o.GetType());

            if (existingItem == null)
            {
                var newItem = new DataItem(o, DataItemRole.Output)
                {
                    Tag = tag,
                    Owner = model
                };
                DataItems.Add(newItem);
                return newItem;
            }

            if (!ReferenceEquals(o, existingItem.Value))
            {
                UpdateModelReferenceIfNeeded(model, existingItem);
                existingItem.Value = o;
            }

            return existingItem;
        }

        /// <summary>
        /// Needed to restore the reference to the model in some situations, like after closing project and opening the same project
        /// without closing the GUI. Opening a project will create a new instance of the model and DataItems are still existing
        /// and referring to the old instance. 
        /// </summary>
        /// <param name="model"> Current instance of the model. </param>
        /// <param name="existingItem"> Retrieved dataItem for specific output. </param>
        private static void UpdateModelReferenceIfNeeded(IDataItemOwner model, DataItem existingItem)
        {
            if (!ReferenceEquals(existingItem.Owner, model))
            {
                existingItem.Owner = model;
            }
        }

        private ClonableToolStripMenuItem CreateWpfSettingsMenuItem(WaterFlowFMModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = "Settings",
                Tag = model
            };
            item.Click += OnSettingsClicked;
            return item;
        }

        private ClonableToolStripMenuItem CreateValidationMenuItem(WaterFlowFMModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.WaterFlowFMModelNodePresenter_CreateValidationMenuItem_Validate___,
                Tag = model,
                Image = Common.Gui.Properties.Resources.validation
            };
            item.Click += OnValidateClicked;
            return item;
        }

        private ClonableToolStripMenuItem CreateFileStructureItem(WaterFlowFMModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = Resources.WaterFlowFMModelNodePresenter_CreateFileStructureItem_File_Tree___,
                Tag = model,
                Image = Common.Gui.Properties.Resources.document_tree
            };
            item.Click += OnFileStructureClicked;
            return item;
        }

        private void OnSettingsClicked(object sender, EventArgs args)
        {
            var model = (WaterFlowFMModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(WpfSettingsView));
        }

        private void OnValidateClicked(object sender, EventArgs args)
        {
            var model = (WaterFlowFMModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }

        private void OnFileStructureClicked(object sender, EventArgs args)
        {
            var model = (WaterFlowFMModel) ((ToolStripItem) sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(WaterFlowFMFileStructureView));
        }
    }
}