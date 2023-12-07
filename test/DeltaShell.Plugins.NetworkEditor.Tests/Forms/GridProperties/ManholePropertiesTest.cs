using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.Common;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using NetTopologySuite.Extensions.Networks;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class ManholePropertiesTest
    {
        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new Manhole { Name = "some_name" };
            var properties = new ManholeProperties { Data = data };
            properties.ManholeNameValidator.AddValidator(validator);

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

            var data = new Manhole { Name = "some_name" };
            var properties = new ManholeProperties { Data = data };
            properties.ManholeNameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_valid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void Name_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.Name = "TestManhole";

            Assert.AreEqual("TestManhole", properties.Name);
        }

        [Test]
        public void X_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.X = 10.0;

            Assert.AreEqual(10.0, properties.X);
        }

        [Test]
        public void Y_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.Y = 20.0;

            Assert.AreEqual(20.0, properties.Y);
        }

        [Test]
        public void CompartmentOneName_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneName = "TestCompartment";

            Assert.AreEqual("TestCompartment", properties.CompartmentOneName);
        }

        [Test]
        public void CompartmentOneBottomLevel_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneBottomLevel = 2.0;

            Assert.AreEqual(2.0, properties.CompartmentOneBottomLevel);
        }

        [Test]
        public void CompartmentOneStreetLevel_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneStreetLevel = 1.0;

            Assert.AreEqual(1.0, properties.CompartmentOneStreetLevel);
        }

        [Test]
        public void CompartmentOneFloodableArea_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneFloodableArea = 5.0;

            Assert.AreEqual(5.0, properties.CompartmentOneFloodableArea);
        }

        [Test]
        public void CompartmentOneStorageType_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneStorageType = CompartmentStorageType.Closed;

            Assert.AreEqual(CompartmentStorageType.Closed, properties.CompartmentOneStorageType);
        }

        [Test]
        public void CompartmentOneUseTable_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneUseTable = true;

            Assert.IsTrue(properties.CompartmentOneUseTable);
        }

        [Test]
        public void CompartmentOneInterpolationType_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneInterpolationType = InterpolationType.Linear;

            Assert.AreEqual(InterpolationType.Linear, properties.CompartmentOneInterpolationType);
        }

        [Test]
        public void CompartmentOneStorage_SetValidValue_ValueIsSet()
        {
            Function storageTable = CreateStorageTable();
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentOneStorage = storageTable;

            Assert.AreEqual(storageTable, properties.CompartmentOneStorage);
        }

        [Test]
        public void CompartmentTwoName_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoName = "TestCompartment";

            Assert.AreEqual("TestCompartment", properties.CompartmentTwoName);
        }

        [Test]
        public void CompartmentTwoBottomLevel_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoBottomLevel = 2.0;

            Assert.AreEqual(2.0, properties.CompartmentTwoBottomLevel);
        }

        [Test]
        public void CompartmentTwoStreetLevel_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoStreetLevel = 1.0;

            Assert.AreEqual(1.0, properties.CompartmentTwoStreetLevel);
        }

        [Test]
        public void CompartmentTwoFloodableArea_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoFloodableArea = 5.0;

            Assert.AreEqual(5.0, properties.CompartmentTwoFloodableArea);
        }

        [Test]
        public void CompartmentTwoStorageType_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoStorageType = CompartmentStorageType.Closed;

            Assert.AreEqual(CompartmentStorageType.Closed, properties.CompartmentTwoStorageType);
        }

        [Test]
        public void CompartmentTwoUseTable_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoUseTable = true;

            Assert.IsTrue(properties.CompartmentTwoUseTable);
        }

        [Test]
        public void CompartmentTwoInterpolationType_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoInterpolationType = InterpolationType.Linear;

            Assert.AreEqual(InterpolationType.Linear, properties.CompartmentTwoInterpolationType);
        }

        [Test]
        public void CompartmentTwoStorage_SetValidValue_ValueIsSet()
        {
            Function storageTable = CreateStorageTable();
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentTwoStorage = storageTable;

            Assert.AreEqual(storageTable, properties.CompartmentTwoStorage);
        }

        [Test]
        public void CompartmentThreeName_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeName = "TestCompartment";

            Assert.AreEqual("TestCompartment", properties.CompartmentThreeName);
        }

        [Test]
        public void CompartmentThreeBottomLevel_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeBottomLevel = 2.0;

            Assert.AreEqual(2.0, properties.CompartmentThreeBottomLevel);
        }

        [Test]
        public void CompartmentThreeStreetLevel_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeStreetLevel = 1.0;

            Assert.AreEqual(1.0, properties.CompartmentThreeStreetLevel);
        }

        [Test]
        public void CompartmentThreeFloodableArea_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeFloodableArea = 5.0;

            Assert.AreEqual(5.0, properties.CompartmentThreeFloodableArea);
        }

        [Test]
        public void CompartmentThreeStorageType_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeStorageType = CompartmentStorageType.Closed;

            Assert.AreEqual(CompartmentStorageType.Closed, properties.CompartmentThreeStorageType);
        }

        [Test]
        public void CompartmentThreeUseTable_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeUseTable = true;

            Assert.IsTrue(properties.CompartmentThreeUseTable);
        }

        [Test]
        public void CompartmentThreeInterpolationType_SetValidValue_ValueIsSet()
        {
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeInterpolationType = InterpolationType.Linear;

            Assert.AreEqual(InterpolationType.Linear, properties.CompartmentThreeInterpolationType);
        }

        [Test]
        public void CompartmentThreeStorage_SetValidValue_ValueIsSet()
        {
            Function storageTable = CreateStorageTable();
            ManholeProperties properties = CreatePropertiesWithManhole();

            properties.CompartmentThreeStorage = storageTable;

            Assert.AreEqual(storageTable, properties.CompartmentThreeStorage);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentOneProperties))]
        [TestCaseSource(nameof(GetCompartmentTwoProperties))]
        [TestCaseSource(nameof(GetCompartmentThreeProperties))]
        public void IsVisible_ManholeWithoutCompartments_ReturnsFalse(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(0);
            ManholeProperties properties = CreateProperties(manhole);

            bool visible = properties.IsVisible(propertyName);

            Assert.IsFalse(visible);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentOneProperties))]
        public void IsVisible_ManholeWithOneCompartmentAndCompartmentOneProperties_ReturnsTrue(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(1);
            ManholeProperties properties = CreateProperties(manhole);

            bool visible = properties.IsVisible(propertyName);

            Assert.IsTrue(visible);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentTwoProperties))]
        [TestCaseSource(nameof(GetCompartmentThreeProperties))]
        public void IsVisible_ManholeWithOneCompartmentAndCompartmentTwoThreeProperties_ReturnsFalse(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(1);
            ManholeProperties properties = CreateProperties(manhole);

            bool visible = properties.IsVisible(propertyName);

            Assert.IsFalse(visible);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentOneProperties))]
        [TestCaseSource(nameof(GetCompartmentTwoProperties))]
        public void IsVisible_ManholeWithTwoCompartmentsAndCompartmentOneTwoProperties_ReturnsTrue(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(2);
            ManholeProperties properties = CreateProperties(manhole);

            bool visible = properties.IsVisible(propertyName);

            Assert.IsTrue(visible);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentThreeProperties))]
        public void IsVisible_ManholeWithTwoCompartmentsAndCompartmentThreeProperties_ReturnsFalse(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(2);
            ManholeProperties properties = CreateProperties(manhole);

            bool visible = properties.IsVisible(propertyName);

            Assert.IsFalse(visible);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentOneProperties))]
        [TestCaseSource(nameof(GetCompartmentTwoProperties))]
        [TestCaseSource(nameof(GetCompartmentThreeProperties))]
        public void IsVisible_ManholeWithThreeCompartmentsAndCompartmentOneTwoThreeProperties_ReturnsTrue(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(3);
            ManholeProperties properties = CreateProperties(manhole);

            bool visible = properties.IsVisible(propertyName);

            Assert.IsTrue(visible);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentTypeDependentProperties))]
        public void IsVisible_ManHoleWithThreeCompartmentsAndCompartmentIsOutletCompartment_ReturnsFalse(string propertyName)
        {
            Manhole manhole = CreateManhole(CreateOutletCompartments(3).ToArray());
            ManholeProperties properties = CreateProperties(manhole);

            bool visible = properties.IsVisible(propertyName);

            Assert.IsFalse(visible);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentOneUseTableDependentProperties))]
        public void IsReadOnly_ManholeWithOneCompartmentAndUseTableIsFalse_ReturnsTrue(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(1);
            ManholeProperties properties = CreateProperties(manhole);

            properties.CompartmentOneUseTable = false;

            bool readOnly = properties.IsReadOnly(propertyName);

            Assert.IsTrue(readOnly);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentOneUseTableDependentProperties))]
        public void IsReadOnly_ManholeWithOneCompartmentAndUseTableIsTrue_ReturnsFalse(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(1);
            ManholeProperties properties = CreateProperties(manhole);

            properties.CompartmentOneUseTable = true;

            bool readOnly = properties.IsReadOnly(propertyName);

            Assert.IsFalse(readOnly);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentTwoUseTableDependentProperties))]
        public void IsReadOnly_ManholeWithTwoCompartmentsAndUseTableIsFalse_ReturnsTrue(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(2);
            ManholeProperties properties = CreateProperties(manhole);

            properties.CompartmentTwoUseTable = false;

            bool readOnly = properties.IsReadOnly(propertyName);

            Assert.IsTrue(readOnly);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentTwoUseTableDependentProperties))]
        public void IsReadOnly_ManholeWithTwoCompartmentsAndUseTableIsTrue_ReturnsFalse(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(2);
            ManholeProperties properties = CreateProperties(manhole);

            properties.CompartmentTwoUseTable = true;

            bool readOnly = properties.IsReadOnly(propertyName);

            Assert.IsFalse(readOnly);
        }

        [Test]
        [TestCaseSource(nameof(GetCompartmentThreeUseTableDependentProperties))]
        public void IsReadOnly_ManholeWithThreeCompartmentsAndUseTableIsTrue_ReturnsFalse(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(3);
            ManholeProperties properties = CreateProperties(manhole);

            properties.CompartmentThreeUseTable = true;

            bool readOnly = properties.IsReadOnly(propertyName);

            Assert.IsFalse(readOnly);
        }

        [TestCaseSource(nameof(GetCompartmentThreeUseTableDependentProperties))]
        public void IsReadOnly_ManholeWithThreeCompartmentsAndUseTableIsFalse_ReturnsTrue(string propertyName)
        {
            Manhole manhole = CreateManholeWithCompartments(3);
            ManholeProperties properties = CreateProperties(manhole);

            properties.CompartmentThreeUseTable = false;

            bool readOnly = properties.IsReadOnly(propertyName);

            Assert.IsTrue(readOnly);
        }

        private static ManholeProperties CreatePropertiesWithManhole()
        {
            Manhole manhole = CreateManholeWithCompartments(3);
            return CreateProperties(manhole);
        }

        private static ManholeProperties CreateProperties(Manhole manhole)
        {
            return new ManholeProperties { Data = manhole };
        }

        private static Manhole CreateManholeWithCompartments(int compartmentCount)
        {
            Compartment[] compartments = CreateCompartments(compartmentCount).ToArray();
            return CreateManhole(compartments);
        }

        private static Manhole CreateManhole<T>(params T[] compartments)
            where T : Compartment
        {
            var manhole = new Manhole();
            var network = new Network();
            manhole.Network = network;
            manhole.Compartments.AddRange(compartments);
            return manhole;
        }

        private static Function CreateStorageTable()
        {
            return new Function();
        }

        private static IEnumerable<Compartment> CreateCompartments(int count)
        {
            return Enumerable.Range(0, count).Select(_ => new Compartment());
        }

        private static IEnumerable<OutletCompartment> CreateOutletCompartments(int count)
        {
            return Enumerable.Range(0, count).Select(_ => new OutletCompartment());
        }

        private static IEnumerable<TestCaseData> GetCompartmentOneProperties()
        {
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneName));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneBottomLevel));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneStreetLevel));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneFloodableArea));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneStorageType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneUseTable));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneStorage));
        }

        private static IEnumerable<TestCaseData> GetCompartmentTwoProperties()
        {
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoName));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoBottomLevel));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoStreetLevel));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoFloodableArea));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoStorageType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoUseTable));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoStorage));
        }

        private static IEnumerable<TestCaseData> GetCompartmentThreeProperties()
        {
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeName));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeBottomLevel));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeStreetLevel));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeFloodableArea));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeStorageType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeUseTable));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeStorage));
        }

        private static IEnumerable<TestCaseData> GetCompartmentTypeDependentProperties()
        {
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneUseTable));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneStorage));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoUseTable));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoStorage));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeUseTable));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeStorage));
        }

        private static IEnumerable<TestCaseData> GetCompartmentOneUseTableDependentProperties()
        {
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentOneStorage));
        }

        private static IEnumerable<TestCaseData> GetCompartmentTwoUseTableDependentProperties()
        {
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentTwoStorage));
        }

        private static IEnumerable<TestCaseData> GetCompartmentThreeUseTableDependentProperties()
        {
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeInterpolationType));
            yield return new TestCaseData(nameof(ManholeProperties.CompartmentThreeStorage));
        }
    }
}