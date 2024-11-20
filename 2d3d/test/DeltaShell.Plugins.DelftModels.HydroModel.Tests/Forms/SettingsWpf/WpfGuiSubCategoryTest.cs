using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class WpfGuiSubCategoryTest
    {
        [Test]
        public void Test_WpfGuiSubCategory()
        {
            var subCategory = new WpfGuiSubCategory("dummyName", null);
            Assert.IsNotNull(subCategory);
            Assert.IsNotNull(subCategory.Properties);
            Assert.IsFalse(subCategory.Properties.Any());
            Assert.IsFalse(subCategory.IsVisible);
        }

        [Test]
        public void Test_WpfGuiSubCategory_WithProperties()
        {
            var fieldUiDescriptions = new List<FieldUIDescription>();
            var fieldUiDescription = new FieldUIDescription(null, null) {Label = "dummyName"};
            fieldUiDescriptions.Add(fieldUiDescription);
            var subCategory = new WpfGuiSubCategory("dummySubCateogry", fieldUiDescriptions);
            Assert.IsNotNull(subCategory);
            Assert.IsNotNull(subCategory.Properties);
            Assert.IsTrue(subCategory.Properties.Any());
            Assert.IsTrue(subCategory.IsVisible);
            Assert.IsTrue(subCategory.Properties.Any(p => p.Label.Equals(fieldUiDescription.Label)));
        }
        
        [Test]
        public void Test_WpfGuiSubCategory_Visibility_Adding_Property()
        {
            var subCategory = new WpfGuiSubCategory("dummySubCateogry", new List<FieldUIDescription>());
            Assert.IsNotNull(subCategory);
            Assert.IsNotNull(subCategory.Properties);
            Assert.IsFalse(subCategory.Properties.Any());
            Assert.IsFalse(subCategory.IsVisible);
            
            //add property
            var property =  new WpfGuiProperty(new FieldUIDescription(null, null) {Label = "dummyName"});
            subCategory.Properties.Add(property);
            
            Assert.IsTrue(subCategory.Properties.Any());
            Assert.IsTrue(subCategory.IsVisible);
        }
    }
}