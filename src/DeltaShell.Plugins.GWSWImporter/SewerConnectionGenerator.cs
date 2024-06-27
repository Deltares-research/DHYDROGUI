using DelftTools.Hydro.SewerFeatures;
using Deltares.Infrastructure.API.Logging;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public class SewerConnectionGenerator : ASewerGenerator, IGwswFeatureGenerator<ISewerFeature>
    {
        public SewerConnectionGenerator(ILogHandler logHandler) : base(logHandler)
        {
        }
        public virtual ISewerFeature Generate(GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswSewerConnection(logHandler)) return null;
            return CreateSewerConnection<SewerConnection>(gwswElement);
        }

        protected T CreateSewerConnection<T>(GwswElement gwswElement) where T : SewerConnection, new()
        {
            if (gwswElement == null) return null;

            //Now we are free to create the connection.
            var connection = CreateNewConnection<T>(gwswElement);
            SetSewerConnectionAttributes(connection, gwswElement);
            SetSewerConnectionDefaultGeometry(connection);

            return connection;
        }

        private T CreateNewConnection<T>(GwswElement gwswElement) where T : SewerConnection, new()
        {
            var sewerConnectionIdAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId, logHandler);
            var connectionName = string.Empty;
            if (sewerConnectionIdAttribute.IsValidAttribute(logHandler))
            {
                connectionName = sewerConnectionIdAttribute.ValueAsString;
            }
            
            return new T { Name = connectionName };
        }

        protected virtual void SetSewerConnectionAttributes(ISewerConnection sewerConnection, GwswElement gwswElement)
        {
            if(!gwswElement.IsValidGwswSewerConnection(logHandler)) return;
            
            sewerConnection.SourceCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.SourceCompartmentId, logHandler, logMessage:$"Cannot find and load source compartment name for sewer connection: {sewerConnection.Name}");
            sewerConnection.TargetCompartmentName = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.TargetCompartmentId, logHandler, logMessage: $"Cannot find and load target compartment name for sewer connection: {sewerConnection.Name}");

            double auxDouble;

            var levelStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelStart, logHandler);
            if( levelStart.TryGetValueAsDouble(logHandler, out auxDouble))
                sewerConnection.LevelSource = auxDouble;    

            var levelEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelEnd, logHandler);
            if( levelEnd.TryGetValueAsDouble(logHandler, out auxDouble))
                sewerConnection.LevelTarget = auxDouble;

            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length, logHandler);
            if (length.TryGetValueAsDouble(logHandler, out auxDouble))
            {
                sewerConnection.IsLengthCustom = true;
                sewerConnection.Length = auxDouble;
            }

            var waterTypeString = gwswElement.GetAttributeValueFromList<string>(SewerConnectionMapping.PropertyKeys.WaterType, logHandler);
            if (waterTypeString != null)
            {
                sewerConnection.WaterType = WaterTypeConverter.ConvertStringToSewerConnectionWaterType(waterTypeString, logHandler);
            }
        }

        private static void SetSewerConnectionDefaultGeometry(ISewerConnection sewerConnection)
        {
            if (sewerConnection.Geometry?.Coordinate != null) return;

            var manholeSource = sewerConnection.Source;
            var manholeTarget = sewerConnection.Target;

            if (manholeSource == null || manholeTarget == null) return;
            Point defaultPoint = new Point(0,0);

            if (manholeSource.Geometry?.Coordinate == null)
                manholeSource.Geometry = defaultPoint;
            if (manholeTarget.Geometry?.Coordinate == null)
                manholeTarget.Geometry = defaultPoint;

            sewerConnection.Geometry = new LineString(
                new[]
                {
                    manholeSource.Geometry.Coordinate,
                    manholeTarget.Geometry.Coordinate
                });
        }
    }
}