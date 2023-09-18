using System;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.CrossSections.StandardShapes;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    public abstract class CrossSectionDefinitionReaderBase : IDefinitionReader<ICrossSectionDefinition>
    {
        public abstract ICrossSectionDefinition ReadDefinition(IniSection iniSection);

        protected static void SetCommonCrossSectionDefinitionsProperties(ICrossSectionDefinition crossSectionDefinition, IniSection iniSection)
        {
            crossSectionDefinition.Name = iniSection.ReadProperty<string>(DefinitionPropertySettings.Id.Key);
            crossSectionDefinition.Thalweg = iniSection.ReadProperty<double>(DefinitionPropertySettings.Thalweg.Key);
            if (crossSectionDefinition is CrossSectionDefinitionStandard standard &&
                standard.Shape is ICrossSectionStandardShapeOpenClosed shape && iniSection.Properties.Any(p => p.Key.Equals(DefinitionPropertySettings.Closed.Key,StringComparison.InvariantCultureIgnoreCase)))
            {
                shape.Closed = iniSection.ReadProperty<string>(DefinitionPropertySettings.Closed.Key).Equals("yes", StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}