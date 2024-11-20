using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Utils.Reflection;
using NetTopologySuite.Geometries;

namespace DelftTools.Hydro.Tests.Helpers
{
    public static class TestSewerNetworkProvider
    {
        public const string SourceManholeId = "myManhole1";
        public const string TargetManholeId = "myManhole2";
        public const string SourceCompartmentName = "myCompartment1";
        public const string TargetCompartmentName = "myCompartment2";
        public const string SewerConnectionName = "mySewerConnection";
        public const string OrificeName = "myOrifice";
        public const string PumpName = "myPump";
        public const string WeirName = "myWeir";

        public const string crossSectionDefinitionName = "crossSectionDef001";
        public static Geometry SourceCompartmentGeometry = new Point(10.0, 10.0);
        public static Geometry TargetCompartmentGeometry = new Point(20.0, 20.0);

        public static HydroNetwork CreateSewerNetwork_OneSewerConnectionTwoManholesWithOneCompartmentEach()
        {
            var network = new HydroNetwork();

            var targetManhole = new Manhole(TargetManholeId);
            var targetCompartment = new Compartment(TargetCompartmentName) { SurfaceLevel = 0.0, Geometry = TargetCompartmentGeometry };
            targetManhole.Compartments.Add(targetCompartment);

            var sourceManhole = new Manhole(SourceManholeId);
            var sourceCompartment = new Compartment(SourceCompartmentName) { SurfaceLevel = 0.0, Geometry = SourceCompartmentGeometry };
            sourceManhole.Compartments.Add(sourceCompartment);

            var sewerConnection = new SewerConnection(SewerConnectionName)
            {
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment
            };

            network.Branches.Add(sewerConnection);
            network.Nodes.Add(sourceManhole);
            network.Nodes.Add(targetManhole);
            return network;
        }

        public static HydroNetwork CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneOrifice()
        {
            var network = new HydroNetwork();

            var targetManhole = new Manhole(TargetManholeId);
            var targetCompartment = new Compartment(TargetCompartmentName) { SurfaceLevel = 0.0, Geometry = TargetCompartmentGeometry };
            targetManhole.Compartments.Add(targetCompartment);

            var sourceManhole = new Manhole(SourceManholeId);
            var sourceCompartment = new Compartment(SourceCompartmentName) { SurfaceLevel = 0.0, Geometry = SourceCompartmentGeometry };
            sourceManhole.Compartments.Add(sourceCompartment);

            var orifice = new Orifice(OrificeName)
            {
                CrestLevel = 0.0,
                MaxDischarge = 0.0,
                Length = 0.0,
                WeirFormula = new GatedWeirFormula
                {
                    ContractionCoefficient = 0.0
                }
            };

            var sewerConnection = new SewerConnection(OrificeName)
            {
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment,
                LevelSource = 0.0,
                LevelTarget = 0.0,
                WaterType = SewerConnectionWaterType.None,
                SourceCompartmentName = SourceCompartmentName,
                TargetCompartmentName = TargetCompartmentName
            };
            sewerConnection.AddStructureToBranch(orifice);

            network.Branches.Add(sewerConnection);
            network.Nodes.Add(sourceManhole);
            network.Nodes.Add(targetManhole);
            return network;
        }

        public static HydroNetwork CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePipeWithACrossSection()
        {
            var network = new HydroNetwork();

            var targetManhole = new Manhole(TargetManholeId);
            var targetCompartment = new Compartment(TargetCompartmentName) { SurfaceLevel = 0.0, Geometry = TargetCompartmentGeometry };
            targetManhole.Compartments.Add(targetCompartment);

            var sourceManhole = new Manhole(SourceManholeId);
            var sourceCompartment = new Compartment(SourceCompartmentName) { SurfaceLevel = 0.0, Geometry = SourceCompartmentGeometry };
            sourceManhole.Compartments.Add(sourceCompartment);
            
            var crossSectionDefinition = new CrossSectionDefinitionStandard(CrossSectionStandardShapeArch.CreateDefault());
            crossSectionDefinition.Name = crossSectionDefinitionName;

            var pipe = new Pipe
            {
                Name = "myPipe",
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment,
                SourceCompartmentName = SourceCompartmentName,
                TargetCompartmentName = TargetCompartmentName,
                LevelSource = 0.0,
                LevelTarget = 0.0,
                WaterType = SewerConnectionWaterType.DryWater,
                CrossSection =  new CrossSection(crossSectionDefinition),
                CrossSectionDefinitionName = crossSectionDefinitionName
            };

            network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);
            network.Branches.Add(pipe);
            network.Nodes.Add(sourceManhole);
            network.Nodes.Add(targetManhole);
            return network;
        }

