using System.Linq;
using Deltares.Infrastructure.Extensions;
using Deltares.Infrastructure.IO.Ini;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceMeteoDataConverterTest
    {
        [Test]
        public void ToMeteoData_EmptyIniSection_ReturnsEmptyMeteoData()
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Meteo);

            BndExtForceMeteoData meteoData = section.ToMeteoData();

            Assert.That(meteoData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(meteoData.LineNumber, Is.Zero);
                Assert.That(meteoData.Quantity, Is.Null);
                Assert.That(meteoData.ForcingFile, Is.Null);
                Assert.That(meteoData.ForcingFileType, Is.EqualTo(BndExtForceDataFileType.None));
                Assert.That(meteoData.TargetMaskFile, Is.Null);
                Assert.That(meteoData.TargetMaskInvert, Is.False);
                Assert.That(meteoData.InterpolationMethod, Is.EqualTo(BndExtForceInterpolationMethod.None));
                Assert.That(meteoData.Operand, Is.EqualTo(BndExtForceOperand.None));
            });
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ToMeteoData_IniSectionWithoutPropertyValues_ReturnsEmptyMeteoData(string value)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Meteo) { LineNumber = 4 };

            section.AddProperty(BndExtForceFileConstants.Keys.Quantity, value);
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, value);
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFileType, value);
            section.AddProperty(BndExtForceFileConstants.Keys.TargetMaskFile, value);
            section.AddProperty(BndExtForceFileConstants.Keys.TargetMaskInvert, value);
            section.AddProperty(BndExtForceFileConstants.Keys.InterpolationMethod, value);
            section.AddProperty(BndExtForceFileConstants.Keys.Operand, value);

            BndExtForceMeteoData meteoData = section.ToMeteoData();

            Assert.That(meteoData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(meteoData.LineNumber, Is.EqualTo(4));
                Assert.That(meteoData.Quantity, Is.Empty);
                Assert.That(meteoData.ForcingFile, Is.Empty);
                Assert.That(meteoData.ForcingFileType, Is.EqualTo(BndExtForceDataFileType.None));
                Assert.That(meteoData.TargetMaskFile, Is.Empty);
                Assert.That(meteoData.TargetMaskInvert, Is.False);
                Assert.That(meteoData.InterpolationMethod, Is.EqualTo(BndExtForceInterpolationMethod.None));
                Assert.That(meteoData.Operand, Is.EqualTo(BndExtForceOperand.None));
            });
        }

        [Test]
        public void ToMeteoData_ValidIniSection_ReturnsBoundaryData(
            [Values] BndExtForceDataFileType dataFileType,
            [Values] BndExtForceInterpolationMethod interpolationMethod,
            [Values] BndExtForceOperand operand)
        {
            var section = new IniSection(BndExtForceFileConstants.Headers.Meteo) { LineNumber = 42 };

            section.AddProperty(BndExtForceFileConstants.Keys.Quantity, "rainfall");
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, "rainfall.nc");
            section.AddProperty(BndExtForceFileConstants.Keys.ForcingFileType, dataFileType);
            section.AddProperty(BndExtForceFileConstants.Keys.TargetMaskFile, "mask_file.pol");
            section.AddProperty(BndExtForceFileConstants.Keys.TargetMaskInvert, true);
            section.AddProperty(BndExtForceFileConstants.Keys.InterpolationMethod, interpolationMethod);
            section.AddProperty(BndExtForceFileConstants.Keys.Operand, operand);

            BndExtForceMeteoData meteoData = section.ToMeteoData();

            Assert.That(meteoData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(meteoData.LineNumber, Is.EqualTo(42));
                Assert.That(meteoData.Quantity, Is.EqualTo("rainfall"));
                Assert.That(meteoData.ForcingFile, Is.EqualTo("rainfall.nc"));
                Assert.That(meteoData.ForcingFileType, Is.EqualTo(dataFileType));
                Assert.That(meteoData.TargetMaskFile, Is.EqualTo("mask_file.pol"));
                Assert.That(meteoData.TargetMaskInvert, Is.True);
                Assert.That(meteoData.InterpolationMethod, Is.EqualTo(interpolationMethod));
                Assert.That(meteoData.Operand, Is.EqualTo(operand));
            });
        }
        
        [Test]
        public void ToMeteoData_MeteoDataWithoutValues_ReturnsIniSectionWithDefaults()
        {
            var meteoData = new BndExtForceMeteoData();

            var section = meteoData.ToIniSection();

            Assert.That(section, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(section.Name, Is.EqualTo(BndExtForceFileConstants.Headers.Meteo));
                Assert.That(section.Properties.Select(x => x.Key), Is.EqualTo(new[]
                {
                    BndExtForceFileConstants.Keys.Quantity, 
                    BndExtForceFileConstants.Keys.ForcingFile,
                    BndExtForceFileConstants.Keys.ForcingFileType,
                    BndExtForceFileConstants.Keys.InterpolationMethod,
                    BndExtForceFileConstants.Keys.Operand
                }));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Quantity), Is.Empty);
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.ForcingFile), Is.Empty);
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.ForcingFileType), Is.Empty);
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.InterpolationMethod), Is.Empty);
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Operand), Is.Empty);
            });
        }

        [Test]
        public void ToIniSection_MeteoDataWithValidValues_ReturnsIniSection(
            [Values] BndExtForceDataFileType dataFileType,
            [Values] BndExtForceInterpolationMethod interpolationMethod,
            [Values] BndExtForceOperand operand)
        {
            var meteoData = new BndExtForceMeteoData
            {
                Quantity = "rainfall",
                ForcingFile = "rainfall.nc",
                ForcingFileType = dataFileType,
                TargetMaskFile = "mask_file.pol",
                TargetMaskInvert = true,
                InterpolationMethod = interpolationMethod,
                Operand = operand
            };

            var section = meteoData.ToIniSection();

            Assert.That(section.Name, Is.EqualTo(BndExtForceFileConstants.Headers.Meteo));
            Assert.Multiple(() =>
            {
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Quantity), Is.EqualTo("rainfall"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.ForcingFile), Is.EqualTo("rainfall.nc"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.ForcingFileType), Is.EqualTo(dataFileType.GetDescription()));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.TargetMaskFile), Is.EqualTo("mask_file.pol"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.TargetMaskInvert), Is.EqualTo("yes"));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.InterpolationMethod), Is.EqualTo(interpolationMethod.GetDescription()));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.Operand), Is.EqualTo(operand.GetDescription()));
            });
        }
    }
}