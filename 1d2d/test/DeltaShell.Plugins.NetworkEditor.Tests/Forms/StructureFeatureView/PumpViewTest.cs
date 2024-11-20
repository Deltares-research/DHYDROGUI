using System.Windows.Forms;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{
    [TestFixture]
    public class PumpViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var pumpView = new PumpView { Data = null };
            WindowsFormsTestHelper.ShowModal(pumpView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPumpView()
        {
            var pump = new Pump();
            var pumpView = new PumpView {Data = pump};
            WindowsFormsTestHelper.ShowModal(pumpView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowPumpViewShouldNotCausePropertyChanged()
        {
            int called = 0;

            var pump = new Pump();
            pump.ControlDirection = PumpControlDirection.DeliverySideControl; //this triggers the checkboxes to change

            ((INotifyPropertyChange)pump).PropertyChanged += (s, e) => called++;
            var pumpView = new PumpView {Data = pump};
            WindowsFormsTestHelper.ShowModal(pumpView);

            Assert.AreEqual(0, called);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ChangingControlDirectionShouldUpdatePumpView()
        {
            var pump = new Pump {ControlDirection = PumpControlDirection.SuctionSideControl};

            var pumpView = new PumpView {Data = pump};

            var checkBoxDeliverySide = (CheckBox)pumpView.Controls.Find("checkBoxDelivery", true)[0];

            WindowsFormsTestHelper.ShowModal(pumpView,
                                             f =>
                                                 {
                                                     Assert.IsFalse(checkBoxDeliverySide.Checked);
                                                     pump.ControlDirection = PumpControlDirection.DeliverySideControl;
                                                     Assert.IsTrue(checkBoxDeliverySide.Checked);
                                                 });
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ChangingPumpDirectionToNegativeShouldUpdatePumpViewCorrectly()
        {
            var pump = new Pump
                {
                    Branch = new Channel()
                };
            var pumpView = new PumpView { Data = pump };

            var radioDirectionIsNegative = (RadioButton)pumpView.Controls.Find("radioButtonNegative", true)[0];
            
            WindowsFormsTestHelper.ShowModal(pumpView,
                                             f =>
                                                 {
                                                     Assert.IsFalse(radioDirectionIsNegative.Checked);
                                                     pump.DirectionIsPositive = false;
                                                     Assert.IsTrue(radioDirectionIsNegative.Checked);
                                                 });
        }
    }
}