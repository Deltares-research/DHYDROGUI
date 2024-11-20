using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CrossSectionView
{
    [TestFixture]
    public class ReplaceVerifyTest
    {
        private const int minimalNumberOfTableRowsAllowed = 3;
        private CrossSectionDefinitionYZ yzCrossSectionDefinition;
        private CrossSectionDefinition nonYzCrossSectionDefinition;
        private IReplaceVerifier replaceVerifier;

        [SetUp]
        public void Setup()
        {
            yzCrossSectionDefinition = new CrossSectionDefinitionYZ();
            nonYzCrossSectionDefinition = new CrossSectionDefinitionStandard();
            replaceVerifier = yzCrossSectionDefinition.GenerateReplaceVerifier(minimalNumberOfTableRowsAllowed);
        }

        [Test]
        public void GivenIsMinimalAmountOfRowsThenReturnTrue()
        {
            //Arrange
            const int minimalNumberOfTableRows = 3;
            //Act & Assert
            Assert.AreEqual(true, replaceVerifier.ShouldReplace(minimalNumberOfTableRows));
        }

        [Test]
        public void GivenAboveMinimalAmountOfRowsThenReturnTrue()
        {
            //Arrange
            const int aboveminimalNumberOfTableRows = 4;
            //Act & Assert
            Assert.AreEqual(true, replaceVerifier.ShouldReplace(aboveminimalNumberOfTableRows));
        }

        [Test]
        public void GivenBelowMinimalAmountOfRowsThenReturnFalse()
        {
            //Arrange
            const int belowminimalNumberOfTableRows = 2;
            //Act & Assert
            Assert.AreEqual(false, replaceVerifier.ShouldReplace(belowminimalNumberOfTableRows));
        }

        [Test]
        public void NonYzCrossSectionDefinitionReturnsNull()
        {
            //Arrange & Act
            replaceVerifier = nonYzCrossSectionDefinition.GenerateReplaceVerifier(minimalNumberOfTableRowsAllowed);

            //Assert
            Assert.That(replaceVerifier, Is.Null);
        }

        [Test]
        public void NonYzCrossSectionDefinitionFinishPasteHandlingDoNotDoAnything()
        {
            //Arrange
            ICrossSectionDefinition copyCrossSectionDefinition = nonYzCrossSectionDefinition;

            //Act
            nonYzCrossSectionDefinition.FinishPasteHandling();

            //Assert
            Assert.That(nonYzCrossSectionDefinition, Is.EqualTo(copyCrossSectionDefinition));
        }
    }
}