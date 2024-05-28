using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.DataSets;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    public class CrossSectionDefinitionYZTest
    {
        [Test]
        public void EmptyProfiles()
        {
            var crossSection = new CrossSectionDefinitionYZ();

            var profile = crossSection.GetProfile();
            var flowProfile = crossSection.FlowProfile;

            Assert.AreEqual(0, profile.Count());
            Assert.AreEqual(0, flowProfile.Count());
        }

        [Test]
        public void GetGeometryWithNoYZReturnsPoint()
        {
            //this is not a requirement but it seems handy. null might also be reasonable
            var crossSection = new CrossSectionDefinitionYZ();
            var branch = new Branch
                                {
                                    Geometry = new LineString(new []{new Coordinate(0,0),new Coordinate(100,0)})
                                };
            
            var geometry = crossSection.GetGeometry(new CrossSection (new CrossSectionDefinitionXYZ()) {Branch = branch, Chainage = 20.0});
            Assert.AreEqual(new Point(20,0),geometry);
        }

        [Test]
        public void SetReferenceLevelYZTest()
        {
            var crossSection = new CrossSectionDefinitionYZ();
            IEnumerable<Coordinate> coordinates = new[]
                                          {
                                              new Coordinate(0, 0),
                                              new Coordinate(0.01, -10),
                                              new Coordinate(19.99, -10),
                                              new Coordinate(20, 0)
                                          };
            crossSection.YZDataTable.SetWithCoordinates(coordinates);


            Assert.AreEqual(-10.0, crossSection.LowestPoint, 1.0e-6);
            Assert.AreEqual(0.0, crossSection.HighestPoint, 1.0e-6);

            crossSection.ShiftLevel(111);

            var yValues = crossSection.GetProfile().Select(p => p.Y).ToList();
            Assert.AreEqual(111.0, yValues[0], 1.0e-6);
            Assert.AreEqual(101.0, yValues[1], 1.0e-6);
            Assert.AreEqual(101.0, yValues[2], 1.0e-6);
            Assert.AreEqual(111.0, yValues[3], 1.0e-6);

            Assert.AreEqual(101.0, crossSection.LowestPoint, 1.0e-6);
            Assert.AreEqual(111.0, crossSection.HighestPoint, 1.0e-6);
        }

        [Test]
        public void TestProfileMatchesDataTable()
        {
            var crossSection = new CrossSectionDefinitionYZ();
            //simple V profile
            crossSection.YZDataTable.AddCrossSectionYZRow(0, 10);
            crossSection.YZDataTable.AddCrossSectionYZRow(5, 0);
            crossSection.YZDataTable.AddCrossSectionYZRow(10, 10);

            var profileY = new double[] { 0, 5, 10 };
            var profileZ = new double[] { 10, 0, 10 };

            var flowProfileY = new double[] { 0, 5, 10 };
            var flowProfileZ = new double[] { 10, 0, 10 };

            Assert.AreEqual(profileY, crossSection.GetProfile().Select(c => c.X).ToArray());
            Assert.AreEqual(profileZ, crossSection.GetProfile().Select(c => c.Y).ToArray());
            Assert.AreEqual(flowProfileY, crossSection.FlowProfile.Select(c => c.X).ToArray());
            Assert.AreEqual(flowProfileZ, crossSection.FlowProfile.Select(c => c.Y).ToArray());
        }

        [Test]
        public void Clone()
        {
            var crossSectionYZ = new CrossSectionDefinitionYZ();
            
            //simple V profile
            crossSectionYZ.YZDataTable.AddCrossSectionYZRow(0, 10);
            crossSectionYZ.YZDataTable.AddCrossSectionYZRow(5, 0);
            crossSectionYZ.YZDataTable.AddCrossSectionYZRow(10, 10);

            crossSectionYZ.Thalweg = 5.0;

            var clone = (CrossSectionDefinitionYZ)crossSectionYZ.Clone();

            Assert.AreEqual(crossSectionYZ.Thalweg,clone.Thalweg);

            var yzDataTable = crossSectionYZ.YZDataTable;
            var cloneYZTable = clone.YZDataTable;
            Assert.AreEqual(yzDataTable.Rows.Count, cloneYZTable.Rows.Count);
            Assert.AreEqual(5.0, clone.Thalweg);
            for (int i = 0;i<yzDataTable.Count;i++)
            {
                Assert.AreEqual(yzDataTable[i].DeltaZStorage, cloneYZTable[i].DeltaZStorage);
                Assert.AreEqual(yzDataTable[i].Yq, cloneYZTable[i].Yq);
                Assert.AreEqual(yzDataTable[i].Z, cloneYZTable[i].Z);
                Assert.AreNotSame(yzDataTable[i], cloneYZTable[i]);
            }

            //assert a change in the original does not affect the clone
            yzDataTable[0].DeltaZStorage = 6;
            Assert.IsTrue(cloneYZTable.GetType() == typeof(FastYZDataTable));
            Assert.AreNotEqual(6,cloneYZTable[0].DeltaZStorage);
        }

        [Test]
        public void CopyFrom()
        {
            var crossSectionYZ = new CrossSectionDefinitionYZ();

            //simple V profile
            crossSectionYZ.YZDataTable.AddCrossSectionYZRow(0, 10);
            crossSectionYZ.YZDataTable.AddCrossSectionYZRow(5, 0);
            crossSectionYZ.YZDataTable.AddCrossSectionYZRow(10, 10);

            crossSectionYZ.Thalweg = 5.0;

            var copyFrom = new CrossSectionDefinitionYZ
            {
                Thalweg = 1.0
            };

            copyFrom.CopyFrom(crossSectionYZ);

            Assert.AreEqual(crossSectionYZ.Thalweg, copyFrom.Thalweg);

            var yzDataTable = crossSectionYZ.YZDataTable;
            var copyYZTable = copyFrom.YZDataTable;
            Assert.AreEqual(yzDataTable.Rows.Count, copyYZTable.Rows.Count);
            Assert.AreEqual(5.0, crossSectionYZ.Thalweg);
            for (int i = 0; i < yzDataTable.Count; i++)
            {
                Assert.AreEqual(yzDataTable[i].DeltaZStorage, copyYZTable[i].DeltaZStorage);
                Assert.AreEqual(yzDataTable[i].Yq, copyYZTable[i].Yq);
                Assert.AreEqual(yzDataTable[i].Z, copyYZTable[i].Z);
                Assert.AreNotSame(yzDataTable[i], copyYZTable[i]);
            }

            //assert a change in the original does not affect the clone
            yzDataTable[0].DeltaZStorage = 6;
            Assert.AreNotEqual(6, copyYZTable[0].DeltaZStorage);
        }

        [Test]
        public void TestValidateCellValue()
        {
            var crossSection = new CrossSectionDefinitionYZ();
            IEnumerable<Coordinate> coordinates = new[]
                                          {
                                              new Coordinate(0, 0),
                                              new Coordinate(0.01, -10),
                                              new Coordinate(19.99, -10),
                                              new Coordinate(20, 0)
                                          };
            crossSection.YZDataTable.SetWithCoordinates(coordinates);

            // Setting Y':
            var validationResult = crossSection.ValidateCellValue(1, 0, 2.0);
            Assert.IsTrue(validationResult.Second,
                "User should be able to set Y'");
            Assert.AreEqual("", validationResult.First);

            // Setting Y' using string:
            validationResult = crossSection.ValidateCellValue(1, 0, "2");
            Assert.IsTrue(validationResult.Second,
                "User should be able to set Y' using strings representing numbers");
            Assert.AreEqual("", validationResult.First);

            // Setting Y' using string:
            validationResult = crossSection.ValidateCellValue(1, 0, "test");
            Assert.IsFalse(validationResult.Second,
                "User should be unable to set Y' using strings not representing numbers");
            Assert.AreEqual("Value must be a number.", validationResult.First);

            validationResult = crossSection.ValidateCellValue(1, 0, double.NaN);
            Assert.IsFalse(validationResult.Second,
                "User should be unable to set anything to NaN");
            Assert.AreEqual("Value must be a number.", validationResult.First);

            // Setting Z:
            validationResult = crossSection.ValidateCellValue(1, 1, 0.0);
            Assert.IsTrue(validationResult.Second,
                "User should be able to set Z freely");
            Assert.AreEqual("", validationResult.First);

            // Setting dZ:
            validationResult = crossSection.ValidateCellValue(1, 2, 1.0);
            Assert.IsTrue(validationResult.Second,
                "User should be able to set deltaZStorage");
            Assert.AreEqual("", validationResult.First);

            // Setting dZ (invalid):
            validationResult = crossSection.ValidateCellValue(1, 2, -1.0);
            Assert.IsFalse(validationResult.Second,
                "deltaZStorage cannot be negative");
            Assert.AreEqual("DeltaZ Storage cannot be negative.", validationResult.First);
        }
    }
}