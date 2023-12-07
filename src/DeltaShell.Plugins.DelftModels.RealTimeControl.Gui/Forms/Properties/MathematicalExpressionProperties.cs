using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [ResourcesDisplayName(typeof(Resources), "MathematicalExpressionProperties_DisplayName")]
    public class MathematicalExpressionProperties : ObjectProperties<MathematicalExpression>
    {
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [ResourcesDescription(typeof(Resources), "MathematicalExpressionProperties_Name_Description")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [ResourcesDisplayName(typeof(Resources), "MathematicalExpressionProperties_InputParameters_DisplayName")]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [TypeConverter(typeof(KeyValuePairArrayConverter<string>))]
        [ResourcesDescription(typeof(Resources), "MathematicalExpressionProperties_InputParameters_Description")]
        [PropertyOrder(2)]
        public KeyValuePair<string, string>[] Inputs => GetInputs();

        [ResourcesDisplayName(typeof(Resources), "MathematicalExpressionProperties_Expression_DisplayName")]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [ResourcesDescription(typeof(Resources), "MathematicalExpressionProperties_Expression_Description")]
        [PropertyOrder(3)]
        public string MathematicalExpression
        {
            get => data.Expression;
            set => data.Expression = value;
        }

        private KeyValuePair<string, string>[] GetInputs()
        {
            return data.InputMapping
                       .Select(i => new KeyValuePair<string, string>(i.Key.ToString(), i.Value.Name))
                       .ToArray();
        }
    }
}