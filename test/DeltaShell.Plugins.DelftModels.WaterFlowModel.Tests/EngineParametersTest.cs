using System.Linq;
using System.Text.RegularExpressions;
using DelftTools.Hydro;
using DelftTools.Hydro.Structures;
using DelftTools.Hydro.Structures.WeirFormula;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.Plugins.DelftModels.WaterFlowModel.ModelApiControllers.ModelApi;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class EngineParametersTest
    {
        [Test]
        public void RegexTest()
        {
            var regex = new Regex("(.+?,)+?", RegexOptions.Multiline);
        }

        [Test]
        public void InitialWeirValues()
        {
            var weir = new Weir {WeirFormula = new GatedWeirFormula(), CrestLevel = 1.4, CrestWidth = 1.5};
            var gate = (IGatedWeirFormula) weir.WeirFormula;
            gate.GateOpening = 0.33;

            Assert.AreEqual(weir.CrestLevel, EngineParameters.GetInitialValue(weir, QuantityType.CrestLevel));
            Assert.AreEqual(weir.CrestWidth, EngineParameters.GetInitialValue(weir, QuantityType.CrestWidth));
            Assert.AreEqual(gate.GateOpening, EngineParameters.GetInitialValue(weir, QuantityType.GateOpeningHeight));
            Assert.AreEqual(weir.CrestLevel + gate.GateOpening
                , EngineParameters.GetInitialValue(weir, QuantityType.GateLowerEdgeLevel));
        }

        [Test]
        public void InitialCulvertValues()
        {
            var culvert = new Culvert {IsGated = true, GateInitialOpening = 0.43};
            Assert.AreEqual(culvert.GateInitialOpening, EngineParameters.GetInitialValue(culvert, QuantityType.ValveOpening));
        }

        [Test]
        public void InitialPumpValues()
        {
            var pump = new Pump{ Capacity = 1.7 };
            Assert.AreEqual(pump.Capacity, EngineParameters.GetInitialValue(pump, QuantityType.Setpoint));
        }

        //[Test]
        //public void InitialLateralSourceValues()
        //{
        //    var lateralSource = new LateralSource{};
        //    Assert.AreEqual(-100.0, EngineParameters.GetInitialValue(lateralSource, QuantityType.Discharge));
        //}

        [Test]
        public void AllowedOutputForSimpleWeir()
        {
            var engineParameters =
                EngineParameters.EngineMapping().Where(ep => (ep.Role & DataItemRole.Input) == DataItemRole.Input);
            var weir = new Weir();
            Assert.AreEqual(2, engineParameters.Count(ep => ((ep.ElementSet == ElementSet.Structures) && (EngineParameters.AllowedAsQuantityTypeForFeature(weir, ep)))));

            Assert.IsTrue(
                EngineParameters.AllowedAsQuantityTypeForFeature(weir,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.CrestLevel &&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
            Assert.IsTrue(
                EngineParameters.AllowedAsQuantityTypeForFeature(weir,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.CrestWidth &&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
        }

        [Test]
        public void AllowedOutputForGatedWeir()
        {
            var engineParameters = EngineParameters.EngineMapping().Where(ep => (ep.Role & DataItemRole.Input) == DataItemRole.Input);
            var weir = new Weir { WeirFormula = new GatedWeirFormula() };
            Assert.AreEqual(4, engineParameters.Count(ep => ((ep.ElementSet == ElementSet.Structures) && (EngineParameters.AllowedAsQuantityTypeForFeature(weir, ep)))));

            Assert.IsTrue(
                EngineParameters.AllowedAsQuantityTypeForFeature(weir,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.CrestLevel &&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
            Assert.IsTrue(
                EngineParameters.AllowedAsQuantityTypeForFeature(weir,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.CrestWidth &&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
            Assert.IsTrue(
                EngineParameters.AllowedAsQuantityTypeForFeature(weir,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.GateLowerEdgeLevel &&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
            Assert.IsTrue(
                EngineParameters.AllowedAsQuantityTypeForFeature(weir,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.GateOpeningHeight &&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
        }

        [Test]
        public void AllowedOutputForSimpleCulvert()
        {
            var engineParameters = EngineParameters.EngineMapping().Where(ep => (ep.Role & DataItemRole.Input) == DataItemRole.Input);
            var culvert = new Culvert();
            Assert.AreEqual(0, engineParameters.Count(ep => ((ep.ElementSet == ElementSet.Structures) && (EngineParameters.AllowedAsQuantityTypeForFeature(culvert, ep)))));

        }

        [Test]
        public void AllowedOutputForGatedCulvert()
        {
            var engineParameters = EngineParameters.EngineMapping().Where(ep => (ep.Role & DataItemRole.Input) == DataItemRole.Input);
            var culvert = new Culvert { IsGated = true };
            Assert.AreEqual(1, engineParameters.Count(ep => ((ep.ElementSet == ElementSet.Structures) && (EngineParameters.AllowedAsQuantityTypeForFeature(culvert, ep)))));

            Assert.IsTrue(
                EngineParameters.AllowedAsQuantityTypeForFeature(culvert,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.ValveOpening&&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
        }

        [Test]
        public void AllowedOutputForPump()
        {
            var engineParameters = EngineParameters.EngineMapping().Where(ep => (ep.Role & DataItemRole.Input) == DataItemRole.Input);
            var pump = new Pump();

            Assert.AreEqual(1, engineParameters.Count(ep => ((ep.ElementSet == ElementSet.Structures) && (EngineParameters.AllowedAsQuantityTypeForFeature(pump, ep)))));

            Assert.IsTrue(condition: EngineParameters.AllowedAsQuantityTypeForFeature(pump,
                                                           engineParameters.FirstOrDefault(ep => ep.QuantityType == QuantityType.Setpoint &&
                                                                                                 ep.ElementSet == ElementSet.Structures)));
        }

        [Test]
        public void AllowedOutputForLateralSource()
        {
            var engineParameters = EngineParameters.EngineMapping()
                .Where(ep => (ep.Role & DataItemRole.Output) == DataItemRole.Output)
                .ToList();

            var lateralSource = new LateralSource();

            Assert.AreEqual(4, engineParameters.Count(ep => ((ep.ElementSet == ElementSet.Laterals) 
                && (EngineParameters.AllowedAsQuantityTypeForFeature(lateralSource, ep)))));

            Assert.IsTrue(EngineParameters.AllowedAsQuantityTypeForFeature(lateralSource, engineParameters
                .FirstOrDefault(ep => ep.QuantityType == QuantityType.ActualDischarge && ep.ElementSet == ElementSet.Laterals)));

            Assert.IsTrue(EngineParameters.AllowedAsQuantityTypeForFeature(lateralSource, engineParameters
                .FirstOrDefault(ep => ep.QuantityType == QuantityType.DefinedDischarge && ep.ElementSet == ElementSet.Laterals)));

            Assert.IsTrue(EngineParameters.AllowedAsQuantityTypeForFeature(lateralSource, engineParameters
                .FirstOrDefault(ep => ep.QuantityType == QuantityType.LateralDifference && ep.ElementSet == ElementSet.Laterals)));

            Assert.IsTrue(EngineParameters.AllowedAsQuantityTypeForFeature(lateralSource, engineParameters
                .FirstOrDefault(ep => ep.QuantityType == QuantityType.WaterLevel && ep.ElementSet == ElementSet.Laterals)));
        }

        [Test]
        public void CanGetStandardNameForAllParameters()
        {
            var engineParameters = EngineParameters.EngineMapping();

            foreach(var param in engineParameters)
            {
                Assert.IsFalse(string.IsNullOrEmpty(EngineParameters.GetStandardName(param.QuantityType, param.ElementSet)));
            }
        }
    }
}
