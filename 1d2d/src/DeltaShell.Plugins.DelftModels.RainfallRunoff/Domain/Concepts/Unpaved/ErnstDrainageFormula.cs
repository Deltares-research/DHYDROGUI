namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved
{
    public class ErnstDrainageFormula : ErnstDeZeeuwHellingaDrainageFormulaBase
    {
        public override bool IsErnst
        {
            get { return true; }
        }

        public override string ToString()
        {
            return "Ernst";
        }
    }
}