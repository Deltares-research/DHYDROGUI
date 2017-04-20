using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class PliFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadFeatureWithColumnsAndLocationNames()
        {
            var feature = new Feature2D
                {
                    Name = "line",
                    Geometry =
                        new LineString(new []
                            {
                                new Coordinate(-12345, 54321), new Coordinate(-23451, 43215), new Coordinate(-34512, 32154),
                                new Coordinate(-45123,21453)
                            }),
                    Attributes = new DictionaryFeatureAttributeCollection()
                };
            feature.Attributes.Add("Column3", new List<double>(new[] {0.5, 1.4, 2.3, 3.1}));
            feature.Attributes.Add("Locations", new List<string>(new[] {"loc_a", "loc_b", "loc_c", "loc_d"}));

            var file = new PliFile<Feature2D>();
            file.Write("feature.pli", new[] {feature});

            var features = file.Read("feature.pli");

            Assert.AreEqual(1, features.Count());

            var featureCopy = features[0];

            Assert.AreEqual(feature.Name, featureCopy.Name);
            Assert.AreEqual(feature.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.AreEqual(feature.Attributes["Column3"], featureCopy.Attributes["Column3"]);
            Assert.AreEqual(feature.Attributes["Locations"], featureCopy.Attributes["Locations"]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteReadFeaturesWithColumnsAndLocationNames()
        {
            var feature1 = new Feature2D
            {
                Name = "line_1",
                Geometry =
                    new LineString(new []
                            {
                                new Coordinate(-12345, 54321), new Coordinate(-23451, 43215), new Coordinate(-34512, 32154),
                                new Coordinate(-45123,21453)
                            }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            feature1.Attributes.Add("Column3", new List<double>(new[] { 0.5, 1.4, 2.3, 3.1 }));
            feature1.Attributes.Add("Locations", new List<string>(new[] { "loc_a", "loc_b", "loc_c", "loc_d" }));

            var feature2 = new Feature2D
            {
                Name = "line_2",
                Geometry =
                    new LineString(new []
                            {
                                new Coordinate(-54321, 12345), new Coordinate(-43215, 23451), new Coordinate(-32154, 34512),
                            }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            feature2.Attributes.Add("Column3", new List<double>(new[] { -0.5, -1.4, -2.3 }));
            feature2.Attributes.Add("Column4", new List<double>(new[] { -0.6, -1.5, -2.4 }));
            feature2.Attributes.Add("Column5", new List<double>(new[] { -0.7, -1.6, -2.5 }));
            feature2.Attributes.Add("Locations", new List<string>(new[] { "loc_a*", "loc_b*", "loc_c*" }));

            var file = new PliFile<Feature2D>();
            file.Write("features.pli", new[] { feature1, feature2 });

            var features = file.Read("features.pli");

            Assert.AreEqual(2, features.Count());

            var featureCopy = features[0];

            Assert.AreEqual(feature1.Name, featureCopy.Name);
            Assert.AreEqual(feature1.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.AreEqual(feature1.Attributes["Column3"], featureCopy.Attributes["Column3"]);
            Assert.AreEqual(feature1.Attributes["Locations"], featureCopy.Attributes["Locations"]);
            Assert.AreEqual(feature1.Name, featureCopy.Name);

            featureCopy = features[1];

            Assert.AreEqual(feature2.Name, featureCopy.Name);
            Assert.AreEqual(feature2.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.AreEqual(feature2.Attributes["Column3"], featureCopy.Attributes["Column3"]);
            Assert.AreEqual(feature2.Attributes["Column4"], featureCopy.Attributes["Column4"]);
            Assert.AreEqual(feature2.Attributes["Column5"], featureCopy.Attributes["Column5"]);
            Assert.AreEqual(feature2.Attributes["Locations"], featureCopy.Attributes["Locations"]);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void WriteManyFeaturesWithColumnsAndLocationNames()
        {
            const int featureCount = 10000;
            const int pointsPerFeature = 100;
            const string fileName = "many_features.pli";

            var featureCollection = new List<Feature2D>();
            for (int i = 0; i < featureCount; ++i)
            {
                var feature = new Feature2D {Name = "feature_" + i};
                var elevation = Math.Tan(2*i*Math.PI/pointsPerFeature);
                var points = Enumerable.Range(0, pointsPerFeature).Select(t => new Coordinate(t, elevation*t)).ToList();
                feature.Geometry = new LineString(points.ToArray());
                feature.Attributes = new DictionaryFeatureAttributeCollection
                    {
                        {"Column3", points.Select(p => p.Y/(1 + p.X*p.X)).ToList()},
                        {"Column4", points.Select(p => p.X/(1 + p.Y*p.Y)).ToList()},
                        {"Locations", Enumerable.Range(0, pointsPerFeature).Select(p => "pt_" + p).ToList()}
                    };
                featureCollection.Add(feature);
            }
            var file = new PliFile<Feature2D>();
            try
            {
                TestHelper.AssertIsFasterThan(16000, () => file.Write(fileName, featureCollection));
            }
            finally
            {
                File.Delete(fileName);
            }
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Performance)]
        [Category(TestCategory.Slow)]
        public void ReadManyFeaturesWithColumnsAndLocationNames()
        {
            const int featureCount = 10000;
            const int pointsPerFeature = 100;
            const string fileName = "many_features.pli";

            var featureCollection = new List<Feature2D>();
            for (int i = 0; i < featureCount; ++i)
            {
                var feature = new Feature2D { Name = "feature_" + i };
                var elevation = Math.Tan(2 * i * Math.PI / pointsPerFeature);
                var points = Enumerable.Range(0, pointsPerFeature).Select(t => new Coordinate(t, elevation * t)).ToList();
                feature.Geometry = new LineString(points.ToArray());
                feature.Attributes = new DictionaryFeatureAttributeCollection
                    {
                        {"Column3", points.Select(p => p.Y/(1 + p.X*p.X)).ToList()},
                        {"Column4", points.Select(p => p.X/(1 + p.Y*p.Y)).ToList()},
                        {"Locations", Enumerable.Range(0, pointsPerFeature).Select(p => "pt_" + p).ToList()}
                    };
                featureCollection.Add(feature);
            }
            var file = new PliFile<Feature2D>();
            try
            {
                file.Write(fileName, featureCollection);
                TestHelper.AssertIsFasterThan(20000, () => file.Read(fileName));
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
