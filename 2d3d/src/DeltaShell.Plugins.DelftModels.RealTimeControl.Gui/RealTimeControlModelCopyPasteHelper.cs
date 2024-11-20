using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RTCShapes.Shapes;
using log4net;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui
{
    /// <summary>
    /// Helper class to assist with the copy paste actions of the Real Time Control Model.
    /// </summary>
    public class RealTimeControlModelCopyPasteHelper
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RealTimeControlModelCopyPasteHelper));

        private static RealTimeControlModelCopyPasteHelper instance;
        private readonly List<ShapeBase> copiedShapes;

        private RealTimeControlModelCopyPasteHelper()
        {
            copiedShapes = new List<ShapeBase>();
            IsDataSet = false;
        }

        /// <summary>
        /// Gets the instance of <see cref="RealTimeControlModelCopyPasteHelper"/>.
        /// </summary>
        public static RealTimeControlModelCopyPasteHelper Instance
        {
            get
            {
                return instance ?? (instance = new RealTimeControlModelCopyPasteHelper());
            }
        }

        /// <summary>
        /// Gets the collection of copied shapes.
        /// </summary>
        public IEnumerable<ShapeBase> CopiedShapes => copiedShapes;

        /// <summary>
        /// Gets the indicator whether the data is set for copying.
        /// </summary>
        public bool IsDataSet { get; private set; }

        /// <summary>
        /// Sets the copied data to the helper.
        /// </summary>
        /// <param name="shapes">The collection of <see cref="ShapeBase"/> to set.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="shapes"/>
        /// is <c>null</c>.
        /// </exception>
        public void SetCopiedData(IEnumerable<ShapeBase> shapes)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            IsDataSet = shapes.Any();
            copiedShapes.AddRange(shapes);
        }

        /// <summary>
        /// Clears the data that is set.
        /// </summary>
        public void ClearData()
        {
            IsDataSet = false;
            copiedShapes.Clear();
        }

        /// <summary>
        /// Copies the shapes to the <see cref="ControlGroupEditorController"/>.
        /// </summary>
        /// <param name="controller">
        /// The <see cref="ControlGroupEditorController"/> to copy the shapes to.
        /// </param>
        /// <param name="mea">The location to place the copied shapes at.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="controller"/> is <c>null</c>.</exception>
        public void CopyShapesToController(ControlGroupEditorController controller, Point mea)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            if (!IsDataSet)
            {
                return;
            }

            Dictionary<Input, Input> inputMapping = CopyConnectionPointData<Input>();
            Dictionary<Output, Output> outputMapping = CopyConnectionPointData<Output>();
            Dictionary<MathematicalExpression, MathematicalExpression> mathematicalExpressionMapping = CopyMathematicalExpressions(inputMapping);

            Dictionary<RuleBase, RuleBase> ruleMapping = CopyRules(inputMapping, mathematicalExpressionMapping, outputMapping);
            List<SignalBase> copiedSignals = CopySignals(inputMapping, ruleMapping);
            List<ConditionBase> copiedConditions = CopyConditions(inputMapping, ruleMapping, mathematicalExpressionMapping);

            ControlGroup controlGroup = controller.ControlGroup;
            List<RuleBase> copiedRules = ruleMapping.Values.ToList();
            List<MathematicalExpression> copiedExpressions = mathematicalExpressionMapping.Values.ToList();
            List<Output> copiedOutputs = outputMapping.Values.ToList();
            List<Input> copiedInputs = inputMapping.Values.ToList();
            PostProcessCopiedData(controlGroup, copiedRules, copiedSignals, copiedExpressions, copiedConditions, copiedOutputs);
            AddDataToController(controller, mea, copiedRules, copiedConditions, copiedInputs, copiedOutputs, copiedSignals, copiedExpressions);
        }

        private static void AddDataToController(ControlGroupEditorController controller,
                                                Point mea,
                                                List<RuleBase> copiedRules,
                                                List<ConditionBase> copiedConditions,
                                                List<Input> inputs,
                                                List<Output> copiedOutputs,
                                                List<SignalBase> copiedSignals,
                                                List<MathematicalExpression> copiedExpressions)
        {
            controller.AddShapesToControlGroupAndPlace(copiedRules,
                                                       copiedConditions,
                                                       inputs,
                                                       copiedOutputs,
                                                       copiedSignals,
                                                       copiedExpressions,
                                                       mea);

            controller.AddConnections(copiedRules, copiedConditions, copiedSignals, copiedExpressions, true);
        }

        private static void PostProcessCopiedData(ControlGroup controlGroup,
                                                  List<RuleBase> copiedRules,
                                                  List<SignalBase> copiedSignals,
                                                  List<MathematicalExpression> copiedExpressions,
                                                  List<ConditionBase> copiedConditions,
                                                  List<Output> copiedOutputs)
        {
            RenameCopiedDataWithUniqueNames(copiedRules, controlGroup.Rules, "Rule");
            RenameCopiedDataWithUniqueNames(copiedSignals, controlGroup.Signals, "Signal");
            RenameCopiedDataWithUniqueNames(copiedExpressions, controlGroup.MathematicalExpressions, "Expression");
            RenameCopiedDataWithUniqueNames(copiedConditions, controlGroup.Conditions, "Condition");
            ResetOutputs(copiedOutputs);
        }

        #region Post process helpers

        private static void ResetOutputs(IEnumerable<Output> outputs)
        {
            foreach (Output output in outputs)
            {
                log.InfoFormat("It is not possible to copy and paste internal data for control group outputs, the connection to {0} will be reset.", output.Name);
                output.Reset();
            }
        }

        private static void RenameCopiedDataWithUniqueNames<T>(IEnumerable<T> copiedData, IEnumerable<T> controllerData, string objName)
            where T : RtcBaseObject
        {
            if (!controllerData.Any())
            {
                return;
            }

            List<T> currentObjects = controllerData.ToList();
            var existingNames = new HashSet<string>(currentObjects.Select(d => d.Name));
            foreach (T copy in copiedData.Where(copy => existingNames.Contains(copy.Name)))
            {
                string uniqueName = RealTimeControlModelHelper.GetUniqueName(objName + " - Copy {0}",
                                                                             currentObjects, "Copy");
                copy.Name = uniqueName;
                existingNames.Add(uniqueName);
                currentObjects.Add(copy);
            }
        }

        #endregion

        #region Copy helpers

        private Dictionary<T, T> CopyConnectionPointData<T>() where T : ConnectionPoint
        {
            IEnumerable<T> inputs = copiedShapes.Select(s => s.Tag).OfType<T>();
            var mapping = new Dictionary<T, T>();
            foreach (T input in inputs)
            {
                mapping[input] = (T) input.Clone();
            }

            return mapping;
        }

        private Dictionary<RuleBase, RuleBase> CopyRules(IReadOnlyDictionary<Input, Input> inputMapping,
                                                         IReadOnlyDictionary<MathematicalExpression, MathematicalExpression> expressionMapping,
                                                         IReadOnlyDictionary<Output, Output> outputMapping)
        {
            IEnumerable<RuleBase> rules = copiedShapes.Select(s => s.Tag).OfType<RuleBase>();

            var mapping = new Dictionary<RuleBase, RuleBase>();
            foreach (RuleBase rule in rules)
            {
                var copiedRule = (RuleBase) rule.Clone();
                SetInputs(copiedRule.Inputs, rule.Inputs, inputMapping, expressionMapping);
                SetOutputs(copiedRule, rule, outputMapping);
                mapping[rule] = copiedRule;
            }

            return mapping;
        }

        private List<SignalBase> CopySignals(IReadOnlyDictionary<Input, Input> inputMapping,
                                             IReadOnlyDictionary<RuleBase, RuleBase> ruleMapping)
        {
            IEnumerable<SignalBase> signals = copiedShapes.Select(s => s.Tag).OfType<SignalBase>();

            var copiedSignals = new List<SignalBase>();
            foreach (SignalBase signal in signals)
            {
                var copiedSignal = (SignalBase) signal.Clone();
                SetInputs(copiedSignal.Inputs, signal.Inputs, inputMapping);
                SetRules(copiedSignal.RuleBases, signal.RuleBases, ruleMapping);
                copiedSignals.Add(copiedSignal);
            }

            return copiedSignals;
        }

        private Dictionary<MathematicalExpression, MathematicalExpression> CopyMathematicalExpressions(IReadOnlyDictionary<Input, Input> inputMapping)
        {
            IEnumerable<MathematicalExpression> mathematicalExpressions = copiedShapes.Select(s => s.Tag).OfType<MathematicalExpression>();

            var expressionMapping = new Dictionary<MathematicalExpression, MathematicalExpression>();
            foreach (MathematicalExpression mathematicalExpression in mathematicalExpressions)
            {
                var copiedMathematicalExpression = (MathematicalExpression) mathematicalExpression.Clone();
                expressionMapping[mathematicalExpression] = copiedMathematicalExpression;
            }

            // Gather all the objects that can be connected to a condition in the true and false outputs
            // Then set the input, as a mathematical expression can have a mathematical expression as an input.
            // Failing to comply will result in a KeyNotFoundException.
            foreach (KeyValuePair<MathematicalExpression, MathematicalExpression> kvp in expressionMapping)
            {
                MathematicalExpression source = kvp.Key;
                MathematicalExpression target = kvp.Value;
                SetInputs(target.Inputs, source.Inputs, inputMapping, expressionMapping);
            }

            return expressionMapping;
        }

        private List<ConditionBase> CopyConditions(IReadOnlyDictionary<Input, Input> inputMapping,
                                                   IReadOnlyDictionary<RuleBase, RuleBase> ruleMapping,
                                                   IReadOnlyDictionary<MathematicalExpression, MathematicalExpression> expressionMapping)
        {
            IEnumerable<ConditionBase> conditions = copiedShapes.Select(s => s.Tag).OfType<ConditionBase>();

            var copiedConditions = new List<ConditionBase>();
            var conditionMapping = new Dictionary<RtcBaseObject, RtcBaseObject>();

            Dictionary<IInput, IInput> iInputMapping = GetIInputMapping(inputMapping, expressionMapping);
            foreach (ConditionBase condition in conditions)
            {
                var copiedCondition = (ConditionBase) condition.Clone();
                copiedCondition.Input = null;

                if (condition.Input != null && iInputMapping.ContainsKey(condition.Input))
                {
                    copiedCondition.Input = iInputMapping[condition.Input];
                }

                conditionMapping[condition] = copiedCondition;
                copiedConditions.Add(copiedCondition);
            }

            // Gather all the objects that can be connected to a condition in the true and false outputs
            Dictionary<RtcBaseObject, RtcBaseObject> rtcObjectMapping =
                conditionMapping.Concat(ruleMapping.ToDictionary(kvp => (RtcBaseObject) kvp.Key, kvp => (RtcBaseObject) kvp.Value))
                                .Concat(expressionMapping.ToDictionary(kvp => (RtcBaseObject) kvp.Key, kvp => (RtcBaseObject) kvp.Value))
                                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // Then set all the values or a KeyNotFoundException can be thrown as the ConditionBases are not mapped yet.
            foreach (ConditionBase originalCondition in conditions)
            {
                var copiedCondition = (ConditionBase) conditionMapping[originalCondition];

                SetOutputs(copiedCondition.TrueOutputs, originalCondition.TrueOutputs, rtcObjectMapping);
                SetOutputs(copiedCondition.FalseOutputs, originalCondition.FalseOutputs, rtcObjectMapping);
            }

            return copiedConditions;
        }

        #endregion

        #region Data helpers

        private static Dictionary<IInput, IInput> GetIInputMapping(IReadOnlyDictionary<Input, Input> inputMapping,
                                                                   IReadOnlyDictionary<MathematicalExpression, MathematicalExpression> expressionMapping)
        {
            Dictionary<IInput, IInput> iInputMapping = inputMapping.ToDictionary(kvp => (IInput) kvp.Key, kvp => (IInput) kvp.Value)
                                                                   .Concat(expressionMapping.ToDictionary(kvp => (IInput) kvp.Key, kvp => (IInput) kvp.Value))
                                                                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return iInputMapping;
        }

        private static void SetInputs(IEventedList<IInput> targetInputs,
                                      IEnumerable<IInput> sourceInputs,
                                      IReadOnlyDictionary<Input, Input> inputMapping,
                                      IReadOnlyDictionary<MathematicalExpression, MathematicalExpression> expressionMapping)
        {
            Dictionary<IInput, IInput> iInputMapping = GetIInputMapping(inputMapping, expressionMapping);

            var inputsToAdd = new List<IInput>();
            foreach (IInput sourceInput in sourceInputs)
            {
                IInput castInput = sourceInput;
                if (iInputMapping.ContainsKey(castInput))
                {
                    inputsToAdd.Add(iInputMapping[castInput]);
                }
            }

            targetInputs.AddRange(inputsToAdd);
        }

        private static void SetInputs(IEventedList<Input> targetInputs,
                                      IEnumerable<Input> sourceInputs,
                                      IReadOnlyDictionary<Input, Input> inputMapping)
        {
            IEnumerable<Input> inputsToAdd = GetItemsToAdd(sourceInputs, inputMapping);
            targetInputs.AddRange(inputsToAdd);
        }

        private static void SetRules(IEventedList<RuleBase> targetRules,
                                     IEnumerable<RuleBase> sourceRules,
                                     IReadOnlyDictionary<RuleBase, RuleBase> ruleMapping)
        {
            IEnumerable<RuleBase> rulesToAdd = GetItemsToAdd(sourceRules, ruleMapping);
            targetRules.AddRange(rulesToAdd);
        }

        private static void SetOutputs(RuleBase target,
                                       RuleBase source,
                                       IReadOnlyDictionary<Output, Output> outputMapping)
        {
            IEnumerable<Output> outputsToAdd = GetItemsToAdd(source.Outputs, outputMapping);
            target.Outputs.AddRange(outputsToAdd);
        }

        private static void SetOutputs(IEventedList<RtcBaseObject> targetOutputs,
                                       IEnumerable<RtcBaseObject> sourceOutput,
                                       IReadOnlyDictionary<RtcBaseObject, RtcBaseObject> objectMapping)
        {
            IEnumerable<RtcBaseObject> objectsToBeAdded = GetItemsToAdd(sourceOutput, objectMapping);
            targetOutputs.AddRange(objectsToBeAdded);
        }

        private static IEnumerable<T> GetItemsToAdd<T>(IEnumerable<T> items,
                                                       IReadOnlyDictionary<T, T> mapping)
            where T : RtcBaseObject
        {
            var itemsToAdd = new List<T>();
            foreach (T item in items.Where(mapping.ContainsKey))
            {
                itemsToAdd.Add(mapping[item]);
            }

            return itemsToAdd;
        }

        #endregion
    }
}