using System;
using System.Collections.Generic;
using Deltares.Infrastructure.IO.Ini;
using Deltares.Infrastructure.TestUtils.Logging;
using DHYDRO.Common.IO.BndExtForce;
using log4net.Core;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceDischargeDataConverterTest
    {
        [Test]
        public void ToDischargeData_DischargePropertyWithoutValue_ReturnsNull()
        {
            var property = new IniProperty(BndExtForceFileConstants.Keys.Discharge);

            BndExtForceDischargeData dischargeData = property.ToDischargeData();

            Assert.That(dischargeData, Is.Null);
        }

        [Test]
        public void ToDischargeData_DischargePropertyWithScalarValue_ReturnsTimeConstantDischargeData()
        {
            var property = IniProperty.Create(BndExtForceFileConstants.Keys.Discharge, 42.3);

            BndExtForceDischargeData dischargeData = property.ToDischargeData();

            Assert.That(dischargeData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(dischargeData.DischargeType, Is.EqualTo(BndExtForceDischargeType.TimeConstant));
                Assert.That(dischargeData.ScalarValue, Is.EqualTo(42.3));
            });
        }

        [Test]
        public void ToDischargeData_DischargePropertyWithRealTimeValue_ReturnsExternalDischargeData()
        {
            var property = IniProperty.Create(BndExtForceFileConstants.Keys.Discharge, BndExtForceFileConstants.RealTimeValue);

            BndExtForceDischargeData dischargeData = property.ToDischargeData();

            Assert.That(dischargeData, Is.Not.Null);
            Assert.That(dischargeData.DischargeType, Is.EqualTo(BndExtForceDischargeType.External));
        }

        [Test]
        [TestCase("discharge.bc")]
        [TestCase("DISCHARGE.BC")]
        public void ToDischargeData_DischargePropertyWithTimeSeriesFile_ReturnsTimeVaryingDischargeData(string timeSeriesFile)
        {
            var property = IniProperty.Create(BndExtForceFileConstants.Keys.Discharge, timeSeriesFile);

            BndExtForceDischargeData dischargeData = property.ToDischargeData();

            Assert.That(dischargeData, Is.Not.Null);
            Assert.That(dischargeData.DischargeType, Is.EqualTo(BndExtForceDischargeType.TimeVarying));
            Assert.That(dischargeData.TimeSeriesFile, Is.EqualTo("discharge.bc").IgnoreCase);
        }

        [Test]
        [TestCase("discharge.cb")]
        [TestCase("discharge.pli")]
        [TestCase("discharge.pol")]
        [TestCase("discharge.tim")]
        [TestCase("discharge.nc")]
        [TestCase("discharge.nc")]
        public void ToDischargeData_DischargePropertyWithInvalidTimeSeriesFileExtension_ReturnsNull(string timeSeriesFile)
        {
            var property = IniProperty.Create(BndExtForceFileConstants.Keys.Discharge, timeSeriesFile);

            BndExtForceDischargeData dischargeData = property.ToDischargeData();

            Assert.That(dischargeData, Is.Null);
        }

        [Test]
        public void ToDischargeData_DischargePropertyWithUnsupportedValue_LogsErrorAndReturnsNull()
        {
            var property = new IniProperty(BndExtForceFileConstants.Keys.Discharge, "unsupported_value") { LineNumber = 3 };

            IEnumerable<string> messages = Log4NetTestHelper.GetAllRenderedMessages(() => property.ToDischargeData(), Level.Error);

            Assert.That(messages, Has.One.EqualTo("Unsupported discharge value: unsupported_value. Line: 3."));
        }

        [Test]
        public void ToIniProperty_TimeConstantDischargeData_ReturnsPropertyWithScalarValue()
        {
            var dischargeData = new BndExtForceDischargeData
            {
                DischargeType = BndExtForceDischargeType.TimeConstant,
                ScalarValue = 42.3
            };

            var property = dischargeData.ToIniProperty();

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo("4.2300000e+001"));
        }

        [Test]
        public void ToIniProperty_TimeVaryingDischargeData_ReturnsPropertyWithTimeSeriesFile()
        {
            var dischargeData = new BndExtForceDischargeData
            {
                DischargeType = BndExtForceDischargeType.TimeVarying,
                TimeSeriesFile = "discharge.bc"
            };

            var property = dischargeData.ToIniProperty();

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo("discharge.bc"));
        }

        [Test]
        public void ToIniProperty_ExternalDischargeData_ReturnsPropertyWithRealTimeValue()
        {
            var dischargeData = new BndExtForceDischargeData { DischargeType = BndExtForceDischargeType.External };

            var property = dischargeData.ToIniProperty();

            Assert.That(property, Is.Not.Null);
            Assert.That(property.Value, Is.EqualTo(BndExtForceFileConstants.RealTimeValue));
        }

        [Test]
        public void ToIniProperty_InvalidDischargeType_ThrowsArgumentOutOfRangeException()
        {
            var dischargeData = new BndExtForceDischargeData { DischargeType = (BndExtForceDischargeType)999 };

            Assert.Throws<ArgumentOutOfRangeException>(() => dischargeData.ToIniProperty());
        }
    }
}