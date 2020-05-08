using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionXYZTest
    {
        [Test]
        public void EmptyProfiles()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                                              {})
            };

            IEnumerable<Coordinate> profile = crossSection.Profile;
            IEnumerable<Coordinate> flowProfile = crossSection.FlowProfile;

            Assert.AreEqual(0, profile.Count());
            Assert.AreEqual(0, flowProfile.Count());
        }

        [Test]
        public void SetGeometry()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(2, 2, -2),
                    new Coordinate(4, 2, -2),
                    new Coordinate(6, 0, 0)
                })
            };

            double diag = 2.0 * Math.Sqrt(2);

            double[] expectedProfileY = new[]
            {
                0.0,
                diag,
                diag + 2,
                (2 * diag) + 2
            };
            double[] expectedProfileZ = new[]
            {
                0.0,
                -2.0,
                -2.0,
                0.0
            };

            List<double> profileY = crossSection.Profile.Select(p => p.X).ToList();
            List<double> profileZ = crossSection.Profile.Select(p => p.Y).ToList();

            for (var i = 0; i < expectedProfileY.Length; i++)
            {
                Assert.AreEqual(expectedProfileY[i], profileY[i], 0.001); //2d profile
                Assert.AreEqual(expectedProfileZ[i], profileZ[i], 0.001);
            }
        }

        [Test]
        public void SetDifferentGeometries()
        {
            var crossSection = new CrossSectionDefinitionXYZ();

            var coordinates = new List<Coordinate>
            {
                new Coordinate(0, 0, 0),
                new Coordinate(2, 2, -2),
                new Coordinate(4, 2, -2),
                new Coordinate(6, 0, 0)
            };

            var geometry1 = new LineString(coordinates.ToArray());
            coordinates.Insert(2, new Coordinate(3, 2, -2));
            var geometry2 = new LineString(coordinates.Select(c => c.Clone() as Coordinate).ToArray()); //full clone
            coordinates.RemoveAt(1);
            var geometry3 = new LineString(coordinates.Select(c => c.Clone() as Coordinate).ToArray());
            coordinates = coordinates.Select(c => new Coordinate(c.X + 10, c.Y, c.Z)).OfType<Coordinate>().ToList();
            var geometry4 = new LineString(coordinates.Select(c => c.Clone() as Coordinate).ToArray());

            crossSection.Geometry = geometry1;
            const int count = 4; //define some storage
            for (var i = 0; i < count; i++)
            {
                crossSection.XYZDataTable[i].DeltaZStorage = i;
            }

            geometry1[2].X = 5; //move a point
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(crossSection.XYZDataTable[i].DeltaZStorage, i); //expect no changes
            }

            var j = 0;
            crossSection.Geometry = geometry2; //add a point
            for (var i = 0; i < count + 1; i++)
            {
                if (i != 2)
                {
                    Assert.AreEqual(crossSection.XYZDataTable[i].DeltaZStorage, j);
                    j++;
                }
            }

            crossSection.Geometry = geometry3; //remove a point
            double[] expected =
            {
                0.0,
                0.0,
                2.0,
                3.0
            };
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(crossSection.XYZDataTable[i].DeltaZStorage, expected[i]);
            }

            crossSection.Geometry = geometry4; //move all points
            for (var i = 0; i < count; i++)
            {
                Assert.AreEqual(crossSection.XYZDataTable[i].DeltaZStorage, expected[i]);
            }
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException), ExpectedMessage = "Cannot add / delete rows from XYZ Cross Section")]
        public void AddToDataTableFails()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(2, 2, -2),
                    new Coordinate(4, 2, -2),
                    new Coordinate(6, 0, 0)
                })
            };

            crossSection.XYZDataTable.AddCrossSectionXYZRow(3, 0, 0);
        }

        [Test]
        public void TableMatchesGeometry()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(2, 2, -2),
                    new Coordinate(4, 2, -2),
                    new Coordinate(6, 0, 0)
                })
            };

            Assert.AreEqual(crossSection.Geometry.Coordinates.Length, crossSection.XYZDataTable.Rows.Count);

            double diag = 2.0 * Math.Sqrt(2);

            double[] expectedProfileY = new[]
            {
                0.0,
                diag,
                diag + 2,
                (2 * diag) + 2
            };
            double[] expectedProfileZ = new[]
            {
                0.0,
                -2.0,
                -2.0,
                0.0
            };
            var expectedStorageZ = new[]
            {
                0.0,
                0.0,
                0.0,
                0.0
            };

            LightBindingList<CrossSectionDataSet.CrossSectionXYZRow> rows = crossSection.XYZDataTable.Rows;

            for (var i = 0; i < expectedProfileY.Length; i++)
            {
                CrossSectionDataSet.CrossSectionXYZRow xyzRow = rows[i];

                Assert.AreEqual(expectedProfileY[i], xyzRow.Yq, 0.001); //2d profile
                Assert.AreEqual(expectedProfileZ[i], xyzRow.Z, 0.001);
                Assert.AreEqual(expectedStorageZ[i], xyzRow.DeltaZStorage, 0.001);
            }
        }

        [Test]
        public void ChangingGeometryChangesTable()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(2, 2, -2),
                    new Coordinate(4, 2, -2),
                    new Coordinate(6, 0, 0)
                })
            };

            crossSection.Geometry.Coordinates[1].Z = -1;
            crossSection.Geometry = crossSection.Geometry;

            Assert.AreEqual(-1, crossSection.XYZDataTable[1].Z);
        }

        [Test]
        public void SetReferenceLevelGeometry()
        {
            var crossSection = new CrossSectionDefinitionXYZ();
            var coordinates = new List<Coordinate>
            {
                new Coordinate(0, 0, 0),
                new Coordinate(10, 0, 0),
                new Coordinate(30, 0, 0),
                new Coordinate(40, 0, 0)
            };
            crossSection.Geometry = new LineString(coordinates.ToArray());

            Assert.AreEqual(0.0, crossSection.LowestPoint, 1.0e-6);
            Assert.AreEqual(0.0, crossSection.HighestPoint, 1.0e-6);

            crossSection.ShiftLevel(111);

            Assert.AreEqual(111.0, crossSection.Geometry.Coordinates[0].Z, 1.0e-6);
            Assert.AreEqual(111.0, crossSection.Geometry.Coordinates[0].Z, 1.0e-6);

            Assert.AreEqual(111.0, crossSection.LowestPoint, 1.0e-6);
            Assert.AreEqual(111.0, crossSection.HighestPoint, 1.0e-6);
        }

        [Test]
        public void Clone()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(2, 2, -2),
                    new Coordinate(4, 2, -2),
                    new Coordinate(6, 0, 0)
                })
            };
            crossSection.XYZDataTable[0].DeltaZStorage = 2;

            var clone = (CrossSectionDefinitionXYZ) crossSection.Clone();

            Assert.AreEqual(crossSection.Geometry, clone.Geometry);
            Assert.AreEqual(typeof(FastXYZDataTable), clone.XYZDataTable.GetType());
            Assert.AreEqual(2, clone.XYZDataTable[0].DeltaZStorage);
        }

        [Test]
        public void CopyFrom()
        {
            var source = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(2, 0, -2),
                    new Coordinate(4, 0, 0)
                })
            };
            const int deltaZStorage = 1;
            source.XYZDataTable[1].DeltaZStorage = deltaZStorage;

            var target = new CrossSectionDefinitionXYZ();

            //action! 
            target.CopyFrom(source);

            //for now we just expect the same geometry
            Assert.AreEqual(source.Geometry, target.Geometry);
            Assert.AreEqual(deltaZStorage, target.XYZDataTable[1].DeltaZStorage);
            Assert.AreEqual(typeof(FastXYZDataTable), target.XYZDataTable.GetType());
        }

        [Test]
        public void TestValidateCellValue()
        {
            var crossSection = new CrossSectionDefinitionXYZ
            {
                Geometry = new LineString(new Coordinate[]
                {
                    new Coordinate(0, 0, 0),
                    new Coordinate(2, 2, -2),
                    new Coordinate(4, 2, -2),
                    new Coordinate(6, 0, 0)
                })
            };

            // Changing Y' for first row:
            Utils.Tuple<string, bool> validationResult = crossSection.ValidateCellValue(0, 0, 1.0);
            Assert.IsFalse(validationResult.Second,
                           "User should be unable to edit Y' values of XYZ cross-section");
            Assert.AreEqual("Cannot edit Y' of XYZ cross-sections", validationResult.First);

            // Changing Z for first row:
            validationResult = crossSection.ValidateCellValue(0, 1, 1.0);
            Assert.IsTrue(validationResult.Second,
                          "User should be able to edit Z values of XYZ cross-section");
            Assert.AreEqual("", validationResult.First);

            // Changing Z for first row:
            validationResult = crossSection.ValidateCellValue(0, 2, 0.0);
            Assert.IsTrue(validationResult.Second,
                          "User should be able to edit dZ values of XYZ cross-section");
            Assert.AreEqual("", validationResult.First);

            // NaN is not valid:
            validationResult = crossSection.ValidateCellValue(0, 1, double.NaN);
            Assert.IsFalse(validationResult.Second,
                           "User should be unable to set NaN values");
            Assert.AreEqual("Value must be a number.", validationResult.First);

            // Using strings instead of double:
            validationResult = crossSection.ValidateCellValue(0, 2, "1.0");
            Assert.IsTrue(validationResult.Second,
                          "User should be able to edit dZ values of XYZ cross-section");
            Assert.AreEqual("", validationResult.First);

            // Using string without number:
            validationResult = crossSection.ValidateCellValue(0, 2, "test");
            Assert.IsFalse(validationResult.Second,
                           "Strings must represent a number to be accepted");
            Assert.AreEqual("Value must be a number.", validationResult.First);

            // Changing Z for first row to invalid value:
            validationResult = crossSection.ValidateCellValue(0, 2, -1.0);
            Assert.IsFalse(validationResult.Second,
                           "User should be able to edit dZ values of XYZ cross-section");
            Assert.AreEqual("DeltaZ Storage cannot be negative.", validationResult.First);
        }
    }
}