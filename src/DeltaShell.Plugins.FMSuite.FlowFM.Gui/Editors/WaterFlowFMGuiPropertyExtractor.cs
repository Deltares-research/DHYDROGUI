using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class WaterFlowFMGuiPropertyExtractor
    {
        public WaterFlowFMGuiPropertyExtractor(WaterFlowFMModel model = null)
        {
            Model = model;
        }

        private WaterFlowFMModel Model { get; set; }

        public ObjectUIDescription ExtractObjectDescription(IEnumerable<string> groupsToSkip)
        {
            var fieldUIDescriptions = new List<FieldUIDescription>();
            var modelDefinition = Model.ModelDefinition;

            foreach (
                var propertyGroup in Model.ModelDefinition.GuiPropertyGroups.Values.Where(g => !groupsToSkip.Contains(g.Name)))
            {
                fieldUIDescriptions.AddRange(
                    modelDefinition.Properties.Where(
                        p => !p.PropertyDefinition.ModelFileOnly &&
                             p.PropertyDefinition.Category.Equals(propertyGroup.Name, StringComparison.InvariantCultureIgnoreCase))
                                   .Select(GetFieldDescription)
                                   .ToList());
            }

            var objectDescription = new ObjectUIDescription
                {
                    FieldDescriptions = fieldUIDescriptions
                };
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

            var label = string.IsNullOrEmpty(prop.PropertyDefinition.Caption)
                            ? prop.PropertyDefinition.MduPropertyName
                            : prop.PropertyDefinition.Caption;
            var hasMinVal = !string.IsNullOrEmpty(prop.PropertyDefinition.MinValueAsString);
            var hasMaxVal = !string.IsNullOrEmpty(prop.PropertyDefinition.MaxValueAsString);
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
                    ToolTip = GetToolTip(prop),
                    HasMinValue = hasMinVal,
                    HasMaxValue = hasMaxVal,
                    MinValue = minVal,
                    MaxValue = maxVal,
                    UnitSymbol = prop.PropertyDefinition.Unit
                };
        }

        private static string GetToolTip(WaterFlowFMProperty prop)
        {
            var validRangeText = new StringBuilder();

            if (!prop.PropertyDefinition.DataType.IsEnum) //enums ranges are enforced by combobox
            {
                if (prop.MinValue != null)
                {
                    validRangeText.Append("Minimum value: ");
                    validRangeText.Append(prop.PropertyDefinition.MinValueAsString);
                    validRangeText.Append(Environment.NewLine);
                }
                if (prop.MaxValue != null)
                {
                    validRangeText.Append("Maximum value: ");
                    validRangeText.Append(prop.PropertyDefinition.MaxValueAsString);
                    validRangeText.Append(Environment.NewLine);
                }
            }
            return string.Format("Mdu name: {0}{3}Description:{3}{1}{3}{2}",
                                 prop.PropertyDefinition.MduPropertyName,
                                 prop.PropertyDefinition.Description,
                                 validRangeText, Environment.NewLine);
        }

    }
}