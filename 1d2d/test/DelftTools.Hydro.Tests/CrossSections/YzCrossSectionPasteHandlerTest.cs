using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.Extensions;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.CrossSections
{
    [TestFixture]
    public class YzCrossSectionPasteHandlerTest
    {
        private CrossSectionDefinitionYZ crossSection;
        private YzCrossSectionPasteHandler pasteHandler;

        [SetUp]
        public void Setup()
        {
            crossSection = new CrossSectionDefinitionYZ();
            pasteHandler = new YzCrossSectionPasteHandler(crossSection);
        }
        
        [Test]
        public void DefaultYZCrossSection_GivesThalwegOf50()
        {
            //Arrange
            crossSection.YZDataTable.AddCrossSectionYZRow(0, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(22.22222222, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(33.33333333, -10);
            crossSection.YZDataTable.AddCrossSectionYZRow(66.66666667, -10);
            crossSection.YZDataTable.AddCrossSectionYZRow(77.77777778, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
            
            //Act
            pasteHandler.FinishPasteActions();

            //Assert
            Assert.That(crossSection.Thalweg, Is.EqualTo(50));
        }

        [Test]
        public void YZCrossSectionWithTwoLowestPointsInSequence_GivesThalwegOfMiddleOfSequence()
        {
            //Arrange
            crossSection.YZDataTable.AddCrossSectionYZRow(0, 100);
            crossSection.YZDataTable.AddCrossSectionYZRow(50, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(150, 100);
            
            //Act
            pasteHandler.FinishPasteActions();

            //Assert
            Assert.That(crossSection.Thalweg, Is.EqualTo(75));
        }

        [Test]
        public void YZCrossSectionWithOneLowestPoint_GivesThalwegOfLowestPoint()
        {
            //Arrange
            crossSection.YZDataTable.AddCrossSectionYZRow(0, 100);
            crossSection.YZDataTable.AddCrossSectionYZRow(50, 100);
            crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(150, 100);
            
            //Act
            pasteHandler.FinishPasteActions();

            //Assert
            Assert.That(crossSection.Thalweg, Is.EqualTo(100));
        }

        [Test]
        public void YZCrossSectionWithMultipleLowestPointsInSequence_GivesThalwegOfMiddleOfSequence()
        {
            //Arrange
            crossSection.YZDataTable.AddCrossSectionYZRow(0, 100);
            crossSection.YZDataTable.AddCrossSectionYZRow(50, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(100, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(150, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(200, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(250, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(300, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(350, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(400, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(450, 100);
            
            //Act
            pasteHandler.FinishPasteActions();

            //Assert
            Assert.That(crossSection.Thalweg, Is.EqualTo(225));
        }
        
        [Test]
        public void YZCrossSectionWithEmptyCrossSection_GivesNoChangedThalweg()
        {
            //Arrange
            double previousThalweg = crossSection.Thalweg;

            //Act
            pasteHandler.FinishPasteActions();

            //Assert
            Assert.That(crossSection.Thalweg, Is.EqualTo(previousThalweg));
        }
    }
}