using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Shell.Gui;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Wave.Gui;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
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
            AssertCategoryExists(wpfGuiCategories, "Output Parameters");
            AssertCategoryExists(wpfGuiCategories, "Domain specific settings");
        }

        [Test]
        public void GetWpfGuiCategories_ShouldContainBoundariesSubCategoryInGeneralCategory()
        {
            // Setup
            var waveModel = new WaveModel();

            // Call
            ObservableCollection<WpfGuiCategory> wpfGuiCategories = WaveSettingsHelper.GetWpfGuiCategories(waveModel, Substitute.For<IGui>());

            // Assert
            WpfGuiCategory generalCategory = wpfGuiCategories.First(c => c.CategoryName == "General");

            WpfGuiSubCategory boundarySubCategory = generalCategory.SubCategories.FirstOrDefault(sc => sc.SubCategoryName == Resources.WaveSettingsHelper_AddCustomWaveSettings_Boundaries_Category_Name);
            Assert.NotNull(boundarySubCategory, "Subcategory boundaries does not exist in General");

            ObservableCollection<WpfGuiProperty> boundaryProperties = boundarySubCategory.Properties;
            Assert.AreEqual(2, boundaryProperties.Count);
            Assert.AreEqual(Resources.WaveSettingsHelper_AddCustomWaveSettings_Use_SWAN_domain_boundary_from_file, boundaryProperties[0].Label);
            Assert.AreEqual(typeof(bool), boundaryProperties[0].ValueType);
            Assert.AreEqual(Resources.WaveSettingsHelper_AddCustomWaveSettings_When_this_option_is_selected_adding_2D_D_Waves_boundaries_is_not_possible_Existing_2D_D_Waves_boundaries_will_be_removed, boundaryProperties[0].ToolTip);

            Assert.AreEqual(Resources.WaveSettingsHelper_AddCustomWaveSettings_Spectrum_File, boundaryProperties[1].Label);
            Assert.AreEqual(typeof(string), boundaryProperties[1].ValueType);
            Assert.AreEqual(string.Empty, boundaryProperties[1].ToolTip);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void GetWpfGuiCategories_SpectrumFileNameShouldBeEnabledDependentOnBoundaryDefinitionPerFileUsed(bool useDefinitionFile)
        {
            var waveModel = new WaveModel();
            ObservableCollection<WpfGuiCategory> wpfGuiCategories = WaveSettingsHelper.GetWpfGuiCategories(waveModel, Substitute.For<IGui>());

            // Act
            waveModel.BoundaryContainer.DefinitionPerFileUsed = useDefinitionFile;

            // Assert
            WpfGuiProperty fileNameProperty = wpfGuiCategories.SelectMany(c => c.Properties)
                                                              .Single(p => p.Label == Resources.WaveSettingsHelper_AddCustomWaveSettings_Spectrum_File);
            Assert.That(fileNameProperty.IsEnabled, Is.EqualTo(useDefinitionFile));
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