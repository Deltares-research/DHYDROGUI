using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.TestUtils;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView.WeirFormulaViews;
using NUnit.Framework;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.StructureFeatureView.WeirFormulaViews
{
    [TestFixture]
    public class GeneralStructureWeirFormulaViewTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Show()
        {
            var view = new GeneralStructureWeirFormulaView
                           {
                               Data = new GeneralStructureWeirFormula()
                           };
            WindowsFormsTestHelper.ShowModal(view);
        }

    }
}