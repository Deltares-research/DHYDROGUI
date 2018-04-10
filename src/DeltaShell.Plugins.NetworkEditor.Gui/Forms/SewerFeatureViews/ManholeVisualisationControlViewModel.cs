using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections.Extensions;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    [Entity]
    public class ManholeVisualisationControlViewModel
    {
        private double compartmentIntervalFactor = 0.1;// Factor to define the interval distance between the compartments; interval = factor * average_compartment_width
        private double compartmentIntervalDistance;

        private Manhole manhole;

        public Manhole Manhole
        {
            get { return manhole; }
            set
            {
                manhole = value;

                if (manhole == null) return;

                CreateShapes();
                DetermineGlobalDimensions();
                PositionShapes();
            }
        }

        public ObservableCollection<IDrawingShape> Shapes { get; set; } = new ObservableCollection<IDrawingShape>();

        public double HeightWithMargin { get; set; }

        public double WidthWithMargin { get; set; }

        private double ActualLevelAtTopOfCanvas { get; set; }

        public double StrokeThickness
        {
            get { return DetermineStrokeThickness(); }
            set { }
        }

        private double DetermineStrokeThickness()
        {
            var compartments = Shapes.OfType<CompartmentShape>().ToList();
            if (compartments.Any())
            {
                var height = compartments.Max(c => c.Compartment.SurfaceLevel) -
                             compartments.Min(c => c.Compartment.BottomLevel);

                return height * 0.01;
            }
            return 0.1;
        }

        private void CreateShapes()
        {
            if (manhole == null) return;

            Shapes = new ObservableCollection<IDrawingShape>();

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
                HeightWithMargin = 0;
                WidthWithMargin = 0;
                return;
            }

            // Determine height
            var maxTopLevelOfCompartments = compartmentShapes.Max(c => c.TopLevel);
            var minBottomLevelOfCompartments = compartmentShapes.Min(c => c.BottomLevel);

            var height = maxTopLevelOfCompartments - minBottomLevelOfCompartments;
            var verticalMargin = height * 0.1; // The margin in vertical direction is 10% of the total height
            HeightWithMargin = height + 2 * verticalMargin;
            ActualLevelAtTopOfCanvas = maxTopLevelOfCompartments + verticalMargin; // The level at the top of the canvas (itemscontrol)

            // Determine width
            compartmentIntervalDistance = compartmentShapes.Average(c => c.Width) * compartmentIntervalFactor;
            var width = GetCompartmentsTotalWidth();
            var horizontalMargin = width * 0.1; // The margin at each side in horizontal direction is 10% of the total width (compartment shapes + interval between shapes)
            WidthWithMargin = width + 2 * horizontalMargin;
        }

        private void PositionShapes()
        {
            if (Shapes == null || !Shapes.Any()) return;

            // Compartment shapes
            PositionCompartmentShapes();

            // Pipe shapes
            PositionPipeShapes();

            // Orifice shapes
            PositionOrificeShapes();

            // Pump shapes
            PositionPumpShapes();
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

        private IEnumerable<IDrawingShape> CreateStructureConnections(IEnumerable<ISewerConnection> structureConnections)
        {
            var structureShapes = new List<IDrawingShape>();
            var pumpConnections = structureConnections.Select(s => s).Where(sc => sc.BranchFeatures.OfType<Pump>().Any()).ToList();
            foreach (var pumpConnection in pumpConnections)
            {
                var pumps = pumpConnection.BranchFeatures.OfType<Pump>();
                var pump = pumps.FirstOrDefault();
                if (pump == null) continue;

                var pumpShape = new PumpShape {Pump = pump};

                var sourceCompartment = pumpConnection.SourceCompartment;
                var targetCompartment = pumpConnection.TargetCompartment;
                // Connection must have source and target
                if (sourceCompartment == null || targetCompartment == null) continue;

                var sourceCompartmentShape = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == sourceCompartment);
                var targetCompartmentShape = Shapes.OfType<CompartmentShape>().FirstOrDefault(cs => cs.Compartment == targetCompartment);
                
                pumpShape.SourceCompartmentShape = sourceCompartmentShape;
                pumpShape.TargetCompartmentShape = targetCompartmentShape;

                structureShapes.Add(pumpShape);

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

            var orificeConnections = internalConnections.OfType<SewerConnectionOrifice>();
            connectionShapes.AddRange(CreateOrificeShapes(orificeConnections));

            var structureConnections = internalConnections.Where(c => c.BranchFeatures.Any());
            connectionShapes.AddRange(CreateStructureConnections(structureConnections));
            return connectionShapes;
        }

        private void PositionCompartmentShapes()
        {
            var compartmentShapes = Shapes.OfType<CompartmentShape>().ToList();

            // Side by side positioning of compartments, with an interval between each compartment
            if (!compartmentShapes.Any()) return;

            var totalCompartmentsWidth = GetCompartmentsTotalWidth();
            var canvasWidth = WidthWithMargin;
            var initialLeftOffset = (canvasWidth - totalCompartmentsWidth) / 2.0;

            EquallyDistributeShapesHorizontal(compartmentShapes, compartmentIntervalDistance, initialLeftOffset);

            // Set top levels of shapes
            SetTopOfShapes(compartmentShapes);

        }

        private void PositionOrificeShapes()
        {
            var orificeShapes = Shapes.OfType<OrificeShape>().ToList();
            if (!orificeShapes.Any()) return;

            foreach (var orificeShape in orificeShapes)
            {
                PositionShapeBetweenOtherShapes(orificeShape, orificeShape.SourceCompartmentShape, orificeShape.TargetCompartmentShape);
                SetTopOfShape(orificeShape);
            }
        }

        private void PositionPumpShapes()
        {
            var pumpShapes = Shapes.OfType<PumpShape>().ToList();
            if (!pumpShapes.Any()) return;

            foreach (var pumpShape in pumpShapes)
            {
                PositionShapeBetweenOtherShapes(pumpShape, pumpShape.SourceCompartmentShape, pumpShape.TargetCompartmentShape);
                SetTopOfShape(pumpShape);
            }
        }


        private void SetTopOfShape(IDrawingShape shape)
        {
            if (shape == null) return;
            shape.TopOffset = ActualLevelAtTopOfCanvas - shape.TopLevel;
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
    }
}