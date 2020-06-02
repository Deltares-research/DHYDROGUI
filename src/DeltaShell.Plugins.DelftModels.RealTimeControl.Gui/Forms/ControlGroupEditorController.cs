using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using log4net;
using Netron.GraphLib;
using Netron.GraphLib.Interfaces;
using Netron.GraphLib.UI;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms
{
    public enum ConnectorType
    {
        Left,
        Top,
        Right,
        Bottom
    }

    public class ControlGroupEditorController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ControlGroupEditorController));
        private static bool adjustingConnectionInDomain;
        private static bool refreshingConnections;
        private static readonly Bitmap TimeConditionIcon = Resources.timecondition;
        private static readonly Bitmap DirectionalConditionIcon = Resources.directionalcondition;
        private static readonly Bitmap StandardConditionIcon = Resources.standardcondition;

        private ControlGroup controlGroup;

        private object replaceable;

        private GraphControl graphControl;

        public ControlGroup ControlGroup
        {
            get => controlGroup;
            set
            {
                DesubscribeControlGroupEvents();
                controlGroup = value;
                InitialGraphFill();
                SubscribeControlGroupEvents();
            }
        }

        public GraphControl GraphControl
        {
            get => graphControl;
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

        public void PlaceShapes(IList<RuleBase> rules, IList<ConditionBase> conditions, IList<Input> inputs,
                                IList<Output> outputs, IList<SignalBase> signals, IList<MathematicalExpression> mathExpressions,
                                Point mea)
        {
            bool useOffset = rules.Count + conditions.Count + inputs.Count + outputs.Count > 1;
            if (controlGroup == null || graphControl == null)
            {
                return;
            }

            PlaceShapes(inputs, useOffset, mea, 10);
            PlaceShapes(mathExpressions, useOffset, mea, 110);
            PlaceShapes(conditions, useOffset, mea, 210);
            PlaceShapes(signals, useOffset, mea, 310);
            PlaceShapes(rules, useOffset, mea, 410);
            PlaceShapes(outputs, useOffset, mea, 510);
        }

        public void AddShapesToControlGroupAndPlace(IList<RuleBase> rules, IList<ConditionBase> conditions,
                                                    IList<Input> inputs, IList<Output> outputs,
                                                    IList<SignalBase> signals, IList<MathematicalExpression> mathExpressions,
                                                    Point mea)
        {
            DesubscribeControlGroupEvents();
            DesubscribeGraphControlEvents();
            if (controlGroup != null && graphControl != null)
            {
                controlGroup.Inputs.AddRange(inputs);
                controlGroup.Outputs.AddRange(outputs);
                controlGroup.Rules.AddRange(rules);
                controlGroup.Conditions.AddRange(conditions);
                controlGroup.Signals.AddRange(signals);
                controlGroup.MathematicalExpressions.AddRange(mathExpressions);

                PlaceShapes(rules, conditions, inputs, outputs, signals, mathExpressions, mea);

                SubscribeControlGroupEvents();
                SubscribeGraphControlEvents();
            }
        }

        public void AddConnections(IList<RuleBase> rules, IList<ConditionBase> conditions,
                                   IList<SignalBase> signals, IList<MathematicalExpression> mathExpressions,
                                   bool skipValidation = false)
        {
            if (skipValidation)
            {
                DesubscribeGraphControlEvents(); // Bypass OnGraphControlConnectionAdded event
            }

            if (controlGroup != null && graphControl != null)
            {
                // ToList for local copy (eventedlist causes invalid modification of source)
                foreach (ConditionBase condition in conditions.ToList())
                {
                    SetUiConditionConnections(condition);
                }

                foreach (RuleBase ruleBase in rules.ToList())
                {
                    SetUiRuleConnections(ruleBase);
                }

                foreach (SignalBase signalBase in signals.ToList())
                {
                    SetUiSignalConnections(signalBase);
                }

                foreach (MathematicalExpression mathExpression in mathExpressions.ToList())
                {
                    SetUiMathematicalExpressionConnections(mathExpression);
                }
            }

            if (skipValidation)
            {
                SubscribeGraphControlEvents();
            }
        }

        public void GraphControlShapesOnShapeRemoved(object sender, Shape shape)
        {
            if (shape == null)
            {
                return;
            }

            DesubscribeControlGroupEvents();

            object tag = shape.Tag;
            switch (tag)
            {
                case RuleBase rule:
                    controlGroup.Rules.Remove(rule);
                    break;
                case SignalBase signal:
                    controlGroup.Signals.Remove(signal);
                    break;
                case ConditionBase condition:
                    controlGroup.Conditions.Remove(condition);
                    break;
                case MathematicalExpression mathematicalExpression:
                    controlGroup.MathematicalExpressions.Remove(mathematicalExpression);
                    break;
                case Input input:
                    controlGroup.Inputs.Remove(input);
                    break;
                case Output output:
                    controlGroup.Outputs.Remove(output);
                    break;
            }

            SubscribeControlGroupEvents();
        }

        public RuleBase ConvertRuleTypeTo(RuleBase oldRule, Type toType)
        {
            DesubscribeControlGroupEvents();

            ShapeBase shape = FindShapeByObject(oldRule);

            var newRule = (RuleBase) Activator.CreateInstance(toType);
            newRule.Name = CopyOldNameOrGenerateNameForRule(oldRule.GetType(), toType, oldRule.Name);
            newRule.LongName = oldRule.LongName;
            //for nhibernate's sake the inputs and outputs must be mapped this way 
            foreach (IInput rul in oldRule.Inputs)
            {
                newRule.Inputs.Add(rul);
            }

            foreach (Output rul in oldRule.Outputs)
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

        public ConditionBase ConvertConditionTypeTo(ConditionBase oldCondition, Type toType)
        {
            DesubscribeControlGroupEvents();
            ShapeBase shape = FindShapeByObject(oldCondition);

            var newCondition = (ConditionBase) Activator.CreateInstance(toType);
            newCondition.Name =
                CopyOldNameOrGenerateNameForCondition(oldCondition.GetType(), toType, oldCondition.Name);
            newCondition.LongName = oldCondition.LongName;
            //for nhibernate's sake the inputs and outputs must be mapped this way 
            foreach (RtcBaseObject trueOutput in oldCondition.TrueOutputs)
            {
                newCondition.TrueOutputs.Add(trueOutput);
            }

            foreach (RtcBaseObject falseOutput in oldCondition.FalseOutputs)
            {
                newCondition.FalseOutputs.Add(falseOutput);
            }

            if (oldCondition.Input != null && newCondition.GetType() == typeof(TimeCondition))
            {
                for (var i = 0; i < graphControl.Connections.Count; i++)
                {
                    if (graphControl.Connections[i].To.BelongsTo is ConditionShape &&
                        graphControl.Connections[i].From.BelongsTo is InputItemShape)
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

        public static void MoveShape(IShape shape, double x, double y)
        {
            if (shape != null)
            {
                shape.Location = new PointF((float) x, (float) y);
            }
        }

        public ShapeBase ObjectToShape(object obj)
        {
            ShapeBase shape = null;

            switch (obj)
            {
                case RuleBase rule:
                    shape = CreateShapeFromObject<RuleShape>(rule);
                    break;
                case ConditionBase condition:
                    shape = CreateShapeFromObject<ConditionShape>(condition);
                    FillConditionDescription(shape, condition);
                    break;
                case Input input:
                    shape = CreateShapeFromObject<InputItemShape>(input);
                    break;
                case Output output:
                    shape = CreateShapeFromObject<OutputItemShape>(output);
                    break;
                case SignalBase signal:
                    shape = CreateShapeFromObject<SignalShape>(signal);
                    break;
                case MathematicalExpression mathExpression:
                    shape = CreateShapeFromObject<MathematicalExpressionShape>(mathExpression);
                    break;
            }

            if (shape != null && GetAutoResizeState != null)
            {
                shape.AutoResize = GetAutoResizeState();
            }

            return shape;
        }

        /// <summary>
        /// Test if new connection meets the constraints set fow RTC.
        /// </summary>
        /// <param name="connection"> </param>
        /// <param name="from"> </param>
        /// <param name="fromConnector"> </param>
        /// <param name="to"> </param>
        /// <param name="toConnector"> </param>
        /// <returns> </returns>
        public static bool IsConnectionAllowed(object from, ConnectorType fromConnector, object to,
                                               ConnectorType toConnector)
        {
            if (refreshingConnections)
            {
                return true;
            }

            if (to is Input)
            {
                log.Error("Input can only be connected from.");
                return false;
            }

            if (from is Output)
            {
                log.Error("Output can only be connected to.");
                return false;
            }

            if (from is Input)
            {
                if (fromConnector != ConnectorType.Bottom)
                {
                    log.Error("Input can only be connected at the lowest connection point.");
                    return false;
                }

                if (to is Output)
                {
                    log.Error("Input can only be connected to a rule, condition or signal.");
                    return false;
                }

                if (to is LookupSignal)
                {
                    var lookupSignal = (LookupSignal) to;
                    if (lookupSignal.Inputs.Any())
                    {
                        log.Error("Lookup signal can only have 1 input.");
                        return false;
                    }
                }

                if (to is SignalBase && toConnector != ConnectorType.Top && toConnector != ConnectorType.Left)
                {
                    log.Error("Input can only be connected to the left or top connection point.");
                    return false;
                }

                if (to is ConditionBase conditionBase && conditionBase.Input != null)
                {
                    log.Error("Condition can only have 1 input.");
                    return false;
                }
            }

            if (from is IInput)
            {
                if (to is RuleBase ruleBase && ruleBase.Inputs.Any())
                {
                    log.Error("Rule can only have 1 input.");
                    return false;
                }

                if (to is ConditionBase conditionBase && conditionBase.Input is Input)
                {
                    log.Error("Condition can only have 1 input.");
                    return false;
                }
            }

            if (from is SignalBase)
            {
                if (fromConnector != ConnectorType.Right)
                {
                    log.Error("Signal can only be connected at the right connection point.");
                    return false;
                }
            }

            // Multiple connections to 1 output are allowed. During runtime only 1 can be active!
            if (from is RuleBase)
            {
                if (fromConnector != ConnectorType.Right)
                {
                    log.Error("Rule can only be connected at the right connection point.");
                    return false;
                }

                var ruleBase = (RuleBase) from;
                if (ruleBase.Outputs.Count > 0)
                {
                    log.Error("Can only connect to 1 output.");
                    return false;
                }
            }

            if (from is ConditionBase)
            {
                if (fromConnector == ConnectorType.Left || fromConnector == ConnectorType.Top)
                {
                    log.Error("Condition can only be connected at the right or bottom connection point.");
                    return false;
                }

                if (to is Output)
                {
                    log.Error("Can not connect condition to output; Output can only be set by rule.");
                    return false;
                }

                var conditionBase = (ConditionBase) from;
                if (fromConnector == ConnectorType.Right && conditionBase.TrueOutputs.Count > 0)
                {
                    log.Error("True output of a condition can only connect to 1 other condition or a rule.");
                    return false;
                }

                if (fromConnector == ConnectorType.Right && conditionBase.FalseOutputs.Contains((RtcBaseObject) to))
                {
                    log.Error("Condition is already connected to false of same condition.");
                    return false;
                }

                if (fromConnector == ConnectorType.Bottom && conditionBase.FalseOutputs.Count > 0)
                {
                    log.Error("True output of a condition can only connect to 1 other condition or a rule.");
                    return false;
                }

                if (fromConnector == ConnectorType.Bottom && conditionBase.TrueOutputs.Contains((RtcBaseObject) to))
                {
                    log.Error("Condition is already connected to true of same condition.");
                    return false;
                }

                if (to is ConditionBase)
                {
                    if (toConnector == ConnectorType.Top)
                    {
                        log.Error("Can only connect a condition to the left of another condition; top is for input.");
                        return false;
                    }
                }

                if (to is RuleBase)
                {
                    if (toConnector == ConnectorType.Top)
                    {
                        log.Error("Can only connect a condition to the left of a rule; top is for input.");
                        return false;
                    }
                }
            }

            if (to is ConditionBase)
            {
                if (from is Input && toConnector != ConnectorType.Top)
                {
                    log.Error("Can only connect an input to the top of a condition; left is for another condition.");
                    return false;
                }

                if (from is ConditionBase && toConnector != ConnectorType.Left)
                {
                    log.Error("Can only connect a condition to the left of a condition; top is for input.");
                    return false;
                }
            }

            if (to is RuleBase)
            {
                if (from is Input && toConnector != ConnectorType.Top)
                {
                    log.Error("Can only connect an input to the top of a rule; left is for condition.");
                    return false;
                }

                if (from is ConditionBase && toConnector != ConnectorType.Left)
                {
                    log.Error("Can only connect a condition to the left of a rule; top is for input.");
                    return false;
                }
            }

            if (from == to)
            {
                log.Error("Can not connect entity to itself.");
                return false;
            }

            if (to is Output)
            {
                if (toConnector != ConnectorType.Left)
                {
                    log.Error("Can only connect to the left of output.");
                    return false;
                }
            }

            if (from is RuleBase && to is ConditionBase)
            {
                log.Error("Can not connect rule to condition; rule can only connect to output.");
                return false;
            }

            if (from is RuleBase && to is SignalBase)
            {
                log.Error("Can not connect rule to signal; rule can only connect to output.");
                return false;
            }

            if (from is RuleBase && to is RuleBase)
            {
                log.Error("Can not connect rule to rule; rule can only connect to output.");
                return false;
            }

            if (to is SignalBase && !(from is Input))
            {
                log.Error("Only input allowed to connect to signal.");
                return false;
            }

            if (from is SignalBase && !(to is RuleBase))
            {
                log.Error("Signal can only be connected to PIDrule or IntervalRule.");
                return false;
            }

            if (from is SignalBase && to is RuleBase && !((RuleBase) to).CanBeLinkedFromSignal())
            {
                log.Error("Signal can only be connected to PIDrule or IntervalRule.");
                return false;
            }

            if (from is MathematicalExpression && !(to is MathematicalExpression ||
                                                    to is ConditionBase ||
                                                    to is RuleBase))
            {
                log.Error("Expression can only connect to conditions, rules and other expressions.");
                return false;
            }

            if (to is MathematicalExpression)
            {
                if (toConnector == ConnectorType.Top && !(from is IInput))
                {
                    log.Error("Only inputs and other expressions can connect to the top an expression.");
                    return false;
                }

                if (toConnector == ConnectorType.Left && !(from is ConditionBase))
                {
                    log.Error("Only conditions can connect to the left of an expression.");
                    return false;
                }
            }

            return true;
        }

        public static bool ConnectionIs(IConnection connection)
        {
            var condition = connection.From.BelongsTo.Tag as ConditionBase;
            if (condition != null)
            {
                var belongsTo = (RtcBaseObject) connection.To.BelongsTo.Tag;
                if (condition.FalseOutputs.Contains<RtcBaseObject>(belongsTo))
                {
                    return false;
                }

                if (condition.TrueOutputs.Contains<RtcBaseObject>(belongsTo))
                {
                    return true;
                }
            }

            throw new ArgumentException("Connection does not contain True or False outputs");
        }

        public static void Connect(object from, ConnectorType fromConnector, object to, ConnectorType toConnector)
        {
            if (refreshingConnections)
            {
                return;
            }

            adjustingConnectionInDomain = true;

            if (from is Input fromInput && to is SignalBase toSignal)
            {
                toSignal.Inputs.Add(fromInput);
            }

            if (from is IInput fromIInput)
            {
                if (to is RuleBase toRule)
                {
                    toRule.Inputs.Add(fromIInput);
                }

                if (to is ConditionBase toCondition)
                {
                    toCondition.Input = fromIInput;
                }

                if (to is MathematicalExpression toMathematicalExpression)
                {
                    toMathematicalExpression.Inputs.Add(fromIInput);
                }
            }

            else if (from is ConditionBase fromCondition && (to is RuleBase || to is ConditionBase || to is MathematicalExpression))
            {
                var obj = (RtcBaseObject) to;
                if (fromConnector == ConnectorType.Right)
                {
                    fromCondition.TrueOutputs.Add(obj);
                }
                else
                {
                    fromCondition.FalseOutputs.Add(obj);
                }
            }
            else if (from is RuleBase fromRule && to is Output toOutput)
            {
                fromRule.Outputs.Add(toOutput);
            }
            else if (from is SignalBase fromSignal && to is RuleBase toRule)
            {
                fromSignal.RuleBases.Add(toRule);
            }

            adjustingConnectionInDomain = false;
        }

        public static void Disconnect(object from, object to)
        {
            adjustingConnectionInDomain = true;

            if (from is Input fromInput)
            {
                if (to is RuleBase toRule)
                {
                    toRule.Inputs.Remove(fromInput);
                }

                if (to is SignalBase toSignal)
                {
                    toSignal.Inputs.Remove(fromInput);
                }

                if (to is ConditionBase toCondition)
                {
                    toCondition.Input = null;
                }
            }

            if (to is MathematicalExpression toMathExpression)
            {
                switch (from)
                {
                    case IInput fromExpressionInput:
                        toMathExpression.Inputs.Remove(fromExpressionInput);
                        break;
                    case ConditionBase condition:
                        condition.FalseOutputs.Remove(toMathExpression);
                        condition.TrueOutputs.Remove(toMathExpression);
                        break;
                }
            }

            if (from is MathematicalExpression fromMathExpression)
            {
                switch (to)
                {
                    case RuleBase toRule:
                        toRule.Inputs.Remove(fromMathExpression);
                        break;
                    case ConditionBase toCondition:
                        toCondition.Input = null;
                        break;
                    case MathematicalExpression toMathematicalExpression:
                        toMathematicalExpression.Inputs.Remove(fromMathExpression);
                        break;
                }
            }

            adjustingConnectionInDomain = false;
        }

        internal void PlaceShapeOnGraphControl(object obj, double x, double y)
        {
            ShapeBase shape = ObjectToShape(obj);
            graphControl.AddShape(shape);
            MoveShape(shape, x, y);
        }

        private void InitialGraphFill()
        {
            DesubscribeGraphControlEvents();
            CleanGraphControl();
            if (controlGroup != null)
            {
                var point = new Point()
                {
                    X = 0,
                    Y = 0
                };
                PlaceShapes(controlGroup.Rules, controlGroup.Conditions, controlGroup.Inputs, controlGroup.Outputs,
                            controlGroup.Signals, controlGroup.MathematicalExpressions, point);
                AddConnections(controlGroup.Rules, controlGroup.Conditions, controlGroup.Signals, controlGroup.MathematicalExpressions);
            }

            SubscribeGraphControlEvents();
        }

        private void CleanGraphControl()
        {
            if (graphControl != null)
            {
                graphControl.Shapes.Clear();
                graphControl.Connections.Clear();
            }
        }

        private void PlaceShapes<T>(ICollection<T> objects, bool useOffset, Point startPoint, int xOffset)
        {
            int x = startPoint.X;
            int y = startPoint.Y;

            for (var i = 0; i < objects.Count; i++)
            {
                int yOffset = 10 + (50 * i);
                PlaceShapeOnGraphControl(objects.ElementAt(i),
                                         useOffset ? x + xOffset : x,
                                         useOffset ? y + yOffset : y);
            }
        }

        private void SetUiRuleConnections(RuleBase ruleBase)
        {
            foreach (IInput input in ruleBase.Inputs.ToList())
            {
                UiConnect(input, "Bottom", ruleBase, "Top");
            }

            foreach (Output output in ruleBase.Outputs.ToList())
            {
                UiConnect(ruleBase, "Right", output, "Left");
            }
        }

        private void SetUiSignalConnections(SignalBase signalBase)
        {
            foreach (Input input in signalBase.Inputs.ToList())
            {
                UiConnect(input, "Bottom", signalBase, "Top");
            }

            foreach (RuleBase rulebase in signalBase.RuleBases.ToList())
            {
                if (rulebase.CanBeLinkedFromSignal())
                {
                    UiConnect(signalBase, "Right", rulebase, "Bottom");
                }
            }
        }

        private void SetUiConditionConnections(ConditionBase condition)
        {
            IInput input = condition.Input;
            if (input != null)
            {
                UiConnect(input, "Bottom", condition, "Top");
            }

            foreach (RtcBaseObject output in condition.FalseOutputs.ToList())
            {
                UiConnect(condition, "Bottom", output, "Left");
            }

            foreach (RtcBaseObject output in condition.TrueOutputs.ToList())
            {
                UiConnect(condition, "Right", output, "Left");
            }
        }

        private void SetUiMathematicalExpressionConnections(MathematicalExpression mathematicalExpression)
        {
            foreach (IInput input in mathematicalExpression.Inputs.ToList())
            {
                UiConnect(input, "Bottom", mathematicalExpression, "Top");
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
            ShapeBase leftShape = FindShapeByObject(source);
            ShapeBase rightShape = FindShapeByObject(target);
            Netron.GraphLib.Connection connection =
                graphControl.AddConnection(leftShape.Connectors[sourceConnector],
                                           rightShape.Connectors[targetConnector]);
            SetConnectionStyle(connection);
        }

        private void SubscribeControlGroupEvents()
        {
            if (controlGroup != null)
            {
                ((INotifyCollectionChanged) controlGroup).CollectionChanged += ControlGroupCollectionChanged;
                ((INotifyCollectionChanging) controlGroup).CollectionChanging += ControlGroupCollectionChanging;
                ((INotifyPropertyChanged) controlGroup).PropertyChanged += ControlGroupPropertyChanged;
            }
        }

        private void DesubscribeControlGroupEvents()
        {
            if (controlGroup != null)
            {
                ((INotifyCollectionChanged) controlGroup).CollectionChanged -= ControlGroupCollectionChanged;
                ((INotifyCollectionChanging) controlGroup).CollectionChanging -= ControlGroupCollectionChanging;
                ((INotifyPropertyChanged) controlGroup).PropertyChanged -= ControlGroupPropertyChanged;
            }
        }

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
            if (sender is ConnectionPoint && e.PropertyName == "Name")
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
                    sender != controlGroup.Signals &&
                    sender != controlGroup.MathematicalExpressions)
                {
                    if (controlGroup.IsEditing)
                    {
                        return;
                    }

                    RefreshConnections();
                    return;
                }

                object removedOrAddedItem = e.GetRemovedOrAddedItem();
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    DesubscribeGraphControlEvents();

                    ShapeBase shape = ObjectToShape(removedOrAddedItem);
                    graphControl.AddShape(shape);
                    SubscribeGraphControlEvents();
                }

                if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    DesubscribeGraphControlEvents();
                    ShapeBase shape = FindShapeByObject(removedOrAddedItem);
                    graphControl.Shapes.Remove(shape);
                    SubscribeGraphControlEvents();
                }

                if (e.Action == NotifyCollectionChangedAction.Replace)
                {
                    if (replaceable != null)
                    {
                        ShapeBase shape = FindShapeByObject(replaceable);
                        shape.Tag = removedOrAddedItem;
                        replaceable = null;
                    }
                }

                graphControl.Invalidate();
            }
        }

        private void RefreshConnections()
        {
            if (adjustingConnectionInDomain)
            {
                return;
            }

            refreshingConnections = true;

            RemoveAllConnections(graphControl.Connections);
            foreach (Shape shape in graphControl.Shapes)
            {
                foreach (Connector connector in shape.Connectors)
                {
                    RemoveAllConnections(connector.Connections);
                }
            }

            AddConnections(controlGroup.Rules, controlGroup.Conditions, controlGroup.Signals, controlGroup.MathematicalExpressions);

            graphControl.Invalidate();

            refreshingConnections = false;
        }

        private void RemoveAllConnections(ConnectionCollection connectionCollection)
        {
            //make copy of list, and then remove each individual item.
            List<Netron.GraphLib.Connection> connections =
                connectionCollection.OfType<Netron.GraphLib.Connection>().ToList();
            foreach (Netron.GraphLib.Connection connection in connections)
            {
                connectionCollection.Remove(connection); //Clear doesn't raise events, bleh
            }
        }

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

        private ShapeBase FindShapeByObject(object obj)
        {
            return graphControl?.Shapes.OfType<ShapeBase>().FirstOrDefault(s => s.Tag == obj);
        }

        private static string CopyOldNameOrGenerateNameForRule(Type oldType, Type newType, string oldName)
        {
            string oldTypeTitle = RuleProvider.GetTitle(oldType);

            if (oldTypeTitle == oldName)
            {
                return RuleProvider.GetTitle(newType);
            }

            return oldName;
        }

        private static string CopyOldNameOrGenerateNameForCondition(Type oldType, Type newType, string oldName)
        {
            string oldTypeTitle = ConditionProvider.GetTitle(oldType);

            if (oldTypeTitle == oldName)
            {
                return ConditionProvider.GetTitle(newType);
            }

            return oldName;
        }

        private void ReconnectRtcBaseObject(RtcBaseObject oldRtcObject, RtcBaseObject newRtcObject)
        {
            IEnumerable<ConditionBase> falseOutputs =
                controlGroup.Conditions.Where(c => c.FalseOutputs.Contains(oldRtcObject));
            foreach (ConditionBase conditionBase in falseOutputs)
            {
                conditionBase.FalseOutputs.Remove(oldRtcObject);
                conditionBase.FalseOutputs.Add(newRtcObject);
            }

            IEnumerable<ConditionBase> trueOutputs =
                controlGroup.Conditions.Where(c => c.TrueOutputs.Contains(oldRtcObject));
            foreach (ConditionBase conditionBase in trueOutputs)
            {
                conditionBase.TrueOutputs.Remove(oldRtcObject);
                conditionBase.TrueOutputs.Add(newRtcObject);
            }

            IEnumerable<SignalBase> signalRuleBases =
                controlGroup.Signals.Where(s => s.RuleBases.Contains(oldRtcObject));
            foreach (SignalBase signalBase in signalRuleBases)
            {
                signalBase.RuleBases.Remove((RuleBase) oldRtcObject);
                signalBase.RuleBases.Add((RuleBase) newRtcObject);
            }
        }

        private static T CreateShapeFromObject<T>(INameable obj) where T : ShapeBase
        {
            var shape = Activator.CreateInstance<T>();
            shape.Text = obj.Name;
            shape.Tag = obj;

            return shape;
        }

        private static void FillConditionDescription(ShapeBase shape, ConditionBase condition)
        {
            if (shape is ConditionShape)
            {
                var conditionShape = shape as ConditionShape;
                conditionShape.Image = GetIconForCondition(condition);
                conditionShape.GetDescriptionDelegate = condition.GetDescription;
                if (condition is TimeCondition)
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
            switch (conditionBase)
            {
                case TimeCondition _:
                    return TimeConditionIcon;
                case DirectionalCondition _:
                    return DirectionalConditionIcon;
                case StandardCondition _:
                    return StandardConditionIcon;
                default:
                    return null;
            }
        }

        private bool OnGraphControlConnectionRemoved(object sender, ConnectionEventArgs e)
        {
            Netron.GraphLib.Connection connection = e.Connection;
            object from = connection.From.BelongsTo.Tag;
            object to = connection.To.BelongsTo.Tag;
            Disconnect(from, to);
            return true;
        }

        /// <summary>
        /// Validate if a newly added connection is allowed.
        /// Some options like a connection starts from an ouotput item are prevented in the Connectors in
        /// the shape.
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="e"> </param>
        /// <returns> </returns>
        private bool OnGraphControlConnectionAdded(object sender, ConnectionEventArgs e)
        {
            Netron.GraphLib.Connection connection = e.Connection;
            object from = connection.From.BelongsTo.Tag;
            object to = connection.To.BelongsTo.Tag;
            var fromConnector = (ConnectorType) Enum.Parse(typeof(ConnectorType), connection.From.Name);
            var toConnector = (ConnectorType) Enum.Parse(typeof(ConnectorType), connection.To.Name);
            // no loop
            if (!IsConnectionAllowed(from, fromConnector, to, toConnector))
            {
                return false;
            }

            Connect(from, fromConnector, to, toConnector);
            SetConnectionStyle(e.Connection);
            return true;
        }

        private static void SetConnectionStyle(Netron.GraphLib.Connection connection)
        {
            connection.LinePath = "ConditionalConnection";
            connection.LineEnd = ConnectionEnd.RightFilledArrow;
            connection.LineWeight = ConnectionWeight.Fat;

            if (connection.From.BelongsTo.Tag is Input || connection.To.BelongsTo.Tag is Output)
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
    }
}