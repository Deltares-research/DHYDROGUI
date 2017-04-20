using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor
{
    public class Sediment
    {
        public Dictionary<string, SedMorProperty> Properties { get; private set; }

        public Sediment()
        {
            Properties = new Dictionary<string, SedMorProperty>();
        }

        public string Name
        {
            get { return (string)Properties[SedProperties.SedName].Value; }
            set { Properties[SedProperties.SedName].Value = value; }
        }

        public double Density
        {
            get { return (double)Properties[SedProperties.RhoSol].Value; }
            set { Properties[SedProperties.RhoSol].Value = value; }
        }
    }
}