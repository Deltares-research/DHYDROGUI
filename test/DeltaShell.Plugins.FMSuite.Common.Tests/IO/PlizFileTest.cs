using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Geometries;
using NUnit.Framework;
using FixedWeir = DelftTools.Hydro.Structures.FixedWeir;

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

            Assert.AreEqual(1, features.Count);

            var featureCopy = features[0];

            Assert.AreEqual(feature.Name, featureCopy.Name);
            Assert.AreEqual(feature.Geometry.Coordinates, featureCopy.Geometry.Coordinates);
            Assert.That(featureCopy.Geometry.Coordinates.Select(c => c.Z).ToArray(), Is.EqualTo(new[] {0.0, 10.0, 20.0, 30.0}));
            Assert.That(featureCopy.Attributes.Count, Is.EqualTo(1));
            Assert.AreEqual(feature.Attributes["Column3"], featureCopy.Attributes["Column3"]);
        }

        [Test]
        public void GivenSimpleFixedWeirFile_WhenReading_ThenFixedWeirLevelValuesAreStored()
        {
            var testFileName = "OneSimpleFixedWeir_fxw.pliz";
            var testDir = TestHelper.CreateLocalCopy(TestHelper.GetTestFilePath("HydroAreaCollection"));
            var filePath = Path.Combine(testDir, testFileName);

            var fileReader = new PlizFile<FixedWeir>();
            var fixedWeirs = fileReader.Read(filePath);
            Assert.That(fixedWeirs.Count, Is.EqualTo(1));

            var fixedWeir = fixedWeirs.FirstOrDefault();
            Assert.That(fixedWeir.Attributes.Count, Is.EqualTo(7));
           
           var crestLevelsAttributes = (fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[0]] as
                    GeometryPointsSyncedList<double>).ToList();

            var GroundLevelsLeftAttributes = (fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[1]] as
                GeometryPointsSyncedList<double>).ToList();

            var GroundLevelsRightAttributes = (fixedWeir.Attributes[PliFile<FixedWeir>.NumericColumnAttributesKeys[2]] as
                GeometryPointsSyncedList<double>).ToList();

            Assert.That(crestLevelsAttributes[0], Is.EqualTo(10.96));
            Assert.That(crestLevelsAttributes[1], Is.EqualTo(10.89));
            Assert.That(GroundLevelsLeftAttributes[0], Is.EqualTo(3.5));
            Assert.That(GroundLevelsLeftAttributes[1], Is.EqualTo(3.0));
            Assert.That(GroundLevelsRightAttributes[0], Is.EqualTo(3.2));
            Assert.That(GroundLevelsRightAttributes[1], Is.EqualTo(3.3));
        }
    }
}