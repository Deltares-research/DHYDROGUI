using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class FixedWeirRowTest
    {
        [Test]
        public void Constructor_WithNullFixedWeir_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new FixedWeirRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsFixedWeir()
        {
            // Arrange
            var fixedWeir = new FixedWeir();
            var row = new FixedWeirRow(fixedWeir);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(fixedWeir));
        }

        [Test]
        public void WhenFixedWeirPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var fixedWeir = new FixedWeir();
            var row = new FixedWeirRow(fixedWeir);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            fixedWeir.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetGroupName_SetsFixedWeirGroupName()
        {
            // Arrange
            var fixedWeir = new FixedWeir();
            var row = new FixedWeirRow(fixedWeir);

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(fixedWeir.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsFixedWeirGroupName()
        {
            // Arrange
            var fixedWeir = new FixedWeir { GroupName = "some_group_name" };
            var row = new FixedWeirRow(fixedWeir);

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }

        [Test]
        public void SetName_SetsFixedWeirName()
        {
            // Arrange
            var fixedWeir = new FixedWeir();
            var row = new FixedWeirRow(fixedWeir);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(fixedWeir.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsFixedWeirName()
        {
            // Arrange
            var fixedWeir = new FixedWeir { Name = "some_name" };
            var row = new FixedWeirRow(fixedWeir);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
    }
}