using System.Collections.Generic;
using DelftTools.Shell.Core.Workflow;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Gui
{
    [TestFixture]
    public class OutputTreeFolderTest
    {
        [Test]
        public void Constructor_ShouldSetCorrectValues()
        {
            // Arrange
            var model = Substitute.For<IModel>();
            var children = Substitute.For<IEnumerable<object>>();

            // Act
            var outputFolder = new OutputTreeFolder(model, children, "test");

            // Assert
            Assert.AreEqual("test", outputFolder.Text);
            Assert.AreSame(children, outputFolder.ChildItems);
            Assert.AreSame(model, outputFolder.Parent);
        }
    }
}