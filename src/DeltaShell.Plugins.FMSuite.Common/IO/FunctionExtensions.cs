using System.Linq;
using DelftTools.Functions;

namespace DeltaShell.Plugins.FMSuite.Common.IO
{
    public static class FunctionExtensions
    {
        // TODO: candidate for moving to the Framework?
        public static int GetComponentIndexByName(this IFunction function, string componentName)
        {
            var componentIndexGrouping = function.Components
                .Select((component, index) => new { component, index })
                .FirstOrDefault(cig => cig.component.Name == componentName);

            return componentIndexGrouping == null ? -1 : componentIndexGrouping.index;
        }
    }
}
