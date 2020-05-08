using System;
using System.Linq;
using System.Windows;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Tests.Forms.SettingsWpf
{
    [TestFixture]
    [Category(TestCategory.Wpf)]
    public class WpfSettingsViewTest
    {
        [Test]
        public void Dispose_CustomControlsViewModelAreDisposed()
        {
            // Setup
            IDisposable customControl = Substitute.For<IDisposable, FrameworkElement>();
            var category = new WpfGuiCategory("category_name", Enumerable.Empty<FieldUIDescription>().ToList()) {CustomControl = (FrameworkElement) customControl};

            var viewModel = new WpfSettingsViewModel();
            viewModel.SettingsCategories.Add(category);

            var view = new WpfSettingsView {ViewModel = viewModel};

            // Call
            view.Dispose();

            // Assert
            customControl.Received(1).Dispose();
        }
    }
}