using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class WpfGuiCategoryTest
    {
        [Test]
        public void Test_WpfGuiCategory_With_Null_Attributes_DoesNot_Throw()
        {
            WpfGuiCategory category = null;
            Assert.DoesNotThrow(() => category = new WpfGuiCategory(null, null));
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
            Assert.DoesNotThrow(() => category = new WpfGuiCategory(dummyCategoryName, null));
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
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void Test_WpfGuiCategory_IsVisible_When_AtLeast_One_Property_Without_CustomControl(bool propertyVisible, bool expectedResult)
        {
            var dummyCategoryName = "dummyCategory";

            var category = new WpfGuiCategory(dummyCategoryName, null);
            Assert.IsNotNull(category);

            var fieldUiDescription = new FieldUIDescription(null, null, o => true, o => propertyVisible);
            category.AddFieldUiDescription(fieldUiDescription);

            Assert.IsTrue(category.Properties.Any());

            WpfGuiProperty property = category.Properties.FirstOrDefault();
            Assert.IsNotNull(property);

            Assert.AreEqual(expectedResult, category.IsVisible);
        }

        [Test]
        public void Test_WpfGuiCategory_With_Properties_Creates_SubCategories()
        {
            var dummyCategoryName = "dummyCategory";
            var dummySubCategoryName = "dummySubCategory";
            var propertyList = new List<FieldUIDescription>() {new FieldUIDescription(null, null) {SubCategory = dummySubCategoryName}};

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

        [Test]
        public void GivenCategoryWithCustomControl_WhenDisposingCategory_ThenCustomControlDisposed()
        {
            // Setup
            IDisposable customControl = Substitute.For<IDisposable, FrameworkElement>();
            var category = new WpfGuiCategory("category_name", new List<FieldUIDescription>()) {CustomControl = (FrameworkElement) customControl};

            // Call
            category.Dispose();

            // Assert
            customControl.Received(1).Dispose();
        }

        [Test]
        public void GivenCategoryWithProperties_WhenDisposedAndNotifyPropertyChangedEventFired_ThenCategoryNotNotified()
        {
            // Given
            var guiProperty = new WpfGuiProperty(new FieldUIDescription(null, (o, v) => {}));

            var isPropertyChangedEventFired = false;
            var category = new WpfGuiCategory(string.Empty, null);
            category.PropertyChanged += (sender, args) => isPropertyChangedEventFired = true;
            category.AddWpfGuiProperty(guiProperty);

            // Precondition
            Assert.That(category.Properties.Single(), Is.SameAs(guiProperty));

            // When
            category.Dispose();
            guiProperty.RaisePropertyChangedEvents(); // Trigger PropertyChangedEvent

            // Then
            Assert.That(isPropertyChangedEventFired, Is.False);
        }

        [Test]
        public void GivenCategory_WhenAddingWpfGuiPropertyAndPropertyFiresEvent_ThenCategoryRaisesPropertyChangedEvent()
        {
            // Given
            var property = new WpfGuiProperty(new FieldUIDescription(null, (o, v) => {}));
            using (var category = new WpfGuiCategory(string.Empty, null))
            {
                var isPropertyChangedEventRaised = false;
                category.PropertyChanged += (sender, args) => isPropertyChangedEventRaised = true;

                // When
                category.AddWpfGuiProperty(property);
                property.Value = new object(); // Trigger PropertyChangedEvent

                // Then
                Assert.That(isPropertyChangedEventRaised, Is.True);
            }
        }

        [Test]
        public void GivenCategory_WhenAddingFieldUIDescriptionAndPropertyFiresEvent_ThenCategoryRaisesPropertyChangedEvent()
        {
            // Given
            var property = new FieldUIDescription(null, (o, v) => {});
            using (var category = new WpfGuiCategory(string.Empty, null))
            {
                var isPropertyChangedEventRaised = false;
                category.PropertyChanged += (sender, args) => isPropertyChangedEventRaised = true;

                // When
                category.AddFieldUiDescription(property);
                WpfGuiProperty guiProperty = category.Properties.Single();
                guiProperty.Value = new object(); // Trigger PropertyChangedEvent

                // Then
                Assert.That(isPropertyChangedEventRaised, Is.True);
            }
        }

        [Test]
        public void GivenCategoryWithProperties_WhenPropertyRaisesPropertyChangedEvent_ThenCategoryRaisesPropertyChangedEvent()
        {
            // Given
            var properties = new List<FieldUIDescription> {new FieldUIDescription(null, (o, v) => {})};

            using (var category = new WpfGuiCategory(string.Empty, properties))
            {
                var isPropertyChangedEventRaised = false;
                category.PropertyChanged += (sender, args) => isPropertyChangedEventRaised = true;

                // When
                WpfGuiProperty property = category.Properties.Single();
                property.Value = new object(); // Trigger PropertyChangedEvent

                // Then
                Assert.That(isPropertyChangedEventRaised, Is.True);
            }
        }
    }
}