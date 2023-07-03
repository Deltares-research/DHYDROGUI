using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// This class contains extension methods for <see cref="HydroArea"/> and <see cref="IGroupableFeature"/>, which are
    /// specific for an FM model.
    /// </summary>
    public static class WaterFlowFMModelHydroAreaExtensions
    {
        /// <summary>
        /// Gets the features from a category mentioned in the dimr xml.
        /// </summary>
        /// <param name="area"> The hydro area of a model. </param>
        /// <param name="category"> The category. </param>
        /// <returns> Features of the hydro area of a model from a specific category. </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="category"/> is unknown.
        /// </exception>
        public static IEnumerable<IFeature> GetFeaturesFromCategory(this HydroArea area, string category)
        {
            switch (category)
            {
                case KnownFeatureCategories.Pumps:
                    return area.Pumps;
                case KnownFeatureCategories.GeneralStructures:
                    return area.Structures.Where(w => w.Formula is GeneralStructureFormula);
                case KnownFeatureCategories.Gates:
                    return area.Structures.Where(w => w.Formula is SimpleGateFormula);
                case KnownFeatureCategories.Weirs:
                    return area.Structures.Where(w => w.Formula is SimpleWeirFormula);
                case KnownFeatureCategories.ObservationPoints:
                    return area.ObservationPoints;
                case KnownFeatureCategories.ObservationCrossSections:
                    return area.ObservationCrossSections;
                default:
                    throw new ArgumentException(string.Format(Resources.WaterFlowFMModelHydroAreaExtensions_GetFeaturesFromCategory_unknown_category__0__used, category));
            }
        }

        public static void UpdateGroupName(this IGroupableFeature groupableFeature, WaterFlowFMModel model)
        {
            groupableFeature.RenameStructureGroupNameToStructureFilePath(model);
            groupableFeature.MakeGroupNameRelative(model.MduFilePath);
        }

        private static void RenameStructureGroupNameToStructureFilePath(this IGroupableFeature hydroAreaFeature,
                                                                        WaterFlowFMModel model)
        {
            if (!(hydroAreaFeature is IStructure) && 
                !(hydroAreaFeature is IPump))
            {
                return;
            }

            ChangeStructureGroupName<IStructure>(hydroAreaFeature, model);
            ChangeStructureGroupName<IPump>(hydroAreaFeature, model);
        }

        private static void ChangeStructureGroupName<TFeat>(IGroupableFeature hydroAreaFeature, WaterFlowFMModel model)
            where TFeat : class, IGroupableFeature
        {
            if (!(hydroAreaFeature is TFeat structure))
            {
                return;
            }

            string strucGroupName = structure.GroupName;
            if (string.IsNullOrEmpty(strucGroupName) || 
                !Path.IsPathRooted(strucGroupName) ||
                strucGroupName.EndsWith(FileConstants.IniFileExtension))
            {
                return;
            }

            string[] iniFiles = Directory.GetFiles(Path.GetDirectoryName(strucGroupName), $"*{FileConstants.IniFileExtension}");
            
            var strucFile = new StructuresFile
            {
                StructureSchema = model.ModelDefinition.StructureSchema,
                ReferenceDate = model.ReferenceTime
            };

            foreach (string file in iniFiles)
            {
                IList<IStructureObject> structures = strucFile.Read(file);
                int numberOfMatchingStructureNames =
                    structures.Count(s => s.Name == Path.GetFileNameWithoutExtension(strucGroupName));

                if (numberOfMatchingStructureNames > 0)
                {
                    structure.GroupName = file;
                    return;
                }
            }

            structure.GroupName =
                Path.Combine(Path.GetDirectoryName(structure.GroupName), model.Name + FileConstants.StructuresFileExtension);
        }
    }
}