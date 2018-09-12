using DelftTools.TestUtils;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors
{
    [TestFixture]
    public class WaveModelViewWPFTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test_WaterFlowFMModelViewWPF()
        {
            var waveModel = new WaveModel();
            var viewWpf = new WpfSettingsView
            {
                Data = waveModel
            };

            var wpfSettingsViewModel = (WpfSettingsViewModel)viewWpf.DataContext;
            SetUiProperties(waveModel, wpfSettingsViewModel);

            WpfTestHelper.ShowModal(viewWpf);

            var props = waveModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        private static void SetUiProperties(WaveModel model, WpfSettingsViewModel settings)
        {
            settings.SettingsCategories = WaveSettingsHelper.GetWpfGuiCategories(model, null);
        }
    }
}
