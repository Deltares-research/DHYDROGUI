using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.SedMor.ModelDefinition;

namespace DeltaShell.Plugins.FMSuite.FlowFM.SedMor
{
    public class MorphologyBoundary
    {
        public Dictionary<string, SedMorProperty> Properties { get; private set; }

        public string Name
        {
            get { return (string)Properties[MorProperties.Name].Value; }
            set { Properties[MorProperties.Name].Value = value; }
        }

        public MorphologyBoundary()
        {
            Properties = new Dictionary<string, SedMorProperty>();
        }
    }
}