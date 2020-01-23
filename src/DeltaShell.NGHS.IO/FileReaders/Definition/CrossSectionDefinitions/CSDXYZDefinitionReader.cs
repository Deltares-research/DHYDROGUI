using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using DeltaShell.NGHS.IO.Helpers;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace DeltaShell.NGHS.IO.FileReaders.Definition.CrossSectionDefinitions
{
    class CSDXYZDefinitionReader : CrossSectionDefinitionReaderBase
    {
        public override ICrossSectionDefinition ReadDefinition(IDelftIniCategory category)
        {
            var crossSectionDefinition = new CrossSectionDefinitionXYZ();
            SetCommonCrossSectionDefinitionsProperties(crossSectionDefinition, category);

            var xCoorList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.XCoors.Key);
            var yCoorList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.YCoors.Key);
            var zCoorList = category.ReadPropertiesToListOfType<double>(DefinitionPropertySettings.ZCoors.Key);

            var xyzCount = category.ReadProperty<int>(DefinitionPropertySettings.XYZCount.Key);

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