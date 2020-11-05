using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
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

        [TestCase(nameof(Culvert.GroundLayerEnabled))]
        [TestCase(nameof(Culvert.GroundLayerRoughness))]
        [TestCase(nameof(Culvert.GroundLayerThickness))]
        public void CulvertWithoutGroundLayer_MDE_Check(string checkName)
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
    }
}