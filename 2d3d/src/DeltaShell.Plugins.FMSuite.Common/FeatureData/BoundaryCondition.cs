using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Units;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DelftTools.Utils.Editing;
using DelftTools.Utils.Reflection;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Features;
using NetTopologySuite.Extensions.Features.Generic;
using NetTopologySuite.Extensions.Geometries;

namespace DeltaShell.Plugins.FMSuite.Common.FeatureData
{
    [Entity]
    public abstract class BoundaryCondition :
        FeatureData<IEventedList<IFunction>, Feature2D>, IBoundaryCondition
    {
        private BoundaryConditionDataType dataType;

        private bool syncing;

        private IGeometry previousGeometry;

        protected BoundaryCondition(BoundaryConditionDataType type)
        {
            dataType = type;

            DataPointIndices = new EventedList<int>();
            DataPointIndices.CollectionChanged += DataPointIndicesCollectionChanged;

            PointData = new EventedList<IFunction>();
            PointData.CollectionChanged += PointDataCollectionChanged;

            PointDepthLayerDefinitions = new EventedList<VerticalProfileDefinition>();
            PointDepthLayerDefinitions.CollectionChanged += PointDepthLayerDefinitionsCollectionChanged;
        }

        [Aggregation]
        public override Feature2D Feature
        {
            get => base.Feature;
            set
            {
                if (Feature != null)
                {
                    ((INotifyPropertyChange) Feature).PropertyChanged -= FeaturePropertyChanged;
                }

                base.Feature = value;
                if (Feature != null)
                {
                    ((INotifyPropertyChange) Feature).PropertyChanged += FeaturePropertyChanged;
                    previousGeometry = Feature.Geometry;
                }
            }
        }

        [FeatureAttribute(Order = 1)]
        [ReadOnly(true)]
        [DisplayName("Boundary")]
        public string FeatureName => Feature.Name;

        public abstract IUnit VariableUnit { get; }

        public abstract int VariableDimension { get; }

        public abstract string ProcessName { get; }

        public virtual string Description => VariableDescription;

        public abstract string VariableName { get; }

        public abstract string VariableDescription { get; }

        public abstract bool IsHorizontallyUniform { get; }

        public abstract bool IsVerticallyUniform { get; }

        [FeatureAttribute(Order = 4)]
        [ReadOnly(true)]
        [DisplayName("Forcing type")]
        public BoundaryConditionDataType DataType
        {
            get => dataType;
            set
            {
                if (value == dataType)
                {
                    return;
                }

                BoundaryConditionDataType previousDataType = dataType;

                dataType = value;

                AfterDataTypeChanged(previousDataType);
            }
        }

        public IEventedList<int> DataPointIndices { get; private set; }

        public IEventedList<IFunction> PointData
        {
            get => Data;
            private set => Data = value;
        }

        public IEventedList<VerticalProfileDefinition> PointDepthLayerDefinitions { get; }

        public bool IsEditing { get; private set; }

        public bool EditWasCancelled { get; private set; }

        public IEditAction CurrentEditAction { get; private set; }

        IFeature IBoundaryCondition.Feature => base.Feature;

        public void ClearData()
        {
            syncing = true;
            DataPointIndices.Clear();
            PointData.Clear();
            PointDepthLayerDefinitions.Clear();
            syncing = false;
        }

        public virtual void AddPoint(int i)
        {
            if (!DataPointIndices.Contains(i))
            {
                DataPointIndices.Add(i);
            }
        }

        public virtual void RemovePoint(int i)
        {
            if (DataPointIndices.Contains(i))
            {
                if (IsHorizontallyUniform)
                {
                    ClearData();
                }
                else
                {
                    DataPointIndices.Remove(i);
                }
            }
        }

        public IFunction GetDataAtPoint(int i)
        {
            if (IsHorizontallyUniform)
            {
                return PointData.FirstOrDefault();
            }

            if (!DataPointIndices.Contains(i))
            {
                return null;
            }

            int pointIndex = DataPointIndices.IndexOf(i);

            return pointIndex < PointData.Count ? PointData[pointIndex] : null;
        }

        public VerticalProfileDefinition GetDepthLayerDefinitionAtPoint(int i)
        {
            if (!DataPointIndices.Contains(i))
            {
                return null;
            }

            if (IsHorizontallyUniform)
            {
                return PointDepthLayerDefinitions[0];
            }

            int pointIndex = DataPointIndices.IndexOf(i);

            return pointIndex < PointDepthLayerDefinitions.Count ? PointDepthLayerDefinitions[pointIndex] : null;
        }

        public void BeginEdit(string action)
        {
            IsEditing = true;
        }

        public void BeginEdit(IEditAction action)
        {
            IsEditing = true;
        }

        public void EndEdit()
        {
            IsEditing = false;
        }

        public void CancelEdit()
        {
            IsEditing = false;
        }

        public object Clone()
        {
            throw new NotImplementedException("Cloning boundary conditions is not implemented yet");
        }

        protected override void UpdateName()
        {
            Name = Feature == null ? "" : FeatureName + " (" + VariableName + ")";
        }

        protected virtual IFunction ConvertMultiLayerFunction(int supportPoint,
                                                              BoundaryConditionDataType previousDataType)
        {
            IFunction function = GetDataAtPoint(supportPoint);
            if (function == null)
            {
                return null;
            }

            if (function.GetValues().Count > 0 && BoundaryDataConverter.CanConvert(previousDataType, DataType))
            {
                int numLayers = GetDepthLayerDefinitionAtPoint(supportPoint).LayerNames.Count();
                int dimension = VariableDimension;
                return BoundaryDataConverter.ConvertDataType(function, previousDataType, DataType,
                                                             numLayers * dimension);
            }

            return CreateMultiLayerFunction(supportPoint);
        }

        protected virtual IFunction CreateFunction()
        {
            var componentSuffixes = new List<string>();
            if (VariableDimension < 2)
            {
                componentSuffixes.Add("");
            }
            else if (VariableDimension < 3)
            {
                componentSuffixes.AddRange(new[]
                {
                    "_x",
                    "_y"
                });
            }
            else if (VariableDimension < 4)
            {
                componentSuffixes.AddRange(new[]
                {
                    "_x",
                    "_y",
                    "_z"
                });
            }
            else
            {
                componentSuffixes.AddRange(Enumerable.Range(1, VariableDimension + 1).Select(i => "_" + i));
            }

            IFunction function = new Function(VariableName);

            switch (DataType)
            {
                case BoundaryConditionDataType.TimeSeries:
                    AddTimeSeriesFunction(function, componentSuffixes);
                    break;
                case BoundaryConditionDataType.AstroComponents:
                    AddAstroComponentsFunction(function, componentSuffixes);
                    break;
                case BoundaryConditionDataType.AstroCorrection:
                    AddAstroCorrectionFunction(function, componentSuffixes);
                    break;
                case BoundaryConditionDataType.Harmonics:
                    AddHarmonicsFunction(function, componentSuffixes);
                    break;
                case BoundaryConditionDataType.HarmonicCorrection:
                    AddHarmonicsCorrectionFunction(function, componentSuffixes);
                    break;
                case BoundaryConditionDataType.Constant:
                    AddConstantFunction(componentSuffixes, function);
                    break;
                case BoundaryConditionDataType.Empty:
                    break;
                default:
                    throw new InvalidOperationException("Data type not supported");
            }

            return function;
        }

        protected virtual void FeaturePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Feature2D.Geometry))
            {
                BeginEdit("Syncing data with geometry points");

                List<int> newIndices =
                    DataPointIndices.Select(
                        i => LookupIndexForCoordinate(previousGeometry.Coordinates[i],
                                                      Feature.Geometry)).ToList();

                if (Feature.Geometry.Coordinates.Count() != previousGeometry.Coordinates.Count())
                {
                    for (int i = DataPointIndices.Count - 1; i > -1; i--)
                    {
                        if (newIndices[i] == -1)
                        {
                            DataPointIndices.RemoveAt(i);
                            newIndices.RemoveAt(i);
                        }
                        else
                        {
                            syncing = true;
                            DataPointIndices[i] = newIndices[i];
                            syncing = false;
                        }
                    }
                }

                EndEdit();
            }

            previousGeometry = Feature.Geometry;
        }

        protected virtual void AfterDataTypeChanged(BoundaryConditionDataType previousDataType)
        {
            BeginEdit("Syncing data with data type");

            if (IsHorizontallyUniform && !DataPointIndices.Any())
            {
                AddPoint(0);
            }
            else
            {
                for (var i = 0; i < DataPointIndices.Count; ++i)
                {
                    if (IsVerticallyUniform)
                    {
                        syncing = true;
                        PointDepthLayerDefinitions[i] = new VerticalProfileDefinition();
                        syncing = false;
                    }

                    syncing = true;
                    PointData[i] = ConvertMultiLayerFunction(DataPointIndices[i], previousDataType);
                    syncing = false;
                }
            }

            EndEdit();
        }

        private void AddConstantFunction(IEnumerable<string> componentSuffixes, IFunction function)
        {
            foreach (string componentSuffix in componentSuffixes)
            {
                function.Components.Add(new Variable<double>(VariableName + componentSuffix, VariableUnit));
            }
        }

        private void AddHarmonicsCorrectionFunction(IFunction function, IEnumerable<string> componentSuffixes)
        {
            function.Arguments.Add(new Variable<double>("Frequency", new Unit("degree per hour", "deg/h")));
            foreach (string componentSuffix in componentSuffixes)
            {
                function.Components.Add(new Variable<double>("Amplitude" + componentSuffix, VariableUnit) {NoDataValue = double.NaN});
                function.Components.Add(new Variable<double>("Phase" + componentSuffix,
                                                             new Unit("degree", "deg")) {NoDataValue = double.NaN});
                function.Components.Add(new Variable<double>("Amplitude corr." + componentSuffix, new Unit("-"))
                {
                    NoDataValue = double.NaN,
                    DefaultValue = (double) 1
                });
                function.Components.Add(new Variable<double>("Phase corr." + componentSuffix,
                                                             new Unit("degree", "deg"))
                {
                    NoDataValue = double.NaN,
                    DefaultValue = (double) 0
                });
            }
        }

        private void AddHarmonicsFunction(IFunction function, IEnumerable<string> componentSuffixes)
        {
            function.Arguments.Add(new Variable<double>("Frequency", new Unit("degree per hour", "deg/h")));
            foreach (string componentSuffix in componentSuffixes)
            {
                function.Components.Add(new Variable<double>("Amplitude" + componentSuffix, VariableUnit) {NoDataValue = double.NaN});
                function.Components.Add(new Variable<double>("Phase" + componentSuffix,
                                                             new Unit("degree", "deg")) {NoDataValue = double.NaN});
            }
        }

        private void AddAstroCorrectionFunction(IFunction function, IEnumerable<string> componentSuffixes)
        {
            function.Arguments.Add(new Variable<string>("Component", new Unit("", "-")) {IsAutoSorted = false});
            foreach (string componentSuffix in componentSuffixes)
            {
                function.Components.Add(new Variable<double>("Amplitude" + componentSuffix, VariableUnit) {NoDataValue = double.NaN});
                function.Components.Add(new Variable<double>("Phase" + componentSuffix,
                                                             new Unit("degree", "deg")) {NoDataValue = double.NaN});
                function.Components.Add(new Variable<double>("Amplitude corr." + componentSuffix, new Unit("-"))
                {
                    NoDataValue = double.NaN,
                    DefaultValue = (double) 1
                });
                function.Components.Add(new Variable<double>("Phase corr." + componentSuffix,
                                                             new Unit("degree", "deg"))
                {
                    NoDataValue = double.NaN,
                    DefaultValue = (double) 0
                });
            }
        }

        private void AddAstroComponentsFunction(IFunction function, IEnumerable<string> componentSuffixes)
        {
            function.Arguments.Add(new Variable<string>("Component", new Unit("", "-")) {IsAutoSorted = false});
            foreach (string componentSuffix in componentSuffixes)
            {
                function.Components.Add(new Variable<double>("Amplitude" + componentSuffix, VariableUnit) {NoDataValue = double.NaN});
                function.Components.Add(new Variable<double>("Phase" + componentSuffix,
                                                             new Unit("degree", "deg")) {NoDataValue = double.NaN});
            }
        }

        private void AddTimeSeriesFunction(IFunction function, IEnumerable<string> componentSuffixes)
        {
            function.Arguments.Add(new Variable<DateTime>("Time"));
            foreach (string componentSuffix in componentSuffixes)
            {
                function.Components.Add(new Variable<double>(VariableName + componentSuffix, VariableUnit) {NoDataValue = double.NaN});
            }
        }

        private IFunction CreateMultiLayerFunction(int supportPoint)
        {
            IFunction function = CreateFunction();

            VerticalProfileDefinition verticalProfileDefinition = GetDepthLayerDefinitionAtPoint(supportPoint);

            if (verticalProfileDefinition != null && verticalProfileDefinition.ProfilePoints > 1)
            {
                int componentCount = function.Components.Count;
                for (var i = 1; i < verticalProfileDefinition.ProfilePoints; ++i)
                {
                    for (var j = 0; j < componentCount; ++j)
                    {
                        var component = (IVariable) function.Components[j].Clone(false);
                        component.Name += "(" + (i + 1) + ")";
                        function.Components.Add(component);
                    }
                }

                for (var j = 0; j < componentCount; ++j)
                {
                    function.Components[j].Name += "(1)";
                }
            }

            return function;
        }

        private bool ValidIndex(int i)
        {
            return i >= 0 && i < Feature.Geometry.Coordinates.Count();
        }

        private void DataPointIndicesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            int removedOrAddedIndex = e.GetRemovedOrAddedIndex();
            if (syncing)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    if (!ValidIndex((int) removedOrAddedItem))
                    {
                        throw new ArgumentException("Attempt to add invalid support point");
                    }

                    if (DataPointIndices.Count(p => p == (int) removedOrAddedItem) > 1)
                    {
                        throw new ArgumentException("Attempt to duplicate support point");
                    }

                    var verticalProfileDefinition = new VerticalProfileDefinition();

                    syncing = true;
                    PointDepthLayerDefinitions.Add(verticalProfileDefinition);
                    verticalProfileDefinition.PointDepths.CollectionChanged += VerticalProfilePointsChanged;
                    PointData.Add(CreateMultiLayerFunction((int) removedOrAddedItem));
                    syncing = false;

                    break;

                case NotifyCollectionChangedAction.Remove:

                    syncing = true;
                    PointData.RemoveAt(removedOrAddedIndex);
                    VerticalProfileDefinition profileDefinition =
                        PointDepthLayerDefinitions.ElementAtOrDefault(removedOrAddedIndex);
                    if (profileDefinition != null)
                    {
                        profileDefinition.PointDepths.CollectionChanged -= VerticalProfilePointsChanged;
                    }

                    PointDepthLayerDefinitions.RemoveAt(removedOrAddedIndex);
                    syncing = false;

                    break;

                case NotifyCollectionChangedAction.Replace:

                    throw new NotImplementedException("Replacing point indices is not supported.");
            }
        }

        private void PointDataCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (syncing)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    throw new NotImplementedException("Adding data without specifying support point is not allowed.");

                case NotifyCollectionChangedAction.Remove:

                    syncing = true;
                    int removedOrAddedIndex = e.GetRemovedOrAddedIndex();
                    DataPointIndices.RemoveAt(removedOrAddedIndex);
                    VerticalProfileDefinition profileDefinition =
                        PointDepthLayerDefinitions.ElementAtOrDefault(removedOrAddedIndex);
                    if (profileDefinition != null)
                    {
                        profileDefinition.PointDepths.CollectionChanged -= VerticalProfilePointsChanged;
                    }

                    PointDepthLayerDefinitions.RemoveAt(removedOrAddedIndex);
                    syncing = false;

                    break;

                case NotifyCollectionChangedAction.Replace:

                    throw new NotImplementedException("Replacing data in boundary condition is not allowed.");
            }
        }

        private void PointDepthLayerDefinitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (syncing)
            {
                return;
            }

            object removedOrAddedItem = e.GetRemovedOrAddedItem();
            int removedOrAddedIndex = e.GetRemovedOrAddedIndex();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    throw new NotImplementedException("Adding layers without specifying support point is not allowed.");

                case NotifyCollectionChangedAction.Remove:

                    syncing = true;

                    if (removedOrAddedItem != null)
                    {
                        ((VerticalProfileDefinition) removedOrAddedItem).PointDepths.CollectionChanged -=
                            VerticalProfilePointsChanged;
                    }

                    DataPointIndices.RemoveAt(removedOrAddedIndex);
                    PointData.RemoveAt(removedOrAddedIndex);
                    syncing = false;

                    break;

                case NotifyCollectionChangedAction.Replace:

                    if (IsVerticallyUniform && ((VerticalProfileDefinition) removedOrAddedItem).ProfilePoints > 1)
                    {
                        throw new ArgumentException("Multi-layered data is not allowed for this boundary condition");
                    }

                    syncing = true;
                    if (e.OldItems[0] != null)
                    {
                        ((VerticalProfileDefinition) removedOrAddedItem).PointDepths.CollectionChanged -=
                            VerticalProfilePointsChanged;
                    }

                    if (removedOrAddedItem != null)
                    {
                        ((VerticalProfileDefinition) removedOrAddedItem).PointDepths.CollectionChanged +=
                            VerticalProfilePointsChanged;
                    }

                    IFunction data = PointData[removedOrAddedIndex];

                    IFunction functionTemplate = CreateMultiLayerFunction(DataPointIndices[removedOrAddedIndex]);

                    int oldComponentsCount = data.Components.Count;
                    int newComponentsCount = functionTemplate.Components.Count;

                    if (oldComponentsCount < newComponentsCount)
                    {
                        IEnumerable<IVariable> componentsToAdd = functionTemplate.Components.Skip(oldComponentsCount);
                        foreach (IVariable variable in componentsToAdd)
                        {
                            var component = (IVariable) TypeUtils.CreateGeneric(typeof(Variable<>), variable.ValueType,
                                                                                variable.Name);
                            component.Unit = variable.Unit;
                            component.NoDataValue = variable.NoDataValue;
                            component.DefaultStep = variable.DefaultStep;
                            component.DefaultValue = variable.DefaultValue;
                            component.AllowSetExtrapolationType = variable.AllowSetExtrapolationType;
                            component.AllowSetInterpolationType = variable.AllowSetInterpolationType;

                            data.Components.Add(component);
                        }
                    }
                    else if (oldComponentsCount > newComponentsCount)
                    {
                        List<IVariable> componentsToRemove = data.Components.Skip(newComponentsCount).ToList();
                        foreach (IVariable variable in componentsToRemove)
                        {
                            data.Components.Remove(variable);
                        }

                        if (removedOrAddedItem != null &&
                            ((VerticalProfileDefinition) removedOrAddedItem).ProfilePoints < 2)
                        {
                            foreach (IVariable component in data.Components)
                            {
                                component.Name = BaseName(component.Name);
                            }
                        }
                    }

                    syncing = false;

                    break;
            }
        }

        private static int LookupIndexForCoordinate(Coordinate coordinate, IGeometry geometry)
        {
            int coordinateCount = geometry.Coordinates.Count();
            var comparator = new CoordinateComparison2D();
            for (var i = 0; i < coordinateCount; ++i)
            {
                if (comparator.Equals(coordinate, geometry.Coordinates[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string BaseName(string name)
        {
            return name.EndsWith("(1)") ? name.Substring(0, name.Length - 3) : name;
        }

        private void VerticalProfilePointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VerticalProfileDefinition verticalProfile =
                PointDepthLayerDefinitions.FirstOrDefault(vp => vp.PointDepths.Equals(sender));
            if (verticalProfile == null)
            {
                return;
            }

            IFunction data = PointData.ElementAtOrDefault(PointDepthLayerDefinitions.IndexOf(verticalProfile));
            if (data == null)
            {
                return;
            }

            data.BeginEdit("Fixing layer components");
            object removedOrAddedItem = e.GetRemovedOrAddedItem();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    int compCount = data.Components.Count;
                    IFunction function = CreateFunction();
                    foreach (IVariable component in function.Components)
                    {
                        var variable =
                            new Variable<double>(BaseName(component.Name) + "(" + verticalProfile.ProfilePoints + ")");
                        variable.CopyFrom(component);
                        data.Components.Add(variable);
                    }

                    if (verticalProfile.ProfilePoints == 2)
                    {
                        for (var i = 0; i < compCount; ++i)
                        {
                            data.Components[i].Name = data.Components[i] + "(1)";
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    List<double> sortedPointDepths = verticalProfile.SortedPointDepths.ToList();
                    sortedPointDepths.Add((double) removedOrAddedItem);
                    sortedPointDepths.Sort();
                    int index = sortedPointDepths.IndexOf((double) removedOrAddedItem);
                    int componentCount = CreateFunction().Components.Count;
                    for (var i = 0; i < componentCount; ++i)
                    {
                        data.Components.RemoveAt(index);
                    }

                    if (verticalProfile.ProfilePoints == 1)
                    {
                        foreach (IVariable component in data.Components)
                        {
                            component.Name = BaseName(component.Name);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Replace:
                    sortedPointDepths = verticalProfile.SortedPointDepths.ToList();
                    index = sortedPointDepths.IndexOf((double) removedOrAddedItem);
                    sortedPointDepths.RemoveAt(index);
                    object oldItem = e.OldItems[0];
                    sortedPointDepths.Add((double) oldItem);
                    sortedPointDepths = verticalProfile.SortDepths(sortedPointDepths).ToList();
                    int oldIndex = sortedPointDepths.IndexOf((double) oldItem);
                    if (oldIndex != index)
                    {
                        componentCount = CreateFunction().Components.Count;
                        for (var i = 0; i < componentCount; i++)
                        {
                            object newComponent = data.Components[index].Values.Clone();
                            data.Components[index++].Values = data.Components[oldIndex].Values;
                            data.Components[oldIndex++].Values = newComponent as IMultiDimensionalArray;
                        }
                    }

                    break;
            }

            data.EndEdit();
        }
    }
}