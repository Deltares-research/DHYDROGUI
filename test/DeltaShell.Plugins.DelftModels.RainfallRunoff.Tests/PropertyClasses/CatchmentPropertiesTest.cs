using System;
using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.PropertyClasses;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.PropertyClasses
{
    [TestFixture]
    public class CatchmentPropertiesTest
    {
        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new Catchment { Name = "some_name" };
            data.AttachNameValidator(validator);
            var properties = new CatchmentProperties { Data = data };

            // Act
            properties.Name = "some_invalid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);

            var data = new Catchment { Name = "some_name" };
            data.AttachNameValidator(validator);
            var properties = new CatchmentProperties { Data = data };

            // Act
            properties.Name = "some_valid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_valid_name"));
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void GivenCatchmentProperties_OpeningInPropertiesWindow_ShouldNotCrash()
        {
            var catchment = new Catchment{CatchmentType = CatchmentType.Unpaved};
            var catchmentData = new UnpavedData(catchment){CalculationArea = 20000};
            var catchmentProperties = new CatchmentProperties
            {
                Data = catchment,
                CatchmentData = catchmentData
            };

            Assert.DoesNotThrow(() => WindowsFormsTestHelper.ShowPropertyGridForObject(catchmentProperties));
        }

        [Test]
        public void GivenCatchmentProperties_SettingCalculationArea_ShouldGiveErrorMessageIfCatchmentDataIsNotSet()
        {
            //Arrange
            var catchment = new Catchment { CatchmentType = CatchmentType.Unpaved };
            var properties = new CatchmentProperties { Data = catchment };

            // Act & Assert
            TestHelper.AssertLogMessageIsGenerated(()=> properties.ComputationArea = 1000, $"Could not set {catchment.Name} computation area", 1);
        }

        [Test]
        public void GivenCatchmentProperties_GetSet_GetsReroutedToCatchmentOrCatchmentData()
        {
            //Arrange
            var polygon = new Polygon(new LinearRing(new[]
            {
                new Coordinate(0,0),
                new Coordinate(0,10),
                new Coordinate(10,10),
                new Coordinate(10,0),
                new Coordinate(0,0)
            }));

            var catchment = new Catchment
            {
                CatchmentType = CatchmentType.Unpaved,
                IsGeometryDerivedFromAreaSize = true,
                Geometry = polygon
            };

            var catchmentData = new UnpavedData(catchment) {CalculationArea = 10};

            var properties = new CatchmentProperties { Data = catchment, CatchmentData = catchmentData };
            
            // Act & Assert

            Assert.AreEqual(catchment.Name, properties.Name);
            properties.Name = "test";
            Assert.AreEqual(catchment.Name, properties.Name);

            Assert.AreEqual(catchment.LongName, properties.LongName);
            properties.LongName = "test";
            Assert.AreEqual(catchment.LongName, properties.LongName);

            properties.ComputationArea = 100;
            Assert.AreEqual(catchmentData.CalculationArea, properties.ComputationArea);

            Assert.AreEqual(catchment.Geometry.Area, properties.GeometryArea);
            Assert.AreEqual(catchment.CatchmentType, properties.CatchmentType);
            Assert.AreEqual(catchment.IsGeometryDerivedFromAreaSize, properties.IsDefaultGeometry);
            Assert.AreEqual(catchment.CatchmentTypes, properties.CatchmentTypes);
        }

        [Test]
        public void CatchmentType_ExpectedDefault()
        {
            var catchment = new Catchment();
            Assert.That(catchment.CatchmentTypes, Is.EqualTo(CatchmentTypes.None));
        }

        private static IEnumerable<TestCaseData> CatchmentTypesData()
        {
            yield return new TestCaseData(CatchmentTypes.Greenhouse, CatchmentType.GreenHouse);
            yield return new TestCaseData(CatchmentTypes.Hbv, CatchmentType.Hbv);
            yield return new TestCaseData(CatchmentTypes.NWRW, CatchmentType.NWRW);
            yield return new TestCaseData(CatchmentTypes.OpenWater, CatchmentType.OpenWater);
            yield return new TestCaseData(CatchmentTypes.Paved, CatchmentType.Paved);
            yield return new TestCaseData(CatchmentTypes.Unpaved, CatchmentType.Unpaved);
            yield return new TestCaseData(CatchmentTypes.Sacramento, CatchmentType.Sacramento);
        }

        [Test]
        [TestCaseSource(nameof(CatchmentTypesData))]
        public void GivenCatchmentTypeEnumProperties_GetSet_CorrectCatchmentTypeEnum(CatchmentTypes value, 
                                                                                     CatchmentType expectedResult)
        {
            var catchment = new Catchment();
            catchment.CatchmentTypes = value;

            Assert.That(catchment.CatchmentTypes, Is.EqualTo(value));
            Assert.That(catchment.CatchmentType, Is.EqualTo(expectedResult));
        }
        
        private static IEnumerable<TestCaseData> CatchmentTypeStringData()
        {
            yield return new TestCaseData(CatchmentType.NoneTypeName, CatchmentType.None);
            yield return new TestCaseData(CatchmentType.HbvTypeName, CatchmentType.Hbv);
            yield return new TestCaseData(CatchmentType.PavedTypeName, CatchmentType.Paved);
            yield return new TestCaseData(CatchmentType.UnpavedTypeName, CatchmentType.Unpaved);
            yield return new TestCaseData(CatchmentType.SacramentoTypeName, CatchmentType.Sacramento);
            yield return new TestCaseData(CatchmentType.GreenhouseTypeName, CatchmentType.GreenHouse);
            yield return new TestCaseData(CatchmentType.OpenwaterTypeName, CatchmentType.OpenWater);
            yield return new TestCaseData(CatchmentType.NwrwTypeName, CatchmentType.NWRW);
        }

        [Test]
        [TestCaseSource(nameof(CatchmentTypeStringData))]
        public void GivenCatchmentTypeString_LoadFromStringReturnsCorrectCatchmentType(string typeName, 
                                                                                       CatchmentType expectedCatchmentType)
        {
            Assert.That(CatchmentType.LoadFromString(typeName), Is.EqualTo(expectedCatchmentType));
        }

        [Test]
        public void GivenInvalidString_LoadFromStringThrowsArgumentException()
        {
            Assert.That(() => CatchmentType.LoadFromString("This throws an exception"), Throws.InstanceOf<ArgumentException>());
        }

        [Test]
        public void GivenCatchmentToPropertyCatchmentTypesDataChangesToGivenCatchment()
        {
            //Arrange
            var catchment = new Catchment
            {
                CatchmentType = CatchmentType.GreenHouse
            };
            
            CatchmentProperties properties = GetGreenHouseProperties(catchment);
            Assert.That(catchment.CatchmentTypes, Is.EqualTo(CatchmentTypes.Greenhouse));

            //Act
            properties.CatchmentTypes = CatchmentTypes.None;
            
            //Assert
            Assert.That(catchment.CatchmentTypes, Is.EqualTo(CatchmentTypes.None));
        }

        private static CatchmentProperties GetGreenHouseProperties(Catchment catchment)
        {
            CatchmentModelData catchmentData = new GreenhouseData(catchment);

            CatchmentProperties properties = new CatchmentProperties
            {
                Data = catchment,
                CatchmentData = catchmentData
            };
            return properties;
        }
    }
}