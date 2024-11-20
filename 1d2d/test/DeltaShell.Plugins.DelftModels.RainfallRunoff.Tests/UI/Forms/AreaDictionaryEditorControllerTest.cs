using System;
using System.Linq;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Controls;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class AreaDictionaryEditorControllerTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var editor = new AreaDictionaryEditor();
            new AreaDictionaryEditorController<String>(editor);
            WindowsFormsTestHelper.ShowModal(editor);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithStringData()
        {
            var data = new AreaDictionary<String>();
            data["Apple"] = 1.0;
            data["Pear"] = 2.0;
            data["Banana"] = 3.0;
            data["Melon"] = 3.0;
            data["Orange"] = 3.0;

            var editor = new AreaDictionaryEditor();
            new AreaDictionaryEditorController<String>(editor) {Data = data};
            WindowsFormsTestHelper.ShowModal(editor);
        }
        
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithEnumData()
        {
            var data = new AreaDictionary<TestEnum>();
            data[TestEnum.ShortName] = 1.0;
            data[TestEnum.LongName] = 3.0;
            data[TestEnum.AdditionalItem] = 2.0;

            var editor = new AreaDictionaryEditor();
            new AreaDictionaryEditorController<TestEnum>(editor) { Data = data };
            WindowsFormsTestHelper.ShowModal(editor);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithEnumDataAndModify()
        {
            var data = new AreaDictionary<TestEnum>();
            data[TestEnum.ShortName] = 1.0;
            data[TestEnum.LongName] = 3.0;
            data[TestEnum.AdditionalItem] = 2.0;
            
            var expected = 5.0;

            var editor = new AreaDictionaryEditor();
            new AreaDictionaryEditorController<TestEnum>(editor) { Data = data };
            WindowsFormsTestHelper.ShowModal(editor,
                                             f =>
                                                 {
                                                     var textBox = editor.ItemPanel.Controls.OfType<TextBox>().First();
                                                     textBox.Focus();
                                                     textBox.Text = expected.ToString();
                                                     var label = editor.ItemPanel.Controls.OfType<Label>().First();
                                                     label.Focus();
                                                     editor.ValidateChildren();
                                                 });
            
            Assert.AreEqual("5",expected.ToString());
            Assert.AreEqual(expected, data[TestEnum.ShortName]);
        }

        [System.ComponentModel.TypeConverter(typeof(EnumDescriptionAttributeTypeConverter))]
        enum TestEnum
        {
            [System.ComponentModel.Description("Long")]
            LongName,
            [System.ComponentModel.Description("Unrelated")]
            ShortName,
            [System.ComponentModel.Description("Additional")]
            AdditionalItem,
        }
    }
}
