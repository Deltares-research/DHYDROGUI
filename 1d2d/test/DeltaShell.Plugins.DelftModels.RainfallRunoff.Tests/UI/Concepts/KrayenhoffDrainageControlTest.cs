using System.Linq;
using System.Windows.Forms;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Unpaved;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Concepts
{
    [TestFixture]
    [Category(TestCategory.WindowsForms)]
    public class KrayenhoffDrainageControlTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var control = new KrayenhoffDrainageControl();
            WindowsFormsTestHelper.ShowModal(control);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWithData()
        {
            var control = new KrayenhoffDrainageControl();
            var formula = new KrayenhoffVanDeLeurDrainageFormula();
            control.Data = formula;
            var expected = 23.0;
            WindowsFormsTestHelper.ShowModal(control,
                                             f => 
                                             { 
                                                 var textBox = control.Controls.OfType<TextBox>().First();
                                                 textBox.Focus();
                                                 textBox.Text = expected.ToString();
                                                 var label = control.Controls.OfType<Label>().First();
                                                 label.Focus();
                                                 control.ValidateChildren();
                                             });
            Assert.AreEqual("23", expected.ToString());
            Assert.AreEqual(expected, formula.ResevoirCoefficient);
        }
    }
}
