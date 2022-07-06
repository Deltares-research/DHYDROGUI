namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public enum SobekObjectType
    {
        /// <summary>
        /// Fixed grid point
        /// </summary>
        SBK_GRIDPOINTFIXED,
    }

    public class SobekObjectTypeData
    {

        public string ID { get; set; }
        public SobekObjectType Type { get; set; }
    }
}