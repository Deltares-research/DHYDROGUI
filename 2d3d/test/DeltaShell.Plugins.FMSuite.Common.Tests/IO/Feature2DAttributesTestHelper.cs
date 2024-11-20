using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    public static class Feature2DAttributesTestHelper
    {
        public static void CheckStringValuesForColumn(this IFeatureAttributeCollection attributes, string columnId, string firstValue, string secondValue)
        {
            var columnValues = (GeometryPointsSyncedList<string>) attributes[columnId];
            Assert.That(columnValues[0], Is.EqualTo(firstValue));
            Assert.That(columnValues[1], Is.EqualTo(secondValue));
        }

        public static void CheckDoubleValuesForColumn(this IFeatureAttributeCollection attributes, string columnId, double firstValue, double secondValue)
        {
            var columnValues = (GeometryPointsSyncedList<double>) attributes[columnId];
            Assert.That(columnValues[0], Is.EqualTo(firstValue));
            Assert.That(columnValues[1], Is.EqualTo(secondValue));
        }
    }
}