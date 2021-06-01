using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf;
using DelftTools.Controls.Swf.Graph;
using DelftTools.Shell.Core.Extensions;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using GeoAPI.Extensions.Feature;
using log4net;
using Netron.GraphLib;
using Clipboard = DelftTools.Controls.Clipboard;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public partial class ControlGroupEditor : UserControl, IContextAwareView
    {
        /*
        DELFT3DFM -1165: 
            We need to limit the amount of feature locations that can be added to the context menu 
            (more than 4000 appears to lead to an out of memory exception in winforms)

            Here we specificy an arbitary (but sensible) limit
            Beware of setting this to anything higher than 4000! 
        */
        private const int MaxLocationsToDisplayIndividually = 900;

        private static readonly ILog Log = LogManager.GetLogger(typeof(ControlGroupEditor));
        private readonly XNamespace fns = "http://www.wldelft.nl/fews";

        private readonly IList<ShapeBase> shapes = new List<ShapeBase>();
        private object created, lastCreated;

        private ControlGroupEditorViewContext context;
        private ControlGroupEditorController controller;

        private ControlGroup controlGroup;

        private InquiryHelper inquiryHelper;

        public ControlGroupEditor()
        {
            InitializeComponent();
            Text = "ControlGroupEditor";

            controller = new ControlGroupEditorController {GetAutoResizeState = () => toolStripButtonResize.Checked};

            graphControl.ScrollBars = true;
            graphControl.ReadOnly = false;
            graphControl.MouseMove += GraphControlMouseMove;
            graphControl.AddLibrary(typeof(RuleShape).Module.FullyQualifiedName);

            graphControl.NetronGraph.OnShapeAdded += GraphControlOnOnShapeAdded;
            graphControl.NetronGraph.OnShapeRemoved += GraphControlOnOnShapeRemove;
            graphControl.NetronGraph.OnShowProperties += graphControl_OnShowProperties;

            graphControl.NetronGraph.OnShowProperties += OnShowProperties;
            graphControl.NetronGraph.OnGraphDragOver += OnGraphControlGraphDragOver;
            graphControl.NetronGraph.OnGraphDragDrop += OnGraphControlGraphDragDrop;
            graphControl.NetronGraph.OnContextMenu += OnGraphControlContextMenu;

            graphControl.NetronGraph.OnDoubleClick += GraphControlOnDoubleClick;
            graphControl.NetronGraph.MouseUp += OnGraphControlMouseUp;
            graphControl.NetronGraph.MouseDown += OnGraphControlMouseDown;

            inquiryHelper = new InquiryHelper();
        }

        public IGui Gui { get; set; } // selection and opening views

        public ControlGroupEditorController Controller
        {
            get => controller;
            private set => controller = value;
        }

        public GraphControl GraphControl => graphControl;

        public IRealTimeControlModel Model { get; set; }

        public object Data
        {
            get => controlGroup;
            set
            {
                shapes.Clear();

                controlGroup = (ControlGroup) value;
                controller.GraphControl = graphControl.NetronGraph;
                context = new ControlGroupEditorViewContext
                {
                    ControlGroup = controlGroup,
                    AutoSize = toolStripButtonResize.Checked
                };
                controller.ControlGroup = controlGroup;
                UpdateShapesInViewContext();
            }
        }

        public Image Image
        {
            get => null;
            set {}
        }

        public ViewInfo ViewInfo { get; set; }

        public IViewContext ViewContext
        {
            get => context;
            set
            {
                context = (ControlGroupEditorViewContext) value;
                SuspendLayout();

                if (context.ShapeList == null)
                {
                    ResumeLayout();
                    throw new InvalidOperationException(
                        "Invalid view context is passed to ControlGroupEditor, shape list can't be null");
                }

                toolStripButtonResize.Checked = context.AutoSize;

                List<ShapeBase> graphControlShapes = graphControl.GetShapes<ShapeBase>().ToList();
                if (graphControlShapes.Count == context.ShapeList.Count)
                {
                    // copy shape locations from view context to graph control
                    var i = 0;
                    foreach (ShapeBase contextShape in context.ShapeList)
                    {
                        graphControlShapes[i].Location = new PointF(contextShape.X, contextShape.Y);
                        graphControlShapes[i].AutoResize = context.AutoSize;

                        if (!contextShape.Rectangle.IsEmpty)
                        {
                            graphControlShapes[i].Rectangle =
                                new RectangleF(contextShape.Rectangle.Location, contextShape.Rectangle.Size);
                        }

                        i++;
                    }
                }

                UpdateShapesInViewContext();

                ResumeLayout();
            }
        }

        public void CopyXmlToClipboard(object sender, EventArgs e)
        {
            object tag = ((MenuItem) sender).Tag;
            if (tag is RtcBaseObject rtcObj)
            {
                CopyXmlToClipboard(rtcObj);
            }
        }

        public void CopyXmlToClipboard(RtcBaseObject rtcObj)
        {
            RtcSerializerBase serializer = SerializerCreator.CreateSerializerType(rtcObj);
            IEnumerable<XElement> listXElements = serializer.ToXml(fns, controlGroup.Name);
            var stringBuilder = new StringBuilder();
            foreach (XElement xElement in listXElements)
            {
                stringBuilder.Append(xElement + Environment.NewLine);
            }

            Clipboard.SetText(stringBuilder.ToString());
        }

        public void Link(Shape shape, IDataItem dataItem, IInquiryHelper inquiryHelper)
        {
            if (shape.Tag is Input input)
            {
                if ((dataItem.Role & DataItemRole.Output) == DataItemRole.Output)
                {
                    LinkDataItems(Model.GetDataItemByValue(input), dataItem);
                }
            }
            else if (shape.Tag is Output output && (dataItem.Role & DataItemRole.Input) == DataItemRole.Input)
            {
                if (dataItem.LinkedTo == null)
                {
                    LinkDataItems(dataItem, Model.GetDataItemByValue(output), true);
                    return;
                }

                bool result = inquiryHelper.InquireContinuation(Resources.RealTimeControlModelNodePresenter_OutputLocationWarningMessage);

                if (!result)
                {
                    return;
                }

                LinkDataItems(dataItem, Model.GetDataItemByValue(output), true);
            }

            shape.Text = dataItem.ToString();
            graphControl.NetronGraph.Invalidate();
        }

        public void EnsureVisible(object item)
        {
            // on interface
        }

        private void graphControl_OnShowProperties(object sender, object[] props)
        {
            // Hack : need to do this after selection is changed (after mouse up is handled)
            ValidateButtons();
        }

        private void GraphControlOnOnShapeRemove(object sender, Shape shape)
        {
            if (shape is ShapeBase shapeBase)
            {
                shapes.Remove(shapeBase);
            }

            UpdateShapesInViewContext();
            ValidateButtons();
        }

        private void GraphControlOnOnShapeAdded(object sender, Shape shape)
        {
            if (shape is ShapeBase shapeBase)
            {
                shapes.Add(shapeBase);
            }

            UpdateShapesInViewContext();
            ValidateButtons();
        }

        private void GraphControlMouseMove(object sender, MouseEventArgs e)
        {
            var shape = TypeUtils.GetField(graphControl.NetronGraph, "Hover") as ShapeBase;
            graphControl.NetronGraph.ToolTip.ToolTipTitle = shape != null ? shape.Title : "";

            if (e.Button == MouseButtons.Left)
            {
                FixNegativeLocationsOfSelectedShapes();
            }
        }

        private void FixNegativeLocationsOfSelectedShapes()
        {
            IEnumerable<ShapeBase> selectedShapes = graphControl.GetSelectedShapes<ShapeBase>();

            foreach (ShapeBase selectedShape in selectedShapes)
            {
                RectangleF r = selectedShape.Rectangle;
                if (r.Left >= 0 && r.Top >= 0)
                {
                    continue;
                }

                float left = r.Left < 0 ? 0.0f : r.Left;
                float top = r.Top < 0 ? 0.0f : r.Top;

                selectedShape.Rectangle = new RectangleF(left, top, r.Width, r.Height);
                graphControl.NetronGraph.Refresh();
            }
        }

        private void ValidateButtons()
        {
            bool canAlign = graphControl.GetSelectedShapes<ShapeBase>().Count() > 1;
            toolStripButtonAlignCenter.Enabled = canAlign;
            toolStripButtonAlignMiddle.Enabled = canAlign;
            toolStripButtonMakeSameHeight.Enabled = canAlign;
            toolStripButtonMakeSameWidth.Enabled = canAlign;
        }

        private void UpdateShapesInViewContext()
        {
            context.ShapeList.Clear();

            foreach (ShapeBase shape in shapes)
            {
                context.ShapeList.Add(shape);
            }
        }

        private void OnShowProperties(object sender, object[] props)
        {
            if (null == Gui)
            {
                Log.WarnFormat("OnShowProperties: Gui not set.");
                return;
            }

            try
            {
                if (lastCreated != null)
                {
                    Gui.Selection = lastCreated;
                    lastCreated = null;
                    return;
                }

                if (props.Length == 1 && props[0] is PropertyBag)
                {
                    var propertyBag = (PropertyBag) props[0];
                    Gui.Selection = propertyBag.Owner.Tag;
                    return;
                }

                Gui.Selection = null;
            }
            catch (Exception exception)
            {
                Log.ErrorFormat("Error linking shape to object; {0}", exception.Message);
            }
        }

        private void OnGraphControlContextMenu(object sender, MouseEventArgs e)
        {
            graphControl.ContextMenuItems.Clear();
            List<ShapeBase> selectedShapes = graphControl.GetSelectedShapes<ShapeBase>().ToList();

            if (selectedShapes.Count == 1 && Model != null)
            {
                ShapeBase selectedShape = selectedShapes[0];
                object tag = selectedShape.Tag;

                if (tag is Input)
                {
                    List<IFeature> locations =
                        Model.GetChildDataItemLocationsFromControlledModels(DataItemRole.Output).ToList();

                    if (locations.Count <= MaxLocationsToDisplayIndividually) // DELFT3DFM-1165
                    {
                        // retrieve locations from controlled models where values are available
                        SetLocationsToContextMenu("Input locations", locations, InputLocationClick, selectedShape);
                    }

                    graphControl.ContextMenuItems.Add(new MenuItem("Choose input locations...",
                                                                   (s, ev) => OpenInputDialog(
                                                                       DataItemRole.Output, "Select Input")));
                }

                if (tag is Output)
                {
                    List<IFeature> locations =
                        Model.GetChildDataItemLocationsFromControlledModels(DataItemRole.Input).ToList();

                    if (locations.Count <= MaxLocationsToDisplayIndividually) // DELFT3DFM-1165
                    {
                        // retrieve locations from controlled models where values can be set
                        SetLocationsToContextMenu("Output locations", locations, OutputLocationClick,
                                                  selectedShape);
                    }

                    graphControl.ContextMenuItems.Add(new MenuItem("Choose output locations...",
                                                                   (s, ev) => OpenInputDialog(
                                                                       DataItemRole.Input, "Select Output")));
                }

                if (tag is ConditionBase)
                {
                    graphControl.ContextMenuItems.Add(
                        new MenuItem("Copy xml to clipboard", CopyXmlToClipboard) {Tag = tag});
                    SetConvertConditionToContextMenu(selectedShape);
                }

                if (tag is RuleBase)
                {
                    graphControl.ContextMenuItems.Add(
                        new MenuItem("Copy xml to clipboard", CopyXmlToClipboard) {Tag = tag});
                    SetConvertRuleToContextMenu(selectedShape);
                }

                if (tag is SignalBase || tag is MathematicalExpression)
                {
                    graphControl.ContextMenuItems.Add(
                        new MenuItem("Copy xml to clipboard", CopyXmlToClipboard) {Tag = tag});
                }
            }

            if (selectedShapes.Count >= 1)
            {
                graphControl.ContextMenuItems.Add(new MenuItem("Copy", CopyAction) {Tag = selectedShapes});
            }

            var helper = RealTimeControlModelCopyPasteHelper.Instance;
            if (helper.IsDataSet && !selectedShapes.Any())
            {
                graphControl.ContextMenuItems.Add(new MenuItem("Paste", PasteAction));
            }

            if (!selectedShapes.Any())
            {
                graphControl.ContextMenuItems.Add(new MenuItem("Copy as image", CopyAsImageToClipboard));
                graphControl.ContextMenuItems.Add(new MenuItem("Save as image...", SaveAsImageAction));
            }
        }

        private void OpenInputDialog(DataItemRole role, string title)
        {
            var dialog = new InputSelectionDialog
            {
                Text = title,
                Features = Model.GetChildDataItemLocationsFromControlledModels(role).ToList(),
                GetDataItemsForFeature = location =>
                    Model.GetChildDataItemsFromControlledModelsForLocation(location)
                         .Where(di => (di.Role & role) == role)
                         .ToList()
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            Link(graphControl.GetSelectedShapes<ShapeBase>().First(), dialog.SelectedDataItem, inquiryHelper);
            graphControl.Refresh();
        }

        private void PasteAction(object sender, EventArgs e)
        {
            Point mea = PointToClient(MousePosition);
            var helper = RealTimeControlModelCopyPasteHelper.Instance;
            if (helper.IsDataSet)
            {
                helper.CopyShapesToController(controller, mea);
                helper.ClearData();
            }
        }

        private void CopyAction(object sender, EventArgs e)
        {
            var menuItem = (MenuItem) sender;
            var helper = RealTimeControlModelCopyPasteHelper.Instance;
            helper.SetCopiedData((IEnumerable<ShapeBase>) menuItem.Tag);
        }

        private void CopyAsImageToClipboard(object sender, EventArgs e)
        {
            Clipboard.SetImage(graphControl.NetronGraph.GetDiagramImage());
        }

        private void SaveAsImageAction(object sender, EventArgs e)
        {
            string tempImagePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            graphControl.NetronGraph.SaveImage(tempImagePath, true);

            using (Image image = Image.FromFile(tempImagePath))
            using (var dialog = new SaveFileDialog
            {
                Title = Resources.ControlGroupEditor_SaveAsImageAction_Save_image,
                Filter = Resources.ControlGroupEditor_SaveAsImageAction_PNG_image___png
            })
            {
                dialog.ShowDialog();

                if (dialog.FileName != string.Empty)
                {
                    image.Save(dialog.FileName, ImageFormat.Png);
                }
            }
        }

        private void SetConvertRuleToContextMenu(Shape shape)
        {
            var ruleBase = (RuleBase) shape.Tag;
            IEnumerable<Type> rulesToConvertTo = null;
            if (ruleBase.IsLinkedFromSignal())
            {
                rulesToConvertTo = RuleProvider.GetAllRules().Where(rt => rt != ruleBase.GetType())
                                               .Where(rt => ((RuleBase) Activator.CreateInstance(rt))
                                                            .CanBeLinkedFromSignal() ==
                                                            ruleBase.CanBeLinkedFromSignal());
            }
            else
            {
                rulesToConvertTo = RuleProvider.GetAllRules().Where(rt => rt != ruleBase.GetType());
            }

            graphControl.ContextMenuItems.Add(
                string.Format("Convert {0} to", RuleProvider.GetTitle(ruleBase.GetType())),
                rulesToConvertTo.Select
                (
                    ruleType =>
                        new MenuItem(RuleProvider.GetTitle(ruleType), OnConvertRuleType)
                        {
                            Tag = new DelftTools.Utils.Tuple<Shape, Type>(
                                shape, ruleType)
                        }
                ).ToArray());
        }

        private void SetConvertConditionToContextMenu(Shape shape)
        {
            var conditionBase = (ConditionBase) shape.Tag;
            graphControl.ContextMenuItems.Add(
                string.Format("Convert {0} to", ConditionProvider.GetTitle(conditionBase.GetType())),
                ConditionProvider.GetAllConditions().Where(rt => rt != conditionBase.GetType()).Select
                (
                    conditionType =>
                        new MenuItem(ConditionProvider.GetTitle(conditionType),
                                     OnConvertConditionType)
                        {
                            Tag = new DelftTools.Utils.Tuple<Shape, Type>(
                                shape, conditionType)
                        }
                ).ToArray());
        }

        private void OnConvertRuleType(object sender, EventArgs e)
        {
            var menuItem = (MenuItem) sender;
            var tuple = (DelftTools.Utils.Tuple<Shape, Type>) menuItem.Tag;
            var ruleBase = (RuleBase) tuple.First.Tag;
            Type newType = tuple.Second;

            controlGroup.BeginEdit(string.Format("Converting condition from {0} to {1}",
                                                 ruleBase.GetType(), newType));
            Gui.Selection = controller.ConvertRuleTypeTo(ruleBase, newType);
            controlGroup.EndEdit();
        }

        private void OnConvertConditionType(object sender, EventArgs e)
        {
            var menuItem = (MenuItem) sender;
            var tuple = (DelftTools.Utils.Tuple<Shape, Type>) menuItem.Tag;
            var conditionBase = (ConditionBase) tuple.First.Tag;
            Type newType = tuple.Second;

            controlGroup.BeginEdit(string.Format("Converting condition from {0} to {1}",
                                                 conditionBase.GetType(), newType));
            Gui.Selection = controller.ConvertConditionTypeTo(conditionBase, newType);
            controlGroup.EndEdit();
        }

        private void SetLocationsToContextMenu(string subMenuText, IEnumerable<IFeature> locations,
                                               EventHandler OnClick, Shape shape)
        {
            IEnumerable<IGrouping<Type, IFeature>> featureTypes = locations.GroupBy(t => t.GetEntityType());

            var items = new List<MenuItem>();

            var action = new DelftTools.Utils.Tuple<Shape, EventHandler>(shape, OnClick);

            const int maxPerGroup = 30;
            foreach (IGrouping<Type, IFeature> featureType in featureTypes)
            {
                List<IFeature> choosableFeatures =
                    featureType.Where(f => ContainsDataItemsForShape(f, shape))
                               .OrderBy(f => f.ToString(), new AlphanumComparator()) //sort 'natural order' iso ASCII
                               .ToList();

                if (choosableFeatures.Count == 0)
                {
                    continue;
                }

                var item = new MenuItem(featureType.Key.Name);
                items.Add(item);

                List<IList<IFeature>>
                    featureGroups =
                        choosableFeatures.SplitInGroups(maxPerGroup).ToList(); //split in to groups of 30 items

                if (featureGroups.Count == 1) //only one group, so just a few items: create subitems directly
                {
                    featureGroups[0].ForEach(f => AddContextMenuItemForFeature(f, action, item));
                }
                else // many items, add some nesting to limit number of items per list
                {
                    foreach (IList<IFeature> group in featureGroups)
                    {
                        string groupName = group.First() + " -> " + group.Last();
                        var groupItem = new MenuItem(groupName);
                        item.MenuItems.Add(groupItem);
                        group.ForEach(f => AddContextMenuItemForFeature(f, action, groupItem));
                    }
                }
            }

            if (items.Any())
            {
                graphControl.NetronGraph.ContextMenu.MenuItems.Add(subMenuText, items.ToArray());
            }
        }

        private void AddContextMenuItemForFeature(IFeature subEntry, DelftTools.Utils.Tuple<Shape, EventHandler> action,
                                                  MenuItem itemToAddTo)
        {
            var subItem = new MenuItem(subEntry.ToString())
            {
                Tag = new DelftTools.Utils.Tuple<DelftTools.Utils.Tuple<Shape, EventHandler>, IFeature>(
                    action, subEntry)
            };
            subItem.Popup += LocationDataItemsPopup;
            subItem.MenuItems.Add(new MenuItem("{dummy}"));
            itemToAddTo.MenuItems.Add(subItem);
        }

        private bool ContainsDataItemsForShape(IFeature feature, Shape shape)
        {
            if (shape is OutputItemShape)
            {
                return Model.GetChildDataItemsFromControlledModelsForLocation(feature).Any(
                    di => (di.Role & DataItemRole.Input) == DataItemRole.Input);
            }

            if (shape is InputItemShape)
            {
                return Model.GetChildDataItemsFromControlledModelsForLocation(feature).Any(
                    di => (di.Role & DataItemRole.Output) == DataItemRole.Output);
            }

            return false;
        }

        private void LocationDataItemsPopup(object sender, EventArgs e)
        {
            var menuItem = (MenuItem) sender;
            menuItem.MenuItems.Clear();
            var tuple = (DelftTools.Utils.Tuple<DelftTools.Utils.Tuple<Shape, EventHandler>, IFeature>) menuItem.Tag;
            IFeature location = tuple.Second;
            DelftTools.Utils.Tuple<Shape, EventHandler> shapeEventHandlerTuple = tuple.First;
            Shape shape = shapeEventHandlerTuple.First;

            var dataItems = new List<IDataItem>();

            if (shape is OutputItemShape)
            {
                dataItems = Model.GetChildDataItemsFromControlledModelsForLocation(location).Where(
                    di => (di.Role & DataItemRole.Input) == DataItemRole.Input).ToList();
            }
            else if (shape is InputItemShape)
            {
                dataItems = Model.GetChildDataItemsFromControlledModelsForLocation(location).Where(
                    di => (di.Role & DataItemRole.Output) == DataItemRole.Output).ToList();
            }

            foreach (
                MenuItem subMenuItem in
                dataItems.Select(
                    dataItem => new MenuItem(dataItem.GetParameterName(), shapeEventHandlerTuple.Second) {Tag = new DelftTools.Utils.Tuple<Shape, IDataItem>(shape, dataItem)}))
            {
                menuItem.MenuItems.Add(subMenuItem);
            }
        }

        private void InputLocationClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem) sender;
            var tuple = (DelftTools.Utils.Tuple<Shape, IDataItem>) menuItem.Tag;
            Link(tuple.First, tuple.Second, inquiryHelper);
            graphControl.Refresh();
        }

        private void OutputLocationClick(object sender, EventArgs e)
        {
            var menuItem = (MenuItem) sender;
            var tuple = (DelftTools.Utils.Tuple<Shape, IDataItem>) menuItem.Tag;
            Link(tuple.First, tuple.Second, inquiryHelper);
        }

        private void LinkDataItems(IDataItem target, IDataItem source, bool unlinkExisting = false)
        {
            Model.BeginEdit(new DefaultEditAction(string.Format("Linking {0} to {1}", target, source)));

            // unlink any existing items connected to this item
            if (unlinkExisting)
            {
                foreach (IDataItem linkee in source.LinkedBy.ToList())
                {
                    linkee.Unlink();
                }
            }

            target.LinkTo(source);

            Model.EndEdit();
        }

        private void GraphControlOnDoubleClick(object sender, object[] props)
        {
            return; //hides the properties of the shape (color, line witdh, font etc.
        }

        private static IFeature GetFeatureFromDragEvents(DragEventArgs dragEventArgs)
        {
            var dataObject = (DataObject) dragEventArgs.Data;
            string[] formats = dataObject.GetFormats();
            return formats.Select(dataObject.GetData).OfType<IFeature>().FirstOrDefault();
        }

        private bool OnGraphControlGraphDragOver(object sender, DragEventArgs dragEventArgs)
        {
            Point point = PointToClient(new Point(dragEventArgs.X, dragEventArgs.Y));
            IFeature feature = GetFeatureFromDragEvents(dragEventArgs);

            if (feature != null)
            {
                Entity entity = graphControl.NetronGraph.HitEntity(point);
                if (entity is Shape)
                {
                    if (CanLinkFeatureToShape(dragEventArgs, feature, entity, typeof(InputItemShape),
                                              DataItemRole.Output))
                    {
                        return true;
                    }

                    if (CanLinkFeatureToShape(dragEventArgs, feature, entity, typeof(OutputItemShape),
                                              DataItemRole.Input))
                    {
                        return true;
                    }
                }

                dragEventArgs.Effect = DragDropEffects.None;
                return true;
            }

            return false;
        }

        private bool OnGraphControlGraphDragDrop(object sender, DragEventArgs dragEventArgs)
        {
            Point point = PointToClient(new Point(dragEventArgs.X, dragEventArgs.Y));
            IFeature feature = GetFeatureFromDragEvents(dragEventArgs);
            if (feature != null)
            {
                Entity entity = graphControl.NetronGraph.HitEntity(point);

                if (entity is Shape)
                {
                    if (entity is InputItemShape &&
                        CanLinkFeatureToShape(dragEventArgs, feature, entity, typeof(InputItemShape),
                                              DataItemRole.Output))
                    {
                        DropFeatureOnShape(feature, entity, DataItemRole.Output);
                    }

                    if (entity is OutputItemShape &&
                        CanLinkFeatureToShape(dragEventArgs, feature, entity, typeof(OutputItemShape),
                                              DataItemRole.Input))
                    {
                        DropFeatureOnShape(feature, entity, DataItemRole.Input);
                    }
                }

                dragEventArgs.Effect = DragDropEffects.None;
                return true;
            }

            return false;
        }

        private void DropFeatureOnShape(IFeature feature, Entity entity, DataItemRole role)
        {
            IEnumerable<IDataItem> dataItems = Model.GetChildDataItemsFromControlledModelsForLocation(feature);
            string[] choices = dataItems.Where(e => (e.Role & role) == role).Select(e => e.GetParameterName())
                                        .ToArray();
            string answer;
            if (choices.Count() > 1)
            {
                var dialog = new ListBasedDialog
                {
                    DataSource = choices,
                    SelectionMode = SelectionMode.One
                };
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    answer = (string) dialog.SelectedItems[0];
                }
                else
                {
                    return;
                }
            }
            else
            {
                answer = choices.FirstOrDefault();
            }

            IDataItem dataItem = dataItems.FirstOrDefault(e => e.GetParameterName() == answer);
            Link((Shape) entity, dataItem, inquiryHelper);
        }

        private bool CanLinkFeatureToShape(DragEventArgs dragEventArgs, IFeature feature, Entity entity, Type t,
                                           DataItemRole role)
        {
            if (entity.GetType() == t && Model.GetChildDataItemLocationsFromControlledModels(role).Contains(feature))
            {
                dragEventArgs.Effect = DragDropEffects.Link;
                return true;
            }

            return false;
        }

        private void OnTsbConditionClick(object sender, EventArgs e)
        {
            created = new StandardCondition {Name = NamingHelper.GetUniqueName("condition{0:D2}", controlGroup.Conditions, null)};
            ResetNewObjectButtons();
            tsbCondition.CheckState = CheckState.Checked;
        }

        private void OnTsbRuleClick(object sender, EventArgs e)
        {
            created = new PIDRule {Name = NamingHelper.GetUniqueName("rule{0:D2}", controlGroup.Rules, null)};
            ResetNewObjectButtons();
            tsbRule.CheckState = CheckState.Checked;
        }

        private void OnTsbOutputClick(object sender, EventArgs e)
        {
            created = new Output();
            ResetNewObjectButtons();
            tsbOutput.CheckState = CheckState.Checked;
        }

        private void OnTsbInputClick(object sender, EventArgs e)
        {
            created = new Input();
            ResetNewObjectButtons();
            tsbInput.CheckState = CheckState.Checked;
        }

        private void OnTsbMathExpressionSignalClick(object sender, EventArgs e)
        {
            created = new MathematicalExpression {Name = NamingHelper.GetUniqueName("f{0}", controlGroup.MathematicalExpressions, null)};
            ResetNewObjectButtons();
            tsbMathExpression.CheckState = CheckState.Checked;
        }

        private void OnTsbSignalClick(object sender, EventArgs e)
        {
            created = new LookupSignal {Name = NamingHelper.GetUniqueName("lookup table{0:D2}", controlGroup.Signals, null)};
            ResetNewObjectButtons();
            tsbSignal.CheckState = CheckState.Checked;
        }

        private void OnGraphControlMouseUp(object sender, MouseEventArgs e)
        {
            if (created != null)
            {
                var objecten = new List<object> {created};
                List<RuleBase> rule = objecten.Where(c => c is RuleBase).Cast<RuleBase>().ToList();
                List<ConditionBase> condition = objecten.Where(c => c is ConditionBase).Cast<ConditionBase>().ToList();
                List<Input> input = objecten.Where(c => c is Input).Cast<Input>().ToList();
                List<Output> output = objecten.Where(c => c is Output).Cast<Output>().ToList();
                List<SignalBase> signal = objecten.Where(c => c is SignalBase).Cast<SignalBase>().ToList();
                List<MathematicalExpression> mathExpressions = objecten.Where(c => c is MathematicalExpression).Cast<MathematicalExpression>().ToList();
                controller.AddShapesToControlGroupAndPlace(rule, condition, input, output, signal, mathExpressions,
                                                           new Point((int) (e.X / graphControl.NetronGraph.Zoom),
                                                                     (int) (e.Y / graphControl.NetronGraph.Zoom)));

                lastCreated = created;
                created = null;

                ResetNewObjectButtons();
            }

            IEnumerable<ShapeBase> graphShapes = graphControl.GetShapes<ShapeBase>();
            if (graphShapes != null)
            {
                foreach (ShapeBase shape in graphShapes)
                {
                    shape.HighLightedConnectors = null;
                }
            }
        }

        private void OnGraphControlMouseDown(object sender, MouseEventArgs e)
        {
            object hoveredItem = TypeUtils.GetField(graphControl.NetronGraph, "Hover");
            if (!(hoveredItem is Connector activeConnector))
            {
                return;
            }

            IEnumerable<ShapeBase> shapesOnGraph = graphControl.GetShapes<ShapeBase>();
            if (shapesOnGraph == null)
            {
                return;
            }

            ShapeBase[] shapeBases = shapesOnGraph.ToArray();
            Connector[] allConnectors = shapeBases.SelectMany(s => s.Connectors.Cast<Connector>()).ToArray();
            var owner = activeConnector.BelongsTo as ShapeBase;

            ConnectorType activeConnectionType = GetActiveConnectionType(owner, activeConnector);
            IEnumerable<Connector> allowedConnectors = FilterAllowableConnectors(owner, activeConnectionType, allConnectors);

            if (!allowedConnectors.Any())
            {
                return;
            }

            foreach (ShapeBase shape in shapeBases)
            {
                shape.HighLightedConnectors = allowedConnectors;
            }
        }

        private static IEnumerable<Connector> FilterAllowableConnectors(ShapeBase sourceShape,
                                                                        ConnectorType sourceConnection,
                                                                        IEnumerable<Connector> availableConnectors)
        {

            if (sourceShape is MathematicalExpressionShape && (sourceConnection == ConnectorType.Left || sourceConnection == ConnectorType.Top))
            {
                return Enumerable.Empty<Connector>();
            }

            var allowedConnectors = new List<Connector>();
            foreach (Connector availableConnector in availableConnectors)
            {
                var targetShape = availableConnector.BelongsTo as ShapeBase;

                ConnectorType targetConnectionType = GetActiveConnectionType(targetShape, availableConnector);
                if (ShapeConnectionsRulesController.IsShapeCompatibleWithTarget(sourceShape, targetShape, targetConnectionType))
                {
                    allowedConnectors.Add(availableConnector);
                }
            }

            return allowedConnectors;
        }

        private static ConnectorType GetActiveConnectionType(ShapeBase owner, Connector activeConnector)
        {
            ConnectorType activeConnectionType = owner is MathematicalExpressionShape
                                                     ? ConvertConnectorNameToType(activeConnector.Name)
                                                     : ConvertTo(activeConnector.ConnectorLocation);
            return activeConnectionType;
        }

        private static ConnectorType ConvertConnectorNameToType(string connectorName)
        {
            switch (connectorName)
            {
                case "Left":
                    return ConnectorType.Left;
                case "Top":
                    return ConnectorType.Top;
                case "Bottom":
                    return ConnectorType.Bottom;
                default:
                    throw new NotSupportedException();
            }
        }

        private static ConnectorType ConvertTo(ConnectorLocation connectorLocation)
        {
            switch (connectorLocation)
            {
                case ConnectorLocation.North:
                    return ConnectorType.Top;
                case ConnectorLocation.East:
                    return ConnectorType.Right;
                case ConnectorLocation.West:
                    return ConnectorType.Left;
                case ConnectorLocation.South:
                    return ConnectorType.Bottom;
                default:
                    throw new NotSupportedException();
            }
        }

        private void ResetNewObjectButtons()
        {
            tsbInput.CheckState = CheckState.Unchecked;
            tsbCondition.CheckState = CheckState.Unchecked;
            tsbRule.CheckState = CheckState.Unchecked;
            tsbOutput.CheckState = CheckState.Unchecked;
            tsbSignal.CheckState = CheckState.Unchecked;
            tsbMathExpression.CheckState = CheckState.Unchecked;
        }

        private void ToolStripButtonAlignCenterClick(object sender, EventArgs e)
        {
            float center = graphControl.GetSelectedShapes<ShapeBase>().Average(s => (s.Width / 2) + s.Rectangle.X);

            DoWithSelectedShapes((shape, currentRectangle) =>
            {
                shape.Rectangle = new RectangleF(center - (currentRectangle.Width / 2),
                                                 currentRectangle.Y, currentRectangle.Width,
                                                 currentRectangle.Height);
            });
        }

        private void ToolStripButtonAlignMiddleClick(object sender, EventArgs e)
        {
            float middle = graphControl.GetSelectedShapes<ShapeBase>().Average(s => (s.Height / 2) + s.Rectangle.Y);

            DoWithSelectedShapes((shape, currentRectangle) =>
            {
                shape.Rectangle = new RectangleF(currentRectangle.X,
                                                 middle - (currentRectangle.Height / 2),
                                                 currentRectangle.Width, currentRectangle.Height);
            });
        }

        private void ToolStripButtonMakeSameHeightClick(object sender, EventArgs e)
        {
            if (toolStripButtonResize.Checked)
            {
                toolStripButtonResize.Checked = false;
            }

            float maxHeight = graphControl.GetSelectedShapes<ShapeBase>().Max(s => s.Rectangle.Height);

            DoWithSelectedShapes((shape, currentRectangle) =>
            {
                shape.Rectangle = new RectangleF(currentRectangle.X, currentRectangle.Y,
                                                 currentRectangle.Width, maxHeight);
            });
        }

        private void ToolStripButtonMakeSameWidthClick(object sender, EventArgs e)
        {
            if (toolStripButtonResize.Checked)
            {
                toolStripButtonResize.Checked = false;
            }

            float maxWidth = graphControl.GetSelectedShapes<ShapeBase>().Max(s => s.Rectangle.Width);

            DoWithSelectedShapes((shape, currentRectangle) =>
            {
                shape.Rectangle = new RectangleF(currentRectangle.X, currentRectangle.Y, maxWidth,
                                                 currentRectangle.Height);
            });
        }

        private void DoWithSelectedShapes(Action<ShapeBase, RectangleF> shapeAction)
        {
            graphControl.NetronGraph.SuspendLayout();

            foreach (ShapeBase shape in graphControl.GetSelectedShapes<ShapeBase>())
            {
                shapeAction(shape, shape.Rectangle);
            }

            graphControl.NetronGraph.Invalidate();
            graphControl.NetronGraph.ResumeLayout(true);
        }

        private void ToolStripButtonResizeCheckedChanged(object sender, EventArgs e)
        {
            foreach (ShapeBase shape in graphControl.GetShapes<ShapeBase>())
            {
                shape.AutoResize = toolStripButtonResize.Checked;
            }

            if (context != null)
            {
                context.AutoSize = toolStripButtonResize.Checked;
            }

            graphControl.NetronGraph.Invalidate();
        }
    }
}