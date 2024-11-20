using System.Collections.Generic;
using System.Linq;
using DelftTools.Shell.Gui;
using DelftTools.Utils.Reflection;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    /// <summary>
    /// <see cref="IMapLayerProvider">MapLayerProvider</see> that works with a predefined set of <see cref="IMapLayerCreationInfo"/> objects.
    /// <para>
    /// This reroutes the <see cref="IMapLayerProvider">MapLayerProvider</see> functions to the correct <see cref="IMapLayerCreationInfo"/> object
    /// that matches in data type and is allowed by <see cref="CanCreateLayerFor">CanCreateLayerFor</see></para> 
    /// </summary>
    public class MapLayerCreationInfoMapLayerProvider : IMapLayerProvider
    {
        private readonly IMapLayerCreationInfo[] layerCreators;

        /// <summary>
        /// Constructs the <see cref="MapLayerCreationInfoMapLayerProvider"/> with the provided <paramref name="layerCreators"/>
        /// </summary>
        /// <param name="layerCreators">List of <see cref="IMapLayerCreationInfo"/> to use to resolve the <see cref="IMapLayerProvider"/> calls</param>
        public MapLayerCreationInfoMapLayerProvider(IMapLayerCreationInfo[] layerCreators)
        {
            this.layerCreators = layerCreators;
        }
        
        /// <inheritdoc cref="IMapLayerProvider"/>
        public virtual ILayer CreateLayer(object data, object parentData)
        {
            return GetMapLayerCreator(data, parentData)?.CreateLayer(data, parentData);
        }

        /// <inheritdoc cref="IMapLayerProvider"/>
        public virtual bool CanCreateLayerFor(object data, object parentData)
        {
            return GetMapLayerCreator(data, parentData) != null;
        }

        /// <inheritdoc cref="IMapLayerProvider"/>
        public virtual IEnumerable<object> ChildLayerObjects(object data)
        {
            return GetMapLayerCreator(data)?.ChildLayerObjects(data) ?? Enumerable.Empty<object>();
        }

        /// <inheritdoc cref="IMapLayerProvider"/>
        public virtual void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            GetMapLayerCreator(layerObject, parentObject)?.AfterCreate(layer, layerObject, parentObject, objectsLookup);
        }

        private IMapLayerCreationInfo GetMapLayerCreator(object data, object parentData = null)
        {
            return data != null
                       ? layerCreators
                           .FirstOrDefault(c =>
                                               data.GetType().Implements(c.SupportedType)
                                               && (parentData == null || c.CanBuildWithParent(parentData)))
                       : null;
        }
    }
}