using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using log4net;
using Netron.GraphLib;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    public class RealTimeControlModelCopyPasteHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RealTimeControlModelCopyPasteHelper));
        private static IEnumerable<ShapeBase> copiedShapes;


        public static IEnumerable<ShapeBase> CopiedShapes
        {
            set { copiedShapes = value; }
        }

        public static bool IsClipBoardRtcObjectSet()
        {
            var clipBoardData = Clipboard.GetData("rtcObjects");

            return clipBoardData != null
                   && clipBoardData.ToString().Equals("copies rtc shapes")
                   && copiedShapes != null;
        }

        public static IEnumerable<ShapeBase> GetClipBoardRtcObjects()
        {
            return copiedShapes;
        }

        public static void SetRtcObjectsToClipBoard(IEnumerable<ShapeBase> shapeCollection)
        {
            CopiedShapes = null;
            Clipboard.Clear();
            if (shapeCollection == null || !shapeCollection.Any())
            {
                return;
            }
            Clipboard.SetData("rtcObjects", "copies rtc shapes");
            copiedShapes = shapeCollection;
        }

        public static void CloneRtcObjectsFromClipBoardAndPlaceOnGraph(IEnumerable<ShapeBase> clipBoardRtcObjects, ControlGroupEditorController controller, Point mea)
        {
            var clonedRtcObjects = new List<object>();
            var source = new Dictionary<object, object>();
            var target = new Dictionary<object, object>();

            foreach (Shape shape in clipBoardRtcObjects)
            {
                var clone = ((ICloneable)shape.Tag).Clone();
                clonedRtcObjects.Add(clone);
                source.Add(clone, shape.Tag);
                target.Add(shape.Tag, clone);
            }

            // SOBEK3-562: Clear data from cloned connection points (inputs & outputs) since the cloned dataitems are not reconnected.
            foreach (var input in clonedRtcObjects.OfType<Input>())
            {
                Log.InfoFormat("It is not possible to copy and paste internal data for control group inputs, the connection to {0} will be reset.", input.Name);
                input.Reset();
            }
            foreach (var output in clonedRtcObjects.OfType<Output>())
            {
                Log.InfoFormat("It is not possible to copy and paste internal data for control group outputs, the connection to {0} will be reset.", output.Name);
                output.Reset();
            }
            
            foreach (object o in clonedRtcObjects)
            {
                SetClonedInputsAndOutputsToClonedObjects(source, target, o);
            }
            PlaceShapesAndConnections(clonedRtcObjects, controller, mea);
        }

        public static void SetClonedInputsAndOutputsToClonedObjects(Dictionary<object, object> source, Dictionary<object, object> target, object o)
        {
            if (o is RuleBase)
            {
                var ruleTarget = (RuleBase)o;
                var ruleSource = (RuleBase)source[ruleTarget];
                foreach (var input in ruleSource.Inputs)
                {
                    if (target.ContainsKey(input))
                    {
                        ruleTarget.Inputs.Add((Input)target[input]);
                    }
                }
                foreach (var output in ruleSource.Outputs)
                {
                    if (target.ContainsKey(output))
                    {
                        ruleTarget.Outputs.Add((Output)target[output]);
                    }
                }
            }
            if (o is ConditionBase)
            {
                var conditionTarget = (ConditionBase)o;
                var conditionSource = (ConditionBase)source[conditionTarget];
                foreach (var trueOutput in conditionSource.TrueOutputs)
                {
                    if (target.ContainsKey(trueOutput))
                    {
                        conditionTarget.TrueOutputs.Add((RtcBaseObject)target[trueOutput]);
                    }
                }
                foreach (var falseOutput in conditionSource.FalseOutputs)
                {
                    if (target.ContainsKey(falseOutput))
                    {
                        conditionTarget.FalseOutputs.Add((RtcBaseObject)target[falseOutput]);
                    }
                }
                if (conditionSource.Input != null && target.ContainsKey(conditionSource.Input))
                {
                    conditionTarget.Input = (Input)target[conditionSource.Input];
                }
            }
            if (o is SignalBase)
            {
                var signalTarget = (SignalBase)o;
                var signalSource = (SignalBase)source[signalTarget];
                foreach (var input in signalSource.Inputs)
                {
                    if (target.ContainsKey(input))
                    {
                        signalTarget.Inputs.Add((Input)target[input]);
                    }
                }
                foreach (var ruleBase in signalSource.RuleBases)
                {
                    if (target.ContainsKey(ruleBase))
                    {
                        signalTarget.RuleBases.Add((RuleBase)target[ruleBase]);
                    }
                }
            }
        }

        private static void PlaceShapesAndConnections(List<object> cloned, ControlGroupEditorController controller, Point mea)
        {
            var clonedRules = cloned.Where(c => c is RuleBase).Cast<RuleBase>().ToList();
            var nonIdenticallyNamedRules = controller.ControlGroup.Rules.ToList();
            foreach (var clonedRule in clonedRules)
            {
                if (nonIdenticallyNamedRules.Count != 0)
                {
                    var identicalName = nonIdenticallyNamedRules.FirstOrDefault(r => r.Name == ((INameable)clonedRule).Name);
                    if (identicalName != null)
                    {
                        ((INameable)clonedRule).Name = RealTimeControlModelHelper.GetUniqueName("Rule - Copy {0}", nonIdenticallyNamedRules, "Copy");
                        nonIdenticallyNamedRules.Add(clonedRule);
                    }
                }
            }

            var clonedSignals = cloned.Where(c => c is SignalBase).Cast<SignalBase>().ToList();
            var nonIdenticallyNamedSignals = controller.ControlGroup.Signals.ToList();
            foreach (var clonedSignal in clonedSignals)
            {
                if (nonIdenticallyNamedSignals.Count != 0)
                {
                    var identicalName = nonIdenticallyNamedSignals.FirstOrDefault(r => r.Name == ((INameable)clonedSignal).Name);
                    if (identicalName != null)
                    {
                        ((INameable)clonedSignal).Name = RealTimeControlModelHelper.GetUniqueName("Signal - Copy {0}", nonIdenticallyNamedSignals, "Copy");
                        nonIdenticallyNamedSignals.Add(clonedSignal);
                    }
                }
            }

            var clonedConditions = cloned.Where(c => c is ConditionBase).Cast<ConditionBase>().ToList();
            var nonIdenticallyNamedConditions = controller.ControlGroup.Conditions.ToList();
            foreach (var clonedCondition in clonedConditions)
            {
                if (nonIdenticallyNamedConditions.Count != 0)
                {
                    var identicalName = nonIdenticallyNamedConditions.FirstOrDefault(r => r.Name == ((INameable)clonedCondition).Name);
                    if (identicalName != null)
                    {
                        ((INameable)clonedCondition).Name = RealTimeControlModelHelper.GetUniqueName("Condition - Copy {0}", nonIdenticallyNamedConditions, "Copy");
                        nonIdenticallyNamedConditions.Add(clonedCondition);
                    }
                }
            }
            controller.AddShapesToControlGroupAndPlace(clonedRules,
                                 clonedConditions,
                                 cloned.Where(c => c is Input).Cast<Input>().ToList(),
                                 cloned.Where(c => c is Output).Cast<Output>().ToList(),
                                 clonedSignals, mea);

            controller.AddConnections(clonedRules, clonedConditions, clonedSignals, true);
        }
    }
}
