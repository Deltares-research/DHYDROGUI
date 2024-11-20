using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class WaterFlowFMGuiPropertyExtractor
    {
        public WaterFlowFMGuiPropertyExtractor(WaterFlowFMModel model = null)
        {
            Model = model;
        }

        public ObjectUIDescription ExtractObjectDescription(IEnumerable<string> groupsToSkip)
        {
            var fieldUIDescriptions = new List<FieldUIDescription>();
            WaterFlowFMModelDefinition modelDefinition = Model.ModelDefinition;

            foreach (
                ModelPropertyGroup propertyGroup in
                WaterFlowFMModelDefinition.GuiPropertyGroups.Values.Where(g => !groupsToSkip.Contains(g.Name)))
            {
                fieldUIDescriptions.AddRange(
                    modelDefinition.Properties.Where(
                                       p => !p.PropertyDefinition.ModelFileOnly &&
                                            p.PropertyDefinition.Category.Equals(propertyGroup.Name, StringComparison.InvariantCultureIgnoreCase))
                                   .Select(GetFieldDescription)
                                   .ToList());
            }

            var objectDescription = new ObjectUIDescription {FieldDescriptions = fieldUIDescriptions};
            return objectDescription;
        }

        public FieldUIDescription GetFieldDescription(WaterFlowFMProperty prop)
        {
            Func<object, object> getter = (o) => prop.Value;
            Action<object, object> setter = (o, v) => { prop.Value = v; };

            Func<object, bool> isEnabled = null;
            Func<object, bool> isVisible = null;
            if (Model != null)
            {
                isEnabled = o => prop.IsEnabled(((WaterFlowFMModel) o)?.ModelDefinition.Properties.ToList());
                isVisible = o => prop.IsVisible(((WaterFlowFMModel) o)?.ModelDefinition.Properties.ToList());
            }

            string label = string.IsNullOrEmpty(prop.PropertyDefinition.Caption)
                               ? prop.PropertyDefinition.MduPropertyName
                               : prop.PropertyDefinition.Caption;
            bool hasMinVal = !string.IsNullOrEmpty(prop.PropertyDefinition.MinValueAsString);
            bool hasMaxVal = !string.IsNullOrEmpty(prop.PropertyDefinition.MaxValueAsString);
            double minVal;
            double.TryParse(prop.PropertyDefinition.MinValueAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out minVal);
            double maxVal;
            double.TryParse(prop.PropertyDefinition.MaxValueAsString, NumberStyles.Any, CultureInfo.InvariantCulture, out maxVal);

            return new FieldUIDescription(getter, setter, isEnabled, isVisible)
            {
                Category = prop.PropertyDefinition.Category,
                SubCategory = prop.PropertyDefinition.SubCategory,
                IsReadOnly = prop.PropertyDefinition.ModelFileOnly,
                Label = label,
                Name = prop.PropertyDefinition.MduPropertyName,
                ValueType = prop.PropertyDefinition.DataType,
                ToolTip = prop.PropertyDefinition.Description,
                HasMinValue = hasMinVal,
                HasMaxValue = hasMaxVal,
                MinValue = minVal,
                MaxValue = maxVal,
                UnitSymbol = prop.PropertyDefinition.Unit
            };
        }

        private WaterFlowFMModel Model { get; set; }
    }
}