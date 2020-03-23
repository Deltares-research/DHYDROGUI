using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    public abstract class CrossSectionDefinitionReaderBase : IDefinitionReader<ICrossSectionDefinition>
    {
        public abstract ICrossSectionDefinition ReadDefinition(IDelftIniCategory category);

        protected static void SetCommonCrossSectionDefinitionsProperties(ICrossSectionDefinition crossSectionDefinition, IDelftIniCategory category)
        {
            crossSectionDefinition.Name = category.ReadProperty<string>(DefinitionPropertySettings.Id.Key);
            crossSectionDefinition.Thalweg = category.ReadProperty<double>(DefinitionPropertySettings.Thalweg.Key);
            if (crossSectionDefinition is CrossSectionDefinitionStandard standard &&
                standard.Shape is ICrossSectionStandardShapeOpenClosed shape && category.Properties.Any(p => p.Name.Equals(DefinitionPropertySettings.Closed.Key,StringComparison.InvariantCultureIgnoreCase)))
            {
                shape.Closed = category.ReadProperty<string>(DefinitionPropertySettings.Closed.Key).Equals("yes", StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}