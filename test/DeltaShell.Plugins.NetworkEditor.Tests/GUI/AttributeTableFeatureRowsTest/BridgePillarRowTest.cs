using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class BridgePillarRowTest
    {
        [Test]
        public void Constructor_WithNullBridgePillar_ThrowsArgumentNullException()
        {
            // Act
            void Call() => new BridgePillarRow(null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsBridgePillar()
        {
            // Arrange
            var bridgePillar = new BridgePillar();
            var row = new BridgePillarRow(bridgePillar);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(bridgePillar));
        }

        [Test]
        public void WhenBridgePillarPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var bridgePillar = new BridgePillar();
            var row = new BridgePillarRow(bridgePillar);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            bridgePillar.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetGroupName_SetsBridgePillarGroupName()
        {
            // Arrange
            var bridgePillar = new BridgePillar();
            var row = new BridgePillarRow(bridgePillar);

            // Act
            row.GroupName = "some_group_name";

            // Assert
            Assert.That(bridgePillar.GroupName, Is.EqualTo("some_group_name"));
        }

        [Test]
        public void GetGroupName_GetsBridgePillarGroupName()
        {
            // Arrange
            var bridgePillar = new BridgePillar { GroupName = "some_group_name" };
            var row = new BridgePillarRow(bridgePillar);

            // Act
            string result = row.GroupName;

            // Assert
            Assert.AreEqual(result, "some_group_name");
        }

        [Test]
        public void SetName_SetsBridgePillarName()
        {
            // Arrange
            var bridgePillar = new BridgePillar();
            var row = new BridgePillarRow(bridgePillar);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(bridgePillar.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsBridgePillarName()
        {
            // Arrange
            var bridgePillar = new BridgePillar { Name = "some_name" };
            var row = new BridgePillarRow(bridgePillar);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }
    }
}