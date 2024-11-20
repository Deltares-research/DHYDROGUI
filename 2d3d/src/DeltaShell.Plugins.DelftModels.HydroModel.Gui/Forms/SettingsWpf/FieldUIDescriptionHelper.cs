using System;
using DelftTools.Controls.Swf.DataEditorGenerator.Metadata;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms.SettingsWpf
{
    /// <summary>
    /// Class which contains helper methods related to <see cref="FieldUIDescription"/>.
    /// </summary>
    public static class FieldUIDescriptionHelper
    {
        /// <summary>
        /// Creates a <see cref="FieldUIDescription"/> based on the input arguments.
        /// </summary>
        /// <param name="fieldDescription">The <see cref="FieldUIDescription"/> to base the description on.</param>
        /// <param name="getValueFunc">The function to get the value from the description.</param>
        /// <param name="setValueAction">The function to set the value to the description.</param>
        /// <returns>A copy of <paramref name="fieldDescription"/> with get and set actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fieldDescription"/> is <c>null</c>.</exception>
        public static FieldUIDescription CreateFieldDescription(FieldUIDescription fieldDescription, Func<object, object> getValueFunc,
                                                                Action<object, object> setValueAction)
        {
            if (fieldDescription == null)
            {
                throw new ArgumentNullException(nameof(fieldDescription));
            }

            var newFieldDescription = new FieldUIDescription(getValueFunc,
                                                             setValueAction,
                                                             fieldDescription.IsEnabled,
                                                             fieldDescription.IsVisible,
                                                             (o, v) =>
                                                             {
                                                                 fieldDescription.Validate(o, v, out string message);
                                                                 return message;
                                                             })
            {
                Category = fieldDescription.Category,
                SubCategory = fieldDescription.SubCategory,
                Label = fieldDescription.Label,
                Name = fieldDescription.Name,
                ValueType = fieldDescription.ValueType,
                ToolTip = fieldDescription.ToolTip,
                MaxValue = fieldDescription.MaxValue,
                MinValue = fieldDescription.MinValue,
                UnitSymbol = fieldDescription.UnitSymbol,
                IsReadOnly = fieldDescription.IsReadOnly
            };

            return newFieldDescription;
        }
    }
}