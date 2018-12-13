using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.FileReaders.SpatialData;
using DeltaShell.NGHS.IO.Helpers;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders
{
    [TestFixture]
    public class SalinityConverterTest
    {
        [Test]
        public void GivenCorrectSalinityDataModel_WhenCovertingForSalinity_ThenEstuaryMoutNodeIdIsReturned()
        {
            // Given
            const string nodeId = "myNodeId";
            var mouthCategory = new DelftIniCategory(SalinityRegion.MouthHeader);
            mouthCategory.AddProperty(SalinityRegion.NodeId.Key, nodeId);
            var categories = new List<DelftIniCategory> { mouthCategory };

            // When
            var estuaryMouthNodeId = SalinityConverter.Convert(categories, new List<string>());

            // Then
            Assert.That(estuaryMouthNodeId, Is.EqualTo(nodeId));
        }

        [Test]
        public void GivenDataModelWithoutMouthCategory_WhenConvertingForSalinity_ThenErrorMessageIsReturnedAndNoException()
        {
            // Given
            var otherCategory = new DelftIniCategory("OtherCategoryName");
            var categories = new List<DelftIniCategory> { otherCategory };
            var errorMessages = new List<string>();

            // When
            var estuaryMouthNodeId = SalinityConverter.Convert(categories, errorMessages);

            // Then
            Assert.IsNull(estuaryMouthNodeId);
            Assert.That(errorMessages.Count, Is.EqualTo(1));
            var expectedErrorMessage = $"Expected a category with name '{SalinityRegion.MouthHeader}' in the file 'Salinity.ini', but it was not present. Nothing was read from this file.";
            Assert.That(errorMessages.FirstOrDefault(), Is.EqualTo(expectedErrorMessage));
        }
    }
}