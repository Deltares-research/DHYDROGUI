using System;
using System.Collections.Generic;
using System.Linq;
using SharpMap.Api.Layers;

namespace DeltaShell.NGHS.Common.Gui.MapLayers
{
    /// <inheritdoc cref="MapLayerCreationInfo{TData,TParent}"/>
    public class MapLayerCreationInfo<TData> : MapLayerCreationInfo<TData, object>
    {

    }

    /// <summary>
    /// Object defining how to create a layer for a <typeparam name="TData"/> object 
    /// </summary>
    /// <typeparam name="TData">Type of the data</typeparam>
    /// <typeparam name="TParent">Type of the parent data</typeparam>
    public class MapLayerCreationInfo<TData, TParent> : IMapLayerCreationInfo
    {
        /// <inheritdoc cref="IMapLayerCreationInfo"/>
        public Type SupportedType { get { return typeof(TData); } }

        /// <inheritdoc cref="IMapLayerCreationInfo"/>
        public bool CanBuildWithParent(object parentData)
        {
            return parentData is TParent typedParentData 
                   && (CanBuildWithParentFunc?.Invoke(typedParentData) ?? true);
        }

        /// <inheritdoc cref="IMapLayerCreationInfo"/>
        public ILayer CreateLayer(object data, object parentData)
        {
            return CreateLayerFunc?.Invoke((TData)data, (TParent)parentData);
        }

        /// <inheritdoc cref="IMapLayerCreationInfo"/>
        public IEnumerable<object> ChildLayerObjects(object data)
        {
            return ChildLayerObjectsFunc?.Invoke((TData)data) ?? Enumerable.Empty<object>();
        }

        /// <inheritdoc cref="IMapLayerCreationInfo"/>
        public void AfterCreate(ILayer layer, object layerObject, object parentObject, IDictionary<ILayer, object> objectsLookup)
        {
            AfterCreateFunc?.Invoke(layer, (TData)layerObject, (TParent)parentObject, objectsLookup);
        }

        /// <summary>
        /// Typed version of <see cref="CanBuildWithParent"/> that can be set
        /// </summary>
        public Func<TParent, bool> CanBuildWithParentFunc { get; set; }

        /// <summary>
        /// Typed version of <see cref="CreateLayer"/> that can be set
        /// </summary>
        public Func<TData, TParent, ILayer> CreateLayerFunc { get; set; }

        /// <summary>
        /// Typed version of <see cref="ChildLayerObjects"/> that can be set
        /// </summary>
        public Func<TData, IEnumerable<object>> ChildLayerObjectsFunc { get; set; }

        /// <summary>
        /// Typed version of <see cref="AfterCreate"/> that can be set
        /// </summary>
        public Action<ILayer, TData, TParent, IDictionary<ILayer, object>> AfterCreateFunc { get; set; }
    }
}