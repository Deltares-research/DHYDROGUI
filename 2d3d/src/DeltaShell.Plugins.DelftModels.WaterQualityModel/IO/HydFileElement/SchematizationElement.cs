namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.IO.HydFileElement
{
    public class SchematizationElement : IHydFileElement
    {
        private LayerType layerType;
        private HydroDynamicModelType modelType;

        public IHydFileElement ParseValue(string textToParse)
        {
            modelType = HydFileStringValueParser.Parse<HydroDynamicModelType>(textToParse);
            layerType = HydFileStringValueParser.Parse<LayerType>(textToParse);

            return this;
        }

        public void SetDataTo(HydFileData hydFileData)
        {
            hydFileData.HydroDynamicModelType = modelType;
            hydFileData.LayerType = layerType;
        }
    }
}