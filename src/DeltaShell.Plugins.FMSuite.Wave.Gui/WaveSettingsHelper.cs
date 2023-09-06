using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Controls;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.Views;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Properties;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public static class WaveSettingsHelper
    {
        private static readonly IFileDialogService fileDialogService = new FileDialogService();

        public static ObservableCollection<WpfGuiCategory> GetWpfGuiCategories(WaveModel data, IGui gui)
        {
            Ensure.NotNull(data, nameof(data));

            List<WpfGuiCategory> wpfGuiCategories = GetWaveSettings(data).FieldDescriptions
                                                                         .GroupBy(fd => fd.Category)
                                                                         .Select(gp => new WpfGuiCategory(gp.Key, gp.ToList()))
                                                                         .ToList();

            wpfGuiCategories.SelectMany(gp => gp.Properties).Distinct().ForEach(p => p.GetModel = () => data);

            ModifyWaveSettings(wpfGuiCategories);
            wpfGuiCategories.Add(GetDomainSpecificSettingsCategory(data));
            AddCustomWaveSettings(data, gui, wpfGuiCategories);

            return new ObservableCollection<WpfGuiCategory>(wpfGuiCategories);
        }

        private static void ModifyWaveSettings(IEnumerable<WpfGuiCategory> wpfGuiCategories)
        {
            SetButtonBehaviour(wpfGuiCategories, KnownWaveProperties.COMFile, new SelectComFileButton(fileDialogService));
            SetButtonBehaviour(wpfGuiCategories, KnownWaveProperties.InputTemplateFile, new SelectInputTemplateFileButton(fileDialogService));
        }

        private static void SetButtonBehaviour(IEnumerable<WpfGuiCategory> wpfGuiCategories, string propertyName,IButtonBehaviour buttonBehaviour)
        {
            WpfGuiProperty property = wpfGuiCategories.SelectMany(c => c.Properties)
                                                                          .Single(p => p.Name == propertyName);
            property.CustomCommand.ButtonBehaviour = buttonBehaviour;
        }

        private static void AddCustomWaveSettings(WaveModel model, IGui gui, IEnumerable<WpfGuiCategory> wpfCategories)
        {
            WpfGuiCategory generalCategory =
                wpfCategories.FirstOrDefault(c => c.CategoryName.ToLower().Equals("general"));
            if (generalCategory != null)
            {
                //Gui Coordinate        
                var coordSys = new WpfGuiProperty(
                    new FieldUIDescription(d => SetCoordinateSystemButton.CoordinateSystemName(model), null)
                    {
                        Category = "General",
                        ToolTip = SetCoordinateSystemButton.ToolTip,
                        Label = SetCoordinateSystemButton.Label,
                        ValueType = typeof(string),
                        HasMaxValue = false,
                        HasMinValue = false
                    });
                coordSys.CustomCommand.TextBoxEnabled = false;
                coordSys.CustomCommand.ButtonBehaviour = new SetCoordinateSystemButton(gui, WaveModel.IsValidCoordinateSystem);
                coordSys.CustomCommand.ButtonImage = SetCoordinateSystemButton.ButtonImage;
                generalCategory.AddWpfGuiProperty(coordSys);

                var waveBoundariesPerFileUsed = new WpfGuiProperty(new FieldUIDescription(o => model.ModelDefinition.BoundaryContainer.DefinitionPerFileUsed,
                                                                                          (d, v) => model.ModelDefinition.BoundaryContainer.DefinitionPerFileUsed = (bool) v)
                {
                    Category = KnownWaveSections.GeneralSection,
                    SubCategory = Resources.WaveSettingsHelper_AddCustomWaveSettings_Boundaries_Category_Name,
                    Label = Resources.WaveSettingsHelper_AddCustomWaveSettings_Use_SWAN_domain_boundary_from_file,
                    ValueType = typeof(bool),
                    ToolTip = Resources.WaveSettingsHelper_AddCustomWaveSettings_When_this_option_is_selected_adding_2D_D_Waves_boundaries_is_not_possible_Existing_2D_D_Waves_boundaries_will_be_removed
                });
                generalCategory.AddWpfGuiProperty(waveBoundariesPerFileUsed);

                var waveBoundariesPerFileName = new WpfGuiProperty(new FieldUIDescription(o => model.ModelDefinition.BoundaryContainer.FilePathForBoundariesPerFile,
                                                                                          (d, v) => model.ModelDefinition.BoundaryContainer.FilePathForBoundariesPerFile = (string) v,
                                                                                          d => model.BoundaryContainer.DefinitionPerFileUsed)
                {
                    Category = KnownWaveSections.GeneralSection,
                    SubCategory = Resources.WaveSettingsHelper_AddCustomWaveSettings_Boundaries_Category_Name,
                    Label = Resources.WaveSettingsHelper_AddCustomWaveSettings_Spectrum_File,
                    ValueType = typeof(string),
                    ToolTip = string.Empty
                });
                waveBoundariesPerFileName.CustomCommand.TextBoxEnabled = false;
                waveBoundariesPerFileName.CustomCommand.ButtonBehaviour = new SelectSp2FileButton(fileDialogService);
                generalCategory.AddWpfGuiProperty(waveBoundariesPerFileName);
            }
        }

        private static WpfGuiCategory GetDomainSpecificSettingsCategory(WaveModel model)
        {
            return new WpfGuiCategory(Wave.Properties.Resources.Wave_Domain_specific_settings,
                                      Enumerable.Empty<FieldUIDescription>().ToList()) {CustomControl = new MainDomainSpecificDataView(new MainDomainSpecificDataViewModel(model.OuterDomain))};
        }

        private static ObjectUIDescription GetWaveSettings(WaveModel data)
        {
            ObjectUIDescription objectDescription = WaveModelUIDescription.Extract(data);

            var flowCouplingCheckBox = new FieldUIDescription(o => data.IsCoupledToFlow,
                                                              (d, v) => data.IsCoupledToFlow = (bool) v)
            {
                Category = KnownWaveSections.GeneralSection,
                SubCategory = Resources.WaveSettingsHelper_GetWaveSettings_Online_Coupling_Time_Frame,
                Label = Resources.WaveSettingsHelper_GetWaveSettings_Coupled_to_D_Flow_FM,
                Name = "IsCoupledToFlow",
                ValueType = typeof(bool),
                ToolTip = Resources.WaveSettingsHelper_GetWaveSettings_When_enabled__run_coupled_to_D_Flow_FM_core
            };

            var startTime = new FieldUIDescription(o => data.StartTime, (d, v) => data.StartTime = (DateTime) v,
                                                   d => data.IsCoupledToFlow)
            {
                Category = KnownWaveSections.GeneralSection,
                SubCategory = Resources.WaveSettingsHelper_GetWaveSettings_Online_Coupling_Time_Frame,
                Label = Resources.WaveSettingsHelper_GetWaveSettings_Coupling_start_time,
                Name = "StartTime",
                ValueType = typeof(DateTime),
                ToolTip = Resources.WaveSettingsHelper_GetWaveSettings_Start_time_within_the_coupled_model_run
            };

            var stopTime = new FieldUIDescription(o => data.StopTime, (d, v) => data.StopTime = (DateTime) v,
                                                  d => data.IsCoupledToFlow)
            {
                Category = KnownWaveSections.GeneralSection,
                SubCategory = Resources.WaveSettingsHelper_GetWaveSettings_Online_Coupling_Time_Frame,
                Label = Resources.WaveSettingsHelper_GetWaveSettings_Coupling_stop_time,
                Name = "StopTime",
                ValueType = typeof(DateTime),
                ToolTip = Resources.WaveSettingsHelper_GetWaveSettings_Stop_time_within_the_coupled_model_run,
                ValidationMethod = ValidateCoupledTime
            };

            var timeStep = new FieldUIDescription(o => data.TimeStep, (d, v) => data.TimeStep = (TimeSpan) v,
                                                  d => data.IsCoupledToFlow)
            {
                Category = KnownWaveSections.GeneralSection,
                SubCategory = Resources.WaveSettingsHelper_GetWaveSettings_Online_Coupling_Time_Frame,
                Label = Resources.WaveSettingsHelper_GetWaveSettings_Coupling_time_step,
                Name = "TimeStep",
                ValueType = typeof(TimeSpan),
                ToolTip = Resources.WaveSettingsHelper_GetWaveSettings_Time_step_within_the_coupled_model_run
            };

            FieldUIDescription fieldDescription = objectDescription.FieldDescriptions.Single(fd => fd.Name == KnownWaveProperties.COMFile);
            fieldDescription.SetIsEnabledFunc(d => !data.IsCoupledToFlow);

            FieldUIDescription[] couplingFieldDescriptions =
            {
                flowCouplingCheckBox,
                startTime,
                stopTime,
                timeStep
            };

            objectDescription.FieldDescriptions = objectDescription.FieldDescriptions.Concat(couplingFieldDescriptions)
                                                                   .ToList();

            return objectDescription;
        }

        private static string ValidateCoupledTime(object waveModelObject, object dateTimeObject)
        {
            var waveModel = (WaveModel) waveModelObject;
            var dateTime = (DateTime) dateTimeObject;

            if (waveModel.StartTime < dateTime || !waveModel.IsCoupledToFlow)
            {
                return string.Empty;
            }

            return "Coupling start time must be smaller than coupling stop time.";
        }
    }
}