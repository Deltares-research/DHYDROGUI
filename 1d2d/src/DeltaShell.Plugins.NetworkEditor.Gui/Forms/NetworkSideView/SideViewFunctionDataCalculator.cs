using System;
using System.Collections.Generic;
using System.Linq;
using Deltares.Infrastructure.API.Guards;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.NetworkSideView
{
    /// <summary>
    /// Class for computing the chainages and function values of a side view with additional structure chainage locations.
    /// </summary>
    internal sealed class SideViewFunctionDataCalculator
    {
        private int outputIndex;
        private int chainageIndex;
        private int structureIndex;

        private IReadOnlyList<double> chainages;
        private IReadOnlyList<double> values;
        private IReadOnlyList<double> structureChainages;

        private IList<double> outputChainages;
        private IList<double> outputValues;

        // The offset applied to structure chainages to create a point just before and just after the location
        private const double offset = 0.001d;
        
        /// <summary>
        /// The computed chainages
        /// </summary>
        public IList<double> OutputChainages
        {
            get => outputChainages;
        }

        /// <summary>
        /// The function values corresponding to the computed chainages
        /// </summary>
        public IList<double> OutputValues
        {
            get => outputValues;
        }

        /// <summary>
        /// Adds structure chainages to side view function data, consisting of chainages and corresponding values.
        /// For each structure two data points are added, with a small (1mm) offset before and after the chainage. 
        /// 
        /// The input chainages and structure chainages are expected to be monotonous, and the output chainages are monotonous.
        /// When the offset for a structure chainage would cause the sequence to become non-monotonous, the offset is cropped to
        /// assure that the output chainages are monotonous.   
        /// <example>
        /// If the structure has a chainage of 10.000. Two additional points are added to an existing function
        /// at chainage 9.999 and 10.001. The values for these new data points are equal to the values
        /// closest to the point before the structure and after the structure.
        /// 
        /// If a chainage of 10.000 already existed, the left structure point is added at chainage 10.000.
        /// </example>
        /// </summary>
        /// <param name="chainages">The location chainages. These are expected to be monotonous.</param>
        /// <param name="values">The function values corresponding to the location chainages.</param>
        /// <param name="structureChainages">The chainages at which structures can be found. These are expected to be monotonous</param>
        /// <exception cref="ArgumentNullException">Thrown when any argument is <c>null</c>.</exception>
        public void Calculate(IReadOnlyList<double> chainages, IReadOnlyList<double> values, IReadOnlyList<double> structureChainages)
        {
            Ensure.NotNull(chainages, nameof(chainages));
            Ensure.NotNull(values, nameof(values));
            Ensure.NotNull(structureChainages, nameof(structureChainages));

            this.chainages = chainages;
            this.values = values;
            this.structureChainages = structureChainages;
            
            if (!structureChainages.Any())
            {
                outputChainages = chainages.ToArray();
                outputValues = values.ToArray();
                return;
            }
            
            int chainageCount = chainages.Count;
            int structureCount = structureChainages.Count;
            int newCount = chainageCount + 2 * structureCount;
            outputChainages = new double[newCount];
            outputValues = new double[newCount];
            
            chainageIndex = 0;
            structureIndex = 0;
            outputIndex = 0;

            // When the location chainage coincides with a structure chainage we first use the location chainage. 
            // Use a tiny tolerance to avoid evaluating equality of two doubles
            double tolerance = 0.001 * offset;
            while (chainageIndex < chainageCount && structureIndex < structureCount)
            {
                if (CurrentChainageValue() < CurrentStructureChainageValue() + tolerance)
                {
                    UseCurrentChainageAsNextValue();
                }
                else
                {
                    UseCurrentStructureChainageAsNextValue();
                }
            }

            while (chainageIndex < chainageCount)
            {
                UseCurrentChainageAsNextValue();
            }

            while (structureIndex < structureCount)
            {
                UseCurrentStructureChainageAsNextValue();
            }
        }

        private double CurrentChainageValue()
        {
            return chainages[chainageIndex];
        }

        private double NextChainageValue()
        {
            return chainageIndex < chainages.Count - 1 ? chainages[chainageIndex + 1] : -1;
        }

        private double CurrentStructureChainageValue()
        {
            return structureChainages[structureIndex];
        }

        private double NextStructureChainageValue()
        {
            return structureIndex < structureChainages.Count - 1 ? structureChainages[structureIndex + 1] : -1;
        }

        private void InsertChainageAndWaterLevel(double chainage, double value)
        {
            outputChainages[outputIndex] = chainage;
            outputValues[outputIndex] = value;
            outputIndex++;
        }

        private void UseCurrentChainageAsNextValue()
        {
            InsertChainageAndWaterLevel(chainages[chainageIndex], values[chainageIndex]);
            chainageIndex++;
        }

        private void UseCurrentStructureChainageAsNextValue()
        {
            InsertChainageAndWaterLevel(ComputeChainageBeforeStructure(), GetWaterLevelClosestBeforeStructure());
            InsertChainageAndWaterLevel(ComputeChainageAfterStructure(), GetWaterLevelClosestAfterStructure());
            structureIndex++;
        }

        // The before-structure chainage is shifted back from the structure chainage by a fixed amount. If this 
        // happens to result in a chainage less than the previous chainage the before-structure chainage is clamped to
        // the previous chainage to assure the resulting chainages are monotonous. 
        private double ComputeChainageBeforeStructure()
        {
            double chainage = Math.Max(0, structureChainages[structureIndex] - offset);

            if (outputIndex > 0)
            {
                chainage = Math.Max(chainage, outputChainages[outputIndex - 1]);
            }

            return chainage;
        }

        private double GetWaterLevelClosestBeforeStructure()
        {
            if (StructureIsFirstItemOfUpdatedFunction())
            {
                return values[0];
            }

            return values[chainageIndex - 1];
        }

        private bool StructureIsFirstItemOfUpdatedFunction()
        {
            return chainageIndex < 1;
        }

        // The after-structure-chainage is shifted forward from the structure chainage by a fixed amount. If this 
        // happens to result in a chainage larger than the next chainage, the after-structure-chainage is clamped to
        // the next chainage to assure the resulting chainages are monotonous. 
        private double ComputeChainageAfterStructure()
        {
            double chainage = structureChainages[structureIndex] + offset;
            var nextChainage = NextChainageValue();
            if (nextChainage >= 0) chainage = Math.Min(chainage, nextChainage);

            var nextStructureChainage = NextStructureChainageValue();
            if (nextStructureChainage >= 0) chainage = Math.Min(chainage, nextStructureChainage - 0.5 * offset);

            return chainage;
        }

        private double GetWaterLevelClosestAfterStructure()
        {
            if (chainageIndex >= chainages.Count)
            {
                return values[chainageIndex - 1];
            }

            return values[chainageIndex];
        }
    }
}