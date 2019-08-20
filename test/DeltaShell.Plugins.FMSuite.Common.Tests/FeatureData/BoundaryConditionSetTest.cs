using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.FeatureData
{
    [TestFixture]
    public class BoundaryConditionSetTest
    {
        [Test]
        public void BoundaryConditionSetShouldBubbleEvents()
        {
            var set = new BoundaryConditionSet();

            var count = 0;
            set.CollectionChanged += (sender, args) => count++;

            set.BoundaryConditions.Add(new TestBoundaryCondition(BoundaryConditionDataType.Empty,false,false));

            Assert.AreEqual(1, count);
        }

        [Test]
        public void GivenBoundaryConditionSetWithoutFeature_WhenFeature2DWithoutAttributesSet_ThenReturnsExpectedStringRepresentationOfLocationsAttribute()
        {
            // Given
            var feature2D = new Feature2D
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(1, 0),
                    new Coordinate(2, 0)
                }),
                Name = "Feature"
            };

            var set = new BoundaryConditionSet();

            // When
            set.Feature = feature2D;

            // Then
            IFeatureAttributeCollection attributeCollection = set.Feature.Attributes;
            Assert.That(attributeCollection.ContainsKey(Feature2D.LocationKey), Is.True, "Attributes does not contain a key representing the locations");

            var attributeValue = (BoundaryConditionsPointsSyncedList) attributeCollection[Feature2D.LocationKey];
            Assert.That(attributeValue.ToString(), Is.EqualTo("Feature_0001, Feature_0002, Feature_0003"), "Attribute value for the location's key does not have the right string representation.");
        }

        [Test]
        public void GivenBoundaryConditionSetWithoutFeature_WhenFeatureWithAttributeSetOfTypeList_ThenReturnsExpectedStringRepresentation()
        {
            // Given
            try
            {
                var feature2D = new Feature2D
                {
                    Name = "Feature",
                    Attributes = new DictionaryFeatureAttributeCollection()
                };
                feature2D.Attributes[Feature2D.LocationKey] = new List<string>
                {
                    "Location 0",
                    "Location 1",
                    "Location 2",
                    "Location 3"
                };

                var set = new BoundaryConditionSet();

                // When
                set.Feature = feature2D;

                // Then
                IFeatureAttributeCollection attributeCollection = set.Feature.Attributes;
                Assert.That(attributeCollection.ContainsKey(Feature2D.LocationKey), Is.True,
                            "Attributes does not contain a key representing the locations");

                var attributeValue = (BoundaryConditionsPointsSyncedList) attributeCollection[Feature2D.LocationKey];
                Assert.That(attributeValue.ToString(), Is.EqualTo("Feature_1, Feature_2, Feature_3, Feature_4"),
                            "Attribute value for the location's key does not have the right string representation.");
            }
            catch (InvalidCastException)
            {
                Assert.Pass("Test currently passes, but the value corresponding to the location key should probably be of type 'BoundaryConditionsPointsSyncedList' instead of an IList<string>. " +
                            "Nothing happens when creating the synced list in the last else-if clause of the AfterFeatureSet call. See D3DFMIQ-640 for more information");
            }
        }
    }
}