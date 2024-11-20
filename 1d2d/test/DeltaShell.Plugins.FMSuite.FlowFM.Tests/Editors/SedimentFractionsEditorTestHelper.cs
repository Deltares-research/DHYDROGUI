using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    public static class SedimentFractionsEditorTestHelper
    {
        public static IEventedList<ISedimentFraction> GetExampleSedimentFractions(int count)
        {
            var sedimentFractions = new EventedList<ISedimentFraction>();

            for (var i = 0; i < count; i++)
            {
                var fractionName = string.Format("Fraction_{0}_of_{1}", i, count);
                sedimentFractions.Add(new SedimentFraction() { Name = fractionName });
            }
            
            return sedimentFractions;
        }
    }
}
