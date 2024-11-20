using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using BruTile.Extensions;

namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    [ExcludeFromCodeCoverage] // contacts web (causes unstable tests)
    public class DefaultRequestHandler : IRequestHandler
    {
        /// <inheritdoc cref="IRequestHandler"/>
        public string DoRequest(string requestUrl)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(requestUrl);
            using (HttpWebResponse webResponse = webRequest.GetSyncResponse(10000))
            {
                if (webResponse == null)
                {
                    throw new WebException("An_error_occurred_while_fetching_tile", null);
                }

                using (Stream responseStream = webResponse.GetResponseStream())
                {
                    if (responseStream == null)
                    {
                        return "";
                    }

                    using (var streamReader = new StreamReader(responseStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }
}