using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Roughness;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using GeoAPI.Extensions.Networks;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers
{
    /// <summary>
    /// Class responsible for setting crossections in model api.
    /// Basically convert DS structures to model API calls. 
    /// Waterflowmodel1d delegate this task to this class.
    /// </summary>
    public class CrossSectionModelApiController
    {
        private readonly IModelApi modelApi;
        private readonly IList<RoughnessSection> roughnessSections;

        public CrossSectionModelApiController(IModelApi modelApi,IList<RoughnessSection> roughnessSections)
        {
            this.modelApi = modelApi;
            this.roughnessSections = roughnessSections;
        }


        public int SetProfileInModelApi(ICrossSection crossSection, ICrossSectionDefinition crossSectionDefinition, bool useReverseRoughness)
        {
            if (crossSectionDefinition.IsProxy)
            {
                var crossSectionDefinitionProxy = (CrossSectionDefinitionProxy) crossSectionDefinition;
                var effectiveDefinition = crossSectionDefinitionProxy.GetUnProxiedDefinition();
                return SetProfileInModelApi(crossSection, effectiveDefinition, useReverseRoughness);
            }

            switch (crossSectionDefinition.CrossSectionType)
            {
                case CrossSectionType.Standard:
                    return SetStandardProfile((CrossSectionDefinitionStandard) crossSectionDefinition); 
                case CrossSectionType.ZW:
                    return SetTabulatedProfile((CrossSectionDefinitionZW) crossSectionDefinition, false, false, 0.0);
                
                //fallthrough!
                case CrossSectionType.YZ:
                case CrossSectionType.GeometryBased:
                    return SetYZProfile(crossSectionDefinition, crossSection, useReverseRoughness);
                default: 
                    throw new NotImplementedException();
            }
        }

        private int SetStandardProfile(CrossSectionDefinitionStandard standardDefinition)
        {
            CrossSectionDefinitionZW crossSectionDefinitionZW = standardDefinition.Shape.GetTabulatedDefinition();
            crossSectionDefinitionZW.ShiftLevel(standardDefinition.LevelShift);
            crossSectionDefinitionZW.AddSection(standardDefinition.Sections[0].SectionType, crossSectionDefinitionZW.Width);
            
            //var isClosed = (standardDefinition.ShapeType == CrossSectionStandardShapeType.Circle || standardDefinition.ShapeType == CrossSectionStandardShapeType.Egg);
            //Wait for implementation closed branches

            const bool isClosed = false;
            return SetTabulatedProfile(crossSectionDefinitionZW, isClosed, false, 0.0);
        }

        /// <summary>
        /// This sets a zw profile in the modelApi.
        /// </summary>
        /// <param name="crossSectionDefinition"></param>
        /// <param name="closed"></param>
        /// <returns></returns>
        public int SetTabulatedProfile(CrossSectionDefinitionZW crossSectionDefinitionZW, bool closed, bool groundlayerUsed, double groundLayerThickness)
        {
            var sortedData = crossSectionDefinitionZW.ZWDataTable.OrderBy(hfsw => hfsw.Z).ToArray();

            var levels = sortedData.Select(r => r.Z).ToArray();
            var flowWidth = sortedData.Select(r => r.Width - r.StorageWidth).ToArray();
            var totalWidth = sortedData.Select(r => r.Width).ToArray();
            
            // set width of section main section, flood plane 1 and flood plane 2 to model engine
            var plains = new double[3];
            plains[0] = crossSectionDefinitionZW.GetSectionWidth(RoughnessDataSet.MainSectionTypeName);
            plains[1] = crossSectionDefinitionZW.GetSectionWidth(RoughnessDataSet.Floodplain1SectionTypeName);
            plains[2] = crossSectionDefinitionZW.GetSectionWidth(RoughnessDataSet.Floodplain2SectionTypeName);

            groundLayerThickness = groundlayerUsed ? groundLayerThickness : 0.0; //just to be sure

            if (!crossSectionDefinitionZW.SummerDike.Active)
            {
                return modelApi.NetworkSetTabCrossSection(levels, flowWidth, totalWidth, plains, closed, groundlayerUsed, groundLayerThickness);
            }

            return modelApi.NetworkSetTabCrossSection(levels, flowWidth, totalWidth, plains,
                                                      crossSectionDefinitionZW.SummerDike.CrestLevel,
                                                      crossSectionDefinitionZW.SummerDike.FloodPlainLevel,
                                                      crossSectionDefinitionZW.SummerDike.FloodSurface,
                                                      crossSectionDefinitionZW.SummerDike.TotalSurface, closed, groundlayerUsed,
                                                      groundLayerThickness);
        }

        private int SetYZProfile(ICrossSectionDefinition crossSectionDefinition, IBranchFeature location, bool useReverseRoughness)
        {
            IList<double> y = crossSectionDefinition.Profile.Select(yz => yz.X).ToArray();
            IList<double> z = crossSectionDefinition.Profile.Select(yz => yz.Y).ToArray();
            
            if (roughnessSections.Count == 0)
            {
                // this should not happen.
                throw new ArgumentException("Can not set yz profile to engine without roughnessSection.",
                                            "roughnessSections");
            }
            if (crossSectionDefinition.Sections.Count == 0)
            {
                IList<CrossSectionSection> crossSectionSections = new List<CrossSectionSection>
                                                                      {
                                                                          new CrossSectionSection
                                                                              {
                                                                                  MinY = y[0],
                                                                                  MaxY = y[y.Count - 1],
                                                                                  // always use "main"?; first is temporary fix
                                                                                  SectionType = roughnessSections[0].CrossSectionSectionType
                                                                              }
                                                                      };
                return SetYZProfile(location, crossSectionSections, y, z, new List<double>(), new List<double>(), useReverseRoughness);
            }

            //NOTE: Rekenhart doesn't support storage for YZ yet, so we're not implementing / sending that yet. Once it gets in,
            //NOTE: some validation test should fail and we can implement it.

            return SetYZProfile(location, crossSectionDefinition.Sections, y, z, new List<double>(), new List<double>(), useReverseRoughness);
        }

        private int SetYZProfile(IBranchFeature location,
                                 IList<CrossSectionSection> crossSectionSections, IList<double> y, IList<double> z,
                                 IList<double> yStorage, IList<double> zStorage, bool useReverseRoughness)
        {
            if (crossSectionSections.Count == 0)
            {
                throw new ArgumentException("Can not set yz profile to engine without roughnessSection.",
                                            "crossSectionSections");
            }

            var sectionCount = crossSectionSections.Count;

            var frictionSectionFrom = new double[sectionCount];
            var frictionSectionTo = new double[sectionCount];
            var frictionTypePos = new int[sectionCount];
            var frictionValuePos = new double[sectionCount];
            var frictionTypeNeg = new int[sectionCount];
            var frictionValueNeg = new double[sectionCount];

            for (var i = 0; i < sectionCount; i++)
            {
                var crossSectionSection = crossSectionSections[i];
                frictionSectionFrom[i] = crossSectionSection.MinY;
                frictionSectionTo[i] = crossSectionSection.MaxY;
                
                var roughnessSection = GetRoughnessSection(crossSectionSection);

                //The roughness values for YZ cannot be Q or H dependent (specifically: not Q dependent without major performance issues and changes to rekenhart). 
                //In the user interface this is not clear, so we need to add a validation warning. It does make life easier here, just use the coverage:

                frictionTypePos[i] =
                    (int) FrictionTypeConverter.ConvertFrictionType(roughnessSection.EvaluateRoughnessType(
                        location.ToNetworkLocation()));
                //For YZ this is not constrained to be the same, but for tabulated it is. To keep things simple, in the UI it must be the same for all. 
                frictionTypeNeg[i] = frictionTypePos[i];

                frictionValuePos[i] = roughnessSection.EvaluateRoughnessValue(
                    location.ToNetworkLocation());

                frictionValueNeg[i] = useReverseRoughness ? GetNegativeFrictionValue(roughnessSection, location) : frictionValuePos[i];
            }

            return modelApi.NetworkSetYZCrossSection(y.ToArray(), z.ToArray(),
                                                            frictionSectionFrom, frictionSectionTo,
                                                            frictionTypePos, frictionValuePos,
                                                            frictionTypeNeg, frictionValueNeg,
                                                            yStorage.ToArray(), zStorage.ToArray());
        }

        private RoughnessSection GetRoughnessSection(CrossSectionSection crossSectionSection)
        {
            var roughnessSection = roughnessSections.FirstOrDefault(rs => rs.Name == crossSectionSection.SectionType.Name);
            if (roughnessSection == null)
            {
                throw new InvalidOperationException("No roughnessSection found with name " + crossSectionSection.SectionType.Name);
            }
            return roughnessSection;
        }

        private double GetNegativeFrictionValue(RoughnessSection roughnessSection, IBranchFeature location)
        {
            var reverseRoughnessSection = roughnessSections.GetApplicableReverseRoughnessSection(roughnessSection);
            var networkLocation = new NetworkLocation(location.Branch, location.Chainage);

            return reverseRoughnessSection.RoughnessNetworkCoverage.EvaluateRoughnessValue(networkLocation);
        }
    }
}
