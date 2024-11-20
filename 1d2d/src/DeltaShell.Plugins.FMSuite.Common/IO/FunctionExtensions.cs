using System;
using System.Linq;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class FunctionExtensions
    {
        public static void RemoveComponentByName(this IFunction function, string componentName)
        {
            var componentIndex = function.GetComponentIndexByName(componentName);
            if (componentIndex >= 0) function.Components.RemoveAt(componentIndex);
        }

        private static int GetComponentIndexByName(this IFunction function, string componentName)
        {
            var componentIndexGrouping = function.Components
                .Select((component, index) => new { component, index })
                .FirstOrDefault(cig => cig.component.Name.Equals(componentName, StringComparison.InvariantCultureIgnoreCase));

            return componentIndexGrouping?.index ?? -1;
        }
    }
}
