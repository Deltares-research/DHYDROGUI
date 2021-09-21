using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Gui.Editors
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class WaterFlowFMModelViewWPFTest
    {

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test_WaterFlowFMModelViewWPF()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWPF = new WpfSettingsView()
            {
                Data = fmModel,
            };

            var wpfSettingsViewModel = (WpfSettingsViewModel)fmViewWPF.DataContext;
            SetUiProperties(fmModel, wpfSettingsViewModel);

            WpfTestHelper.ShowModal(fmViewWPF);

            var props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test_WaterFlowFMModelViewWPF_AddExtras_Category_Sediment()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWpf = new WpfSettingsView
            {
                Data = fmModel,
            };
            var wpfSettingsViewModel = (WpfSettingsViewModel)fmViewWpf.DataContext;

            SetUiProperties(fmModel, wpfSettingsViewModel);
            var fieldUi = new FieldUIDescription(o => fmModel.UseMorSed, null, o => true, o =>
            {
                var waterFlowFmModel = o as WaterFlowFMModel;
                return waterFlowFmModel != null && waterFlowFmModel.UseMorSed;
            });

            var fieldUiDescriptions = new List<FieldUIDescription>();
            fieldUiDescriptions.Add(fieldUi);

            var cat = new WpfGuiCategory("Sediment", fieldUiDescriptions);
            var sedProperty = cat.Properties.FirstOrDefault();
            Assert.IsNotNull(sedProperty);
            sedProperty.CustomControl = new SedimentFractionsEditor(fmModel.SedimentFractions, fmModel.SedimentOverallProperties);
            sedProperty.GetModel = () => fmModel;

            wpfSettingsViewModel.SettingsCategories.Add(cat);
            Assert.IsTrue(wpfSettingsViewModel.SettingsCategories.Contains(cat));

            WpfTestHelper.ShowModal(fmViewWpf);

            var props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test_WaterFlowFMModelViewWPF_AddExtras_Property()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWpf = new WpfSettingsView
            {
                Data = fmModel
            };

            var wpfSettingsViewModel = (WpfSettingsViewModel)fmViewWpf.DataContext;
            Func<object, bool> isEnabledFunc = o => true;
            Func<object, bool> isVisibleFunc = o => (o is WaterFlowFMModel) && (o as WaterFlowFMModel).DepthLayerDefinition != null;
            var depthlayers = new FieldUIDescription(d => fmModel.DepthLayerDefinition?.Description, null, isEnabledFunc, isVisibleFunc)
            {
                Category = "General",
                ToolTip = "Adjust layers",
                Label = "Layer",
                ValueType = typeof(string),
                IsReadOnly = false,
                HasMaxValue = false,
                HasMinValue = false,
            };

            Assert.IsTrue(depthlayers.IsVisible(fmModel));
            var wpfGuiCategory = new WpfGuiCategory("General", new List<FieldUIDescription>() { depthlayers });
            var prop = wpfGuiCategory.Properties.FirstOrDefault();
            wpfGuiCategory.Properties.ForEach(p => p.GetModel = () => fmModel);
            wpfSettingsViewModel.SettingsCategories = new ObservableCollection<WpfGuiCategory>(
                new List<WpfGuiCategory> { wpfGuiCategory });

            Assert.IsNotNull(prop);
            prop.CustomCommand.ButtonFunction = (o) => EditDepthLayersHelper.ButtonAction(o);
            prop.CustomCommand.ButtonImage = EditDepthLayersHelper.ButtonImage;
            WpfTestHelper.ShowModal(fmViewWpf);

            var props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test_IsEnabled_WaterFlowFMModelViewWPF()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWPF = new WpfSettingsView()
            {
                Data = fmModel,
            };

            var wpfSettingsViewModel = (WpfSettingsViewModel)fmViewWPF.DataContext;
            SetUiProperties(fmModel, wpfSettingsViewModel);

            fmModel.ModelDefinition.Properties.ForEach(p => p.PropertyDefinition.IsEnabled = (c) => false);

            WpfTestHelper.ShowModal(fmViewWPF);

            var props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        [Test]
        [Category(TestCategory.WindowsForms)]
        public void Test_IsVisible_WaterFlowFMModelViewWPF()
        {
            var fmModel = new WaterFlowFMModel();
            var fmViewWPF = new WpfSettingsView()
            {
                Data = fmModel,
            };

            var wpfSettingsViewModel = (WpfSettingsViewModel)fmViewWPF.DataContext;
            SetUiProperties(fmModel, wpfSettingsViewModel);

            fmModel.ModelDefinition.Properties.ForEach(p => p.PropertyDefinition.IsVisible = (c) => false);

            WpfTestHelper.ShowModal(fmViewWPF);

            var props = fmModel.ModelDefinition.Properties;
            Assert.IsNotNull(props);
        }

        private void SetUiProperties(WaterFlowFMModel model, WpfSettingsViewModel settings)
        {
            settings.SettingsCategories = WaterFlowFmSettingsHelper.GetWpfGuiCategories(model, null);
        }
    }
}