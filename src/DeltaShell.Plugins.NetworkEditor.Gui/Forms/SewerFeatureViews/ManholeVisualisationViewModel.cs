using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using DelftTools.Hydro;
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

        public double HeightWidthRatio {get { return (maxY - minY) / (maxX - minX); } }

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

        private void UpdateShapeDimensions()
        {
            if (!Shapes.Any() || isUpdating) return;

            isUpdating = true;
            var dim = Shapes.GetDimensionWithMargin(0); // TODO Remove margin
            minX = dim.MinX;
            maxX = dim.MaxX;
            minY = dim.MinY;
            maxY = dim.MaxY;

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

        private void CompartmentsOnCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                var compartment = e.Item as Compartment;
                if (compartment != null)
                {
                    Shapes.Add(new CompartmentShape { Compartment = compartment });
                }

            }

            if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                var compartment = e.Item as Compartment;
                if (compartment != null)
                {
                    var compartmentShapeToRemove = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == compartment);
                    Shapes.Remove(compartmentShapeToRemove);
                }
            }
        }

        private void Branches_CollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                var sewerConnection = e.Item as SewerConnection;
                if (sewerConnection == null) return;

                var newConnection = sewerConnection.CreateStructureShape();
                if (newConnection == null) return;
                newConnection.Width = 0.5;

                Shapes.Add(newConnection);
                Shapes = new ObservableCollection<IDrawingShape>(Shapes.OrderShapes());
            }

            if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                var sewerConnection = e.Item as SewerConnection;
                if (sewerConnection == null) return;

                var connectionShapes = Shapes.OfType<ConnectionShape>();
                var connectionToRemove = connectionShapes.FirstOrDefault(cs => (SewerConnection) cs.SewerConnection == sewerConnection);

                Shapes.Remove(connectionToRemove);
            }
        }

        private void ShapesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    if (item == null) continue;
                    ((INotifyPropertyChanged)item).PropertyChanged += OnShapePropertyChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    if (item == null) continue;
                    ((INotifyPropertyChanged)item).PropertyChanged -= OnShapePropertyChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                // Update all connections
                var items = (sender as IList<IDrawingShape>)?.Where(ids => !(ids is PipeShape)).ToList();
                if (items != null && items.Any())
                {
                    var connections = items.OfType<ConnectionShape>().ToList();

                    foreach (var connection in connections)
                    {
                        var connectionIndex = items.IndexOf(connection);
                        var sewerConnection = connection.SewerConnection;

                        var sourceItem = connectionIndex > 0 ? items[connectionIndex - 1] : null;
                        var targetItem = connectionIndex < items.Count - 1 ? items[connectionIndex + 1] : null;

                        sewerConnection.SourceCompartment = (sourceItem as CompartmentShape)?.Compartment;
                        sewerConnection.TargetCompartment = (targetItem as CompartmentShape)?.Compartment;
                    }
                }
            }

            if (isCreating) return;

            UpdateShapeDimensions();
        }

        private void OnShapePropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            UpdateShapeDimensions();
        }
        
        public Func<double> ContainerWidth { get; set; }
        
        public Func<double> ContainerHeight { get; set; }

        public Action SetWindowSize { get; set; }

        public void SetShapesPixelValues()
        {
            foreach (var shape in Shapes)
            {
                shape.SetPixelValues(minX, maxX, minY, maxY, ContainerWidth.Invoke(), ContainerHeight.Invoke());
            }
        }
    }

    public static class CreateShapesHelper
    {
        public static IEnumerable<IDrawingShape> CreateShapes(this Manhole manhole)
        {
            var shapes = new List<IDrawingShape>();
            if (manhole == null) return shapes;

            // Compartment shapes
            shapes.AddRange(CreateCompartmentShapes(manhole));

            // Pipe shapes
            shapes.AddRange(CreatePipeShapes(manhole, shapes));

            // Connection shapes
            shapes.AddRange(CreateConnectionShapes(manhole, shapes));
            return shapes;
        }

        public static IEnumerable<IDrawingShape> OrderShapes(this IList<IDrawingShape> shapes)
        {
            // TODO this can be done in a more elegant way.
            var connectionShapes = shapes.OfType<ConnectionShape>().ToList();
            var compartmentShapes = shapes.OfType<CompartmentShape>().ToList();

            if (compartmentShapes.Count < 2) return shapes;

            var reorderedList = new List<IDrawingShape>();

            CompartmentShape firtCompartmentShape = null;
            CompartmentShape lastCompartmentShape = null;

            foreach (var compartmentShape in compartmentShapes)
            {
                var isFirstCompartment = connectionShapes.All(cs => cs.SewerConnection.TargetCompartment != compartmentShape.Compartment);
                if (isFirstCompartment)
                {
                    firtCompartmentShape = compartmentShape;
                    continue;
                }

                var isLastCompartment = connectionShapes.All(cs => cs.SewerConnection.SourceCompartment != compartmentShape.Compartment);
                if (isLastCompartment)
                {
                    lastCompartmentShape = compartmentShape;
                    continue;
                }
            }

            reorderedList.Add(firtCompartmentShape);

            IDrawingShape lastAdded = firtCompartmentShape;
            var counter = 0;
            while (!reorderedList.Contains(lastCompartmentShape) && counter <= connectionShapes.Count + compartmentShapes.Count)
            {
                counter++;

                var compartmentShape = lastAdded as CompartmentShape;
                if (compartmentShape != null)
                {
                    // Now add a connection
                    var connection = connectionShapes.FirstOrDefault(cs => cs.SewerConnection.SourceCompartment == compartmentShape.Compartment);
                    if (connection != null)
                    {
                        reorderedList.Add(connection);
                        lastAdded = connection;
                    }

                    continue;
                }

                var connectionShape = lastAdded as ConnectionShape;
                if (connectionShape != null)
                {
                    // Now add a compartment
                    var compartment = compartmentShapes.FirstOrDefault(cs => connectionShape.SewerConnection.TargetCompartment == cs.Compartment);
                    if (compartment != null)
                    {
                        reorderedList.Add(compartment);
                        lastAdded = compartment;
                    }
                }
            }

            // Add all shapes which are not in the reorderedList yet
            reorderedList.AddRange(shapes.Where(s => !reorderedList.Contains(s)));
            return reorderedList;
        }

        // Create
        private static IEnumerable<CompartmentShape> CreateCompartmentShapes(Manhole manhole)
        {
            var compartments = new List<CompartmentShape>();
            if (manhole == null) return compartments;

            compartments.AddRange(manhole.Compartments.Select(compartment => new CompartmentShape { Compartment = compartment }));
            return compartments;
        }

        // Create
        private static IEnumerable<IDrawingShape> CreateConnectionShapes(Manhole manhole, ICollection<IDrawingShape> shapes)
        {
            var connectionShapes = new List<IDrawingShape>();
            if (manhole == null) return connectionShapes;

            var internalConnections = manhole.InternalConnections().ToList();

            connectionShapes.AddRange(CreateStructureShapes(internalConnections));

            // get width based on current compartments
            const double connectionWidth = 0.5; // compartments.Sum(cs => cs.Width) * 0.1;

            // Set width for each connection
            connectionShapes.ForEach(cs => cs.Width = connectionWidth);

            return connectionShapes;
        }

        // Create
        private static IEnumerable<IDrawingShape> CreatePipeShapes(Manhole manhole, ICollection<IDrawingShape> shapes)
        {
            var network = manhole.Network as IHydroNetwork;
            if (network == null) return new List<IDrawingShape>();

            var pipes = manhole.Pipes();
            var pipeShapes = pipes.Select(pipe => pipe.CreatePipeShape()).ToList();

            foreach (var pipeShape in pipeShapes)
            {
                var connectedCompartmentShape = FindConnectedCompartmentShape(pipeShape, manhole, shapes);
                if (connectedCompartmentShape == null) continue;

                pipeShape.ConnectedCompartmentShape = connectedCompartmentShape;
            }

            return pipeShapes;
        }

        private static PipeShape CreatePipeShape(this IPipe pipe)
        {
            return new PipeShape { Pipe = pipe };
        }

        // Create
        private static IEnumerable<IDrawingShape> CreateStructureShapes(IEnumerable<ISewerConnection> structureConnections)
        {
            var structureShapes = structureConnections.Select(sc => sc.CreateStructureShape()).Where(shape => shape != null);

            return structureShapes;
        }

        public static IDrawingShape CreateStructureShape(this ISewerConnection connection)
        {
            var orifice = connection.GetStructuresFromBranchFeatures<Orifice>().FirstOrDefault();
            if (orifice != null)
            {
                return connection.CreateOrificeShape(orifice);
            }

            var pump = connection.GetStructuresFromBranchFeatures<Pump>().FirstOrDefault();
            if (pump != null)
            {
                return connection.CreatePumpShape(pump);
            }

            var weir = connection.GetStructuresFromBranchFeatures<Weir>().FirstOrDefault();
            if (weir != null)
            {
                return connection.CreateWeirShape(weir);
            }

            return null;
        }

        private static PumpShape CreatePumpShape(this ISewerConnection connection, Pump pump)
        {
            return new PumpShape
            {
                Pump = pump,
                SewerConnection = connection,
            };
        }

        private static WeirShape CreateWeirShape(this ISewerConnection connection, Weir weir)
        {
            return new WeirShape
            {
                Weir = weir,
                SewerConnection = connection,
            };
        }

        private static OrificeShape CreateOrificeShape(this ISewerConnection connection, Orifice orifice)
        {
            return new OrificeShape
            {
                Orifice = orifice,
                SewerConnection = connection,
            };
        }

        /// <summary>
        /// Finds the compartment shape to which the pipe is connected. Returns null if it is an internal pipe or if no match can be found
        /// </summary>
        /// <param name="pipeShape"></param>
        /// <param name="manhole"></param>
        /// <param name="shapes"></param>
        /// <returns></returns>
        private static CompartmentShape FindConnectedCompartmentShape(PipeShape pipeShape, Manhole manhole, ICollection<IDrawingShape> shapes)
        {
            // Find connected compartment in current manhole
            var sourceCompartment = manhole.Compartments.FirstOrDefault(c => c == pipeShape.Pipe.SourceCompartment);
            var targetCompartment = manhole.Compartments.FirstOrDefault(c => c == pipeShape.Pipe.TargetCompartment);

            if (sourceCompartment == null && targetCompartment == null)
            {
                // Can't find a matching compartment in this manhole
                return null;
            }

            // Are source and/or target 
            if (sourceCompartment != null && targetCompartment != null)
            {
                // pipe is an internal pipe between compartments. What to do with this? Skip for now.
                return null;
            }

            var connectedCompartment = sourceCompartment ?? targetCompartment;
            var connectedCompartmentShape = shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == connectedCompartment);
            return connectedCompartmentShape;
        }
    }

    public static class PositionHelper
    {
        // Helper
        public static void PositionShapes(this ICollection<IDrawingShape> shapes, double maxY)
        {
            if (shapes == null || !shapes.Any()) return;

            // Position side by side shapes
            var compartments = shapes.Where(s => s is CompartmentShape || s is ConnectionShape).ToList();

            PositionShapesSideBySide(compartments, maxY);

            // Pipe shapes
            PositionPipeShapes(shapes.OfType<PipeShape>(), maxY);
        }

        private static void PositionShapesSideBySide(IList<IDrawingShape> shapes, double maxY)
        {
            EquallyDistributeShapesHorizontal(shapes, 0, 0);

            // Set top levels of shapes
            SetTopOfShapes(shapes, maxY);
        }

        // Helper - Position
        private static void SetTopOfShape(IDrawingShape shape, double maxY)
        {
            if (shape == null) return;
            shape.TopOffset = maxY - shape.TopLevel;
        }

        // Helper - position
        private static void SetTopOfShapes(IEnumerable<IDrawingShape> shapes, double maxY)
        {
            foreach (var shape in shapes)
            {
                SetTopOfShape(shape, maxY);
            }
        }

        // Helper - position
        private static void EquallyDistributeShapesHorizontal(IEnumerable<IDrawingShape> shapes, double distanceInterval, double initialLeftOffset)
        {
            var left = initialLeftOffset;
            foreach (var shape in shapes)
            {
                shape.LeftOffset = left;
                left += shape.Width + distanceInterval;
            }
        }

        // Helper
        private static void PositionPipeShapes(IEnumerable<PipeShape> shapes, double maxY)
        {
            var pipeShapes = shapes.ToList();
            if (!pipeShapes.Any()) return;

            // Group all pipe shapes by compartment shape
            var groupedPipeShapes = pipeShapes.GroupBy(p => p.ConnectedCompartmentShape?.Compartment);
            // loop over groups -> 
            foreach (var group in groupedPipeShapes)
            {
                var items = group.ToList();

                if (!items.Any()) continue;

                if (items.Count == 1)
                {
                    // Only one pipe shape present in this compartment -> set left and top offset based on compartment shape
                    var shape = items.FirstOrDefault();
                    if (shape == null) continue;

                    SetTopOfShape(shape, maxY);
                    SetShapeLeftOffsetRelativeTo(shape, shape.ConnectedCompartmentShape, 0.5);
                    continue;
                }

                // More than 1 item -> Find overlap and divide overlapping items. Non overlapping items can be given a location done as above.
                // TODO Implement method to give overlapping shapes different positions
                foreach (var shape in items)
                {
                    SetTopOfShape(shape, maxY);
                    SetShapeLeftOffsetRelativeTo(shape, shape.ConnectedCompartmentShape, 0.5);
                }
            }
        }

        /// <summary>
        /// Sets a shape horizontal position relative to another shape. 
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="referenceShape">Factor to determine the offset, 0) shapes collide at the left edge, 1) at right edge, 0.5) in the middle</param>
        /// <param name="offsetFactor"></param>
        private static void SetShapeLeftOffsetRelativeTo(IDrawingShape shape, IDrawingShape referenceShape, double offsetFactor)
        {
            var leftOffset = referenceShape.LeftOffset + referenceShape.Width * offsetFactor - shape.Width * 0.5;
            shape.LeftOffset = leftOffset;
        }
    }

    public struct Dimension
    {
        public double MaxY { get; set; }
        public double MinY { get; set; }
        public double MaxX { get; set; }
        public double MinX { get; set; }

        public double Height { get { return MaxY - MinY; } }
        public double Width { get { return MaxX - MinX; } }
    }

    public static class DimensionsHelper
    {
        public static Dimension GetDimensionWithMargin(this IEnumerable<IDrawingShape> shapes, double margin)
        {
            var dim = GetDimensions(shapes);

            var height = dim.Height;
            var width = dim.Width;

            var dimensionWithMargin = new Dimension
            {
                MaxY = dim.MaxY + margin * height,
                MinY = dim.MinY - margin * height,
                MaxX = dim.MaxX + margin * width,
                MinX = dim.MinX - margin * width,

            };
            return dimensionWithMargin;
        }

        private static Dimension GetDimensions(IEnumerable<IDrawingShape> shapes)
        {
            double minX = 0;
            double maxX = 0;

            double minY = 0;
            double maxY = 0;

            HeightDimensionsFromShapes(shapes, out minY, out maxY);
            WidthDimensionsFromShapes(shapes, out minX, out maxX);

            var dim = new Dimension
            {
                MinX = minX,
                MaxX = maxX,
                MinY = minY,
                MaxY = maxY,
            };

            return dim;
        }

        private static double HeightDimensionsFromShapes(IEnumerable<IDrawingShape> shapes, out double minY, out double maxY)
        {
            var drawingShapes = shapes.ToList();
            var max = drawingShapes.Max(s => s.TopLevel);
            var min = drawingShapes.Min(s => s.BottomLevel);

            var height = max - min;


            minY = min;
            maxY = max;
            return height;
        }

        private static double WidthDimensionsFromShapes(IEnumerable<IDrawingShape> shapes, out double minX, out double maxX)
        {
            // Get width of compartments and ISC, maar niet pipes
            var drawingShapes = shapes.ToList();
            var pipeShapes = drawingShapes.OfType<PipeShape>();
            var otehr = drawingShapes.Except(pipeShapes);

            var width = otehr.Sum(s => s.Width);

            minX = 0;
            maxX = width;

            return width;
        }
    }
}