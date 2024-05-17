using System.Text.RegularExpressions;
using DelftTools.Hydro.Area.Objects;
using DelftTools.Hydro.Area.Objects.StructureObjects;
using DelftTools.Hydro.Area.Objects.StructureObjects.StructureFormulas;
using DelftTools.Hydro.GroupableFeatures;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public static class WaterFlowFMMduFileTestHelper
    {
        public static GroupableFeature2DPoint GetNewGroupableFeature2DPoint(string groupName, string featureName, bool isDefaultGroup = true)
        {
            return new GroupableFeature2DPoint
            {
                GroupName = groupName,
                Name = featureName,
                Geometry = new Point(new Coordinate(0, 100)),
                IsDefaultGroup = isDefaultGroup
            };
        }

        public static GroupablePointFeature GetNewGroupablePointFeature(string groupName)
        {
            return new GroupablePointFeature
            {
                GroupName = groupName,
                Geometry = new Point(new Coordinate(0, 100))
            };
        }

        public static GroupableFeature2DPolygon GetNewGroupableFeature2DPolygon(string groupName, string featureName)
        {
            return new GroupableFeature2DPolygon
            {
                GroupName = groupName,
                Name = featureName,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100),
                    new Coordinate(50, 50),
                    new Coordinate(0, 0)
                })
            };
        }

        public static LandBoundary2D GetNewLandBoundary2D(string groupName, string featureName)
        {
            return new LandBoundary2D
            {
                GroupName = groupName,
                Name = featureName,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100),
                    new Coordinate(50, 50)
                })
            };
        }

        public static Structure GetNewWeir2DWithGateFormula(string groupName, string featureName)
        {
            return new Structure()
            {
                GroupName = groupName,
                Name = featureName,
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, 100),
                    new Coordinate(50, 50)
                }),
                Formula = new SimpleGateFormula()
            };
        }

        /// <summary>
        /// Asserts that the <paramref name="inputText"/> contains an mdu line with the
        /// <paramref name="propertyName"/>, the <paramref name="propertyValue"/>
        /// and the <paramref name="propertyComment"/>, regardless of the number of white characters./>.
        /// </summary>
        /// <param name="inputText">The input text.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="propertyComment">The property comment.</param>
        /// <example>
        /// A typical mdu line looks like this:
        /// 'PropertyName       = Value     # Comment'
        /// </example>
        public static void AssertContainsMduLine(string inputText, string propertyName, string propertyValue, string propertyComment = null)
        {
            propertyName = Regex.Escape(propertyName);
            propertyValue = Regex.Escape(propertyValue);

            var searchPattern = $@"{propertyName}\s*=\s*{propertyValue}";
            if (propertyComment != null)
            {
                propertyComment = Regex.Escape(propertyComment);
                searchPattern += $@"\s*{propertyComment}";
            }

            var regex = new Regex(searchPattern);

            Assert.IsTrue(regex.IsMatch(inputText),
                          $"File did not contain expected text: '{propertyName} = {propertyValue} {propertyComment}'");
        }
    }
}