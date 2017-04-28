using System;
using System.Linq;

namespace RainfallRunoffModelEngine
{
    /// <summary>
    /// Processes aggregated calls during execute to minimize the number of remote calls (=overhead) during execute. (Makes 
    /// the interface less chatty) This class lives in the remote instance. It works together with the RemoteRRApiWrapper, 
    /// which lives inside DeltaShell.
    /// </summary>
    public class RRRemoteHybridEngine : RRModelHybridEngine, IRRRemoteModelApi
    {
        private QuantityType[] quantities;
        private ElementSet[] elementSets;
        private int[] sizes;

        public void SetAllValuesFormat(int[] quantities, int[] elementSets, int[] sizes)
        {
            if ((quantities.Length != elementSets.Length) || (quantities.Length != sizes.Length))
            {
                throw new ArgumentException("All input arrays must be of same length");
            }
            this.quantities = quantities.Select(q => (QuantityType)q).ToArray();
            this.elementSets = elementSets.Select(e => (ElementSet)e).ToArray();
            this.sizes = sizes;
        }

        public double[] Execute(double[] boundaryLevelsIn, ref bool modelRan)
        {
            base.SetValues(QuantityType.BndLevels, ElementSet.BoundaryElmSet, boundaryLevelsIn);
            
            modelRan = base.ModelPerformTimeStep();
            
            var returnValues = new double[sizes.Sum()];
            var destinationIndex = 0;
            for (int i = 0; i < quantities.Length; i++)
            {
                var values = new double[sizes[i]];
                base.GetValues(quantities[i], elementSets[i], ref values);
                Array.Copy(values, 0, returnValues, destinationIndex, values.Length);
                destinationIndex += sizes[i];
            }

            return returnValues;
        }
    }
}