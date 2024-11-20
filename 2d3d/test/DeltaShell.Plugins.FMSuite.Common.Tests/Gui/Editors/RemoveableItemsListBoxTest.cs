using System.Windows.Forms;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.Gui.Editors
{
    [TestFixture]
    public class RemoveableItemsListBoxTest
    {
        [Test]
        public void WhenConstructorRemoveableItemsListBox_ThenExpectedValues()
        {
            //Arrange
            const int expectedItemHeigth = 13;

            //Act
            var removableItemsListBox = new RemoveableItemsListBox();

            //Assert
            Assert.That(removableItemsListBox.ItemHeight, Is.EqualTo(expectedItemHeigth));
            Assert.That(removableItemsListBox.DrawMode, Is.EqualTo(DrawMode.OwnerDrawFixed));
            Assert.That(removableItemsListBox.IntegralHeight, Is.False);
        }
    }
}