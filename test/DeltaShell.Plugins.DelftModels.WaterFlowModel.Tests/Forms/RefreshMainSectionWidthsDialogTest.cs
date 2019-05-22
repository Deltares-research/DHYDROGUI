using System.Collections.Generic;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests.Forms
{
    [TestFixture]
    public class RefreshMainSectionWidthsDialogTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void ShowDialogWithData()
        {
            var dialog = new RefreshMainSectionWidthsDialog();

            var branch = new Channel{Name = "Channel 1"};
            dialog.Data = new List<ICrossSection>
            {
                new CrossSection(new CrossSectionDefinitionYZ()) {Branch = branch, Chainage = 0},
                new CrossSection(new CrossSectionDefinitionXYZ()){Branch = branch, Chainage = 20},
                new CrossSection(new CrossSectionDefinitionZW()){Branch = branch, Chainage = 40},
                new CrossSection(new CrossSectionDefinitionZW()){Chainage = 60}
            };

            WpfTestHelper.ShowModal(dialog);
        }
    }
}