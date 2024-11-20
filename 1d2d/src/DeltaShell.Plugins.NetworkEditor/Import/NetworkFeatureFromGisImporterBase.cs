using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using log4net;
using NetTopologySuite.Extensions.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Import
{
    public abstract class NetworkFeatureFromGisImporterBase : FeatureFromGisImporterBase
    {
        private const double epsilon = 1.0e-7;

        private static readonly ILog log = LogManager.GetLogger(typeof(NetworkFeatureFromGisImporterBase));

        protected IHydroNetwork HydroNetwork => HydroRegion as IHydroNetwork ?? HydroRegion.SubRegions.OfType<IHydroNetwork>().First();

        protected T AddOrUpdateBranchFeatureFromNetwork<T>(IFeature feature, string columnName, Func<IBranch, T> builder) where T : class, IBranchFeature
        {
            var featureName = feature.Attributes[columnName].ToString();
            var branchFeature = HydroNetwork.BranchFeatures.OfType<T>().FirstOrDefault(bf => bf.Name == featureName);
            var nearestBranch = NetworkHelper.GetNearestBranch(HydroNetwork.Branches, feature.Geometry, SnappingTolerance);

            if (nearestBranch == null)
            {
                return null;
            }

            if (branchFeature == null)
            {
                branchFeature = builder(nearestBranch);
                branchFeature.Name = featureName;
            }

            var coordinate = GeometryHelper.GetNearestPointAtLine((ILineString)branchFeature.Branch.Geometry,
                                                                  feature.Geometry.Coordinate, SnappingTolerance);
            if (coordinate == null)
            {
                log.ErrorFormat(
                    "Feature \"{0}\" has probably moved to another branch. Make a new ID in the source data for this item.",
                    featureName);
                return null;
            }

            branchFeature.Geometry = new Point(coordinate);

            NetworkHelper.UpdateBranchFeatureChainageFromGeometry(branchFeature);

            var structure = branchFeature as IStructure1D;
            if (structure != null)
            {
                if (structure.ParentStructure == null)
                {
                    HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(structure, branchFeature.Branch);
                }
                else
                {
                    if (Math.Abs(branchFeature.Chainage - structure.ParentStructure.Chainage) < epsilon)
                    {
                        structure.Branch.BranchFeatures.Remove(structure);
                        structure.ParentStructure.Structures.Remove(structure);
                        HydroNetworkHelper.AddStructureToExistingCompositeStructureOrToANewOne(structure, branchFeature.Branch);
                    }
                }
            }
            else
            {
                if (!branchFeature.Branch.BranchFeatures.Contains(branchFeature))
                {
                    branchFeature.Branch.BranchFeatures.Add(branchFeature);
                }
            }
            return branchFeature;
        }

        protected ICrossSection AddOrUpdateCrossSectionFromHydroNetwork(IFeature feature, string columnName, CrossSectionType crossSectionType)
        {
            var featureName = feature.Attributes[columnName].ToString();
            var crossSection = HydroNetwork.CrossSections.FirstOrDefault(cs => cs.Name == featureName);
            var nearestBranch = NetworkHelper.GetNearestBranch(HydroNetwork.Branches, feature.Geometry, SnappingTolerance);

            var isNew = crossSection == null;
            if (isNew)
            {
                if (nearestBranch == null)
                {
                    log.ErrorFormat("Could not snap new cross section {0} geometry to network branch", featureName);
                    return null;
                }
                crossSection = CrossSection.CreateDefault(crossSectionType, nearestBranch);
            }
            else
            {
                if (crossSection.CrossSectionType != crossSectionType)
                {
                    throw new NotSupportedException("Changing type of existing cross section is not supported");
                }
            }
            crossSection.Name = featureName;

            double chainage;
            var branchGeometry = (ILineString) nearestBranch.Geometry;
            if (feature.Geometry is ILineString)
            {
                chainage = GeometryHelper.LineStringFirstIntersectionOffset(branchGeometry,
                                                                            (ILineString) feature.Geometry);
                if (crossSection.GeometryBased)
                {
                    crossSection.Definition.BeginEdit("set geometry");
                    crossSection.Geometry = feature.Geometry;
                    crossSection.Definition.EndEdit();
                }
            }
            else
            {
                var coordinate = GeometryHelper.GetNearestPointAtLine(branchGeometry, feature.Geometry.Coordinate,
                                                                      SnappingTolerance);

                chainage = GeometryHelper.Distance(branchGeometry, coordinate);
            }

            if (isNew)
            {
                NetworkHelper.AddBranchFeatureToBranch(crossSection, nearestBranch, chainage);
            }
            else
            {
                crossSection.Chainage = BranchFeature.SnapChainage(nearestBranch.Length, chainage);
            }

            return crossSection;
        }
    }
}

