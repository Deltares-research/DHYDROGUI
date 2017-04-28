using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class PlizFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadFeatureWithZCoordinateAndColumns()
        {
            var feature = new Feature2D
            {
                Name = "line",
                Geometry =
                    new LineString(new []
                    {
                        new Coordinate(-12345, 54321, 0), 
                        new Coordinate(-23451, 43215, 10.0),
                        new Coordinate(-34512, 32154, 20.0),
                        new Coordinate(-45123, 21453, 30.0)
                    }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            feature.Attributes.Add("Column3", new List<double>(new[] {0.5, 1.4, 2.3, 3.1}));

            var file = new PlizFile<Feature2D>();
            file.Write("feature.pliz", new[] {feature});

            var features = file.Read("feature.pliz");

            Assert.AreEqual(1, features.Count());

            var featureCopy = features[0];

            Assert.AreEqual(feature.Name, featureCopy.Name);
            Assert.AreEqual(feature.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.AreEqual(featureCopy.Geometry.Coordinates.Select(c => c.Z).ToArray(), new[] {0.0, 10.0, 20.0, 30.0});
            Assert.AreEqual(feature.Attributes["Column3"], featureCopy.Attributes["Column3"]);
        }
    }
}