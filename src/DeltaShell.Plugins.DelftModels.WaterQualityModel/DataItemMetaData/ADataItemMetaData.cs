namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.DataItemMetaData
{
    public abstract class ADataItemMetaData
    {
        protected string name;
        protected string tag;
        
        public string Name { get { return name; } }
        public string Tag { get { return tag; } }
    }
}
