namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Unpaved
{
    public class DeZeeuwHellingaDrainageFormula : ErnstDeZeeuwHellingaDrainageFormulaBase
    {
        public override bool IsErnst
        {
            get { return false; }
        }

        public override string ToString()
        {
            return "De Zeeuw Hellinga";
        }
    }
}