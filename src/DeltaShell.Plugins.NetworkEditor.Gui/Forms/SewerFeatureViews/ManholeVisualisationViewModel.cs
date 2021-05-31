using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Extensions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ManholeVisualisationViewModel
    {
        private double minX;
        private double maxX;
        private double minY;
        private double maxY;

        private Manhole manhole;
        private IHydroNetwork network;
        private ObservableCollection<IDrawingShape> shapes;
        private bool isUpdating;
        private bool isCreating;

        public ManholeVisualisationViewModel()
        {
            Shapes = new ObservableCollection<IDrawingShape>();
        }

        public Manhole Manhole
        {
            get { return manhole; }
            set
            {
                if (manhole != null)
                {
                    manhole.Compartments.CollectionChanged -= CompartmentsOnCollectionChanged;
                }

                manhole = value;
                Network = manhole?.Network as IHydroNetwork;

                if (manhole != null)
                {
                    manhole.Compartments.CollectionChanged += CompartmentsOnCollectionChanged;
                }

                UpdateShapes();
                UpdateShapeDimensions();
            }
        }

        public ObservableCollection<IDrawingShape> Shapes
        {
            get { return shapes; }
            set
            {
                if (shapes != null)
                {
                    UnsubscribeShapePropertyChanged();
                    shapes.CollectionChanged -= ShapesOnCollectionChanged;
                }
                shapes = value;
                if (shapes != null)
                {
                    SubscribeShapePropertyChanged();
                    shapes.CollectionChanged += ShapesOnCollectionChanged;
                }
            }
        }

        private IEnumerable<PipeShape> PipeShapes { get { return shapes.OfType<PipeShape>(); } }

        private IEnumerable<CompartmentShape> CompartmentShapes { get { return shapes.OfType<CompartmentShape>(); } }

        public double HeightWidthRatio {get { return (maxY - minY) / (maxX - minX); } }
        public Func<double> ContainerWidth { get; set; }
        
        public Func<double> ContainerHeight { get; set; }

        public Action SetWindowSize { get; set; }

        private IHydroNetwork Network
        {
            get { return network; }
            set
            {
                if (network != null)
                {
                    network.Branches.CollectionChanged -= Branches_CollectionChanged;
                }
                network = value;
                if (network != null)
                {
                    network.Branches.CollectionChanged += Branches_CollectionChanged;
                }
            }
        }

        public bool ShowLabels { get; set; }

        public void SetShapesPixelValues()
        {
            foreach (var shape in Shapes)
            {
                shape.SetPixelValues(minX, maxX, minY, maxY, ContainerWidth.Invoke(), ContainerHeight.Invoke());
            }
        }

        public int GetIndexFor(Point pos)
        {
            for (var index = 0; index < shapes.Count; index++)
            {
                var drawingShape = shapes[index];
                var shapeMiddleX = drawingShape.LeftOffsetPix + 0.5 * drawingShape.WidthPix;

                if (pos.X < shapeMiddleX)
                {
                    return index;
                }
            }

            return shapes.Count;
        }

        private void UpdateShapes()
        {
            isCreating = true;

            try
            {
                Shapes.Clear();

                var drawingShapes = new List<IDrawingShape>();
                drawingShapes.AddRange(manhole.CreateShapes());
                var reorderedshapes = drawingShapes.OrderShapes();

                Shapes.AddRange(reorderedshapes);
            }
            finally
            {
                isCreating = false;
            }
        }

        [InvokeRequired]
        private void UpdateShapeDimensions()
        {
            if (!Shapes.Any() || isUpdating) return;

            isUpdating = true;
            Shapes.GetDimensions(out minX, out maxX, out minY, out maxY);
            
            Shapes.PositionShapes(maxY);
            SetWindowSize?.Invoke();
            SetShapesPixelValues();

            isUpdating = false;
        }

        private void UnsubscribeShapePropertyChanged()
        {
            foreach (var shape in Shapes)
            {
                ((INotifyPropertyChanged)shape).PropertyChanged -= OnShapePropertyChanged;
            }
        }

        private void SubscribeShapePropertyChanged()
        {
            foreach (var shape in Shapes)
            {
                ((INotifyPropertyChanged)shape).PropertyChanged += OnShapePropertyChanged;
            }
        }

        private void CompartmentsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var compartment = e.GetRemovedOrAddedItem() as Compartment;
                if (compartment != null)
                {
                    Shapes.Add(new CompartmentShape { Compartment = compartment });
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var compartment = e.GetRemovedOrAddedItem() as Compartment;
                if (compartment != null)
                {
                    var compartmentShapeToRemove = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == compartment);
                    // Set new compartment shape in pipe when a pipe is connected to the compartment to remove
                    var pipeShapes = PipeShapes.Where(ps => ps.ConnectedCompartmentShape == compartmentShapeToRemove);
                    foreach (var pipeShape in pipeShapes)
                    {
                        var compartmentInManhole = (Manhole)pipeShape.Pipe.Source == manhole
                            ? pipeShape.Pipe.SourceCompartment
                            : pipeShape.Pipe.TargetCompartment;

                        pipeShape.ConnectedCompartmentShape = CompartmentShapes.FirstOrDefault(cs => cs.Compartment == compartmentInManhole);
                    }
                    Shapes.Remove(compartmentShapeToRemove);
                }
            }
        }

        private void Branches_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var sewerConnection = e.GetRemovedOrAddedItem() as SewerConnection;
                if (sewerConnection == null) return;

                var newConnection = sewerConnection.CreateStructureShape();
                if (newConnection == null) return;
                newConnection.Width = 0.5;

                Shapes.Add(newConnection);
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var sewerConnection = e.GetRemovedOrAddedItem() as SewerConnection;
                if (sewerConnection == null) return;

                var connectionShapes = Shapes.OfType<InternalConnectionShape>();
                var connectionToRemove = connectionShapes.FirstOrDefault(cs => (SewerConnection) cs.SewerConnection == sewerConnection);

                Shapes.Remove(connectionToRemove);
            }
        }

        private void ShapesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var list = sender as IList;
            var drawingShapes = list as List<IDrawingShape> ?? new List<IDrawingShape>();

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item == null) continue;
                    ((INotifyPropertyChanged)item).PropertyChanged += OnShapePropertyChanged;

                    if (isCreating) continue;
                    var indexAddedItem = e.NewStartingIndex;
                    var addedItem = item as IDrawingShape;
                    UpdateSewerConnectionsAfterAdd(drawingShapes, addedItem, indexAddedItem);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    if (item == null) continue;
                    ((INotifyPropertyChanged)item).PropertyChanged -= OnShapePropertyChanged;

                    if (isCreating) continue;
                    var indexRemovedItem = e.OldStartingIndex;
                    var removedItem = item as IDrawingShape;
                    UpdateSewerConnectionsAfterRemove(drawingShapes, removedItem, indexRemovedItem);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                foreach (var item in e.NewItems)
                {
                    if (item == null) continue;
                    if (isCreating) continue;
                    var indexOld = e.OldStartingIndex;
                    var indexNew = e.NewStartingIndex;
                    UpdateSewerConnectionsAfterMove(drawingShapes, item as IDrawingShape, indexOld, indexNew);
                }
            }

            if (isCreating) return;

            UpdateShapeDimensions();
        }

        private static void UpdateSewerConnectionsAfterMove(List<IDrawingShape> items, IDrawingShape movedItem, int indexOld, int indexNew)
        {
            UpdateSewerConnectionsAfterRemove(items, movedItem, indexOld);
            UpdateSewerConnectionsAfterAdd(items, movedItem, indexNew);
        }

        private static void UpdateSewerConnectionsAfterRemove(List<IDrawingShape> items, IDrawingShape removedItem, int indexRemovedItem)
        {
            var leftCandidate = LeftCandidate(items, indexRemovedItem - 1);
            var rightCandidate = RightCandidate(items, indexRemovedItem);

            DisconnectShapes(leftCandidate, removedItem);
            DisconnectShapes(removedItem, rightCandidate);
            ConnectShapes(leftCandidate, rightCandidate);
        }

        private static void UpdateSewerConnectionsAfterAdd(List<IDrawingShape> items, IDrawingShape addedItem, int indexAddedItem)
        {
            var leftCandidate = LeftCandidate(items, indexAddedItem - 1);
            var rightCandidate = RightCandidate(items, indexAddedItem + 1);

            DisconnectShapes(leftCandidate, rightCandidate);
            ConnectShapes(leftCandidate, addedItem);
            ConnectShapes(addedItem, rightCandidate);
        }

        private static IDrawingShape LeftCandidate(List<IDrawingShape> items, int startIndex)
        {
            for (var i = startIndex; i >= 0; i--)
            {
                if (!items.IndexInRange(i)) continue;
                var item = items[i];
                if (item is PipeShape) continue;
                return item;
            }
            return null;
        }

        private static IDrawingShape RightCandidate(List<IDrawingShape> items, int startIndex)
        {
            for (var i = startIndex; i < items.Count; i++)
            {
                if (!items.IndexInRange(i)) continue;
                var item = items[i];
                if (item is PipeShape) continue;
                return item; 
                
            }
            return null;
        }

        private static void ConnectShapes(IDrawingShape left, IDrawingShape right)
        {
            if (left is CompartmentShape && right is CompartmentShape) return;

            if (left is InternalConnectionShape && right is InternalConnectionShape) return;
            
            if (left is InternalConnectionShape && right is CompartmentShape)
            {
                ((InternalConnectionShape)left).SewerConnection.TargetCompartment = ((CompartmentShape)right).Compartment;
                return;
            }

            if (left is CompartmentShape && right is InternalConnectionShape)
            {
                ((InternalConnectionShape)right).SewerConnection.SourceCompartment = ((CompartmentShape)left).Compartment;
            }
        }

        private static void DisconnectShapes(IDrawingShape left, IDrawingShape right)
        {
            if (left is CompartmentShape && right is CompartmentShape) return;

            if (left is InternalConnectionShape && right is InternalConnectionShape)
            {
                ((InternalConnectionShape)left).SewerConnection.TargetCompartment = null;
                ((InternalConnectionShape)right).SewerConnection.SourceCompartment = null;
                return;
            }

            if (left is InternalConnectionShape && right is CompartmentShape)
            {
                ((InternalConnectionShape)left).SewerConnection.TargetCompartment = null;
                return;
            }

            if (left is CompartmentShape && right is InternalConnectionShape)
            {
                ((InternalConnectionShape)right).SewerConnection.SourceCompartment = null;
            }
        }

        private void OnShapePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateShapeDimensions();
        }
    }
}