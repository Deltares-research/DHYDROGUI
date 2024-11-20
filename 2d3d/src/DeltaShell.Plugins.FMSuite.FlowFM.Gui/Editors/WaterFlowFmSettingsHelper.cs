using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Collections;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf;
using DeltaShell.Plugins.FMSuite.Common.Gui.Editors.Buttons;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public static class WaterFlowFmSettingsHelper
    {
        /// <summary>
        /// Get the WPF GUI categories <seealso cref="WpfGuiCategory"/>.
        /// </summary>
        /// <param name="model"> The model.</param>
        /// <param name="gui"> The GUI.</param>
        /// <returns>
        /// IF model != null THEN
        /// An ObservableCollection containing all the relevant WpfGuiCategories
        /// ELSE
        /// An empty ObservableCollection
        /// </returns>
        public static ObservableCollection<WpfGuiCategory> GetWpfGuiCategories(WaterFlowFMModel model, IGui gui)
        {
            if (model == null)
            {
                return new ObservableCollection<WpfGuiCategory>();
            }

            List<WpfGuiCategory> wpfGuiCategories =
                GetWaterFlowFmSettings(model).FieldDescriptions
                                             .GroupBy(fd => fd.Category)
                                             .Select(gp => new WpfGuiCategory(gp.Key, gp.ToList()))
                                             .ToList();

            wpfGuiCategories.SelectMany(gp => gp.Properties).Distinct()
                            .ForEach(p => p.GetModel = () => model);

            SetFlowFmExtraSettings(model, gui, wpfGuiCategories);

            return new ObservableCollection<WpfGuiCategory>(wpfGuiCategories);
        }

        private static ObjectUIDescription GetWaterFlowFmSettings(WaterFlowFMModel model)
        {
            //Extract the UiDescription
            var groupsToSkip = new List<string>(0);
            var waterFlowFmGuiPropertyExtractor = new WaterFlowFMGuiPropertyExtractor(model);
            ObjectUIDescription uiProperties = waterFlowFmGuiPropertyExtractor.ExtractObjectDescription(groupsToSkip);
            return ExtendedUiProperties(model, uiProperties);
        }

        private static void SetFlowFmExtraSettings(IModel model, IGui gui, IList<WpfGuiCategory> wpfCategories)
        {
            if (!(model is WaterFlowFMModel))
            {
                return;
            }

            var fmModel = model as WaterFlowFMModel;

            AddCoordinateSystemPropertyToGeneralSettingsCategory(gui, wpfCategories, fmModel);
            AddAdditionalCategories(wpfCategories, fmModel);
        }

        private static void AddAdditionalCategories(IList<WpfGuiCategory> wpfCategories, WaterFlowFMModel fmModel)
        {
            AddSedimentCategory(wpfCategories, fmModel);
            AddTracerCategory(wpfCategories, fmModel);
        }

        private static void AddTracerCategory(IList<WpfGuiCategory> wpfCategories, WaterFlowFMModel fmModel)
        {
            WpfGuiCategory morphologyCategory = wpfCategories.FirstOrDefault(wCat => wCat.CategoryName.ToLower() == "morphology");
            if (morphologyCategory != null)
            {
                morphologyCategory.CategoryVisibility = () => fmModel.UseMorSed;
            }

            var tracersCategory = new WpfGuiSubCategory("Tracers", new List<FieldUIDescription> { new FieldUIDescription(null, null, o => true, o => true) }) { CustomControl = new TracerDefinitionsEditorWpf { Tracers = fmModel.TracerDefinitions } };

            WpfGuiCategory processesCategory = wpfCategories.FirstOrDefault(c => c.CategoryName.ToLower().Equals("processes"));
            processesCategory?.SubCategories.Add(tracersCategory);
        }

        private static void AddSedimentCategory(IList<WpfGuiCategory> wpfCategories, WaterFlowFMModel fmModel)
        {
            Func<object, bool> isEnabledFunc = o => true;
            Func<object, bool> isVisibleFunc = o => o is WaterFlowFMModel && (o as WaterFlowFMModel).UseMorSed;
            var fieldUi = new FieldUIDescription(null, null, isEnabledFunc, isVisibleFunc);
            var fieldUiDescriptions = new List<FieldUIDescription>();
            fieldUiDescriptions.Add(fieldUi);

            var sedimentCategory = new WpfGuiCategory("Sediment", fieldUiDescriptions)
            {
                CategoryVisibility = () => fmModel.UseMorSed,
                CustomControl = new SedimentFractionsEditor(fmModel.SedimentFractions, fmModel.SedimentOverallProperties)
            };

            wpfCategories.Add(sedimentCategory);
        }

        private static void AddCoordinateSystemPropertyToGeneralSettingsCategory(IGui gui, IList<WpfGuiCategory> wpfCategories, WaterFlowFMModel fmModel)
        {
            // Gui Coordinate
            WpfGuiCategory generalCategory = wpfCategories.FirstOrDefault(c => c.CategoryName.ToLower().Equals("general"));
            if (generalCategory != null)
            {
                var coordSys = new WpfGuiProperty(new FieldUIDescription(d => SetCoordinateSystemButton.CoordinateSystemName(fmModel), null)
                {
                    Category = "General",
                    SubCategory = "Global Position",
                    Name = "CoordinateSystem",
                    ToolTip = SetCoordinateSystemButton.ToolTip,
                    Label = SetCoordinateSystemButton.Label,
                    ValueType = typeof(string),
                    HasMaxValue = false,
                    HasMinValue = false
                });

                coordSys.CustomCommand.TextBoxEnabled = false;
                coordSys.CustomCommand.ButtonBehaviour = new SetCoordinateSystemButton(gui, WaterFlowFMModel.IsValidCoordinateSystem);
                coordSys.CustomCommand.ButtonImage = SetCoordinateSystemButton.ButtonImage;
                generalCategory.AddWpfGuiProperty(coordSys);
            }
        }

        /*Extracted from WaterFlowFMModelView.cs */
        private static ObjectUIDescription ExtendedUiProperties(WaterFlowFMModel data, ObjectUIDescription objectDescription)
        {
            if (data == null)
            {
                return objectDescription;
            }

            // Cache the fieldDescriptions before returning
            FieldUIDescription[] cachedFieldDescriptions = objectDescription.FieldDescriptions.ToArray();

            // Map description to the model
            MapDataOntoProperty(o => data.StopTime, (o, v) => data.StopTime = (DateTime) v, cachedFieldDescriptions, KnownProperties.StopDateTime);
            MapDataOntoProperty(o => data.StartTime, (o, v) => data.StartTime = (DateTime) v, cachedFieldDescriptions, KnownProperties.StartDateTime);
            MapDataOntoProperty(o => data.TimeStep, (o, v) => data.TimeStep = (TimeSpan) v, cachedFieldDescriptions, KnownProperties.DtUser);

            // Restore the fieldDescription
            objectDescription.FieldDescriptions = cachedFieldDescriptions;

            objectDescription.FieldDescriptions.First(f => f.Name.Equals(KnownProperties.StopDateTime, StringComparison.InvariantCultureIgnoreCase)).ValidationMethod =
                (m, t) =>
                    ((WaterFlowFMModel)m).StartTime < (DateTime)t
                        ? ""
                        : "Start time must be smaller than stop time";

            objectDescription.FieldDescriptions.First(f => f.Name == "AngLat").VisibilityMethod =
                o => data.CoordinateSystem == null || !data.CoordinateSystem.IsGeographic;

            objectDescription.FieldDescriptions.First(f => f.Name == "AngLon").VisibilityMethod =
                o => data.CoordinateSystem == null || !data.CoordinateSystem.IsGeographic;
            return objectDescription;
        }

        /// <summary>
        /// Maps a <see cref="FieldUIDescription"/> parameter with a custom get and set value function.
        /// </summary>
        /// <param name="getValueFunc">The function to retrieve the value from the description.</param>
        /// <param name="setValueAction">The action to set the value to the description</param>
        /// <param name="fieldDescriptions">The collection of <see cref="FieldUIDescription"/>.</param>
        /// <param name="parameterName">The name of the parameter to map.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="fieldDescriptions"/>
        /// has no or multiple definitions of <paramref name="parameterName"/>.
        /// </exception>
        private static void MapDataOntoProperty(Func<object, object> getValueFunc,
                                                Action<object, object> setValueAction,
                                                FieldUIDescription[] fieldDescriptions,
                                                string parameterName)
        {
            // Check if it is present. Do a dual lookup as the field description should replace the original definition.
            int descriptionIndex = Array.FindIndex(fieldDescriptions, f => string.Equals(f.Name, parameterName, StringComparison.OrdinalIgnoreCase));
            int secondDescriptionIndex = Array.FindLastIndex(fieldDescriptions, f => string.Equals(f.Name, parameterName, StringComparison.OrdinalIgnoreCase));

            if (descriptionIndex == -1 || descriptionIndex != secondDescriptionIndex)
            {
                throw new ArgumentException($"Could not find {parameterName} or multiple definitions found.");
            }

            FieldUIDescription currentFieldDescription = fieldDescriptions[descriptionIndex];
            FieldUIDescription newFieldDescription = FieldUIDescriptionHelper.CreateFieldDescription(currentFieldDescription, getValueFunc, setValueAction);

            fieldDescriptions[descriptionIndex] = newFieldDescription;
        }
    }
}