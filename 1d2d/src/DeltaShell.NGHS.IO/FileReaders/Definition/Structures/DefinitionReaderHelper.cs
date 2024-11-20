using System;
using System.Globalization;
using System.Linq;
using DelftTools.Functions;
using DeltaShell.NGHS.Utils.Extensions;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.Structures
{
    internal static class DefinitionReaderHelper
    {
        internal static IFunction CreateFunctionFromArrays(this IFunction function, double[] argumentValues,
            double[] componentValues)
        {
            if (argumentValues == null || componentValues == null)
            {
                return null;
            }

            function.Clear();
            function.Arguments[0].SetValues(argumentValues);
            function.Components[0].SetValues(componentValues);
            return function;
        }

        internal static double[] ToDoubleArray(this string valuesString)
        {
            return valuesString?.SplitOnEmptySpace().Select(v => Convert.ToDouble(v, CultureInfo.InvariantCulture))
                .ToArray();
        }
    }
}