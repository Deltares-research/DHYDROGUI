using System.Linq;
using Deltares.Infrastructure.IO.Ini;
using DHYDRO.Common.IO.BndExtForce;
using NUnit.Framework;

namespace DHYDRO.Common.Tests.IO.BndExtForce
{
    [TestFixture]
    public class BndExtForceFileDataConverterTest
    {
        [Test]
        public void ToExtForceFileData_NullIniData_ThrowsArgumentNullException()
        {
            Assert.That(() => BndExtForceFileDataConverter.ToExtForceFileData(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ToExtForceFileData_EmptyIniData_ReturnsEmptyExtForceFileData()
        {
            var iniData = new IniData();

            BndExtForceFileData extForceFileData = iniData.ToExtForceFileData();

            Assert.That(extForceFileData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(extForceFileData.FileInfo, Is.Not.Null);
                Assert.That(extForceFileData.BoundaryForcings, Is.Empty);
                Assert.That(extForceFileData.LateralForcings, Is.Empty);
                Assert.That(extForceFileData.MeteoForcings, Is.Empty);
            });
        }

        [Test]
        public void ToExtForceFileData_EmptyIniSections_ReturnsEmptyExtForceFileData()
        {
            var iniData = new IniData();

            iniData.AddSection(BndExtForceFileConstants.Headers.General);
            iniData.AddSection(BndExtForceFileConstants.Headers.Boundary);
            iniData.AddSection(BndExtForceFileConstants.Headers.Lateral);
            iniData.AddSection(BndExtForceFileConstants.Headers.Meteo);

            BndExtForceFileData extForceFileData = iniData.ToExtForceFileData();

            Assert.That(extForceFileData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(extForceFileData.FileInfo, Is.Not.Null);
                Assert.That(extForceFileData.BoundaryForcings, Has.Exactly(1).Items);
                Assert.That(extForceFileData.LateralForcings, Has.Exactly(1).Items);
                Assert.That(extForceFileData.MeteoForcings, Has.Exactly(1).Items);
            });
        }

        [Test]
        public void ToExtForceFileData_ValidIniData_ReturnsExtForceFileData()
        {
            var iniData = new IniData();

            IniSection generalSection = iniData.AddSection(BndExtForceFileConstants.Headers.General);
            generalSection.AddProperty(BndExtForceFileConstants.Keys.FileVersion, "1.0");
            generalSection.AddProperty(BndExtForceFileConstants.Keys.FileType, "extForce");

            IniSection boundarySection = iniData.AddSection(BndExtForceFileConstants.Headers.Boundary);
            boundarySection.AddProperty(BndExtForceFileConstants.Keys.Quantity, "waterlevel");
            boundarySection.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, "forcing1.bc");
            boundarySection.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, "forcing2.bc");

            IniSection lateralSection = iniData.AddSection(BndExtForceFileConstants.Headers.Lateral);
            lateralSection.AddProperty(BndExtForceFileConstants.Keys.Id, "Lateral1");
            lateralSection.AddProperty(BndExtForceFileConstants.Keys.Name, "Test Lateral");
            lateralSection.AddProperty(BndExtForceFileConstants.Keys.Discharge, 42);

            IniSection meteoSection = iniData.AddSection(BndExtForceFileConstants.Headers.Meteo);
            meteoSection.AddProperty(BndExtForceFileConstants.Keys.Quantity, "rainfall");
            meteoSection.AddProperty(BndExtForceFileConstants.Keys.ForcingFile, "rainfall.nc");
            meteoSection.AddProperty(BndExtForceFileConstants.Keys.ForcingFileType, BndExtForceDataFileType.Uniform);

            BndExtForceFileData extForceFileData = iniData.ToExtForceFileData();

