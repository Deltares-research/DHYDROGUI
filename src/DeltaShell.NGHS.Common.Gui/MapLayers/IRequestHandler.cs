namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    public interface IRequestHandler
    {
        /// <summary>
        /// Translates the url get response into a string
        /// </summary>
        /// <param name="requestUrl">Url to request</param>
        /// <returns>Response text</returns>
        string DoRequest(string requestUrl);
    }
}