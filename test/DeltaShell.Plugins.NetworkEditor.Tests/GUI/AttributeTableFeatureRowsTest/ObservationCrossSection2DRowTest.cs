using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class ObservationCrossSection2DRowTest
    {
        [Test]
        public void Constructor_WithNullObservationCrossSection2D_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new ObservationCrossSection2DRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsObservationCrossSection2D()
        {
            // Arrange
            var observationCrossSection2D = new ObservationCrossSection2D();
            var row = new ObservationCrossSection2DRow(observationCrossSection2D);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(observationCrossSection2D));
        }

        [Test]
        public void WhenObservationCrossSection2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var observationCrossSection2D = new ObservationCrossSection2D();
            var row = new ObservationCrossSection2DRow(observationCrossSection2D);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            observationCrossSection2D.GroupName = "some_group_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetGroupName_SetsObservationCrossSection2DGroupName()
        {
            // Arrange
            var observationCrossSection2D = new ObservationCrossSection2D();
            var row = new ObservationCrossSection2DRow(observationCrossSection2D);

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(observationCrossSection2D.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsObservationCrossSection2DGroupName()
        {
            // Arrange
            var observationCrossSection2D = new ObservationCrossSection2D { GroupName = "some_group_name" };
            var row = new ObservationCrossSection2DRow(observationCrossSection2D);

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }

        [Test]
        public void SetName_SetsObservationCrossSection2DName()
        {
            // Arrange
            var observationCrossSection2D = new ObservationCrossSection2D();
            var row = new ObservationCrossSection2DRow(observationCrossSection2D);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(observationCrossSection2D.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsObservationCrossSection2DName()
        {
            // Arrange
            var observationCrossSection2D = new ObservationCrossSection2D { Name = "some_name" };
            var row = new ObservationCrossSection2DRow(observationCrossSection2D);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
    }
}