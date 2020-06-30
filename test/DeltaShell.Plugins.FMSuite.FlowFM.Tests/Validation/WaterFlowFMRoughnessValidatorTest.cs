using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.Hydro.Roughness;
using DelftTools.Utils.Validation;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.Plugins.FMSuite.FlowFM.Validation;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Validation
{
    [TestFixture]
    public class WaterFlowFMRoughnessValidatorTest
    {
        [Test]
        public void GivenFmModelWithoutRoughnessDefinitions_WhenValidatingRoughness_ThenValidationReportEmpty()
        {
            // Given
            using (var waterFlowFmModel = CreateValidModelWithSimpleNetwork())
            {
                // When
                var report = WaterFlowFMRoughnessValidator.Validate(waterFlowFmModel);

                // Then
                Assert.IsTrue(report.IsEmpty);
            }
        }

        [Test]
        [TestCaseSource(nameof(InvalidRoughnessTestCaseSource))]
        public void GivenFmModelWithInvalidRoughnessDefinition_WhenValidatingRoughness_ThenValidationReportAsExpected(
            RoughnessTestCaseData testCaseData)
        {
            // Given
            using (var waterFlowFmModel = CreateValidModelWithSimpleNetwork())
            {
                testCaseData.ConfigureInvalidRoughness(waterFlowFmModel);

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
            return new WaterFlowFMModel
            {
                TimeStep = new TimeSpan(0, 0, 1, 0),
                StartTime = new DateTime(2000, 1, 1),
                StopTime = new DateTime(2000, 1, 2),
                OutputTimeStep = new TimeSpan(0, 0, 2, 0),
                Network =
                {
                    Branches =
                    {
                        new Channel
                        {
                            Length = 100
                        },
                        new Channel
                        {
                            Length = 200
                        }
                    }
                }
            };
        }

        private IEnumerable<RoughnessTestCaseData> InvalidRoughnessTestCaseSource
        {
            get
            {
                yield return new RoughnessTestCaseData
                {
                    ConfigureInvalidRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.Constant;
                    },
                    ExpectedMessage = "No 'Constant' values defined",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureInvalidRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfQ;
                    },
                    ExpectedMessage = "No 'absDischarge' values defined",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureInvalidRoughness = waterFlowFmModel =>
                    {
                        var channelFrictionDefinition = waterFlowFmModel.ChannelFrictionDefinitions.First();
                        channelFrictionDefinition.SpecificationType = ChannelFrictionSpecificationType.SpatialChannelFrictionDefinition;
                        channelFrictionDefinition.SpatialChannelFrictionDefinition.FunctionType = RoughnessFunction.FunctionOfH;
                    },
                    ExpectedMessage = "No 'Waterlevel' values defined",
                    ExpectedSubject = waterFlowFmModel => waterFlowFmModel.ChannelFrictionDefinitions.First().Channel
                };
                yield return new RoughnessTestCaseData
                {
                    ConfigureInvalidRoughness = waterFlowFmModel =>
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
                yield return new RoughnessTestCaseData
                {
                    ConfigureInvalidRoughness = waterFlowFmModel =>
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
                yield return new RoughnessTestCaseData
                {
                    ConfigureInvalidRoughness = waterFlowFmModel =>
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
            }
        }

        public class RoughnessTestCaseData
        {
            public Action<WaterFlowFMModel> ConfigureInvalidRoughness { get; set; }

            public string ExpectedMessage { get; set; }

            public Func<WaterFlowFMModel, object> ExpectedSubject { get; set; }
        }
    }
}
