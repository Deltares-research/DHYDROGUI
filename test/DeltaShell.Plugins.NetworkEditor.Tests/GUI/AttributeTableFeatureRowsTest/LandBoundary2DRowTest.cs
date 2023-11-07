using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class LandBoundary2DRowTest
    {
        [Test]
        public void Constructor_WithNullLandBoundary2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new LandBoundary2DRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsLandBoundary2D()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(landBoundary2D));
        }

        [Test]
        public void WhenLandBoundary2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            landBoundary2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsLandBoundary2DName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(landBoundary2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsLandBoundary2DName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D { Name = "some_name" };
            var row = new LandBoundary2DRow(landBoundary2D);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void SetGroupName_SetsLandBoundary2DGroupName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D();
            var row = new LandBoundary2DRow(landBoundary2D);

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(landBoundary2D.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsLandBoundary2DGroupName()
        {
            // Arrange
            var landBoundary2D = new LandBoundary2D { GroupName = "some_group_name" };
            var row = new LandBoundary2DRow(landBoundary2D);

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }
    }
}