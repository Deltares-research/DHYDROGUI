using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Coverages;
using NetTopologySuite.Extensions.Coverages;
using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Coverages
{
    public static class VariableExtensions
    {
        public static void ClearWithoutEventing(this IVariable variable)
        {
            variable.BeginEdit("clear coverage"); 
            IMultiDimensionalArray targetMda = variable.Values;
            if (targetMda.Count == 0)
            {
                return;
            }

            bool wasFiring = targetMda.FireEvents;
            bool wasAutoSorted = targetMda.IsAutoSorted;
            try
            {
                targetMda.FireEvents = false;
                targetMda.IsAutoSorted = false;
                targetMda.Clear();
            }
            finally
            {
                targetMda.FireEvents = wasFiring;
                targetMda.IsAutoSorted = wasAutoSorted;
                variable.EndEdit();
            }
        }
    }
}