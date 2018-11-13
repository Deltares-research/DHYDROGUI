using System.Collections.Generic;
using System.Linq;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.FileWriters.Location;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ImportExport.Boundary;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.ImportExport.Boundary
{
    [TestFixture]
    class BoundaryLocationConverterTest
    {
        // Happy Flow Handling
        /// <summary>
        /// GIVEN A set containing a single DelftIniCategory containing a valid nodeID and type
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A set containing a single BoundaryLocation with this nodeID and type is returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingAValidNodeIDAndType_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenASetContainingASingleBoundaryLocationWithThisNodeIDAndTypeIsReturned()
        {
            // Given
            const string nodeID = "SomeNodeID";
            const BoundaryType boundaryType = BoundaryType.Level;

            var validCategoriesSet = new List<DelftIniCategory>();

            var validCategory = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            validCategory.AddProperty(BoundaryRegion.NodeId.Key, nodeID, BoundaryRegion.NodeId.Description);
            validCategory.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(boundaryType), BoundaryRegion.Type.Description);

            validCategoriesSet.Add(validCategory);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(validCategoriesSet, errorMsgs);

            // Then
            Assert.That(outputSet.Count, Is.EqualTo(1));
            var boundaryLocation = outputSet.First();

            Assert.That(boundaryLocation.Name, Is.EqualTo(nodeID));
            Assert.That(boundaryLocation.BoundaryType, Is.EqualTo(boundaryType));
            Assert.That(boundaryLocation.ThatcherHarlemannCoefficient, Is.EqualTo(0));

            Assert.That(errorMsgs.Count, Is.EqualTo(0));
        }


        /// <summary>
        /// GIVEN A set containing a single DelftIniCategory containing a valid nodeID and type and thatcher harlemann coefficient
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A set containing a single BoundaryLocation with this nodeID and type and thatcher harlemann coefficient is returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingAValidNodeIDAndTypeAndThatcherHarlemannCoefficient_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenASetContainingASingleBoundaryLocationWithThisNodeIDAndTypeAndThatcherHarlemannCoefficientIsReturned()
        {
            // Given
            const string nodeID = "SomeNodeID";
            const BoundaryType boundaryType = BoundaryType.Level;
            const double thatcherHarlemannCoeff = 4.0;

            var validCategoriesSet = new List<DelftIniCategory>();

            var validCategory = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            validCategory.AddProperty(BoundaryRegion.NodeId.Key, nodeID, BoundaryRegion.NodeId.Description);
            validCategory.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(boundaryType), BoundaryRegion.Type.Description);
            validCategory.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, thatcherHarlemannCoeff, BoundaryRegion.ThatcherHarlemanCoeff.Description);

            validCategoriesSet.Add(validCategory);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(validCategoriesSet, errorMsgs);

            // Then
            Assert.That(outputSet.Count, Is.EqualTo(1));
            var boundaryLocation = outputSet.First();

            Assert.That(boundaryLocation.Name, Is.EqualTo(nodeID));
            Assert.That(boundaryLocation.BoundaryType, Is.EqualTo(boundaryType));
            Assert.That(boundaryLocation.ThatcherHarlemannCoefficient, Is.EqualTo(thatcherHarlemannCoeff));

            Assert.That(errorMsgs.Count, Is.EqualTo(0));
        }


        /// <summary>
        /// GIVEN A set of valid DelftIniCategories
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A set of corresponding BoundaryLocations should be returned
        /// </summary>
        [Test]
        public void GivenASetOfValidDelftIniCategories_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenASetOfCorrespondingBoundaryLocationsShouldBeReturned()
        {
            var validCategoriesSet = new List<DelftIniCategory>();

            for (int i = 0; i < 17; i++)
            {
                var validCategory = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
                validCategory.AddProperty(BoundaryRegion.NodeId.Key, $"thisIsAName{i}", BoundaryRegion.NodeId.Description);
                validCategory.AddProperty(BoundaryRegion.Type.Key, (i & 1) + 1, BoundaryRegion.Type.Description);

                // Add a thatcher harlemann to every third element
                if ((i + 1) % 3 == 0)
                {
                    validCategory.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, i * 50.0 + 10.0,
                        BoundaryRegion.ThatcherHarlemanCoeff.Description);
                }
                validCategoriesSet.Add(validCategory);
            }

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(validCategoriesSet, errorMsgs);

            // Then
            Assert.That(outputSet.Count, Is.EqualTo(validCategoriesSet.Count));

            foreach (var category in validCategoriesSet)
            {
                var correspondingBoundaryLocations = outputSet.Where(e => e.Name == category.GetPropertyValue(BoundaryRegion.NodeId.Key)).ToList();
                Assert.That(correspondingBoundaryLocations.Count(), Is.EqualTo(1));

                var correspondingBoundaryLocation = correspondingBoundaryLocations.First();
                Assert.That(correspondingBoundaryLocation.BoundaryType,
                    Is.EqualTo(boundaryTypeFromVal(category.ReadProperty<int>(BoundaryRegion.Type.Key))));

                var thCoeff = category.ReadProperty<double>(BoundaryRegion.ThatcherHarlemanCoeff.Key, isOptional:true);
                Assert.That(correspondingBoundaryLocation.ThatcherHarlemannCoefficient, Is.EqualTo(thCoeff));
            }

            Assert.That(errorMsgs.Count, Is.EqualTo(0));
        }


        // Exception Handling Flow
        /// <summary>
        /// GIVEN A Null set of DelftIniCategories
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenANullSetOfDelftIniCategories_WhenBoundaryLocationConverterConvertIsCalledWithThisSet__ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            // Given
            const IList<DelftIniCategory> nullSet = null;
            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(nullSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            const string expectedErrorMsg = "Unable to parse empty set of boundary locations.";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }   


        /// <summary>
        /// GIVEN An empty set of DelftIniCategories
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenAnEmptySetOfDelftIniCategories_WhenBoundaryLocationConverterConvertIsCalledWithThisSet__ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            // Given
            var emptySet = new List<DelftIniCategory>();
            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(emptySet, errorMsgs);
            
            // Then
            Assert.That(outputSet.Any(), Is.False);

            const string expectedErrorMsg = "Unable to parse empty set of boundary locations.";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN a set of categories containing a single null item
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning is logged
        ///  AND An empty set is returned
        /// </summary>
        [Test]
        public void GivenASetOfCategoriesContainingASingleNullItem_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningIsLoggedAndAnEmptySetIsReturned()
        {
            // Given
            var setWithNull = new List<DelftIniCategory>();
            setWithNull.Add(null);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(setWithNull, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            const string expectedErrorMsg = "Unable to parse null category.";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }


        /// <summary>
        /// GIVEN A set containing a single unknown category
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning is logged
        ///  AND An empty set is returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleUnknownCategory_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningIsLoggedAndAnEmptySetIsReturned()
        {
            // Given
            const string header = "SomeInvalidHeader";
            var category = new DelftIniCategory(header);
            var invalidSet = new List<DelftIniCategory>();
            invalidSet.Add(category);
            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {header} at line {category.LineNumber}: Invalid header";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN A Set containing a single DelftIniCategory with a valid header and no data
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryWithAValidHeaderAndNoData_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            // Given
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);
            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Missing data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN A set of DelftIniCategories containing a single category which does not contain a nodeID
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetOfDelftIniCategoriesContainingASingleCategoryWhichDoesNotContainANodeID_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            // Given
            var invalidSet = new List<DelftIniCategory>();

            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, 1, BoundaryRegion.Type.Description);

            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Missing data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }


        /// <summary>
        /// GIVEN A set of DelftIniCategories containing a single category which does not contain a type
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetOfDelftIniCategoriesContainingASingleCategoryWhichDoesNotContainAType_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            // Given
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID", BoundaryRegion.NodeId.Description);
            invalidSet.Add(categoryWithInvalidValues);
            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Missing data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }


        /// <summary>
        /// GIVEN A set of DelftIniCategories containing a single category which contains an invalid type
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetOfDelftIniCategoriesContainingASingleCategoryWhichContainsAnInvalidType_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            // Given
            const int someInvalidTypeValue = 777;

            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, someInvalidTypeValue, BoundaryRegion.Type.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Invalid data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        // ----------------------
        // TODO FROM HERE
        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one type and one unknown property
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneTypeAndOneUnknownProperty_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            const BoundaryType someType = BoundaryType.Level;
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty("nodeID", "SomeNodeID", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(someType), BoundaryRegion.Type.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Missing data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one type and two nodeIDs
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneTypeAndTwoNodeIDs_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            const BoundaryType someType = BoundaryType.Level;
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID1", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID2", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(someType), BoundaryRegion.Type.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Multiple defined data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one nodeID and one unknown property
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneNodeIDAndOneUnknownProperty_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            const BoundaryType someType = BoundaryType.Level;
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty("typo", valFromBoundaryType(someType), BoundaryRegion.Type.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Missing data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one nodeID and two types
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneNodeIDAndTwoTypes_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            const BoundaryType someType = BoundaryType.Level;
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(someType), BoundaryRegion.Type.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(someType), BoundaryRegion.Type.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Multiple defined data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));

        }

        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one nodeID and type with a character
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneNodeIDAndTypeWithACharacter_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            const BoundaryType someType = BoundaryType.Level;
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, "definitelyNotAnInt", BoundaryRegion.Type.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Invalid data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one nodeID and one type and two Thatcher Harlemann coefficients
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneNodeIDAndOneTypeAndTwoThatcherHarlemannCoefficients_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            const BoundaryType someType = BoundaryType.Level;
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(someType), BoundaryRegion.Type.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, 1.0, BoundaryRegion.ThatcherHarlemanCoeff.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, 1.0, BoundaryRegion.ThatcherHarlemanCoeff.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Multiple defined data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one nodeID and one type and one Thatcher Harlemann Coefficient with a character
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneNodeIDAndOneTypeAndOneThatcherHarlemannCoefficientWithACharacter_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            const BoundaryType someType = BoundaryType.Level;
            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, "SomeNodeID", BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(someType), BoundaryRegion.Type.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, "DefinitelyNotADouble", BoundaryRegion.ThatcherHarlemanCoeff.Description);
            invalidSet.Add(categoryWithInvalidValues);
            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Any(), Is.False);

            var expectedErrorMsg = $"Could not parse boundary location category: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Invalid data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN a set containing a single DelftIniCategory containing one nodeID and one type and one Thatcher Harlemann coefficient and one unknown property
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An set containing a BoundaryLocation corresponding with the category without unknown data should be returned
        /// </summary>
        [Test]
        public void GivenASetContainingASingleDelftIniCategoryContainingOneNodeIDAndOneTypeAndOneThatcherHarlemannCoefficientAndOneUnknownProperty_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnSetContainingABoundaryLocationCorrespondingWithTheCategoryWithoutUnknownDataShouldBeReturned()
        {
            // Given
            const string nodeID = "SomeNodeID";
            const BoundaryType boundaryType = BoundaryType.Level;
            const double thatcherHarlemannCoeff = 4.0;

            var categoriesSet = new List<DelftIniCategory>();

            var category = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            category.AddProperty(BoundaryRegion.NodeId.Key, nodeID, BoundaryRegion.NodeId.Description);
            category.AddProperty(BoundaryRegion.Type.Key, valFromBoundaryType(boundaryType), BoundaryRegion.Type.Description);
            category.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, thatcherHarlemannCoeff, BoundaryRegion.ThatcherHarlemanCoeff.Description);
            category.AddProperty("SomeUnknownProperty", "With a value", "And a description.");

            categoriesSet.Add(category);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(categoriesSet, errorMsgs);

            // Then
            Assert.That(outputSet.Count, Is.EqualTo(1));
            var boundaryLocation = outputSet.First();

            Assert.That(boundaryLocation.Name, Is.EqualTo(nodeID));
            Assert.That(boundaryLocation.BoundaryType, Is.EqualTo(boundaryType));
            Assert.That(boundaryLocation.ThatcherHarlemannCoefficient, Is.EqualTo(thatcherHarlemannCoeff));

            var expectedErrorMsg = $"Location category contains additional data: {category.Name} at line {category.LineNumber}: Unknown data";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }

        /// <summary>
        /// GIVEN A set of DelftIniCategories containing a single category which contains an invalid Thatcher-Harlemann coefficient
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A warning should be logged
        ///  AND An empty set should be returned
        /// </summary>
        [Test]
        public void GivenASetOfDelftIniCategoriesContainingASingleCategoryWhichContainsAnInvalidThatcherHarlemannCoefficient_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenAWarningShouldBeLoggedAndAnEmptySetShouldBeReturned()
        {
            // Given
            const double someInvalidThatcherHarlemannValue = -20.0;
            const string nodeID = "SomeNodeID";
            const int region = 1;

            var invalidSet = new List<DelftIniCategory>();
            var categoryWithInvalidValues = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.NodeId.Key, nodeID, BoundaryRegion.NodeId.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.Type.Key, region, BoundaryRegion.Type.Description);
            categoryWithInvalidValues.AddProperty(BoundaryRegion.ThatcherHarlemanCoeff.Key, someInvalidThatcherHarlemannValue, BoundaryRegion.ThatcherHarlemanCoeff.Description);
            invalidSet.Add(categoryWithInvalidValues);

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(invalidSet, errorMsgs);

            // Then
            Assert.That(outputSet.Count(), Is.EqualTo(1));
            var locationBoundary = outputSet.First();
            Assert.That(locationBoundary.Name, Is.EqualTo(nodeID));
            Assert.That((int) locationBoundary.BoundaryType, Is.EqualTo(region));
            Assert.That(locationBoundary.ThatcherHarlemannCoefficient, Is.EqualTo(0.0));

            var expectedErrorMsg = $"Could not parse Thatcher Harlemann Coefficient: {categoryWithInvalidValues.Name} at line {categoryWithInvalidValues.LineNumber}: Defaulting to zero";
            Assert.That(errorMsgs.Count, Is.EqualTo(1));
            Assert.That(errorMsgs.First(), Is.EqualTo(expectedErrorMsg));
        }


        // TODO dependent on PO
        /// <summary>
        /// GIVEN A set of DelftIniCategories containing both valid and invalid categories
        /// WHEN BoundaryLocationConverter Convert is called with this set
        /// THEN A set containing the valid categories should be returned
        ///  AND Warnings for each invalid category should be logged
        /// </summary>
        [Test]
        public void GivenASetOfDelftIniCategoriesContainingBothValidAndInvalidCategories_WhenBoundaryLocationConverterConvertIsCalledWithThisSet_ThenASetContainingTheValidCategoriesShouldBeReturnedAndWarningsForEachInvalidCategoryShouldBeLogged()
        {
            // Given
            var setWithValidAndInvalidCategories = new List<DelftIniCategory>();
            var validCategories = new List<DelftIniCategory>();
            var invalidCategories = new List<DelftIniCategory>();
            var unknownCategories = new List<DelftIniCategory>();

            const int nItems = 5;
            for (int i = 0; i < nItems; i++)
            {
                // Invalid Categories
                var categoryInvalid = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
                categoryInvalid.AddProperty(BoundaryRegion.NodeId.Key, $"SomeId{i}", BoundaryRegion.NodeId.Description);
                setWithValidAndInvalidCategories.Add(categoryInvalid);

                // Valid Categories
                var categoryValid = new DelftIniCategory(BoundaryRegion.BoundaryHeader);
                categoryValid.AddProperty(BoundaryRegion.NodeId.Key, $"SomeId{i + nItems}", BoundaryRegion.NodeId.Description);
                categoryValid.AddProperty(BoundaryRegion.Type.Key, 1, BoundaryRegion.Type.Description);
                setWithValidAndInvalidCategories.Add(categoryValid);
                validCategories.Add(categoryValid);

                // Invalid Categories
                var unknownCategory = new DelftIniCategory($"SomeUnknownHeader{i}");
                unknownCategory.LineNumber = i * 10;
                setWithValidAndInvalidCategories.Add(unknownCategory);

            }

            var errorMsgs = new List<string>();

            // When
            var outputSet = BoundaryLocationConverter.Convert(setWithValidAndInvalidCategories, errorMsgs).ToList();

            // Then
            // verify output set
            Assert.That(outputSet.Count, Is.EqualTo(validCategories.Count));

            foreach (var category in validCategories)
            {
                Assert.That(outputSet.Exists(e => 
                    (e.Name == category.GetPropertyValue(BoundaryRegion.NodeId.Key) &&
                    ((int) e.BoundaryType == int.Parse(category.GetPropertyValue(BoundaryRegion.Type.Key))
                     ))));
            }

            Assert.That(errorMsgs.Count, Is.EqualTo(10));

            foreach (var category in invalidCategories)
            {
                var expectedErrorMsg = $"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Missing Data";
                Assert.That(errorMsgs.Contains(expectedErrorMsg));
            }

            foreach (var category in unknownCategories)
            {
                var expectedErrorMsg = $"Could not parse boundary location category: {category.Name} at line {category.LineNumber}: Invalid Header";
                Assert.That(errorMsgs.Contains(expectedErrorMsg));
            }
        }

        private static int valFromBoundaryType(BoundaryType type)
        {
            if (type == BoundaryType.Level)
                return 1;
            if (type == BoundaryType.Discharge)
                return 2;
            return -1;
        }

        private static BoundaryType boundaryTypeFromVal(int val)
        {
            if (val == 1)
                return BoundaryType.Level;
            if (val == 2)
                return BoundaryType.Discharge;
            return BoundaryType.None;
        }
    }
}
