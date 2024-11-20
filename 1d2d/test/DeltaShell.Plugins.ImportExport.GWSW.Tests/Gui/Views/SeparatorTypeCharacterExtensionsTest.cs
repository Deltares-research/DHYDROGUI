using DeltaShell.Plugins.ImportExport.GWSW.Views;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.GWSW.Tests.Gui.Views
{
    [TestFixture]
    public class SeparatorTypeCharacterExtensionsTest
    {
        [Test]
        [TestCase(SeparatorType.Comma, ',')]
        [TestCase(SeparatorType.Semicolon, ';')]
        [TestCase(SeparatorType.Tab, '\t')]
        [TestCase(SeparatorType.Space, ' ')]
        [TestCase(SeparatorType.Other, '-')]
        public void GivenSeparatorType_GetChar_ShouldReturnCorrectCharacter(SeparatorType type, char expectedChar)
        {
            //Arrange
            char otherChar = '-';

            // Act & Assert
            Assert.AreEqual(expectedChar, type.GetChar(otherChar));
        }
    }
}