            Assert.That(extForceFileData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(extForceFileData.FileInfo, Is.Not.Null);
                Assert.That(extForceFileData.FileInfo.FileVersion, Is.EqualTo("1.0"));
                Assert.That(extForceFileData.FileInfo.FileType, Is.EqualTo("extForce"));

                Assert.That(extForceFileData.BoundaryForcings, Has.Exactly(1).Items);
                BndExtForceBoundaryData boundaryForcing = extForceFileData.BoundaryForcings.First();
                Assert.That(boundaryForcing.Quantity, Is.EqualTo("waterlevel"));
                Assert.That(boundaryForcing.ForcingFiles, Is.EqualTo(new[] { "forcing1.bc", "forcing2.bc" }));

                Assert.That(extForceFileData.LateralForcings, Has.Exactly(1).Items);
                BndExtForceLateralData lateralForcing = extForceFileData.LateralForcings.First();
                Assert.That(lateralForcing.Id, Is.EqualTo("Lateral1"));
                Assert.That(lateralForcing.Name, Is.EqualTo("Test Lateral"));
                Assert.That(lateralForcing.Discharge, Is.Not.Null);
                Assert.That(lateralForcing.Discharge.DischargeType, Is.EqualTo(BndExtForceDischargeType.TimeConstant));
                Assert.That(lateralForcing.Discharge.ScalarValue, Is.EqualTo(42.0));

                Assert.That(extForceFileData.MeteoForcings, Has.Exactly(1).Items);
                BndExtForceMeteoData meteoForcing = extForceFileData.MeteoForcings.First();
                Assert.That(meteoForcing.Quantity, Is.EqualTo("rainfall"));
                Assert.That(meteoForcing.ForcingFile, Is.EqualTo("rainfall.nc"));
                Assert.That(meteoForcing.ForcingFileType, Is.EqualTo(BndExtForceDataFileType.Uniform));
            });
        }

        [Test]
        public void ToIniData_NullExtForceFileData_ThrowsArgumentNullException()
        {
            Assert.That(() => BndExtForceFileDataConverter.ToIniData(null), Throws.ArgumentNullException);
        }

        [Test]
        public void ToIniData_EmptyExtForceFileData_ReturnsIniDataWithGeneralSectionOnly()
        {
            var extForceFileData = new BndExtForceFileData();

            var iniData = extForceFileData.ToIniData();

            Assert.That(iniData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(iniData.Sections, Has.One.Items);

                IniSection section = iniData.Sections.First();
                Assert.That(section.Name, Is.EqualTo(BndExtForceFileConstants.Headers.General));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.FileVersion), Is.EqualTo(BndExtForceFileConstants.DefaultFileVersion));
                Assert.That(section.GetPropertyValue(BndExtForceFileConstants.Keys.FileType), Is.EqualTo(BndExtForceFileConstants.DefaultFileType));
            });
        }

        [Test]
        public void ToIniData_ExtForceFileDataWithValidValues_ReturnsCorrectIniSection()
        {
            var fileInfo = new BndExtForceFileInfo
            {
                FileVersion = "1.0",
                FileType = "extForce"
            };

            var boundaryData = new BndExtForceBoundaryData
            {
                Quantity = "waterlevel",
                ForcingFiles = new[] { "forcing1.bc", "forcing2.bc" }
            };

            var dischargeData = new BndExtForceDischargeData
            {
                DischargeType = BndExtForceDischargeType.TimeConstant,
                ScalarValue = 42.0
            };

            var lateralData = new BndExtForceLateralData
            {
                Id = "Lateral1",
                Name = "Test Lateral",
                Discharge = dischargeData
            };

            var meteoData = new BndExtForceMeteoData
            {
                Quantity = "rainfall",
                ForcingFile = "rainfall.nc",
                ForcingFileType = BndExtForceDataFileType.NetCDF
            };

            var extForceFileData = new BndExtForceFileData { FileInfo = fileInfo };
            extForceFileData.AddBoundaryForcing(boundaryData);
            extForceFileData.AddLateralForcing(lateralData);
            extForceFileData.AddMeteoForcing(meteoData);

            var iniData = extForceFileData.ToIniData();

            Assert.That(iniData, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(iniData.Sections.Select(x => x.Name), Is.EqualTo(new[]
                {
                    BndExtForceFileConstants.Headers.General,
                    BndExtForceFileConstants.Headers.Boundary,
                    BndExtForceFileConstants.Headers.Lateral,
                    BndExtForceFileConstants.Headers.Meteo,
                }));

                IniSection generalSection = iniData.FindSection(BndExtForceFileConstants.Headers.General);
                Assert.That(generalSection, Is.Not.Null);
                Assert.That(generalSection.GetPropertyValue(BndExtForceFileConstants.Keys.FileVersion), Is.EqualTo("1.0"));
                Assert.That(generalSection.GetPropertyValue(BndExtForceFileConstants.Keys.FileType), Is.EqualTo("extForce"));

                IniSection boundarySection = iniData.FindSection(BndExtForceFileConstants.Headers.Boundary);
                Assert.That(boundarySection, Is.Not.Null);
                Assert.That(boundarySection.GetPropertyValue(BndExtForceFileConstants.Keys.Quantity), Is.EqualTo("waterlevel"));
                Assert.That(boundarySection.GetAllPropertyValues(BndExtForceFileConstants.Keys.ForcingFile), Is.EqualTo(new[] { "forcing1.bc", "forcing2.bc" }));

                IniSection lateralSection = iniData.FindSection(BndExtForceFileConstants.Headers.Lateral);
                Assert.That(lateralSection, Is.Not.Null);
                Assert.That(lateralSection.GetPropertyValue(BndExtForceFileConstants.Keys.Id), Is.EqualTo("Lateral1"));
                Assert.That(lateralSection.GetPropertyValue(BndExtForceFileConstants.Keys.Name), Is.EqualTo("Test Lateral"));
                Assert.That(lateralSection.GetPropertyValue(BndExtForceFileConstants.Keys.Discharge), Is.EqualTo("4.2000000e+001"));

                IniSection meteoSection = iniData.FindSection(BndExtForceFileConstants.Headers.Meteo);
                Assert.That(meteoSection, Is.Not.Null);
                Assert.That(meteoSection.GetPropertyValue(BndExtForceFileConstants.Keys.Quantity), Is.EqualTo("rainfall"));
                Assert.That(meteoSection.GetPropertyValue(BndExtForceFileConstants.Keys.ForcingFile), Is.EqualTo("rainfall.nc"));
                Assert.That(meteoSection.GetPropertyValue(BndExtForceFileConstants.Keys.ForcingFileType), Is.EqualTo("netcdf"));
            });
        }
    }
}