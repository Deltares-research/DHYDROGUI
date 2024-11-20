using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Properties;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms.Properties
{
    [DisplayName("Mathematical Expression")]
    public class MathematicalExpressionProperties : ObjectProperties<MathematicalExpression>
    {
        [ResourcesDisplayName(typeof(Resources), "Common_Name_DisplayName")]
        [ResourcesCategory(typeof(Resources), "Categories_General")]
        [PropertyOrder(1)]
        public string Name
        {
            get => data.Name;
            set => data.Name = value;
        }

        [DisplayName("Input parameters")]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
        [TypeConverter(typeof(KeyValuePairArrayConverter<string>))]
        [PropertyOrder(2)]
        public KeyValuePair<string, string>[] Inputs => GetInputs();

        [DisplayName("Expression")]
        [ResourcesCategory(typeof(Resources), "Category_Data")]
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