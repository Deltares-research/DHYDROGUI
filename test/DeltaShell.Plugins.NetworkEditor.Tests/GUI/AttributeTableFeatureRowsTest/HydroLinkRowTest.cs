using DelftTools.Hydro;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using GeoAPI.Extensions.Feature;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.AttributeTableFeatureRowsTest
{
    [TestFixture]
    public class HydroLinkRowTest
    {
        [Test]
        public void Constructor_WithNullHydroLink_ThrowsArgumentNullException()
        {
            // Act
            void Call()
            {
                new HydroLinkRow(null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void GetFeature_GetsHydroLink()
        {
            // Arrange
            var hydroLink = new HydroLink();
            var row = new HydroLinkRow(hydroLink);

            // Act
            IFeature result = row.GetFeature();

            // Assert
            Assert.That(result, Is.SameAs(hydroLink));
        }

        [Test]
        public void WhenHydroLinkPropertyChanged_RowShouldFirePropertyChangedEvent()
        {
            // Arrange
            var eventRaised = false;
            var hydroLink = new HydroLink();
            var row = new HydroLinkRow(hydroLink);
            row.PropertyChanged += (sender, args) => eventRaised = true;

            // Act
            hydroLink.Name = "some_name";

            // Assert
            Assert.That(eventRaised);
        }

        [Test]
        public void SetName_SetsHydroLinkName()
        {
            // Arrange
            var hydroLink = new HydroLink();
            var row = new HydroLinkRow(hydroLink);

            // Act
            row.Name = "some_name";

            // Assert
            Assert.That(hydroLink.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void GetName_GetsHydroLinkName()
        {
            // Arrange
            var hydroLink = new HydroLink { Name = "some_name" };
            var row = new HydroLinkRow(hydroLink);

            // Act
            string result = row.Name;

            // Assert
            Assert.AreEqual(result, "some_name");
        }

        [Test]
        public void GetSource_GetsHydroLinkSourceName()
        {
            // Arrange
            var hydroObject = Substitute.For<IHydroObject>();
            hydroObject.Name = "some_node_name";
            var hydroLink = new HydroLink { Source = hydroObject };
            var row = new HydroLinkRow(hydroLink);

            // Act
            string result = row.Source;

            // Assert
            Assert.AreEqual(result, "some_node_name");
        }

        [Test]
        public void GetTarget_GetsHydroLinkTargetName()
        {
            // Arrange
            var hydroObject = Substitute.For<IHydroObject>();
            hydroObject.Name = "some_node_name";
            var hydroLink = new HydroLink { Target = hydroObject };
            var row = new HydroLinkRow(hydroLink);

            // Act
            string result = row.Target;

            // Assert
            Assert.AreEqual(result, "some_node_name");
        }
    }
}