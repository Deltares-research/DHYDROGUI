using System;
using System.Collections.Generic;
using System.Globalization;
using DeltaShell.NGHS.Common.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.Export
{
    [TestFixture]
    public class ExpressionParserTest
    {
        private readonly Random random = new Random();

        [Test]
        public void TryParse_ExpressionValid_ReturnsTrue()
        {
            // Setup
            int depth = random.Next(1, 5);
            string expressionStr = GetRandomExpression(depth);

            // Call
            bool result = ExpressionParser.TryParse(expressionStr, out IExpressionNode rootNode, out string errorMessage);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(rootNode, Is.Not.Null);
            Assert.That(errorMessage, Is.Null);
        }

        [TestCase("a * b")]
        [TestCase("(A * B")]
        [TestCase("Min(A,B)")]
        [TestCase("miN(A,B)")]
        [TestCase("Max(A,B)")]
        [TestCase("max(A,b)")]
        [TestCase(@"A \ B")]
        [TestCase("A % B")]
        [TestCase("A = B")]
        [TestCase("")]
        public void TryParse_ExpressionInvalid_ReturnsFalse(string expressionStr)
        {
            // Call
            bool result = ExpressionParser.TryParse(expressionStr, out IExpressionNode rootNode, out string errorMessage);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(rootNode, Is.Null);
            Assert.That(errorMessage, Is.Not.Null);
        }

        [TestCase(Operator.Add)]
        [TestCase(Operator.Subtract)]
        [TestCase(Operator.Divide)]
        [TestCase(Operator.Multiply)]
        [TestCase(Operator.Min)]
        [TestCase(Operator.Max)]
        public void TryParse_ExpressionValid_ReturnsCorrectTree(Operator @operator)
        {
            // Setup
            string leafValue1 = GetRandomLeafValue();
            string leafValue2 = GetRandomParameter();
            string leafValue3 = GetRandomLeafValue();
            string leafValue4 = GetRandomConstant();

            Operator operator1 = GetRandomOperator();
            Operator operator2 = GetRandomOperator();

            // extra braces to be able to validate the expression tree -> A + B + C = (A + B) + C = A + (B + C)
            var subExpression1 = $"({GetExpressionString(leafValue1, leafValue2, operator1)})";
            var subExpression2 = $"({GetExpressionString(leafValue3, leafValue4, operator2)})";

            string expression = GetExpressionString(subExpression1, subExpression2, @operator);

            // Call
            bool result = ExpressionParser.TryParse(expression, out IExpressionNode rootNode, out string errorMessage);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(rootNode, Is.Not.Null);
            Assert.That(errorMessage, Is.Null);

            var branchNode = rootNode as BranchNode;
            Assert.That(branchNode, Is.Not.Null);
            Assert.That(branchNode.OperatorValue, Is.EqualTo(@operator));

            AssertCorrectBranchNode(branchNode.FirstNode, operator1, leafValue1, leafValue2);
            AssertCorrectBranchNode(branchNode.SecondNode, operator2, leafValue3, leafValue4);
        }

        private static void AssertCorrectBranchNode(IExpressionNode node, Operator @operator,
                                                    string firstLeafValue, string secondLeafValue)
        {
            var branchNode = node as BranchNode;
            Assert.That(branchNode, Is.Not.Null);
            Assert.That(branchNode.OperatorValue, Is.EqualTo(@operator));

            AssertCorrectLeafNode(branchNode.FirstNode, firstLeafValue);
            AssertCorrectLeafNode(branchNode.SecondNode, secondLeafValue);
        }

        private static void AssertCorrectLeafNode(IExpressionNode node, string value)
        {
            var leafNode = node as ILeafNode;
            Assert.That(leafNode, Is.Not.Null);
            Assert.That(leafNode.Value, Is.EqualTo(value));
        }

        private string GetRandomExpression(int depth)
        {
            var expressions = new Queue<string>();

            int nLeaves = 1 << depth;
            Repeat.Action(nLeaves, () => expressions.Enqueue(GetRandomLeafValue()));

            while (expressions.Count > 1)
            {
                string a = expressions.Dequeue();
                string b = expressions.Dequeue();
                string expression = GetRandomExpression(a, b);

                expressions.Enqueue(expression);
            }

            return expressions.Dequeue();
        }

        private string GetRandomExpression(string a, string b)
        {
            Operator @operator = GetRandomOperator();
            return GetExpressionString(a, b, @operator);
        }

        private static string GetExpressionString(string a, string b, Operator @operator)
        {
            return string.Format(@operator.ToFormatString(), a, b);
        }

        private Operator GetRandomOperator()
        {
            // TODO use random.NextEnumValue
            Array operatorValues = Enum.GetValues(typeof(Operator));
            int randInt = random.Next(operatorValues.Length - 1);
            return (Operator) operatorValues.GetValue(randInt);
        }

        private string GetRandomLeafValue()
        {
            // TODO use random.NextBool
            return random.Next(0, 1) == 0
                       ? GetRandomConstant()
                       : GetRandomParameter();
        }

        private string GetRandomParameter()
        {
            return Convert.ToChar(random.Next(25) + 65).ToString();
        }

        private string GetRandomConstant()
        {
            double value = random.Next(10) * random.NextDouble();
            return Math.Round(value, 1).ToString(CultureInfo.InvariantCulture);
        }
    }
}