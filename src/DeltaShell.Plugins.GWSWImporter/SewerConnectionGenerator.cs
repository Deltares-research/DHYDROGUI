using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Gwsw
{
    public class SewerConnectionGenerator : ISewerFeatureGenerator
    {
        public virtual ISewerFeature Generate(GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswSewerConnection()) return null;
            return CreateSewerConnection<SewerConnection>(gwswElement);
        }

        protected  T CreateSewerConnection<T>(GwswElement gwswElement) where T : SewerConnection, new()
        {
            if (gwswElement == null) return null;

            //Now we are free to create the connection.
            var connection = CreateNewConnection<T>(gwswElement);
            SetSewerConnectionAttributes(connection, gwswElement);
            SetSewerConnectionDefaultGeometry(connection);

            return connection;
        }

        private static T CreateNewConnection<T>(GwswElement gwswElement) where T : SewerConnection, new()
        {
            var sewerConnectionIdAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.UniqueId);
            var connectionName = string.Empty;
            if (sewerConnectionIdAttribute.IsValidAttribute())
            {
                connectionName = sewerConnectionIdAttribute.ValueAsString;
            }
            
            return new T { Name = connectionName };
        }

        protected virtual void SetSewerConnectionAttributes(ISewerConnection sewerConnection, GwswElement gwswElement)
        {
            if(!gwswElement.IsValidGwswSewerConnection()) return;
            
            var nodeIdStartAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SourceCompartmentId);
            sewerConnection.SourceCompartmentName = nodeIdStartAttribute.GetValidStringValue();

            var nodeIdEndAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TargetCompartmentId);
            sewerConnection.TargetCompartmentName = nodeIdEndAttribute.GetValidStringValue();

            double auxDouble;

            var levelStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelStart);
            if( levelStart.TryGetValueAsDouble(out auxDouble))
                sewerConnection.LevelSource = auxDouble;

            var levelEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.LevelEnd);
            if( levelEnd.TryGetValueAsDouble(out auxDouble))
                sewerConnection.LevelTarget = auxDouble;

            var length = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.Length);
            if (length.TryGetValueAsDouble(out auxDouble))
            {
                sewerConnection.IsLengthCustom = true;
                sewerConnection.Length = auxDouble;
            }

            var waterType = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.WaterType);
            if (waterType.IsValidAttribute())
            {
                //Find type
                sewerConnection.WaterType = waterType.GetValueFromDescription<SewerConnectionWaterType>();
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