using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Filters;
using DelftTools.Utils;
using Deltares.Infrastructure.API.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Properties;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff
{
    /// <summary>
    /// <see cref="TimeDependentFunctionSplitter"/> implements <see cref="ITimeDependentFunctionSplitter"/>
    /// to split the appropriate <see cref="Domain.Meteo.IMeteoData.Data"/>.
    /// </summary>
    public class TimeDependentFunctionSplitter : ITimeDependentFunctionSplitter
    {
        public ICollection<IFunction> SplitIntoFunctionsPerArgumentValue(IFunction function)
        {
            Ensure.NotNull(function, nameof(function));

            if (function.Arguments.Count < 2 || function.Arguments[0].ValueType != typeof(DateTime))
            {
                throw new ArgumentException(Resources.TimeDependentFunctionSplitter_SplitIntoFunctionsPerArgumentValue_Not_a_valid_time_dependent_function);
            }

            return function.Arguments[1].Values.OfType<object>().
                Select(i => ExtractSeriesForArgumentValueCore(function, i)).
                ToList();
        }

        public IFunction ExtractSeriesForArgumentValue(IFunction function, object argumentValue)
        {
            Ensure.NotNull(function, nameof(function));
            Ensure.NotNull(argumentValue, nameof(argumentValue));

            return ExtractSeriesForArgumentValueCore(function, argumentValue);
        }

        private static IFunction ExtractSeriesForArgumentValueCore(IFunction function, object argumentValue)
        {

            IFunction func = function.Filter(
                new VariableValueFilter<object>(function.Arguments[1], argumentValue), 
                new VariableReduceFilter(function.Arguments[1]));

            switch (argumentValue)
            {
                case INameable nameable:
                    func.Components[0].Name = nameable.Name;
                    break;
                case string value:
                    func.Components[0].Name = value;
                    break;
            }

            return func;
        }
    }
}