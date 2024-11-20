using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMRoughnessValidatorTest
    {
        [Test]
        [TestCaseSource(nameof(ValidRoughnessTestCaseSource))]
        public void GivenFmModelWithValidRoughnessDefinition_WhenValidatingRoughness_ThenValidationReportEmpty(RoughnessTestCaseData testCaseData)
        {
            // Given
            using (var waterFlowFmModel = CreateValidModelWithSimpleNetwork())
            {
                testCaseData.ConfigureRoughness(waterFlowFmModel);

                // When
                var report = WaterFlowFMRoughnessValidator.Validate(waterFlowFmModel);

                // Then
                Assert.IsTrue(report.IsEmpty);
            }
        }

        [Test]
        [TestCaseSource(nameof(InvalidRoughnessTestCaseSource))]
        public void GivenFmModelWithInvalidRoughnessDefinition_WhenValidatingRoughness_ThenValidationReportAsExpected(
            InvalidRoughnessTestCaseData testCaseData)
        {
            // Given
            using (var waterFlowFmModel = CreateValidModelWithSimpleNetwork())
            {
                testCaseData.ConfigureRoughness(waterFlowFmModel);

                // When
                var report = WaterFlowFMRoughnessValidator.Validate(waterFlowFmModel);

                // Then
                Assert.IsFalse(report.IsEmpty);
                
                var issues = report.Issues.ToArray();
                Assert.AreEqual(1, issues.Length);
                
                var issue = issues.ElementAt(0);
                Assert.AreEqual(ValidationSeverity.Error, issue.Severity);
                Assert.AreEqual(testCaseData.ExpectedMessage, issue.Message);
                Assert.AreSame(testCaseData.ExpectedSubject(waterFlowFmModel), issue.Subject);
            }
        }

        private static WaterFlowFMModel CreateValidModelWithSimpleNetwork()
        {
            var flowFmModel = new WaterFlowFMModel
            {
                TimeStep = new TimeSpan(0, 0, 1, 0),
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 2),
                OutputTimeStep = new TimeSpan(0, 0, 2, 0)
            };

            HydroNetworkHelper.AddSnakeHydroNetwork(flowFmModel.Network, new Point(0, 0), new Point(0, 100), new Point(0, 300));

            return flowFmModel;
        }

        private static IEnumerable<RoughnessTestCaseData> ValidRoughnessTestCaseSource
        {
            get
            {
                yield return new RoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel => { }
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.AddRange(new[]
                        {
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 0,
                                Value = 2
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 50,
                                Value = 3
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 100,
                                Value = 4
                            }
                        });
                    }
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;

                        var function = channelFrictionDefinition.SpatialChannelFrictionDefinition.Function;
                        function[0.0, 1.0] = 2.0;
                        function[50.0, 3.0] = 4.0;
                        function[100.0, 5.0] = 6.0;
                    }
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;

                        var function = channelFrictionDefinition.SpatialChannelFrictionDefinition.Function;
                        function[0.0, 1.0] = 2.0;
                        function[50.0, 3.0] = 4.0;
                        function[100.0, 5.0] = 6.0;
                    }
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        waterFlowFmModel.ChannelFrictionDefinitions.First().SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
                        waterFlowFmModel.ChannelFrictionDefinitions.Last().SpecificationType = ChannelFrictionSpecificationType.ModelSettings;

                        var hydroNetwork = waterFlowFmModel.Network;
                        var channel1 = hydroNetwork.Channels.First();
                        var channel2 = hydroNetwork.Channels.Last();
                        var crossSection1 = new CrossSection(new CrossSectionDefinitionYZ("crs1"));
                        var crossSection2 = new CrossSection(new CrossSectionDefinitionYZ("crs2"));

                        channel1.BranchFeatures.Add(crossSection1);
                        channel2.BranchFeatures.Add(crossSection2);
                        crossSection1.ShareDefinitionAndChangeToProxy();
                        crossSection2.UseSharedDefinition(hydroNetwork.SharedCrossSectionDefinitions.First());
                    }
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        waterFlowFmModel.ChannelFrictionDefinitions.First().SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;
                        waterFlowFmModel.ChannelFrictionDefinitions.Last().SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;

                        var hydroNetwork = waterFlowFmModel.Network;
                        var channel1 = hydroNetwork.Channels.First();
                        var channel2 = hydroNetwork.Channels.Last();
                        var crossSection1 = new CrossSection(new CrossSectionDefinitionYZ("crs1"));
                        var crossSection2 = new CrossSection(new CrossSectionDefinitionYZ("crs2"));

                        channel1.BranchFeatures.Add(crossSection1);
                        channel2.BranchFeatures.Add(crossSection2);
                        crossSection1.ShareDefinitionAndChangeToProxy();
                        crossSection2.UseSharedDefinition(hydroNetwork.SharedCrossSectionDefinitions.First());
                    }
                };
            }
        }

        private static IEnumerable<InvalidRoughnessTestCaseData> InvalidRoughnessTestCaseSource
        {
            get
            {
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
                    },
                    ExpectedMessage = "No 'Constant' values defined",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
                    },
                    ExpectedMessage = "No 'absDischarge' values defined",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
                    },
                    ExpectedMessage = "No 'Waterlevel' values defined",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.AddRange(new[]
                        {
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 0,
                                Value = 1
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 50,
                                Value = 2
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 50,
                                Value = 3
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 100,
                                Value = 4
                            },
                        });
                    },
                    ExpectedMessage = "One or more 'Constant' values have a duplicate 'Chainage'",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.ConstantSpatialChannelFrictionDefinitions.AddRange(new[]
                        {
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = -1,
                                Value = 1
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 0,
                                Value = 2
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 50,
                                Value = 3
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 100,
                                Value = 4
                            },
                            new ConstantSpatialChannelFrictionDefinition
                            {
                                Chainage = 101,
                                Value = 5
                            }
                        });
                    },
                    ExpectedMessage = "One or more 'Constant' values are invalid regarding their 'Chainage'. The chainages involved are: -1, 101.",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;

                        var function = channelFrictionDefinition.SpatialChannelFrictionDefinition.Function;
                        function[-1.0, 1.0] = 2.0;
                        function[0.0, 3.0] = 4.0;
                        function[50.0, 5.0] = 6.0;
                        function[100.0, 7.0] = 8.0;
                        function[101.0, 9.0] = 10.0;
                    },
                    ExpectedMessage = "One or more 'absDischarge' values are invalid regarding their 'Chainage'. The chainages involved are: -1, 101.",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;

                        var function = channelFrictionDefinition.SpatialChannelFrictionDefinition.Function;
                        function[-1.0, 1.0] = 2.0;
                        function[0.0, 3.0] = 4.0;
                        function[50.0, 5.0] = 6.0;
                        function[100.0, 7.0] = 8.0;
                        function[101.0, 9.0] = 10.0;
                    },
                    ExpectedMessage = "One or more 'Waterlevel' values are invalid regarding their 'Chainage'. The chainages involved are: -1, 101.",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new InvalidRoughnessTestCaseData
                {
                    ConfigureRoughness = waterFlowFmModel =>
                    {
                        waterFlowFmModel.ChannelFrictionDefinitions.First().SpecificationType = ChannelFrictionSpecificationType.ModelSettings;
                        waterFlowFmModel.ChannelFrictionDefinitions.Last().SpecificationType = ChannelFrictionSpecificationType.RoughnessSections;

                        var hydroNetwork = waterFlowFmModel.Network;
                        var channel1 = hydroNetwork.Channels.First();
                        var channel2 = hydroNetwork.Channels.Last();
                        var crossSection1 = new CrossSection(new CrossSectionDefinitionYZ("crs1"));
                        var crossSection2 = new CrossSection(new CrossSectionDefinitionYZ("crs2"));

                        channel1.BranchFeatures.Add(crossSection1);
                        channel2.BranchFeatures.Add(crossSection2);
                        crossSection1.ShareDefinitionAndChangeToProxy();
                        crossSection2.UseSharedDefinition(hydroNetwork.SharedCrossSectionDefinitions.First());
                    },
                    ExpectedMessage = "This shared cross section definition is used on branches that have a conflicting roughness Specification type. The branches involved are: branch1, branch2.",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.Network.SharedCrossSectionDefinitions.First()
                };
            }
        }

        public class RoughnessTestCaseData
        {
            public Action<WaterFlowFMModel> ConfigureRoughness { get; set; }
        }

        public class InvalidRoughnessTestCaseData : RoughnessTestCaseData
        {
            public string ExpectedMessage { get; set; }

            public Func<WaterFlowFMModel, object> ExpectedSubject { get; set; }
        }
    }
}
