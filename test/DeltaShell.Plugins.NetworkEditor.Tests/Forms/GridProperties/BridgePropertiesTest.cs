using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using Deltares.Infrastructure.API.Validation;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class BridgePropertiesTest
    {
        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new BridgeProperties { Data = new Bridge() });
        }

        [Test]
        public void SetName_InvalidName_OriginalNameIsPreserved()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_invalid_name").Returns(ValidationResult.Fail("message"));

            var data = new Bridge { Name = "some_name" };
            var properties = new BridgeProperties { Data = data };
            properties.NameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_invalid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_name"));
        }

        [Test]
        public void SetName_ValidName_NameIsUpdated()
        {
            // Arrange
            var validator = Substitute.For<IValidator<string>>();
            validator.Validate("some_valid_name").Returns(ValidationResult.Success);

            var data = new Bridge { Name = "some_name" };
            var properties = new BridgeProperties { Data = data };
            properties.NameValidator.AddValidator(validator);

            // Act
            properties.Name = "some_valid_name";

            // Assert
            Assert.That(properties.Name, Is.EqualTo("some_valid_name"));
        }

        [Test]
        public void BridgePropertiesWithoutGroundLayer()
        {
            var culvertProperties = new BridgeProperties { Data = new Bridge() };
            var grid = new PropertyGrid()
            {
                SelectedObject = culvertProperties
            };
            GridItemCollection categories;
            if (grid.SelectedGridItem.GridItemType == GridItemType.Category)
            {
                categories = grid.SelectedGridItem.Parent.GridItems;
            }
            else
            {
                categories = grid.SelectedGridItem.Parent.Parent.GridItems;
            }

            var cats = categories.Cast<GridItem>().Select(ge => ge.Label);
            Assert.That(cats, Has.No.Member(PropertyWindowCategoryHelper.GroundLayerCategory));
        }

        [TestCase(nameof(Bridge.GroundLayerEnabled))]
        [TestCase(nameof(Bridge.GroundLayerRoughness))]
        [TestCase(nameof(Bridge.GroundLayerThickness))]
        public void BridgeWithoutGroundLayer_MDE_Check(string checkName)
        {
            var bridge = new Bridge();
            var grid = new PropertyGrid()
            {
                SelectedObject = bridge
            };
            GridItemCollection categories;
            if (grid.SelectedGridItem.GridItemType == GridItemType.Category)
            {
                categories = grid.SelectedGridItem.Parent.GridItems;
            }
            else
            {
                categories = grid.SelectedGridItem.Parent.Parent.GridItems;
            }

            var cats = categories.Cast<GridItem>().Where(gi => gi.GridItemType == GridItemType.Category).SelectMany(gi => gi.GridItems.Cast<GridItem>()).Select(ge => ge.Label);
            var propertyInfo = TypeUtils.GetPropertyInfo(bridge.GetType(), checkName);
            var displayNameAttribute = (DisplayNameAttribute)propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).SingleOrDefault();
            Assert.That(displayNameAttribute, Is.Not.Null);
            Assert.That(cats, Has.No.Member(displayNameAttribute.DisplayName));
        }
    }
}