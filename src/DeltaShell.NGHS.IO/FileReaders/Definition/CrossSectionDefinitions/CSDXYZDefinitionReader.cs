using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using DHYDRO.Common.IO.Ini;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDXYZDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IniSection iniSection)
        {
            var crossSectionDefinition = new CrossSectionDefinitionXYZ();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, iniSection);

            var xCoorList = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.XCoors.Key);
            var yCoorList = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.YCoors.Key);
            var zCoorList = iniSection.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.ZCoors.Key);

            var xyzCount = iniSection.ReadProperty<int>(DefinitionPropertySettings.XYZCount.Key);

            if (xyzCount != xCoorList.Count || xyzCount != yCoorList.Count ||
                xyzCount != zCoorList.Count)
            {
                var errorMessage = "xyz count property is not equal to number of x, y or z coordinates or delta z storage"; 
                throw new FileReadingException(errorMessage);
            }

            var geometryCoors = new Coordinate[xyzCount];
            for (var i = 0; i < xyzCount; i++)
            {
                geometryCoors[i] = new Coordinate(xCoorList[i], yCoorList[i], zCoorList[i]);
            }

            crossSectionDefinition.Geometry = new LineString(geometryCoors);

            return crossSectionDefinition;
        }
    }
}