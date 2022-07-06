using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using GeoAPI.Extensions.Networks;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CompositeStructureView
{
    public static class CompositeStructureViewHelper
    {
        public static void UpdateMinMaxForBranchFeatures(IList<IBranchFeature> BranchFeatures, ref double minValue, ref double maxValue)
        {
            double min, max;
            var localMinMaxValues = new List<double>();
            foreach (var branchFeature in BranchFeatures)
            {
                if (branchFeature is ICompositeBranchStructure)
                {
                    var compositeBranchStructure = (ICompositeBranchStructure)branchFeature;
                    foreach (var structure in compositeBranchStructure.Structures)
                    {
                        min = max = double.NaN;
                        if (structure is IWeir)
                        {
                            UpdateMinMaxForWeir((IWeir)structure, ref min, ref max);
                        }
                        if (structure is IPump)
                        {
                            UpdateMinMaxForPump((IPump) structure, ref min, ref max);
                        }
                        if (structure is IBridge)
                        {
                            UpdateMinMaxForBridge((IBridge)structure, ref min, ref max);
                        }
                        if (structure is ICulvert)
                        {
                            UpdateMinMaxForCulvert((ICulvert)structure, ref min, ref max);
                        }
                        localMinMaxValues.AddRange(new[] {min, max});
                    }
                }
                if (branchFeature is ICrossSection)
                {
                    var crossSectionDefinition = ((ICrossSection)branchFeature).Definition;
                    // this is a hack; a cross sections highest or lowest point may not be valid when newly added;
                    double low = crossSectionDefinition.LowestPoint;
                    double high = crossSectionDefinition.HighestPoint;
                    localMinMaxValues.AddRange(new[] {low, high});
                }
            }
            if (localMinMaxValues.Any())
            {
                var localMinMaxValuesNoNaN = localMinMaxValues.Where(v => !double.IsNaN(v)).ToList();
                if (localMinMaxValuesNoNaN.Any())
                {
                    minValue = double.IsNaN(minValue) ? localMinMaxValuesNoNaN.Min() : Math.Min(localMinMaxValuesNoNaN.Min(), minValue);
                    maxValue = double.IsNaN(maxValue) ? localMinMaxValuesNoNaN.Max() : Math.Max(localMinMaxValuesNoNaN.Max(), maxValue);
                }
            }
        }

        private static void UpdateMinMaxForCulvert(ICulvert culvert, ref double min, ref double max)
        {
            //all z values from inlet and outlet crossection
            var zValues = culvert.CrossSectionDefinitionAtInletAbsolute.ZWDataTable.
                Concat(culvert.CrossSectionDefinitionAtOutletAbsolute.ZWDataTable).
                Select(hfsw => hfsw.Z).Where(v => !double.IsNaN(v)).ToList();
            
            if (zValues.Any())
            {
                min = double.IsNaN(min) ? zValues.Min() : Math.Min(min, zValues.Min());
                max = double.IsNaN(max) ? zValues.Max() : Math.Max(max, zValues.Max());
            }
        }

        private static void UpdateMinMaxForBridge(IBridge bridge, ref double min, ref double max)
        {
            var zValues = bridge.BridgeType == BridgeType.YzProfile
                ? bridge.YZCrossSectionDefinition.YZDataTable.Select(h => h.Z).Where(v => !double.IsNaN(v)).ToList()
                : bridge.EffectiveCrossSectionDefinition.ZWDataTable.Select(h => h.Z).Where(v => !double.IsNaN(v)).ToList();
            if (zValues.Any())
            {
                min = double.IsNaN(min) ? zValues.Min() : Math.Min(min, zValues.Min());
                max = double.IsNaN(max) ? zValues.Max() : Math.Max(max, zValues.Max());
            }
        }

        private static void UpdateMinMaxForPump(IPump pump, ref double min, ref double max)
        {
            //all z related values of pump (levels and pump OffsetZ)
            var zValues = new[]
            {
                pump.OffsetZ,
                pump.StartDelivery,
                pump.StartSuction,
                pump.StopDelivery,
                pump.StopSuction
            }.Where(v => !double.IsNaN(v)).ToList();
            if (zValues.Any())
            {
                min = double.IsNaN(min) ? zValues.Min() : Math.Min(min, zValues.Min());
                max = double.IsNaN(max) ? zValues.Max() : Math.Max(max, zValues.Max());
            }
        }

        private static void UpdateMinMaxForWeir(IWeir weir, ref double min, ref double max)
        {
            max = double.IsNaN(max) ? weir.CrestLevel : Math.Max(max, weir.CrestLevel);
            //add 10 for weir
            if (weir.WeirFormula is IGatedWeirFormula)
            {
                var formula = (IGatedWeirFormula)weir.WeirFormula;
                var newVal = weir.CrestLevel + formula.GateOpening;
                if (!double.IsNaN(newVal)) max = Math.Max(max, newVal);    
            }
            //we want to see a weir when it is below the bottom
            min = double.IsNaN(min) ? weir.CrestLevel : Math.Min(min, weir.CrestLevel);
        }
    }
}
