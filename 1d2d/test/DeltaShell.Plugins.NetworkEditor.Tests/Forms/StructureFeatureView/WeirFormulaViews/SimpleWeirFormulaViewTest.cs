using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView.WeirFormulaViews
{
    [TestFixture]
    public class SimpleWeirFormulaViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowWeirView()
        {
            var simpleWeirFormula = new SimpleWeirFormula();
            var simpleWeirFormulaView = new SimpleWeirFormulaView
                                            {
                                                Data = simpleWeirFormula
                                            };

            WindowsFormsTestHelper.ShowModal(simpleWeirFormulaView);
        }
    }
}