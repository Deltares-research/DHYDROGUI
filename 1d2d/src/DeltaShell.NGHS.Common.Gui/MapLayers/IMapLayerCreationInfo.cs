using System;
using System.Collections.Generic;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    public interface IMapLayerCreationInfo
    {
        /// <summary>
        /// Type of the IMapLayerProvider data object
        /// </summary>
        Type SupportedType { get; }

        /// <summary>
        /// Indicates if a layer can be build for the provided <paramref name="parentData"/>
        /// </summary>
        /// <param name="parentData">Parent data to check</param>
        bool CanBuildWithParent(object parentData);

        /// <summary>
        /// Creates a layer for the provided <paramref name="data"/> and <paramref name="parentData"/>
        /// </summary>
        /// <param name="data">Data to create a layer for</param>
        /// <param name="parentData">Parent object of the <paramref name="data"/></param>
        /// <returns>Layer for <paramref name="data"/></returns>
        ILayer CreateLayer(object data, object parentData);

        /// <summary>
        /// Child objects for <paramref name="data"/> to create additional layers for
        /// </summary>
        /// <param name="data">Data to check</param>
        /// <returns>Child objects for the <paramref name="data"/></returns>
        IEnumerable<object> ChildLayerObjects(object data);

        /// <summary>
        /// Called after creation of the layer
        /// </summary>
        /// <param name="layer">The created layer</param>
        /// <param name="layerObject">The object that the layer represents</param>
        /// <param name="parentObject">Parent object of the <paramref name="layerObject"/></param>
        /// <param name="objectsLookup">Lookup for the layer objects</param>
        void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup);
    }
}