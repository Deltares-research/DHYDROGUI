using System.Collections.Generic;
using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class BranchNodeTest
    {
        [Test]
        public void SetFirstNode_SetsCorrectly()
        {
            // Setup
            const Operator @operator = Operator.Add;
            var branchNode = new BranchNode(@operator);

            var expressionNode = Substitute.For<IExpressionNode>();

            // Call
            branchNode.FirstNode = expressionNode;

            // Assert
            Assert.That(branchNode.FirstNodeReference.Node, Is.SameAs(expressionNode));
        }

        [Test]
        public void GetFirstNode_GetsCorrectResult()
        {
            // Setup
            const Operator @operator = Operator.Add;
            var branchNode = new BranchNode(@operator);

            var expressionNode = Substitute.For<IExpressionNode>();
            branchNode.FirstNodeReference.Node = expressionNode;

            // Call
            IExpressionNode result = branchNode.FirstNode;

            // Assert
            Assert.That(result, Is.SameAs(expressionNode));
        }

        [Test]
        public void SetSecondNode_SetsCorrectly()
        {
            // Setup
            const Operator @operator = Operator.Add;
            var branchNode = new BranchNode(@operator);

            var expressionNode = Substitute.For<IExpressionNode>();

            // Call
            branchNode.SecondNode = expressionNode;

            // Assert
            Assert.That(branchNode.SecondNodeReference.Node, Is.SameAs(expressionNode));
        }

        [Test]
        public void GetSecondNode_GetsCorrectResult()
        {
            // Setup
            const Operator @operator = Operator.Add;
            var branchNode = new BranchNode(@operator);

            var expressionNode = Substitute.For<IExpressionNode>();
            branchNode.SecondNodeReference.Node = expressionNode;

            // Call
            IExpressionNode result = branchNode.SecondNode;

            // Assert
            Assert.That(result, Is.SameAs(expressionNode));
        }

        [TestCase(Operator.Add)]
        [TestCase(Operator.Subtract)]
        [TestCase(Operator.Multiply)]
        [TestCase(Operator.Divide)]
        [TestCase(Operator.Min)]
        [TestCase(Operator.Max)]
        public void Constructor_InitializesInstanceCorrectly(Operator @operator)
        {
            // Call
            var branchNode = new BranchNode(@operator);

            // Assert
            Assert.That(branchNode.OperatorValue, Is.EqualTo(@operator));
            Assert.That(branchNode.FirstNodeReference, Is.Not.Null);
            Assert.That(branchNode.SecondNodeReference, Is.Not.Null);
        }

        [TestCase(Operator.Add)]
        [TestCase(Operator.Subtract)]
        [TestCase(Operator.Multiply)]
        [TestCase(Operator.Divide)]
        [TestCase(Operator.Min)]
        [TestCase(Operator.Max)]
        public void Constructor_WithYName_InitializesInstanceCorrectly(Operator @operator)
        {
            // Setup
            const string yName = "y_name";

            // Call
            var branchNode = new BranchNode(@operator, yName);

            // Assert
            Assert.That(branchNode.OperatorValue, Is.EqualTo(@operator));
            Assert.That(branchNode.YName, Is.EqualTo(yName));
            Assert.That(branchNode.FirstNodeReference, Is.Not.Null);
            Assert.That(branchNode.SecondNodeReference, Is.Not.Null);
        }

        [TestCaseSource(nameof(GetChildNodesTestCases))]
        public void GetChildNodes_ReturnsCorrectResults(IExpressionNode firstNode, IExpressionNode secondNode,
                                                        IExpressionNode[] expectedChildNodes)
        {
            // Setup
            const Operator operatorValue = Operator.Add;
            var branchNode = new BranchNode(operatorValue)
            {
                FirstNode = firstNode,
                SecondNode = secondNode
            };

            // Call
            IExpressionNode[] childNodes = branchNode.GetChildNodes().ToArray();

            // Assert
            CollectionAssert.AreEquivalent(expectedChildNodes, childNodes);
        }

        [TestCase(Operator.Add)]
        [TestCase(Operator.Subtract)]
        [TestCase(Operator.Multiply)]
        [TestCase(Operator.Divide)]
        [TestCase(Operator.Min)]
        [TestCase(Operator.Max)]
        public void GetExpression_ReturnsCorrectResult(Operator @operator)
        {
            // Setup
            const string firstSubExpression = "expression_A";
            IExpressionNode firstNode = GetExpressionNode(firstSubExpression);

            const string secondSubExpression = "expression_B";
            IExpressionNode secondNode = GetExpressionNode(secondSubExpression);

            var branchNode = new BranchNode(@operator, "")
            {
                FirstNode = firstNode,
                SecondNode = secondNode
            };

            // Call
            string result = branchNode.GetExpression();

            // Assert
            string expected = string.Format(@operator.ToFormatString(), firstSubExpression, secondSubExpression);
            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(Operator.Add)]
        [TestCase(Operator.Subtract)]
        [TestCase(Operator.Multiply)]
        [TestCase(Operator.Divide)]
        [TestCase(Operator.Min)]
        [TestCase(Operator.Max)]
        public void ToString_ReturnsCorrectResult(Operator @operator)
        {
            // Setup
            IExpressionNode firstNode = GetExpressionNode("expression_A");
            IExpressionNode secondNode = GetExpressionNode("expression_B");

            var branchNode = new BranchNode(@operator, "")
            {
                FirstNode = firstNode,
                SecondNode = secondNode
            };

            // Call
            var result = branchNode.ToString();

            // Assert
            string expected = branchNode.GetExpression();
            Assert.That(result, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> GetChildNodesTestCases()
        {
            yield return GetTwoLeafNodesTestCase();
            yield return GetOneBranchNodeOneLeafNodeTestCase();
            yield return GetOneLeafNodeOneBranchNodeTestCase();
            yield return GetTwoBranchNodesTestCase();
        }

        private static TestCaseData GetOneBranchNodeOneLeafNodeTestCase()
        {
            var firstChildNode = Substitute.For<IBranchNode>();
            var firstGrandChildNode = Substitute.For<IExpressionNode>();
            var secondGrandChildNode = Substitute.For<IExpressionNode>();
            firstChildNode.GetChildNodes().Returns(new List<IExpressionNode>
            {
                firstGrandChildNode,
                secondGrandChildNode
            });
            var secondChildNode = Substitute.For<IExpressionNode>();

            IExpressionNode[] expectedResult =
            {
                firstChildNode,
                firstGrandChildNode,
                secondGrandChildNode,
                secondChildNode
            };
            return new TestCaseData(firstChildNode, secondChildNode, expectedResult);
        }

        private static TestCaseData GetOneLeafNodeOneBranchNodeTestCase()
        {
            var firstChildNode = Substitute.For<IExpressionNode>();
            var secondChildNode = Substitute.For<IBranchNode>();
            var firstGrandChildNode = Substitute.For<IExpressionNode>();
            var secondGrandChildNode = Substitute.For<IExpressionNode>();
            secondChildNode.GetChildNodes().Returns(new List<IExpressionNode>
            {
                firstGrandChildNode,
                secondGrandChildNode
            });

            IExpressionNode[] expectedResult =
            {
                firstChildNode,
                firstGrandChildNode,
                secondGrandChildNode,
                secondChildNode
            };
            return new TestCaseData(firstChildNode, secondChildNode, expectedResult);
        }

        private static TestCaseData GetTwoBranchNodesTestCase()
        {
            var firstChildNode = Substitute.For<IBranchNode>();
            var firstGrandChildNode = Substitute.For<IExpressionNode>();
            var secondGrandChildNode = Substitute.For<IExpressionNode>();
            firstChildNode.GetChildNodes().Returns(new List<IExpressionNode>
            {
                firstGrandChildNode,
                secondGrandChildNode
            });

            var secondChildNode = Substitute.For<IBranchNode>();
            var thirdGrandChildNode = Substitute.For<IExpressionNode>();
            var fourthGrandChildNode = Substitute.For<IExpressionNode>();
            secondChildNode.GetChildNodes().Returns(new List<IExpressionNode>
            {
                thirdGrandChildNode,
                fourthGrandChildNode
            });

            IExpressionNode[] expectedResult =
            {
                firstChildNode,
                firstGrandChildNode,
                secondGrandChildNode,
                secondChildNode,
                thirdGrandChildNode,
                fourthGrandChildNode
            };
            return new TestCaseData(firstChildNode, secondChildNode, expectedResult);
        }

        private static TestCaseData GetTwoLeafNodesTestCase()
        {
            var firstChildNode = Substitute.For<IExpressionNode>();
            var secondChildNode = Substitute.For<IExpressionNode>();

            IExpressionNode[] expectedResult =
            {
                firstChildNode,
                secondChildNode
            };
            return new TestCaseData(firstChildNode, secondChildNode, expectedResult);
        }

        private static IExpressionNode GetExpressionNode(string firstSubExpression)
        {
            var firstNode = Substitute.For<IExpressionNode>();
            firstNode.GetExpression().Returns(firstSubExpression);

            return firstNode;
        }
    }
}