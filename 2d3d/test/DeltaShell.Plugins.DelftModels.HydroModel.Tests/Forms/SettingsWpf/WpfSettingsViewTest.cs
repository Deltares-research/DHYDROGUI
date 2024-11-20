using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    [NUnit.Framework.Category(TestCategory.Wpf)]
    public class WpfSettingsViewTest
    {
        [Test]
        public void Constructor_ExpectedValues()
        {
            // Call
            using (var view = new WpfSettingsView())
            {
                // Assert
                Assert.That(view, Is.InstanceOf<IAdditionalView>());

                WpfSettingsViewModel viewModel = view.ViewModel;
                Assert.That(viewModel, Is.Not.Null);
                Assert.That(view.SettingsCategories, Is.SameAs(viewModel.SettingsCategories));
            }
        }

        [Test]
        public void GivenViewWithData_WhenViewDisposedAndDataFiresNotifyPropertyChangedEvent_ThenViewNotNotified()
        {
            // Given
            const string propertyName = "PropertyName";
            var property = new WpfGuiProperty(new FieldUIDescription(null, (o, v) => {}) {Name = propertyName});

            var isPropertyChangedEventFired = false;
            var category = new WpfGuiCategory(string.Empty, null);
            category.PropertyChanged += (sender, args) => isPropertyChangedEventFired = true;
            category.AddWpfGuiProperty(property);

            INotifyPropertyChange model = Substitute.For<INotifyPropertyChange, IHydroModel>();
            var view = new WpfSettingsView
            {
                Data = model,
                SettingsCategories = new ObservableCollection<WpfGuiCategory>(new[]
                {
                    category
                }),
                GetChangedPropertyName = (sender, args) => propertyName
            };

            // Precondition
            WpfSettingsViewModel viewModel = view.ViewModel;
            Assert.That(viewModel.DataModel, Is.SameAs(model));
            Assert.That(viewModel.SettingsCategories.Single(), Is.SameAs(category));

            // When
            view.Dispose();
            model.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs(string.Empty));

            // Then
            // This verifies that the PropertyChangedEvent event handler is properly unsubscribed from the data model.
            // The event triggers an update from the properties which propagate through the category. 
            Assert.That(isPropertyChangedEventFired, Is.False);
        }

        [Test]
        public void GivenViewWithViewModel_WhenDisposedAndCategoryTriggersEvent_ThenViewNotNotified()
        {
            // Given
            var category = new WpfGuiCategory("category", new[]
            {
                new FieldUIDescription(null, (o, v) => {}) {ValueType = typeof(object)}
            }) {CategoryVisibility = () => true};
            IEnumerable<WpfGuiCategory> categories = new[]
            {
                category
            };

            var view = new WpfSettingsView {SettingsCategories = new ObservableCollection<WpfGuiCategory>(categories)};

            // Precondition
            WpfGuiProperty property = category.Properties.Single(); // Cache the property as with a proper disposal, the category will clear its properties
            Assert.That(view.SettingsCategories.Single(), Is.SameAs(category));
            Assert.That(category.IsVisible, Is.True); // Ensure that the category is visible when test starts to ensure that the NotifyPropertyChanged event was subscribed

            // When
            view.Dispose();

            var isCollectionChanged = false;
            view.SettingsCategories.CollectionChanged += (sender, args) => isCollectionChanged = true;

            category.CategoryVisibility = () => false; // Make the category invisible to make sure collection changed event would be triggered if its still subscribed
            property.RaisePropertyChangedEvents();

            // Then
            // The property changed event of a property triggers a CollectionChangedEvents
            // to add visible or remove invisible items. As the viewmodel is disposed, the 
            // viewmodel should NOT trigger any CollectionChangedEvents.
            Assert.That(isCollectionChanged, Is.False);
            CollectionAssert.IsEmpty(view.SettingsCategories);
        }

        [Test]
        public void SetSynchronizedProperties_PropertiesNull_ThrowsArgumentNullException()
        {
            // Setup
            using (var view = new WpfSettingsView())
            {
                // Call
                TestDelegate call = () => view.SetSynchronizedProperties(null);

                // Assert
                Assert.That(call, Throws.TypeOf<ArgumentNullException>()
                                        .With.Property(nameof(ArgumentNullException.ParamName))
                                        .EqualTo("properties"));
            }
        }

        [Test]
        public void GivenViewWithNoDataSet_WhenSetSynchronizedPropertiesCalled_ThenInvalidOperationExceptionThrown()
        {
            // Given
            using (var view = new WpfSettingsView())
            {
                // Call
                TestDelegate call = () => view.SetSynchronizedProperties(Enumerable.Empty<WpfGuiProperty>());

                // Then
                Assert.That(call, Throws.TypeOf<InvalidOperationException>()
                                        .With.Message
                                        .EqualTo("Cannot synchronize properties when private field synchronizer is null."));
            }
        }

        [Test]
        public void GivenViewWithDataAndSynchronizedProperties_WhenDataNotifyPropertyEventChangedFired_ThenPropertiesNotified()
        {
            // Given
            INotifyPropertyChanged observable = Substitute.For<INotifyPropertyChanged, IHydroModel>();

            var propertyOneNotified = false;
            var propertyOne = new WpfGuiProperty(new FieldUIDescription(null, null));
            propertyOne.PropertyChanged += (sender, args) => { propertyOneNotified = true; };

            var propertyTwoNotified = false;
            var propertyTwo = new WpfGuiProperty(new FieldUIDescription(null, null));
            propertyTwo.PropertyChanged += (sender, args) => { propertyTwoNotified = true; };

            using (var view = new WpfSettingsView
            {
                Data = observable,
                GetChangedPropertyName = (sender, args) => string.Empty
            })
            {
                view.SetSynchronizedProperties(new[]
                {
                    propertyOne,
                    propertyTwo
                });

                // When
                observable.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs(string.Empty));

                // Then
                Assert.That(propertyOneNotified, Is.True);
                Assert.That(propertyTwoNotified, Is.True);
            }
        }

        [Test]
        public void GivenViewWithDataAndSynchronizedProperties_WhenDisposedAndDataNotifyPropertyEventChangedFired_ThenPropertiesNotified()
        {
            // Given
            INotifyPropertyChanged observable = Substitute.For<INotifyPropertyChanged, IHydroModel>();

            var propertyOneNotified = false;
            var propertyOne = new WpfGuiProperty(new FieldUIDescription(null, null));
            propertyOne.PropertyChanged += (sender, args) => { propertyOneNotified = true; };

            var propertyTwoNotified = false;
            var propertyTwo = new WpfGuiProperty(new FieldUIDescription(null, null));
            propertyTwo.PropertyChanged += (sender, args) => { propertyTwoNotified = true; };

            var view = new WpfSettingsView
            {
                Data = observable,
                GetChangedPropertyName = (sender, args) => string.Empty
            };

            view.SetSynchronizedProperties(new[]
            {
                propertyOne,
                propertyTwo
            });

            // When
            view.Dispose();
            observable.PropertyChanged += Raise.Event<PropertyChangedEventHandler>(this, new PropertyChangedEventArgs(string.Empty));

            // Then
            Assert.That(propertyOneNotified, Is.False);
            Assert.That(propertyTwoNotified, Is.False);
        }

        [Test]
        public void GivenViewWithoutDataAndSynchronizer_WhenDisposing_ThenNoExceptionThrown()
        {
            // Given
            var view = new WpfSettingsView();

            // When
            TestDelegate call = () => view.Dispose();

            // Then
            Assert.That(call, Throws.Nothing);
        }
    }
}