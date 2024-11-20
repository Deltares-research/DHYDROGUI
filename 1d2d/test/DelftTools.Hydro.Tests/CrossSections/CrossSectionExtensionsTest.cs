using DelftTools.Controls;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using NSubstitute;
using NUnit.Framework;
using Rhino.Mocks;

namespace DelftTools.Hydro.Tests.CrossSections
{
    [TestFixture]
    public class CrossSectionExtensionsTest
    {
        [Test]
        public void WhenGeneratingReplaceVerifierWithYzCrossSection_GivesYzReplaceVerifier()
        {
            //Arrange
            var crossSection = new CrossSectionDefinitionYZ();

            //Act
            IReplaceVerifier returnedReplaceVerifier = crossSection.GenerateReplaceVerifier(0);
            
            //Assert
            Assert.That(returnedReplaceVerifier.GetType(), Is.EqualTo(typeof(YzCrossSectionAndMinimalNumberOfTableRowsReplaceVerifier)));
        }

        [Test]
        public void WhenGeneratingReplaceVerifierWithNoYzCrossSection_GivesNull()
        {
            //Arrange
            var crossSection = new CrossSectionDefinitionZW();

            //Act
            IReplaceVerifier returnedReplaceVerifier = crossSection.GenerateReplaceVerifier(0);
            
            //Assert
            Assert.That(returnedReplaceVerifier, Is.Null);
        }

        [Test]
        public void WhenFinishPasteHandlingOfYzCrossSection_ThenFinishPasteActionsCalledAndThalwegExpectedValue()
        {
            //Arrange
            CrossSectionDefinitionYZ crossSection = new CrossSectionDefinitionYZ();
            crossSection.YZDataTable.AddCrossSectionYZRow(0, 100);
            crossSection.YZDataTable.AddCrossSectionYZRow(50, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(150, 100);
            
            //Act
            crossSection.FinishPasteHandling();

            //Assert
            Assert.That(crossSection.Thalweg, Is.EqualTo(75));
        }
        
        [Test]
        public void WhenFinishPasteHandlingOfNoYzCrossSection_ThenFinishPasteActionsNotCalledAndNoChangeInThalweg()
        {
            //Arrange
            CrossSectionDefinitionZW crossSection = new CrossSectionDefinitionZW();
            crossSection.ZWDataTable.AddCrossSectionZWRow(0, 100 ,0 );
            crossSection.ZWDataTable.AddCrossSectionZWRow(50, 0,0 );
            crossSection.ZWDataTable.AddCrossSectionZWRow(100, 0,0 );
            crossSection.ZWDataTable.AddCrossSectionZWRow(150, 100,0 );
            var expectedThalweg = crossSection.Thalweg;
            
            //Act
            crossSection.FinishPasteHandling();

            //Assert
            Assert.That(crossSection.Thalweg, Is.EqualTo(expectedThalweg));
        }
    }
}