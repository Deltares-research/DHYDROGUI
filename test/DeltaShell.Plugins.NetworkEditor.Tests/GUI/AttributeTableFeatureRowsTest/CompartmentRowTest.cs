using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class CompartmentRowTest
    {
        [Test]
        public void Constructor_WithNullCompartment_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new CompartmentRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsCompartment()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(compartment));
        }

        [Test]
        public void WhenCompartmentPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            compartment.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void GetManholeName_GetsCompartmentManholeName()
        {
            // Arrange
            var manhole = Substitute.For<IManhole>();
            manhole.Name = "some_manhole_name";
            var compartment = new Compartment { ParentManhole = manhole };
            var row = new CompartmentRow(compartment);

            // Act
            string result = row.ManholeName;

            // Assert
            Assert.That(result, Is.EqualTo("some_manhole_name"));
        }

        [Test]
        public void SetName_SetsCompartmentName()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(compartment.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsCompartmentName()
        {
            // Arrange
            var compartment = new Compartment { Name = "some_name" };
            var row = new CompartmentRow(compartment);

            // Act
            string result = row.Name;

            // Assert
            Assert.That(result, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetShape_SetsCompartmentName([Values] CompartmentShape compartmentShape)
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.Shape = compartmentShape;

            // Assert
            Assert.That(compartment.Shape, Is.EqualTo(compartmentShape));
        }

        [Test]
        public void GetShape_GetsCompartmentName([Values] CompartmentShape compartmentShape)
        {
            // Arrange
            var compartment = new Compartment { Shape = compartmentShape };
            var row = new CompartmentRow(compartment);

            // Act
            CompartmentShape result = row.Shape;

            // Assert
            Assert.That(result, Is.EqualTo(compartmentShape));
        }

        [Test]
        public void SetShape_SetsCompartmentCompartmentStorageType([Values] CompartmentStorageType compartmentStorageType)
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.CompartmentStorageType = compartmentStorageType;

            // Assert
            Assert.That(compartment.CompartmentStorageType, Is.EqualTo(compartmentStorageType));
        }

        [Test]
        public void GetShape_GetsCompartmentName([Values] CompartmentStorageType compartmentStorageType)
        {
            // Arrange
            var compartment = new Compartment { CompartmentStorageType = compartmentStorageType };
            var row = new CompartmentRow(compartment);

            // Act
            CompartmentStorageType result = row.CompartmentStorageType;

            // Assert
            Assert.That(result, Is.EqualTo(compartmentStorageType));
        }

        [Test]
        public void SetLength_SetsCompartmentManholeLength()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.Length = 1.23;

            // Assert
            Assert.That(compartment.ManholeLength, Is.EqualTo(1.23));
        }

        [Test]
        public void GetLength_GetsCompartmentManholeLength()
        {
            // Arrange
            var compartment = new Compartment { ManholeLength = 1.23 };
            var row = new CompartmentRow(compartment);

            // Act
            double result = row.Length;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetWidth_SetsCompartmentManholeWidth()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.Width = 1.23;

            // Assert
            Assert.That(compartment.ManholeWidth, Is.EqualTo(1.23));
        }

        [Test]
        public void GetWidth_GetsCompartmentManholeWidth()
        {
            // Arrange
            var compartment = new Compartment { ManholeWidth = 1.23 };
            var row = new CompartmentRow(compartment);

            // Act
            double result = row.Width;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetFloodableArea_SetsCompartmentFloodableArea()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.FloodableArea = 1.23;

            // Assert
            Assert.That(compartment.FloodableArea, Is.EqualTo(1.23));
        }

        [Test]
        public void GetFloodableArea_GetsCompartmentFloodableArea()
        {
            // Arrange
            var compartment = new Compartment { FloodableArea = 1.23 };
            var row = new CompartmentRow(compartment);

            // Act
            double result = row.FloodableArea;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetBottomLevel_SetsCompartmentBottomLevel()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.BottomLevel = 1.23;

            // Assert
            Assert.That(compartment.BottomLevel, Is.EqualTo(1.23));
        }

        [Test]
        public void GetBottomLevel_GetsCompartmentBottomLevel()
        {
            // Arrange
            var compartment = new Compartment { BottomLevel = 1.23 };
            var row = new CompartmentRow(compartment);

            // Act
            double result = row.BottomLevel;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetSurfaceLevel_SetsCompartmentSurfaceLevel()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.SurfaceLevel = 1.23;

            // Assert
            Assert.That(compartment.SurfaceLevel, Is.EqualTo(1.23));
        }

        [Test]
        public void GetSurfaceLevel_GetsCompartmentSurfaceLevel()
        {
            // Arrange
            var compartment = new Compartment { SurfaceLevel = 1.23 };
            var row = new CompartmentRow(compartment);

            // Act
            double result = row.SurfaceLevel;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void SetUseStorageTable_SetsCompartmentUseStorageTable([Values] bool useStorageTable)
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.UseStorageTable = useStorageTable;

            // Assert
            Assert.That(compartment.UseTable, Is.EqualTo(useStorageTable));
        }

        [Test]
        public void GetUseStorageTable_GetsCompartmentUseTable([Values] bool useStorageTable)
        {
            // Arrange
            var compartment = new Compartment { UseTable = useStorageTable };
            var row = new CompartmentRow(compartment);

            // Act
            bool result = row.UseStorageTable;

            // Assert
            Assert.That(result, Is.EqualTo(useStorageTable));
        }

        [Test]
        public void SetStorage_SetsCompartmentStorage()
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);
            var function = Substitute.For<IFunction>();

            // Act
            row.Storage = function;

            // Assert
            Assert.That(compartment.Storage, Is.SameAs(function));
        }

        [Test]
        public void GetStorage_GetsCompartmentStorage()
        {
            // Arrange
            var function = Substitute.For<IFunction>();
            var compartment = new Compartment { Storage = function };
            var row = new CompartmentRow(compartment);

            // Act
            IFunction result = row.Storage;

            // Assert
            Assert.That(result, Is.SameAs(function));
        }

        [Test]
        public void SetInterpolationType_SetsCompartmentInterpolationType([Values] InterpolationType interpolationType)
        {
            // Arrange
            var compartment = new Compartment();
            var row = new CompartmentRow(compartment);

            // Act
            row.InterpolationType = interpolationType;

            // Assert
            Assert.That(compartment.InterpolationType, Is.EqualTo(interpolationType));
        }

        [Test]
        public void GetInterpolationType_GetsCompartmentInterpolationType([Values] InterpolationType interpolationType)
        {
            // Arrange
            var compartment = new Compartment { InterpolationType = interpolationType };
            var row = new CompartmentRow(compartment);

            // Act
            InterpolationType result = row.InterpolationType;

            // Assert
            Assert.That(result, Is.EqualTo(interpolationType));
        }
    }
}