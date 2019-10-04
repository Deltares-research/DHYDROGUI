using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.IO.Files.Structures;
using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Extensions.Feature;
using log4net;

namespace DeltaShell.Plugins.FMSuite.FlowFM.IO
{
    public static class WaterFlowFMModelHydroAreaExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(WaterFlowFMModelHydroAreaExtensions));

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
                case KnownFeatureCategories.Observations:
                    return area.ObservationPoints;
                case KnownFeatureCategories.CrossSections:
                    return area.ObservationCrossSections;
                default:
                    return Enumerable.Empty<IFeature>();
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