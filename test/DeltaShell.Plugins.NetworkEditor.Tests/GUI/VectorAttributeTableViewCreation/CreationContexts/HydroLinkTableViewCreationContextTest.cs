using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui.AttributeTableFeatureRows;
using DeltaShell.Plugins.NetworkEditor.Gui.VectorAttributeTableViewCreation.CreationContexts;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.GUI.VectorAttributeTableViewCreation.CreationContexts
{
    [TestFixture]
    public class HydroLinkTableViewCreationContextTest
    {
        [Test]
        public void GetDescription_ReturnsCorrectString()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();

            // Act
            string result = creationContext.GetDescription();

            // Assert
            Assert.That(result, Is.EqualTo("Hydro link table view"));
        }

        [Test]
        public void IsRegionData_WithNullRegion_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(null, Substitute.For<IEnumerable<HydroLink>>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WithNullData_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.IsRegionData(Substitute.For<IHydroRegion>(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void IsRegionData_WhenRegionDoesNotContainLinks_ReturnsFalse()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();
            var region = Substitute.For<IHydroRegion>();
            region.Links.Returns(new EventedList<HydroLink>(new HydroLink[3]));

            // Act
            bool result = creationContext.IsRegionData(region, new HydroLink[3]);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsRegionData_WhenRegionContainsLinks_ReturnsTrue()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();
            var region = Substitute.For<IHydroRegion>();
            region.Links.Returns(new EventedList<HydroLink>(new HydroLink[3]));

            // Act
            bool result = creationContext.IsRegionData(region, region.Links);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void CreateFeatureRowObject_WhenFeatureNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(null, Enumerable.Empty<HydroLink>());
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ALlFeaturesNull_ThrowsArgumentNullException()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CreateFeatureRowObject(new HydroLink(), null);
            }

            // Assert
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void CreateFeatureRowObject_ReturnsNewRow()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();
            HydroLink feature = Substitute.For<HydroLink, INotifyPropertyChanged>();

            // Act
            HydroLinkRow result = creationContext.CreateFeatureRowObject(feature, Enumerable.Empty<HydroLink>());

            // Assert
            Assert.That(result, Is.Not.Null);
        }

        [Test]
        public void CustomizeTableViews_DoesNothing()
        {
            // Arrange
            var creationContext = new HydroLinkTableViewCreationContext();

            // Act
            void Call()
            {
                creationContext.CustomizeTableView(null, null, null);
            }

            // Assert
            Assert.That(Call, Throws.Nothing);
        }
    }
}