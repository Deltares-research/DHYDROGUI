using System.ComponentModel;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Shell.Gui;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.PropertyGrid
{
    public class SewerConnectionProperties : ObjectProperties<SewerConnection>
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SewerConnectionProperties));

        [Category("General")]
        public double Length { get { return data.Length;} }

        [Category("General")]
        public double LevelSource { get { return data.LevelSource;}  }

        [Category("General")]
        public double LevelTarget { get { return data.LevelTarget;}  }

        [Category("General")]
        public string SourceCompartmentName { get { return data.SourceCompartmentName;}  }

        [Category("General")]
        public string TargetCompartmentName { get { return data.TargetCompartmentName;}  }

        [Category("General")]
        public SewerConnectionSpecialConnectionType SewerConnectionSpecialConnectionType { get { return data.SpecialConnectionType; } }
    }
}