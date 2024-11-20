using System;
using DelftTools.Functions;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    public interface IBoundaryCondition : IEditableObject, IFeatureData, ICloneable
    {
        /// <summary>
        /// Physical process, like water flow, sediments transport, ..., see <see cref="FunctionAttributes.StandardProcessNames"/> for a list of possible values.
        /// </summary>
        string ProcessName { get; }

        /// <summary>
        /// Boundary condition description.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Variable being used for this boundary condition.
        /// </summary>
        string VariableName { get; }

        /// <summary>
        /// Description of the quantity.
        /// </summary>
        string VariableDescription { get; }

        /// <summary>
        /// Defines structure of the data in <see cref="PointData"/> functions (e.g. const, time series, harmonic, etc.).
        /// </summary>
        BoundaryConditionDataType DataType { get; set; }

        /// <summary>
        /// Actual boundary feature.
        /// </summary>
        IFeature Feature { get; }

        /// <summary>
        /// Indices of the coordinates in where boundary condition series are defined.
        /// </summary>
        IEventedList<int> DataPointIndices { get; }

        /// <summary>
        /// Function that represents a boundary condition data for every point in <see cref="DataPointIndices" />.
        /// </summary>
        IEventedList<IFunction> PointData { get; }

        /// <summary>
        /// Z layers.
        /// </summary>
        IEventedList<VerticalProfileDefinition> PointDepthLayerDefinitions { get; }

        /// <summary>
        /// When true - only a single point can be used for data.
        /// </summary>
        bool IsHorizontallyUniform { get; }

        /// <summary>
        /// When true - only <see cref="float"/> can be used to define Z layers.
        /// </summary>
        bool IsVerticallyUniform { get; }
        
        /// <summary>
        /// Gets or sets the timezone for this boundary condition.

        /// </summary>
        TimeSpan TimeZone { get; set; }

        /// <summary>
        /// Adds a new point and data, depth layers. Returns the created function.
        /// </summary>
        /// <param name="i"></param>
        void AddPoint(int i);

        /// <summary>
        /// Removes point, data and depth layer for point.
        /// </summary>
        /// <param name="i"></param>
        void RemovePoint(int i);

        /// <summary>
        /// Gets point data for point <paramref name="i"/>.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        IFunction GetDataAtPoint(int i);

        /// <summary>
        /// Gets Z layer for point <paramref name="i"/>.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        VerticalProfileDefinition GetDepthLayerDefinitionAtPoint(int i);
    }
}