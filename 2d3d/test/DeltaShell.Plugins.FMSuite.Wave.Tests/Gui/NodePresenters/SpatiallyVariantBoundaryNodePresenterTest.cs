using System;
using DelftTools.Controls;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries;
using DeltaShell.Plugins.FMSuite.Wave.Gui.NodePresenters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.NodePresenters
{
    [TestFixture]
    public class SpatiallyVariantBoundaryNodePresenterTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IBoundaryContainer GetBoundaryContainer(IWaveBoundary _) => boundaryContainer;

            // Call
            var nodePresenter = new SpatiallyVariantBoundaryNodePresenter(GetBoundaryContainer);

            // Assert
            Assert.That(nodePresenter, Is.InstanceOf<FMSuiteNodePresenterBase<IWaveBoundary>>());
        }

        [Test]
        public void Constructor_GetBoundaryContainerFuncNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => new SpatiallyVariantBoundaryNodePresenter(null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("getBoundaryContainerFunc"),
                        "Expected a different ParamName:");
        }

        [Test]
        public void CanRemove_ReturnsTrue()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IBoundaryContainer GetBoundaryContainer(IWaveBoundary _) => boundaryContainer;

            var nodePresenter = new SpatiallyVariantBoundaryNodePresenter(GetBoundaryContainer);

            var boundary = Substitute.For<IWaveBoundary>();

            // Call
            bool result = nodePresenter.CanRemove(boundaryContainer, boundary);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void UpdateNode_SetsCorrectValues()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();
            var node = Substitute.For<ITreeNode>();

            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IBoundaryContainer GetBoundaryContainer(IWaveBoundary _) => boundaryContainer;

            const string boundaryName = "BoundaryName";
            boundary.Name = boundaryName;

            var nodePresenter = new SpatiallyVariantBoundaryNodePresenter(GetBoundaryContainer);

            // Call
            nodePresenter.UpdateNode(null, node, boundary);

            // Assert
            node.Received(1).Text = boundaryName;
        }

        [Test]
        public void RemoveNodeData_RemovesBoundaryFromBoundaryContainer()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();

            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IBoundaryContainer GetBoundaryContainer(IWaveBoundary _) => boundaryContainer;

            var boundaries = new EventedList<IWaveBoundary> {boundary};
            boundaryContainer.Boundaries.Returns(boundaries);

            const string boundaryName = "BoundaryName";
            boundary.Name = boundaryName;

            var nodePresenter = new SpatiallyVariantBoundaryNodePresenter(GetBoundaryContainer);

            // Call
            nodePresenter.RemoveNodeData(null, boundary);

            // Assert
            Assert.That(boundaries, Is.Empty);
        }

        [Test]
        public void GetContextMenu_NodeDataNotIWaveBoundary_ReturnsNull()
        {
            // Setup
            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IBoundaryContainer GetBoundaryContainer(IWaveBoundary _) => boundaryContainer;

            var nodePresenter = new SpatiallyVariantBoundaryNodePresenter(GetBoundaryContainer);

            var sender = Substitute.For<ITreeNode>();
            var someOtherData = new object();

            // Call
            IMenuItem result = nodePresenter.GetContextMenu(sender, someOtherData);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetContextMenu_CorrectArgs_ReturnsValidContextMenu()
        {
            // Setup
            var boundary = Substitute.For<IWaveBoundary>();

            var boundaryContainer = Substitute.For<IBoundaryContainer>();
            IBoundaryContainer GetBoundaryContainer(IWaveBoundary _) => boundaryContainer;
            var nodePresenter = new SpatiallyVariantBoundaryNodePresenter(GetBoundaryContainer);

            // Call
            IMenuItem result = nodePresenter.GetContextMenu(null, boundary);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1),
                        "Expected the number of elements in the Menu to be different:");

            IMenuItem menuItem = result[0];

            Assert.That(menuItem, Is.Not.Null, "Expected the MenuItem not to be null:");
            Assert.That(menuItem.Text, Is.EqualTo("Delete"));
        }
    }
}