using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.TestUtils.Domain;
using NUnit.Framework;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class PIDRuleTest
    {
        private const string IffezheimKi = "[IP]pid rule";
        private const string DifferentialPart = "[DP]pid rule";
        private const string RuleName = "pid rule";
        private const string IffezheimHin1 = "Iffezheim";
        private const string IffezheimHin2 = "HIn";
        private const string IffezheimHsp = "[SetPoint]pid rule";
        private const string IffezheimSout1 = "Iffezheim";
        private const string IffezheimSout2 = "SOut";
        private const double SMin = 116;
        private const double SMax = 123.6;
        private const double SMaxSpeed = 0.2;
        private const double Kp = 0.5;
        private const double Ki = 0.2;
        private const double Kd = 0;

        private Setting setting;
        private Input input;
        private Output output;

        [SetUp]
        public void SetUp()
        {
            setting = new Setting
            {
                Min = SMin,
                Max = SMax,
                MaxSpeed = SMaxSpeed
            };
            input = new Input
            {
                ParameterName = IffezheimHin2,
                Feature = new RtcTestFeature {Name = IffezheimHin1},
                SetPoint = IffezheimHsp
            };
            output = new Output
            {
                ParameterName = IffezheimSout2,
                Feature = new RtcTestFeature {Name = IffezheimSout1},
                IntegralPart = IffezheimKi,
                DifferentialPart = DifferentialPart
            };
        }

        [Test]
        public void PIDRuleRequiresOneInputValidation()
        {
            var pidRule = new PIDRule
            {
                Name = RuleName,
                Kp = Kp,
                Ki = Ki,
                Kd = Kd,
                Setting = setting,
                Outputs = new EventedList<Output> {output}
            };
            ValidationResult validationResult = pidRule.Validate(); // ValidationAspects call
            Assert.IsFalse(validationResult.IsValid);
        }

        [Test]
        public void Clone()
        {
            var pidRule = new PIDRule
            {
                Name = RuleName,
                //IsAConstant = true,
                //ConstantValue = 1.0,
                Kp = Kp,
                Ki = Ki,
                Kd = Kd,
                Setting = setting,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output}
            };

            var clone = (PIDRule) pidRule.Clone();

            Assert.AreEqual(pidRule.Name, clone.Name);
            Assert.AreEqual(pidRule.Kp, clone.Kp);
            Assert.AreEqual(pidRule.Ki, clone.Ki);
            Assert.AreEqual(pidRule.Kd, clone.Kd);
            Assert.IsNotNull(clone.Setting);

            clone.Name = "";
            clone.Kp = -1;
            clone.Ki = -1;
            clone.Kd = -1;

            Assert.AreNotEqual(pidRule.Name, clone.Name);
            Assert.AreNotEqual(pidRule.Kp, clone.Kp);
            Assert.AreNotEqual(pidRule.Ki, clone.Ki);
            Assert.AreNotEqual(pidRule.Kd, clone.Kd);
        }

        [Test]
        public void CopyFrom()
        {
            var pidRuleSource = new PIDRule
            {
                Name = RuleName,
                Kp = Kp,
                Ki = Ki,
                Kd = Kd,
                Setting = setting,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output}
            };

            var pidRule = new PIDRule();

            pidRule.CopyFrom(pidRuleSource);

            Assert.AreEqual(RuleName, pidRule.Name);
            Assert.AreEqual(Kp, pidRule.Kp);
            Assert.AreEqual(Ki, pidRule.Ki);
            Assert.AreEqual(Kd, pidRule.Kd);
            Assert.AreEqual(setting.Min, pidRule.Setting.Min);
            Assert.AreEqual(setting.Max, pidRule.Setting.Max);
            Assert.AreEqual(setting.MaxSpeed, pidRule.Setting.MaxSpeed);
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = new PIDRule
            {
                Name = RuleName,
                Kp = Kp,
                Ki = Ki,
                Kd = Kd,
                Setting = setting,
                Inputs = new EventedList<IInput> {input},
                Outputs = new EventedList<Output> {output}
            };

            var newRule = new PIDRule();
            newRule.CopyFrom(source);

            Assert.AreEqual(RuleName, newRule.Name);
            Assert.AreEqual(Kp, newRule.Kp);
            Assert.AreEqual(Ki, newRule.Ki);
            Assert.AreEqual(Kd, newRule.Kd);
            Assert.AreEqual(setting.Min, newRule.Setting.Min);
            Assert.AreEqual(setting.Max, newRule.Setting.Max);
            Assert.AreEqual(setting.MaxSpeed, newRule.Setting.MaxSpeed);

            var clone = (PIDRule) source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }
    }
}