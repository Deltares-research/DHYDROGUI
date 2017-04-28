namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum SobekObjectType
    {
        /// <summary>
        /// Fixed grid point
        /// </summary>
        SBK_GRIDPOINTFIXED,
        // TODO: Add more support for other types if needed
    }

    public class SobekObjectTypeData
    {

        public string ID { get; set; }
        public SobekObjectType Type { get; set; }
    }
}