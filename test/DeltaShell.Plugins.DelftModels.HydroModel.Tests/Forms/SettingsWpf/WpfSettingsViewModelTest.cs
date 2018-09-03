using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    public class WpfSettingsViewModelTest
    {
        [Test]
        public void Test_WpfSettingsViewModel()
        {
            var viewModel = new WpfSettingsViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.SettingsCategories);
        }

        [Test]
        public void Test_SettingsCategories_ShowOnly_VisibleCategories()
        {
            var viewModel = new WpfSettingsViewModel();
            Assert.IsNotNull(viewModel);

            var wpfGuiCategoryVisible = new WpfGuiCategory("cat", null);
            var wpfGuiCategoryHidden = new WpfGuiCategory("cat2", null)
            {
                CategoryVisibility = () => false,
            };

            viewModel.SettingsCategories.AddRange(new List<WpfGuiCategory>{wpfGuiCategoryHidden, wpfGuiCategoryVisible});
            Assert.IsTrue( viewModel.SettingsCategories.Any());

            Assert.IsTrue( viewModel.SettingsCategories.Contains(wpfGuiCategoryVisible));
            Assert.IsFalse( viewModel.SettingsCategories.Contains(wpfGuiCategoryHidden));
        }
    }
}