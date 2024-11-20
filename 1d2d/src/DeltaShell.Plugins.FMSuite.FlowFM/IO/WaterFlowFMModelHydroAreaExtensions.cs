using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelHydroAreaExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModelHydroAreaExtensions));

        /// <summary>
        /// Gets the features from a category mentioned in the dimr xml.
        /// </summary>
        /// <param name="area"> The hydro area of a model. </param>
        /// <param name="category"> The category. </param>
        /// <returns> Features of the hydro area of a model from a specific category or empty when <paramref name="category"/> is unknown. </returns>
        public static IEnumerable<IFeature> GetFeaturesFromCategory(this HydroArea area, string category)
        {
            switch (category)
            {
                case KnownFeatureCategories.Pumps:
                    return area.Pumps;
                case KnownFeatureCategories.GeneralStructures:
                    return area.Weirs.Where(w => w.WeirFormula is GeneralStructureWeirFormula);
                case KnownFeatureCategories.Gates:
                    return area.Weirs.Where(w => w.WeirFormula is GatedWeirFormula);
                case KnownFeatureCategories.Weirs:
                    return area.Weirs.Where(w => w.WeirFormula is SimpleWeirFormula);
                case KnownFeatureCategories.ObservationPoints:
                    return area.ObservationPoints;
                case KnownFeatureCategories.ObservationCrossSections:
                    return area.ObservationCrossSections;
                case Model1DParametersCategories.LeveeBreaches:
                    return area.LeveeBreaches;
                default:
                    return Enumerable.Empty<IFeature>();
            }
        }
        /// <summary>
        /// Gets the features from a category mentioned in the dimr xml.
        /// </summary>
        /// <param name="network"> The hydro network a of a model. </param>
        /// <param name="category"> The category. </param>
        /// <returns> Features of the hydro area of a model from a specific category. </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="category"/> is unknown.
        /// </exception>
        public static IEnumerable<IFeature> GetFeaturesFromCategory(this IHydroNetwork network, string category)
        {
            switch (category)
            {
                case Model1DParametersCategories.Weirs:
                    return network.Weirs;
                case Model1DParametersCategories.ObservationPoints:
                    return network.ObservationPoints;
                case Model1DParametersCategories.Culverts:
                    return network.Culverts;
                case Model1DParametersCategories.Pumps:
                    return network.Pumps;
                case Model1DParametersCategories.Laterals:
                    return network.LateralSources;
                case Model1DParametersCategories.Gates:
                    return network.Gates;
                case Model1DParametersCategories.CrossSections:
                    return network.CrossSections;
                case Model1DParametersCategories.Orifices:
                    return network.Orifices;
                case Model1DParametersCategories.GeneralStructures:
                    return network.Weirs.Where(w => w.WeirFormula is GeneralStructureWeirFormula);
                case Model1DParametersCategories.Retentions:
                    return network.Retentions;
                default:
                    return Enumerable.Empty<IFeature>();
            }
        }

        #region RemoveDuplicateFeatures

        public static void RemoveDuplicateFeatures(object features, IGroupableFeature addedFeature, string modelName)
        {
            if (addedFeature == null) return;

            var landBoundary = addedFeature as LandBoundary2D;
            if (landBoundary != null)
            {
                RemoveAddedFeatureIfDuplicate(features, landBoundary, modelName);
                return;
            }

            var polygon = addedFeature as GroupableFeature2DPolygon;
            if (polygon != null)
            {
                RemoveAddedFeatureIfDuplicate(features, polygon, modelName);
                return;
            }

            var thinDam = addedFeature as ThinDam2D;
            if (thinDam != null)
            {
                RemoveAddedFeatureIfDuplicate(features, thinDam, modelName);
                return;
            }

            var fixedWeir = addedFeature as FixedWeir;
            if (fixedWeir != null)
            {
                RemoveAddedFeatureIfDuplicate(features, fixedWeir, modelName);
                return;
            }

            var point = addedFeature as GroupableFeature2DPoint;
            if (point != null)
            {
                RemoveAddedFeatureIfDuplicate(features, point, modelName);
                return;
            }

            var crossSection = addedFeature as ObservationCrossSection2D;
            if (crossSection != null)
            {
                RemoveAddedFeatureIfDuplicate(features, crossSection, modelName);
                return;
            }

            var pump = addedFeature as Pump2D;
            if (pump != null)
            {
                RemoveAddedFeatureIfDuplicate(features, pump, modelName);
                return;
            }

            var weir = addedFeature as Weir2D;
            if (weir != null)
            {
                RemoveAddedFeatureIfDuplicate(features, weir, modelName);
                return;
            }

            var gate = addedFeature as Gate2D;
            if (gate != null) RemoveAddedFeatureIfDuplicate(features, gate, modelName);

            var bridgePillar = addedFeature as BridgePillar;
            if (bridgePillar != null) RemoveAddedFeatureIfDuplicate(features, bridgePillar, modelName);

        }

        private static void RemoveAddedFeatureIfDuplicate<T>(object features, T addedFeature, string modelName) where T : IGroupableFeature, INameable
        {
            var featureList = features as EventedList<T>;
            if (featureList != null)
            {
                RemoveNewObjectFromListIfDuplicate(featureList, addedFeature, modelName);
            }
        }

        private static void RemoveNewObjectFromListIfDuplicate<T>(EventedList<T> features, T addedFeature, string modelName) where T : IGroupableFeature, INameable
        {
            if (features.Count(f => f.Name == addedFeature.Name && f.GroupName == addedFeature.GroupName) > 1)
            {
                features.RemoveAt(features.Count - 1);
                Log.WarnFormat("Feature with group name '{0}'and name '{1}' has not been added to model '{2}', because a feature with the same properties already exists."
                    , addedFeature.GroupName, addedFeature.Name, modelName);
            }
        }

        #endregion

        public static void UpdateGroupName(this IGroupableFeature groupableFeature, WaterFlowFMModel model)
        {
            groupableFeature.RenameStructureGroupNameToStructureFilePath(model);
            groupableFeature.MakeGroupNameRelative(model.GetModelDirectory(), model.GetMduDirectory());
        }

        private static void RenameStructureGroupNameToStructureFilePath(this IGroupableFeature hydroAreaFeature, WaterFlowFMModel model)
        {
            if (!(hydroAreaFeature is Gate2D) && !(hydroAreaFeature is Weir2D) && !(hydroAreaFeature is Pump2D))
            {
                return;
            }
            
            ChangeStructureGroupName<Gate2D>(hydroAreaFeature, model);
            ChangeStructureGroupName<Weir2D>(hydroAreaFeature, model);
            ChangeStructureGroupName<Pump2D>(hydroAreaFeature, model);
        }

        private static void ChangeStructureGroupName<TFeat>(IGroupableFeature hydroAreaFeature, WaterFlowFMModel model) where TFeat : class, IGroupableFeature
        {
            var structure = hydroAreaFeature as TFeat;
            if(structure == null) return;

            var groupName = structure.GroupName;
            if (string.IsNullOrEmpty(groupName) || !Path.IsPathRooted(groupName) || groupName.EndsWith(FileConstants.IniFileExtension))
            {
                return;
            }

            string[] iniFiles = Directory.GetFiles(Path.GetDirectoryName(groupName), $"*{FileConstants.IniFileExtension}");
            
            var structuresFile = new StructuresFile
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = model.ModelDefinition.GetReferenceDateAsDateTime()
            };

            foreach (string file in iniFiles)
            {
                var structures = structuresFile.Read(file);
                var numberOfMatchingStructureNames = structures.Count(s => s.Name == Path.GetFileNameWithoutExtension(groupName));
                if (numberOfMatchingStructureNames > 0)
                {
                    structure.GroupName = file;
                    return;
                }
            }
            
            structure.GroupName = Path.Combine(Path.GetDirectoryName(structure.GroupName), model.Name + FileConstants.StructuresFileExtension);
        }
    }
}
