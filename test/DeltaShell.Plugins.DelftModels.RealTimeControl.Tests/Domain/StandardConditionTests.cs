using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using Rhino.Mocks;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class StandardConditionTests
    {
        private static readonly XNamespace Fns = "http://www.wldelft.nl/fews";

        private const string Implicit = "IMPLICIT";
        private const string Name = "Trigger31";
        private const string InputName = "AlarmREGEN";
        private const string InputParameterName = "DeadBandTime";
        private const double Value = 1.5;
        private const string TrueReference = "REGEN-ORANGE";
        private const string TrueAndFalseReference = "Thunersee Messung";
        private const string FalseReference = "REGEN-ROT";

        private MockRepository mocks;
        private IControlGroup controlGroup;
        private PIDRule trueRule;
        private PIDRule falseRule;
        private PIDRule trueAndFalseRule;
        private StandardCondition standardCondition;

        [SetUp]
        public void SetUp()
        {
            mocks = new MockRepository();
            controlGroup = mocks.StrictMock<IControlGroup>();
            trueRule = new PIDRule { Name = TrueReference };
            falseRule = new PIDRule { Name = FalseReference };
            trueAndFalseRule = new PIDRule { Name = TrueAndFalseReference };

            standardCondition = new StandardCondition
                                   {
                                       Name = Name,
                                       Reference = Implicit,
                                       Operation = Operation.Greater,
                                       Input =
                                           new Input
                                               {
                                                   ParameterName = InputParameterName,
                                                   Feature = new RtcTestFeature { Name = InputName }
                                               },
                                       Value = Value,
                                   };

        }

        [Test]
        public void CheckXmlGeneration()
        {

            standardCondition.TrueOutputs.Add(trueRule);
            standardCondition.FalseOutputs.Add(falseRule);

            Assert.AreEqual(OriginXml(), standardCondition.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        [Test]
        public void CheckXmlGenerationOtherBooleanCheck()
        { 
            standardCondition.Operation = Operation.Less;

            standardCondition.TrueOutputs.Add(trueRule);
            standardCondition.FalseOutputs.Add(falseRule);

            Assert.AreEqual(SecondaryConditionLessThanXml(), standardCondition.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }
 
        [Test]
        public void TrueAndFalseOutputsCanContainRulesAndConditions()
        {
            var condition = new StandardCondition();
            condition.TrueOutputs.Add(new PIDRule());
            condition.FalseOutputs.Add(new StandardCondition());
            Assert.AreEqual(1, condition.TrueOutputs.Count);
            Assert.AreEqual(1, condition.FalseOutputs.Count);
        }

        [Test]
        public void GenerateXmlConditionToCondition()
        {
            var trueCondition = new StandardCondition()
            {
                Name = "trueCondition",
                Reference = Implicit,
                Operation = Operation.Greater,
                Input =
                    new Input
                    {
                        ParameterName = InputParameterName,
                        Feature = new RtcTestFeature { Name = InputName }
                    },
                Value = Value,
            };

            var falseCondition = new StandardCondition()
            {
                Name = "falseCondition",
                Reference = Implicit,
                Operation = Operation.Greater,
                Input =
                    new Input
                    {
                        ParameterName = InputParameterName,
                        Feature = new RtcTestFeature { Name = InputName }
                    },
                Value = Value,
            };

            standardCondition.TrueOutputs.Add(trueCondition);
            standardCondition.FalseOutputs.Add(falseCondition);

            Assert.AreEqual(xmlConditionToCondition(), standardCondition.ToXml(Fns, "").ToString(SaveOptions.DisableFormatting));
        }

        private string OriginXml()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                    "<standard id=\"/" + standardCondition.Name + "\">" +
                    //"<input ref=\"" + Implicit + "\">" + InputName + "_" + InputParameterName + "</input>" +
                    //"<greaterThan>" + Value.ToString(CultureInfo.InvariantCulture) + "</greaterThan>" +
                    "<condition>" +
                    "<x1Series ref=\"" + Implicit + "\">" + RtcXmlTag.Input + InputName + "/" + InputParameterName + "</x1Series>" +
  	                "<relationalOperator>Greater</relationalOperator>"+
                    "<x2Value>" + Value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                    "</condition>" +
                    "<true>" +
					"<trigger><ruleReference>/"+TrueReference+"</ruleReference></trigger>"+
				    "</true>"+
					"<false>"+
					"<trigger><ruleReference>/"+FalseReference+"</ruleReference></trigger>"+
					"</false>"+
                    "<output>" +
                    "<status>" + RtcXmlTag.StandardCondition + RtcXmlTag.Status + "/" + standardCondition.Name + "</status>" +
                    "</output>" +
                    "</standard>" +
                    "</trigger>";
        }

        private string SecondaryConditionLessThanXml()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                    "<standard id=\"/" + standardCondition.Name + "\">" +
                    //"<input ref=\"" + Implicit + "\">" + InputName + "_" + InputParameterName + "</input>" +
                    //"<lessThan>" + Value.ToString(CultureInfo.InvariantCulture) + "</lessThan>" +
                    "<condition>" +
                    "<x1Series ref=\"" + Implicit + "\">" + RtcXmlTag.Input + InputName + "/" + InputParameterName + "</x1Series>" +
                    "<relationalOperator>Less</relationalOperator>" +
                    "<x2Value>" + Value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                    "</condition>" +
                    "<true>" +
                    "<trigger><ruleReference>/" + TrueReference + "</ruleReference></trigger>" +
                    "</true>" +
                    "<false>" +
                    "<trigger><ruleReference>/" + FalseReference + "</ruleReference></trigger>" +
                    "</false>" +
                    "<output>" +
                    "<status>" + RtcXmlTag.StandardCondition + RtcXmlTag.Status+ "/" + standardCondition.Name + "</status>" +
                    "</output>" +
                    "</standard>" +
                    "</trigger>";
        }

        private string xmlConditionToCondition()
        {
            return "<trigger xmlns=\"http://www.wldelft.nl/fews\">" +
                    "<standard id=\"/" + standardCondition.Name + "\">" +
                    "<condition>" +
                    "<x1Series ref=\"" + Implicit + "\">" + RtcXmlTag.Input + InputName + "/" + InputParameterName + "</x1Series>" +
                    "<relationalOperator>Greater</relationalOperator>" +
                    "<x2Value>" + Value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                    "</condition>" +
                    "<true>" +
                    "<trigger>"+
                             "<standard id=\"/trueCondition\">" +
                            "<condition>" +
                            "<x1Series ref=\"" + Implicit + "\">" + RtcXmlTag.Input + InputName + "/" + InputParameterName + "</x1Series>" +
                            "<relationalOperator>Greater</relationalOperator>" +
                            "<x2Value>" + Value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                            "</condition>" +
                            "<output>" +
                            "<status>" + RtcXmlTag.StandardCondition + RtcXmlTag.Status + "/trueCondition</status>" +
                            "</output>" +
                            "</standard>" +
                            "</trigger>" +
                    "</true>" +
                    "<false>" +
                    "<trigger>"+
                             "<standard id=\"/falseCondition\">" +
                            "<condition>" +
                            "<x1Series ref=\"" + Implicit + "\">" + RtcXmlTag.Input + InputName + "/" + InputParameterName + "</x1Series>" +
                            "<relationalOperator>Greater</relationalOperator>" +
                            "<x2Value>" + Value.ToString(CultureInfo.InvariantCulture) + "</x2Value>" +
                            "</condition>" +
                            "<output>" +
                            "<status>" + RtcXmlTag.StandardCondition + RtcXmlTag.Status + "/falseCondition</status>" +
                            "</output>" +
                            "</standard>" +
                    "</trigger>" +
                    "</false>" +
                    "<output>" +
                    "<status>" + RtcXmlTag.StandardCondition + RtcXmlTag.Status +"/"+ standardCondition.Name + "</status>" +
                    "</output>" +
                    "</standard>" +
                    "</trigger>";
        }


        [Test]
        public void CopyFrom()
        {
            var condition = new StandardCondition();
            condition.CopyFrom(standardCondition);
            Assert.AreEqual(Name, condition.Name);
            Assert.AreEqual(Value, condition.Value);
            Assert.AreEqual(Implicit, condition.Reference);
            Assert.AreEqual(Operation.Greater, condition.Operation);
        }

        [Test]
        public void CopyFromAndCreateClone()
        {
            var source = new StandardCondition()
            {
                Name = "test",
                LongName = "testLong",
                Value = 0.1,
                Reference = "reference",
                Operation = Operation.Equal
            };

            source.TrueOutputs.Add(source);
            source.FalseOutputs.Add(source);

            var newCondition = new StandardCondition();
            newCondition.CopyFrom(source);

            Assert.AreEqual(source.Name, newCondition.Name);
            Assert.AreEqual(source.LongName, newCondition.LongName);
            Assert.AreEqual(source.Value, newCondition.Value);
            Assert.AreEqual(source.Reference, newCondition.Reference);
            Assert.AreEqual(source.Operation, newCondition.Operation);
            
            var clone = (StandardCondition) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        public void ValidationTestBaseClass()
        {
            // see TOOLS-4373 RTC validation: valid connections
            // output condition: either input other condition or input rule
            var condition = new StandardCondition();
            var validationResult = condition.Validate();
            var noOutputexceptionCount = validationResult.Messages.Count();
            Assert.Greater(noOutputexceptionCount, 0);

            // add output and check result has less validation exceptions
            condition.TrueOutputs.Add(new StandardCondition());
            validationResult = condition.Validate();
            var oneTrueOutputExceptionCount = validationResult.Messages.Count();
            Assert.Less(oneTrueOutputExceptionCount, noOutputexceptionCount);

            // TOOLS-4371 RTC validation: a condition has maximum 1 True and/or 1 False output 
            // add another output and check result has more validation exceptions
            condition.TrueOutputs.Add(new StandardCondition());
            validationResult = condition.Validate();
            var twoTrueOutputExceptionCount = validationResult.Messages.Count();
            Assert.Greater(twoTrueOutputExceptionCount, oneTrueOutputExceptionCount);

            // do same check for False
            condition.TrueOutputs.Clear();
            noOutputexceptionCount = condition.Validate().Messages.Count();
            Assert.Greater(noOutputexceptionCount, 0);

            condition.FalseOutputs.Add(new StandardCondition());
            var oneFalseOutputExceptionCount = condition.Validate().Messages.Count();
            Assert.Less(oneFalseOutputExceptionCount, noOutputexceptionCount);

            condition.FalseOutputs.Add(new StandardCondition());
            var twoFalseOutputExceptionCount = condition.Validate().Messages.Count();
            Assert.Greater(twoFalseOutputExceptionCount, oneFalseOutputExceptionCount);
        }
    }
}
