using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Gui.Forms.FunctionListView
{
    [Entity]
    internal class FunctionWrapper
    {
        private readonly IEventedList<IFunction> functions;
        private readonly ICollection<IFunctionTypeCreator> functionTypes;

        public FunctionWrapper(IFunction function, IEventedList<IFunction> functions, IEditableObject functionOwner,
                               ICollection<IFunctionTypeCreator> functionTypes)
        {
            this.functions = functions;
            FunctionOwner = functionOwner;
            this.functionTypes = functionTypes ?? new Collection<IFunctionTypeCreator>();
            Function = function;
        }

        public string Name
        {
            get => Function.Name;
            set => Function.Name = value;
        }

        public string Description
        {
            get
            {
                string result;
                return Function.Attributes.TryGetValue(WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE, out result)
                           ? result
                           : null;
            }
        }

        public string FunctionType
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(Function));
                return functionType != null ? functionType.FunctionTypeName : "";
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.FunctionTypeName == value);
                if (functionType == null)
                {
                    return;
                }

                Function = FunctionTypeCreator.ReplaceFunctionUsingCreator(functions, Function,
                                                                           functionType, FunctionOwner,
                                                                           string.Format("from {0} ", FunctionType));
            }
        }

        public double DefaultValue
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(Function));
                if (functionType == null)
                {
                    return double.NaN;
                }

                return functionType.GetDefaultValueForFunction(Function);
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(Function));
                if (functionType == null)
                {
                    return;
                }

                functionType.SetDefaultValueForFunction(Function, value);
            }
        }

        public string Unit
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(Function));
                if (functionType == null)
                {
                    return "";
                }

                return functionType.GetUnitForFunction(Function);
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(Function));
                if (functionType == null)
                {
                    return;
                }

                functionType.SetUnitForFunction(Function, value);
            }
        }

        public string Url
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(Function));
                if (functionType == null)
                {
                    return "";
                }

                return functionType.GetUrlForFunction(Function);
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(Function));
                if (functionType == null)
                {
                    return;
                }

                functionType.SetUrlForFunction(Function, value);
            }
        }

        public string Arguments => VariablesToString(Function.Arguments);

        public string Components => VariablesToString(Function.Components);

        public string Edit => string.Empty;

        public IFunction Function { get; private set; }

        /// <summary>
        /// The owner of <see cref="Function"/>
        /// </summary>
        public IEditableObject FunctionOwner { get; set; }

        private static string VariablesToString(IEnumerable<IVariable> variables)
        {
            return variables.Aggregate("", (tot, c) => tot + c.DisplayName + ",").TrimEnd(',');
        }
    }
}