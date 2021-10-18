namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    /// <summary>
    /// Data found in the casedesc.cmt file of the Sobek case
    /// </summary>
    public class SobekCaseData
    {
        /// <summary>
        /// The full path to the wind file.
        /// </summary>
        public string WindDataPath { get; set; }
        
        /// <summary>
        /// The full path to the bui file.
        /// </summary>
        public string BuiDataPath { get; set; }
    }
}
