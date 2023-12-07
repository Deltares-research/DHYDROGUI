using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Validation.Common;
using DelftTools.Utils.Validation.NameValidation;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class OutletCompartmentRowTest
    {
        [Test]
        public void Constructor_WithNullOutletCompartment_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new OutletCompartmentRow(null, new NameValidator());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void Constructor_WithNullNameValidator_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new OutletCompartmentRow(new OutletCompartment(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsOutletCompartment()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(outletCompartment));
        }

        [Test]
        public void WhenOutletCompartmentPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            outletCompartment.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsOutletCompartmentName()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(outletCompartment.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsOutletCompartmentName()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { Name = "some_name" };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var outletCompartment = new OutletCompartment { Name = "some_name" };
            var row = new OutletCompartmentRow(outletCompartment, nameValidator);

            // Act
            row.Name = "some_invalid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);
            var nameValidator = new NameValidator();
            nameValidator.AddValidator(validator);

            var outletCompartment = new OutletCompartment { Name = "some_name" };
            var row = new OutletCompartmentRow(outletCompartment, nameValidator);

            // Act
            row.Name = "some_valid_name";

            // Assert
            Assert.That(row.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void SetShape_SetsOutletCompartmentShape()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.Shape = CompartmentShape.Rectangular;

            // Assert
            Assert.That(outletCompartment.Shape, Is.EqualTo(CompartmentShape.Rectangular));
        }

        [Test]
        public void GetShape_GetsOutletCompartmentShape()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { Shape = CompartmentShape.Rectangular };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            CompartmentShape result = row.Shape;

            // Assert
            Assert.That(result, Is.EqualTo(CompartmentShape.Rectangular));
        }

        [Test]
        public void SetCompartmentStorageType_SetsOutletCompartmentCompartmentStorageType([Values] CompartmentStorageType compartmentStorageType)
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.CompartmentStorageType = compartmentStorageType;

            // Assert
            Assert.That(outletCompartment.CompartmentStorageType, Is.EqualTo(compartmentStorageType));
        }

        [Test]
        public void GetCompartmentStorageType_GetsOutletCompartmentCompartmentStorageType([Values] CompartmentStorageType compartmentStorageType)
        {
            // Arrange
            var outletCompartment = new OutletCompartment { CompartmentStorageType = compartmentStorageType };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            CompartmentStorageType result = row.CompartmentStorageType;

            // Assert
            Assert.That(result, Is.EqualTo(compartmentStorageType));
        }

        [Test]
        public void SetLength_SetsOutletCompartmentLength()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.Length = 10.5;

            // Assert
            Assert.That(outletCompartment.ManholeLength, Is.EqualTo(10.5));
        }

        [Test]
        public void GetLength_GetsOutletCompartmentLength()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { ManholeLength = 8.2 };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            double result = row.Length;

            // Assert
            Assert.That(result, Is.EqualTo(8.2));
        }

        // ... (previous test methods)

        [Test]
        public void SetWidth_SetsOutletCompartmentWidth()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.Width = 3.5;

            // Assert
            Assert.That(outletCompartment.ManholeWidth, Is.EqualTo(3.5));
        }

        [Test]
        public void GetWidth_GetsOutletCompartmentWidth()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { ManholeWidth = 4.2 };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            double result = row.Width;

            // Assert
            Assert.That(result, Is.EqualTo(4.2));
        }

        [Test]
        public void SetFloodableArea_SetsOutletCompartmentFloodableArea()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.FloodableArea = 15.7;

            // Assert
            Assert.That(outletCompartment.FloodableArea, Is.EqualTo(15.7));
        }

        [Test]
        public void GetFloodableArea_GetsOutletCompartmentFloodableArea()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { FloodableArea = 13.4 };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            double result = row.FloodableArea;

            // Assert
            Assert.That(result, Is.EqualTo(13.4));
        }

        [Test]
        public void SetBottomLevel_SetsOutletCompartmentBottomLevel()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.BottomLevel = 2.5;

            // Assert
            Assert.That(outletCompartment.BottomLevel, Is.EqualTo(2.5));
        }

        [Test]
        public void GetBottomLevel_GetsOutletCompartmentBottomLevel()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { BottomLevel = 1.3 };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            double result = row.BottomLevel;

            // Assert
            Assert.That(result, Is.EqualTo(1.3));
        }

        [Test]
        public void SetSurfaceLevel_SetsOutletCompartmentSurfaceLevel()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.SurfaceLevel = 7.1;

            // Assert
            Assert.That(outletCompartment.SurfaceLevel, Is.EqualTo(7.1));
        }

        [Test]
        public void GetSurfaceLevel_GetsOutletCompartmentSurfaceLevel()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { SurfaceLevel = 9.0 };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            double result = row.SurfaceLevel;

            // Assert
            Assert.That(result, Is.EqualTo(9.0));
        }

        [Test]
        public void SetUseStorageTable_SetsOutletCompartmentUseStorageTable()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.UseStorageTable = true;

            // Assert
            Assert.That(outletCompartment.UseTable, Is.True);
        }

        [Test]
        public void GetUseStorageTable_GetsOutletCompartmentUseStorageTable()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { UseTable = true };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            bool result = row.UseStorageTable;

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void SetStorage_SetsOutletCompartmentStorage()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());
            var storageFunction = new Function();

            // Act
            row.Storage = storageFunction;

            // Assert
            Assert.That(outletCompartment.Storage, Is.SameAs(storageFunction));
        }

        [Test]
        public void GetStorage_GetsOutletCompartmentStorage()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { Storage = new Function() };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            IFunction result = row.Storage;

            // Assert
            Assert.That(result, Is.SameAs(outletCompartment.Storage));
        }

        [Test]
        public void SetInterpolationType_SetsOutletCompartmentInterpolationType([Values] InterpolationType interpolationType)
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.InterpolationType = interpolationType;

            // Assert
            Assert.That(outletCompartment.InterpolationType, Is.EqualTo(interpolationType));
        }

        [Test]
        public void GetInterpolationType_GetsOutletCompartmentInterpolationType([Values] InterpolationType interpolationType)
        {
            // Arrange
            var outletCompartment = new OutletCompartment { InterpolationType = interpolationType };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            InterpolationType result = row.InterpolationType;

            // Assert
            Assert.That(result, Is.EqualTo(interpolationType));
        }

        [Test]
        public void SetSurfaceWaterLevel_SetsOutletCompartmentSurfaceWaterLevel()
        {
            // Arrange
            var outletCompartment = new OutletCompartment();
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            row.SurfaceWaterLevel = 6.2;

            // Assert
            Assert.That(outletCompartment.SurfaceWaterLevel, Is.EqualTo(6.2));
        }

        [Test]
        public void GetSurfaceWaterLevel_GetsOutletCompartmentSurfaceWaterLevel()
        {
            // Arrange
            var outletCompartment = new OutletCompartment { SurfaceWaterLevel = 4.8 };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            double result = row.SurfaceWaterLevel;

            // Assert
            Assert.That(result, Is.EqualTo(4.8));
        }

        [Test]
        public void GetManholeName_GetsOutletCompartmentManholeName()
        {
            // Arrange
            var manhole = Substitute.For<IManhole>();
            manhole.Name = "some_manhole_name";
            var outletCompartment = new OutletCompartment { ParentManhole = manhole };
            var row = new OutletCompartmentRow(outletCompartment, new NameValidator());

            // Act
            string result = row.ManholeName;

            // Assert
            Assert.That(result, Is.EqualTo("some_manhole_name"));
        }
    }
}