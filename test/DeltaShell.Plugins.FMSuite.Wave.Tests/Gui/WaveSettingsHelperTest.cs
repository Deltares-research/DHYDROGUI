using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        [Test]
        public void GetWpfGuiCategories_DataNull_ThrowsArgumentNullException()
        {
            // Call
            void Call() => WaveSettingsHelper.GetWpfGuiCategories(null, Substitute.For<IGui>());

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("data"));
        }

        [Test]
        public void GetWpfGuiCategories_ReturnsCorrectResult()
        {
            // Setup
            var waveModel = new WaveModel();

            // Call
            ObservableCollection<WpfGuiCategory> wpfGuiCategories = WaveSettingsHelper.GetWpfGuiCategories(waveModel, Substitute.For<IGui>());

            // Assert
            Assert.That(wpfGuiCategories, Has.Count.EqualTo(6));

            AssertCategoryExists(wpfGuiCategories, "General");
            AssertCategoryExists(wpfGuiCategories, "Spectral Domain");
            AssertCategoryExists(wpfGuiCategories, "Physical Processes");
            AssertCategoryExists(wpfGuiCategories, "Numerical Parameters");
            AssertCategoryExists(wpfGuiCategories, "Output");
            AssertCategoryExists(wpfGuiCategories, "Domain specific settings");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void GetWpfGuiCategories_ComFileWpfGuiPropertyEnabledIsDependentOnCouplingToFmModel(bool coupledToFlow)
        {
            // Arrange
            var waveModel = new WaveModel();
            ObservableCollection<WpfGuiCategory> wpfGuiCategories = WaveSettingsHelper.GetWpfGuiCategories(waveModel, Substitute.For<IGui>());

            // Act
            waveModel.IsCoupledToFlow = coupledToFlow;

            // Assert
            WpfGuiProperty comFileProperty = wpfGuiCategories.SelectMany(c => c.Properties)
                                                             .Single(p => p.Name == KnownWaveProperties.COMFile);
            Assert.That(comFileProperty.IsEnabled, Is.EqualTo(!coupledToFlow));
        }

        private static void AssertCategoryExists(IEnumerable<WpfGuiCategory> wpfGuiCategories, string categoryName)
        {
            WpfGuiCategory category = wpfGuiCategories.FirstOrDefault(c => c.CategoryName == categoryName);
            Assert.That(category, Is.Not.Null, $"Category '{categoryName}' does not exist.");
        }
    }
}