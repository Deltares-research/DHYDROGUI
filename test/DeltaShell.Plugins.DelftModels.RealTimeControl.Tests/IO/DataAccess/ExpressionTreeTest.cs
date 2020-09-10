using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.DataAccess;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.IO.DataAccess
{
    [TestFixture]
    public class ExpressionTreeTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Setup
            const string id = "expression_id";
            const string controlGroupName = "controlGroupName";
            var branchNode = Substitute.For<IBranchNode>();
            var expression = new MathematicalExpression();

            // Call
            var expressionTree = new ExpressionTree(branchNode, controlGroupName, id, expression);

            // Assert
            Assert.That(expressionTree.Id, Is.EqualTo(id));
            Assert.That(expressionTree.ControlGroupName, Is.EqualTo(controlGroupName));
            Assert.That(expressionTree.RootNode, Is.SameAs(branchNode));
            Assert.That(expressionTree.Object, Is.SameAs(expression));
        }
    }
}