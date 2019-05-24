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
        private IFunction function;
        private readonly IEventedList<IFunction> functions;
        private readonly ICollection<IFunctionTypeCreator> functionTypes;
        private IEditableObject functionOwner;

        public FunctionWrapper(IFunction function, IEventedList<IFunction> functions, IEditableObject functionOwner,
                               ICollection<IFunctionTypeCreator> functionTypes)
        {
            this.functions = functions;
            this.functionOwner = functionOwner;
            this.functionTypes = functionTypes ?? new Collection<IFunctionTypeCreator>();
            this.function = function;
        }

        public string Name
        {
            get => function.Name;
            set => function.Name = value;
        }

        public string Description
        {
            get
            {
                string result;
                return function.Attributes.TryGetValue(WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE, out result)
                           ? result
                           : null;
            }
        }

        public string FunctionType
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                return functionType != null ? functionType.FunctionTypeName : "";
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.FunctionTypeName == value);
                if (functionType == null)
                {
                    return;
                }

                function = FunctionTypeCreator.ReplaceFunctionUsingCreator(functions, function,
                                                                           functionType, functionOwner,
                                                                           string.Format("from {0} ", FunctionType));
            }
        }

        public double DefaultValue
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null)
                {
                    return double.NaN;
                }

                return functionType.GetDefaultValueForFunction(function);
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null)
                {
                    return;
                }

                functionType.SetDefaultValueForFunction(function, value);
            }
        }

        public string Unit
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null)
                {
                    return "";
                }

                return functionType.GetUnitForFunction(function);
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null)
                {
                    return;
                }

                functionType.SetUnitForFunction(function, value);
            }
        }

        public string Url
        {
            get
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null)
                {
                    return "";
                }

                return functionType.GetUrlForFunction(function);
            }
            set
            {
                IFunctionTypeCreator functionType = functionTypes.FirstOrDefault(ft => ft.IsThisFunctionType(function));
                if (functionType == null)
                {
                    return;
                }

                functionType.SetUrlForFunction(function, value);
            }
        }

        public string Arguments => VariablesToString(function.Arguments);

        public string Components => VariablesToString(function.Components);

        public string Edit
        {
            get => "";
            set {}
        }

        public IFunction Function
        {
            get => function;
            private set => function = value;
// For propagating Property Changed
        }

        /// <summary>
        /// The owner of <see cref="Function" />
        /// </summary>
        public IEditableObject FunctionOwner
        {
            get => functionOwner;
            set => functionOwner = value;
        }

        private static string VariablesToString(IEnumerable<IVariable> variables)
        {
            return variables.Aggregate("", (tot, c) => tot + c.DisplayName + ",").TrimEnd(',');
        }
    }
}