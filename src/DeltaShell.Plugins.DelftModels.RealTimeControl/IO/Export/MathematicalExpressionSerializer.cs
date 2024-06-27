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
        /// <returns> The collection of <see cref="XElement"/>. </returns>
        public IEnumerable<XElement> GetDataConfigXmlElements(XNamespace xNamespace)
        {
            IBranchNode rootNode = RetrieveRootBranchNode();

            List<IExpressionNode> allSubNodes = rootNode.GetChildNodes().ToList();
            List<IBranchNode> subBranchNodes = allSubNodes.OfType<IBranchNode>().ToList();
            IEnumerable<ParameterLeafNode> subParameterLeafNodes = allSubNodes.OfType<ParameterLeafNode>();

            CorrectAllNodesByUsingOriginalInputNames(rootNode, subBranchNodes, subParameterLeafNodes);

            yield return new XElement(xNamespace + "timeSeries",
                                      new XAttribute("id", rootNode.YName));

            foreach (IBranchNode branchNode in subBranchNodes)
            {
                yield return new XElement(xNamespace + "timeSeries",
                                          new XAttribute("id", branchNode.YName));
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

            CorrectAllNodesByUsingOriginalInputNames(rootNode, subBranchNodes, subParameterLeafNodes);

            string idRootNode = GetXmlNameWithoutTag(prefix);

            foreach (IBranchNode subBranchNode in subBranchNodes)
            {
                string idSubBranchNodes = prefix + subBranchNode.YName;
                yield return CreateTriggerForExpression(xNamespace, subBranchNode, idSubBranchNodes);
            }

            yield return CreateTriggerForExpression(xNamespace, rootNode, idRootNode);
        }

        /// <summary>
        /// Used by other RTC components if they are connected to a Mathematical Expression.
        /// </summary>
        /// <returns> The xml name of the mathematical expression. </returns>
        public override string GetXmlName()
        {
            return MathematicalExpression.Name;
        }

        protected override string XmlTag { get; }
        private MathematicalExpression MathematicalExpression { get; }

        private XElement CreateTriggerForExpression(XNamespace xNamespace, IBranchNode branchNode, string id)
        {
            var expression = new XElement(xNamespace + "expression", new XAttribute("id", id));

            expression.Add(CreateXElementForNode(xNamespace, branchNode.FirstNode, "x1Value", "x1Series"));
            expression.Add(new XElement(xNamespace + "mathematicalOperator",
                                        ConvertToMathematicalOperatorEnumStringType(branchNode.OperatorValue)));
            expression.Add(CreateXElementForNode(xNamespace, branchNode.SecondNode, "x2Value", "x2Series"));
            expression.Add(new XElement(xNamespace + "y", branchNode.YName));

            return new XElement(xNamespace + "trigger", expression);
        }

        private XElement CreateXElementForNode(XNamespace xNamespace, IExpressionNode node, string constantName,
                                               string seriesName)
        {
            string nodeReference = RetrieveNodeReference(node);

            bool isConstant = node is ConstantValueLeafNode;
            XName xName = xNamespace + (isConstant ? constantName : seriesName);
            XAttribute xAttribute = isConstant ? null : new XAttribute("ref", "IMPLICIT");
            return new XElement(xName, xAttribute, nodeReference);
        }

        private string RetrieveNodeReference(IExpressionNode node)
        {
            var nodeReference = node.ToString();
            if (node is BranchNode)
            {
                nodeReference = MathematicalExpression.Name + "/" + nodeReference;
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

        private void CorrectAllNodesByUsingOriginalInputNames(IBranchNode rootNode, IEnumerable<IBranchNode> subBranchNodes, IEnumerable<ParameterLeafNode> subParameterLeafNodes)
        {
            CorrectXmlInputNamesForLeafNodes(subParameterLeafNodes);
            SetYNameOfRootNode(rootNode);
            SetYNamesOfSubBranchNodes(subBranchNodes);
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

        private void CorrectXmlInputNamesForLeafNodes(IEnumerable<ParameterLeafNode> leafNodes)
        {
            foreach (ParameterLeafNode leafNode in leafNodes)
            {
                KeyValuePair<char, IInput> parameterKvp = MathematicalExpression.InputMapping.SingleOrDefault(input => input.Key.ToString().Equals(leafNode.Value));
                if (!parameterKvp.Equals(default(KeyValuePair<char, IInput>)))
                {
                    IInput expressionInput = MathematicalExpression.Inputs.First(i => i.Equals(parameterKvp.Value));

                    var serializer = SerializerCreator.CreateSerializerType<InputSerializerBase>((RtcBaseObject) expressionInput);
                    string xmlName = serializer.GetXmlName();

                    leafNode.Value = xmlName;
                }
            }
        }

        private void SetYNameOfRootNode(IBranchNode branchNode)
        {
            branchNode.YName = MathematicalExpression.Name;
        }

        private void SetYNamesOfSubBranchNodes(IEnumerable<IBranchNode> branchNodes)
        {
            foreach (IBranchNode branchNode in branchNodes)
            {
                branchNode.YName = MathematicalExpression.Name + "/" + branchNode;
            }
        }
    }
}
