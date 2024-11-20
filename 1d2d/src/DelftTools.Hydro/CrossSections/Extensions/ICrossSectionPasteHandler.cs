namespace DelftTools.Hydro.CrossSections.Extensions
{
    /// <summary>
    /// interface Handler for pasting into a cross-section.
    /// </summary>
    internal interface ICrossSectionPasteHandler
    {
        /// <summary>
        /// Last handling to be taken to finish the paste.
        /// </summary>
        void FinishPasteActions();
    }
}