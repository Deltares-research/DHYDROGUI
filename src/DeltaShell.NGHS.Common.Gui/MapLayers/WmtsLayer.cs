using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using BruTile;
using BruTile.Cache;
using BruTile.Web;
using BruTile.Wmts;
using DelftTools.Utils;
using DelftTools.Utils.Guards;
using DelftTools.Utils.IO;
using SharpMap.Extensions.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    public sealed class WmtsLayer : AsyncTileLayer
    {
        private ITileCache<byte[]> cache;
        private ITileSource selectedTileSource;
        private ResourceUrl resourceUrl;

        private WmtsLayer() // used for cloning/saving
        {

        }

        /// <summary>
        /// Creates a WMTS layer based on the <paramref name="tileSources"/> and <paramref name="resourceUrl"/>
        /// </summary>
        /// <param name="tileSources"><see cref="ITileSource"/>s for this layer (contains schema information)</param>
        /// <param name="resourceUrl">Describes the resource information (template etc.)</param>
        public WmtsLayer(IList<ITileSource> tileSources, ResourceUrl resourceUrl)
        {
            Ensure.NotNull(resourceUrl, nameof(resourceUrl));
            TransparentColor = Color.White;
            NameIsReadOnly = true;
            ReadOnly = true;
            this.resourceUrl = resourceUrl;
            TileSources = tileSources;
        }

        /// <summary>
        /// Available tile resources (projections)
        /// </summary>
        public IList<ITileSource> TileSources { get; private set; }

        /// <summary>
        /// Selected tile resource (projection)
        /// </summary>
        public ITileSource SelectedTileSource
        {
            get
            {
                return selectedTileSource ?? (selectedTileSource = TileSources.FirstOrDefault());
            }
            set
            {
                selectedTileSource = value;

                // re-initialize
                cache = null;
                FileUtils.DeleteIfExists(CacheLocation);
                Initialize();
                RenderRequired = true;
            }
        }

        /// <summary>
        /// Name of the current <see cref="SelectedTileSource"/>
        /// </summary>
        public override string Name
        {
            get
            {
                return SelectedTileSource?.Name;
            }
            set{}
        }

        private string CacheLocation
        {
            get
            {
                string path = SettingsHelper.GetApplicationLocalUserSettingsDirectory();
                return Path.Combine(path,
                                    $"cache_wmts_{WmtsTileSchema.Identifier}"
                                        .Replace(':', '_')
                                          .Replace('/', '_')
                                          .Replace('&', '_')
                                          .Replace('?', '_'));
            }
        }

        private WmtsTileSchema WmtsTileSchema
        {
            get { return SelectedTileSource?.Schema as WmtsTileSchema; }
        }

        public override object Clone()
        {
            var clone = (WmtsLayer)base.Clone();
            clone.TileSources = TileSources;
            clone.SelectedTileSource = SelectedTileSource;
            clone.resourceUrl = resourceUrl;
            return clone;
        }

        protected override ITileCache<byte[]> GetOrCreateCache()
        {
            if (cache != null)
            {
                return cache;
            }

            //no cache so mem
            cache = CacheLocation == null
                        ? (ITileCache<byte[]>)new MemoryCache<byte[]>(1000, 100000)
                        : new FileCache(CacheLocation, "jpg");

            return cache;
        }

        protected override ITileSchema CreateTileSchema()
        {
            return SelectedTileSource.Schema;
        }

        protected override IRequest CreateRequest()
        {
            // create a new ResourceUrl based on resourceUrl field and replace the tile matrix with that of
            // the selected TileResource schema
            var currentResourceUrl = new ResourceUrl
            {
                ResourceType = resourceUrl.ResourceType,
                Format = resourceUrl.Format,
                Template = resourceUrl.Template.Replace("{TileMatrixSet}", WmtsTileSchema.TileMatrixSet)
            };

            return new WmtsRequest(new []{ currentResourceUrl });
        }
    }
}