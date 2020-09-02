using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.TestUtils;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using SharpMapTestUtils;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests
{
    public static class FlowFMTestHelper
    {
        private static string expectedEnclosurePolFileContent = "\r\n    6    2\r\n                      10                      10\r\n                      22                      10\r\n                      20                      15\r\n                      20                      20\r\n                      12                      20\r\n                      10                      15\r\n";

        public static string GetExpectedEnclosurePolFileContent(string featureName)
        {
            return string.Concat(featureName, expectedEnclosurePolFileContent);
        }

        public static Polygon GetValidGeometryForEnclosureExample()
        {
            /* Wrapper emulating the Interior ring. */
            return new Polygon(GetJustLinearRing());
        }

        public static Polygon GetInvalidGeometryForEnclosureExample()
        {
            /* Wrapper emulating the Interior ring. */
            return new Polygon(new LinearRing(new[]
            {
                /* 
                       (10.0, 20.0)   O------O (20.0, 20.0)             
                                       \    /   
                                        \  /   
                                         \/   
                                         /\
                                        /  \
                                       /    \
                     (10.0, 10.0)     O------O (20.0, 10.0)
                */
                new Coordinate(10.0, 10.0),
                new Coordinate(20.0, 20.0),
                new Coordinate(10.0, 20.0),
                new Coordinate(20.0, 10.0),
                new Coordinate(10.0, 10.0)
            }));
        }

        public static LinearRing GetJustLinearRing()
        {
            return new LinearRing(new[]
            {
                /* 
                       (12.0, 20.0) O----------O (20.0, 20.0)             
                                  /           |
                                 /            |
                     (10.0, 15.0) O             O (20.0, 15.0)
                                |              \
                                |               \
                     (10.0, 10.0) O----------------O (22.0, 10.0)
                */
                new Coordinate(10.0, 10.0),
                new Coordinate(22.0, 10.0),
                new Coordinate(20.0, 15.0),
                new Coordinate(20.0, 20.0),
                new Coordinate(12.0, 20.0),
                new Coordinate(10.0, 15.0),
                new Coordinate(10.0, 10.0)
            });
        }

        public static GroupableFeature2DPolygon CreateFeature2DPolygonFromGeometry(string featureName, Geometry geometry)
        {
            var enclosureFeature = new GroupableFeature2DPolygon()
            {
                Name = featureName,
                Geometry = geometry
            };
            return enclosureFeature;
        }

        public static bool ContainsError(this ValidationReport report, string errorMessage)
        {
            return ContainsValidationIssue(report, errorMessage, ValidationSeverity.Error);
        }

        public static bool ContainsWarning(this ValidationReport report, string errorMessage)
        {
            return report.ContainsValidationIssue(errorMessage, ValidationSeverity.Warning);
        }

        public static WaterFlowFMModel GetSmallestValidModel(TemporaryDirectory temp)
        {
            string mduFilePath = Path.Combine(temp.Path, "model.mdu");

            var model = new WaterFlowFMModel {Grid = UnstructuredGridTestHelper.GenerateRegularGrid(2, 2, 2, 2)};
            model.ExportTo(mduFilePath);
            model.ReloadGrid(true, true);
            model.ImportFromMdu(mduFilePath);

            model.StopTime = model.StartTime.AddMinutes(20);
            model.ModelDefinition.GetModelProperty(GuiProperties.WriteMapFile).Value = false;
            model.ModelDefinition.GetModelProperty(GuiProperties.WriteHisFile).Value = false;

            ValidationReport report = model.Validate();
            Assert.AreEqual(0, report.AllErrors.Count(), "Model has errors in the validation report.");

            return model;
        }

        private static bool ContainsValidationIssue(this ValidationReport report, string errorMessage, ValidationSeverity severity)
        {
            foreach (ValidationIssue issue in report.Issues.Where(i => i.Severity == severity))
            {
                Console.WriteLine(issue.Message);

                if (issue.Message == errorMessage)
                {
                    return true;
                }
            }

            return report.SubReports.Any(subReport => ContainsValidationIssue(subReport, errorMessage, severity));
        }
    }
}