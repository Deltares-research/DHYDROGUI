using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Utils;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    public static class TimeDependentFunctionSplitter
    {
        public static ICollection<IFunction> SplitIntoFunctionsPerArgumentValue(IFunction function)
        {
            if (function == null || function.Arguments.Count < 2 || function.Arguments[0].ValueType != typeof(DateTime))
            {
                throw new ArgumentException("Not a valid time dependent function");
            }

            return function.Arguments[1].Values.OfType<object>().
                Select(i => ExtractSeriesForArgumentValue(function, i)).
                ToList();
        }

        internal static IFunction ExtractSeriesForArgumentValue(IFunction function, object argumentValue)
        {
            IFunction func = function.Filter(new IVariableFilter[]
                {
                    new VariableValueFilter<object>(function.Arguments[1], argumentValue),
                    new VariableReduceFilter(function.Arguments[1])
                });

            if (argumentValue is INameable)
            {
                func.Components[0].Name = (argumentValue as INameable).Name;
            }
            if (argumentValue is string)
            {
                func.Components[0].Name = (string)argumentValue;
            }
            return func;
        }
    }
}