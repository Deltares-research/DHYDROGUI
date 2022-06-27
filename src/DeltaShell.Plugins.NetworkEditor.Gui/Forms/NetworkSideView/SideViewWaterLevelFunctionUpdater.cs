using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Utils.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// Class for updating an existing water level function of the side view.
    /// </summary>
    public static class SideViewWaterLevelFunctionUpdater
    {
        private const int numberOfDataPointsPerStructure = 2;

        /// <summary>
        /// Updates an existing water level function with two new data points for each given structure.
        /// One data point is added just before the location of structure and the other point is added
        /// right after the location of the structure.
        /// <example>
        /// If the structure has a chainage of 10. Two additional points are added to an existing function
        /// at chainage 9.999 and 10.001. The water levels for these new data points are equal to the water
        /// levels closest to the point before the structure and after the structure.
        /// </example>
        /// </summary>
        /// <param name="function">The water level function to update.</param>
        /// <param name="structureChainages">The chainages at which structures can be found.</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public static void UpdateFunctionWithExtraDataPointsForStructures(IFunction function,
                                                                          IReadOnlyList<double> structureChainages)
        {
            Ensure.NotNull(function, nameof(function));
            Ensure.NotNull(structureChainages, nameof(structureChainages));
            
            if (!structureChainages.Any())
            {
                return;
            }

            UpdateWaterLevelFunctionWithAdditionalDataFromStructures(function, structureChainages);
        }

        private static void UpdateWaterLevelFunctionWithAdditionalDataFromStructures(IFunction function,
                                                                                IReadOnlyList<double> structureChainages)
        {
            double[] chainages = GetChainagesFromFunction(function);
            double[] waterLevels = GetWaterLevelsFromFunction(function);

            int newNumberOfDataPoints = GetNewDataPointCount(chainages, structureChainages);

            var updatedChainages = new double[newNumberOfDataPoints];
            var updatedWaterLevels = new double[newNumberOfDataPoints];

            CreateUpdatedFunctionData(chainages, waterLevels, structureChainages, updatedChainages, updatedWaterLevels);

            UpdateWaterLevelFunction(function, updatedChainages, updatedWaterLevels);
        }

        private static double[] GetChainagesFromFunction(IFunction function)
        {
            return function.Arguments[0].GetValues<double>().ToArray();
        }

        private static double[] GetWaterLevelsFromFunction(IFunction function)
        {
            return function.Components[0].GetValues<double>().ToArray();
        }

        private static int GetNewDataPointCount(IReadOnlyCollection<double> chainages,
                                                      IReadOnlyCollection<double> structureChainages)
        {
            return chainages.Count + numberOfDataPointsPerStructure * structureChainages.Count;
        }

        private static void CreateUpdatedFunctionData(IReadOnlyList<double> chainages,
                                                      IReadOnlyList<double> waterLevels,
                                                      IReadOnlyList<double> structureChainages,
                                                      IList<double> updatedChainages,
                                                      IList<double> updatedWaterLevels)
        {
            var updateHelper = new FunctionDataUpdateHelper(chainages, waterLevels, structureChainages,
                                                            updatedChainages, updatedWaterLevels);

            while (BothChainageCollectionsHaveDataLeftToProcess(updateHelper))
            {
                ProcessChainageCollections(updateHelper);
            }

            while (ChainageCollectionHasDataLeftToProcess(updateHelper))
            {
                ProcessRemainingChainages(updateHelper);
            }

            while (StructureChainageCollectionHasDataLeftToProcess(updateHelper))
            {
                ProcessRemainingStructureChainages(updateHelper);
            }
        }
        
        private static void ProcessChainageCollections(FunctionDataUpdateHelper updateHelper)
        {
            if (updateHelper.CurrentChainageValue() < updateHelper.CurrentStructureChainageValue())
            {
                updateHelper.UseCurrentChainageAsNextValue();
            }
            else
            {
                updateHelper.UseCurrentStructureChainageAsNextValue();
            }
        }

        private static void ProcessRemainingChainages(FunctionDataUpdateHelper updateHelper)
        {
            updateHelper.UseCurrentChainageAsNextValue();
        }
        
        private static void ProcessRemainingStructureChainages(FunctionDataUpdateHelper updateHelper)
        {
            updateHelper.UseCurrentStructureChainageAsNextValue();
        }
        
        private static bool BothChainageCollectionsHaveDataLeftToProcess(FunctionDataUpdateHelper functionDataUpdateHelper)
        {
            return functionDataUpdateHelper.ChainageIndex < functionDataUpdateHelper.ChainagesCount 
                   && functionDataUpdateHelper.StructureChainageIndex < functionDataUpdateHelper.StructureChainagesCount;
        }

        private static bool StructureChainageCollectionHasDataLeftToProcess(FunctionDataUpdateHelper updateHelper)
        {
            return updateHelper.StructureChainageIndex < updateHelper.StructureChainagesCount;
        }
        
        private static bool ChainageCollectionHasDataLeftToProcess(FunctionDataUpdateHelper updateHelper)
        {
            return updateHelper.ChainageIndex < updateHelper.ChainagesCount;
        }
        
        private static void UpdateWaterLevelFunction(IFunction function, 
                                                     IEnumerable<double> updatedChainages, 
                                                     IEnumerable<double> updatedWaterLevels)
        {
            function.Arguments[0].Clear();
            function.Components[0].Clear();
            function.Arguments[0].SetValues(updatedChainages);
            function.Components[0].SetValues(updatedWaterLevels);
        }

        private sealed class FunctionDataUpdateHelper
        {
            private const double structureShift = 0.001;

            private int newCollectionIndex;

            private readonly IReadOnlyList<double> chainages;
            private readonly IReadOnlyList<double> waterLevels;
            private readonly IReadOnlyList<double> structureChainages;

            private readonly IList<double> updatedChainages;
            private readonly IList<double> updatedWaterLevels;
            
            public FunctionDataUpdateHelper(IReadOnlyList<double> chainages, 
                                IReadOnlyList<double> waterLevels,
                                IReadOnlyList<double> structureChainages,
                                IList<double> updatedChainages,
                                IList<double> updatedWaterLevels)
            {
                ChainageIndex = 0;
                StructureChainageIndex = 0;
                newCollectionIndex = 0;

                this.chainages = chainages;
                this.waterLevels = waterLevels;
                this.structureChainages = structureChainages;
                this.updatedChainages = updatedChainages;
                this.updatedWaterLevels = updatedWaterLevels;

                ChainagesCount = chainages.Count();
                StructureChainagesCount = structureChainages.Count();
            }

            public int ChainageIndex { get; private set; }
            public int StructureChainageIndex { get; private set; }
            public int ChainagesCount { get; }
            public int StructureChainagesCount { get; }

            public double CurrentChainageValue()
            {
                return chainages[ChainageIndex];
            }

            public double CurrentStructureChainageValue()
            {
                return structureChainages[StructureChainageIndex];
            }

            public void UseCurrentChainageAsNextValue()
            {
                updatedChainages[newCollectionIndex] = chainages[ChainageIndex];
                updatedWaterLevels[newCollectionIndex] = waterLevels[ChainageIndex];

                ChainageIndex++;
                newCollectionIndex++;
            }

            public void UseCurrentStructureChainageAsNextValue()
            {
                SetDataPointBeforeStructure();

                newCollectionIndex++;

                SetDataPointAfterStructure();

                StructureChainageIndex++;
                newCollectionIndex++;
            }

            private void SetDataPointBeforeStructure()
            {
                double chainage = structureChainages[StructureChainageIndex] - structureShift;
                updatedChainages[newCollectionIndex] = chainage < 0 ? 0 : chainage;
                updatedWaterLevels[newCollectionIndex] = GetWaterLevelClosestBeforeStructure();
            }

            private double GetWaterLevelClosestBeforeStructure()
            {
                if (StructureIsFirstItemOfUpdatedFunction())
                {
                    return waterLevels[0];
                }

                return waterLevels[ChainageIndex - 1];

            }

            private bool StructureIsFirstItemOfUpdatedFunction()
            {
                return ChainageIndex - 1 < 0;
            }

            private void SetDataPointAfterStructure()
            {
                updatedChainages[newCollectionIndex] = structureChainages[StructureChainageIndex] + structureShift;
                updatedWaterLevels[newCollectionIndex] = GetWaterLevelClosestAfterStructure();
            }

            private double GetWaterLevelClosestAfterStructure()
            {
                if (ChainageIndex >= chainages.Count)
                {
                    return waterLevels[ChainageIndex - 1];
                }

                return waterLevels[ChainageIndex];
            }
        }
    }
}