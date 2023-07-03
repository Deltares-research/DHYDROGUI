using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [Category(TestCategory.Wpf)]
    public class SettingsTemplateSelectorTest
    {
        private readonly WpfSettingsView settingsView = new WpfSettingsView();

        [Test]
        public void SelectTemplate_NotGivingWpfElement_DoesNotCrash()
        {
            const bool dummy = true;
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selector.SelectTemplate(dummy, settingsView));
        }

        [Test]
        public void SelectTemplate_GivenWpfGuiCategory_ReturnsTemplate()
        {
            var item = new WpfGuiCategory("dummyCategory", null);

            VerifyCall(item, "TabContentTemplate");
        }

        [Test]
        public void SelectTemplate_GivenWpfGuiCategory_WithCustomControl_ReturnsTemplate()
        {
            var item = new WpfGuiCategory("dummyCategory", null) {CustomControl = new UserControl()};
            VerifyCall(item, "TabCustomContentTemplate");
        }

        [Test]
        public void SelectTemplate_GivenWpfGuiSubCategory_ReturnsTemplate()
        {
            var item = new WpfGuiSubCategory("dummySubCategory", null);
            VerifyCall(item, "SubCategoryTemplate");
        }

        [Test]
        public void SelectTemplate_GivenWpfGuiSubCategory_WithCustomControl_ReturnsTemplate()
        {
            var item = new WpfGuiSubCategory("dummySubCategory", null) {CustomControl = new UserControl()};
            VerifyCall(item, "SubCategoryCustomTemplate");
        }

        [Test]
        [TestCase(typeof(string), "TextBoxTemplate")]
        [TestCase(typeof(double), "TextBoxTemplate")]
        [TestCase(typeof(int), "TextBoxTemplate")]
        [TestCase(typeof(DateTime), "DateTimeTemplate")]
        [TestCase(typeof(DateOnly), "DateOnlyTemplate")]
        [TestCase(typeof(bool), "CheckboxTemplate")]
        [TestCase(typeof(TimeSpan), "TimeSpanTemplate")]
        [TestCase(typeof(IList<double>), "ListTemplate")]
        [TestCase(typeof(Enum), "ComboBoxTemplate")]
        [TestCase(typeof(TestEnum), "ComboBoxTemplate")]
        public void Test_SelectTemplate_GivenWpfGuiProperty_ReturnsTemplate(Type propertyType, string expectedTemplateKey)
        {
            var item = new WpfGuiProperty(new FieldUIDescription((o) => propertyType.IsValueType ? Activator.CreateInstance(propertyType) : null, (o, o1) => {}) {ValueType = propertyType});

            VerifyCall(item, expectedTemplateKey);
        }

        [Test]
        public void SelectTemplate_GivenWpfGuiProperty_WithUnMappedType_DoesNotThrow()
        {
            var item = new WpfGuiProperty(new FieldUIDescription(null, null) {ValueType = null});

            DataTemplate selectedTemplate = null;
            var frameworkElement = new WpfSettingsView();
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selectedTemplate = selector.SelectTemplate(item, frameworkElement));
            Assert.That(selectedTemplate, Is.Null);
        }

        private enum TestEnum {}

        [TestCase("yyyy-mm-dd", "DateTemplate")]
        [TestCase("[yyyy-mm-dd]", "DateTemplate")]
        [TestCase("YYYY-MM-DD", "DateTemplate")]
        [TestCase("yyyy-mm-dd hh:mm:ss", "DateTimeTemplate")]
        [TestCase("", "DateTimeTemplate")]
        [TestCase(null, "DateTimeTemplate")]
        public void SelectTemplate_ForWpfGuiProperty_WithDateTimeType_ReturnsTemplate(string unit, string expectedTemplateKey)
        {
            var fieldDescription = new FieldUIDescription(null, null)
            {
                ValueType = typeof(DateTime),
                UnitSymbol = unit
            };
            var property = new WpfGuiProperty(fieldDescription);

            VerifyCall(property, expectedTemplateKey);
        }

        private void VerifyCall(object item, string expectedTemplateKey)
        {
            object expectedTemplate = settingsView.FindResource(expectedTemplateKey);
            var selector = new SettingsTemplateSelector();

            // Call
            DataTemplate selectedTemplate = selector.SelectTemplate(item, settingsView);

            // Assert
            Assert.That(selectedTemplate, Is.Not.Null);
            Assert.That(selectedTemplate, Is.SameAs(expectedTemplate));
        }
    }
}