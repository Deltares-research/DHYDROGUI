using System.Threading;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WeirViewWpfTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var view = new WeirViewWpf();
            var weir = new Weir("weir", true)
            {
                WeirFormula = new SimpleWeirFormula(),
            };

            WpfTestHelper.ShowModal(view, () =>
            {
                view.Data = weir;
            });
        }

        [Test, Category(TestCategory.WindowsForms)]
        public void ShowSimpleWeirFormulaViewWpf()
        {
            var view = new SimpleWeirFormulaViewWpf();
            WpfTestHelper.ShowModal(view);
        }

        [Test, Category(TestCategory.WindowsForms)]
        public void ShowGeneralStructureWeirFormulaViewWpf()
        {
            var view = new GeneralStructureWeirFormulaViewWpf();
            WpfTestHelper.ShowModal(view);
        }
    }
}