using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public static class WaveModelUIDescription
    {
        public static ObjectUIDescription Extract(WaveModel data)
        {
            var definition = data.ModelDefinition;
            var descriptions = new List<FieldUIDescription>();

            // all properties from GUIGroups, defined in csv
            foreach (var guiGroup in data.ModelDefinition.ModelSchema.GuiPropertyGroups.Values)
            {
                descriptions.AddRange(definition.Properties
                                                .Where(p => p.PropertyDefinition.Category == guiGroup.Name &&
                                                            !p.PropertyDefinition.ModelFileOnly)
                                                .Select(p => CreateFieldDescription(p, data)));
            }

            return new ObjectUIDescription {FieldDescriptions = descriptions};
        }

        private static FieldUIDescription CreateFieldDescription(WaveModelProperty prop, WaveModel model)
        {
            Func<object, object> getter = p => prop.Value;
            Action<object, object> setter = (p, v) => prop.Value = v;

            Func<object, bool> isEnabled = null;
            Func<object, bool> isVisible = null;
            if (model != null)
            {
                isEnabled = o => prop.IsEnabled(model.ModelDefinition.Properties);
                isVisible = o => prop.IsVisible(model.ModelDefinition.Properties);
            }

            var label = string.IsNullOrEmpty(prop.PropertyDefinition.Caption) ? prop.PropertyDefinition.FilePropertyName : prop.PropertyDefinition.Caption;

            return new FieldUIDescription(getter, setter, isEnabled, isVisible)
                {
                    AlwaysRefresh = isEnabled != null,
                    Category = prop.PropertyDefinition.Category,
                    SubCategory = prop.PropertyDefinition.SubCategory,
                    IsReadOnly = prop.PropertyDefinition.ModelFileOnly,
                    Label = label,
                    Name = prop.PropertyDefinition.FilePropertyName,
                    ValueType = prop.PropertyDefinition.DataType,
                    ToolTip = GetToolTip(prop)
                };
        }

        private static string GetToolTip(WaveModelProperty prop)
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
            return string.Format("Mdw name: {0}{3}Description:{3}\t{1}{3}{2}",
                                 prop.PropertyDefinition.FilePropertyName,
                                 prop.PropertyDefinition.Description,
                                 validRangeText, Environment.NewLine);
        }
    }
}