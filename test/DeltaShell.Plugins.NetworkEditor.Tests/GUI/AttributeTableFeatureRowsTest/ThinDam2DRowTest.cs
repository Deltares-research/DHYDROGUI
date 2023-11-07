using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class ThinDam2DRowTest
    {
        [Test]
        public void Constructor_WithNullThinDam2D_ThrowsArgumentNullException()
        {
            // Act
            void Call() => new ThinDam2DRow(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsThinDam2D()
        {
            // Arrange
            var thinDam2D = new ThinDam2D();
            var row = new ThinDam2DRow(thinDam2D);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(thinDam2D));
        }

        [Test]
        public void WhenThinDam2DPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var thinDam2D = new ThinDam2D();
            var row = new ThinDam2DRow(thinDam2D);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            thinDam2D.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }
        
        [Test]
        public void SetName_SetsThinDam2DName()
        {
            // Arrange
            var thinDam2D = new ThinDam2D();
            var row = new ThinDam2DRow(thinDam2D);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(thinDam2D.Name, Is.EqualTo("some_name"));
        }
        
        [Test]
        public void GetName_GetsThinDam2DName()
        {
            // Arrange
            var thinDam2D = new ThinDam2D { Name = "some_name" };
            var row = new ThinDam2DRow(thinDam2D);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
        
        [Test]
        public void SetGroupName_SetsThinDam2DGroupName()
        {
            // Arrange
            var thinDam2D = new ThinDam2D();
            var row = new ThinDam2DRow(thinDam2D);

            // Act
            row.GroupName = "some_name";

            // Assert
            Assert.That(thinDam2D.GroupName, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetGroupName_GetsThinDam2DGroupName()
        {
            // Arrange
            var thinDam2D = new ThinDam2D { GroupName = "some_name" };
            var row = new ThinDam2DRow(thinDam2D);

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
    }
}