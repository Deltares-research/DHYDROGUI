using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class Feature2DRowTest
    {
        [Test]
        public void Constructor_WithNullFeature2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new Feature2DRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsFeature2D()
        {
            // Arrange
            var feature2D = new Feature2D();
            var row = new Feature2DRow(feature2D);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(feature2D));
        }

        [Test]
        public void WhenFeature2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var feature2D = new Feature2D();
            var row = new Feature2DRow(feature2D);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            feature2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsFeature2DName()
        {
            // Arrange
            var feature2D = new Feature2D();
            var row = new Feature2DRow(feature2D);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(feature2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsFeature2DName()
        {
            // Arrange
            var feature2D = new Feature2D { Name = "some_name" };
            var row = new Feature2DRow(feature2D);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
    }
}