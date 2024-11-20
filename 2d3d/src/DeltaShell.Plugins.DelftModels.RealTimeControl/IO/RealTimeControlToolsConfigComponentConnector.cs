using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO
{
    /// <summary>
    /// Responsible for connecting all inputs and outputs of the rtc components to the rtc components.
    /// </summary>
    public class RealTimeControlToolsConfigComponentConnector
    {
        private readonly IControlGroup controlGroup;
        private readonly IList<Output> outputs = new List<Output>();
        private readonly IList<Input> inputs = new List<Input>();
        private IRtcDataAccessObject<RtcBaseObject>[] dataAccessObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeControlToolsConfigComponentConnector"/> class.
        /// </summary>
        /// <param name="controlGroupName"> Name of the control group. </param>
        public RealTimeControlToolsConfigComponentConnector(string controlGroupName)
        {
            controlGroup = new ControlGroup {Name = controlGroupName};
        }

        /// <summary>
        /// Assembles a control group by connecting the specified <paramref name="groupedDataAccessObjects"/>.
        /// </summary>
        /// <param name="groupedDataAccessObjects"> The data access objects that belong to one control group. </param>
        /// <returns>
        /// The assembled control group
        /// </returns>
        public IControlGroup AssembleControlGroup(IRtcDataAccessObject<RtcBaseObject>[] groupedDataAccessObjects)
        {
            dataAccessObjects = groupedDataAccessObjects;

            foreach (IRtcDataAccessObject<RtcBaseObject> dataAccessObject in dataAccessObjects)
            {
                switch (dataAccessObject)
                {
                    case ConditionDataAccessObject conditionDataAccessObject:
                        ConnectCondition(conditionDataAccessObject);
                        controlGroup.Conditions.Add(conditionDataAccessObject.Object);
                        break;
                    case RuleDataAccessObject ruleDataAccessObject:
                        ConnectRule(ruleDataAccessObject);
                        controlGroup.Rules.Add(ruleDataAccessObject.Object);
                        break;
                    case SignalDataAccessObject signalDataAccessObject:
                        ConnectSignal(signalDataAccessObject);
                        controlGroup.Signals.Add(signalDataAccessObject.Object);
                        break;
                    case ExpressionTree expressionTree:
                        ConnectMathematicalExpression(expressionTree);
                        controlGroup.MathematicalExpressions.Add(expressionTree.Object);
                        break;
                }
            }

            controlGroup.Inputs.AddRange(inputs);
            controlGroup.Outputs.AddRange(outputs);

            return controlGroup;
        }

        private void ConnectCondition(ConditionDataAccessObject dataAccessObject)
        {
            ConditionBase condition = dataAccessObject.Object;
            foreach (string inputReference in dataAccessObject.InputReferences)
            {
                condition.Input = FindInputs(inputReference).FirstOrDefault();
            }

            foreach (string trueOutputRef in dataAccessObject.TrueOutputReferences)
            {
                RtcBaseObject trueOutput = FindById(trueOutputRef);
                condition.TrueOutputs.Add(trueOutput);
            }

            foreach (string falseOutputRef in dataAccessObject.FalseOutputReferences)
            {
                RtcBaseObject falseOutput = FindById(falseOutputRef);
                condition.FalseOutputs.Add(falseOutput);
            }
        }

        private void ConnectRule(RuleDataAccessObject dataAccessObject)
        {
            RuleBase rule = dataAccessObject.Object;
            foreach (string inputRef in dataAccessObject.InputReferences)
            {
                rule.Inputs.AddRange(FindInputs(inputRef));
            }

            foreach (SignalBase signal in dataAccessObject.SignalReferences.Select(FindById<SignalBase>))
            {
                signal.RuleBases.Add(rule);
            }

            foreach (string outputRef in dataAccessObject.OutputReferences)
            {
                Output output = GetOutput(outputRef);
                rule.Outputs.Add(output);
            }
        }

        private void ConnectSignal(SignalDataAccessObject dataAccessObject)
        {
            SignalBase signal = dataAccessObject.Object;
            foreach (string inputRef in dataAccessObject.InputReferences)
            {
                Input input = GetInput(inputRef);
                signal.Inputs.Add(input);
            }
        }

        private void ConnectMathematicalExpression(ExpressionTree tree)
        {
            MathematicalExpression mathematicalExpression = tree.Object;

            IEnumerable<string> inputReferences = tree.RootNode.GetChildNodes().OfType<ParameterLeafNode>()
                                                      .Select(n => n.Value)
                                                      .Distinct();

            foreach (string inputName in inputReferences)
            {
                mathematicalExpression.Inputs.AddRange(FindInputs(inputName));
            }

            MapInputParameters(tree);
            mathematicalExpression.Expression = tree.RootNode.GetExpression();
        }

        private static void MapInputParameters(ExpressionTree expressionTree)
        {
            MathematicalExpression mathematicalExpression = expressionTree.Object;
            IEnumerable<ParameterLeafNode> leafNodes = expressionTree.RootNode.GetChildNodes().OfType<ParameterLeafNode>();
            foreach (ParameterLeafNode leafNode in leafNodes)
            {
                KeyValuePair<char, IInput> parameterKvp =
                    mathematicalExpression.InputMapping.FirstOrDefault(i => i.Value.Name == leafNode.Value);
                if (!parameterKvp.Equals(default(KeyValuePair<char, IInput>)))
                {
                    leafNode.Value = parameterKvp.Key.ToString();
                }
            }
        }

        private IEnumerable<IInput> FindInputs(string reference)
        {
            if (reference.StartsWith(RtcXmlTag.Input) || reference.StartsWith(RtcXmlTag.DelayedInput))
            {
                yield return GetInput(reference);
            }
            else
            {
                string name = RemoveControlGroupPrefix(reference);
                foreach (MathematicalExpression expression in FindByName<MathematicalExpression>(name))
                {
                    yield return expression;
                }
            }
        }

        private string RemoveControlGroupPrefix(string name)
        {
            string controlGroupNamePrefixRemoved = name;
            string combinedControlGroupName = controlGroup.Name + "/";
            if (controlGroupNamePrefixRemoved.StartsWith(combinedControlGroupName))
            {
                controlGroupNamePrefixRemoved = controlGroupNamePrefixRemoved.Replace(combinedControlGroupName, "");
            }

            return controlGroupNamePrefixRemoved;
        }

        private Input GetInput(string inputRef)
        {
            string inputName = new Regex(@"\[(\d+)\]")
                               .Replace(inputRef, string.Empty)
                               .Replace(RtcXmlTag.Delayed, string.Empty);

            Input input = inputs.GetByName(inputName);
            if (input != null)
            {
                return input;
            }

            input = new Input {Name = inputName};
            inputs.Add(input);

            return input;
        }

        private Output GetOutput(string outputRef)
        {
            Output output = outputs.GetByName(outputRef);
            if (output != null)
            {
                return output;
            }

            output = new Output {Name = outputRef};
            outputs.Add(output);

            return output;
        }

        private T FindById<T>(string id) where T : RtcBaseObject
        {
            return dataAccessObjects.Where(o => o.Id == id)
                                    .Select(o => o.Object).OfType<T>()
                                    .FirstOrDefault();
        }

        private RtcBaseObject FindById(string id)
        {
            return dataAccessObjects.FirstOrDefault(o => o.Id == id)?.Object;
        }

        private IEnumerable<T> FindByName<T>(string name) where T : RtcBaseObject
        {
            return dataAccessObjects.OfType<IRtcDataAccessObject<T>>()
                                    .Where(o => o.Object.Name == name)
                                    .Select(o => o.Object);
        }
    }
}
