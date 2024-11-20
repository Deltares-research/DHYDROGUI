using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.NGHS.Common.Gui.WPF.SettingsView;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.WPF.SettingsView
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SettingsTemplateSelectorTest
    {
        [Test]
        public void Test_SelectTemplate_NotGivingWpfElement_DoesNotCrash()
        {
            var dummy = true;
            var frameworkElement = new Gui.WPF.SettingsView.SettingsView();
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selector.SelectTemplate(dummy, frameworkElement));
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiCategory_ReturnsTemplate()
        {
            var item = new GuiCategory("dummyCategory", null);
            GetAndCheckDataTemplate(item);
        }
        [Test]
        public void Test_SelectTemplate_GivenWpfGuiCategory_WithCustomControl_ReturnsTemplate()
        {
            var item = new GuiCategory("dummyCategory", null)
            {
                CustomControl = new UserControl(),
            };
            GetAndCheckDataTemplate(item);
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiSubCategory_ReturnsTemplate()
        {
            var item = new GuiSubCategory("dummySubCategory", null);
            GetAndCheckDataTemplate(item);
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiSubCategory_WithCustomControl_ReturnsTemplate()
        {
            var item = new GuiSubCategory("dummySubCategory", null)
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
        [TestCase(typeof(DateOnly))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(TimeSpan))]
        [TestCase(typeof(Enum))]
        public void Test_SelectTemplate_GivenWpfGuiProperty_ReturnsTemplate(Type propertyType)
        {
            var item = new GuiProperty(new FieldUIDescription(null, null)
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
                new GuiProperty(new FieldUIDescription(null, null)
                {
                    ValueType = typeof(IList<double>)
                });
            };
            Assert.That(call, Throws.Exception.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiProperty_WithCustomControl_ReturnsTemplate()
        {
            var item = new GuiProperty(new FieldUIDescription(null, null))
            {
                CustomControl = new UserControl(),
            };
            GetAndCheckDataTemplate(item);
        }

        [Test]
        public void Test_SelectTemplate_GivenWpfGuiProperty_WithUnMappedType_DoesNotThrow()
        {
            var item = new GuiProperty(new FieldUIDescription(null, null)
            {
                ValueType = null
            });
            var frameworkElement = new Gui.WPF.SettingsView.SettingsView();
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selector.SelectTemplate(item, frameworkElement));
        }

        private static void GetAndCheckDataTemplate(object item)
        {
            DataTemplate selectedTemplate = null;
            var frameworkElement = new Gui.WPF.SettingsView.SettingsView();
            var selector = new SettingsTemplateSelector();

            Assert.DoesNotThrow(() => selectedTemplate = selector.SelectTemplate(item, frameworkElement));
            Assert.IsNotNull(selectedTemplate);
        }
    }
}