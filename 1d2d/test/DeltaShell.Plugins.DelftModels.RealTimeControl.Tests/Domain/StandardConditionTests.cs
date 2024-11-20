using System.Linq;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class StandardConditionTests
    {
        private const string Implicit = StandardCondition.ReferenceType.Implicit;
        private const string Name = "Trigger31";
        private const string InputName = "AlarmREGEN";
        private const string InputParameterName = "DeadBandTime";
        private const double Value = 1.5;

        private StandardCondition standardCondition;

        [SetUp]
        public void SetUp()
        {
            standardCondition = new StandardCondition
            {
                Name = Name,
                Reference = Implicit,
                Operation = Operation.Greater,
                Input =
                    new Input
                    {
                        ParameterName = InputParameterName,
                        Feature = new RtcTestFeature {Name = InputName}
                    },
                Value = Value
            };
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
            var source = new StandardCondition
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
        public void TrueAndFalseOutputsCanContainRulesAndConditions()
        {
            var condition = new StandardCondition();
            condition.TrueOutputs.Add(new PIDRule());
            condition.FalseOutputs.Add(new StandardCondition());
            Assert.AreEqual(1, condition.TrueOutputs.Count);
            Assert.AreEqual(1, condition.FalseOutputs.Count);
        }

        [Test]
        public void ValidationTestBaseClass()
        {
            // see TOOLS-4373 RTC validation: valid connections
            // output condition: either input other condition or input rule
            var condition = new StandardCondition();
            ValidationResult validationResult = condition.Validate();
            int noOutputexceptionCount = validationResult.Messages.Count();
            Assert.Greater(noOutputexceptionCount, 0);

            // add output and check result has less validation exceptions
            condition.TrueOutputs.Add(new StandardCondition());
            validationResult = condition.Validate();
            int oneTrueOutputExceptionCount = validationResult.Messages.Count();
            Assert.Less(oneTrueOutputExceptionCount, noOutputexceptionCount);

            // TOOLS-4371 RTC validation: a condition has maximum 1 True and/or 1 False output 
            // add another output and check result has more validation exceptions
            condition.TrueOutputs.Add(new StandardCondition());
            validationResult = condition.Validate();
            int twoTrueOutputExceptionCount = validationResult.Messages.Count();
            Assert.Greater(twoTrueOutputExceptionCount, oneTrueOutputExceptionCount);

            // do same check for False
            condition.TrueOutputs.Clear();
            noOutputexceptionCount = condition.Validate().Messages.Count();
            Assert.Greater(noOutputexceptionCount, 0);

            condition.FalseOutputs.Add(new StandardCondition());
            int oneFalseOutputExceptionCount = condition.Validate().Messages.Count();
            Assert.Less(oneFalseOutputExceptionCount, noOutputexceptionCount);

            condition.FalseOutputs.Add(new StandardCondition());
            int twoFalseOutputExceptionCount = condition.Validate().Messages.Count();
            Assert.Greater(twoFalseOutputExceptionCount, oneFalseOutputExceptionCount);
        }
    }
}