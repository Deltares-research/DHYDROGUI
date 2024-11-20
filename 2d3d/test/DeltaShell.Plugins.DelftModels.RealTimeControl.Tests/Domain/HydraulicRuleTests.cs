using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO;
using DeltaShell.Plugins.DelftModels.RealTimeControl.IO.Export;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class HydraulicRuleTests
    {
        private const string RuleName = "hydraulic rule name";
        private const string InputName = "Maxau";
        private const string InputParameterName = "QIn";
        private const string OutputName = "Iffezheim";
        private const string OutputParameterName = "HSP";
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";
        private int nTimeSteps;

        private Function tableFunction;
        private Input input;
        private Output output;

        [SetUp]
        public void SetUp()
        {
            tableFunction = HydraulicRule.DefineFunction();
            tableFunction[0.0] = 123.6;
            tableFunction[3400.0] = 123.6;
            tableFunction[4800.0] = 123.0;
            tableFunction[8000.0] = 123.0;
            input = new Input
            {
                ParameterName = InputParameterName,
                Feature = new RtcTestFeature {Name = InputName}
            };
            output = new Output
            {
                ParameterName = OutputParameterName,
                Feature = new RtcTestFeature {Name = OutputName}
            };
        }

        [Test]
        public void CheckXmlGeneration()
        {
            var hydraulicRule = new HydraulicRule
            {
                Name = RuleName,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                Function = tableFunction,
                Interpolation = InterpolationType.Linear
            };

            var serializer = new HydraulicRuleSerializer(hydraulicRule);
            Assert.AreEqual(OriginXml(), serializer.ToXml(Fns, "").First().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void RuleHasOutputValidation()
        {
            // see TOOLS-4373 RTC validation: valid connections
            // output rule always to input output parameter
            var rule = new HydraulicRule();
            ValidationResult validationResult = rule.Validate();
            int exceptionCount = validationResult.Messages.Count();
            Assert.Greater(exceptionCount, 0);

            // add output and check result has less validation exceptions
            rule.Outputs.Add(new Output());
            validationResult = rule.Validate();
            int newExceptionCount = validationResult.Messages.Count();
            Assert.Greater(exceptionCount, newExceptionCount);
        }

        [Test]
        public void SetTimeLagToTimeSteps()
        {
            var hydraulicRule = new HydraulicRule();

            hydraulicRule.TimeLag = 2000;
            hydraulicRule.SetTimeLagToTimeSteps(new TimeSpan(0, 0, 200));

            Assert.AreEqual(10, hydraulicRule.TimeLagInTimeSteps);
        }

        [Test]
        public void XmlGenerationWithTimeLag()
        {
            var hydraulicRule = new HydraulicRule
            {
                Name = RuleName,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output},
                Function = tableFunction
            };

            hydraulicRule.TimeLag = 2000;
            hydraulicRule.SetTimeLagToTimeSteps(new TimeSpan(0, 0, 200));
            nTimeSteps = hydraulicRule.TimeLagInTimeSteps;
            var serializer = new HydraulicRuleSerializer(hydraulicRule);
            Assert.AreEqual(HydraulicRuleWithTimeLagXml(), serializer.ToXml(Fns, "").First().ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void ValidationNoOutPutNoLookupTableTest()
        {
            var rule = new HydraulicRule();
            ValidationResult result = rule.Validate();
            Assert.AreEqual(4, result.Messages.Count());
            Assert.AreEqual(false, result.IsValid);
        }

        [Test]
        public void ValidationUnlinkedOutputNoLookupTest()
        {
            var rule = new HydraulicRule();
            rule.Outputs.Add(new Output());
            ValidationResult result = rule.Validate();
            Assert.AreEqual(3, result.Messages.Count()); // only 1 output
            Assert.AreEqual(false, result.IsValid);
        }

        [Test]
        public void ValidationNoLookupTest()
        {
            var rule = new HydraulicRule();
            rule.Outputs.Add(new Output
            {
                ParameterName = "parameter",
                Feature = new RtcTestFeature()
            });
            ValidationResult result = rule.Validate();
            Assert.AreEqual(2, result.Messages.Count()); // empty lookup, only 1 output
            Assert.AreEqual(false, result.IsValid);
        }

        [Test]
        public void ValidationSuccessTest()
        {
            var rule = new HydraulicRule();
            rule.Inputs.Add(new Input
            {
                ParameterName = "parameter",
                Feature = new RtcTestFeature()
            });
            rule.Outputs.Add(new Output
            {
                ParameterName = "parameter",
                Feature = new RtcTestFeature()
            });
            rule.Function[0.0] = 0.0;
            ValidationResult result = rule.Validate();
            Assert.AreEqual(0, result.Messages.Count());
            Assert.AreEqual(true, result.IsValid);
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = new HydraulicRule {Name = "test"};

            var newRule = new HydraulicRule();
            double[] argumentValues = new[]
            {
                60,
                120.0,
                360.0
            };
            var componentValues = new[]
            {
                8.0,
                9.0,
                10.0
            };
            for (var i = 0; i < argumentValues.Count(); i++)
            {
                source.Function[argumentValues[i]] = componentValues[i];
            }

            newRule.CopyFrom(source);

            Assert.AreEqual(source.Name, newRule.Name);
            for (var i = 0; i < source.Function.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.Function.Arguments[0].Values[i], newRule.Function.Arguments[0].Values[i]);
                Assert.AreEqual(source.Function.Components[0].Values[i], newRule.Function.Components[0].Values[i]);
            }

            Assert.AreEqual(source.Interpolation, newRule.Interpolation);

            var clone = (HydraulicRule) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        public void DoNotSupportNoneInterpolation()
        {
            Assert.That(() => new HydraulicRule
            {
                Name = "test",
                Interpolation = InterpolationType.None
            }, Throws.ArgumentException);
        }

        private string OriginXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<lookupTable id=\"" + "[HydraulicRule]" + RuleName + "\">" +
                   "<table>" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "</table>" +
                   "<interpolationOption>LINEAR</interpolationOption>" +
                   "<extrapolationOption>BLOCK</extrapolationOption>" +
                   "<input>" +
                   "<x ref=\"IMPLICIT\">" + RtcXmlTag.Input + InputName + "/" + InputParameterName + "</x>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + OutputName + "/" + OutputParameterName + "</y>" +
                   "</output>" +
                   "</lookupTable>" +
                   "</rule>";
        }

        private string HydraulicRuleWithTimeLagXml()
        {
            return "<rule xmlns=\"http://www.wldelft.nl/fews\">" +
                   "<lookupTable id=\"" + "[HydraulicRule]" + RuleName + "\">" +
                   "<table>" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[0]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[1]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[2]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "<record x=\"" +
                   ((double) tableFunction.Arguments[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" y=\"" +
                   ((double) tableFunction.Components[0].Values[3]).ToString(CultureInfo.InvariantCulture) + "\" />" +
                   "</table>" +
                   "<interpolationOption>BLOCK</interpolationOption>" +
                   "<extrapolationOption>BLOCK</extrapolationOption>" +
                   "<input>" +
                   "<x ref=\"EXPLICIT\">" + RtcXmlTag.Delayed + RtcXmlTag.Input + InputName + "/" + InputParameterName + "[" + (nTimeSteps - 2) + "]</x>" +
                   "</input>" +
                   "<output>" +
                   "<y>" + RtcXmlTag.Output + OutputName + "/" + OutputParameterName + "</y>" +
                   "</output>" +
                   "</lookupTable>" +
                   "</rule>";
        }
    }
}