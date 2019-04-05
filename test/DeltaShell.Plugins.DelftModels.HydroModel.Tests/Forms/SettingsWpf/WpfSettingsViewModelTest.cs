using System.Collections.ObjectModel;
using System.Linq;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
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

            viewModel.SettingsCategories = new ObservableCollection<WpfGuiCategory>{ wpfGuiCategoryHidden , wpfGuiCategoryVisible };
            Assert.IsTrue( viewModel.SettingsCategories.Any());

            Assert.IsTrue( viewModel.SettingsCategories.Contains(wpfGuiCategoryVisible));
            Assert.IsFalse( viewModel.SettingsCategories.Contains(wpfGuiCategoryHidden));
        }
    }
}