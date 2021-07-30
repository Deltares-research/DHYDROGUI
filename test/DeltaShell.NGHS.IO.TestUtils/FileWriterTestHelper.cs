using System;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.TestUtils
{
    public static class FileWriterTestHelper
    {
        public const string RelativeTargetDirectory = "FileWriters";
        private static readonly string targetPath = Path.Combine(Environment.CurrentDirectory, RelativeTargetDirectory);
        public static readonly ModelFileNames ModelFileNames = new ModelFileNames(Path.Combine(targetPath, ModelFileNames.ModelDefinitionFilename));
        
        public static IHydroNetwork SetupSimpleHydroNetworkWith2NodesAnd1Branch()
        {
            // specify your network here
            var network = new HydroNetwork();

            // add nodes and branches
            IHydroNode node1 = new HydroNode {Name = "node1", LongName = string.Empty, Network = network};
            IHydroNode node2 = new HydroNode {Name = "node2", LongName = string.Empty, Network = network};
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch = new Channel("branch", node1, node2)
            {
                LongName = string.Empty,
                OrderNumber = 0,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(100, 0)
                })
            };

            network.Branches.Add(branch);
            return network;
        }

        /// <summary>
        /// add a cross section on a branch
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="csType"></param>
        /// <param name="chainage"></param>
        /// <param name="levelShift"></param>
        /// <param name="makeProxy"></param>
        public static ICrossSection AddCrossSection(IBranch branch, CrossSectionType csType, double chainage, double levelShift = 0.0, bool makeProxy = false)
        {
            // By default cross section width = 100 and this is divided over 6 points (see: SetDefaultYZTableAndUpdateThalWeg)
            var crossSection = CrossSection.CreateDefault(csType, branch, chainage);
            var name = HydroNetworkHelper.GetUniqueFeatureName((IHydroRegion)branch.Network, crossSection);
            crossSection.Name = name;
            if (!crossSection.Definition.IsProxy)
            {
                crossSection.Definition.Name = name;
            }

            branch.BranchFeatures.Add(crossSection);

            if (makeProxy)
            {
                crossSection.ShareDefinitionAndChangeToProxy();
                var crossSectionDefinitionProxy = crossSection.Definition as CrossSectionDefinitionProxy;
                if (crossSectionDefinitionProxy != null) crossSectionDefinitionProxy.LevelShift = levelShift;
            }
            
            AddCrossSectionDefinitionSection(crossSection.Definition, RoughnessDataSet.MainSectionTypeName, 0.0, 25.0);
            AddCrossSectionDefinitionSection(crossSection.Definition, RoughnessDataSet.Floodplain1SectionTypeName, 25.0, 75.0);
            AddCrossSectionDefinitionSection(crossSection.Definition, RoughnessDataSet.Floodplain2SectionTypeName, 75.0, 100.0);
            return crossSection;
        }

        public static void AddCrossSectionDefinitionSection(ICrossSectionDefinition crossSectionDefinition, string name, double yMin, double yMax)
        {
            crossSectionDefinition.Sections.Add(new CrossSectionSection
            {
                SectionType = new CrossSectionSectionType { Name = name },
                MinY = yMin,
                MaxY = yMax
            });
        }

        public static void AddObservationPoint(IBranch branch, int obsPntId, string name, double chainage)
        {
            var obsPnt = ObservationPoint.CreateDefault(branch);
            obsPnt.Name = obsPntId.ToString();
            obsPnt.LongName = name;
            obsPnt.Chainage = chainage;
            branch.BranchFeatures.Add(obsPnt);
        }
    }
}