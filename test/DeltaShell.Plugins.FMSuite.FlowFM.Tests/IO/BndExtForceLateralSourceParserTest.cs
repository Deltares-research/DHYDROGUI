using System;
using System.Collections.Generic;
using System.IO;
using DelftTools.Hydro;
using DelftTools.Hydro.SewerFeatures;
using DelftTools.Hydro.Structures;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects;
using DeltaShell.NGHS.IO.FileReaders.Boundary;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using DHYDRO.Common.Logging;
using GeoAPI.Extensions.Networks;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Networks;
using NetTopologySuite.Geometries;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.IO
{
    public class BndExtForceLateralSourceParserTest
    {
        [TestCase(null)]
        [TestCase("")]
        public void Constructor_SourceFilePathNullOrEmpty_ThrowsArgumentException(string sourceFilePath)
        {
            // Call
            void Call() => new BndExtForceLateralSourceParser(sourceFilePath, Substitute.For<IHydroNetwork>(), false, false, Substitute.For<IBoundaryFileReader>());

            // Assert
            var e = Assert.Throws<ArgumentException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("sourceFilePath"));
        }

        [Test]
        [TestCaseSource(nameof(ArgumentNullCases))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(INetwork network, IBoundaryFileReader boundaryFileReader, string expParamName)
        {
            // Call
            void Call() => new BndExtForceLateralSourceParser("path/to/some.file", network, false, false, boundaryFileReader);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo(expParamName));
        }

        [Test]
        public void Parse_SectionNull_ThrowsArgumentNullException()
        {
            // Setup
            var parser = new BndExtForceLateralSourceParser("path/to/some.file", Substitute.For<INetwork>(), false, false, Substitute.For<IBoundaryFileReader>());

            // Call
            void Call() => parser.Parse(null);

            // Assert
            var e = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(e.ParamName, Is.EqualTo("section"));
        }

        [TestCaseSource(nameof(Parse_OnPipeCompartmentCases))]
        public void Parse_OnPipeCompartment_ReturnsCorrectLateralSourceData(IPipe pipe1, IPipe pipe2, string nodeId,
                                                                            IPipe expPipe, double expChainage, ICompartment expCompartment)
        {
            // Setup
            var category = Substitute.For<ILateralSourceExtSection>();
            category.Id.Returns("lateral_source_id");
            category.Name.Returns("lateral_source_name");
            category.NodeName.Returns(nodeId);
            category.Discharge.Returns(4.56);

            var network = new HydroNetwork();
            network.Branches.Add(pipe1);
            network.Branches.Add(pipe2);

            var boundaryFileReader = Substitute.For<IBoundaryFileReader>();

            var parser = new BndExtForceLateralSourceParser("path/to/some.file", network, false, false, boundaryFileReader);

            // Call
            Model1DLateralSourceData lateralSourceData = parser.Parse(category);

            // Assert
            Assert.That(lateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowConstant));
            Assert.That(lateralSourceData.Flow, Is.EqualTo(4.56));
            Assert.That(lateralSourceData.Compartment, Is.SameAs(expCompartment));

            LateralSource lateralSource = lateralSourceData.Feature;
            Assert.That(lateralSource.Branch, Is.SameAs(expPipe));
            Assert.That(lateralSource.Chainage, Is.EqualTo(expChainage));
            Assert.That(lateralSource.Name, Is.EqualTo("lateral_source_id"));
            Assert.That(lateralSource.LongName, Is.EqualTo("lateral_source_name"));
            Assert.That(lateralSource.Geometry, Is.TypeOf<Point>());
            Assert.That(lateralSource.Geometry.InteriorPoint.X, Is.EqualTo(expPipe.Geometry.InteriorPoint.X + expChainage));
            Assert.That(lateralSource.Geometry.InteriorPoint.Y, Is.EqualTo(0));
        }

        [TestCaseSource(nameof(Parse_OnBranchNodeCases))]
        public void Parse_OnBranchNode_ReturnsCorrectLateralSourceData(IBranch branch1, IBranch branch2, string nodeId,
                                                                       IBranch expBranch, double expChainage)
        {
            // Setup
            var category = Substitute.For<ILateralSourceExtSection>();
            category.Id.Returns("lateral_source_id");
            category.Name.Returns("lateral_source_name");
            category.NodeName.Returns(nodeId);
            category.Discharge.Returns(4.56);

            var network = new HydroNetwork();
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            var boundaryFileReader = Substitute.For<IBoundaryFileReader>();

            var parser = new BndExtForceLateralSourceParser("path/to/some.file", network, false, false, boundaryFileReader);

            // Call
            Model1DLateralSourceData lateralSourceData = parser.Parse(category);

            // Assert
            Assert.That(lateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowConstant));
            Assert.That(lateralSourceData.Flow, Is.EqualTo(4.56));
            Assert.That(lateralSourceData.Compartment, Is.Null);

            LateralSource lateralSource = lateralSourceData.Feature;
            Assert.That(lateralSource.Branch, Is.SameAs(expBranch));
            Assert.That(lateralSource.Chainage, Is.EqualTo(expChainage));
            Assert.That(lateralSource.Name, Is.EqualTo("lateral_source_id"));
            Assert.That(lateralSource.LongName, Is.EqualTo("lateral_source_name"));
            Assert.That(lateralSource.Geometry, Is.TypeOf<Point>());
            Assert.That(lateralSource.Geometry.InteriorPoint.X, Is.EqualTo(expBranch.Geometry.InteriorPoint.X + expChainage));
            Assert.That(lateralSource.Geometry.InteriorPoint.Y, Is.EqualTo(0));
        }

        [TestCaseSource(nameof(Parse_OnBranchChainageCases))]
        public void Parse_OnBranchChainage_ReturnsCorrectLateralSourceData(IBranch branch1, IBranch branch2, string branchId,
                                                                           IBranch expBranch)
        {
            // Setup
            var category = Substitute.For<ILateralSourceExtSection>();
            category.Id.Returns("lateral_source_id");
            category.Name.Returns("lateral_source_name");
            category.BranchName.Returns(branchId);
            category.Chainage.Returns(50);
            category.Discharge.Returns(4.56);

            var network = new HydroNetwork();
            network.Branches.Add(branch1);
            network.Branches.Add(branch2);

            var boundaryFileReader = Substitute.For<IBoundaryFileReader>();

            var parser = new BndExtForceLateralSourceParser("path/to/some.file", network, false, false, boundaryFileReader);

            // Call
            Model1DLateralSourceData lateralSourceData = parser.Parse(category);

            // Assert
            Assert.That(lateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowConstant));
            Assert.That(lateralSourceData.Flow, Is.EqualTo(4.56));
            Assert.That(lateralSourceData.Compartment, Is.Null);

            LateralSource lateralSource = lateralSourceData.Feature;
            Assert.That(lateralSource.Branch, Is.SameAs(expBranch));
            Assert.That(lateralSource.Chainage, Is.EqualTo(50));
            Assert.That(lateralSource.Name, Is.EqualTo("lateral_source_id"));
            Assert.That(lateralSource.LongName, Is.EqualTo("lateral_source_name"));
            Assert.That(lateralSource.Geometry, Is.TypeOf<Point>());
            Assert.That(lateralSource.Geometry.InteriorPoint.X, Is.EqualTo(expBranch.Geometry.InteriorPoint.X + 50));
            Assert.That(lateralSource.Geometry.InteriorPoint.Y, Is.EqualTo(0));
        }

        [Test]
        public void Parse_RealTimeDischarge_ReturnsCorrectLateralSourceData()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var category = Substitute.For<ILateralSourceExtSection>();
                category.Id.Returns("lateral_source_id");
                category.Name.Returns("lateral_source_name");
                category.Discharge.Returns(double.NaN);
                category.DischargeFile.Returns((string) null);
                category.NodeName.Returns("node_id_2");

                IBranch branch = CreateBranch(100, 0, 200, 0, "node_id_1", "node_id_2");

                var network = new HydroNetwork();
                network.Branches.Add(branch);

                string sourceFilePath = Path.Combine(temp.Path, "some_file_bnd.ext");
                var boundaryFileReader = Substitute.For<IBoundaryFileReader>();
                var logHandler = Substitute.For<ILogHandler>();
                var parser = new BndExtForceLateralSourceParser(sourceFilePath, network, false, false, boundaryFileReader, logHandler);

                // Call
                Model1DLateralSourceData lateralSourceData = parser.Parse(category);

                // Assert
                Assert.That(lateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowRealTime));
                Assert.That(lateralSourceData.Flow, Is.EqualTo(0));
                Assert.That(lateralSourceData.Compartment, Is.Null);
                Assert.That(lateralSourceData.Data, Is.Not.Null);

                LateralSource lateralSource = lateralSourceData.Feature;
                Assert.That(lateralSource.Branch, Is.SameAs(branch));
                Assert.That(lateralSource.Chainage, Is.EqualTo(100));
                Assert.That(lateralSource.Name, Is.EqualTo("lateral_source_id"));
                Assert.That(lateralSource.LongName, Is.EqualTo("lateral_source_name"));
                Assert.That(lateralSource.Geometry, Is.TypeOf<Point>());
                Assert.That(lateralSource.Geometry.InteriorPoint.X, Is.EqualTo(200));
                Assert.That(lateralSource.Geometry.InteriorPoint.Y, Is.EqualTo(0));
            }
        }

        [Test]
        public void Parse_DischargeFromFile_ReturnsCorrectLateralSourceData()
        {
            using (var temp = new TemporaryDirectory())
            {
                // Setup
                var category = Substitute.For<ILateralSourceExtSection>();
                category.Id.Returns("lateral_source_id");
                category.Name.Returns("lateral_source_name");
                category.NodeName.Returns("node_id_2");
                category.Discharge.Returns(double.NaN);
                category.DischargeFile.Returns("some_file.bc");

                IBranch branch = CreateBranch(100, 0, 200, 0, "node_id_1", "node_id_2");

                var network = new HydroNetwork();
                network.Branches.Add(branch);

                string sourceFilePath = Path.Combine(temp.Path, "some_file_bnd.ext");
                var boundaryFileReader = Substitute.For<IBoundaryFileReader>();
                var logHandler = Substitute.For<ILogHandler>();
                var parser = new BndExtForceLateralSourceParser(sourceFilePath, network, false, false, boundaryFileReader, logHandler);

                var bcCategory = Substitute.For<ILateralSourceBcSection>();
                bcCategory.Name.Returns("lateral_source_id");
                bcCategory.DischargeFunction.Returns(HydroTimeSeriesFactory.CreateFlowTimeSeries());
                bcCategory.DataType.Returns(Model1DLateralDataType.FlowTimeSeries);
                bcCategory.Discharge.Returns(1.23);

                string bcFile = temp.CreateFile("some_file.bc");
                boundaryFileReader.ReadLateralSourcesFromBcFile(bcFile, logHandler).Returns(new[]
                {
                    bcCategory
                });

                // Call
                Model1DLateralSourceData lateralSourceData = parser.Parse(category);

                // Assert
                Assert.That(lateralSourceData.DataType, Is.EqualTo(Model1DLateralDataType.FlowTimeSeries));
                Assert.That(lateralSourceData.Flow, Is.EqualTo(1.23));
                Assert.That(lateralSourceData.Compartment, Is.Null);
                Assert.That(lateralSourceData.Data, Is.SameAs(bcCategory.DischargeFunction));

                LateralSource lateralSource = lateralSourceData.Feature;
                Assert.That(lateralSource.Branch, Is.SameAs(branch));
                Assert.That(lateralSource.Chainage, Is.EqualTo(100));
                Assert.That(lateralSource.Name, Is.EqualTo("lateral_source_id"));
                Assert.That(lateralSource.LongName, Is.EqualTo("lateral_source_name"));
                Assert.That(lateralSource.Geometry, Is.TypeOf<Point>());
                Assert.That(lateralSource.Geometry.InteriorPoint.X, Is.EqualTo(200));
                Assert.That(lateralSource.Geometry.InteriorPoint.Y, Is.EqualTo(0));
            }
        }

        private static IEnumerable<TestCaseData> ArgumentNullCases()
        {
            yield return new TestCaseData(null, Substitute.For<IBoundaryFileReader>(), "network");
            yield return new TestCaseData(Substitute.For<INetwork>(), null, "boundaryFileReader");
        }

        private static IEnumerable<TestCaseData> Parse_OnPipeCompartmentCases()
        {
            // first pipe, second pipe, referenced nodeId, exp. pipe of lateral source, exp. chainage of lateral source, exp. compartment of lateral source data
            IPipe pipeA1 = CreatePipe(100, 0, 200, 0, "node_id_1", "node_id_2");
            IPipe pipeA2 = CreatePipe(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(pipeA1, pipeA2, "node_id_1", pipeA1, 0, pipeA1.SourceCompartment);

            IPipe pipeB1 = CreatePipe(100, 0, 200, 0, "node_id_1", "node_id_2");
            IPipe pipeB2 = CreatePipe(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(pipeB1, pipeB2, "node_id_2", pipeB2, 0, pipeB2.SourceCompartment);

            IPipe pipeC1 = CreatePipe(100, 0, 200, 0, "node_id_1", "node_id_2");
            IPipe pipeC2 = CreatePipe(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(pipeC1, pipeC2, "node_id_3", pipeC2, 100, pipeC2.TargetCompartment);
        }

        private static IEnumerable<TestCaseData> Parse_OnBranchNodeCases()
        {
            // first branch, second second branch, referenced nodeId, exp. branch of lateral source, exp. chainage of lateral source
            IBranch branchA1 = CreateBranch(100, 0, 200, 0, "node_id_1", "node_id_2");
            IBranch branchA2 = CreateBranch(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(branchA1, branchA2, "node_id_1", branchA1, 0);

            IBranch branchB1 = CreateBranch(100, 0, 200, 0, "node_id_1", "node_id_2");
            IBranch branchB2 = CreateBranch(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(branchB1, branchB2, "node_id_2", branchB2, 0);

            IBranch pipeC1 = CreateBranch(100, 0, 200, 0, "node_id_1", "node_id_2");
            IBranch pipeC2 = CreateBranch(200, 0, 300, 0, "node_id_2", "node_id_3");
            yield return new TestCaseData(pipeC1, pipeC2, "node_id_3", pipeC2, 100);
        }

        private static IEnumerable<TestCaseData> Parse_OnBranchChainageCases()
        {
            // first branch, second second branch, referenced branchId, exp. branch of lateral source
            IBranch branchA1 = CreateBranch(100, 0, 200, 0, "branch_id_1");
            IBranch branchA2 = CreateBranch(200, 0, 300, 0, "branch_id_2");
            yield return new TestCaseData(branchA1, branchA2, "branch_id_1", branchA1);

            IBranch branchB1 = CreateBranch(100, 0, 200, 0, "branch_id_1");
            IBranch branchB2 = CreateBranch(200, 0, 300, 0, "branch_id_2");
            yield return new TestCaseData(branchB1, branchB2, "branch_id_2", branchB2);
        }

        private static IPipe CreatePipe(double x1, double y1, double x2, double y2, string sourceCompartmentName, string targetCompartmentName)
        {
            var c1 = new Coordinate(x1, y1);
            var c2 = new Coordinate(x2, y2);

            var geometry = new LineString(new[]
            {
                c1,
                c2
            });

            double length = Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            return new Pipe
            {
                Length = length,
                Geometry = geometry,
                SourceCompartment = Substitute.For<ICompartment>(),
                SourceCompartmentName = sourceCompartmentName,
                TargetCompartment = Substitute.For<ICompartment>(),
                TargetCompartmentName = targetCompartmentName
            };
        }

        private static IBranch CreateBranch(double x1, double y1, double x2, double y2, string sourceNodeName, string targetNodeName)
        {
            var c1 = new Coordinate(x1, y1);
            var c2 = new Coordinate(x2, y2);

            var geometry = new LineString(new[]
            {
                c1,
                c2
            });

            double length = Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            var sourceNode = Substitute.For<INode>();
            sourceNode.Name = sourceNodeName;

            var targetNode = Substitute.For<INode>();
            targetNode.Name = targetNodeName;

            return new Branch
            {
                Length = length,
                Geometry = geometry,
                Source = sourceNode,
                Target = targetNode
            };
        }

        private static IBranch CreateBranch(double x1, double y1, double x2, double y2, string branchName)
        {
            var c1 = new Coordinate(x1, y1);
            var c2 = new Coordinate(x2, y2);

            var geometry = new LineString(new[]
            {
                c1,
                c2
            });

            double length = Math.Sqrt(((x2 - x1) * (x2 - x1)) + ((y2 - y1) * (y2 - y1)));

            return new Branch
            {
                Name = branchName,
                Length = length,
                Geometry = geometry,
                Source = Substitute.For<INode>(),
                Target = Substitute.For<INode>()
            };
        }
    }
}