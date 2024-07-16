using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Helpers;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.NodePresenters;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Restart;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Import;
using DeltaShell.Plugins.DelftModels.RTCShapes.IO;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms;
using DeltaShell.Plugins.SharpMapGis.Gui.Forms.CoverageViews;
using GeoAPI.Extensions.Coverages;
using log4net;
using Mono.Addins;
using ValidationAspects;
using Clipboard = DelftTools.Controls.Clipboard;
using PropertyInfo = DelftTools.Shell.Gui.PropertyInfo;
using TreeNode = DelftTools.Controls.Swf.TreeViewControls.TreeNode;
using TreeView = DelftTools.Controls.Swf.TreeViewControls.TreeView;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    [Extension(typeof(IPlugin))]
    public class RealTimeControlGuiPlugin : GuiPlugin
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlGuiPlugin));

        private IGui gui;
        private ContextMenuStrip contextMenuStripControlGroups;
        private ContextMenuStrip contextMenuStripControlGroup;
        private ToolStripMenuItem addNewControlGroupToolStripMenuItem;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripMenuItem copyXmlToClipboardToolStripMenuItem;
        private ToolStripMenuItem copyToolsXmlToClipboardToolStripMenuItem;
        private ClonableToolStripMenuItem convertCoordinateSystemToolStripMenuItem;
        private ContextMenuStrip convertCoordinateSystemContextMenu;

        public RealTimeControlGuiPlugin()
        {
            InitializeComponent();
        }

        public override string Name => "Real Time Control (UI)";

        public override string DisplayName => "D-Real Time Control Plugin (UI)";

        public override string Description =>
            RealTimeControl.Properties.Resources.RealTimeControlApplicationPlugin_Description;

        public override string Version => AssemblyUtils.GetAssemblyInfo(GetType().Assembly).Version;

        public override string FileFormatVersion => "3.5.0.0";

        public override IGui Gui
        {
            get => gui;
            set
            {
                if (base.Gui != null)
                {
                    Gui.Application.ProjectService.ProjectOpened -= ApplicationProjectOpened;
                    Gui.Application.ProjectService.ProjectCreated -= ApplicationProjectOpened;
                    Gui.Application.ActivityRunner.ActivityStatusChanged -= ActivityRunnerActivityStatusChanged;
                }

                gui = value;
                if (gui == null)
                {
                    return;
                }

                Gui.Application.ProjectService.ProjectOpened += ApplicationProjectOpened;
                Gui.Application.ProjectService.ProjectCreated += ApplicationProjectOpened;
                Gui.Application.ActivityRunner.ActivityStatusChanged += ActivityRunnerActivityStatusChanged;
            }
        }

        public override IMapLayerProvider MapLayerProvider => new RealTimeControlMapLayerProvider();

        public override IEnumerable<PropertyInfo> GetPropertyInfos()
        {
            yield return new PropertyInfo<RealTimeControlModel, RealTimeControlModelProperties>();
            yield return new PropertyInfo<Output, OutputProperties>();
            yield return new PropertyInfo<Input, InputProperties>();
            yield return new PropertyInfo<LookupSignal, LookupSignalProperties>();
            yield return new PropertyInfo<PIDRule, PIDRuleProperties>();
            yield return new PropertyInfo<FactorRule, FactorRuleProperties>();
            yield return new PropertyInfo<HydraulicRule, HydraulicRuleProperties>();
            yield return new PropertyInfo<TimeRule, TimeRuleProperties>();
            yield return new PropertyInfo<RelativeTimeRule, RelativeTimeRuleProperties>();
            yield return new PropertyInfo<IntervalRule, IntervalRuleProperties>();
            yield return new PropertyInfo<DirectionalCondition, DirectionalConditionProperties>();
            yield return new PropertyInfo<TimeCondition, TimeConditionProperties>();
            yield return new PropertyInfo<StandardCondition, StandardConditionProperties>();
            yield return new PropertyInfo<ControlGroup, ControlGroupProperties>();
            yield return new PropertyInfo<MathematicalExpression, MathematicalExpressionProperties>();
        }

        public override IEnumerable<ViewInfo> GetViewInfoObjects()
        {
            yield return new ViewInfo<ControlGroup, ControlGroupGraphView>
            {
                Description = "Control Group Editor",
                AfterCreate = (v, o) =>
                {
                    v.Gui = gui;
                    v.Model = GetModel(o);
                    v.EnsureVisible(o);
                }
            };
            yield return new ViewInfo<IEventedList<ControlGroup>, ControlGroupLayerEditorView>
            {
                GetViewName = (v, o) => "Control Group Editor",
                Description = "Control groups",
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = o => GetModel(o),
                AfterCreate = (v, o) =>
                {
                    v.RtcModel = GetModel(o);
                    v.OpenViewAction = ob => Gui.CommandHandler.OpenView(ob);
                }
            };
            yield return new ViewInfo<RealTimeControlModel, ValidationView>
            {
                Description = "Validation Report",
                AfterCreate = (v, o) =>
                {
                    v.Gui = Gui;
                    v.OnValidate = d => o.Validate();
                }
            };
            yield return new ViewInfo<RealTimeControlModelExporter, RtcExporterDialog>();

            yield return new ViewInfo<IFeatureCoverage, CoverageTableView>
            {
                Description = "Output",
                AdditionalDataCheck = o => GetModelForFeatureCoverage(o) != null,
                CompositeViewType = typeof(ProjectItemMapView),
                GetCompositeViewData = GetModelForFeatureCoverage
            };
        }

        public override bool CanCopy(IProjectItem item)
            => IsActionEnabledFor(item);

        public override bool CanCut(IProjectItem item)
            => IsActionEnabledFor(item);

        public override bool CanExport(IProjectItem item)
            => IsActionEnabledFor(item);
        
        private static bool IsActionEnabledFor(IProjectItem item)
            => !(item is RealTimeControlModel);

        public override void Activate()
        {
            gui.Application.ProjectService.ProjectClosing += Application_ProjectClosing;
            base.Activate();
        }

        public override void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            gui.Application.ProjectService.ProjectClosing -= Application_ProjectClosing;
            base.Deactivate();
        }

        public override IMenuItem GetContextMenu(object sender, object data)
        {
            if (data is IEventedList<ControlGroup>)
            {
                contextMenuStripControlGroups.Items[0].Tag = sender;
                return new MenuItemContextMenuStripAdapter(contextMenuStripControlGroups);
            }

            if (data is ControlGroup)
            {
                contextMenuStripControlGroup.Items[0].Tag = sender;
                contextMenuStripControlGroup.Items[1].Tag = sender;
                contextMenuStripControlGroup.Items[2].Tag = sender;
                return new MenuItemContextMenuStripAdapter(contextMenuStripControlGroup);
            }

            if (data is IRealTimeControlModel)
            {
                convertCoordinateSystemToolStripMenuItem.Tag = data;
                return new MenuItemContextMenuStripAdapter(convertCoordinateSystemContextMenu);
            }

            return base.GetContextMenu(sender, data);
        }

        public override IEnumerable<ITreeNodePresenter> GetProjectTreeViewNodePresenters()
        {
            yield return new RealTimeControlModelNodePresenter(this);
            yield return new RtcObjectNodePresenter {GuiPlugin = this};
            yield return new RtcOutputFileFunctionStoreNodePresenter();
            yield return new ControlGroupCollectionNodePresenter {GuiPlugin = this};
            yield return new ControlGroupNodePresenter(this);
            yield return new RealTimeControlInputRestartFileNodePresenter(this);
            yield return new RealTimeControlOutputRestartFileNodePresenter(this);
            yield return new OutputTreeFolderNodePresenter();
        }

        public override IEnumerable<Assembly> GetPersistentAssemblies()
        {
            yield return GetType().Assembly;
            yield return typeof(ShapeBase).Assembly;
        }
        
        private void ApplicationProjectOpened(object sender, EventArgs<Project> e)
        {
            RealTimeControlModelExporter exporter = Gui.Application.FileExporters.OfType<RealTimeControlModelExporter>().First();
            RealTimeControlModelImporter importer = Gui.Application.FileImporters.OfType<RealTimeControlModelImporter>().First();

            exporter.XmlWriters.RemoveAllWhere(x => x is ShapesXmlWriter);
            importer.XmlReaders.RemoveAllWhere(x => x is ShapesXmlReader);

            var controller = new ControlGroupShapeController(Gui.ViewContextManager);

            exporter.XmlWriters.Add(new ShapesXmlWriter(controller));
            importer.XmlReaders.Add(new ShapesXmlReader(controller));
        }
        
        private void ActivityRunnerActivityStatusChanged(object sender, ActivityStatusChangedEventArgs e)
        {
            if (sender is IModel model && e.NewStatus == ActivityStatus.Initializing)
            {
                CloseRtcModelOutput(model);
            }

            if (sender is RealTimeControlModel realTimeControlModel && e.NewStatus == ActivityStatus.Failed)
            {
                OpenValidationView(realTimeControlModel);
            }
        }

        private void CloseRtcModelOutput(IModel model)
        {
            switch (model)
            {
                case RealTimeControlModel rtcModel:
                    CloseOutputFileViews(rtcModel);
                    break;
                case ICompositeActivity compositeActivity:
                    compositeActivity.Activities.OfType<RealTimeControlModel>()
                                     .ForEach(CloseOutputFileViews);
                    break;
            }
        }

        [InvokeRequired]
        private void OpenValidationView(RealTimeControlModel realTimeControlModel)
        {
            Gui.CommandHandler.OpenView(realTimeControlModel, typeof(ValidationView));
        }

        [InvokeRequired]
        private void CloseOutputFileViews(RealTimeControlModel model)
        {
            model.OutputDocuments.ForEach(Gui.CommandHandler.RemoveAllViewsForItem);
        }

        private void InitializeComponent()
        {
            contextMenuStripControlGroups = new ContextMenuStrip
            {
                Name = "contextMenuStripControlGroups",
                Size = new Size(237, 50)
            };
            contextMenuStripControlGroup = new ContextMenuStrip
            {
                Name = "contextMenuStripControlGroup",
                Size = new Size(254, 76)
            };

            addNewControlGroupToolStripMenuItem = new ToolStripMenuItem
            {
                Image = RealTimeControl.Properties.Resources.controlgroup_add,
                Name = "addNewControlGroupToolStripMenuItem",
                Text = "Add New Control Group..."
            };

            deleteToolStripMenuItem = new ToolStripMenuItem
            {
                Image = RealTimeControl.Properties.Resources.DeleteHS,
                Name = "deleteToolStripMenuItem",
                Text = "Delete"
            };

            copyXmlToClipboardToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "copyXmlToClipboardToolStripMenuItem",
                Text = "Copy Data Xml to clipboard"
            };

            copyToolsXmlToClipboardToolStripMenuItem = new ToolStripMenuItem
            {
                Name = "copyToolsXmlToClipboardToolStripMenuItem",
                Text = "Copy Tools Xml to Clipboard"
            };

            addNewControlGroupToolStripMenuItem.Click += AddNewControlGroupToolStripMenuItemClick;
            deleteToolStripMenuItem.Click += ButtonDeleteRtcItemToolStripMenuItem_Click;
            copyXmlToClipboardToolStripMenuItem.Click += CopyXmlToClipboardToolStripMenuItemClick;
            copyToolsXmlToClipboardToolStripMenuItem.Click += CopyToolsXmlToClipboardToolStripMenuItemClick;

            contextMenuStripControlGroup.Items.AddRange(new ToolStripItem[]
            {
                deleteToolStripMenuItem,
                copyXmlToClipboardToolStripMenuItem,
                copyToolsXmlToClipboardToolStripMenuItem
            });

            contextMenuStripControlGroups.Items.AddRange(new ToolStripItem[]
            {
                addNewControlGroupToolStripMenuItem
            });
            convertCoordinateSystemToolStripMenuItem = new ClonableToolStripMenuItem
            {
                Image = Properties.Resources.HydroRegion,
                Name = "convertCoordinateSystemToolStripMenuItem",
                Text = "Convert to Coordinate System..."
            };
            convertCoordinateSystemToolStripMenuItem.Click += ConvertCoordinateSystemToolStripMenuItemClick;

            convertCoordinateSystemContextMenu = new ContextMenuStrip {Name = "convertCoordinateSystemMenu"};
            convertCoordinateSystemContextMenu.Items.AddRange(new ToolStripItem[]
            {
                convertCoordinateSystemToolStripMenuItem
            });
        }

        private void ConvertCoordinateSystemToolStripMenuItemClick(object sender, EventArgs e)
        {
            var realTimeControlModel = ((ToolStripMenuItem) sender).Tag as IRealTimeControlModel;
            if (realTimeControlModel == null)
            {
                throw new InvalidOperationException("Can not find model when converting the coordinate system.");
            }

            if (RTCModelCoordinateConvertor.Convert(realTimeControlModel))
            {
                MapView mapView = GetFocusedMapView();
                mapView?.Map?.ZoomToExtents();
            }
        }

        private MapView GetFocusedMapView()
        {
            IView viewToSearch = gui?.DocumentViews?.ActiveView;
            return viewToSearch.GetViewsOfType<MapView>().FirstOrDefault();
        }

        private void Application_ProjectClosing(object sender, EventArgs<Project> e)
        {
            var helper = RealTimeControlModelCopyPasteHelper.Instance;
            helper.ClearData();
        }

        private void AddNewControlGroupToolStripMenuItemClick(object sender, EventArgs e)
        {
            var realTimeControlModel = (RealTimeControlModel) ((ToolStripMenuItem) sender).Tag;

            string[] choices = RealTimeControlModelHelper.StandardControlGroups.ToArray();

            var dialog = new ListBasedDialog
            {
                DataSource = choices,
                SelectionMode = SelectionMode.One
            };
            string name = RealTimeControlModelHelper.GetUniqueName("Control Group {0}",
                                                                   realTimeControlModel.ControlGroups, "?");
            if (DialogResult.OK == dialog.ShowDialog())
            {
                ControlGroup controlGroup = RealTimeControlModelHelper.CreateStandardControlGroup((string) dialog.SelectedItems[0]);
                controlGroup.Name = name;
                realTimeControlModel.ControlGroups.Add(controlGroup);

                gui.Selection = controlGroup;
                gui.CommandHandler.OpenViewForSelection();
            }
        }

        private void CopyXmlToClipboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            var controlGroupTreeNode = (TreeNode) ((ToolStripMenuItem) sender).Tag;
            var controlGroup = (ControlGroup) controlGroupTreeNode.Tag;
            if (controlGroup != null)
            {
                try
                {
                    if (!ValidateControlGroup(controlGroup))
                    {
                        return;
                    }

                    var model = (RealTimeControlModel) controlGroupTreeNode.Parent.Parent.Parent.Tag;

                    XDocument xDocument = RealTimeControlXmlWriter.GetDataConfigXml("", model, new List<ControlGroup> {controlGroup}, null);
                    Clipboard.SetText(xDocument.ToString());
                }
                catch (Exception exception)
                {
                    Log.Error(exception.Message);
                }
            }
        }

        private void CopyToolsXmlToClipboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            var controlGroup = (ControlGroup) ((TreeNode) ((ToolStripMenuItem) sender).Tag).Tag;
            if (controlGroup != null)
            {
                try
                {
                    if (!ValidateControlGroup(controlGroup))
                    {
                        return;
                    }

                    XDocument xDocument = RealTimeControlXmlWriter.GetToolsConfigXml("", new List<ControlGroup> {controlGroup});
                    Clipboard.SetText(xDocument.ToString());
                }
                catch (Exception exception)
                {
                    Log.Error(exception.Message);
                }
            }
        }

        private static bool ValidateControlGroup(ControlGroup controlGroup)
        {
            ValidationResult result = controlGroup.Validate();
            if (!result.IsValid)
            {
                result.Messages.ForEach(message => Log.Error(message));
                return false;
            }

            return true;
        }

        private void ButtonDeleteRtcItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var node = (ITreeNode) ((ToolStripMenuItem) sender).Tag;
            ((TreeView) node.TreeView).DeleteNodeData();
        }

        private RealTimeControlModel GetModel(IEventedList<ControlGroup> controlGroups)
        {
            IEnumerable<RealTimeControlModel> allRtcModels = Gui.Application.GetAllModelsInProject().OfType<RealTimeControlModel>();
            return allRtcModels.First(m => m.ControlGroups.Equals(controlGroups));
        }

        private RealTimeControlModel GetModel(ControlGroup controlGroup)
        {
            IEnumerable<RealTimeControlModel> allRtcModels = Gui.Application.GetAllModelsInProject().OfType<RealTimeControlModel>();
            return allRtcModels.First(m => m.ControlGroups.Contains(controlGroup));
        }

        private RealTimeControlModel GetModelForFeatureCoverage(IFeatureCoverage featureCoverage)
        {
            return Gui.Application.GetAllModelsInProject()
                      .OfType<RealTimeControlModel>()
                      .FirstOrDefault(m => m.OutputFileFunctionStore != null &&
                                           m.OutputFileFunctionStore.Functions.Contains(featureCoverage));
        }
    }
}