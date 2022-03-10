using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SettingsTemplateSelectorTest
    {
        [Test]
        public void Test_SelectTemplate_NotGivingWpfElement_DoesNotCrash()
        {
            var dummy = true;
            var frameworkElement = new WpfSettingsView();
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selector.SelectTemplate(dummy, frameworkElement));
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiCategory_ReturnsTemplate()
        {
            var item = new WpfGuiCategory("dummyCategory", null);
            GetAndCheckDataTemplate(item);
        }
        [Test]
        public void Test_SelectTemplate_GivenWpfGuiCategory_WithCustomControl_ReturnsTemplate()
        {
            var item = new WpfGuiCategory("dummyCategory", null)
            {
                CustomControl = new UserControl(),
            };
            GetAndCheckDataTemplate(item);
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiSubCategory_ReturnsTemplate()
        {
            var item = new WpfGuiSubCategory("dummySubCategory", null);
            GetAndCheckDataTemplate(item);
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiSubCategory_WithCustomControl_ReturnsTemplate()
        {
            var item = new WpfGuiSubCategory("dummySubCategory", null)
            {
                CustomControl = new UserControl(),
            };
            GetAndCheckDataTemplate(item);
        }

        [Test]
        [TestCase(typeof(string))]
        [TestCase(typeof(double))]
        [TestCase(typeof(int))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeSpan))]
        [TestCase(typeof(Enum))]
        public void Test_SelectTemplate_GivenWpfGuiProperty_ReturnsTemplate(Type propertyType)
        {
            var item = new WpfGuiProperty(new FieldUIDescription(null, null)
            {
                ValueType = propertyType
            });

            GetAndCheckDataTemplate(item);
        }

        [Test]
        public void Test_SelectTemplate_GivenFieldUIDescriptionWithNullFunc_ThrowsException()
        {
            TestDelegate call = () =>
            {
                new WpfGuiProperty(new FieldUIDescription(null, null)
                {
                    ValueType = typeof(IList<double>)
                });
            };
            Assert.That(call, Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiProperty_WithCustomControl_ReturnsTemplate()
        {
            var item = new WpfGuiProperty(new FieldUIDescription(null, null))
            {
                CustomControl = new UserControl(),
            };
            GetAndCheckDataTemplate(item);
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiProperty_WithUnMappedType_DoesNotThrow()
        {
            var item = new WpfGuiProperty(new FieldUIDescription(null, null)
            {
                ValueType = null
            });
            var frameworkElement = new WpfSettingsView();
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selector.SelectTemplate(item, frameworkElement));
        }

        private static void GetAndCheckDataTemplate(object item)
        {
            DataTemplate selectedTemplate = null;
            var frameworkElement = new WpfSettingsView();
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selectedTemplate = selector.SelectTemplate(item, frameworkElement));
            Assert.IsNotNull(selectedTemplate);
        }
    }
}