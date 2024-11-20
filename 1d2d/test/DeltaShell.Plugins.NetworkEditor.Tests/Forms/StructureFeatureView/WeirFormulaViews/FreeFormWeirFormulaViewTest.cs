using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView.WeirFormulaViews
{
    [TestFixture]
    public class FreeFormWeirFormulaViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirView()
        {
            var freeWeirFormula = new FreeFormWeirFormula();
            var freeWeirFormulaView = new FreeFormWeirFormulaView
                                          {
                                              Data = freeWeirFormula
                                          };

            WindowsFormsTestHelper.ShowModal(freeWeirFormulaView);
        }
    }
}
