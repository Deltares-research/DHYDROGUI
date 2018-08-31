using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.SewerFeatureViews
{
    public static class DrawingShapesCreationHelper
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
            var connectionShapes = shapes.OfType<InternalConnectionShape>().ToList();
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

                var connectionShape = lastAdded as InternalConnectionShape;
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

        private static IEnumerable<CompartmentShape> CreateCompartmentShapes(Manhole manhole)
        {
            var compartments = new List<CompartmentShape>();
            if (manhole == null) return compartments;

            compartments.AddRange(manhole.Compartments.Select(compartment => new CompartmentShape { Compartment = compartment }));
            return compartments;
        }

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
}