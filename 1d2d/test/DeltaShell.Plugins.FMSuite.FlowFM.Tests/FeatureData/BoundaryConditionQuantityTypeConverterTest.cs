using DeltaShell.Plugins.FMSuite.FlowFM.FeatureData;
using DeltaShell.Plugins.FMSuite.FlowFM.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.FeatureData
{
    [TestFixture()]
    public class BoundaryConditionQuantityTypeConverterTest
    {
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelFixed, MorphologyBoundaryConditionQuantityType.BedLevelFixed)]
        [TestCase(FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint, MorphologyBoundaryConditionQuantityType.NoBedLevelConstraint)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed, MorphologyBoundaryConditionQuantityType.BedLevelChangeSpecifiedAsFunctionOfTime)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLevelPrescribed, MorphologyBoundaryConditionQuantityType.BedLevelSpecifiedAsFunctionOfTime)]
        [TestCase(FlowBoundaryQuantityType.MorphologyBedLoadTransport, MorphologyBoundaryConditionQuantityType.BedLoadTransportRatePrescribed)]
        [TestCase(FlowBoundaryQuantityType.Discharge, MorphologyBoundaryConditionQuantityType.NoBedLevelConstraint)]
        public void TestConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(FlowBoundaryQuantityType type, MorphologyBoundaryConditionQuantityType expectation)
        {
            Assert.That(BoundaryConditionQuantityTypeConverter.ConvertFlowBoundaryConditionQuantityTypeToMorphologyBoundaryConditionQuantityType(type), Is.EqualTo(expectation));
        }

        [TestCase(MorphologyBoundaryConditionQuantityType.BedLevelFixed, FlowBoundaryQuantityType.MorphologyBedLevelFixed)]
        [TestCase(MorphologyBoundaryConditionQuantityType.NoBedLevelConstraint, FlowBoundaryQuantityType.MorphologyNoBedLevelConstraint)]
        [TestCase(MorphologyBoundaryConditionQuantityType.BedLevelChangeSpecifiedAsFunctionOfTime, FlowBoundaryQuantityType.MorphologyBedLevelChangePrescribed)]
        [TestCase(MorphologyBoundaryConditionQuantityType.BedLevelSpecifiedAsFunctionOfTime, FlowBoundaryQuantityType.MorphologyBedLevelPrescribed)]
        [TestCase(MorphologyBoundaryConditionQuantityType.BedLoadTransportRatePrescribed, FlowBoundaryQuantityType.MorphologyBedLoadTransport)]
        public void TestConvertMorphologyBoundaryConditionQuantityTypeToFlowBoundaryConditionQuantityType(MorphologyBoundaryConditionQuantityType type, FlowBoundaryQuantityType expectation)
        {
            Assert.That(BoundaryConditionQuantityTypeConverter.ConvertMorphologyBoundaryConditionQuantityTypeToFlowBoundaryConditionQuantityType(type), Is.EqualTo(expectation));
        }
    }
}