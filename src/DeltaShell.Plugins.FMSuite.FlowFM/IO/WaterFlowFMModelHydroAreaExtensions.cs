using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelHydroAreaExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModelHydroAreaExtensions));

        #region RemoveDuplicateFeatures

        public static void RemoveDuplicateFeatures(object features, IGroupableFeature addedFeature, string modelName)
        {
            if (addedFeature == null)
            {
                return;
            }

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

            var bridgePillar = addedFeature as BridgePillar;
            if (bridgePillar != null)
            {
                RemoveAddedFeatureIfDuplicate(features, bridgePillar, modelName);
            }
        }

        private static void RemoveAddedFeatureIfDuplicate<T>(object features, T addedFeature, string modelName)
            where T : IGroupableFeature, INameable
        {
            var featureList = features as EventedList<T>;
            if (featureList != null)
            {
                RemoveNewObjectFromListIfDuplicate(featureList, addedFeature, modelName);
            }
        }

        private static void RemoveNewObjectFromListIfDuplicate<T>(EventedList<T> features, T addedFeature,
                                                                  string modelName)
            where T : IGroupableFeature, INameable
        {
            if (features.Count(f => f.Name == addedFeature.Name && f.GroupName == addedFeature.GroupName) > 1)
            {
                features.RemoveAt(features.Count - 1);
                Log.WarnFormat(
                    "Feature with group name '{0}'and name '{1}' has not been added to model '{2}', because a feature with the same properties already exists."
                    , addedFeature.GroupName, addedFeature.Name, modelName);
            }
        }

        #endregion

        public static void UpdateGroupName(this IGroupableFeature groupableFeature, WaterFlowFMModel model)
        {
            groupableFeature.RenameStructureGroupNameToStructureFilePath(model);
            groupableFeature.MakeGroupNameRelative(model.MduFilePath);
        }

        private static void RenameStructureGroupNameToStructureFilePath(this IGroupableFeature hydroAreaFeature,
                                                                        WaterFlowFMModel model)
        {
            if (!(hydroAreaFeature is Weir2D) && !(hydroAreaFeature is Pump2D))
            {
                return;
            }

            ChangeStructureGroupName<Weir2D>(hydroAreaFeature, model);
            ChangeStructureGroupName<Pump2D>(hydroAreaFeature, model);
        }

        private static void ChangeStructureGroupName<TFeat>(IGroupableFeature hydroAreaFeature, WaterFlowFMModel model)
            where TFeat : class, IGroupableFeature
        {
            var structure = hydroAreaFeature as TFeat;
            if (structure == null)
            {
                return;
            }

            string strucGroupName = structure.GroupName;
            if (string.IsNullOrEmpty(strucGroupName) || !Path.IsPathRooted(strucGroupName) ||
                strucGroupName.EndsWith(".ini"))
            {
                return;
            }

            string[] iniFiles = Directory.GetFiles(Path.GetDirectoryName(strucGroupName), "*.ini");
            var strucFile = new StructuresFile
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = (DateTime) model.ModelDefinition.GetModelProperty(KnownProperties.RefDate).Value
            };

            foreach (string file in iniFiles)
            {
                IList<IStructure> structures = strucFile.Read(file);
                int numberOfMatchingStructureNames =
                    structures.Count(s => s.Name == Path.GetFileNameWithoutExtension(strucGroupName));
                if (numberOfMatchingStructureNames > 0)
                {
                    structure.GroupName = file;
                    return;
                }
            }

            structure.GroupName =
                Path.Combine(Path.GetDirectoryName(structure.GroupName), model.Name + "_structures.ini");
        }
    }
}