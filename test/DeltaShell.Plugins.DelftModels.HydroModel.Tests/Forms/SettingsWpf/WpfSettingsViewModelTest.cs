using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NSubstitute;
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

        [Test]
        public void Dispose_DisposesCustomControls()
        {
            // Setup
            IDisposable customControl = Substitute.For<IDisposable, FrameworkElement>();
            var category = new WpfGuiCategory("category_name", Enumerable.Empty<FieldUIDescription>().ToList())
            {
                CustomControl = (FrameworkElement) customControl
            };

            var viewModel = new WpfSettingsViewModel();
            viewModel.SettingsCategories.Add(category);

            // Call
            viewModel.Dispose();

            // Assert
            customControl.Received(1).Dispose();
        }
    }
}