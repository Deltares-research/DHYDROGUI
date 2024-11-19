using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceBoundaryDataConverterTest
    {
        [Test]
        public void ToBoundaryData_EmptyIniSection_ReturnsEmptyBoundaryData()
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Boundary);

            BndExtForceBoundaryData boundaryData = section.ToBoundaryData();

            Assert.That(boundaryData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(boundaryData.LineNumber, Is.Zero);
                Assert.That(boundaryData.Quantity, Is.Null);
                Assert.That(boundaryData.NodeId, Is.Null);
                Assert.That(boundaryData.LocationFile, Is.Null);
                Assert.That(boundaryData.ForcingFiles, Is.Empty);
                Assert.That(boundaryData.ReturnTime, Is.NaN);
                Assert.That(boundaryData.TracerFallVelocity, Is.NaN);
                Assert.That(boundaryData.TracerDecayTime, Is.NaN);
                Assert.That(boundaryData.FlowLinkWidth, Is.NaN);
                Assert.That(boundaryData.BedLevelDepth, Is.NaN);
            });
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ToBoundaryData_IniSectionWithoutPropertyValues_ReturnsEmptyBoundaryData(string value)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Boundary) { LineNumber = 2 };

            section.AddProperty(BndExtForceFileConstants.Keys.Quantity, value);
            section.AddProperty(BndExtForceFileConstants.Keys.NodeId, value);
            section.AddProperty(BndExtForceFileConstants.Keys.LocationFile, value);
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, value);
            section.AddProperty(BndExtForceFileConstants.Keys.ReturnTime, value);
            section.AddProperty(BndExtForceFileConstants.Keys.TracerFallVelocity, value);
            section.AddProperty(BndExtForceFileConstants.Keys.TracerDecayTime, value);
            section.AddProperty(BndExtForceFileConstants.Keys.FlowLinkWidth, value);
            section.AddProperty(BndExtForceFileConstants.Keys.BedLevelDepth, value);

            BndExtForceBoundaryData boundaryData = section.ToBoundaryData();

            Assert.That(boundaryData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(boundaryData.LineNumber, Is.EqualTo(2));
                Assert.That(boundaryData.Quantity, Is.Empty);
                Assert.That(boundaryData.NodeId, Is.Empty);
                Assert.That(boundaryData.LocationFile, Is.Empty);
                Assert.That(boundaryData.ForcingFiles, Is.Empty);
                Assert.That(boundaryData.ReturnTime, Is.NaN);
                Assert.That(boundaryData.TracerFallVelocity, Is.NaN);
                Assert.That(boundaryData.TracerDecayTime, Is.NaN);
                Assert.That(boundaryData.FlowLinkWidth, Is.NaN);
                Assert.That(boundaryData.BedLevelDepth, Is.NaN);
            });
        }

        [Test]
        public void ToBoundaryData_ValidIniSection_ReturnsBoundaryData()
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Boundary) { LineNumber = 5 };

            section.AddProperty(BndExtForceFileConstants.Keys.Quantity, "waterlevel");
            section.AddProperty(BndExtForceFileConstants.Keys.NodeId, "Node123");
            section.AddProperty(BndExtForceFileConstants.Keys.LocationFile, "location.dat");
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, "forcing1.bc");
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, "forcing2.bc");
            section.AddProperty(BndExtForceFileConstants.Keys.ReturnTime, 10.5);
            section.AddProperty(BndExtForceFileConstants.Keys.TracerFallVelocity, 2.3);
            section.AddProperty(BndExtForceFileConstants.Keys.TracerDecayTime, 5.7);
            section.AddProperty(BndExtForceFileConstants.Keys.FlowLinkWidth, 0.8);
            section.AddProperty(BndExtForceFileConstants.Keys.BedLevelDepth, 1.2);

            BndExtForceBoundaryData boundaryData = section.ToBoundaryData();

            Assert.That(boundaryData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(boundaryData.LineNumber, Is.EqualTo(5));
                Assert.That(boundaryData.Quantity, Is.EqualTo("waterlevel"));
                Assert.That(boundaryData.NodeId, Is.EqualTo("Node123"));
                Assert.That(boundaryData.LocationFile, Is.EqualTo("location.dat"));
                Assert.That(boundaryData.ForcingFiles, Is.EqualTo(new[] { "forcing1.bc", "forcing2.bc" }));
                Assert.That(boundaryData.ReturnTime, Is.EqualTo(10.5));
                Assert.That(boundaryData.TracerFallVelocity, Is.EqualTo(2.3));
                Assert.That(boundaryData.TracerDecayTime, Is.EqualTo(5.7));
                Assert.That(boundaryData.FlowLinkWidth, Is.EqualTo(0.8));
                Assert.That(boundaryData.BedLevelDepth, Is.EqualTo(1.2));
            });
        }

        [Test]
        public void ToIniSection_BoundaryDataWithoutValues_ReturnsIniSectionWithDefaults()
        {
            var boundaryData = new BndExtForceBoundaryData();

            var section = boundaryData.ToIniSection();

            Assert.That(section, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(section.Name, Is.EqualTo(BndExtForceFileConstants.Headers.Boundary));
                Assert.That(section.Properties.Select(x => x.Key), Is.Empty);
            });
        }

        [Test]
        public void ToIniSection_BoundaryDataWithValidValues_ReturnsIniSection()
        {
            var boundaryData = new BndExtForceBoundaryData
            {
                Quantity = "waterlevel",
                NodeId = "Node123",
                LocationFile = "left01.pli",
                ForcingFiles = new[] { "waterlevel1.bc", "waterlevel2.bc" },
                ReturnTime = 10.5,
                TracerFallVelocity = 2.3,
                TracerDecayTime = 5.7,
                FlowLinkWidth = 0.8,
                BedLevelDepth = 1.2
            };

            var section = boundaryData.ToIniSection();

            Assert.That(section, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(section.Name, Is.EqualTo(BndExtForceFileConstants.Headers.Boundary));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Quantity), Is.EqualTo("waterlevel"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.NodeId), Is.EqualTo("Node123"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.LocationFile), Is.EqualTo("left01.pli"));
                Assert.That(section.GetAllPropertyValues(BndExtForceFileConstants.Keys.ForcingFile), Is.EqualTo(new[] { "waterlevel1.bc", "waterlevel2.bc" }));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.ReturnTime), Is.EqualTo("1.0500000e+001"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.TracerFallVelocity), Is.EqualTo("2.3000000e+000"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.TracerDecayTime), Is.EqualTo("5.7000000e+000"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.FlowLinkWidth), Is.EqualTo("8.0000000e-001"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.BedLevelDepth), Is.EqualTo("1.2000000e+000"));
            });
        }
    }
}