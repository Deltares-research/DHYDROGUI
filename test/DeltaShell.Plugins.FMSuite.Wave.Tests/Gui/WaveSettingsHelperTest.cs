using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui
{
    [TestFixture]
    public class WaveSettingsHelperTest
    {
        [TestCase(true)]
        [TestCase(false)]
        public void GetWpfGuiCategories_ComFileWpfGuiPropertyEnabledIsDependentOnCouplingToFmModel(bool coupledToFlow)
        {
            // Arrange
            var waveModel = new WaveModel();
            ObservableCollection<WpfGuiCategory> wpfGuiCategories = WaveSettingsHelper.GetWpfGuiCategories(waveModel, Substitute.For<IGui>());

            // Act
            if (coupledToFlow)
            {
                waveModel.Owner = Substitute.For<ICompositeActivity>();
            }

            // Assert
            WpfGuiProperty comFileProperty = wpfGuiCategories.SelectMany(c => c.Properties)
                                                             .Single(p => p.Name == KnownWaveProperties.COMFile);
            Assert.That(comFileProperty.IsEnabled, Is.EqualTo(!coupledToFlow));
        }
    }
}