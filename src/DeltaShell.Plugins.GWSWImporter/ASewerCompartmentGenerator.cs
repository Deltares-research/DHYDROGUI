using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using log4net;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public abstract class ASewerCompartmentGenerator : IGwswFeatureGenerator<ISewerFeature>
    {
        private static ILog Log = LogManager.GetLogger(typeof(ASewerCompartmentGenerator));

        public abstract ISewerFeature Generate(GwswElement gwswElement);

        protected ISewerFeature CreateCompartment<T>(GwswElement gwswElement) 
            where T : Compartment, new()
        {
            if (gwswElement == null) return null;

            var compartmentToBeAdded = CreateNewCompartment<T>(gwswElement);
            SetCompartmentProperties(compartmentToBeAdded, gwswElement);

            return compartmentToBeAdded;
        }

        private T CreateNewCompartment<T>(GwswElement gwswElement) where T : Compartment, new()
        {
            var compartmentIdAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.UniqueId);
            var compartmentName = compartmentIdAttribute.GetValidStringValue();
            if (compartmentName == null)
            {
                Log.WarnFormat(Resources.SewerCompartmentGenerator_FindOrGetNewCompartment__0__in_line__1__does_not_have_a_name_and_will_be_added_to_the_network_with_a_unique_name, "Compartment", gwswElement.GetElementLine());
            }

            return new T { Name = compartmentName };
        }

        protected abstract void SetCompartmentProperties(Compartment compartment, GwswElement gwswElement);

        protected void SetBaseCompartmentProperties(Compartment compartment, GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswCompartment()) return;

            GwswAttribute manholeIdAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);
            compartment.ParentManholeName = manholeIdAttribute.GetValidStringValue();

            SetGeometry(compartment, gwswElement);
            SetNodeShape(compartment, gwswElement);
            SetNodeWidth(compartment, gwswElement);
            SetNodeLength(compartment, gwswElement); // required if NodeShape is 'Rectangular'
            SetBottomLevel(compartment, gwswElement);
            SetSurfaceLevel(compartment, gwswElement);
            SetCompartmentStorageType(compartment, gwswElement);
            SetFloodableArea(compartment, gwswElement); // required if CompartmentStorageType is 'RES'
        }

        private static void SetNodeShape(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute nodeShapeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeShape);
            if (nodeShapeAttribute.IsValidAttribute())
            {
                compartment.Shape = CompartmentShapeConverter.ConvertStringToCompartmentShape(nodeShapeAttribute.GetValidStringValue());
            }
        }

        private static void SetCompartmentStorageType(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute compartmentStorageTypeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.CompartmentStorageType);
            var compartmentStorageType = compartmentStorageTypeAttribute?.GetValueFromDescription<ManholeMapping.GwswCompartmentStorageType>();
            switch (compartmentStorageType)
            {
                case ManholeMapping.GwswCompartmentStorageType.Reservoir:
                    compartment.CompartmentStorageType = CompartmentStorageType.Reservoir;
                    break;
                case ManholeMapping.GwswCompartmentStorageType.Closed:
                    compartment.FloodableArea = 0;
                    compartment.CompartmentStorageType = CompartmentStorageType.Closed;
                    break;
                case ManholeMapping.GwswCompartmentStorageType.Loss:
                    Log.WarnFormat($"Compartment {compartment.Name} has an unsupported compartment storage type 'VRL'. " +
                                   $"Setting the default compartment storage type 'RES' instead.");
                    compartment.CompartmentStorageType = CompartmentStorageType.Reservoir;
                    break;
                default:
                    Log.WarnFormat($"Compartment {compartment.Name} has an unsupported compartment storage type. Setting default 'Reservoir'.");
                    compartment.CompartmentStorageType = CompartmentStorageType.Reservoir;
                    break;
            }
        }

        private static void SetSurfaceLevel(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute surfaceLevelAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.SurfaceLevel);
            if (surfaceLevelAttribute.TryGetValueAsDouble(out double auxDouble))
            {
                compartment.SurfaceLevel = auxDouble;
            }
            else
            {
                Log.WarnFormat($"Missing surface level value for '{compartment.Name}', using default value: {compartment.SurfaceLevel}");
            }
        }

        private static void SetBottomLevel(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute bottomLevelAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.BottomLevel);
            if (bottomLevelAttribute.TryGetValueAsDouble(out double auxDouble))
            {
                compartment.BottomLevel = auxDouble;
            }
            else
            {
                Log.WarnFormat($"Missing bottom level value for '{compartment.Name}', using default value: {compartment.BottomLevel}");
            }
        }

        private static void SetFloodableArea(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute floodableAreaAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.FloodableArea);
            if (floodableAreaAttribute.TryGetValueAsDouble(out double auxDouble))
            {
                compartment.FloodableArea = auxDouble;
            }
            else
            {
                if (compartment.CompartmentStorageType == CompartmentStorageType.Reservoir)
                {
                    compartment.FloodableArea = 100;
                    Log.WarnFormat($"Missing floodable area value for '{compartment.Name}', using default value: {compartment.FloodableArea}");
                }
            }
        }

        private static void SetNodeWidth(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute nodeWidthAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeWidth);
            if (nodeWidthAttribute.TryGetValueAsDouble(out double auxDouble))
            {
                compartment.ManholeWidth = auxDouble / 1000.0; // Conversion from mm to m
            }
            else
            {
                compartment.ManholeWidth = 0.8d;
                Log.WarnFormat($"Missing width value for '{compartment.Name}', using default value: {compartment.ManholeWidth}");
            }
        }

        private static void SetNodeLength(ICompartment compartment, GwswElement gwswElement)
        {
            GwswAttribute nodeLengthAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeLength);
            if (nodeLengthAttribute.TryGetValueAsDouble(out double auxDouble))
            {
                compartment.ManholeLength = auxDouble / 1000.0; // Conversion from mm to m
            }
            else
            {
                compartment.ManholeLength = 0.8d;
                if (compartment.Shape == CompartmentShape.Rectangular)
                {
                    Log.WarnFormat($"Missing length value for '{compartment.Name}', using default value: {compartment.ManholeLength}");
                }
            }
        }

        private static void SetGeometry(ICompartment compartment, GwswElement gwswElement)
        {
            double xCoordinate;
            double yCoordinate;
            
            var xCoordinateAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.XCoordinate);
            var yCoordinateAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.YCoordinate);
            
            if (!xCoordinateAttribute.TryGetValueAsDouble(out xCoordinate))
            {
                Log.WarnFormat($"Missing xCoordinate value for compartment '{compartment.Name}', using default value: 0");
            }

            if (!yCoordinateAttribute.TryGetValueAsDouble(out yCoordinate))
            {
                Log.WarnFormat($"Missing yCoordinate value for compartment '{compartment.Name}', using default value: 0");
            }

            compartment.Geometry = new Point(xCoordinate, yCoordinate);
        }
    }
}