using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Shell.Core.Workflow;
using Deltares.Infrastructure.API.Logging;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Nwrw;

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

        public static IGwswFeatureGenerator<T> GetGwswFeatureGenerator<T>(SewerFeatureType featureType, GwswElement gwswElement, ILogHandler logHandler)
        {
            if (typeof(T) == typeof(ISewerFeature))
                return (IGwswFeatureGenerator<T>) GetSewerFeatureGenerator(featureType, gwswElement, logHandler);
            if (typeof(T) == typeof(INwrwFeature))
                return (IGwswFeatureGenerator<T>)GetNwrwFeatureGenerator(featureType, logHandler);
            return null;
        }

        private static IGwswFeatureGenerator<INwrwFeature> GetNwrwFeatureGenerator(SewerFeatureType elementType, ILogHandler logHandler)
        {
            IGwswFeatureGenerator<INwrwFeature> generator;
            switch (elementType)
            {
                case SewerFeatureType.Surface:
                    // Surface types (oppervlak.csv)
                    generator = new GwswNwrwSurfaceDataGenerator(logHandler);
                    break;
                case SewerFeatureType.Runoff:
                    // Runoff types (nwrw.csv)
                    generator = new GwswNwrwRunoffDefinitionGenerator(logHandler);
                    break;
                case SewerFeatureType.Distribution:
                    // Distribution types (verloop.csv)
                    generator = new GwswNwrwDryWeatherFlowDefinitionGenerator(logHandler);
                    break;
                case SewerFeatureType.Discharge:
                    // Discharge types (debiet.csv)
                    generator = new GwswNwrwDischargeDataGenerator(logHandler);
                    break;
                default:
                    generator = null;
                    break;
            }
            return generator;
        }

        private static IGwswFeatureGenerator<ISewerFeature> GetSewerFeatureGenerator(SewerFeatureType elementType, GwswElement gwswElement, ILogHandler logHandler)
        {
            IGwswFeatureGenerator<ISewerFeature> generator;
            switch (elementType)
            {
                case SewerFeatureType.Node:
                    generator = gwswElement.GetSewerCompartmentGenerator(logHandler);
                    break;
                case SewerFeatureType.Crosssection:
                    generator = gwswElement.GetCrossSectionGenerator(logHandler);
                    break;
                case SewerFeatureType.Connection:
                    generator = gwswElement.GetSewerConnectionGenerator(logHandler);
                    break;
                case SewerFeatureType.Structure:
                    generator = gwswElement.GetSewerStructureGenerator(logHandler);
                    break;
                default:
                    generator = null;
                    break;
            }
            return generator;
        }

        private static IGwswFeatureGenerator<ISewerFeature> GetCrossSectionGenerator(this GwswElement gwswElement, ILogHandler logHandler )
        {
            if (!gwswElement.IsValidGwswSewerProfile(logHandler)) return null;

            var profileShapeAttribute = gwswElement.GetAttributeFromList(SewerProfileMapping.PropertyKeys.SewerProfileShape, logHandler);
            var structureType = profileShapeAttribute.GetValueFromDescription<SewerProfileMapping.SewerProfileType>(logHandler);
            switch (structureType)
            {
                case SewerProfileMapping.SewerProfileType.InvertedEgg:
                    return new InvertedEggCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.Egg:
                    return new EggCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.UShape:
                    return new UShapeCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.Arch:
                    return new ArchCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.Cunette:
                    return new CunetteCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.Rectangle:
                    return new RectangleCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.Elliptical:
                    return new EllipticalCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.Circle:
                    return new CircleCrossSectionShapeGenerator(logHandler);
                case SewerProfileMapping.SewerProfileType.Trapezoid:
                    return new TrapezoidCrossSectionShapeGenerator(logHandler);
                default:
                    return new DefaultCrossSectionShapeGenerator(logHandler);
            }
        }

        private static IGwswFeatureGenerator<ISewerFeature> GetSewerStructureGenerator(this GwswElement gwswElement, ILogHandler logHandler)
        {
            var structureTypeAttribute = gwswElement.GetAttributeFromList(SewerStructureMapping.PropertyKeys.StructureType, logHandler);
            if (!structureTypeAttribute.IsValidAttribute(logHandler)) return null;

            var structureType = structureTypeAttribute.GetValueFromDescription<SewerStructureMapping.StructureType>(logHandler);
            switch (structureType)
            {
                case SewerStructureMapping.StructureType.Pump:
                    return new SewerPumpGenerator(logHandler);
                case SewerStructureMapping.StructureType.Crest:
                    return new SewerWeirGenerator(logHandler);
                case SewerStructureMapping.StructureType.Orifice:
                    return new SewerOrificeGenerator(logHandler);
                case SewerStructureMapping.StructureType.Outlet:
                    return new SewerOutletCompartmentGenerator(logHandler);
                default:
                    return new SewerConnectionGenerator(logHandler);
            }
        }

        private static ASewerCompartmentGenerator GetSewerCompartmentGenerator(this GwswElement gwswElement, ILogHandler logHandler)
        {
            ASewerCompartmentGenerator compartmentGenerator = new SewerCompartmentGenerator(logHandler);
                
            var nodeTypeAttribute = gwswElement.GetAttributeFromList(ManholeMapping.PropertyKeys.NodeType, logHandler);
            if (nodeTypeAttribute.IsValidAttribute(logHandler) && nodeTypeAttribute.IsGwswOutlet(logHandler))
                compartmentGenerator = new SewerOutletCompartmentGenerator(logHandler);

            return compartmentGenerator;
        }

        private static IGwswFeatureGenerator<ISewerFeature> GetSewerConnectionGenerator(this GwswElement gwswElement, ILogHandler logHandler)
        {
            var sewerTypeAttribute = gwswElement.GetAttributeFromList(SewerConnectionMapping.PropertyKeys.PipeType, logHandler);
            var basicGenerator = new SewerConnectionGenerator(logHandler);
            if (!sewerTypeAttribute.IsValidAttribute(logHandler)) return basicGenerator;

            if (sewerTypeAttribute.IsGwswPipe(logHandler)) return new SewerConnectionPipeGenerator(logHandler);
            if (sewerTypeAttribute.IsGwswOrifice(logHandler)) return new SewerOrificeGenerator(logHandler);
            if (sewerTypeAttribute.IsGwswPump(logHandler)) return new SewerPumpGenerator(logHandler);
            if (sewerTypeAttribute.IsGwswWeir(logHandler)) return new SewerWeirGenerator(logHandler);

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
}
