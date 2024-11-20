using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using GeoAPI.Extensions.Feature;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class ControlGroupFeatureCollectionTest
    {
        [Test]
        public void CollectionForConnectionPoints()
        {
            var controlGroups = new EventedList<ControlGroup>();
            var collection = new ControlGroupFeatureCollection(controlGroups);

            controlGroups.Add(new ControlGroup
            {
                Inputs = {new Input()},
                Outputs = {new Output()}
            });
            controlGroups.Add(new ControlGroup
            {
                Inputs =
                {
                    new Input(),
                    new Input()
                },
                Outputs = {new Output()}
            });

            Assert.AreEqual(5, collection.Features.Count);
            Assert.AreEqual(typeof(ConnectionPoint), collection.FeatureType);
        }

        [Test]
        public void CollectionForConnections()
        {
            var controlGroups = new EventedList<ControlGroup>();

            var collection = new ControlGroupFeatureCollection(controlGroups) {UseConnections = true};

            var rule = new PIDRule("testRule");
            var input = new Input
            {
                ParameterName = "InParam",
                Feature = new RtcTestFeature {Geometry = new Point(0, 0)}
            };
            var output = new Output
            {
                ParameterName = "OutParam",
                Feature = new RtcTestFeature {Geometry = new Point(10, 10)}
            };
            var controlGroup = new ControlGroup {Name = "testControlGroup"};
            var condition = new StandardCondition
            {
                Name = "testCondition",
                Input = input
            };

            rule.Outputs.Add(output);

            condition.Input = input;
            condition.FalseOutputs.Add(rule);

            controlGroup.Rules.Add(rule);
            controlGroup.Conditions.Add(condition);

            controlGroup.Inputs.Add(input);
            controlGroup.Outputs.Add(output);

            controlGroups.Add(controlGroup);

            Assert.AreEqual(1, collection.Features.Count);
            Assert.AreEqual(typeof(Connection), collection.FeatureType);
            Assert.AreEqual(new LineString(new[]
                            {
                                new Coordinate(0, 0),
                                new Coordinate(10, 10)
                            }), ((IFeature) collection.Features[0]).Geometry);
        }
    }
}