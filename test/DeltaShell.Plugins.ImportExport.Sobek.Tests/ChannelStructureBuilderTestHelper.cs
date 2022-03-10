using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.ImportExport.Sobek.Builders;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    public class ChannelStructureBuilderTestHelper
    {
        public static void SetStructureOnChannel(SobekStructureDefinition sobekStructureDefinition, Channel channel)
        {
            SetStructureOnChannel(null, "mapping", sobekStructureDefinition, channel, null);
        }

        public static void SetStructureOnChannel(string definitionId, string locationID, SobekStructureDefinition sobekStructureDefinition, Channel channel, SobekStructureFriction sobekStructureFriction)
        {
            sobekStructureDefinition.Id = definitionId ?? "1";
            var vertices = new List<Coordinate>
                               {
                                   new Coordinate(0, 0),
                                   new Coordinate(100, 0)
                               };

            
            GeometryFactory geometryFactory = new GeometryFactory();
            channel.Geometry = geometryFactory.CreateLineString(vertices.ToArray());

            string branchId = "1";
            Dictionary<string, IBranch> channels = new Dictionary<string, IBranch>
                                                        {
                                                            {branchId, channel}
                                                        };
            IEnumerable<SobekStructureLocation> locations = new List<SobekStructureLocation>
                                                             {
                                                                 new SobekStructureLocation
                                                                     {
                                                                         BranchID = branchId ,
                                                                         ID = locationID,
                                                                         Name = "struct loc 1"
                                                                     }
                                                             };

            IEnumerable<SobekStructureDefinition> definitions = new List<SobekStructureDefinition>
                                                                    {
                                                                        sobekStructureDefinition
                                                                    };
            var mapping = new SobekStructureMapping
                              {
                                  DefinitionId = definitionId ?? "1",
                                  StructureId = locationID,
                                  Name = "struct map 1"
                              };
            IEnumerable<SobekStructureMapping> mappings = new List<SobekStructureMapping>
                                                              {
                                                                  mapping
                                                              };

            List<SobekStructureFriction> sobekStructureFrictions = new List<SobekStructureFriction>();
            List<SobekExtraResistance> sobekExtraFrictions = new List<SobekExtraResistance>();
            if (sobekStructureFriction != null)
            {
                sobekStructureFrictions.Add(sobekStructureFriction);
            }

            var builder = new ChannelStructureBuilder(channels, locations, definitions, mappings,
                                                      new List<SobekCompoundStructure>(), null,
                                                      new List<SobekValveData>(), sobekStructureFrictions,
                                                      sobekExtraFrictions);
            builder.SetStructuresOnChannels();
        }
    }
}
