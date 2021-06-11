using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Validators;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Validation;
using GeoAPI.Extensions.Coverages;
using GeoAPI.Geometries;
using NetTopologySuite.Extensions.Coverages;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests.Validators
{
    [TestFixture]
    public class DiscretizationValidatorTests
    {
        [Test]
        public void ValidateEmptyDiscretizationTest()
        {
            var discretization = new Discretization();
            var report = DiscretizationValidator.Validate(discretization);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));
            Assert.That(report.Issues.Count(), Is.GreaterThanOrEqualTo(1));
            Assert.That(report.Issues.Select(i =>i.Message), Contains.Item("No computational grid defined."));
        }

        [Test]
        public void ValidateValidDiscretizationTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var discretization = new Discretization(){Network = network};
            var channel = network.Channels.First();
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });
            var report = DiscretizationValidator.Validate(discretization);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.None));
            Assert.That(report.Issues.Count(), Is.EqualTo(0));
        }

        /// o------channel1-----o------channel2-----o------channel3-----o------channel4-----o
        /// x-x-x-x-x-x-x-x-x-x-x------------------- -------------------x-------------------x
        [Test]
        public void ValidateDiscretizationMissing1ChannelCalcPointsTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var discretization = new Discretization(){Network = network, SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered};
            Assert.That(discretization.Segments.AllValues.Count, Is.EqualTo(4));
            var channel = network.Channels.First();
            var channel2 = network.Channels.ElementAt(1);
            var channel2Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel2));
            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel2Segments));
            var channel3 = network.Channels.ElementAt(2);
            var channel3Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel3));
            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel3Segments));
            var channel4 = network.Channels.ElementAt(3);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });
            discretization.Locations.AllValues.Add(new NetworkLocation(channel3, channel3.Length));
            discretization.Locations.AllValues.Add(new NetworkLocation(channel4, channel4.Length));
            var report = DiscretizationValidator.Validate(discretization);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));
            // and no location on branch2 end (because branch1 end equals branch2 start)
            Assert.That(report.AllErrors.Count(), Is.EqualTo(1));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel2.Name}, not at end of branch; can not start calculation."));
        }

        /// o------channel1-----o------channel2-----o------channel3-----o------channel4-----o
        /// x-x-x-x-x-x-x-x-x-x-x------------------- ------------------- -------------------x
        [Test]
        public void ValidateDiscretizationMissing3ChannelCalcPointsTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(4);
            var discretization = new Discretization(){Network = network, SegmentGenerationMethod = SegmentGenerationMethod.SegmentBetweenLocationsAndConnectedBranchesWithoutLocationOnThemFullyCovered};
            Assert.That(discretization.Segments.AllValues.Count, Is.EqualTo(4));
            var channel = network.Channels.First();
            var channel2 = network.Channels.ElementAt(1);
            var channel2Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel2));
            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel2Segments));
            var channel3 = network.Channels.ElementAt(2);
            var channel3Segments = discretization.Segments.AllValues.Where(s => s.Branch.Equals(channel3));
            discretization.Segments.RemoveValues(discretization.Segments.CreateValuesFilter(channel3Segments));
            var channel4 = network.Channels.ElementAt(3);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });
            discretization.Locations.AllValues.Add(new NetworkLocation(channel4, channel4.Length));
            var report = DiscretizationValidator.Validate(discretization);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));
        
            // error no locations on branch3 at start and end
            // and no location on branch2 end (because branch1 end equals branch2 start)
            Assert.That(report.AllErrors.Count(), Is.EqualTo(3));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel3.Name}, not at start of branch; can not start calculation."));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel2.Name}, not at end of branch; can not start calculation."));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"No computational grid cells defined for branch : {channel3.Name}, not at end of branch; can not start calculation."));
        }
        
        [Test]
        public void ValidateDiscretizationWithADoubleCalcPointTest()
        {
            var network = HydroNetworkHelper.GetSnakeHydroNetwork(1);
            var discretization = new Discretization(){Network = network};
            var channel = network.Channels.First();
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });
            discretization.Locations.Values[1].Geometry = (IGeometry) discretization.Locations.Values[2].Geometry.Clone();
            var report = DiscretizationValidator.Validate(discretization);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));
            Assert.That(report.AllErrors.Count(), Is.EqualTo(1));
            Assert.That(report.AllErrors.Select(i => i.Message), Contains.Item($"There are duplicate calculation points at same the location. Kernel cannot handle this. Please remove one of the points."));
        }

        [Test]
        public void ValidateCheckBranchStructureLocationsDiscretizationTest()
        {
            IHydroNetwork network = GetNetworkWithChannel(100);
            var discretization = new Discretization(){Network = network};
            var channel = network.Channels.First();
            var structure = new Weir("weir1"){Chainage = channel.Length/2-1};
            var structure1 = new Weir("weir2") {Chainage = channel.Length/2+1};
            var structure2 = new Weir("weir3") {Chainage = (channel.Length/2)+2};
            channel.BranchFeatures.Add(structure);
            channel.BranchFeatures.Add(structure1);
            channel.BranchFeatures.Add(structure2);
            HydroNetworkHelper.GenerateDiscretization(discretization, true, false, 100.0, false, 0.0, false, false,
                true, 10.0, new List<IChannel> { channel });
            var report = DiscretizationValidator.Validate(discretization);
            Assert.That(report.Severity(), Is.EqualTo(ValidationSeverity.Error));
            Console.WriteLine(string.Join(Environment.NewLine, report.AllErrors.Select(i => i.Message)));
            Assert.That(report.AllErrors.Count(), Is.EqualTo(1));
            report.AllErrors.Select(i => i.Message).ForEach(m => Assert.That(m,  Contains.Substring($"No grid points defined between structure")));
        }

        private static IHydroNetwork GetNetworkWithChannel(double length)
        {
            var node1 = new HydroNode("node1") {Geometry = new Point(0, 0)};
            var node2 = new HydroNode("node2") {Geometry = new Point(0, length)};
            
            var channel = new Channel("channel", node1, node2)
            {
                Geometry = new LineString(new[]
                {
                    new Coordinate(0, 0),
                    new Coordinate(0, length)
                })
            };
            
            var network = new HydroNetwork();
            network.Nodes.Add(node1);
            network.Nodes.Add(node2);
            network.Branches.Add(channel);

            return network;
        }
    }
}