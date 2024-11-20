using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui
{
    public static class WaveModelUIDescription
    {
        public static ObjectUIDescription Extract(WaveModel data)
        {
            WaveModelDefinition definition = data.ModelDefinition;
            var descriptions = new List<FieldUIDescription>();

            // all properties from GUIGroups, defined in csv
            foreach (ModelPropertyGroup guiGroup in data.ModelDefinition.ModelSchema.GuiPropertyGroups.Values)
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

            string label = string.IsNullOrEmpty(prop.PropertyDefinition.Caption)
                               ? prop.PropertyDefinition.FilePropertyKey
                               : prop.PropertyDefinition.Caption;

            return new FieldUIDescription(getter, setter, isEnabled, isVisible)
            {
                AlwaysRefresh = isEnabled != null,
                Category = prop.PropertyDefinition.Category,
                SubCategory = prop.PropertyDefinition.SubCategory,
                IsReadOnly = prop.PropertyDefinition.ModelFileOnly,
                Label = label,
                Name = prop.PropertyDefinition.FilePropertyKey,
                ValueType = prop.PropertyDefinition.DataType,
                ToolTip = prop.PropertyDefinition.Description,
                UnitSymbol = prop.PropertyDefinition.Unit
            };
        }
    }
}