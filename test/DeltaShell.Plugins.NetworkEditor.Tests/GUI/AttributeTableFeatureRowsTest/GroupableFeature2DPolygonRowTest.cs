using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class GroupableFeature2DPolygonRowTest
    {
        [Test]
        public void Constructor_WithNullFeature_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new GroupableFeature2DPolygonRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsGroupableFeature2DPolygon()
        {
            // Arrange
            var feature = new GroupableFeature2DPolygon();
            var row = new GroupableFeature2DPolygonRow(feature);

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
            var feature = new GroupableFeature2DPolygon();
            var row = new GroupableFeature2DPolygonRow(feature);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            feature.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetGroupName_SetsFeatureGroupName()
        {
            // Arrange
            var feature = new GroupableFeature2DPolygon();
            var row = new GroupableFeature2DPolygonRow(feature);

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(feature.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsFeatureGroupName()
        {
            // Arrange
            var feature = new GroupableFeature2DPolygon { GroupName = "some_group_name" };
            var row = new GroupableFeature2DPolygonRow(feature);

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }

        [Test]
        public void SetName_SetsFeatureName()
        {
            // Arrange
            var feature = new GroupableFeature2DPolygon();
            var row = new GroupableFeature2DPolygonRow(feature);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(feature.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsFeatureName()
        {
            // Arrange
            var feature = new GroupableFeature2DPolygon { Name = "some_name" };
            var row = new GroupableFeature2DPolygonRow(feature);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
    }
}