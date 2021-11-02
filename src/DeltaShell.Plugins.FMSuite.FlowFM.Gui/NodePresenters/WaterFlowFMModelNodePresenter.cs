using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Functions;
using DelftTools.Hydro;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DeltaShell.Dimr;
using DeltaShell.NGHS.Common.Gui;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileWriters.Roughness;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters
{
    public class WaterFlowFMModelNodePresenter : ModelNodePresenterBase<WaterFlowFMModel>
    {
        internal static Bitmap UnstrucModelIcon;
        public static readonly Bitmap ThinDamIcon = new Bitmap(Resources.thindam, 16, 16);
        public static readonly Bitmap FixedWeirIcon = new Bitmap(Resources.fixedweir, 16, 16);
        public static readonly Bitmap LandBoundaryIcon = new Bitmap(Resources.landboundary, 16, 16);
        public static readonly Bitmap DryPointIcon = new Bitmap(Resources.dry_point, 16, 16);
        public static readonly Bitmap ObsIcon = new Bitmap(Common.Gui.Properties.Resources.Observation, 16, 16);
        public static readonly Bitmap ObsCSIcon = new Bitmap(Common.Gui.Properties.Resources.ObservationCS, 16, 16);
        public static readonly Bitmap UnstrucIcon = new Bitmap(Resources.unstruc, 16, 16);
        public static readonly Bitmap Link1D2DIcon = new Bitmap(Resources.links1d2d, 16, 16);
        public static readonly Bitmap RoofAreaIcon = new Bitmap(Resources.Roof, 16, 16);
        public static readonly Bitmap GullyIcon = new Bitmap(Resources.Gully, 16, 16);
        private static readonly Bitmap TimeFrameIcon = new Bitmap(Common.Gui.Properties.Resources.timers, 16, 16);
        private static readonly Bitmap InitialConditionsIcon = new Bitmap(Resources.initial_folder, 16, 16);
        private static readonly Bitmap BoundaryConditionIcon = new Bitmap(Common.Gui.Properties.Resources.boundary_folder, 16, 16);
        private static readonly Bitmap FolderIcon = new Bitmap(Resources.folder, 16, 16);
        private static readonly Bitmap SourceSinkIcon = new Bitmap(Resources.SourceSinkFolder, 16, 16);
        private static readonly Bitmap PhysParamIcon = new Bitmap(Common.Gui.Properties.Resources.folder_wrench, 16, 16);
        private static readonly Bitmap NumParamIcon = new Bitmap(Common.Gui.Properties.Resources.settings, 16, 16);
        private static readonly Bitmap OutParamIcon = new Bitmap(Common.Gui.Properties.Resources.output_param, 16, 16);
        private static readonly Bitmap NetworkDiscretizationIcon = new Bitmap(SharpMapGis.Gui.Properties.Resources.discretization, 16, 16);

        // boolean is used only the first time to expand the node after creation.
        private bool firstTimeCreate = true;

        public WaterFlowFMModelNodePresenter(GuiPlugin guiPlugin) : base(guiPlugin)
        {
            var graphicsProvider = guiPlugin.GraphicsProvider;
            UnstrucModelIcon = new DrawingBrush{Drawing = graphicsProvider.CreateDrawingGroupFor(typeof(WaterFlowFMModel))}.BitmapFromBrush(16, 16);
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

            yield return new TreeFolder(parentNodeData, GetOutputItems(parentNodeData), "Output", FolderImageType.Output);
        }

        protected override void OnPropertyChanged(WaterFlowFMModel item, ITreeNode node, PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(item, node, e);

            if (e.PropertyName == nameof(WaterFlowFMModel.InitialCoverageSetChanged))
            {
                TreeView.RefreshChildNodes(node);
            }
        }

        private IEnumerable GetInputItems(WaterFlowFMModel model)
        {
            // some of these are shortcuts because they have not specific data object
            // others are wrapped in shortcuts because they are not IProjectItem

            yield return new FmModelTreeShortcut("General", NumParamIcon, model, "General", ShortCutType.SettingsTab,
                new[]
                {
                    new FmModelTreeShortcut("Time Frame", TimeFrameIcon, model, "Time Frame"),
                    new FmModelTreeShortcut("Geometry Parameters", NumParamIcon, model, "Geometry Parameters"),
                    new FmModelTreeShortcut("Initial Conditions", InitialConditionsIcon, model, "Initial Conditions"),
                    new FmModelTreeShortcut("Physical Parameters", PhysParamIcon, model, "Physical Parameters"),
                    new FmModelTreeShortcut("Numerical Parameters", NumParamIcon, model, "Numerical Parameters"),
                    new FmModelTreeShortcut("Output Parameters", OutParamIcon, model, "Output Parameters")
                });

            yield return new TreeFolder(model, new object[]
            {
                model.GetDataItemByValue(model.Network),
                new FmModelTreeShortcut(model.NetworkDiscretization.Name, NetworkDiscretizationIcon, model, model.NetworkDiscretization, ShortCutType.Default),
                new TreeFolder(model, new List<object>
                {
                    new FmModelTreeShortcut("Channels", Resources.FrictionDefinition, model, model.ChannelFrictionDefinitions, ShortCutType.FeatureSet),
                    new FmModelTreeShortcut("Sewer", Resources.FrictionDefinition, model, model.PipeFrictionDefinitions, ShortCutType.FeatureSet),
                    new FmModelTreeShortcut("Lanes", FolderIcon, model, null, ShortCutType.FeatureSet, model.RoughnessSections)
                }, "1D Roughness", FolderImageType.None),
                new TreeFolder(model, new List<object>
                {
                    new FmModelTreeShortcut(GetInitialConditionsShortCutName(model), Resources.waterLayers, model, model.ChannelInitialConditionDefinitions, ShortCutType.FeatureSet),
                }, "1D Initial Conditions", FolderImageType.None),
                new FmModelTreeShortcut("1D Boundary Conditions", BoundaryConditionIcon, model, model.BoundaryConditions1D, ShortCutType.FeatureSet),
                new FmModelTreeShortcut("Lateral Sources", FolderIcon, model, model.LateralSourcesData, ShortCutType.FeatureSet),
            }, "1D", FolderImageType.None);

            yield return new TreeFolder(model, new object[]
            {
                model.GetDataItemByValue(model.Area),
                new FmModelTreeShortcut("Grid", UnstrucIcon, model, model.Grid, ShortCutType.Grid),
                new FmModelTreeShortcut("Bed Level", Resources.unstrucWater, model, model.Bathymetry, ShortCutType.SpatialCoverage),
                new FmModelTreeShortcut("Initial Conditions", InitialConditionsIcon, model, "Initial Conditions", ShortCutType.SettingsTab, GetInitialConditionsItems(model)),
                new FmModelTreeShortcut("Boundary Conditions", BoundaryConditionIcon, model, model.BoundaryConditionSets, ShortCutType.FeatureSet, model.BoundaryConditionSets),
                new FmModelTreeShortcut("Physical Parameters", PhysParamIcon, model, "Physical Parameters", ShortCutType.SettingsTab, GetPhysicalSubItems(model)),
                new FmModelTreeShortcut("Sources and Sinks", SourceSinkIcon, model, model.SourcesAndSinks, ShortCutType.FeatureSet, model.SourcesAndSinks)
            }, "2D", FolderImageType.None);

            yield return new FmModelTreeShortcut("1D2D Links", Link1D2DIcon, model, model.Links, ShortCutType.FeatureSet);
        }

        private string GetInitialConditionsShortCutName(WaterFlowFMModel fmModel)
        {
            var property = fmModel.ModelDefinition.GetModelProperty(GuiProperties.InitialConditionGlobalQuantity1D);

            if (property != null && int.TryParse(property.GetValueAsString(), out var quantity) && Enum.IsDefined(typeof(InitialConditionQuantity), quantity))
            {
                var quantityAsString = InitialConditionQuantityTypeConverter
                    .ConvertInitialConditionQuantityToString((InitialConditionQuantity) quantity);
                return $"{RoughnessDataRegion.SectionId.DefaultValue} - {quantityAsString}";
            }
            return $"{RoughnessDataRegion.SectionId.DefaultValue}";
        }

        private static IEnumerable<object> GetInitialConditionsItems(WaterFlowFMModel model)
        {
            var initialWaterCondition2DQuantity = (InitialConditionQuantity) (int) model.ModelDefinition
                .GetModelProperty(GuiProperties.InitialConditionGlobalQuantity2D).Value;
            yield return new FmModelTreeShortcut(initialWaterCondition2DQuantity == InitialConditionQuantity.WaterLevel 
                ? WaterFlowFMModelDefinition.InitialWaterLevelDataItemName
                : WaterFlowFMModelDefinition.InitialWaterDepthDataItemName, Resources.waterLayers, model, model.InitialWaterLevel, ShortCutType.SpatialCoverage);

            if (model.UseSalinity)
            {
                yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InitialSalinityDataItemName, Resources.salt, model, model.InitialSalinity.Coverages[0], ShortCutType.SpatialCoverage);
            }

            if (model.HeatFluxModelType != HeatFluxModelType.None)
            {
                yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InitialTemperatureDataItemName, Resources.thermometer, model, model.InitialTemperature, ShortCutType.SpatialCoverage);
            }

            foreach (var tracer in model.InitialTracers)
            {
                yield return new FmModelTreeShortcut(tracer.Name, Resources.pipette, model, tracer, ShortCutType.SpatialCoverage);
            }

            if (!model.UseMorSed) yield break;

            foreach (var fraction in model.InitialFractions)
            {
                yield return new FmModelTreeShortcut(fraction.Name, Resources.pipette, model, fraction, ShortCutType.SpatialCoverage);
            }
        }

        private static IEnumerable<object> GetPhysicalSubItems(WaterFlowFMModel model)
        {
            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.RoughnessDataItemName, Resources.Roughness, model, model.Roughness, ShortCutType.SpatialCoverage);
            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.ViscosityDataItemName, Resources.tube, model, model.Viscosity, ShortCutType.SpatialCoverage);
            yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.DiffusivityDataItemName, Resources.drop, model, model.Diffusivity, ShortCutType.SpatialCoverage);

            if (model.UseInfiltration)
            {
                yield return new FmModelTreeShortcut(WaterFlowFMModelDefinition.InfiltrationDataItemName, Resources.infiltration, model, model.Infiltration, ShortCutType.SpatialCoverage);
            }
            
            if (model.ModelDefinition.HeatFluxModel.MeteoData != null)
            {
                yield return model.ModelDefinition.HeatFluxModel;
            }

            yield return model.FmMeteoFields;
            yield return model.WindFields;
        }

        private IEnumerable GetOutputItems(WaterFlowFMModel model)
        {
            var dimrLogDataItem = model.GetDataItems<TextDocument>(DataItemRole.Output).FirstOrDefault(di => di.Tag == DimrRunHelper.dimrRunLogfileDataItemTag);
            if (dimrLogDataItem != null) yield return dimrLogDataItem;

            var diaLogDataItem = model.GetDataItems<TextDocument>(DataItemRole.Output).FirstOrDefault(di => di.Tag == WaterFlowFMModelDataSet.DiaFileDataItemTag);
            if (diaLogDataItem != null) yield return diaLogDataItem;

            foreach (var p in GetOutputDataItemsCore(model))
            {

                yield return p;
            }
        }

        private IEnumerable GetOutputDataItemsCore(WaterFlowFMModel model)
        {
            if (model.OutputHisFileStore == null) 
                yield break;

            foreach (var func in model.OutputHisFileStore.Functions.OfType<TimeSeries>())
                yield return WrapIntoOutputItem(func, func.Name, model);
            
            if (model.OutputClassMapFileStore != null)
            {
                foreach (IFunction func in model.OutputClassMapFileStore.Functions)
                {
                    yield return WrapIntoOutputItem(func, func.Name, model);
                }
            }
            // wrapped in dataitems for central map resolve logic..
        }

        private static IDataItem WrapIntoOutputItem(object o, string tag, IDataItemOwner model)
        {
            var existingItems = DataItems.Where(di => Equals(di.Tag, tag) && Equals(di.Owner, model)).ToList();
            var existingItem = existingItems.FirstOrDefault(di => di.ValueType == o.GetType());

            if (existingItem == null)
            {
                var newItem = new DataItem(o, DataItemRole.Output) { Tag = tag, Owner = model };
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

        public override IMenuItem GetContextMenu(ITreeNode sender, object nodeData)
        {
            var menu = base.GetContextMenu(sender, nodeData);

            var model = nodeData as WaterFlowFMModel;
            if (model == null) return menu;

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
            if (menu == null) return flowMenu;

            menu.Add(flowMenu);

            // remove properties because there is already a settings option
            var propertiesItem = (menu as MenuItemContextMenuStripAdapter)?.ContextMenuStrip?.Items?
                .OfType<ToolStripItem>()
                .FirstOrDefault(i => i.Name == "buttonModelProperties");

            if (propertiesItem != null)
            {
                propertiesItem.Visible = false;
            }

            return menu;
        }

        private ClonableToolStripMenuItem CreateWpfSettingsMenuItem(WaterFlowFMModel model)
        {
            var item = new ClonableToolStripMenuItem
            {
                Text = "Settings",
                Tag = model,
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
            var model = (WaterFlowFMModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(WpfSettingsView));
        }

        private void OnValidateClicked(object sender, EventArgs args)
        {
            var model = (WaterFlowFMModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(ValidationView));
        }

        private void OnFileStructureClicked(object sender, EventArgs args)
        {
            var model = (WaterFlowFMModel)((ToolStripItem)sender).Tag;
            Gui.DocumentViewsResolver.OpenViewForData(model, typeof(WaterFlowFMFileStructureView));
        }
    }
}