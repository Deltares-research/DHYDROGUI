using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DeltaShell.Plugins.FMSuite.Common.IO;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.Properties;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    /// <summary>
    /// This class contains extension methods for <see cref="HydroArea"/> and <see cref="IGroupableFeature"/>, which are specific for an FM model.
    /// </summary>
    public static class WaterFlowFMModelHydroAreaExtensions
    {
        /// <summary>
        /// Gets the features from a category mentioned in the dimr xml. 
        /// </summary>
        /// <param name="area"> The hydro area of a model. </param>
        /// <param name="category"> The category. </param>
        /// <returns> Features of the hydro area of a model from a specific category. </returns>
        /// <exception cref="ArgumentException"> ArgumentException will be  returned if category is unknown. </exception>
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
                strucGroupName.EndsWith(FileConstants.IniFileExtension))
            {
                return;
            }

            string[] iniFiles = Directory.GetFiles(Path.GetDirectoryName(strucGroupName), $"*{FileConstants.IniFileExtension}");
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
                Path.Combine(Path.GetDirectoryName(structure.GroupName), model.Name + FileConstants.StructuresFileExtension);
        }
    }
}