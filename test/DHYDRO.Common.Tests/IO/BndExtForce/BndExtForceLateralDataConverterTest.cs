using System.Linq;
using Deltares.Infrastructure.Extensions;
using Deltares.Infrastructure.IO.Ini;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceLateralDataConverterTest
    {
        [Test]
        public void ToLateralData_EmptyIniSection_ReturnsEmptyLateralData()
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Lateral);

            BndExtForceLateralData lateralData = section.ToLateralData();

            Assert.That(lateralData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(lateralData.LineNumber, Is.Zero);
                Assert.That(lateralData.Id, Is.Null);
                Assert.That(lateralData.Name, Is.Null);
                Assert.That(lateralData.LocationType, Is.EqualTo(BndExtForceLocationType.All));
                Assert.That(lateralData.NodeId, Is.Null);
                Assert.That(lateralData.BranchId, Is.Null);
                Assert.That(lateralData.Chainage, Is.Zero);
                Assert.That(lateralData.NumCoordinates, Is.Zero);
                Assert.That(lateralData.XCoordinates, Is.Empty);
                Assert.That(lateralData.YCoordinates, Is.Empty);
                Assert.That(lateralData.LocationFile, Is.Null);
                Assert.That(lateralData.Discharge, Is.Null);
            });
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ToLateralData_IniSectionWithoutPropertyValues_ReturnsEmptyLateralData(string value)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Lateral) { LineNumber = 2 };

            section.AddProperty(BndExtForceFileConstants.Keys.Id, value);
            section.AddProperty(BndExtForceFileConstants.Keys.Name, value);
            section.AddProperty(BndExtForceFileConstants.Keys.LocationType, value);
            section.AddProperty(BndExtForceFileConstants.Keys.NodeId, value);
            section.AddProperty(BndExtForceFileConstants.Keys.BranchId, value);
            section.AddProperty(BndExtForceFileConstants.Keys.Chainage, value);
            section.AddProperty(BndExtForceFileConstants.Keys.XCoordinates, value);
            section.AddProperty(BndExtForceFileConstants.Keys.YCoordinates, value);
            section.AddProperty(BndExtForceFileConstants.Keys.LocationFile, value);
            section.AddProperty(BndExtForceFileConstants.Keys.Discharge, value);

            BndExtForceLateralData lateralData = section.ToLateralData();

            Assert.That(lateralData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(lateralData.LineNumber, Is.EqualTo(2));
                Assert.That(lateralData.Id, Is.Empty);
                Assert.That(lateralData.Name, Is.Empty);
                Assert.That(lateralData.LocationType, Is.EqualTo(BndExtForceLocationType.All));
                Assert.That(lateralData.NodeId, Is.Empty);
                Assert.That(lateralData.BranchId, Is.Empty);
                Assert.That(lateralData.Chainage, Is.Zero);
                Assert.That(lateralData.NumCoordinates, Is.Zero);
                Assert.That(lateralData.XCoordinates, Is.Empty);
                Assert.That(lateralData.YCoordinates, Is.Empty);
                Assert.That(lateralData.LocationFile, Is.Empty);
                Assert.That(lateralData.Discharge, Is.Null);
            });
        }

        [Test]
        public void ToLateralData_ValidIniSection_ReturnsLateralData(
            [Values] BndExtForceLocationType locationType)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Lateral) { LineNumber = 5 };

            section.AddProperty(BndExtForceFileConstants.Keys.Id, "Lateral1");
            section.AddProperty(BndExtForceFileConstants.Keys.Name, "Test Lateral");
            section.AddProperty(BndExtForceFileConstants.Keys.LocationType, locationType);
            section.AddProperty(BndExtForceFileConstants.Keys.NodeId, "Node1");
            section.AddProperty(BndExtForceFileConstants.Keys.BranchId, "Branch1");
            section.AddProperty(BndExtForceFileConstants.Keys.Chainage, 100.5);
            section.AddProperty(BndExtForceFileConstants.Keys.NumCoordinates, 2);
            section.AddMultiValueProperty(BndExtForceFileConstants.Keys.XCoordinates, new[] { 10.0, 20.0 });
            section.AddMultiValueProperty(BndExtForceFileConstants.Keys.YCoordinates, new[] { 30.0, 40.0 });
            section.AddProperty(BndExtForceFileConstants.Keys.LocationFile, "lateral.pol");
            section.AddProperty(BndExtForceFileConstants.Keys.Discharge, 42);

            BndExtForceLateralData lateralData = section.ToLateralData();

            Assert.That(lateralData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(lateralData.Id, Is.EqualTo("Lateral1"));
                Assert.That(lateralData.Name, Is.EqualTo("Test Lateral"));
                Assert.That(lateralData.LocationType, Is.EqualTo(locationType));
                Assert.That(lateralData.NodeId, Is.EqualTo("Node1"));
                Assert.That(lateralData.BranchId, Is.EqualTo("Branch1"));
                Assert.That(lateralData.Chainage, Is.EqualTo(100.5));
                Assert.That(lateralData.NumCoordinates, Is.EqualTo(2));
                Assert.That(lateralData.XCoordinates, Is.EquivalentTo(new[] { 10.0, 20.0 }));
                Assert.That(lateralData.YCoordinates, Is.EquivalentTo(new[] { 30.0, 40.0 }));
                Assert.That(lateralData.LocationFile, Is.EqualTo("lateral.pol"));
                Assert.That(lateralData.Discharge, Is.Not.Null);
                Assert.That(lateralData.Discharge.DischargeType, Is.EqualTo(BndExtForceDischargeType.TimeConstant));
                Assert.That(lateralData.Discharge.ScalarValue, Is.EqualTo(42));
            });
        }

        [Test]
        public void ToIniSection_LateralDataWithoutValues_ReturnsIniSectionWithDefaults()
        {
            var lateralData = new BndExtForceLateralData();

            var section = lateralData.ToIniSection();

            Assert.That(section, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(section.Name, Is.EqualTo(BndExtForceFileConstants.Headers.Lateral));
                Assert.That(section.Properties.Select(x => x.Key), Is.EqualTo(new[]
                {
                    BndExtForceFileConstants.Keys.Id, 
                    BndExtForceFileConstants.Keys.Name, 
                    BndExtForceFileConstants.Keys.Discharge
                }));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Id), Is.Empty);
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Name), Is.Empty);
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Discharge), Is.Empty);
            });
        }

        [Test]
        public void ToIniSection_LateralDataWithValidValues_ReturnsCorrectIniSection(
            [Values] BndExtForceLocationType locationType)
        {
            var lateralData = new BndExtForceLateralData
            {
                Id = "Lateral1",
                Name = "Test Lateral",
                LocationType = locationType,
                NodeId = "Node1",
                BranchId = "Branch1",
                Chainage = 100.5,
                NumCoordinates = 2,
                XCoordinates = new[] { 11.1, 22.2 },
                YCoordinates = new[] { 33.3, 44.4 },
                LocationFile = "discharge.pol",
                Discharge = new BndExtForceDischargeData
                {
                    DischargeType = BndExtForceDischargeType.TimeConstant,
                    ScalarValue = 42.33
                }
            };

            var section = lateralData.ToIniSection();

            Assert.That(section, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(section.Name, Is.EqualTo(BndExtForceFileConstants.Headers.Lateral));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Id), Is.EqualTo("Lateral1"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Name), Is.EqualTo("Test Lateral"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.LocationType), Is.EqualTo(locationType.GetDescription()));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.NodeId), Is.EqualTo("Node1"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.BranchId), Is.EqualTo("Branch1"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Chainage), Is.EqualTo("1.0050000e+002"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.NumCoordinates), Is.EqualTo("2"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.XCoordinates), Is.EqualTo("11.10 22.20"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.YCoordinates), Is.EqualTo("33.30 44.40"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.LocationFile), Is.EqualTo("discharge.pol"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Discharge), Is.EqualTo("4.2330000e+001"));
            });
        }
    }
}