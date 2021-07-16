using System.Collections.Generic;
using System.Data;
using DelftTools.Functions;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using log4net;

namespace DeltaShell.Plugins.ImportExport.Sobek
{
    public class PumpBuilder:BranchStructureBuilderBase<Pump>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(PumpBuilder));

        private static void SetCapacity(IPump pump, DataRow row, double prevCap)
        {

            pump.Capacity = (double)row[0] - prevCap;
            pump.StartSuction = (double)row[1];
            pump.StopSuction = (double)row[2];                
            pump.StartDelivery = (double)row[3];
            pump.StopDelivery = (double)row[4];
        }

        private static PumpControlDirection GetPumpDirection(int dir)
        {
            switch (dir)
            {
                case -1:
                case 1:
                    return PumpControlDirection.SuctionSideControl;
                case -2:
                case 2:
                    return PumpControlDirection.DeliverySideControl;
                case -3:
                case 3:
                    return PumpControlDirection.SuctionAndDeliverySideControl;
                default:
                    return PumpControlDirection.SuctionAndDeliverySideControl;
            }
        }

        public override IEnumerable<Pump> GetBranchStructures(SobekStructureDefinition structure)
        {
            if (structure == null || (!(structure.Definition is SobekPump)))
                yield break;

            var sobekPump = structure.Definition as SobekPump;
            

            // No capacity data available then return a empty pump list
            // TODO: Find out what to do here? Logging / exception both? For now just log it an return an empty pump list
            if (sobekPump.CapacityTable.Rows.Count == 0)
            {
                Log.Warn("Couldn't import the pump structure");
                yield break;
            }

            // Create a pumps for each capacity DelftTools.Utils.Tuple
            var prevCap = 0.0;
            for (int i = 1; i <= sobekPump.CapacityTable.Rows.Count; i++) 
            {
                // Names will be numbered  if there are more then one
                string name = "";
                if (i > 1)
                {
                    name += i;
                }
                var pump = new Pump(name);

                var row = sobekPump.CapacityTable.Rows[i - 1];

                // Set capacity values
                SetCapacity(pump, row, prevCap);

                prevCap = (double) row[0];

                var dir = sobekPump.Direction;

                // Set control direction values
                pump.ControlDirection = GetPumpDirection(dir);
                pump.DirectionIsPositive = dir > 0;

                FunctionHelper.AddDataTableRowsToFunction(sobekPump.ReductionTable, pump.ReductionTable);

                yield return pump;
            }
        }
    }
}