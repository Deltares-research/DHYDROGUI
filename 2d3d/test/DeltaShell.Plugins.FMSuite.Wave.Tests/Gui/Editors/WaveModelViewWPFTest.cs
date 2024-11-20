using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors
{
    [TestFixture]
    public class WaveModelViewWPFTest
    {
        [Test]
        [Category(TestCategory.Wpf)]
        public void Test_WaterFlowFMModelViewWPF()
        {
            var waveModel = new WaveModel();
            var viewWpf = new WpfSettingsView {Data = waveModel};

            var wpfSettingsViewModel = (WpfSettingsViewModel) viewWpf.DataContext;
            SetUiProperties(waveModel, wpfSettingsViewModel);

            WpfTestHelper.ShowModal(viewWpf);

            IEventedList<WaveModelProperty> props = waveModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        private static void SetUiProperties(WaveModel model, WpfSettingsViewModel settings)
        {
            settings.SettingsCategories = WaveSettingsHelper.GetWpfGuiCategories(model, null);
        }
    }
}