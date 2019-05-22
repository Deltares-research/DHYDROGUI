using System;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Helpers;
using DelftTools.TestUtils;
using DeltaShell.Plugins.Fews.Assemblers;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Extensions.Networks;
using NUnit.Framework;
using SharpMap.Converters.WellKnownText;
using Point = NetTopologySuite.Geometries.Point;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace DeltaShell.Plugins.Fews.Tests.Assemblers
{
    // DTO = Data transfer object (see Martin Fowler: Design Patterns for Enterprise Architectures)
    
    // loop times
    //var results = networkCoverage.GetValues<double>(new VariableValueFilter<DateTime>(networkCoverage.Time, time)); // 1d
    // loop over all intersected locations
    //double v = results[locationIndex];

    [TestFixture]
    public class BranchesComplexTypeAssemblerTest
    {
        [Test]
        [ExpectedException(typeof (InvalidOperationException))]
        public void Assemble_WithoutNetworkCoverage_Throws()
        {
            var assembler = new BranchesComplexTypeAssembler();
            assembler.AssembleDto("dummy", null);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Assemble_NetworkCoverageWithoutNetwork_Throws()
        {
            var assembler = new BranchesComplexTypeAssembler();
            assembler.AssembleDto("dummy", null);
        }

        [Test]
        public void Assemble_UsingNetworkWithOneBranch_DtoHasValues()
        {
            // setup
            var network = new Network();

            const int startChainage = 10;
            const int endChainage = 90;
            const double x1 = 0;
            const double x2 = 100;

            var node1 = new Node { Geometry = new Point(x1, 0) };
            var node2 = new Node { Geometry = new Point(x2, 0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);

            var branch1 = new Branch
            {
                Name = "my_branch",
                Geometry = new LineString(new[] { new Coordinate(x1, 0), new Coordinate(x2, 0), }),
                Source = node1,
                Target = node2
            };

            network.Branches.Add(branch1);

            var networkCoverage = new NetworkCoverage
            {
                Network = network
            };

            const int offset1 = startChainage;
            const int offset2 = endChainage;

            var location1 = new NetworkLocation(branch1, offset1);
            var location2 = new NetworkLocation(branch1, offset2);

            networkCoverage.Locations.Values.Add(location1);
            networkCoverage.Locations.Values.Add(location2);

            const string route1Name = "route_1";
            var route1 = new Route
            {
                Name = route1Name,
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations
            };
            route1.Locations.Values.Add(location1);
            route1.Locations.Values.Add(location2);

            var assembler = new BranchesComplexTypeAssembler
            {
                NetworkCoverage = networkCoverage,
                Route = route1
            };
            BranchesComplexType branchesComplexType = new BranchesComplexType();
            // call
            assembler.AssembleDto("dummy", branchesComplexType);

            //validate xml 
            branchesComplexType.geoDatum =
                BranchesComplexType.GetXmlEnumAttributeValueFromEnum(
                    BranchesComplexType.geoDatumEnumStringType.RijksDriehoekstelsel);
            var xml = branchesComplexType.Serialize();
            XmlValidate(xml);
            // checks

            var doc =  XDocument.Parse(xml);
            Assert.That(GetXmlElement(doc,"geoDatum").Value,Is.EqualTo(BranchesComplexType.GetXmlEnumAttributeValueFromEnum(
                    BranchesComplexType.geoDatumEnumStringType.RijksDriehoekstelsel)));
            Assert.That(GetXmlElement(doc,"branch").Attribute("id").Value,Is.EqualTo(route1Name+"dummy"));
            Assert.That(GetXmlElement(doc,"branchName").Value,Is.EqualTo(route1Name+"dummy"));
            Assert.That(GetXmlElement(doc,"startChainage").Value,Is.EqualTo("0"));
            Assert.That(GetXmlElement(doc,"endChainage").Value,Is.EqualTo((endChainage-startChainage).ToString()));
            Assert.That(GetXmlElement(doc,"upNode").Value,Is.StringStarting(branch1.Name+"_"+startChainage));
            Assert.That(GetXmlElement(doc,"downNode").Value,Is.StringStarting(branch1.Name+"_"+endChainage));

            Assert.That(GetXmlElement(doc, "pt", 0).Attribute("x").Value, Is.EqualTo(startChainage.ToString()));
            Assert.That(GetXmlElement(doc, "pt", 0).Attribute("y").Value, Is.EqualTo("0"));
            Assert.That(GetXmlElement(doc, "pt", 0).Attribute("chainage").Value, Is.EqualTo("0"));
            Assert.That(GetXmlElement(doc, "pt", 0).Attribute("label").Value, Is.StringStarting(branch1.Name + "_" + startChainage));

            Assert.That(GetXmlElement(doc, "pt", 1).Attribute("x").Value, Is.EqualTo(endChainage.ToString()));
            Assert.That(GetXmlElement(doc, "pt", 1).Attribute("y").Value, Is.EqualTo("0"));
            Assert.That(GetXmlElement(doc, "pt", 1).Attribute("chainage").Value, Is.EqualTo((endChainage-startChainage).ToString()));
            Assert.That(GetXmlElement(doc, "pt", 1).Attribute("label").Value, Is.StringStarting(branch1.Name + "_" + endChainage));
        }

        private XElement GetXmlElement(XDocument doc, string element, int index = -1)
        {
            XElement xElement;
            if (index == -1)
            {
                xElement = doc.Descendants().SingleOrDefault(p => p.Name.LocalName == element);
            }
            else
            {
                xElement = doc.Descendants().Where(p => p.Name.LocalName == element).ElementAt(index);
            }
            Assert.NotNull(xElement, "not found element " + element);
            return xElement;
        }
        [Test]
        public void Assemble_UsingRoute_DtoHasCorrectStartAndEndChainage()
        {
            // 
            //
            //
            //

            // setup network
            var network = new Network();

            const double xNode1 = 0d;
            const double xNode2 = 100d;
            const double xNode3 = 200d;

            var node1 = new Node { Geometry = new Point(xNode1, 0) };
            var node2 = new Node { Geometry = new Point(xNode2, 0) };
            var node3 = new Node { Geometry = new Point(xNode3, 0) };
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Nodes.Add(node3);

            const string branch1Name = "Branch1";
            var branch1 = new Branch
            {
                Name = branch1Name,
                Geometry = GeometryFromWKT.Parse("LINESTRING (" + xNode1 + " 0, " + xNode2 + " 0)"),
                Source = node1,
                Target = node2
            };
            const string branch2Name = "Branch2";
            var branch2 = new Branch
            {
                Name = branch2Name,
                Geometry = GeometryFromWKT.Parse("LINESTRING (" + xNode2 + " 0, " + xNode3 + " 0)"),
                Source = node2,
                Target = node3
            };
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            // setup network coverage containing points for route
            const double x1InRoute1 = 25d;
            const double x2InRoute1and2 = 75d;
            const double x2InRoute2 = 40d;
            const double x3InRoute2 = 80d;
            var networkCoverage = new NetworkCoverage
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.SegmentPerLocation
            };
            networkCoverage.Locations.Values.Add(new NetworkLocation(branch1, x1InRoute1));
            networkCoverage.Locations.Values.Add(new NetworkLocation(branch1, x2InRoute1and2));
            networkCoverage.Locations.Values.Add(new NetworkLocation(branch1, x2InRoute2));
            networkCoverage.Locations.Values.Add(new NetworkLocation(branch1, x3InRoute2));

            const double startOfRoute1 = 10d; // on branch 1
            const double endOfRoute1 = 90d; // on branch 1

            const double startOfRoute2 = 40d; // on branch 1
            const double endOfRoute2 = 90d;  // on branch 2

            const string route1Name = "route_1";
            var route1 = new Route
            {
                Name = route1Name,
                Network = network, 
                SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations
            };
            route1.Locations.Values.Add(new NetworkLocation(branch1, startOfRoute1));
            route1.Locations.Values.Add(new NetworkLocation(branch1, endOfRoute1));

            var assembler = new BranchesComplexTypeAssembler
            {
                NetworkCoverage = networkCoverage,
                Route = route1
            };
            BranchesComplexType branchesComplexType = new BranchesComplexType();
            assembler.AssembleDto("dummypoints", branchesComplexType);
            //validate xml 
            branchesComplexType.geoDatum =
                BranchesComplexType.GetXmlEnumAttributeValueFromEnum(
                    BranchesComplexType.geoDatumEnumStringType.RijksDriehoekstelsel);
            XmlValidate(branchesComplexType.Serialize());

            // checks
            Assert.IsTrue(branchesComplexType.branch.Count == 1);
            Assert.AreEqual(route1Name + "dummypoints", branchesComplexType.branch[0].id);
            Assert.AreEqual(route1Name + "dummypoints", branchesComplexType.branch[0].branchName);
            Assert.AreEqual(0d, branchesComplexType.branch[0].startChainage);
            double chainageLengthRoute1 = endOfRoute1 - startOfRoute1;
            Assert.AreEqual(chainageLengthRoute1, branchesComplexType.branch[0].endChainage);

            var route2 = new Route
            {
                Network = network,
                SegmentGenerationMethod = SegmentGenerationMethod.RouteBetweenLocations
            };
            route2.Locations.Values.Add(new NetworkLocation(branch1, startOfRoute2));
            route2.Locations.Values.Add(new NetworkLocation(branch2, endOfRoute2));

            assembler = new BranchesComplexTypeAssembler
            {
                NetworkCoverage = networkCoverage,
                Route = route2
            };
            branchesComplexType = new BranchesComplexType();
            assembler.AssembleDto("dummypoints", branchesComplexType);

            Assert.IsTrue(branchesComplexType.branch.Count == 1);
            Assert.AreEqual(0d, branchesComplexType.branch[0].startChainage);
            double chainageLengthRoute2 = branch1.Length - startOfRoute2 + endOfRoute2;
            Assert.AreEqual(chainageLengthRoute2, branchesComplexType.branch[0].endChainage);
        }

        private void XmlValidate(string xmlString)
        {
            var stringReader = new System.IO.StringReader(xmlString);
            // Set the validation settings.
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(stringReader, settings);

            // Parse the file. 
            while (reader.Read()) ;

        }

        // Display any warnings or errors.
        private static void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            if (args.Severity == XmlSeverityType.Warning)
                Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
            else
                Assert.Fail("\tValidation error: " + args.Message);
                //Console.WriteLine("\tValidation error: " + args.Message);

        }

        [Test]
        [Category(TestCategory.WorkInProgress)]
        //[Ignore("Need to implement a good unit test using a route")]
        public void Assemble_UsingRoute_DtoHasCorrectChainageSequence()
        {
            // setup
            var network = new HydroNetwork();

            const int startChainage = 0;
            const int endChainage = 100;
            const double x1 = 0;
            const double x2 = 100;

            var node1 = new Node { Geometry = new Point(x1, 0), Name = "Node1"};
            var node2 = new Node { Geometry = new Point(x2, 0), Name = "Node2"};
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            const string branchName = "Branch1";
            var branch1 = new Branch
            {
                Name = branchName,
                Geometry = new LineString(new Coordinate[] { new Coordinate(x1, 0), new Coordinate(x2, 0), }),
                Source = node1,
                Target = node2
            };

            network.Branches.Add(branch1);

            var networkCoverage = new NetworkCoverage
            {
                Network = network
            };

            const int offset1 = startChainage;
            const int offset2 = (endChainage - startChainage) / 2;
            const int offset3 = endChainage;

            var location1 = new NetworkLocation(branch1, offset1);
            var location2 = new NetworkLocation(branch1, offset2) { Name = "testLocation" };
            var location3 = new NetworkLocation(branch1, offset3);

            networkCoverage.Locations.Values.Add(location1);
            networkCoverage.Locations.Values.Add(location2);
            networkCoverage.Locations.Values.Add(location3);

            var route = HydroNetworkHelper.AddNewRouteToNetwork(network);
            route.Locations.Values.Add(location1);
            route.Locations.Values.Add(location2);
            route.Locations.Values.Add(location3);

            var assembler = new BranchesComplexTypeAssembler
            {
                NetworkCoverage = networkCoverage,
                Route = route
            };

            BranchesComplexType branchesComplexType = new BranchesComplexType();
            // call
            assembler.AssembleDto("dummy", branchesComplexType);
            
            //validate xml 
            branchesComplexType.geoDatum =
                BranchesComplexType.GetXmlEnumAttributeValueFromEnum(
                    BranchesComplexType.geoDatumEnumStringType.RijksDriehoekstelsel);
            XmlValidate(branchesComplexType.Serialize());
            
            // checks
            Assert.IsTrue(branchesComplexType.branch.Count == 1);
            Assert.AreEqual(startChainage, branchesComplexType.branch[0].startChainage);
            Assert.AreEqual(endChainage, branchesComplexType.branch[0].endChainage);

            // checks
            BranchComplexType branchItem = branchesComplexType.branch[0];
            NodePointComplexType[] nodes = branchItem.pt.ToArray();

            Assert.IsNotNull(nodes);
            Assert.AreEqual(networkCoverage.Locations.Values.Count, nodes.Length);

            NodePointComplexType firstNode = nodes[0];
            Assert.AreEqual(firstNode.chainage, location1.Chainage);
            Assert.AreEqual(firstNode.label, location1.Name);
            Assert.AreEqual(firstNode.x, x1);
            Assert.AreEqual(firstNode.y, 0);
            Assert.AreEqual(firstNode.z, double.NaN);

            NodePointComplexType secondNode = nodes[1];
            Assert.AreEqual(secondNode.chainage, location2.Chainage);
            Assert.AreEqual(secondNode.label, location2.Name);
            Assert.AreEqual(secondNode.x, offset2);
            Assert.AreEqual(secondNode.y, 0);
            Assert.AreEqual(secondNode.z, double.NaN);

            NodePointComplexType thirdNode = nodes[2];
            Assert.AreEqual(thirdNode.chainage, location3.Chainage);
            Assert.AreEqual(thirdNode.label, location3.Name);
            Assert.AreEqual(thirdNode.x, x2);
            Assert.AreEqual(thirdNode.y, 0);
            Assert.AreEqual(thirdNode.z, double.NaN);
        }
    }
}