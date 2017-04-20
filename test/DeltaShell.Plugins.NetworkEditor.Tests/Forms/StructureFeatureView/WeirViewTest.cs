using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView
{

    [TestFixture]
    public class WeirViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowEmpty()
        {
            var view = new WeirView
                           {
                               Data = null
                           };
            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirView()
        {
            var weir = new Weir("TestWeir");

            var weirView = new WeirView
                               {
                                   Data = weir
                               };

            WindowsFormsTestHelper.ShowModal(weirView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirViewWithGatedWeir()
        {
            var weir = new Weir("TestWeir");
            weir.WeirFormula = new GatedWeirFormula {GateOpening = 5};
            var weirView = new WeirView
            {
                Data = weir
            };

            WindowsFormsTestHelper.ShowModal(weirView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirViewWithDetailedCrestDefinition()
        {
            var weir = new Weir("TestWeir") {WeirFormula = new RiverWeirFormula()};
            var weirView = new WeirView
            {
                Data = weir
            };

            WindowsFormsTestHelper.ShowModal(weirView);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        [Category(TestCategory.Jira)] // TOOLS-3668
        public void ShowRiverWeirViewShouldNotChangeValues()
        {
            var weir = new Weir("TestWeir")
                           {
                               WeirFormula =
                                   new RiverWeirFormula
                                       {
                                           CorrectionCoefficientNeg = 0.15,
                                           CorrectionCoefficientPos = 0.29,
                                           SubmergeLimitNeg = 0.77,
                                           SubmergeLimitPos = 0.44
                                       }
                           };
            var weirView = new WeirView
            {
                Data = weir
            };

            WindowsFormsTestHelper.ShowModal(weirView);
            Assert.AreEqual(0.15, ((RiverWeirFormula)weir.WeirFormula).CorrectionCoefficientNeg);
            Assert.AreEqual(0.29, ((RiverWeirFormula)weir.WeirFormula).CorrectionCoefficientPos);
            Assert.AreEqual(0.77, ((RiverWeirFormula)weir.WeirFormula).SubmergeLimitNeg);
            Assert.AreEqual(0.44, ((RiverWeirFormula)weir.WeirFormula).SubmergeLimitPos);
        }
        
    }
}