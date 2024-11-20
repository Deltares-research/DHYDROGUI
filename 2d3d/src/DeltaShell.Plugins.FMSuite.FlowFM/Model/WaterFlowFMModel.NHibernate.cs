namespace DeltaShell.Plugins.FMSuite.FlowFM.Model
{
    public partial class WaterFlowFMModel
    {
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