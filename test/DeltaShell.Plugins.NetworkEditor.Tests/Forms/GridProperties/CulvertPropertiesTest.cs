using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid;
using DeltaShell.Plugins.NetworkEditor.Gui.Helpers;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.GridProperties
{
    [TestFixture]
    public class CulvertPropertiesTest
    {
        [Test]
        [NUnit.Framework.Category(TestCategory.WindowsForms)]
        public void ShowProperties()
        {
            WindowsFormsTestHelper.ShowPropertyGridForObject(new CulvertProperties { Data = new Culvert() });
        }

        [Test]
        public void CulvertPropertiesWithoutGroundLayer()
        {
            var culvertProperties = new CulvertProperties { Data = new Culvert() };
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

        [Test]
        public void CulvertPropertiesWithReadOnlyCulvertType()
        {
            var culvertProperties = new CulvertProperties { Data = new Culvert() };
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

            var propertyInfo = TypeUtils.GetPropertyInfo(culvertProperties.GetType(), nameof(culvertProperties.CulvertType));
            var displayNameAttribute = (DisplayNameAttribute)propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).SingleOrDefault();
            Assert.That(displayNameAttribute, Is.Not.Null);
            var culvertTypeGridItem = categories.Cast<GridItem>().Where(gi => gi.GridItemType == GridItemType.Category).SelectMany(gi => gi.GridItems.Cast<GridItem>()).SingleOrDefault(ge => string.Equals(ge.Label, displayNameAttribute.DisplayName, StringComparison.InvariantCultureIgnoreCase));
            var propertyDescriptor = TypeDescriptor.GetProperties(culvertTypeGridItem)["Value"];
            Assert.That(propertyDescriptor, Is.Not.Null);
            Assert.That(propertyDescriptor.Attributes, Is.Not.Null);
            Assert.That(propertyDescriptor.Attributes.Cast<Attribute>().Select(a => a.GetType()), Contains.Item(typeof(ReadOnlyAttribute)));
            var attribute = propertyDescriptor.Attributes[typeof(ReadOnlyAttribute)];
            var fieldInfo = attribute.GetType().GetField("isReadOnly", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.That(fieldInfo, Is.Not.Null);
            Assert.That(fieldInfo.GetValue(attribute), Is.True);
        }

        [TestCase(nameof(CulvertProperties.SiphonOnLevel))]
        [TestCase(nameof(CulvertProperties.SiphonOffLevel))]
        public void CulvertPropertiesWithoutSiphonProperties(string checkName)
        {
            var culvertProperties = new CulvertProperties { Data = new Culvert() };
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
            
            var cats = categories.Cast<GridItem>().Where(gi => gi.GridItemType == GridItemType.Category).SelectMany(gi => gi.GridItems.Cast<GridItem>()).Select(ge => ge.Label);
            var propertyInfo = TypeUtils.GetPropertyInfo(culvertProperties.GetType(), checkName);
            var displayNameAttribute = (DisplayNameAttribute)propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).SingleOrDefault();
            Assert.That(displayNameAttribute, Is.Not.Null);
            Assert.That(cats, Has.No.Member(displayNameAttribute.DisplayName));
        }

        [TestCase(nameof(Culvert.GroundLayerEnabled))]
        [TestCase(nameof(Culvert.GroundLayerRoughness))]
        [TestCase(nameof(Culvert.GroundLayerThickness))]
        [TestCase(nameof(Culvert.BendLossCoefficient))]
        [TestCase(nameof(Culvert.SiphonOnLevel))]
        [TestCase(nameof(Culvert.SiphonOffLevel))]
        public void CulvertWithout_Property_Check(string checkName)
        {
            var culvert = new Culvert() ;
            var grid = new PropertyGrid()
            {
                SelectedObject = culvert
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
            var propertyInfo = TypeUtils.GetPropertyInfo(culvert.GetType(), checkName);
            var displayNameAttribute = (DisplayNameAttribute)propertyInfo.GetCustomAttributes(typeof(DisplayNameAttribute), false).SingleOrDefault();
            Assert.That(displayNameAttribute, Is.Not.Null);
            Assert.That(cats, Has.No.Member(displayNameAttribute.DisplayName));
        }

        [TestCase(CulvertType.Culvert, nameof(CulvertProperties.BendLossCoefficient), true)]
        [TestCase(CulvertType.Siphon, nameof(CulvertProperties.BendLossCoefficient), true)]
        [TestCase(CulvertType.InvertedSiphon, nameof(CulvertProperties.BendLossCoefficient), false)]
        [TestCase(CulvertType.Culvert, nameof(CulvertProperties.AllowNegativeFlow), false)]
        [TestCase(CulvertType.Siphon, nameof(CulvertProperties.AllowNegativeFlow), true)]
        [TestCase(CulvertType.InvertedSiphon, nameof(CulvertProperties.AllowNegativeFlow), false)]
        public void IsReadOnly_IsCorrect(CulvertType culvertType, string propertyName, bool expReadOnly)
        {
            // Setup
            var properties = new CulvertProperties
            {
                Data = new Culvert(),
                CulvertType = culvertType
            };

            // Call
            bool isReadOnly = properties.IsReadOnly(propertyName);

            // Assert
            Assert.That(isReadOnly, Is.EqualTo(expReadOnly));
        }
    }
}