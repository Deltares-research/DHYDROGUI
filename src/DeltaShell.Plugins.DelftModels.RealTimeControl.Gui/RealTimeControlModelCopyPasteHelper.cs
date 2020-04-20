using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using log4net;
using Netron.GraphLib;
using Clipboard = DelftTools.Controls.Clipboard;

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
            ControlGroup controlGroup = controller.ControlGroup;
            IList<RuleBase> clonedRules = GetUniquelyNamedClones(cloned, controlGroup.Rules, "Rule");
            IList<SignalBase> clonedSignals = GetUniquelyNamedClones(cloned, controlGroup.Signals, "Signal");
            IList<ConditionBase> clonedConditions = GetUniquelyNamedClones(cloned, controlGroup.Conditions, "Condition");
            IList<MathematicalExpression> clonedMathExpressions = GetUniquelyNamedClones(cloned, controlGroup.MathematicalExpressions,
                                                                                         "Expression");

            controller.AddShapesToControlGroupAndPlace(clonedRules,
                                 clonedConditions,
                                 cloned.Where(c => c is Input).Cast<Input>().ToList(),
                                 cloned.Where(c => c is Output).Cast<Output>().ToList(),
                                 clonedSignals, clonedMathExpressions, mea);

            controller.AddConnections(clonedRules, clonedConditions, clonedSignals, clonedMathExpressions, true);
        }

        private static IList<T> GetUniquelyNamedClones<T>(List<object> clonedObjects, IEnumerable<T> existingObjects, string objName)
            where T : RtcBaseObject
        {
            List<T> newObjects = clonedObjects.OfType<T>().ToList();
            List<T> copyExistingObjects = existingObjects.ToList();

            foreach (T newObject in newObjects)
            {
                if (!copyExistingObjects.Any())
                {
                    continue;
                }

                T identicalName = copyExistingObjects.FirstOrDefault(o => o.Name == newObject.Name);
                if (identicalName == null)
                {
                    continue;
                }

                newObject.Name = RealTimeControlModelHelper.GetUniqueName(objName + " - Copy {0}",
                                                                          copyExistingObjects, "Copy");

                copyExistingObjects.Add(newObject);
            }

            return newObjects;
        }
    }
}
