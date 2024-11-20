using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro.Area.Objects;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.FMSuite.Common.IO.Files;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    [Category(TestCategory.DataAccess)]
    public class PlizFileTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void Write_BridgePillarFeature_WithZCoordinateAndColumns()
        {
            var feature = new BridgePillar()
            {
                Name = "BridgePillarTest",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(0, 160),
                        new Coordinate(40, 80),
                        new Coordinate(80, 40),
                        new Coordinate(160, 0)
                    }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            var column3 = new List<double>()
            {
                1.0,
                2.5,
                5.0,
                10.0
            };
            var column4 = new List<double>()
            {
                10.0,
                5.0,
                2.5,
                1.0
            };

            feature.Attributes.Add("Column3", column3);
            feature.Attributes.Add("Column4", column4);

            var file = new PlizFile<BridgePillar>();
            var pliFilePath = @"BridgePillarTest\feature.pliz";
            file.Write(pliFilePath, new[]
            {
                feature
            });

            string[] textLines = File.ReadAllLines(pliFilePath);

            var expectedLines = new List<string>()
            {
                "BridgePillarTest",
                "    4    4",
                "0.000000000000000E+000  1.600000000000000E+002  1.000000000000000E+000  1.000000000000000E+001",
                "4.000000000000000E+001  8.000000000000000E+001  2.500000000000000E+000  5.000000000000000E+000",
                "8.000000000000000E+001  4.000000000000000E+001  5.000000000000000E+000  2.500000000000000E+000",
                "1.600000000000000E+002  0.000000000000000E+000  1.000000000000000E+001  1.000000000000000E+000"
            };

            var idx = 0;
            foreach (string textLine in textLines)
            {
                string expectedLine = expectedLines[idx];
                Assert.AreEqual(expectedLine, textLine);
                idx++;
            }
        }

        [TestCaseSource(nameof(GetFeature2DReadFunctions))]
        public void WriteReadFeatureWithZCoordinateAndColumns(Func<string, IList<Feature2D>> readFunction)
        {
            var feature = new Feature2D
            {
                Name = "line",
                Geometry =
                    new LineString(new[]
                    {
                        new Coordinate(-12345, 54321, 0),
                        new Coordinate(-23451, 43215, 10.0),
                        new Coordinate(-34512, 32154, 20.0),
                        new Coordinate(-45123, 21453, 30.0)
                    }),
                Attributes = new DictionaryFeatureAttributeCollection()
            };
            feature.Attributes.Add("Column3", new List<double>(new[]
            {
                0.5,
                1.4,
                2.3,
                3.1
            }));

            new PlizFile<Feature2D>().Write("feature.pliz", new[]
            {
                feature
            });

            IList<Feature2D> features = readFunction.Invoke("feature.pliz");

            Assert.AreEqual(1, features.Count);

            Feature2D featureCopy = features[0];

            Assert.AreEqual(feature.Name, featureCopy.Name);
            Assert.AreEqual(feature.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.That(featureCopy.Geometry.Coordinates.Select(c => c.Z).ToArray(), Is.EqualTo(new[]
            {
                0.0,
                10.0,
                20.0,
                30.0
            }));
            Assert.That(featureCopy.Attributes.Count, Is.EqualTo(1));
            Assert.AreEqual(feature.Attributes["Column3"], featureCopy.Attributes["Column3"]);
        }

        [TestCaseSource(nameof(GetFixedWeirReadFunctions))]
        public void GivenSimpleFixedWeirFile_WhenReading_ThenFixedWeirLevelValuesAreStored(Func<string, IList<FixedWeir>> readFunction)
        {
            var testFileName = "OneSimpleFixedWeir_fxw.pliz";
            string testDir = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("HydroAreaCollection"));
            string filePath = Path.Combine(testDir, testFileName);

            IList<FixedWeir> fixedWeirs = readFunction.Invoke(filePath);

            Assert.That(fixedWeirs.Count, Is.EqualTo(1));

            FixedWeir fixedWeir = fixedWeirs.FirstOrDefault();
            Assert.That(fixedWeir.Attributes.Count, Is.EqualTo(7));

            List<double> crestLevelsAttributes = (fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[0]] as
                                                      GeometryPointsSyncedList<double>).ToList();

            List<double> GroundLevelsLeftAttributes = (fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[1]] as
                                                           GeometryPointsSyncedList<double>).ToList();

            List<double> GroundLevelsRightAttributes = (fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[2]] as
                                                            GeometryPointsSyncedList<double>).ToList();

            Assert.That(crestLevelsAttributes[0], Is.EqualTo(10.96));
            Assert.That(crestLevelsAttributes[1], Is.EqualTo(10.89));
            Assert.That(GroundLevelsLeftAttributes[0], Is.EqualTo(3.5));
            Assert.That(GroundLevelsLeftAttributes[1], Is.EqualTo(3.0));
            Assert.That(GroundLevelsRightAttributes[0], Is.EqualTo(3.2));
            Assert.That(GroundLevelsRightAttributes[1], Is.EqualTo(3.3));
        }

        [TestCaseSource(nameof(GetFeature2DReadFunctions))]
        public void Read_ThenCorrectFeatureIsCreated(Func<string, IList<Feature2D>> readFunction)
        {
            // Setup
            var fileContent = new[]
            {
                "feature_name",
                "3 3",
                "10 20 2",
                "30 40 3",
                "50 60 4"
            };

            var expectedCoordinates = new[]
            {
                new Coordinate(10, 20, 2),
                new Coordinate(30, 40, 3),
                new Coordinate(50, 60, 4)
            };

            using (var tempDirectory = new TemporaryDirectory())
            {
                string plizFilePath = Path.Combine(tempDirectory.Path, "feature.pliz");
                File.WriteAllLines(plizFilePath, fileContent);

                // Call
                IList<Feature2D> features = readFunction.Invoke(plizFilePath);

                // Assert
                Assert.That(features.Count, Is.EqualTo(1),
                            "Only one feature was in the file.");

                Feature2D feature = features[0];
                Assert.That(feature.Name, Is.EqualTo("feature_name"),
                            "The name of the read feature is incorrect.");

                IGeometry geometry = feature.Geometry;
                Assert.That(geometry, Is.TypeOf<LineString>(),
                            $"Geometry type should be a {nameof(LineString)}");

                Coordinate[] resultedCoordinates = geometry.Coordinates;
                Assert.That(resultedCoordinates.Length, Is.EqualTo(expectedCoordinates.Length),
                            "Geometry has incorrect number of coordinates.");

                resultedCoordinates.ForEach((c, i) =>
                {
                    Coordinate expectedCoordinate = expectedCoordinates[i];
                    Assert.That(c.Equals3D(expectedCoordinate),
                                $"Expected: {expectedCoordinate}, but was: {c}");
                });
            }
        }

        private static IEnumerable<Func<string, IList<FixedWeir>>> GetFixedWeirReadFunctions()
        {
            return GetReadFunctions<FixedWeir>();
        }

        private static IEnumerable<Func<string, IList<Feature2D>>> GetFeature2DReadFunctions()
        {
            return GetReadFunctions<Feature2D>();
        }

        private static IEnumerable<Func<string, IList<TFeat>>> GetReadFunctions<TFeat>() where TFeat : IFeature, INameable, new()
        {
            yield return path => new PlizFile<TFeat>().Read(path);
            yield return path => new PlizFile<TFeat>().Read(path, null);
            yield return path => new PlizFile<TFeat>().Read(path, (n, s, t) => {});
        }
    }
}