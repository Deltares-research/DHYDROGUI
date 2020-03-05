using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.ImportExport.Export
{
    /// <summary>
    /// Serializer for Mathematical Expressions.
    /// </summary>
    public class MathematicalExpressionSerializer : InputSerializerBase
    {
        private MathematicalExpression MathematicalExpression { get; }

        /// <summary>
        /// Creates a MathematicalExpressionSerializer for one GUI Mathematical Expression,
        /// which can be defined by using multiple expression blocks in the toolsconfig.xml.
        /// </summary>
        /// <param name="mathematicalExpression"></param>
        public MathematicalExpressionSerializer(MathematicalExpression mathematicalExpression) : base(
            mathematicalExpression)
        {
            MathematicalExpression = mathematicalExpression;
        }

        protected override string XmlTag { get; }

        /// <summary>
        /// Creates XElement(s) for the Mathematical Expression.
        /// </summary>
        /// <param name="xNamespace"></param>
        /// <param name="prefix"></param>
        /// <returns>IEnumerable with XElements for every expression block</returns>
        public override IEnumerable<XElement> ToXml(XNamespace xNamespace, string prefix)
        {
            BranchNode rootNode = ParseMathematicalExpressionToRootBranchNode(
                    MathematicalExpression, out List<BranchNode> subBranchNodes);

            string idRootNode = GetXmlNameWithoutTag(prefix);
            yield return CreateTriggerForExpression(xNamespace, rootNode, idRootNode);

            foreach (BranchNode subBranchNode in subBranchNodes)
            {
                string idSubBranchNodes = prefix + subBranchNode.YName;
                yield return CreateTriggerForExpression(xNamespace, subBranchNode, idSubBranchNodes);
            }
        }

        /// <summary>
        /// Used by other RTC components if they are connected to a Mathematical Expression.
        /// </summary>
        /// <returns></returns>
        public override string GetXmlName()
        {
            return MathematicalExpression.Name;
        }

        public IEnumerable<XElement> GetDataConfigXmlElements(XNamespace xNamespace)
        {
            BranchNode rootNode = ParseMathematicalExpressionToRootBranchNode(MathematicalExpression, out List<BranchNode> subBranchNodes);

            yield return new XElement(xNamespace + "timeSeries",
                                      new XAttribute("id", rootNode.YName));

            foreach (BranchNode branchNode in subBranchNodes)
            {
                yield return new XElement(xNamespace + "timeSeries",
                                          new XAttribute("id", branchNode.YName));
            }
        }

        private XElement CreateTriggerForExpression(XNamespace xNamespace, BranchNode branchNode, string id)
        {
            XElement trigger = new XElement(xNamespace + "trigger");
            var expression = new XElement(xNamespace + "expression", new XAttribute("id", id));

            expression.Add(CreateXElementForNode(xNamespace, branchNode.FirstNode, "x1Value", "x1Series"));
            expression.Add(new XElement(xNamespace + "mathematicalOperator",
                                        ConvertToMathematicalOperatorEnumStringType(branchNode.OperatorValue)));
            expression.Add(CreateXElementForNode(xNamespace, branchNode.SecondNode, "x2Value", "x2Series"));
            expression.Add(new XElement(xNamespace + "y", branchNode.YName));

            trigger.Add(expression);
            return trigger;
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
                nodeReference = MathematicalExpression.Name + "/" + nodeReference;
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
        
        private static BranchNode ParseMathematicalExpressionToRootBranchNode(MathematicalExpression mathematicalExpression, out List<BranchNode> subBranchNodes)
        {
            bool resultParsing = ExpressionParser.TryParse(mathematicalExpression.Expression, out IExpressionNode iRootNode,
                                                           out string errorMessage);

            if (!resultParsing)
            {
                throw new ArgumentException(errorMessage);
            }

            if (!(iRootNode is BranchNode rootNode))
            {
                throw new InvalidOperationException(
                    String.Format(
                        Resources
                            .MathematicalExpressionSerializer_ParseMathematicalExpressionToRootBranchNode_Mathematical_expression__0__contains_invalid_expression__1__,
                        mathematicalExpression.Name, mathematicalExpression.Expression));
            }

            List<IExpressionNode> allSubNodes = rootNode.GetChildNodes().ToList();

            subBranchNodes = allSubNodes.OfType<BranchNode>().ToList();
            List<ParameterLeafNode> subParameterLeafNodes = allSubNodes.OfType<ParameterLeafNode>().ToList();

            CorrectXmlInputNamesForLeafNodes(mathematicalExpression, subParameterLeafNodes);

            SetYNameOfRootNode(rootNode, mathematicalExpression);
            SetYNamesOfSubBranchNodes(subBranchNodes, mathematicalExpression);
            return rootNode;
        }
        private static void CorrectXmlInputNamesForLeafNodes(MathematicalExpression mathematicalExpression, List<ParameterLeafNode> leafNodes)
        {
            foreach (ParameterLeafNode leafNode in leafNodes)
            {
                KeyValuePair<char, string> parameterKvp =
                    mathematicalExpression.InputMapping.FirstOrDefault(i => i.Key.ToString() == leafNode.Value);
                if (!parameterKvp.Equals(default(KeyValuePair<char, string>)))
                {
                    IInput expressionInput =
                        mathematicalExpression.Inputs.First(i => i.Name.Equals(parameterKvp.Value));
                    
                    InputSerializerBase serializer =
                        SerializerCreator.CreateSerializerType<InputSerializerBase>((RtcBaseObject)expressionInput);
                    string xmlName = serializer.GetXmlName();

                    leafNode.Value = xmlName;
                }
            }
        }

        private static void SetYNameOfRootNode(BranchNode branchNode, MathematicalExpression mathematicalExpression)
        {
            branchNode.YName = mathematicalExpression.Name;
        }

        private static void SetYNamesOfSubBranchNodes(List<BranchNode> branchNodes, MathematicalExpression mathematicalExpression)
        {
            foreach (BranchNode branchNode in branchNodes)
            {
                branchNode.YName = mathematicalExpression.Name + "/" + branchNode;
            }
        }
    }
}