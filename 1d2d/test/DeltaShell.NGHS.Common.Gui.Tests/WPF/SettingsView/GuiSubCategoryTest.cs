using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.NGHS.Common.Gui.WPF.SettingsView;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.WPF.SettingsView
{
    [TestFixture]
    public class GuiSubCategoryTest
    {
        [Test]
        public void Test_WpfGuiSubCategory()
        {
            var subCategory = new GuiSubCategory("dummyName", null);
            Assert.IsNotNull(subCategory);
            Assert.IsNotNull(subCategory.Properties);
            Assert.IsFalse(subCategory.IsVisible);
        }

        [Test]
        public void Test_WpfGuiSubCategory_WithProperties()
        {
            var fieldUiDescriptions = new List<FieldUIDescription>();
            var fieldUiDescription = new FieldUIDescription(null, null)
            {
                Label = "dummyName",
            };
            fieldUiDescriptions.Add(fieldUiDescription);
            var subCategory = new GuiSubCategory("dummySubCateogry", fieldUiDescriptions);
            Assert.IsNotNull(subCategory);
            Assert.IsNotNull(subCategory.Properties);
            Assert.IsTrue(subCategory.Properties.Any( p => p.Label.Equals(fieldUiDescription.Label)));
        }
    }
}