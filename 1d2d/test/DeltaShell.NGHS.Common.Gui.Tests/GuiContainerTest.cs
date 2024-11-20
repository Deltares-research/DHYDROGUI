using DelftTools.Shell.Gui;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests
{
    [TestFixture]
    public class GuiContainerTest
    {
        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            var container = new GuiContainer();

            // Assert
            Assert.That(container.Gui, Is.Null);
        }

        [Test]
        public void GetGui_ReturnsOriginalValue()
        {
            // Setup
            var gui = Substitute.For<IGui>();
            var container = new GuiContainer();

            // Call
            container.Gui = gui;

            // Assert
            Assert.That(container.Gui, Is.SameAs(gui));
        }
    }
}