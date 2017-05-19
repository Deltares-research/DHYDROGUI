using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

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
        private static readonly Bitmap PhysParamIcon = new Bitmap(Common.Gui.Properties.Resources.folder_wrench, 16, 16);
        private static readonly Bitmap NumParamIcon = new Bitmap(Common.Gui.Properties.Resources.settings, 16, 16);
        private static readonly Bitmap OutParamIcon = new Bitmap(Common.Gui.Properties.Resources.output_param, 16, 16);
        private static readonly Bitmap WindIcon = new Bitmap(Resources.Wind1, 16,16);

        // boolean is used only the first time to expand the node after creation.
        private bool firstTimeCreate = true;
        
        public WaterFlowFMModelNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
        }

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
            // experimental: don't have 'Input' folder..
            foreach (var input in GetInputItems(parentNodeData))
                yield return input;

            //yield return new TreeFolder(parentNodeData, , "Input", FolderImageType.Input);
            yield return new TreeFolder(parentNodeData, GetOutputItems(parentNodeData), "Output", FolderImageType.Output);
        }

        private IEnumerable GetInputItems(WaterFlowFMModel model)
        {
            // some of these are shortcuts because they have not specific data object
            // others are wrapped in shortcuts because they are not IProjectItem

            yield return new FlowFMTreeShortcut("General", NumParamIcon, model);
            yield return model.GetDataItemByValue(model.Area);
            yield return new FlowFMTreeShortcut("Grid", UnstrucIcon, model, model.Grid);
            yield return
                new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>("Bed Level",
                    Resources.unstrucWater, model, model.Bathymetry, "General")
                {
                    ContextMenuDataGetter = o => ((WaterFlowFMModel)o).Bathymetry
                };
            yield return new FlowFMTreeShortcut("Time Frame", TimeFrameIcon, model);
            yield return new FlowFMTreeShortcut("Processes", ProcessesIcon, model);
            yield return
                new FlowFMTreeShortcut("Initial Conditions", InitialConditionsIcon, model, null,
                    GetInitialConditionsItems(model));
            yield return
                new FlowFMTreeShortcut("Boundary Conditions", BoundaryConditionIcon, model, model.BoundaryConditionSets,
                    model.BoundaryConditionSets);
            yield return
                new FlowFMTreeShortcut("Physical Parameters", PhysParamIcon, model, null, GetPhysicalSubItems(model));
            yield return
                new FlowFMTreeShortcut("Sources and Sinks", SourceSinkIcon, model, model.SourcesAndSinks,
                    model.SourcesAndSinks);
            yield return new FlowFMTreeShortcut("Numerical Parameters", NumParamIcon, model);
            yield return new FlowFMTreeShortcut("Output Parameters", OutParamIcon, model);
        }

        private static IEnumerable<object> GetInitialConditionsItems(WaterFlowFMModel model)
        {
            yield return model.GetDataItemByValue(model.RestartInput);
            string tabText = "Initial Conditions";
            yield return
                new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                    WaterFlowFMModelDefinition.InitialWaterLevelDataItemName, Resources.waterLayers, model,
                    model.InitialWaterLevel, tabText)
                {
                    ContextMenuDataGetter = o => ((WaterFlowFMModel) o).InitialWaterLevel
                };
            if (model.UseSalinity)
            {
                yield return
                    new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                        WaterFlowFMModelDefinition.InitialSalinityDataItemName, Resources.salt, model,
                        model.InitialSalinity.Coverages[0], tabText)
                    {
                        ContextMenuDataGetter = o => ((WaterFlowFMModel) o).InitialSalinity.Coverages[0]
                    };
            }
            if (model.HeatFluxModelType != HeatFluxModelType.None)
            {
                yield return
                    new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                        WaterFlowFMModelDefinition.InitialTemperatureDataItemName, Resources.thermometer, model,
                        model.InitialTemperature, tabText)
                    {
                        ContextMenuDataGetter = o => ((WaterFlowFMModel) o).InitialTemperature
                    };
            }
            
            foreach (var tracer in model.InitialTracers)
            {
                var treeShortCut = new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                    tracer.Name, Resources.pipette, model, tracer,
                    tabText)
                {
                    ContextMenuDataGetter =
                        o => ((WaterFlowFMModel) o).InitialTracers.First(tr => tr.Name == tracer.Name)
                };
                yield return treeShortCut;
            }
            if (model.UseMorSed)
            {
                tabText = "Sediment";
                foreach (var fraction in model.InitialFractions)
                {
                    var treeShortCut = new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                        fraction.Name, Resources.pipette, model, fraction,
                        tabText)
                    {
                        ContextMenuDataGetter =
                            o => ((WaterFlowFMModel)o).InitialFractions.First(tr => tr.Name == fraction.Name)
                    };
                    yield return treeShortCut;
                }
            }
        }

        private static IEnumerable<object> GetPhysicalSubItems(WaterFlowFMModel model)
        {
            yield return
                new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                    WaterFlowFMModelDefinition.RoughnessDataItemName, Resources.Roughness, model, model.Roughness,
                    "Physical Parameters")
                {
                    ContextMenuDataGetter = o => ((WaterFlowFMModel) o).Roughness
                };
            yield return
                new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                    WaterFlowFMModelDefinition.ViscosityDataItemName, Resources.tube, model, model.Viscosity,
                    "Physical Parameters") {ContextMenuDataGetter = o => ((WaterFlowFMModel) o).Viscosity};

            yield return
                new SpatialOperationCoverageTreeShortcut<WaterFlowFMModel, WaterFlowFMModelView>(
                    WaterFlowFMModelDefinition.DiffusivityDataItemName, Resources.drop, model, model.Diffusivity,
                    "Physical Parameters") {ContextMenuDataGetter = o => ((WaterFlowFMModel) o).Diffusivity};
            
            if (model.ModelDefinition.HeatFluxModel.MeteoData != null)
            {
                yield return model.ModelDefinition.HeatFluxModel;
            }

            yield return model.WindFields;
        }

        private IEnumerable GetOutputItems(WaterFlowFMModel model)
        {
            yield return new TreeFolder(model, GetRestartStates(model), "States", FolderImageType.None);

            foreach (var p in GetOutputDataItemsCore(model))
            {
               
                yield return p;
            }
        }

        private IEnumerable GetOutputDataItemsCore(WaterFlowFMModel model)
        {
            if (model.OutputMapFileStore != null)
            {
                foreach (var func in model.OutputMapFileStore.Functions)
                    yield return WrapIntoOutputItem(func, func.Name, model);
                // wrapped in dataitems for central map resolve logic..
            }

            if (model.OutputHisFileStore != null)
            {
                foreach (var func in model.OutputHisFileStore.Functions)
                    yield return WrapIntoOutputItem(func, func.Name, model);
                // wrapped in dataitems for central map resolve logic..
            }
        }

        private static IDataItem WrapIntoOutputItem(object o, string tag, IDataItemOwner model)
        {
            var existingItems = DataItems.Where(di => Equals(di.Tag, tag) && Equals(di.Owner, model)).ToList();
            var existingItem = existingItems.FirstOrDefault(di => di.ValueType == o.GetType());

            if (existingItem == null)
            {
                var newItem = new DataItem(o, DataItemRole.Output) {Tag = tag, Owner = model};
                DataItems.Add(newItem);
                return newItem;
            }

            if (!ReferenceEquals(o, existingItem.Value))
            {
                existingItem.Value = o;
            }

            return existingItem;
        }

        private static readonly IList<DataItem> DataItems = new List<DataItem>();

        private static IEnumerable GetRestartStates(WaterFlowFMModel data)
        {
            var restartStates =
                data.DataItems.Where(
                    dataItem => dataItem.Value is FileBasedRestartState && dataItem.Role == DataItemRole.Output);
            return restartStates;
        }

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menu = base.GetContextMenu(sender, nodeData);

            var model = nodeData as WaterFlowFMModel;

            if (model != null)
            {
                var contextMenu = new ContextMenuStrip();
                if (model.CoordinateSystem != null)
                {
                    contextMenu.Items.Add(FMMenuItemHelper.CreateResetCoordinateSystemItem(model));
                    contextMenu.Items.Add(FMMenuItemHelper.CreateCoordinateTransformItem(model, Gui));
                }
                contextMenu.Items.Add(CreateValidationMenuItem(model));
                contextMenu.Items.Add(CreateFileStructureItem(model));

                var flowMenu = new MenuItemContextMenuStripAdapter(contextMenu);

                if (menu != null)
                    menu.Insert(menu.Count - 2, flowMenu);
                else
                    return flowMenu;
            }
            return menu;
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

        private void OnValidateClicked(object sender, EventArgs args)
        {
            var model = (WaterFlowFMModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof (ValidationView));
        }

        private void OnFileStructureClicked(object sender, EventArgs args)
        {
            var model = (WaterFlowFMModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(WaterFlowFMFileStructureView));
        }
    }
}