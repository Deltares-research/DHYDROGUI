using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Hydro;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Buttons;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public static class WaveSettingsHelper
    {
        public static ObservableCollection<WpfGuiCategory> GetWpfGuiCategories(WaveModel data, IGui gui)
        {
            var wpfGuiCategories = new List<WpfGuiCategory>();
            if (data != null)
            {
                wpfGuiCategories = GetWaveSettings(data)?.FieldDescriptions
                                                        .GroupBy(fd => fd.Category)
                                                        .Select(gp => new WpfGuiCategory(gp.Key, gp.ToList()))
                                                        .ToList();
                wpfGuiCategories?.SelectMany(gp => gp.Properties).Distinct().ForEach(p => p.GetModel = () => data);

                ModifyWaveSettings(wpfGuiCategories);
            }
            
            AddCustomWaveSettings(data, gui, wpfGuiCategories);
            return new ObservableCollection<WpfGuiCategory>(wpfGuiCategories);
        }

        private static void ModifyWaveSettings(IEnumerable<WpfGuiCategory> wpfGuiCategories)
        {
            WpfGuiProperty comFileGuiProperty = wpfGuiCategories.SelectMany(c => c.Properties)
                                                                .Single(p => p.Name == KnownWaveProperties.COMFile);
            comFileGuiProperty.CustomCommand.ButtonFunction = SelectComFileButton.ButtonAction;
        }

        private static void AddCustomWaveSettings(IHasCoordinateSystem model, IGui gui, IEnumerable<WpfGuiCategory> wpfCategories)
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
                        HasMinValue = false,
                    });

                coordSys.CustomCommand.TextBoxEnabled = false;
                coordSys.CustomCommand.ButtonFunction = o => SetCoordinateSystemButton.ButtonAction(o, gui, WaveModel.IsValidCoordinateSystem);
                coordSys.CustomCommand.ButtonImage = SetCoordinateSystemButton.ButtonImage;
                generalCategory.AddWpfGuiProperty(coordSys);
            }
        }

        private static ObjectUIDescription GetWaveSettings(WaveModel data)
        {
            ObjectUIDescription objectDescription = WaveModelUIDescription.Extract(data);

            var flowCouplingCheckBox = new FieldUIDescription(o => data.IsCoupledToFlow,
                                                              (d, v) => data.IsCoupledToFlow = (bool) v)
            {
                Category = KnownWaveCategories.GeneralCategory,
                SubCategory = "Data from D-Flow FM",
                Label = "Coupled to D-Flow FM",
                Name = "IsCoupledToFlow",
                ValueType = typeof(bool),
                ToolTip = "When enabled, run coupled to D-Flow FM core"
            };

            var startTime = new FieldUIDescription(o => data.StartTime, (d, v) => data.StartTime = (DateTime) v,
                                                   (d) => data.IsCoupledToFlow)
            {
                Category = KnownWaveCategories.GeneralCategory,
                SubCategory = "Coupling time frame",
                Label = "Start time",
                Name = "StartTime",
                ValueType = typeof(DateTime),
                ToolTip = "Start time within the coupled model run"
            };

            var stopTime = new FieldUIDescription(o => data.StopTime, (d, v) => data.StopTime = (DateTime) v,
                                                  (d) => data.IsCoupledToFlow)
            {
                Category = KnownWaveCategories.GeneralCategory,
                SubCategory = "Coupling time frame",
                Label = "Stop time",
                Name = "StopTime",
                ValueType = typeof(DateTime),
                ToolTip = "Stop time within the coupled model run",
                ValidationMethod = ValidateCoupledTime
            };

            var timeStep = new FieldUIDescription(o => data.TimeStep, (d, v) => data.TimeStep = (TimeSpan) v,
                                                  (d) => data.IsCoupledToFlow)
            {
                Category = KnownWaveCategories.GeneralCategory,
                SubCategory = "Coupling time frame",
                Label = "Time step",
                Name = "TimeStep",
                ValueType = typeof(TimeSpan),
                ToolTip = "Coupling time step"
            };

            FieldUIDescription fieldDescription = objectDescription.FieldDescriptions.Single(fd => fd.Name == KnownWaveProperties.COMFile);
            fieldDescription.SetIsEnabledFunc(d => !data.IsCoupledToFlow);

            objectDescription.FieldDescriptions =
                new[]
                    {
                        /*setCoordinateSystemBtn,*/
                        flowCouplingCheckBox,
                        startTime,
                        stopTime,
                        timeStep
                    }.Concat(objectDescription.FieldDescriptions)
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