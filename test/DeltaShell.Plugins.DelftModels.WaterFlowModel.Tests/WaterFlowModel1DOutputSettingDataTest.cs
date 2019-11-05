using System;
using DeltaShell.NGHS.IO.DataObjects.Model1D;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Tests
{
    [TestFixture]
    public class WaterFlowModel1DOutputSettingDataTest
    {
        [Test]
        public void CopyFrom()
        {
            var source = new WaterFlowModel1DOutputSettingData();
            var target = new WaterFlowModel1DOutputSettingData();
            Assert.AreEqual(target.EngineParameters.Count, source.EngineParameters.Count);
            source.GridOutputTimeStep = new TimeSpan(1, 2, 3, 4);
            source.StructureOutputTimeStep = new TimeSpan(5, 6, 7, 8);
            source.EngineParameters[0].AggregationOptions = AggregationOptions.Maximum;
            Assert.AreNotEqual(target.GridOutputTimeStep, source.GridOutputTimeStep);
            Assert.AreNotEqual(target.StructureOutputTimeStep, source.StructureOutputTimeStep);
            Assert.AreNotEqual(target.EngineParameters[0].AggregationOptions, source.EngineParameters[0].AggregationOptions);
            target.CopyFrom(source);
            Assert.AreEqual(target.GridOutputTimeStep, source.GridOutputTimeStep);
            Assert.AreEqual(target.StructureOutputTimeStep, source.StructureOutputTimeStep);
            Assert.AreEqual(target.EngineParameters[0].AggregationOptions, source.EngineParameters[0].AggregationOptions);
        }
    }
}
