using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Reflection;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.StructureFeatureView
{
    internal class GateViewData
    {
        private readonly Dictionary<string, GateOpeningDirection> gateOpeningDirections;
 
        public GateViewData()
        {
            // generate a dictionary of gate opening directions with nice names
            gateOpeningDirections = new Dictionary<string, GateOpeningDirection>();
            foreach (var dir in Enum.GetValues(typeof(GateOpeningDirection)))
            {
                var name = ((GateOpeningDirection)dir).GetDescription();
                gateOpeningDirections[name] = (GateOpeningDirection)dir;
            }
        }

        public IDictionary<string,GateOpeningDirection> GetGateOpeningTypes()
        {
            return gateOpeningDirections;
        }

        public GateOpeningDirection GetGateOpeningType(string nameInDictionary)
        {
            return gateOpeningDirections[nameInDictionary];
        }

        public string GetGateOpeningName(GateOpeningDirection horizontalOpeningDirection)
        {
            return gateOpeningDirections.First(kvp => kvp.Value == horizontalOpeningDirection).Key;
        }
    }
}