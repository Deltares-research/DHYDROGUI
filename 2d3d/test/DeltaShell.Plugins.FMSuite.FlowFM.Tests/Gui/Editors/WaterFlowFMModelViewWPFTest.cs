using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Shell.Gui;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Editors
{
    [TestFixture]
    public class WaterFlowFMModelViewWPFTest
    {
        [Test]
        [Category(TestCategory.Wpf)]
        public void Test_WaterFlowFMModelViewWPF()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWPF = new WpfSettingsView() {Data = fmModel};

            var wpfSettingsViewModel = (WpfSettingsViewModel) fmViewWPF.DataContext;
            SetUiProperties(fmModel, wpfSettingsViewModel);

            WpfTestHelper.ShowModal(fmViewWPF);

            IEventedList<WaterFlowFMProperty> props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Test_WaterFlowFMModelViewWPF_AddExtras_Category_Sediment()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWpf = new WpfSettingsView {Data = fmModel};
            var wpfSettingsViewModel = (WpfSettingsViewModel) fmViewWpf.DataContext;

            SetUiProperties(fmModel, wpfSettingsViewModel);
            var fieldUi = new FieldUIDescription(o => fmModel.UseMorSed, null, o => true, o =>
            {
                var waterFlowFmModel = o as WaterFlowFMModel;
                return waterFlowFmModel != null && waterFlowFmModel.UseMorSed;
            });

            var fieldUiDescriptions = new List<FieldUIDescription>();
            fieldUiDescriptions.Add(fieldUi);

            var cat = new WpfGuiCategory("Sediment", fieldUiDescriptions);
            WpfGuiProperty sedProperty = cat.Properties.FirstOrDefault();
            Assert.IsNotNull(sedProperty);
            sedProperty.GetModel = () => fmModel;

            wpfSettingsViewModel.SettingsCategories.Add(cat);
            Assert.IsTrue(wpfSettingsViewModel.SettingsCategories.Contains(cat));

            WpfTestHelper.ShowModal(fmViewWpf);

            IEventedList<WaterFlowFMProperty> props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Test_WaterFlowFMModelViewWPF_AddExtras_Property()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWpf = new WpfSettingsView {Data = fmModel};

            var wpfSettingsViewModel = (WpfSettingsViewModel) fmViewWpf.DataContext;
            Func<object, bool> isEnabledFunc = o => true;
            Func<object, bool> isVisibleFunc = o => o is WaterFlowFMModel && (o as WaterFlowFMModel).DepthLayerDefinition != null;
            var depthlayers = new FieldUIDescription(d => fmModel.DepthLayerDefinition?.Description, null, isEnabledFunc, isVisibleFunc)
            {
                Category = "General",
                ToolTip = EditDepthLayersHelper.ToolTip,
                Label = EditDepthLayersHelper.Label,
                ValueType = typeof(string),
                IsReadOnly = false,
                HasMaxValue = false,
                HasMinValue = false
            };

            Assert.IsTrue(depthlayers.IsVisible(fmModel));
            var wpfGuiCategory = new WpfGuiCategory("General", new List<FieldUIDescription>() {depthlayers});
            WpfGuiProperty prop = wpfGuiCategory.Properties.FirstOrDefault();
            wpfGuiCategory.Properties.ForEach(p => p.GetModel = () => fmModel);
            wpfSettingsViewModel.SettingsCategories = new ObservableCollection<WpfGuiCategory>(
                new List<WpfGuiCategory> {wpfGuiCategory});

            Assert.IsNotNull(prop);
            prop.CustomCommand.ButtonBehaviour = new EditDepthLayersHelper();
            prop.CustomCommand.ButtonImage = EditDepthLayersHelper.ButtonImage;
            WpfTestHelper.ShowModal(fmViewWpf);

            IEventedList<WaterFlowFMProperty> props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Test_IsEnabled_WaterFlowFMModelViewWPF()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWPF = new WpfSettingsView() {Data = fmModel};

            var wpfSettingsViewModel = (WpfSettingsViewModel) fmViewWPF.DataContext;
            SetUiProperties(fmModel, wpfSettingsViewModel);

            fmModel.ModelDefinition.Properties.ForEach(p => p.PropertyDefinition.IsEnabled = (c) => false);

            WpfTestHelper.ShowModal(fmViewWPF);

            IEventedList<WaterFlowFMProperty> props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.Wpf)]
        public void Test_IsVisible_WaterFlowFMModelViewWPF()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWPF = new WpfSettingsView() {Data = fmModel};

            var wpfSettingsViewModel = (WpfSettingsViewModel) fmViewWPF.DataContext;
            SetUiProperties(fmModel, wpfSettingsViewModel);

            fmModel.ModelDefinition.Properties.ForEach(p => p.PropertyDefinition.IsVisible = (c) => false);

            WpfTestHelper.ShowModal(fmViewWPF);

            IEventedList<WaterFlowFMProperty> props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        /// <summary>
        /// WHEN GetWpfGuiCategories is called with a null model
        /// THEN an empty collection is returned
        /// </summary>
        [Test]
        public void WhenGetWpfGuiCategoriesIsCalledWithANullModel_ThenAnEmptyCollectionIsReturned()
        {
            // Given
            var guiStub = MockRepository.GenerateStub<IGui>();

            // When
            ObservableCollection<WpfGuiCategory> result = WaterFlowFmSettingsHelper.GetWpfGuiCategories(null, guiStub);

            // Then
            Assert.That(result, Is.Not.Null, "Expected a non null result.");
            Assert.That(result, Is.Empty, "Expected an empty collection.");
        }

        private void SetUiProperties(WaterFlowFMModel model, WpfSettingsViewModel settings)
        {
            settings.SettingsCategories = WaterFlowFmSettingsHelper.GetWpfGuiCategories(model, null);
        }
    }
}