        public static HydroNetwork CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOnePump()
        {
            var network = new HydroNetwork();

            var targetManhole = new Manhole(TargetManholeId);
            var targetCompartment = new Compartment(TargetCompartmentName) { SurfaceLevel = 0.0, Geometry = TargetCompartmentGeometry };
            targetManhole.Compartments.Add(targetCompartment);

            var sourceManhole = new Manhole(SourceManholeId);
            var sourceCompartment = new Compartment(SourceCompartmentName) { SurfaceLevel = 0.0, Geometry = SourceCompartmentGeometry };
            sourceManhole.Compartments.Add(sourceCompartment);

            var pump = new Pump(PumpName)
            {
                DirectionIsPositive = false,
                Capacity = 0.0,
                StartDelivery = 0.0,
                StopDelivery = 0.0,
                StartSuction = 0.0,
                StopSuction = 0.0
            };

            var sewerConnection = new SewerConnection(PumpName)
            {
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment
            };
            sewerConnection.AddStructureToBranch(pump);

            network.Branches.Add(sewerConnection);
            network.Nodes.Add(sourceManhole);
            network.Nodes.Add(targetManhole);
            return network;
        }

        public static HydroNetwork CreateSewerNetwork_TwoManholesWithOneCompartmentEachAndOneWeir()
        {
            var network = new HydroNetwork();

            var targetManhole = new Manhole(TargetManholeId);
            var targetCompartment = new Compartment(TargetCompartmentName) { SurfaceLevel = 0.0, Geometry = TargetCompartmentGeometry };
            targetManhole.Compartments.Add(targetCompartment);

            var sourceManhole = new Manhole(SourceManholeId);
            var sourceCompartment = new Compartment(SourceCompartmentName) { SurfaceLevel = 0.0, Geometry = SourceCompartmentGeometry };
            sourceManhole.Compartments.Add(sourceCompartment);

            var weir = new Weir(WeirName)
            {
                FlowDirection = FlowDirection.None,
                CrestWidth = 0.0,
                CrestLevel = 0.0,
                WeirFormula = new SimpleWeirFormula
                {
                    CorrectionCoefficient = 0.0
                }
            };

            var sewerConnection = new SewerConnection(WeirName)
            {
                SourceCompartment = sourceCompartment,
                TargetCompartment = targetCompartment
            };
            sewerConnection.AddStructureToBranch(weir);

            network.Branches.Add(sewerConnection);
            network.Nodes.Add(sourceManhole);
            network.Nodes.Add(targetManhole);
            return network;
        }

        public static HydroNetwork CreateSewerNetwork_TwoManholesWithOneCompartmentEach()
        {
            var network = new HydroNetwork();
            
            var sourceManhole = new Manhole(SourceManholeId);
            var sourceCompartment = new Compartment(SourceCompartmentName) { SurfaceLevel = 0.0, Geometry = SourceCompartmentGeometry };
            sourceManhole.Compartments.Add(sourceCompartment);

            var targetManhole = new Manhole(TargetManholeId);
            var targetCompartment = new Compartment(TargetCompartmentName) { SurfaceLevel = 0.0, Geometry = TargetCompartmentGeometry };
            targetManhole.Compartments.Add(targetCompartment);

            network.Nodes.Add(sourceManhole);
            network.Nodes.Add(targetManhole);
            return network;
        }

        public static HydroNetwork CreateSewerNetwork_OneSharedCrossSection()
        {
            var network = new HydroNetwork();

            var crossSectionDefinition = CrossSectionDefinitionStandard.CreateDefault();
            crossSectionDefinition.Name = crossSectionDefinitionName;
            var csDefinitionStandard = crossSectionDefinition as CrossSectionDefinitionStandard;
            if (csDefinitionStandard != null) csDefinitionStandard.Shape.MaterialName = SewerProfileMapping.SewerProfileMaterial.Concrete.GetDescription();
            network.SharedCrossSectionDefinitions.Add(crossSectionDefinition);

            return network;
        }
    }
}