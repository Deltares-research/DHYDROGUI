using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Collections.Extensions;
using DeltaShell.NGHS.Common.Gui.WPF.SettingsView;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Gui.Tests.WPF.SettingsView
{
    [TestFixture]
    public class SettingsViewModelTest
    {
        [Test]
        public void Test_WpfSettingsViewModel()
        {
            var viewModel = new SettingsViewModel();
            Assert.IsNotNull(viewModel);
            Assert.IsNotNull(viewModel.SettingsCategories);
        }

        [Test]
        [Category("Quarantine")]
        public void Test_SettingsCategories_ShowOnly_VisibleCategories()
        {
            var viewModel = new SettingsViewModel();
            Assert.IsNotNull(viewModel);

            var wpfGuiCategoryVisible = new GuiCategory("cat", null);
            var wpfGuiCategoryHidden = new GuiCategory("cat2", null)
            {
                CategoryVisibility = () => false,
            };

            viewModel.SettingsCategories.AddRange(new List<GuiCategory>{wpfGuiCategoryHidden, wpfGuiCategoryVisible});
            Assert.IsTrue( viewModel.SettingsCategories.Any());

            Assert.IsTrue( viewModel.SettingsCategories.Contains(wpfGuiCategoryVisible));
            Assert.IsFalse( viewModel.SettingsCategories.Contains(wpfGuiCategoryHidden));
        }
    }
}