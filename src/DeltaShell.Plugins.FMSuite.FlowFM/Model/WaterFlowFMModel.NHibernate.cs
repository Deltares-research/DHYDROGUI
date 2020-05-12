using System;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
        [Obsolete("Necessary due to NHibernate")]
        public WaterFlowFMModel() : this(null) { }

        private int dirtyCounter; // tells NHibernate we need to be saved

        private void MarkDirty()
        {
            unchecked
            {
                dirtyCounter++;
            } //unchecked is default, but its here to declare intent
        }
    }
}
