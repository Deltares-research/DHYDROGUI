using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export
{
    /// <summary>
    /// Serializer for a <see cref="MathematicalExpression"/>.
    /// </summary>
    public class MathematicalExpressionSerializer : InputSerializerBase
    {
        /// <summary>
        /// Creates a MathematicalExpressionSerializer for one GUI Mathematical Expression,
        /// which can be defined by using multiple expression blocks in the toolsconfig.xml.
        /// </summary>
        /// <param name="mathematicalExpression"> The mathematical expression to serialize. </param>
        public MathematicalExpressionSerializer(MathematicalExpression mathematicalExpression) : base(
            mathematicalExpression)
        {
            MathematicalExpression = mathematicalExpression;
        }

        /// <summary>
        /// Converts the mathematical expression to a collection of <see cref="XElement"/>
        /// to be written to export time series in the data config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix">An optional prefix string that can be used to add GroupName to elements. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public IEnumerable<XElement> GetDataConfigXmlElements(XNamespace xNamespace, string prefix)
        {
            IBranchNode rootNode = RetrieveRootBranchNode();

            List<IExpressionNode> allSubNodes = rootNode.GetChildNodes().ToList();
            List<IBranchNode> subBranchNodes = allSubNodes.OfType<IBranchNode>().ToList();
            IEnumerable<ParameterLeafNode> subParameterLeafNodes = allSubNodes.OfType<ParameterLeafNode>();

            CorrectAllNodesByUsingOriginalInputNames(rootNode, subBranchNodes, subParameterLeafNodes, prefix);

            yield return new XElement(xNamespace + "timeSeries", new XAttribute("id", rootNode.YName));

            foreach (IBranchNode branchNode in subBranchNodes)
            {
                yield return new XElement(xNamespace + "timeSeries", new XAttribute("id", branchNode.YName));
            }
        }

        /// <summary>
        /// Converts the mathematical expression to a collection of <see cref="XElement"/>
        /// to be written to the tools config xml file.
        /// </summary>
        /// <param name="xNamespace"> The xml namespace. </param>
        /// <param name="prefix"> The prefix. </param>
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            IBranchNode rootNode = RetrieveRootBranchNode();

            List<IExpressionNode> allSubNodes = rootNode.GetChildNodes().ToList();
            List<IBranchNode> subBranchNodes = allSubNodes.OfType<IBranchNode>().ToList();
            IEnumerable<ParameterLeafNode> subParameterLeafNodes = allSubNodes.OfType<ParameterLeafNode>();

            CorrectAllNodesByUsingOriginalInputNames(rootNode, subBranchNodes, subParameterLeafNodes, prefix);

            string idRootNode = GetXmlNameWithoutTag(prefix);

            foreach (IBranchNode subBranchNode in subBranchNodes)
            {
                string idSubBranchNodes = subBranchNode.YName;
                yield return CreateTriggerForExpression(xNamespace, subBranchNode, idSubBranchNodes, prefix);
            }

            yield return CreateTriggerForExpression(xNamespace, rootNode, idRootNode, prefix);
        }

        /// <summary>
        /// Used by other RTC components if they are connected to a Mathematical Expression.
        /// </summary>
        /// <param name="prefix">A string that is prepended to the Xml name</param>
        /// <returns> The xml name of the mathematical expression. </returns>
        public override string GetXmlName(string prefix)
        {
            return prefix + MathematicalExpression.Name;
        }
        
        private MathematicalExpression MathematicalExpression { get; }

        private XElement CreateTriggerForExpression(XNamespace xNamespace, IBranchNode branchNode, string id, string prefix)
        {
            var expression = new XElement(xNamespace + "expression", new XAttribute("id", id));

            expression.Add(CreateXElementForNode(xNamespace, branchNode.FirstNode, "x1Value", "x1Series", prefix));
            expression.Add(new XElement(xNamespace + "mathematicalOperator",
                                        ConvertToMathematicalOperatorEnumStringType(branchNode.OperatorValue)));
            expression.Add(CreateXElementForNode(xNamespace, branchNode.SecondNode, "x2Value", "x2Series", prefix));
            expression.Add(new XElement(xNamespace + "y", branchNode.YName));

            return new XElement(xNamespace + "trigger", expression);
        }

        private XElement CreateXElementForNode(XNamespace xNamespace, IExpressionNode node, string constantName,
                                               string seriesName, string prefix)
        {
            string nodeReference = RetrieveNodeReference(node, prefix);

            bool isConstant = node is ConstantValueLeafNode;
            XName xName = xNamespace + (isConstant ? constantName : seriesName);
            XAttribute xAttribute = isConstant ? null : new XAttribute("ref", "IMPLICIT");
            return new XElement(xName, xAttribute, nodeReference);
        }

        private string RetrieveNodeReference(IExpressionNode node, string prefix)
        {
            var nodeReference = node.ToString();
            if (node is BranchNode)
            {
                nodeReference = GetXmlName(prefix) + "/" + nodeReference;
            }

            return nodeReference;
        }

        private string ConvertToMathematicalOperatorEnumStringType(Operator mathOperator)
        {
            switch (mathOperator)
            {
                case Operator.Add:
                    return "+";
                case Operator.Subtract:
                    return "-";
                case Operator.Multiply:
                    return "*";
                case Operator.Divide:
                    return "/";
                case Operator.Min:
                    return "min";
                case Operator.Max:
                    return "max";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mathOperator), mathOperator, null);
            }
        }

        private void CorrectAllNodesByUsingOriginalInputNames(IBranchNode rootNode, IEnumerable<IBranchNode> subBranchNodes, IEnumerable<ParameterLeafNode> subParameterLeafNodes, string prefix)
        {
            CorrectXmlInputNamesForLeafNodes(subParameterLeafNodes, prefix);
            SetYNameOfRootNode(rootNode, prefix);
            SetYNamesOfSubBranchNodes(subBranchNodes, prefix);
        }

        private IBranchNode RetrieveRootBranchNode()
        {
            bool resultParsing = ExpressionParser.TryParse(MathematicalExpression.Expression, out IExpressionNode iRootNode,
                                                           out string errorMessage);

            if (!resultParsing)
            {
                throw new ArgumentException(errorMessage);
            }

            if (!(iRootNode is BranchNode rootNode))
            {
                throw new InvalidOperationException(
                    string.Format(
                        Resources
                            .MathematicalExpressionSerializer_ParseMathematicalExpressionToRootBranchNode_Mathematical_expression__0__contains_invalid_expression__1__,
                        MathematicalExpression.Name, MathematicalExpression.Expression));
            }

            return rootNode;
        }

        private void CorrectXmlInputNamesForLeafNodes(IEnumerable<ParameterLeafNode> leafNodes, string prefix)
        {
            foreach (ParameterLeafNode leafNode in leafNodes)
            {
                KeyValuePair<char, IInput> parameterKvp = MathematicalExpression.InputMapping.SingleOrDefault(input => input.Key.ToString().Equals(leafNode.Value));
                if (!parameterKvp.Equals(default(KeyValuePair<char, IInput>)))
                {
                    IInput expressionInput = MathematicalExpression.Inputs.First(i => i.Equals(parameterKvp.Value));

                    var serializer = SerializerCreator.CreateSerializerType<InputSerializerBase>((RtcBaseObject) expressionInput);
                    leafNode.Value = serializer.GetXmlName(prefix);
                }
            }
        }

        private void SetYNameOfRootNode(IBranchNode branchNode, string prefix)
        {
            branchNode.YName = GetXmlName(prefix);
        }

        private void SetYNamesOfSubBranchNodes(IEnumerable<IBranchNode> branchNodes, string prefix)
        {
            foreach (IBranchNode branchNode in branchNodes)
            {
                branchNode.YName = GetXmlName(prefix) + "/" + branchNode;
            }
        }
    }
}
