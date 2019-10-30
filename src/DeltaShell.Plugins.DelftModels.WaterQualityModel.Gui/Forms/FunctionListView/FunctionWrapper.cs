using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using IEditableObject = DelftTools.Utils.Editing.IEditableObject;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView
{
    [Entity]
    internal class FunctionWrapper
    {
        private IFunction function;
        private readonly IEventedList<IFunction> functions;
        private readonly ICollection<IFunctionTypeCreator> functionTypes;
        private IEditableObject functionOwner;

        public FunctionWrapper(IFunction function, IEventedList<IFunction> functions, IEditableObject functionOwner, ICollection<IFunctionTypeCreator> functionTypes)
        {
            this.functions = functions;
            this.functionOwner = functionOwner;
            this.functionTypes = functionTypes ?? new Collection<IFunctionTypeCreator>();
            this.function = function;
        }

        public string Name
        {
            get { return function.Name; }
            set { function.Name = value; }
        }

        public string Description
        {
            get
            {
                string result;
                return function.Attributes.TryGetValue(WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE, out result) ? result : null;
            }
        }

        public string FunctionType
        {
            get
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                return functionType != null ? functionType.FunctionTypeName : "";
            }
            set
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.FunctionTypeName == value);
                if (functionType == null) return;

                function = FunctionTypeCreator.ReplaceFunctionUsingCreator(functions, function, 
                    functionType, functionOwner, string.Format("from {0} ", FunctionType));
            }
        }

        public double DefaultValue
        {
            get
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null) return double.NaN;

                return functionType.GetDefaultValueForFunction(function);
            }
            set
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null) return;

                functionType.SetDefaultValueForFunction(function, value);
            }
        }

        public string Unit
        {
            get
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null) return "";

                return functionType.GetUnitForFunction(function);
            }
            set
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null) return;

                functionType.SetUnitForFunction(function, value);
            }
        }

        public string Url
        {
            get
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null) return "";

                return functionType.GetUrlForFunction(function);
            }
            set
            {
                var functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null) return;

                functionType.SetUrlForFunction(function, value);
            }
        }

        public string Arguments
        {
            get { return VariablesToString(function.Arguments); }
        }

        public string Components
        {
            get { return VariablesToString(function.Components); }
        }

        public string Edit
        {
            get { return ""; }
            set { }
        }

        public IFunction Function { 
            get { return function; } 
            private set { function = value; } // For propagating Property Changed
        }

        /// <summary>
        /// The owner of <see cref="Function"/>
        /// </summary>
        public IEditableObject FunctionOwner
        {
            get { return functionOwner; }
            set { functionOwner = value; }
        }

        private static string VariablesToString(IEnumerable<IVariable> variables)
        {
            return variables.Aggregate("", (tot, c) => tot + c.DisplayName + ",").TrimEnd(',');
        }
    }
}