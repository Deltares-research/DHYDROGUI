using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.NGHS.Common.Gui.WPF.SettingsView;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.NGHS.Common.Gui.Tests.WPF.SettingsView
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class GuiPropertyTest
    {
        [Test]
        public void Test_WpfGuiProperty_AsNull()
        {
            var property = new GuiProperty(null);
            Assert.IsNotNull(property);
        }

        [Test]
        public void Test_WpfGuiProperty()
        {
            var dummyField = new FieldUIDescription( null, null )
            {
                Label = "dummyName",
            };

            var property = new GuiProperty(dummyField);
            Assert.IsNotNull(property);
            Assert.AreEqual(dummyField.Label, property.Label);
            Assert.IsFalse(property.HasCustomControl);
        }

        [Test]
        public void Test_WpfGuiProperty_With_ControlHelper_GetsUserControlHosted()
        {
            var helper = MockRepository.GenerateStrictMock<ICustomControlHelper>();
            helper.Expect(h => h.CreateControl()).Return(new Control());
            helper.Replay();

            var dummyField = new FieldUIDescription(null, null)
            {
                Label = "dummyName",
                CustomControlHelper = helper,
            };

            var property = new GuiProperty(dummyField);
            Assert.IsNotNull(property);
            Assert.AreEqual(dummyField.Label, property.Label);
            Assert.IsTrue(property.HasCustomControl);

            Assert.IsNotNull(property.HasCustomControl);
        }

        [Test]
        public void Test_WpfGuiProperty_ValueTypeList_Sets_Collection()
        {
            var doubleList = new List<double>() {1.0, 2.0};
            var dummyField = new FieldUIDescription((o) => doubleList, (o, o1) => doubleList = o1 as List<double>)
            {
                Label = "dummyName",
                ValueType = typeof(IList<double>),
            };

            var prop = new GuiProperty(dummyField);
            prop.GetModel = () => true; /*Dummy value, we just don't want it to crash. */
            Assert.IsNotNull(prop);

            Assert.IsNotNull(prop.ValueCollection);
            Assert.AreEqual(doubleList, prop.ValueCollection.Select( vc => vc.WrapperValue).ToList());

            /*Check the Double Wrapper SetBackValue action works as expected.*/
            prop.ValueCollection[0].WrapperValue = 3.5;
            Assert.AreEqual(doubleList[0], 3.5);
        }

        [Test]
        public void Test_WpfGuiProperty_TimeSpan_Is_Correctly_Set()
        {
            var timeSpanInput = new TimeSpan(0, 01, 00, 01, 400);
            var dummyField = new FieldUIDescription((o) => timeSpanInput, (o, o1) => timeSpanInput = (TimeSpan)o1)
            {
                Label = "dummyName",
                ValueType = typeof(TimeSpan),
            };

            var prop = new GuiProperty(dummyField);
            prop.GetModel = () => true; /*Dummy value, we just don't want it to crash. */
            //Try to set a new value
            var newTimeSpan = new TimeSpan(0, 02, 01, 04, 700);
            prop.Value = newTimeSpan;
            Assert.IsNotNull(prop);

            Assert.IsNotNull(prop.Value);
            Assert.AreEqual(newTimeSpan, prop.Value);
        }
    }
}