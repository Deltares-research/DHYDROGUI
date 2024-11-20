using System;
using System.Collections.Concurrent;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Structures;

namespace DelftTools.Hydro.SewerFeatures
{
    public class SewerImporterHelper{
        public ConcurrentDictionary<string, IManhole> ManholesByManholeName { get; } = new ConcurrentDictionary<string, IManhole>(StringComparer.InvariantCultureIgnoreCase);
        public ConcurrentDictionary<string, IManhole> ManholesByCompartmentName { get; } = new ConcurrentDictionary<string, IManhole>(StringComparer.InvariantCultureIgnoreCase);
        public ConcurrentDictionary<string, ISewerConnection> SewerConnectionsByName { get; } = new ConcurrentDictionary<string, ISewerConnection>(StringComparer.InvariantCultureIgnoreCase);
        public ConcurrentDictionary<string, CrossSectionDefinitionProxy> CrossSectionDefinitionsByPipe { get; } = new ConcurrentDictionary<string, CrossSectionDefinitionProxy>(StringComparer.InvariantCultureIgnoreCase);
        public ConcurrentDictionary<string, SewerProfileMapping.SewerProfileMaterial> SewerProfileMaterialsByPipe { get; } = new ConcurrentDictionary<string, SewerProfileMapping.SewerProfileMaterial>(StringComparer.InvariantCultureIgnoreCase);
        public ConcurrentQueue<ICrossSection> PipeCrossSections { get; } = new ConcurrentQueue<ICrossSection>();
        public ConcurrentQueue<ICompositeBranchStructure> CompositeBranchStructures { get; } = new ConcurrentQueue<ICompositeBranchStructure>();
    }
}