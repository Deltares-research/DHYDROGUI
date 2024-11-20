using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class WpfSettingsViewModelTest
    {
        [Test]
        public void Test_WpfSettingsViewModel()
        {
            // Call
            var viewModel = new WpfSettingsViewModel();

            // Assert
            Assert.That(viewModel, Is.InstanceOf<IDisposable>());
            Assert.That(viewModel.DataModel, Is.Null);
            CollectionAssert.IsEmpty(viewModel.SettingsCategories);
        }

        [Test]
        public void Test_SettingsCategories_ShowOnly_VisibleCategories()
        {
            // Setup
            using (var viewModel = new WpfSettingsViewModel())
            {
                var wpfGuiCategoryVisible = new WpfGuiCategory("cat", null);
                var wpfGuiCategoryHidden = new WpfGuiCategory("cat2", null) {CategoryVisibility = () => false};

                // Call
                viewModel.SettingsCategories = new ObservableCollection<WpfGuiCategory>
                {
                    wpfGuiCategoryHidden,
                    wpfGuiCategoryVisible
                };

                // Assert
                CollectionAssert.AreEqual(new[]
                {
                    wpfGuiCategoryVisible
                }, viewModel.SettingsCategories);
            }
        }

        [Test]
        public void GivenViewModelWithCategories_WhenCategoryRaisesPropertyChangedEvent_ThenCollectionUpdated()
        {
            // Given
            var category = new WpfGuiCategory("category", new[]
            {
                new FieldUIDescription(null, (o, v) => {}) {ValueType = typeof(object)}
            });
            var categories = new ObservableCollection<WpfGuiCategory> {category};
            using (var viewModel = new WpfSettingsViewModel {SettingsCategories = categories})
            {
                var isCollectionChanged = false;
                viewModel.SettingsCategories.CollectionChanged += (sender, args) => isCollectionChanged = true;

                // Precondition 
                CollectionAssert.AreEqual(categories, viewModel.SettingsCategories);

                // When
                category.CategoryVisibility = () => false;              // Make the category invisible
                WpfGuiProperty property = category.Properties.Single(); // Trigger PropertyChangedEvent
                property.Value = new object();

                // Then
                // The property changed event of a category triggers a collection changed event
                // to add visible or remove invisible items.
                Assert.That(isCollectionChanged, Is.True);
                CollectionAssert.IsEmpty(viewModel.SettingsCategories);
            }
        }

        [Test]
        public void GivenViewModelWithCategories_WhenSettingNewGuiCategoriesAndOldSettingsRaisePropertyChangedEvent_ThenNothingHappens()
        {
            // Given
            var oldCategory = new WpfGuiCategory("oldCategory", new[]
            {
                new FieldUIDescription(null, (o, v) => {}) {ValueType = typeof(object)}
            });
            using (var viewModel = new WpfSettingsViewModel {SettingsCategories = new ObservableCollection<WpfGuiCategory> {oldCategory}})
            {
                var newCategory = new WpfGuiCategory("newCategory", null);
                var newCategories = new ObservableCollection<WpfGuiCategory> {newCategory};
                viewModel.SettingsCategories = newCategories;

                var isCollectionChanged = false;
                viewModel.SettingsCategories.CollectionChanged += (sender, args) => isCollectionChanged = true;

                // When
                oldCategory.CategoryVisibility = () => false;              // Make the category invisible
                WpfGuiProperty property = oldCategory.Properties.Single(); // Trigger PropertyChangedEvent
                property.Value = new object();

                // Then
                // The property changed event of a category triggers a collection changed event
                // to add visible or remove invisible items.
                Assert.That(isCollectionChanged, Is.False);
                CollectionAssert.AreEqual(newCategories, viewModel.SettingsCategories);
            }
        }

        [Test]
        public void GivenViewModel_WhenAddingCategoriesAndPropertyChangedEventTriggered_ThenCollectionUpdated()
        {
            // Given
            var category = new WpfGuiCategory("category", new[]
            {
                new FieldUIDescription(null, (o, v) => {}) {ValueType = typeof(object)}
            });
            IEnumerable<WpfGuiCategory> categories = new[]
            {
                category
            };

            using (var viewModel = new WpfSettingsViewModel())
            {
                // Precondition
                Assert.That(category.IsVisible, Is.True); // Ensure that the category is visible when test starts

                // When
                viewModel.SettingsCategories.AddRange(categories);
                CollectionAssert.AreEqual(categories, viewModel.SettingsCategories);

                var isCollectionChanged = false;
                viewModel.SettingsCategories.CollectionChanged += (sender, args) => isCollectionChanged = true;

                category.CategoryVisibility = () => false;              // Make the category invisible
                WpfGuiProperty property = category.Properties.Single(); // Trigger PropertyChangedEvent
                property.Value = new object();

                // Then
                // The property changed event of a category triggers a collection changed event
                // to add visible or remove invisible items.
                Assert.That(isCollectionChanged, Is.True);
                CollectionAssert.IsEmpty(viewModel.SettingsCategories);
            }
        }

        [Test]
        public void GivenViewModelWithCategories_WhenDisposedAndCategoryTriggersPropertyChangedEvent_ThenViewModelNotNotified()
        {
            // Given
            var category = new WpfGuiCategory("category", new[]
            {
                new FieldUIDescription(null, (o, v) => {}) {ValueType = typeof(object)}
            }) {CategoryVisibility = () => true};

            var viewModel = new WpfSettingsViewModel
            {
                SettingsCategories = new ObservableCollection<WpfGuiCategory>(new[]
                {
                    category
                })
            };

            // Precondition
            WpfGuiProperty property = category.Properties.Single(); // Cache the property as with a proper disposal, the category will clear its properties
            Assert.That(viewModel.SettingsCategories.Single(), Is.SameAs(category));
            Assert.That(category.IsVisible, Is.True); // Ensure that the category is visible when test starts to ensure that the NotifyPropertyChanged event was subscribed

            // When
            viewModel.Dispose();

            var isCollectionChanged = false;
            viewModel.SettingsCategories.CollectionChanged += (sender, args) => isCollectionChanged = true;

            category.CategoryVisibility = () => false; // Make the category invisible to make sure collection changed event would be triggered if its still subscribed
            property.RaisePropertyChangedEvents();

            // Then
            // The property changed event of a category triggers a CollectionChangedEvent
            // to add visible or remove invisible items. As the viewmodel is disposed, the 
            // viewmodel should NOT trigger any CollectionChangedEvents.
            Assert.That(isCollectionChanged, Is.False);
            CollectionAssert.IsEmpty(viewModel.SettingsCategories);
        }
    }
}