using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class GroupablePointFeatureRowTest
    {
        [Test]
        public void Constructor_WithNullFeature_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new GroupablePointFeatureRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsGroupablePointFeature()
        {
            // Arrange
            var feature = new GroupablePointFeature();
            var row = new GroupablePointFeatureRow(feature);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(feature));
        }

        [Test]
        public void WhenFeaturePropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var feature = new GroupablePointFeature();
            var row = new GroupablePointFeatureRow(feature);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            feature.GroupName = "some_group_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetGroupName_SetsFeatureGroupName()
        {
            // Arrange
            var feature = new GroupablePointFeature();
            var row = new GroupablePointFeatureRow(feature);

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(feature.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsFeatureGroupName()
        {
            // Arrange
            var feature = new GroupablePointFeature { GroupName = "some_group_name" };
            var row = new GroupablePointFeatureRow(feature);

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }

        [Test]
        public void GetX_GetsFeatureX()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var feature = new GroupablePointFeature { Geometry = point };
            var row = new GroupablePointFeatureRow(feature);

            // Act
            double result = row.X;

            // Assert
            Assert.That(result, Is.EqualTo(1.23));
        }

        [Test]
        public void GetY_GetsFeatureY()
        {
            // Arrange
            var point = Substitute.For<IPoint>();
            point.Coordinate.Returns(new Coordinate(1.23, 4.56));
            var feature = new GroupablePointFeature { Geometry = point };
            var row = new GroupablePointFeatureRow(feature);

            // Act
            double result = row.Y;

            // Assert
            Assert.That(result, Is.EqualTo(4.56));
        }
    }
}