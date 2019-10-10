using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DeltaShell.Plugins.FMSuite.FlowFM.IO.Importers;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM
{
    public static class SewerFeatureFactory
    {
        private static ILog Log = LogManager.GetLogger(typeof(SewerFeatureFactory));

        #region Creators

        /// <summary>
        /// Generate multiple sewer features from a list of GwswElements.
        /// </summary>
        /// <param name="gwswElements">List of GwswElements.</param>
        /// <returns>IList of ISewerFeature objects that have been created from objects in gwswElements.<param name="gwswElements"/></returns>
        public static IList<ISewerFeature> CreateSewerEntities(IList<GwswElement> gwswElements)
        {
            var createdSewerFeatures = new List<ISewerFeature>();
            var elementTypesList = new List<KeyValuePair<SewerFeatureType, GwswElement>>();

            foreach (var gwswElement in gwswElements)
            {
                SewerFeatureType elementType;
                if(!Enum.TryParse(gwswElement?.ElementTypeName, out elementType)) continue;

                elementTypesList.Add(new KeyValuePair<SewerFeatureType, GwswElement>(elementType, gwswElement));
            }

            // node types
            var nodeTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Node).Select(k => k.Value).ToList();
            if (nodeTypes.Any())
            {
                createdSewerFeatures.AddRange(nodeTypes.Select(CreateSewerFeature));
            }
            
            // Cross section types
            var crossSectionTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Crosssection).Select(k => k.Value).ToList();
            if (crossSectionTypes.Any())
            {
                createdSewerFeatures.AddRange(crossSectionTypes.Select(CreateSewerFeature));
            }

            // Connection types
            var connectionTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Connection).Select(k => k.Value).ToList();
            if (connectionTypes.Any())
            {
                var connectionSewerFeatures = connectionTypes.Select(CreateSewerFeature);
                createdSewerFeatures.AddRange(connectionSewerFeatures);
            }
            
            // Structure types 
            var structureTypes = elementTypesList.Where(k => k.Key == SewerFeatureType.Structure).Select(k => k.Value).ToList();
            if (structureTypes.Any())
            {
                var structureFeatures = structureTypes.Select(CreateSewerFeature).ToList();
                var pointFeatures = structureFeatures.OfType<IStructure1D>();

                foreach (var pointFeature in pointFeatures)
                {

                    if (pointFeature.Branch != null && pointFeature.Branch.Source == pointFeature.Branch.Target)
                    {
                        // is internal connection
                        pointFeature.ParentPointFeature = (Manhole) pointFeature.Branch.Source;
                    }
                }
                
                createdSewerFeatures.AddRange(structureFeatures);
            }
            return createdSewerFeatures;
        }

        /// <summary>
        /// Generates a single sewer feature out of a GwswElement.
        /// </summary>
        /// <param name="gwswElement">Collection of attributes and values extracted from a Csv Element.</param>
        /// <returns>Single sewer feature representing the <param name="gwswElement"/> given as a parameter.</returns>
        private static ISewerFeature CreateSewerFeature(GwswElement gwswElement)
        {
            var generator = GetSewerFeatureGenerator(gwswElement);
            return generator?.Generate(gwswElement);
        }

        #endregion

        private static ISewerFeatureGenerator GetSewerFeatureGenerator(GwswElement gwswElement)
        {
            SewerFeatureType elementType;
            if (!Enum.TryParse(gwswElement?.ElementTypeName, out elementType)) return null;

            ISewerFeatureGenerator generator;
            switch (elementType)
            {
                case SewerFeatureType.Node:
                    generator = gwswElement.GetSewerCompartmentGenerator();
                    break;
                case SewerFeatureType.Crosssection:
                    generator = gwswElement.GetCrossSectionGenerator();
                    break;
                case SewerFeatureType.Connection:
                    generator = gwswElement.GetSewerConnectionGenerator();
                    break;
                case SewerFeatureType.Structure:
                    generator = gwswElement.GetSewerStructureGenerator();
                    break;
                default:
                    generator = null;
                    break;
            }
            return generator;
        }

        private static ISewerFeatureGenerator GetCrossSectionGenerator(this GwswElement gwswElement)
        {
            if (!gwswElement.IsValidGwswSewerProfile()) return null;

            var profileShapeAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileShape);
            var structureType = profileShapeAttribute.GetValueFromDescription<SewerProfileMapping.SewerProfileType>();
            switch (structureType)
            {
                case SewerProfileMapping.SewerProfileType.InvertedEgg:
                    return new InvertedEggCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.Egg:
                    return new EggCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.UShape:
                    return new UShapeCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.Arch:
                    return new ArchCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.Cunette:
                    return new CunetteCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.Rectangle:
                    return new RectangleCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.Elliptical:
                    return new EllipticalCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.Circle:
                    return new CircleCrossSectionShapeGenerator();
                case SewerProfileMapping.SewerProfileType.Trapezoid:
                    return new TrapezoidCrossSectionShapeGenerator();
                default:
                    return new DefaultCrossSectionShapeGenerator();
            }
        }

        private static ISewerFeatureGenerator GetSewerStructureGenerator(this GwswElement gwswElement)
        {
            var structureTypeAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StructureType);
            if (!structureTypeAttribute.IsValidAttribute()) return null;

            var structureType = structureTypeAttribute.GetValueFromDescription<SewerStructureMapping.StructureType>();
            switch (structureType)
            {
                case SewerStructureMapping.StructureType.Pump:
                    return new SewerPumpGenerator();
                case SewerStructureMapping.StructureType.Crest:
                    return new SewerWeirGenerator();
                case SewerStructureMapping.StructureType.Orifice:
                    return new SewerOrificeGenerator();
                case SewerStructureMapping.StructureType.Outlet:
                    return new SewerOutletCompartmentGenerator();
                default:
                    return new SewerConnectionGenerator();
            }
        }

        private static ASewerCompartmentGenerator GetSewerCompartmentGenerator(this GwswElement gwswElement)
        {
            ASewerCompartmentGenerator compartmentGenerator = new SewerCompartmentGenerator();

            var nodeTypeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeType);
            if (nodeTypeAttribute.IsValidAttribute() && nodeTypeAttribute.IsGwswOutlet())
                compartmentGenerator = new SewerOutletCompartmentGenerator();

            return compartmentGenerator;
        }

        private static ISewerFeatureGenerator GetSewerConnectionGenerator(this GwswElement gwswElement)
        {
            var sewerTypeAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PipeType);
            var basicGenerator = new SewerConnectionGenerator();
            if (!sewerTypeAttribute.IsValidAttribute()) return basicGenerator;

            if (sewerTypeAttribute.IsGwswPipe()) return new SewerConnectionPipeGenerator();
            if (sewerTypeAttribute.IsGwswOrifice()) return new SewerOrificeGenerator();
            if (sewerTypeAttribute.IsGwswPump()) return new SewerPumpGenerator();
            if (sewerTypeAttribute.IsGwswWeir()) return new SewerWeirGenerator();

            return basicGenerator;
        }

        #region Helpers

        private static double GetAverageCoordinate(IEnumerable<GwswAttribute> xCoords)
        {
            var cSum = 0.0;
            var validCoords = 0;
            foreach (var xCoord in xCoords)
            {
                var auxDouble = 0.0;
                if (xCoord.TryGetValueAsDouble(out auxDouble)) ++validCoords;
                cSum += auxDouble;
            }
            var xAvgCoord = cSum / validCoords;
            return xAvgCoord;
        }

        #endregion
    }

    internal static class GwswElementValidationExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswElementValidationExtensions));

        public static bool IsAuxGwswManhole(this GwswAttribute sewerTypeAttribute)
        {
            var nodeType = sewerTypeAttribute.GetValueFromDescription<ManholeMapping.NodeType>();
            return nodeType == ManholeMapping.NodeType.Manhole;
        }

        public static bool IsValidGwswManhole(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;
            var manholeName = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.ManholeId);

            //No need for log message because as per now, GwswManholes are our own creation (check CreateAuxiliarGwswElements)
            if (!manholeName.IsValidAttribute()) return false;

            var typeAttr = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeType);
            var manholeType = typeAttr.GetValueFromDescription<ManholeMapping.NodeType>();
            return manholeType == ManholeMapping.NodeType.Manhole;
        }

        public static bool IsGwswOutlet(this GwswAttribute sewerTypeAttribute)
        {
            var nodeType = sewerTypeAttribute.GetValueFromDescription<ManholeMapping.NodeType>();
            return nodeType == ManholeMapping.NodeType.Outlet;
        }

        public static bool IsGwswOrifice(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            return connectionType == SewerConnectionMapping.ConnectionType.Orifice;
        }

        public static bool IsGwswPump(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            return connectionType == SewerConnectionMapping.ConnectionType.Pump;
        }

        public static bool IsGwswPipe(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            if (connectionType == SewerConnectionMapping.ConnectionType.ClosedConnection ||
                connectionType == SewerConnectionMapping.ConnectionType.InfiltrationPipe ||
                connectionType == SewerConnectionMapping.ConnectionType.Open)
            {
                return true;
            }
            return false;
        }

        public static bool IsGwswWeir(this GwswAttribute sewerTypeAttribute)
        {
            var connectionType = sewerTypeAttribute.GetValueFromDescription<SewerConnectionMapping.ConnectionType>();
            return connectionType == SewerConnectionMapping.ConnectionType.Crest;
        }

        public static bool IsValidGwswCompartment(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;

            var featureType = GetEnumValueFromDescription<SewerFeatureType>(gwswElement.ElementTypeName);
            var isNodeGwswElement = featureType == SewerFeatureType.Node;
            if (isNodeGwswElement) return true;

            bool isOutletGwswElement;
            var structureTypeAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StructureType);
            if (structureTypeAttribute == null)
            {
                isOutletGwswElement = false;
            }
            else
            {
                var structureType = GetEnumValueFromDescription<SewerStructureMapping.StructureType>(structureTypeAttribute.ValueAsString);
                isOutletGwswElement = featureType == SewerFeatureType.Structure && structureType == SewerStructureMapping.StructureType.Outlet;
            }

            return isOutletGwswElement;
        }

        private static TEnum GetEnumValueFromDescription<TEnum>(string valueAsString)
        {
            return (TEnum) EnumDescriptionAttributeTypeConverter.GetEnumValue<TEnum>(valueAsString);
        }

        public static bool IsValidGwswSewerConnection(this GwswElement gwswElement)
        {
            if (gwswElement == null || gwswElement.ElementTypeName != SewerFeatureType.Connection.ToString()) return false;

            var nodeIdStart = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.SourceCompartmentId);
            var nodeIdEnd = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.TargetCompartmentId);
            if (nodeIdStart == null || nodeIdEnd == null)
            {
                Log.ErrorFormat(Resources
                    .SewerFeatureFactory_SewerConnectionFactory_Cannot_import_sewer_connection_s__without_Source_and_Target_nodes__Please_check_the_file_for_said_empty_fields);
                return false;
            }

            return true;
        }

        public static bool IsValidGwswSewerProfile(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;

            var profileId = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileId);
            if (!profileId.IsValidAttribute())
            {
                Log.Error(Resources.GwswElementValidationExtensions_IsValidGwswSewerProfile_Cannot_import_sewer_profile_s__without_profile_id__Please_check__Profiel_csv__for_empty_profile_id_s);
                return false;
            }

            var featureType = (SewerFeatureType)EnumDescriptionAttributeTypeConverter.GetEnumValue<SewerFeatureType>(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Crosssection;
        }

        public static bool IsValidGwswStructure(this GwswElement gwswElement)
        {
            if (gwswElement == null) return false;

            var profileId = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.UniqueId);
            if (!profileId.IsValidAttribute())
            {
                Log.Error(Resources.GwswElementValidationExtensions_IsValidGwswStructure_Cannot_import_sewer_structure_s__without_a_unique_id__Please_check__Kunstwerk_csv__for_empty_unique_id_s);
                return false;
            }

            var featureType = (SewerFeatureType)EnumDescriptionAttributeTypeConverter.GetEnumValue<SewerFeatureType>(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Structure;
        }
    }
}
