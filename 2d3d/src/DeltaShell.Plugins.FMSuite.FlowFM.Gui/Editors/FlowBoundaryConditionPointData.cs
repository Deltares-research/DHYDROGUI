using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class FlowBoundaryConditionPointData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlowBoundaryConditionPointData"/> class.
        /// </summary>
        /// <param name="boundaryCondition">The boundary condition.</param>
        /// <param name="supportPoint">The support point.</param>
        /// <param name="useLayers">if set to <c>true</c> [use layers].</param>
        public FlowBoundaryConditionPointData(FlowBoundaryCondition boundaryCondition, int supportPoint, bool useLayers)
        {
            BoundaryCondition = boundaryCondition;
            SupportPoint = supportPoint;
            UseLayers = useLayers;
        }

        public FlowBoundaryCondition BoundaryCondition { get; }

        public IFunction Function => BoundaryCondition == null ? null : BoundaryCondition.GetDataAtPoint(SupportPoint);

        public BoundaryConditionDataType ForcingType => BoundaryCondition == null ? BoundaryConditionDataType.Empty : BoundaryCondition.DataType;

        public int ForcingTypeDimension
        {
            get
            {
                if (BoundaryCondition.FlowQuantity == FlowBoundaryQuantityType.MorphologyBedLoadTransport)
                {
                    return BoundaryCondition.SedimentFractionNames.Count;
                }

                switch (ForcingType)
                {
                    case BoundaryConditionDataType.Empty:
                        return 0;
                    case BoundaryConditionDataType.TimeSeries:
                    case BoundaryConditionDataType.Qh:
                        return 1;
                    case BoundaryConditionDataType.AstroComponents:
                    case BoundaryConditionDataType.Harmonics:
                        return 2;
                    case BoundaryConditionDataType.AstroCorrection:
                    case BoundaryConditionDataType.HarmonicCorrection:
                        return 4;
                    default:
                        throw new NotImplementedException("Forcing type unknown to flow module.");
                }
            }
        }

        public int VariableDimension => BoundaryCondition == null ? 0 : BoundaryCondition.VariableDimension;

        public bool UseLayers { get; }

        /// <summary>
        /// Filters the layers and components.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <param name="variableComponent">The variable component.</param>
        /// <returns></returns>
        public IEnumerable<IVariable> FilterLayersAndComponents(int layer, int variableComponent)
        {
            if (Function == null)
            {
                yield break;
            }

            int startIndex = (variableComponent + (layer * VariableDimension)) * ForcingTypeDimension;

            for (int i = startIndex; i < startIndex + ForcingTypeDimension; i++)
            {
                yield return Function.Components[i];
            }
        }

        private int SupportPoint { get; }
    }
}