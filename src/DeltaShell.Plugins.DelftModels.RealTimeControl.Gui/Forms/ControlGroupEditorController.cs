using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using log4net;
using Netron.GraphLib;
using Netron.GraphLib.Interfaces;
using Netron.GraphLib.UI;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public enum ConnectorType
    {
        Left, Top, Right, Bottom
    }

    public class ControlGroupEditorController
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ControlGroupEditorController));

        private ControlGroup controlGroup;
       
        public ControlGroup ControlGroup
        {
            get { return controlGroup; }
            set
            {
                DesubscribeControlGroupEvents();
                controlGroup = value;
                InitialGraphFill();
                SubscribeControlGroupEvents();
            }
        }

        private void InitialGraphFill()
        {
            DesubscribeGraphControlEvents();
            CleanGraphControl();
            if (controlGroup != null)
            {
                PlaceShapes(controlGroup.Rules, controlGroup.Conditions, controlGroup.Inputs, controlGroup.Outputs, controlGroup.Signals);
                AddConnections(controlGroup.Rules, controlGroup.Conditions, controlGroup.Signals);    
            }
            SubscribeGraphControlEvents();
        }

        private void PlaceShapes(IEventedList<RuleBase> rules, IEventedList<ConditionBase> conditions, IEventedList<Input> inputs, 
                                 IEventedList<Output> outputs, IEventedList<SignalBase> signals)
        {
            Point point = new Point() {X = 0, Y = 0};
            PlaceShapes(rules, conditions, inputs, outputs, signals, point);
        }

        private void CleanGraphControl()
        {
            if (graphControl != null)
            {
                graphControl.Shapes.Clear();
                graphControl.Connections.Clear();
            }
        }

        public void PlaceShapes(IList<RuleBase> rules, IList<ConditionBase> conditions, IList<Input> inputs, IList<Output> outputs, IList<SignalBase> signals, Point mea)
        {
            int x = mea.X;
            int y = mea.Y;
            var useOffset = (rules.Count + conditions.Count + inputs.Count + outputs.Count) > 1;
            if ((controlGroup != null) && (graphControl != null))
            {
                for (var i = 0; i < inputs.Count; i++)
                {
                    PlaceShapeOnGraphControl(inputs[i], useOffset     ? (x + 10) : x, useOffset ? (y + 10 + (50 * i)) : y);
                }
                for (var i = 0; i < outputs.Count; i++)
                {
                    PlaceShapeOnGraphControl(outputs[i], useOffset    ? (x + 410) : x, useOffset ? (y + 10 + (50 * i)) : y);
                }
                for (var i = 0; i < rules.Count; i++)
                {
                    PlaceShapeOnGraphControl(rules[i],useOffset       ? (x + 250) : x,useOffset ?  (y + 10 + (50 * i)) : y);
                }
                for (var i = 0; i < conditions.Count; i++)
                {
                    PlaceShapeOnGraphControl(conditions[i], useOffset ? (x + 110) : x, useOffset ? (y + 10 + (50 * i)) : y);
                }
                for (var i = 0; i < signals.Count; i++)
                {
                    PlaceShapeOnGraphControl(signals[i], useOffset    ? (x + 250) : x, useOffset ? (y + 110 + (50 * i)) : y);
                }
            }
        }

        public void AddShapesToControlGroupAndPlace(IList<RuleBase> rules, IList<ConditionBase> conditions, IList<Input> inputs, 
                                                    IList<Output> outputs, IList<SignalBase> signals, Point mea)
        {
            DesubscribeControlGroupEvents();
            DesubscribeGraphControlEvents();
            if ((controlGroup != null) && (graphControl != null))
            {
                for (var i = 0; i < inputs.Count; i++)
                {
                    controlGroup.Inputs.Add(inputs[i]);
                }
                for (var i = 0; i < outputs.Count; i++)
                {
                    controlGroup.Outputs.Add(outputs[i]);
                }
                for (var i = 0; i < rules.Count; i++)
                {
                    controlGroup.Rules.Add(rules[i]);
                }
                for (var i = 0; i < conditions.Count; i++)
                {
                    controlGroup.Conditions.Add(conditions[i]);
                }
                for (var i = 0; i < signals.Count; i++)
                {
                    controlGroup.Signals.Add(signals[i]);
                }
                PlaceShapes(rules, conditions, inputs, outputs, signals, mea);

                SubscribeControlGroupEvents();
                SubscribeGraphControlEvents();
            }
        }

        internal void PlaceShapeOnGraphControl(object obj, double x, double y)
        {
            var shape = ObjectToShape(obj);
            graphControl.AddShape(shape);
            MoveShape(shape, x, y);
        }

        public void AddConnections(IList<RuleBase> rules, IList<ConditionBase> conditions, IList<SignalBase> signals, bool skipValidation = false)
        {
            if (skipValidation) DesubscribeGraphControlEvents(); // Bypass OnGraphControlConnectionAdded event
            if ((controlGroup != null) && (graphControl != null))
            {
                // ToList for local copy (eventedlist causes invalid modification of source)
                foreach (var condition in conditions.ToList())
                {
                    SetUiConditionConnections(condition);
                }
                foreach (var ruleBase in rules.ToList())
                {
                    SetUiRuleConnections(ruleBase);
                }
                foreach (var signalBase in signals.ToList())
                {
                    SetUiSignalConnections(signalBase);
                }
            }
            if (skipValidation) SubscribeGraphControlEvents();
        }

        internal void SetUiRuleConnections(RuleBase ruleBase)
        {
            foreach (var input in ruleBase.Inputs.ToList())
            {
                UiConnect(input, "Bottom", ruleBase, "Top");
            }
            foreach (var output in ruleBase.Outputs.ToList())
            {
                UiConnect(ruleBase, "Right", output, "Left");
            }
        }

        internal void SetUiSignalConnections(SignalBase signalBase)
        {
            foreach (var input in signalBase.Inputs.ToList())
            {
                UiConnect(input, "Bottom", signalBase, "Top");
            }
            foreach (var rulebase in signalBase.RuleBases.ToList())
            {
                if (rulebase.CanBeLinkedFromSignal())
                {
                    UiConnect(signalBase, "Right", rulebase, "Bottom");
                }
            }
        }

        internal void SetUiConditionConnections(ConditionBase condition)
        {
            if (condition.Input != null)
            {
                UiConnect(condition.Input, "Bottom", condition, "Top");
            }
            foreach (var output in condition.FalseOutputs.ToList())
            {
                UiConnect(condition, "Bottom", output, "Left");
            }
            foreach (var output in condition.TrueOutputs.ToList())
            {
                UiConnect(condition, "Right", output, "Left");
            }
        }

       
        /// <summary>
        /// Connects 2 shapes.
        /// </summary>
        /// <param name="source">
        /// The object where the connection starts
        /// </param>
        /// <param name="sourceConnector">
        /// The connector in the source shape where connnection starts
        /// </param>
        /// <param name="target">
        /// The object where the connection ends
        /// </param>
        /// <param name="targetConnector">
        /// The connector in the target shape where connnection ends
        /// </param>
        private void UiConnect(object source, string sourceConnector, object target, string targetConnector)
        {
            var leftShape = FindShapeByObject(source);
            var rightShape = FindShapeByObject(target);
            var connection = graphControl.AddConnection(leftShape.Connectors[sourceConnector], rightShape.Connectors[targetConnector]);
            SetConnectionStyle(connection);
        }

        private void SubscribeControlGroupEvents()
        {
            if (controlGroup != null)
            {
                ((INotifyCollectionChanged)controlGroup).CollectionChanged += ControlGroupCollectionChanged;
                ((INotifyCollectionChanging)controlGroup).CollectionChanging += ControlGroupCollectionChanging;
                ((INotifyPropertyChanged)controlGroup).PropertyChanged += ControlGroupPropertyChanged;
            }
        }
        
        private void DesubscribeControlGroupEvents()
        {
            if (controlGroup != null)
            {
                ((INotifyCollectionChanged)controlGroup).CollectionChanged -= ControlGroupCollectionChanged;
                ((INotifyCollectionChanging)controlGroup).CollectionChanging -= ControlGroupCollectionChanging;
                ((INotifyPropertyChanged)controlGroup).PropertyChanged -= ControlGroupPropertyChanged;
            }
        }

        private object replaceable;

        private void ControlGroupCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Replace)
            {
                replaceable = e.Item;
            }
        }

        private void ControlGroupPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is ConditionBase && e.PropertyName == "Input")
            {
                RefreshConnections();
            }

            if (sender is ControlGroup && e.PropertyName == "IsEditing")
            {
                if (!controlGroup.IsEditing)
                {
                    RefreshConnections();
                }
            }

            // force redrawing to fix rendering bug in netron (TOOLS-7748, point 5).
            if(sender is ConnectionPoint && e.PropertyName == "Name")
            {
                graphControl.Invalidate();
            }
        }

        private void ControlGroupCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (graphControl != null)
            {
                if (sender != controlGroup.Inputs && 
                    sender != controlGroup.Outputs && 
                    sender != controlGroup.Rules &&
                    sender != controlGroup.Conditions &&
                    sender != controlGroup.Signals)
                {
                    if (controlGroup.IsEditing)
                        return;
                    RefreshConnections();
                    return;
                }

                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    DesubscribeGraphControlEvents();
                    var shape = ObjectToShape(e.GetRemovedOrAddedItem());
                    graphControl.AddShape(shape);
                    SubscribeGraphControlEvents();
                }
                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    DesubscribeGraphControlEvents();
                    var shape = FindShapeByObject(e.GetRemovedOrAddedItem());
                    graphControl.Shapes.Remove(shape);
                    SubscribeGraphControlEvents();
                }
                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    if (replaceable != null)
                    {
                        var shape = FindShapeByObject(replaceable);
                        shape.Tag = e.GetRemovedOrAddedItem();
                        replaceable = null;
                    }
                }
                graphControl.Invalidate();
            }
        }

        private void RefreshConnections()
        {
            if (adjustingConnectionInDomain)
                return;

            refreshingConnections = true;

            RemoveAllConnections(graphControl.Connections);
            foreach (Shape shape in graphControl.Shapes)
            {
                foreach(Connector connector in shape.Connectors)
                {
                    RemoveAllConnections(connector.Connections);
                }
            }

            AddConnections(controlGroup.Rules, controlGroup.Conditions, controlGroup.Signals);

            graphControl.Invalidate();

            refreshingConnections = false;
        }

        private void RemoveAllConnections(ConnectionCollection connectionCollection)
        {
            //make copy of list, and then remove each individual item.
            var connections = connectionCollection.OfType<Netron.GraphLib.Connection>().ToList();
            foreach (var connection in connections)
            {
                connectionCollection.Remove(connection); //Clear doesn't raise events, bleh
            }
        }

        private GraphControl graphControl;
        private static bool adjustingConnectionInDomain;
        private static bool refreshingConnections;
        private static readonly Bitmap TimeConditionIcon = RealTimeControl.Properties.Resources.timecondition;
        private static readonly Bitmap DirectionalConditionIcon = RealTimeControl.Properties.Resources.directionalcondition;
        private static readonly Bitmap StandardConditionIcon = RealTimeControl.Properties.Resources.standardcondition;

        public GraphControl GraphControl
        {
            get { return graphControl; }
            set
            {
                if (graphControl != null)
                {
                    DesubscribeGraphControlEvents();
                }
                graphControl = value;
                if (graphControl != null)
                {
                    SubscribeGraphControlEvents();
                }
            }
        }

        public Func<bool> GetAutoResizeState { get; set; }

        private void SubscribeGraphControlEvents()
        {
            if (graphControl != null)
            {
                graphControl.Shapes.OnShapeRemoved += GraphControlShapesOnShapeRemoved;
                // see ControlGroupEditor::OnGraphControlMouseUp for OnShapeAdded processing
                graphControl.OnConnectionRemoved += OnGraphControlConnectionRemoved;
                graphControl.OnConnectionAdded += OnGraphControlConnectionAdded;
            }
        }

        private void DesubscribeGraphControlEvents()
        {
            if (graphControl != null)
            {
                graphControl.Shapes.OnShapeRemoved -= GraphControlShapesOnShapeRemoved;
                graphControl.OnConnectionRemoved -= OnGraphControlConnectionRemoved;
                graphControl.OnConnectionAdded -= OnGraphControlConnectionAdded;
            }
        }

        public void GraphControlShapesOnShapeRemoved(object sender, Shape shape)
        {
            if (shape != null)
            {
                DesubscribeControlGroupEvents();
                if (shape.Tag is RuleBase)
                {
                    controlGroup.Rules.Remove((RuleBase)shape.Tag);
                }
                if (shape.Tag is SignalBase)
                {
                    controlGroup.Signals.Remove((SignalBase)shape.Tag);
                }
                if (shape.Tag is ConditionBase)
                {
                    controlGroup.Conditions.Remove((ConditionBase)shape.Tag);
                }
                if (shape.Tag is Input)
                {
                    controlGroup.Inputs.Remove((Input)shape.Tag);
                }
                if (shape.Tag is Output)
                {
                    controlGroup.Outputs.Remove((Output)shape.Tag);
                }
                SubscribeControlGroupEvents();
            }
        }

        private ShapeBase FindShapeByObject(object obj)
        {
            if (graphControl == null)
            {
                return null;
            }
            return graphControl.Shapes.Cast<object>().Where(shape => ((Shape) shape).Tag == obj).Cast<ShapeBase>().FirstOrDefault();
        }

        public RuleBase ConvertRuleTypeTo(RuleBase oldRule, Type toType)
        {
            DesubscribeControlGroupEvents();

            var shape = FindShapeByObject(oldRule);

            var newRule = (RuleBase)Activator.CreateInstance(toType);
            newRule.Name = CopyOldNameOrGenerateNameForRule(oldRule.GetType(), toType, oldRule.Name);
            newRule.LongName = oldRule.LongName;
            //for nhibernate's sake the inputs and outputs must be mapped this way 
            foreach (var rul in oldRule.Inputs)
            {
                newRule.Inputs.Add(rul);
            }
            foreach (var rul in oldRule.Outputs)
            {
                newRule.Outputs.Add(rul);
            }

            controlGroup.Rules.Remove(oldRule);
            ReconnectRtcBaseObject(oldRule, newRule);
            controlGroup.Rules.Add(newRule);

            if (shape != null)
            {
                shape.Tag = newRule;
            }

            SubscribeControlGroupEvents();

            return newRule;
        }

        private static string CopyOldNameOrGenerateNameForRule(Type oldType, Type newType, string oldName)
        {
            var oldTypeTitle = RuleProvider.GetTitle(oldType);

            if (oldTypeTitle == oldName)
            {
                return RuleProvider.GetTitle(newType);
            }
            return oldName;
        }

        private static string CopyOldNameOrGenerateNameForCondition(Type oldType, Type newType, string oldName)
        {
            var oldTypeTitle = ConditionProvider.GetTitle(oldType);

            if (oldTypeTitle == oldName)
            {
                return ConditionProvider.GetTitle(newType);
            }
            return oldName;
        }

        private static string CopyOldNameOrGenerateNameForSignal(Type oldType, Type newType, string oldName)
        {
            var oldTypeTitle = SignalProvider.GetTitle(oldType);

            if (oldTypeTitle == oldName)
            {
                return SignalProvider.GetTitle(newType);
            }
            return oldName;
        }

        public ConditionBase ConvertConditionTypeTo(ConditionBase oldCondition, Type toType)
        {
            DesubscribeControlGroupEvents();
            var shape = FindShapeByObject(oldCondition);

            var newCondition = (ConditionBase)Activator.CreateInstance(toType);
            newCondition.Name = CopyOldNameOrGenerateNameForCondition(oldCondition.GetType(), toType, oldCondition.Name);
            newCondition.LongName = oldCondition.LongName;
            //for nhibernate's sake the inputs and outputs must be mapped this way 
            foreach (var trueOutput in oldCondition.TrueOutputs)
            {
                newCondition.TrueOutputs.Add(trueOutput);
            }
            foreach (var falseOutput in oldCondition.FalseOutputs)
            {
                newCondition.FalseOutputs.Add(falseOutput);
            }
            if (oldCondition.Input != null && newCondition.GetType() == typeof(TimeCondition))
            {
                for (int i = 0; i < graphControl.Connections.Count; i++)
                {
                    if (graphControl.Connections[i].To.BelongsTo is ConditionShape && graphControl.Connections[i].From.BelongsTo is InputItemShape)
                    {
                        graphControl.Connections.Remove(graphControl.Connections[i]);
                    }
                }
            }
            controlGroup.Conditions.Remove(oldCondition);
            ReconnectRtcBaseObject(oldCondition, newCondition);
            controlGroup.Conditions.Add(newCondition);

            if (shape != null)
            {
                shape.Tag = newCondition;
                FillConditionDescription(shape, newCondition);
            }
            SubscribeControlGroupEvents();

            return newCondition;
        }

        private void ReconnectRtcBaseObject(RtcBaseObject oldRtcObject, RtcBaseObject newRtcObject)
        {
            var falseOutputs = controlGroup.Conditions.Where(c => c.FalseOutputs.Contains(oldRtcObject));
            foreach (var conditionBase in falseOutputs)
            {
                conditionBase.FalseOutputs.Remove(oldRtcObject);
                conditionBase.FalseOutputs.Add(newRtcObject);
            }
            var trueOutputs = controlGroup.Conditions.Where(c => c.TrueOutputs.Contains(oldRtcObject));
            foreach (var conditionBase in trueOutputs)
            {
                conditionBase.TrueOutputs.Remove(oldRtcObject);
                conditionBase.TrueOutputs.Add(newRtcObject);
            }
            var signalRuleBases = controlGroup.Signals.Where(s => s.RuleBases.Contains(oldRtcObject));
            foreach (var signalBase in signalRuleBases)
            {
                signalBase.RuleBases.Remove((RuleBase)oldRtcObject);
                signalBase.RuleBases.Add((RuleBase)newRtcObject);
            }
        }

        public static void MoveShape(IShape shape, double x, double y)
        {
            if (shape != null)
            {
                shape.Location = new PointF((float)x, (float)y);
            }
        }

        public ShapeBase ObjectToShape(object obj)
        {
            ShapeBase shape = null;
            if (obj is RuleBase)
            {
                shape = new RuleShape { Text = ((RuleBase)obj).Name, Tag = obj };
            }
            if (obj is ConditionBase)
            {
                var conditionBase = (ConditionBase)obj;
                shape = new ConditionShape
                            {
                                Text = conditionBase.Name,
                                Tag = obj
                            };
                FillConditionDescription(shape, conditionBase);
            }
            if (obj is Input)
            {
                shape = new InputItemShape { Text = ((Input)obj).Name, Tag = obj };
            }
            if (obj is Output)
            {
                shape = new OutputItemShape { Text = ((Output)obj).Name, Tag = obj };
            }
            if (obj is SignalBase)
            {
                shape = new SignalShape { Text = ((SignalBase)obj).Name, Tag = obj };
            }

            if (shape != null && GetAutoResizeState != null)
            {
                shape.AutoResize = GetAutoResizeState();
            }

            return shape;
        }

        private static void FillConditionDescription(ShapeBase shape, ConditionBase condition)
        {
            if (shape is ConditionShape)
            {
                var conditionShape = shape as ConditionShape;
                conditionShape.Image = GetIconForCondition(condition);
                conditionShape.GetDescriptionDelegate = condition.GetDescription;
                if(condition is TimeCondition)
                {
                    conditionShape.DisableInputConnections();
                }
                else
                {
                    conditionShape.EnableInputConnections();
                }
            }
        }

        private static Bitmap GetIconForCondition(ConditionBase conditionBase)
        {
            if (conditionBase is TimeCondition)
            {
                return TimeConditionIcon;
            }
            if (conditionBase is DirectionalCondition)
            {
                return DirectionalConditionIcon;
            }
            if (conditionBase is StandardCondition)
            {
                return StandardConditionIcon;
            }
            return null;
        }

        private bool OnGraphControlConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            var connection = e.Connection;
            var from = connection.From.BelongsTo.Tag;
            var to = connection.To.BelongsTo.Tag;
            Disconnect(from, to);
            return true;
        }

        /// <summary>
        /// Validate if a newly added connection is allowed.
        /// Some options like a connection starts from an ouotput item are prevented in the Connectors in
        /// the shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool OnGraphControlConnectionAdded(object sender, ConnectionEventArgs e)
        {
            var connection = e.Connection;
            var from = connection.From.BelongsTo.Tag;
            var to = connection.To.BelongsTo.Tag;
            var fromConnector = (ConnectorType)Enum.Parse(typeof(ConnectorType), connection.From.Name);
            var toConnector = (ConnectorType)Enum.Parse(typeof(ConnectorType), connection.To.Name);
            // no loop
            if (!IsConnectionAllowed(from, fromConnector, to, toConnector))
            {
                return false;
            }
            Connect(from, fromConnector, to, toConnector);
            SetConnectionStyle(e.Connection);
            return true;
        }

        /// <summary>
        /// Test if new connection meets the constraints set fow RTC.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="from"></param>
        /// <param name="fromConnector"></param>
        /// <param name="to"></param>
        /// <param name="toConnector"></param>
        /// <returns></returns>
        public static bool IsConnectionAllowed(object from, ConnectorType fromConnector, object to, ConnectorType toConnector)
        {
            if (refreshingConnections)
            {
                return true;
            }

            if (to is Input)
            {
                Log.Error("Input can only be connected from.");
                return false;
            }
            if (from is Output)
            {
                Log.Error("Output can only be connected to.");
                return false;
            }
            if (from is Input)
            {
                if (fromConnector != ConnectorType.Bottom)
                {
                    Log.Error("Input can only be connected at the lowest connection point.");
                    return false;
                }
                if (to is Output)
                {
                    Log.Error("Input can only be connected to a rule, condition or signal.");
                    return false;
                }
                if (to is RuleBase)
                {
                    var ruleBase = (RuleBase)to;
                    if (ruleBase.Inputs.Count > 0)
                    {
                        Log.Error("Rule can only have 1 input.");
                        return false;
                    }
                }

                if (to is LookupSignal)
                {
                    var lookupSignal = (LookupSignal)to;
                    if (lookupSignal.Inputs.Count > 0)
                    {
                        Log.Error("Lookup signal can only have 1 input.");
                        return false;
                    }
                }

                if (to is ConditionBase)
                {
                    var conditionBase = (ConditionBase)to;
                    if (conditionBase.Input != null)
                    {
                        Log.Error("Condition can only have 1 input.");
                        return false;
                    }
                }

                if (to is SignalBase)
                {
                    if ((toConnector != ConnectorType.Top) && (toConnector != ConnectorType.Left))
                    {
                        Log.Error("Input can only be connected to the left or top connection point.");
                        return false;
                    }
                }
            }

            if (from is SignalBase)
            {
                if (fromConnector != ConnectorType.Right)
                {
                    Log.Error("Signal can only be connected at the right connection point.");
                    return false;
                }
            }

            
            // Multiple connections to 1 output are allowed. During runtime only 1 can be active!
            if (from is RuleBase)
            {
                if (fromConnector != ConnectorType.Right)
                {
                    Log.Error("Rule can only be connected at the right connection point.");
                    return false;
                }
                var ruleBase = (RuleBase)from;
                if (ruleBase.Outputs.Count > 0)
                {
                    Log.Error("Can only connect to 1 output.");
                    return false;
                }
            }

            if (from is ConditionBase)
            {
                if ((fromConnector == ConnectorType.Left) || (fromConnector == ConnectorType.Top))
                {
                    Log.Error("Condition can only be connected at the right or bottom connection point.");
                    return false;
                }
                if (to is Output)
                {
                    Log.Error("Can not connect condition to output; Output can only be set by rule.");
                    return false;
                }
                var conditionBase = (ConditionBase)from;
                if ((fromConnector == ConnectorType.Right) && (conditionBase.TrueOutputs.Count > 0))
                {
                    Log.Error("True output of a condition can only connect to 1 other condition or a rule.");
                    return false;
                }
                if ((fromConnector == ConnectorType.Right) && (conditionBase.FalseOutputs.Contains((RtcBaseObject)to)))
                {
                    Log.Error("Condition is already connected to false of same condition.");
                    return false;
                }
                if ((fromConnector == ConnectorType.Bottom) && (conditionBase.FalseOutputs.Count > 0))
                {
                    Log.Error("True output of a condition can only connect to 1 other condition or a rule.");
                    return false;
                }
                if ((fromConnector == ConnectorType.Bottom) && (conditionBase.TrueOutputs.Contains((RtcBaseObject)to)))
                {
                    Log.Error("Condition is already connected to true of same condition.");
                    return false;
                }
                if (to is ConditionBase)
                {
                    if (toConnector == ConnectorType.Top)
                    {
                        Log.Error("Can only connect a condition to the left of another condition; top is for input.");
                        return false;
                    }
                }
                if (to is RuleBase)
                {
                    if (toConnector == ConnectorType.Top)
                    {
                        Log.Error("Can only connect a condition to the left of a rule; top is for input.");
                        return false;
                    }
                }
            }

            if (to is ConditionBase)
            {
                if ((from is Input) && (toConnector != ConnectorType.Top))
                {
                    Log.Error("Can only connect an input to the top of a condition; left is for another condition.");
                    return false;
                }
                if ((from is ConditionBase) && (toConnector != ConnectorType.Left))
                {
                    Log.Error("Can only connect a condition to the left of a condition; top is for input.");
                    return false;
                }
            }

            if (to is RuleBase)
            {
                if ((from is Input) && (toConnector != ConnectorType.Top))
                {
                    Log.Error("Can only connect an input to the top of a rule; left is for condition.");
                    return false;
                }
                if ((from is ConditionBase) && (toConnector != ConnectorType.Left))
                {
                    Log.Error("Can only connect a condition to the left of a rule; top is for input.");
                    return false;
                }
            }
            if (from == to)
            {
                Log.Error("Can not connect entity to itself.");
                return false;
            }

            if (to is Output)
            {
                if (toConnector != ConnectorType.Left)
                {
                    Log.Error("Can only connect to the left of output.");
                    return false;
                }
            }
            if ((from is RuleBase) && (to is ConditionBase))
            {
                Log.Error("Can not connect rule to condition; rule can only connect to output.");
                return false;
            }
            if ((from is RuleBase) && (to is SignalBase))
            {
                Log.Error("Can not connect rule to signal; rule can only connect to output.");
                return false;
            }
            if ((from is RuleBase) && (to is RuleBase))
            {
                Log.Error("Can not connect rule to rule; rule can only connect to output.");
                return false;
            }
            if ((to is SignalBase) && !(from is Input))
            {
                Log.Error("Only input allowed to connect to signal.");
                return false;
            }
            if ((from is SignalBase) && !(to is RuleBase))
            {
                Log.Error("Signal can only be connected to PIDrule or IntervalRule.");
                return false;
            }
            if ((from is SignalBase) && (to is RuleBase) && !((RuleBase)to).CanBeLinkedFromSignal())
            {
                Log.Error("Signal can only be connected to PIDrule or IntervalRule.");
                return false;
            }
            return true;
        }

        private static void SetConnectionStyle(Netron.GraphLib.Connection connection)
        {
            connection.LinePath = "ConditionalConnection";
            connection.LineEnd = ConnectionEnd.RightFilledArrow;
            connection.LineWeight = ConnectionWeight.Fat;

            if ((connection.From.BelongsTo.Tag is Input) || ((connection.To.BelongsTo.Tag is Output)))
            {
                //  extend : NetronGraph expose pattern for DashStyle.Custom
                //           make connection.LineWeight = ConnectionWeight.Fat work or expose pen.Width
                connection.LineStyle = DashStyle.Dash;
                connection.LineWeight = ConnectionWeight.Fat; // does not work -> fix in Netron Graph
            }
            else
            {
                connection.LineStyle = DashStyle.Solid;
                connection.LineWeight = ConnectionWeight.Fat;
            }

            connection.Text = "";
            if (connection.From.BelongsTo.Tag is ConditionBase)
            {
                if (ConnectionIs(connection))
                {
                    connection.Text = "T";
                }
                else
                {
                    connection.Text = "F";
                }
            }
        }
        
        public static bool ConnectionIs(IConnection connection)
        {
            ConditionBase condition  = connection.From.BelongsTo.Tag as ConditionBase;
            if (condition != null)
            {
                if (condition.FalseOutputs.Contains<RtcBaseObject>((RtcBaseObject)connection.To.BelongsTo.Tag))
                {
                    return false;
                }
                if (condition.TrueOutputs.Contains<RtcBaseObject>((RtcBaseObject)connection.To.BelongsTo.Tag))
                {
                    return true;
                }
            }
           throw new ArgumentException("Connection does not contain True or False outputs");
        }

        public static void Connect(object from, ConnectorType fromConnector, object to, ConnectorType toConnector)
        {
            if (refreshingConnections)
                return;

            adjustingConnectionInDomain = true;

            if ((from is Input) && (to is RuleBase))
            {
                ((RuleBase)to).Inputs.Add((Input)from);
            }
            else if ((from is Input) && (to is SignalBase))
            {
                ((SignalBase)to).Inputs.Add((Input)from);
            }
            else if ((from is Input) && (to is ConditionBase))
            {
                ((ConditionBase)to).Input = (Input)from;
            }
            else if ((from is ConditionBase) && ((to is RuleBase) || (to is ConditionBase)))
            {
                if (fromConnector == ConnectorType.Right)
                {
                    ((ConditionBase)from).TrueOutputs.Add((RtcBaseObject) to);
                }
                else
                {
                    ((ConditionBase)from).FalseOutputs.Add((RtcBaseObject) to);
                }
            }
            else if ((from is ConditionBase) && (to is ConditionBase))
            {
                ((ConditionBase)from).TrueOutputs.Add((RuleBase)to);
            }
            else if ((from is RuleBase) && (to is Output))
            {
                ((RuleBase)from).Outputs.Add((Output)to);
            }
            else if ((from is SignalBase) && (to is RuleBase))
            {
                ((SignalBase)from).RuleBases.Add((RuleBase)to);
            }

            adjustingConnectionInDomain = false;
        }

        public static void Disconnect(object from, object to)
        {
            adjustingConnectionInDomain = true;

            if ((from is Input) && (to is RuleBase))
            {
                ((RuleBase)to).Inputs.Remove((Input)from);
            }
            else if ((from is Input) && (to is SignalBase))
            {
                ((SignalBase)to).Inputs.Remove((Input)from);
            }
            else if ((from is Input) && (to is ConditionBase))
            {
                ((ConditionBase)to).Input = null;
            }
            else if ((from is ConditionBase) && ((to is RuleBase) || (to is ConditionBase)))
            {
                if (((ConditionBase)from).TrueOutputs.Contains<RtcBaseObject>((RtcBaseObject) to))
                {
                    ((ConditionBase)from).TrueOutputs.Remove((RtcBaseObject) to);
                }
                if (((ConditionBase)from).FalseOutputs.Contains<RtcBaseObject>((RtcBaseObject) to))
                {
                    ((ConditionBase)from).FalseOutputs.Remove((RtcBaseObject) to);
                }
            }
            else if ((from is RuleBase) && (to is Output))
            {
                ((RuleBase)from).Outputs.Remove((Output)to);
            }
            else if ((from is SignalBase) && (to is RuleBase))
            {
                ((SignalBase)from).RuleBases.Remove((RuleBase)to);
            }
            adjustingConnectionInDomain = false;
        }
    }
}
