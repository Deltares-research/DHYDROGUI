using System;
using GeoAPI.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Utils.Test
{
    [TestFixture]
    public class BranchFeatureExtensionsTest
    {
        [Test]
        [TestCase(100, true)]
        [TestCase(100 - 1e-12, true)]
        [TestCase(99, false)]
        [TestCase(double.NaN, false)]
        [TestCase(double.PositiveInfinity, false)]
        public void GivenBranchFeature_IsOnEndOfBranch_ShouldReturnCorrectResult(double chainage, bool endOfBranch)
        {
            //Arrange
            var location = Substitute.For<IBranchFeature>();
            var branch = Substitute.For<IBranch>();

            location.Branch.Returns(branch);
            branch.Length.Returns(100);

            // Act & Assert
            location.Chainage = chainage;
            Assert.AreEqual(endOfBranch, location.IsOnEndOfBranch());
        }

        [Test]
        public void GivenBranchFeature_IsOnEndOfBranch_ShouldThrowWhenNull()
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentNullException>(() => ((IBranchFeature)null).IsOnEndOfBranch());
        }

        [Test]
        public void GivenBranchFeature_IsOnEndOfBranch_ShouldThrowWhenBranchNull()
        {
            //Arrange
            var location = Substitute.For<IBranchFeature>();

            location.Chainage = 1;
            location.Branch = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => location.IsOnEndOfBranch());
        }

        [Test]
        [TestCase(0, true)]
        [TestCase(100, false)]
        [TestCase(100 - 1e-12, false)]
        public void GivenBranchFeatureExtensions_GetNodeForPosition_ShouldReturnExpectedNode(double chainage, bool onSourceNode)
        {
            //Arrange
            var location = Substitute.For<IBranchFeature>();
            var branch = Substitute.For<IBranch>();
            var sourceNode = Substitute.For<INode>();
            var targetNode = Substitute.For<INode>();

            location.Branch.Returns(branch);

            branch.Length.Returns(100);
            branch.Source.Returns(sourceNode);
            branch.Target.Returns(targetNode);

            location.Chainage = chainage;
            
            // Act
            var onNode = location.TryGetNode(out var node);

            // Assert
            Assert.IsTrue(onNode, "Expected node to be found");

            var expectedNode = onSourceNode ? sourceNode : targetNode;
            Assert.AreEqual(expectedNode, node, $"Expected the node to be the {(onSourceNode ? "source" : "target")} node");
        }

        [Test]
        public void GivenBranchFeatureExtensions_GetNodeForPosition_ShouldReturnNoNodeWhenNotAtBeginEndOfBranch()
        {
            //Arrange
            var location = Substitute.For<IBranchFeature>();
            var branch = Substitute.For<IBranch>();
            var sourceNode = Substitute.For<INode>();
            var targetNode = Substitute.For<INode>();

            location.Branch.Returns(branch);

            branch.Length.Returns(100);
            branch.Source.Returns(sourceNode);
            branch.Target.Returns(targetNode);

            location.Chainage = 50;

            // Act
            var onNode = location.TryGetNode(out var node);

            // Assert
            Assert.IsFalse(onNode, "Expected node to be found");
        }
    }
}