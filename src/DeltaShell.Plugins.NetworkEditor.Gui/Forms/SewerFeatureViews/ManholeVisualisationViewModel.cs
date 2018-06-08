using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private double compartmentIntervalFactor = 0.1;// Factor to define the interval distance between the compartments; interval = factor * average_compartment_width
        private double compartmentIntervalDistance;

        private double minX;
        private double maxX;
        private double minY;
        private double maxY;
        private double widthWithMargin;

        public double HeigthWidthRatio => (maxY - minY) / (maxX - minX);

        private Manhole manhole;

        public Manhole Manhole
        {
            get { return manhole; }
            set
            {
                if (manhole != null)
                {
                    // unsubscribe
                    manhole.Compartments.CollectionChanged -= CompartmentsOnCollectionChanged;
                    UnsubscribeCompartmentPropertyChanged();
                }

                manhole = value;

                if (manhole != null)
                {
                    // subscribe 
                    manhole.Compartments.CollectionChanged += CompartmentsOnCollectionChanged;

                    SubscribeCompartmentPropertyChanged();

                    Update();
                }
            }
        }

        private void UnsubscribeCompartmentPropertyChanged()
        {
            foreach (var compartment in manhole.Compartments)
            {
                ((INotifyPropertyChanged)compartment).PropertyChanged += CompartmentPropertyChanged;
            }
        }

        private void SubscribeCompartmentPropertyChanged()
        {
            foreach (var compartment in manhole.Compartments)
            {
                ((INotifyPropertyChanged) compartment).PropertyChanged += CompartmentPropertyChanged;
            }
        }

        private void CompartmentPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        private void Update()
        {
            CreateShapes();
            DetermineGlobalDimensions();
            PositionShapes();
            UpdateShapePositions();
            SetWindowSize?.Invoke();
        }

        private void CompartmentsOnCollectionChanged(object sender, NotifyCollectionChangingEventArgs e)
        {
            if (e.Action == NotifyCollectionChangeAction.Add)
            {
                ((INotifyPropertyChanged) e.Item).PropertyChanged += CompartmentPropertyChanged;
            }

            if (e.Action == NotifyCollectionChangeAction.Remove)
            {
                ((INotifyPropertyChanged) e.Item).PropertyChanged -= CompartmentPropertyChanged;
            }

            Update();
        }


        public ObservableCollection<IDrawingShape> Shapes { get; set; } = new ObservableCollection<IDrawingShape>();
        
        public Func<double> ContainerWidth { get; set; }

        public Func<double> ContainerHeight { get; set; }

        public Action SetWindowSize { get; set; }
        
        private void CreateShapes()
        {
            if (manhole == null) return;

            Shapes.Clear();

            // Compartment shapes
            Shapes.AddRange(CreateCompartmentShapes());

            // Pipe shapes
            Shapes.AddRange(CreatePipeShapes());

            // Connection shapes
            Shapes.AddRange(CreateConnectionShapes());
        }

        private void DetermineGlobalDimensions()
        {
            // width is globally defined by the width of the compartments 
            var compartmentShapes = Shapes?.OfType<CompartmentShape>().ToList();

            if (compartmentShapes == null || !compartmentShapes.Any())
            {
                //heightWithMargin = 0;
                widthWithMargin = 0;
                return;
            }

            DetermineHeight();

            DetermineWidth(compartmentShapes);
        }

        private void DetermineWidth(List<CompartmentShape> compartmentShapes)
        {
            compartmentIntervalDistance = compartmentShapes.Average(c => c.Width) * compartmentIntervalFactor;
            var width = GetCompartmentsTotalWidth();
            var horizontalMargin = width * 0.1; // The margin at each side in horizontal direction is 10% of the total width (compartment shapes + interval between shapes)
            widthWithMargin = width + 2 * horizontalMargin;
            minX = 0;
            maxX = widthWithMargin;
        }

        private void DetermineHeight()
        {
            if (Shapes == null || !Shapes.Any())
            {
                widthWithMargin = 0;
                return;
            }

            var maxTopLevel = Shapes.Max(c => c.TopLevel);
            var minBottomLevel = Shapes.Min(c => c.BottomLevel);

            var height = maxTopLevel - minBottomLevel;
            var verticalMargin = height * 0.1; // The margin in vertical direction is 10% of the total height
            
            maxY = maxTopLevel + verticalMargin; // The level at the top of the canvas (itemscontrol)
            minY = minBottomLevel - verticalMargin;
        }

        private void PositionShapes()
        {
            if (Shapes == null || !Shapes.Any()) return;

            // Compartment shapes
            PositionCompartmentShapes();

            // Pipe shapes
            PositionPipeShapes();

            // Internal connection shapes
            PositionConnectionShapes();
        }

        private IEnumerable<CompartmentShape> CreateCompartmentShapes()
        {
            var compartments = new List<CompartmentShape>();
            if (manhole == null) return compartments;

            compartments.AddRange(manhole.Compartments.Select(compartment => new CompartmentShape { Compartment = compartment }));
            return compartments;
        }

        private IEnumerable<IDrawingShape> CreatePipeShapes()
        {
            var network = manhole.Network as IHydroNetwork;
            if (network == null) return new List<IDrawingShape>();

            var pipes = manhole.GetPipesConnectedToManhole(network.Pipes);
            var pipeShapes = pipes.Select(pipe => new PipeShape { Pipe = pipe }).ToList();

            foreach (var pipeShape in pipeShapes)
            {
                var connectedCompartmentShape = FindConnectedCompartmentShape(pipeShape);
                if (connectedCompartmentShape == null) continue;

                pipeShape.ConnectedCompartmentShape = connectedCompartmentShape;
            }

            return pipeShapes;
        }

        /// <summary>
        /// Finds the compartment shape to which the pipe is connected. Returns null if it is an internal pipe or if no match can be found
        /// </summary>
        /// <param name="pipeShape"></param>
        /// <returns></returns>
        private CompartmentShape FindConnectedCompartmentShape(PipeShape pipeShape)
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
            var connectedCompartmentShape = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == connectedCompartment);
            return connectedCompartmentShape;
        }

        private void PositionPipeShapes()
        {
            var pipeShapes = Shapes.OfType<PipeShape>().ToList();
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

                    SetTopOfShape(shape);
                    SetShapeLeftOffsetRelativeTo(shape, shape.ConnectedCompartmentShape, 0.5);
                    continue;
                }

                // More than 1 item -> Find overlap and divide overlapping items. Non overlapping items can be given a location done as above.
                // TODO Implement method to give overlapping shapes different positions
                foreach (var shape in items)
                {
                    SetTopOfShape(shape);
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
        private void SetShapeLeftOffsetRelativeTo(IDrawingShape shape, IDrawingShape referenceShape, double offsetFactor)
        {
            var leftOffset = referenceShape.LeftOffset + referenceShape.Width * offsetFactor - shape.Width * 0.5;
            shape.LeftOffset = leftOffset;
        }

        private IEnumerable<IDrawingShape> CreateStructureConnections(IList<ISewerConnection> structureConnections)
        {
            var structureShapes = new List<IDrawingShape>();
            var pumpConnections = structureConnections.Select(s => s).Where(sc => sc.BranchFeatures.OfType<Pump>().Any()).ToList();
            foreach (var pumpConnection in pumpConnections)
            {
                var pumps = pumpConnection.BranchFeatures.OfType<Pump>();

                foreach (var pump in pumps)
                {
                    var pumpShape = new PumpShape { Pump = pump };

                    if (!TrySetShapeSourceAndTargetCompartment(pumpConnection, pumpShape)) continue;

                    structureShapes.Add(pumpShape);
                }
            }

            var weirConnections = structureConnections.Select(s => s).Where(sc => sc.BranchFeatures.OfType<Weir>().Any()).ToList();
            foreach (var weirConnection in weirConnections)
            {
                var weirs = weirConnection.BranchFeatures.OfType<Weir>();
                foreach (var weir in weirs)
                {
                    var weirShape = new WeirShape { Weir = weir };
                    if (!TrySetShapeSourceAndTargetCompartment(weirConnection, weirShape)) continue;

                    structureShapes.Add(weirShape);
                }
            }

            return structureShapes;
        }


        private IEnumerable<IDrawingShape> CreateOrificeShapes(IEnumerable<SewerConnectionOrifice> orifices)
        {
            var orificeShapes = new List<OrificeShape>();
            foreach (var orifice in orifices)
            {
                var orificeShape = new OrificeShape { Orifice = orifice };
                orificeShapes.Add(orificeShape);

                // Find compartment
                var sourceCompartment = manhole.Compartments.FirstOrDefault(c => c == orificeShape.Orifice.SourceCompartment);
                var targetCompartment = manhole.Compartments.FirstOrDefault(c => c == orificeShape.Orifice.TargetCompartment);

                // Connection must have source and target
                if (sourceCompartment == null || targetCompartment == null) continue;

                var sourceCompartmentShape = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == sourceCompartment);
                var targetCompartmentShape = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == targetCompartment);

                orificeShape.SourceCompartmentShape = sourceCompartmentShape;
                orificeShape.TargetCompartmentShape = targetCompartmentShape;
            }

            return orificeShapes;
        }

        private IEnumerable<IDrawingShape> CreateConnectionShapes()
        {
            var connectionShapes = new List<IDrawingShape>();
            if (manhole == null) return connectionShapes;

            var internalConnections = manhole.GetManholeInternalConnections().ToList();

            var orificeConnections = internalConnections.OfType<SewerConnectionOrifice>().Where(c => c.SourceCompartment != null && c.TargetCompartment != null);
            connectionShapes.AddRange(CreateOrificeShapes(orificeConnections));

            var structureConnections = internalConnections.Where(c => c.BranchFeatures.Any());
            connectionShapes.AddRange(CreateStructureConnections(structureConnections.ToList()));
            return connectionShapes;
        }

        private void PositionCompartmentShapes()
        {
            var compartmentShapes = Shapes.OfType<CompartmentShape>().ToList();

            // Side by side positioning of compartments, with an interval between each compartment
            if (!compartmentShapes.Any()) return;

            var totalCompartmentsWidth = GetCompartmentsTotalWidth();
            var initialLeftOffset = (widthWithMargin - totalCompartmentsWidth) / 2.0;

            EquallyDistributeShapesHorizontal(compartmentShapes, compartmentIntervalDistance, initialLeftOffset);

            // Set top levels of shapes
            SetTopOfShapes(compartmentShapes);
        }

        private void PositionConnectionShapes()
        {
            var connectionShapes = Shapes.OfType<ConnectionShape>().ToList();
            if (!connectionShapes.Any()) return;

            foreach (var connectionShape in connectionShapes)
            {
                PositionShapeBetweenOtherShapes(connectionShape, connectionShape.SourceCompartmentShape, connectionShape.TargetCompartmentShape);
                SetTopOfShape(connectionShape);
            }
        }

        private void SetTopOfShape(IDrawingShape shape)
        {
            if (shape == null) return;
            shape.TopOffset = maxY - shape.TopLevel;
        }

        private void SetTopOfShapes(IEnumerable<IDrawingShape> shapes)
        {
            foreach (var shape in shapes)
            {
                SetTopOfShape(shape);
            }
        }

        private static void EquallyDistributeShapesHorizontal(IEnumerable<IDrawingShape> shapes, double distanceInterval, double initialLeftOffset)
        {
            var left = initialLeftOffset;
            foreach (var shape in shapes)
            {
                shape.LeftOffset = left;
                left += shape.Width + distanceInterval;
            }
        }

        private static void PositionShapeBetweenOtherShapes(IDrawingShape shape, IDrawingShape referenceShape1, IDrawingShape referenceShape2)
        {
            if (shape == null || referenceShape1 == null || referenceShape2 == null) return;
            var reference1IsLeft = referenceShape1.LeftOffset < referenceShape2.LeftOffset;
            var leftShape = reference1IsLeft ? referenceShape1 : referenceShape2;
            var rightShape = reference1IsLeft ? referenceShape2 : referenceShape1;

            shape.LeftOffset = (rightShape.LeftOffset + leftShape.LeftOffset + leftShape.Width) * 0.5 - 0.5 * shape.Width;
        }

        /// <summary>
        /// Returns the total width of the compartments, placed side by side, with a defined distance between each compartment
        /// </summary>
        /// <returns></returns>
        private double GetCompartmentsTotalWidth()
        {
            var compartmentShapes = Shapes.OfType<CompartmentShape>().ToList();

            if (!compartmentShapes.Any()) return 0;
            return compartmentShapes.Sum(c => c.Width) + (compartmentShapes.Count - 1) * compartmentIntervalDistance;
        }

        /// <summary>
        /// Sets the source and target compartment shapes of a connection shape
        /// </summary>
        /// <param name="pumpConnection"></param>
        /// <param name="pumpShape"></param>
        /// <returns>Returns true if valid, else returns false</returns>
        private bool TrySetShapeSourceAndTargetCompartment(ISewerConnection pumpConnection, ConnectionShape pumpShape)
        {
            var sourceCompartment = pumpConnection.SourceCompartment;
            var targetCompartment = pumpConnection.TargetCompartment;
            // Connection must have source and target
            if (sourceCompartment == null || targetCompartment == null) return false;

            var sourceCompartmentShape = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == sourceCompartment);
            var targetCompartmentShape = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == targetCompartment);

            pumpShape.SourceCompartmentShape = sourceCompartmentShape;
            pumpShape.TargetCompartmentShape = targetCompartmentShape;
            return true;
        }

        public void UpdateShapePositions()
        {
            foreach (var shape in Shapes)
            {
                shape.SetPixelValues(minX, maxX, minY, maxY, ContainerWidth.Invoke(), ContainerHeight.Invoke());
            }
        }
    }
}