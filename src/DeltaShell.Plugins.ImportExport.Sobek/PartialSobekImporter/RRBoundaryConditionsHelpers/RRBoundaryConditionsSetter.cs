using System;
using System.Collections.Generic;
using DelftTools.Utils.Guards;
using DeltaShell.Plugins.DelftModels.RainfallRunoff;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts;
using DeltaShell.Plugins.ImportExport.Sobek.Properties;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using DHYDRO.Common.Logging;

namespace DeltaShell.Plugins.ImportExport.Sobek.PartialSobekImporter.RRBoundaryConditionsHelpers
{
    /// <summary>
    /// Setter to set the <see cref="RainfallRunoffBoundaryData"/> for <see cref="UnpavedData"/> in the
    /// <see cref="RainfallRunoffModel"/>.
    /// </summary>
    public class RRBoundaryConditionsSetter
    {
        private readonly ILogHandler logHandler;

        /// <summary>
        /// Constructs <see cref="RRBoundaryConditionsSetter"/> to set the <see cref="RainfallRunoffBoundaryData"/> for
        /// <see cref="UnpavedData"/> in the <see cref="RainfallRunoffModel"/>.
        /// </summary>
        /// <param name="logHandler">Log handler to log information.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="logHandler"/> is null.</exception>
        public RRBoundaryConditionsSetter(ILogHandler logHandler)
        {
            Ensure.NotNull(logHandler, nameof(logHandler));
            this.logHandler = logHandler;
        }

        /// <summary>
        /// Sets the <see cref="RainfallRunoffBoundaryData"/> for <see cref="UnpavedData"/> in the
        /// <see cref="RainfallRunoffModel"/>.
        /// </summary>
        /// <param name="rrBoundarySetterDataBlock">
        /// Data block containing the data for setting the linked boundary data, data in
        /// format of <see cref="RRModelBoundarySetterData"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">When <paramref name="rrBoundarySetterDataBlock"/> is <c>null</c>.</exception>
        public void Set(RRModelBoundarySetterData rrBoundarySetterDataBlock)
        {
            Ensure.NotNull(rrBoundarySetterDataBlock, nameof(rrBoundarySetterDataBlock));

            foreach (KeyValuePair<string, RainfallRunoffBoundaryData> block in rrBoundarySetterDataBlock.BoundaryDataByBoundaryName)
            {
                string boundaryName = block.Key;
                RainfallRunoffBoundaryData boundaryData = block.Value;

                if (rrBoundarySetterDataBlock.LateralToCatchmentLookup.TryGetValue(boundaryName, out SobekRRLink[] incomingLinksToBoundary))
                {
                    SetAllLinkedBoundaryData(incomingLinksToBoundary, rrBoundarySetterDataBlock.UnpavedDataByName, boundaryData);
                }
                else
                {
                    logHandler.ReportWarning(string.Format(Resources.RRBoundaryConditionsSetter_Set_Could_not_find__0__linked_to_boundary_, boundaryName));
                }
            }
        }

        private void SetAllLinkedBoundaryData(SobekRRLink[] incomingLinksToBoundary, IReadOnlyDictionary<string, UnpavedData> unpavedDataByName, RainfallRunoffBoundaryData boundaryData)
        {
            foreach (SobekRRLink linkToBoundary in incomingLinksToBoundary)
            {
                if (unpavedDataByName.TryGetValue(linkToBoundary.NodeFromId, out UnpavedData unpavedData))
                {
                    unpavedData.BoundaryData = boundaryData;
                }
            }
        }
    }
}