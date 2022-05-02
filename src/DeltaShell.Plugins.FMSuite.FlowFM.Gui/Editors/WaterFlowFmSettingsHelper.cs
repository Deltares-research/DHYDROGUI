using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DeltaShell.NGHS.Common.Gui.WPF.SettingsView;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class WaterFlowFmSettingsHelper
    {
        /// <summary>
        /// Gets the WPF GUI categories <seealso cref="GuiCategory"/>.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <param name="gui">The GUI.</param>
        /// <returns></returns>
        public static ObservableCollection<GuiCategory> GetWpfGuiCategories(WaterFlowFMModel model, IGui gui)
        {
            var wpfGuiCategories = new List<GuiCategory>();
            if (model != null)
            {
                wpfGuiCategories = GetWaterFlowFmSettings(model)?.FieldDescriptions
                    .GroupBy(fd => fd.Category)
                    .Select(gp => new GuiCategory(gp.Key, gp.ToList())).ToList();
                wpfGuiCategories?.SelectMany(gp => gp.Properties).Distinct().ForEach(p => p.GetModel = () => model);
                if (wpfGuiCategories == null) return null;
                SetFlowFmExtraSettings(model, gui, wpfGuiCategories);
            }
            return new ObservableCollection<GuiCategory>(wpfGuiCategories);
        }

        private static ObjectUIDescription GetWaterFlowFmSettings(WaterFlowFMModel model)
        {
            //Extract the UiDescription
            var groupsToSkip = new List<string>(0);
            var waterFlowFmGuiPropertyExtractor = new WaterFlowFMGuiPropertyExtractor(model);
            var uiProperties = waterFlowFmGuiPropertyExtractor.ExtractObjectDescription(groupsToSkip);
            return ExtendedUiProperties(model, uiProperties);
        }

        private static void SetFlowFmExtraSettings(IModel model, IGui gui, IList<GuiCategory> wpfCategories)
        {
            if (!(model is WaterFlowFMModel)) return;

            var fmModel = model as WaterFlowFMModel;
            //General settings
            var generalCategory = wpfCategories.FirstOrDefault(c => c.CategoryName.ToLower().Equals("general"));
            if (generalCategory != null)
            {
                var depthlayers = new GuiProperty(new FieldUIDescription(d => fmModel.DepthLayerDefinition?.Description, null, o => false, o => false)
                {
                    Category = "General",
                    SubCategory = "Layers",
                    ToolTip = "Adjust layers",
                    Label = "Layer",
                    ValueType = typeof(string),
                    HasMaxValue = false,
                    HasMinValue = false,
                });

                depthlayers.CustomCommand.ButtonFunction = (o) => EditDepthLayersHelper.ButtonAction(o);
                depthlayers.CustomCommand.ButtonImage = EditDepthLayersHelper.ButtonImage;
                generalCategory.AddWpfGuiProperty(depthlayers);

                //Gui Coordinate
                var coordSys = new GuiProperty(new FieldUIDescription(d => SetCoordinateSystemButton.CoordinateSystemName(fmModel), null)
                {
                    Category = "General",
                    SubCategory = "Global Position",
                    ToolTip = SetCoordinateSystemButton.ToolTip,
                    Label = SetCoordinateSystemButton.Label,
                    ValueType = typeof(string),
                    HasMaxValue = false,
                    HasMinValue = false,
                });

                coordSys.CustomCommand.ButtonFunction =
                    (o) => SetCoordinateSystemButton.ButtonAction(o, gui, WaterFlowFMModel.IsValidCoordinateSystem);
                coordSys.CustomCommand.ButtonImage = SetCoordinateSystemButton.ButtonImage;
                generalCategory.AddWpfGuiProperty(coordSys);
            }

            var tsCategory = wpfCategories.FirstOrDefault(c => string.Equals(c.CategoryName, "Time Frame", StringComparison.InvariantCultureIgnoreCase));
            var property = tsCategory?.Properties.FirstOrDefault(p => string.Equals(p.Name, KnownProperties.DtUser, StringComparison.InvariantCultureIgnoreCase));
            if (property != null)
            {
                property.CustomCommand.ButtonFunction = o =>
                {
                    if (!(o is WaterFlowFMModel fmmodel))
                        return;

                    var timeStep = fmmodel.TimeStep.TotalSeconds.ToString(CultureInfo.InvariantCulture);
                    fmmodel.ModelDefinition.SetModelProperty(GuiProperties.HisOutputDeltaT, timeStep);
                    fmmodel.ModelDefinition.SetModelProperty(GuiProperties.MapOutputDeltaT, timeStep);
                    fmmodel.ModelDefinition.SetModelProperty(GuiProperties.ClassMapOutputDeltaT, timeStep);
                    fmmodel.ModelDefinition.SetModelProperty(GuiProperties.RstOutputDeltaT, timeStep);
                };
                property.CustomCommand.ButtonImage = Properties.Resources.Synchronize_grey_16x;
                property.CustomCommand.Tooltip = "Synchronize with output time step";
            }

            var icCategory = wpfCategories.FirstOrDefault(c => c.CategoryName.ToLower().Equals("initial conditions"));
            if (icCategory != null)
            {
                var coverageLayers = new GuiProperty(new FieldUIDescription(d => EditCoverageLayersHelper.DepthLayersToString(model), null, o => false, o => false)
                {
                    Category = "Initial Conditions",
                    SubCategory = "Salinity",
                    ToolTip = "Edit number of depth layers.",
                    Label = "Depth layers",
                    ValueType = typeof(string),
                    HasMaxValue = false,
                    HasMinValue = false,
                });
                coverageLayers.CustomCommand.ButtonFunction = EditCoverageLayersHelper.ButtonAction;
                coverageLayers.CustomCommand.ButtonImage = Properties.Resources.waterLayers;

                icCategory.AddWpfGuiProperty(coverageLayers);
            }

            //Add more settings
            //Use the FieldUIDescription to generate the getters and the enable / disable functions.
            Func<object, bool> isEnabledFunc = o => true;
            Func<object, bool> isVisibleFunc = o => (o is WaterFlowFMModel) && (o as WaterFlowFMModel).UseMorSed;
            var fieldUi = new FieldUIDescription(null, null, isEnabledFunc, isVisibleFunc);
            var fieldUiDescriptions = new List<FieldUIDescription>();
            fieldUiDescriptions.Add(fieldUi);

            var sedimentCategory = new GuiCategory("Sediment", fieldUiDescriptions)
            {
                CategoryVisibility = () => fmModel.UseMorSed,
                CustomControl = new SedimentFractionsEditor(fmModel.SedimentFractions, fmModel.SedimentOverallProperties)
            };

            wpfCategories.Add(sedimentCategory);

            var morphologyCategory = wpfCategories.FirstOrDefault(wCat => wCat.CategoryName.ToLower() == "morphology");
            if (morphologyCategory != null)
            {
                morphologyCategory.CategoryVisibility = () => fmModel.UseMorSed;
            }

            var tracersCategory = new GuiSubCategory("Tracers", new List<FieldUIDescription>{ new FieldUIDescription(null,null, o => true, o => true)})
            {
                CustomControl = new TracerDefinitionsEditorWpf
                {
                    Tracers = fmModel.TracerDefinitions
                }
            };

            var processesCategory = wpfCategories.FirstOrDefault(c => c.CategoryName.ToLower().Equals("processes"));
            processesCategory?.SubCategories.Add(tracersCategory);
        }

        /*Extraced from WaterFlowFMModelView.cs */
        private static ObjectUIDescription ExtendedUiProperties(WaterFlowFMModel data, ObjectUIDescription objectDescription)
        {
            if (data == null) return objectDescription;

            objectDescription.FieldDescriptions
                // add to begin:
                = objectDescription.FieldDescriptions
                    // add to end:
                    .ToList();

            objectDescription.FieldDescriptions.First(f => f.Name == "StopTime").ValidationMethod =
                (m, t) =>
                    ((WaterFlowFMModel) m).StartTime < (DateTime) t
                        ? ""
                        : "Start time must be smaller than stop time";

            objectDescription.FieldDescriptions.First(f => f.Name == "AngLat").VisibilityMethod =
                o => (data.CoordinateSystem == null || !data.CoordinateSystem.IsGeographic);

            objectDescription.FieldDescriptions.First(f => f.Name == "AngLon").VisibilityMethod =
                o => (data.CoordinateSystem == null || !data.CoordinateSystem.IsGeographic);
            return objectDescription;
        }
    }
}