using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using BruTile;
using BruTile.Wmts;
using BruTile.Wmts.Generated;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.Utils;
using log4net;
using SharpMap.Api.Layers;
using SharpMap.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    public class WmtsGroupLayer : GroupLayer
    {
        private string url;
        private readonly IRequestHandler handler;
        private static readonly ILog log = LogManager.GetLogger(typeof(WmtsGroupLayer));

        public WmtsGroupLayer(): this(new DefaultRequestHandler())
        {
            
        }

        public WmtsGroupLayer(IRequestHandler requestHandler)
        {
            handler = requestHandler ?? new DefaultRequestHandler();
            LayersReadOnly = true;
            NameIsReadOnly = true;
            ReadOnly = true;
        }
        
        /// <summary>
        /// Url of the WMTS service
        /// </summary>
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                Initialize();
            }
        }

        /// <summary>
        /// Initialize and create sub-layers
        /// </summary>
        private void Initialize()
        {
            this.DoWithPropertySet(nameof(LayersReadOnly), false, () =>
            {
                Layers = new EventedList<ILayer>(CreateChildLayers());
            });

            // Only first layer should be visible by default
            var firstLayer = Layers.FirstOrDefault();
            if (firstLayer != null)
            {
                firstLayer.Visible = true;
            }
        }

        public override object Clone()
        {
            var clone = (WmtsGroupLayer) base.Clone();
            clone.Url = url;
            return clone;
        }

        /// <summary>
        /// Generates child layers from WMTS capabilities
        /// </summary>
        /// <returns></returns>
        private IEnumerable<ILayer> CreateChildLayers()
        {
            if (Url == null)
            {
                yield break;
            }

            // Get lookup and tile sources
            Dictionary<string, ResourceUrl> layerResourceUrlLookup;
            IEnumerable<IGrouping<string, ITileSource>> groupedTileSources;

            try
            {
                var capabilitiesText = handler.DoRequest($"{url}?REQUEST=GetCapabilities&SERVICE=WMTS");
                var capabilities = GetCapabilities(capabilitiesText);

                // set name with general tile
                name = capabilities?.ServiceIdentification?.Title?.FirstOrDefault()?.Value;

                layerResourceUrlLookup = GetLayerResourceUrlLookup(capabilities, url);

                using (var stream = GenerateStreamFromString(capabilitiesText))
                {
                    var tileSources = WmtsParser.Parse(stream);
                    groupedTileSources = tileSources.GroupBy(s => ((WmtsTileSchema)s.Schema).Layer);
                }
            }
            catch (WebException exception)
            {
                log.Error($"Could not load tiles from \"{url}\"", exception);
                yield break;
            }

            foreach (IGrouping<string, ITileSource> tileSourceGrouping in groupedTileSources)
            {
                var foundResourceUrl = layerResourceUrlLookup[tileSourceGrouping.Key];

                yield return new WmtsLayer(tileSourceGrouping.ToArray(), foundResourceUrl)
                {
                    Visible = false
                };
            }
        }

        /// <summary>
        /// Creates a dictionary with the <see cref="ResourceUrl"/> for each layer in the <see cref="Capabilities"/> content
        /// </summary>
        /// <param name="capabilities">Capabilities of the service</param>
        /// <param name="url">Url of the service</param>
        /// <returns>Dictionary with the <see cref="ResourceUrl"/> for each layer</returns>
        private static Dictionary<string, ResourceUrl> GetLayerResourceUrlLookup(Capabilities capabilities, string url)
        {
            var layerResourceUrlLookup = new Dictionary<string, ResourceUrl>();

            foreach (LayerType contentsLayer in capabilities.Contents.Layers)
            {
                var identifier = contentsLayer.Identifier.Value;
                var format = contentsLayer.Format.FirstOrDefault();
                var resourceUrlTemplate = contentsLayer.ResourceURL?.FirstOrDefault();

                var resourceUrl = resourceUrlTemplate != null
                                      ? new ResourceUrl
                                      {
                                          Format = resourceUrlTemplate.format,
                                          ResourceType = resourceUrlTemplate.resourceType,
                                          Template = resourceUrlTemplate.template
                                      }
                                      : new ResourceUrl
                                      {
                                          Format = format,
                                          ResourceType = URLTemplateTypeResourceType.tile,
                                          Template = $@"{url}/{identifier}/" + @"{TileMatrixSet}/{TileMatrix}/{TileCol}/{TileRow}." + format?.Split('/')[1]
                                      };

                layerResourceUrlLookup.Add(identifier, resourceUrl);
            }

            return layerResourceUrlLookup;
        }

        /// <summary>
        /// Parses the <paramref name="capabilitiesText"/> to a <see cref="Capabilities"/> object
        /// </summary>
        /// <param name="capabilitiesText">XML text of the capabilities</param>
        /// <returns><see cref="Capabilities"/> object from <paramref name="capabilitiesText"/></returns>
        private static Capabilities GetCapabilities(string capabilitiesText)
        {
            using (Stream stream = GenerateStreamFromString(capabilitiesText))
            using (var streamReader = new StreamReader(stream))
            {
                return (Capabilities)new XmlSerializer(typeof(Capabilities)).Deserialize(streamReader);
            }
        }

        /// <summary>
        /// Creates a stream from the provided string <paramref name="s"/>
        /// </summary>
        /// <param name="s">String to use</param>
        /// <returns>Stream from string</returns>
        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}