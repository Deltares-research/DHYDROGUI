using System;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;
using ValidationAspects;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Domain
{
    [TestFixture]
    public class LookupSignalTests
    {
        private Function tableFunction;

        [SetUp]
        public void SetUp()
        {
            tableFunction = LookupSignal.DefineFunction();
            tableFunction[8.65] = 8.20;
            tableFunction[9.10] = 8.05;
            tableFunction[9.60] = 7.60;
            tableFunction[10.0] = 7.40;
        }

        [Test]
        public void SignalHasValidation()
        {
            var signal = new LookupSignal();

            // Bare Lookup Signal
            var validationResult = signal.Validate();
            var exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(4, exceptionCount);
            Assert.AreEqual(false, validationResult.IsValid);

            // Table Added
            signal.Function = tableFunction;
            validationResult = signal.Validate();
            exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(3, exceptionCount);
            Assert.AreEqual(false, validationResult.IsValid);

            // Input Added
            signal.Inputs.Add(new Input());
            validationResult = signal.Validate();
            exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(2, exceptionCount);
            Assert.AreEqual(false, validationResult.IsValid);

            // Rule Added
            signal.RuleBases.Add(new PIDRule());
            validationResult = signal.Validate();
            exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(0, exceptionCount);
            Assert.AreEqual(true, validationResult.IsValid);
        }

        [Test]
        public void ValidationBareLookupSignal()
        {
            var signal = new LookupSignal();

            // Bare Lookup Signal
            var validationResult = signal.Validate();
            var exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(4, exceptionCount);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [Test]
        public void ValidationTable()
        {
            var signal = new LookupSignal();

            signal.Function = tableFunction;
            var validationResult = signal.Validate();
            var exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(3, exceptionCount);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [Test]
        public void ValidationInput()
        {
            var signal = new LookupSignal();

            // Input Added
            signal.Inputs.Add(new Input());
            var validationResult = signal.Validate();
            var exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(3, exceptionCount);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [Test]
        public void ValidationRule()
        {
            var signal = new LookupSignal();

            // Rule Added
            signal.RuleBases.Add(new PIDRule());
            var validationResult = signal.Validate();
            var exceptionCount = validationResult.Messages.Count();
            Assert.AreEqual(2, exceptionCount);
            Assert.AreEqual(false, validationResult.IsValid);
        }

        [Test]
        public void CopyFromAndClone()
        {
            var source = new LookupSignal
            {
                Name = "signaltest"
            };

            var newSignal = new LookupSignal();
            var argumentValues = new[] { 60, 120.0, 360.0 };
            var componentValues = new[] { 8.0, 9.0, 10.0 };
            for (int i = 0; i < argumentValues.Count(); i++)
            {
                source.Function[argumentValues[i]] = componentValues[i];
            }
            newSignal.CopyFrom(source);

            Assert.AreEqual(source.Name, newSignal.Name);

            for (int i = 0; i < source.Function.Arguments[0].Values.Count; i++)
            {
                Assert.AreEqual(source.Function.Arguments[0].Values[i], newSignal.Function.Arguments[0].Values[i]);
                Assert.AreEqual(source.Function.Components[0].Values[i], newSignal.Function.Components[0].Values[i]);
            }
            Assert.AreEqual(source.Interpolation, newSignal.Interpolation);

            var clone = (LookupSignal)source.Clone();
            Assert.IsFalse(ReferenceEquals(source, clone));
            Assert.AreEqual(source.Name, clone.Name);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void DoNotSupportNoneInterpolation()
        {
            new LookupSignal()
            {
                Name = "test",
                Interpolation = InterpolationType.None
            };
        }


    }

}
