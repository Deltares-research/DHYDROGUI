using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WpfGuiCategoryTest
    {
        [Test]
        public void Test_WpfGuiCategory_With_Null_Attributes_DoesNot_Throw()
        {
            WpfGuiCategory category = null;
            Assert.DoesNotThrow( () => category = new WpfGuiCategory(null, null));
            Assert.IsNotNull(category);
            Assert.IsNotNull(category.IsVisible);
            Assert.IsNotNull(category.SubCategories);
            Assert.IsNotNull(category.Properties);
        }

        [Test]
        public void Test_WpfGuiCategory_With_Name_DoesNot_Throw()
        {
            WpfGuiCategory category = null;
            var dummyCategoryName = "dummyCategory";
            Assert.DoesNotThrow( () => category = new WpfGuiCategory(dummyCategoryName, null));
            Assert.IsNotNull(category);
            Assert.AreEqual(dummyCategoryName, category.CategoryName);
            Assert.IsNotNull(category.IsVisible);
            Assert.IsNotNull(category.SubCategories);
            Assert.IsNotNull(category.Properties);
        }

        [Test]
        public void Test_WpfGuiCategory_IsVisible_When_NoProperties()
        {
            var dummyCategoryName = "dummyCategory";
            var category = new WpfGuiCategory(dummyCategoryName, null);
            Assert.IsNotNull(category);

            Assert.IsFalse(category.Properties.Any());
            Assert.IsTrue(category.IsVisible);
        }

        [Test]
        [TestCase(false, false, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        public void Test_WpfGuiCategory_IsVisible_When_AtLeast_One_Property_Without_CustomControl(bool withCustomControl, bool propertyVisible, bool expectedResult)
        {
            var dummyCategoryName = "dummyCategory";

            var category = new WpfGuiCategory(dummyCategoryName, null);
            Assert.IsNotNull(category);

            var fieldUiDescription = new FieldUIDescription(null, null, o => true, o=> propertyVisible);
            category.AddFieldUiDescription(fieldUiDescription);
            Assert.IsTrue(category.Properties.Any());

            var property = category.Properties.FirstOrDefault();
            Assert.IsNotNull(property);
            if (withCustomControl) property.CustomControl = new UserControl();

            Assert.AreEqual(expectedResult, category.IsVisible);
        }

        [Test]
        public void Test_WpfGuiCategory_With_Properties_Creates_SubCategories()
        {
            var dummyCategoryName = "dummyCategory";
            var dummySubCategoryName = "dummySubCategory";
            var propertyList = new List<FieldUIDescription>()
            {
                new FieldUIDescription(null, null)
                {
                    SubCategory = dummySubCategoryName
                }
            };

            var category = new WpfGuiCategory(dummyCategoryName, propertyList);
            Assert.IsNotNull(category);
            
            Assert.IsNotNull(category.SubCategories);
            Assert.IsTrue(category.SubCategories.Any());
            Assert.IsTrue(category.SubCategories.Count == 1);
            Assert.AreEqual(dummySubCategoryName, category.SubCategories.FirstOrDefault()?.SubCategoryName);

            Assert.IsNotNull(category.Properties);
            Assert.IsTrue(category.Properties.Any());
        }

        [Test]
        public void Test_AddFieldUiDescription_Creates_Property_And_SubCategory()
        {
            var dummyCategory = new WpfGuiCategory("dummyCategory", null);
            Assert.IsNotNull(dummyCategory);

            var dummySubCategoryName = "dummySubCategory";
            var dummyPropertyName = "dummyProperty";
            var dummyFieldUi = new FieldUIDescription(null, null)
            {
                SubCategory = dummySubCategoryName,
                Label = dummyPropertyName
            };
            
            //Check the fields are empty.
            Assert.IsFalse(dummyCategory.SubCategories.Any());
            Assert.IsFalse(dummyCategory.Properties.Any());

            dummyCategory.AddFieldUiDescription(dummyFieldUi);
            
            //Check the fields have now the property and subCategory.
            Assert.IsTrue(dummyCategory.SubCategories.Any());
            Assert.IsTrue(dummyCategory.Properties.Any());

            Assert.AreEqual(dummySubCategoryName, dummyCategory.SubCategories.FirstOrDefault()?.SubCategoryName);
            Assert.AreEqual(dummyPropertyName, dummyCategory.Properties.FirstOrDefault()?.Label);
        }

        [Test]
        public void Test_AddFieldUiDescription_Creates_Property_And_Adds_To_Existing_SubCategory()
        {
            var dummyCategory = new WpfGuiCategory("dummyCategory", null);
            Assert.IsNotNull(dummyCategory);

            var dummySubCategoryName = "dummySubCategory";
            var dummySubCategory = new WpfGuiSubCategory(dummySubCategoryName, null);
            dummyCategory.SubCategories.Add(dummySubCategory);
            Assert.IsTrue(dummyCategory.SubCategories.Contains(dummySubCategory));
            Assert.AreEqual(1, dummyCategory.SubCategories.Count);

            var dummyPropertyName = "dummyProperty";
            var dummyFieldUi = new FieldUIDescription(null, null)
            {
                SubCategory = dummySubCategoryName,
                Label = dummyPropertyName
            };

            //Check the fields are empty.
            Assert.IsFalse(dummyCategory.Properties.Any());

            dummyCategory.AddFieldUiDescription(dummyFieldUi);

            Assert.AreEqual(1, dummyCategory.SubCategories.Count);
            Assert.AreEqual(dummyPropertyName, dummySubCategory.Properties.FirstOrDefault()?.Label);

            Assert.IsTrue(dummyCategory.Properties.Any());
            Assert.AreEqual(dummyPropertyName, dummyCategory.Properties.FirstOrDefault()?.Label);
        }
    }
}