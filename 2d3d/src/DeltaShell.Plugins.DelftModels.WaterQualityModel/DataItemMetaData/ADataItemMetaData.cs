namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData
{
    public abstract class ADataItemMetaData
    {
        protected string name;
        protected string tag;

        public string Name => name;
        public string Tag => tag;
    }
}