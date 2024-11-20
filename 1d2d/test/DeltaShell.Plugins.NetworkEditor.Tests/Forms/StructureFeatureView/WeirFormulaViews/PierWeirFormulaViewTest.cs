using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView.WeirFormulaViews
{
    [TestFixture]
    public class PierWeirFormulaViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirView()
        {
            var pierWeirFormula = new PierWeirFormula();
            var pierWeirFormulaView = new PierWeirFormulaView
                                          {
                                              Data = pierWeirFormula
                                          };

            WindowsFormsTestHelper.ShowModal(pierWeirFormulaView);
        }
    }
}