using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;
using DeltaShell.Plugins.ImportExport.GWSW.Properties;
using log4net;

namespace DeltaShell.Plugins.ImportExport.GWSW
{
    public static class SewerFeatureFactory
    {
        #region Creators
        
        /// <summary>
        /// Generate multiple sewer features from a list of GwswElements.
        /// </summary>
        /// <param name="elementTypesList">List of GwswElements by key.</param>
        /// <param name="setProgress"></param>
        /// <param name="gwswFileImporter"></param>
        /// <returns>IList of ISewerFeature objects that have been created from objects in gwswElements.<param name="gwswElements"/></returns>
        public static IEnumerable<ISewerFeature> CreateSewerEntities(ILookup<SewerFeatureType, GwswElement> elementTypesList, GwswFileImporter importer)
        {
            var listOfGwswElementGenerationActivities = new List<GwswElementGenerationActivity<ISewerFeature>>();
            foreach (var element in elementTypesList)
            {
                var gwswElementGenerationActivity = new GwswElementGenerationActivity<ISewerFeature>(element.Key, element.ToArray(), importer);
                listOfGwswElementGenerationActivities.Add(gwswElementGenerationActivity);
            }

            foreach (var gwswFileImportActivity in listOfGwswElementGenerationActivities)
            {
                importer.ActivityRunner.Enqueue(gwswFileImportActivity);
            }

            while (listOfGwswElementGenerationActivities.Any(im => im.Status != ActivityStatus.Cleaned))
            {
                Thread.Sleep(100);
            }

            return listOfGwswElementGenerationActivities
                .SelectMany(l => l.Features);
        }
        
        #endregion

        public static IGwswFeatureGenerator<T> GetGwswFeatureGenerator<T>(SewerFeatureType featureType, GwswElement gwswElement)
        {
            if (typeof(T) == typeof(ISewerFeature))
                return (IGwswFeatureGenerator<T>) GetSewerFeatureGenerator(featureType, gwswElement);
            if (typeof(T) == typeof(INwrwFeature))
                return (IGwswFeatureGenerator<T>) GetNwrwFeatureGenerator(featureType);
            return null;
        }

        private static IGwswFeatureGenerator<INwrwFeature> GetNwrwFeatureGenerator(SewerFeatureType elementType)
        {
            IGwswFeatureGenerator<INwrwFeature> generator;
            switch (elementType)
            {
                case SewerFeatureType.Surface:
                    // Surface types (oppervlak.csv)
                    generator = new GwswNwrwSurfaceDataGenerator();
                    break;
                case SewerFeatureType.Runoff:
                    // Runoff types (nwrw.csv)
                    generator = new GwswNwrwRunoffDefinitionGenerator();
                    break;
                case SewerFeatureType.Distribution:
                    // Distribution types (verloop.csv)
                    generator = new GwswNwrwDryWeatherFlowDefinitionGenerator();
                    break;
                case SewerFeatureType.Discharge:
                    // Discharge types (debiet.csv)
                    generator = new GwswNwrwDischargeDataGenerator();
                    break;
                default:
                    generator = null;
                    break;
            }
            return generator;
        }

        private static IGwswFeatureGenerator<ISewerFeature> GetSewerFeatureGenerator(SewerFeatureType elementType, GwswElement gwswElement)
        {
            IGwswFeatureGenerator<ISewerFeature> generator;
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

        private static IGwswFeatureGenerator<ISewerFeature> GetCrossSectionGenerator(this GwswElement gwswElement)
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

        private static IGwswFeatureGenerator<ISewerFeature> GetSewerStructureGenerator(this GwswElement gwswElement)
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

        private static IGwswFeatureGenerator<ISewerFeature> GetSewerConnectionGenerator(this GwswElement gwswElement)
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

        public static IEnumerable<INwrwFeature> CreateNwrwEntities(
            ILookup<SewerFeatureType, GwswElement> elementTypesList, GwswFileImporter importer,
            List<string> errorsDuringImport)
        {
            var listOfGwswElementGenerationActivities = new List<GwswElementGenerationActivity<INwrwFeature>>();
            foreach (var element in elementTypesList)
            {
                var gwswElementGenerationActivity = new GwswElementGenerationActivity<INwrwFeature>(element.Key, element.ToArray(), importer);
                listOfGwswElementGenerationActivities.Add(gwswElementGenerationActivity);
            }
            foreach (var gwswFileImportActivity in listOfGwswElementGenerationActivities)
            {
                importer.ActivityRunner.Enqueue(gwswFileImportActivity);
            }

            while (listOfGwswElementGenerationActivities.Any(im => im.Status != ActivityStatus.Cleaned))
            {
                Thread.Sleep(100);
            }
            errorsDuringImport.AddRange(listOfGwswElementGenerationActivities.SelectMany(l=>l.GenerationExceptions));
            return listOfGwswElementGenerationActivities
                .SelectMany(l => l.Features);
        }
        
    }

    internal static class GwswElementValidationExtensions
    {
        private static ILog Log = LogManager.GetLogger(typeof(GwswElementValidationExtensions));

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
            return (TEnum) typeof(TEnum).GetEnumValueFromDescription(valueAsString);
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

            var featureType = (SewerFeatureType)typeof(SewerFeatureType).GetEnumValueFromDescription(gwswElement.ElementTypeName);
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

            var featureType = (SewerFeatureType)typeof(SewerFeatureType).GetEnumValueFromDescription(gwswElement.ElementTypeName);
            return featureType == SewerFeatureType.Structure;
        }
    }
}
