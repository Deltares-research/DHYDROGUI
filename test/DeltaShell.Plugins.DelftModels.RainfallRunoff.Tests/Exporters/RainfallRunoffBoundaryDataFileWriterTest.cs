using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.Hydro;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileWriters.Boundary;
using DeltaShell.NGHS.IO.FileWriters.General;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Exporters;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.Exporters
{
    [TestFixture]
    class RainfallRunoffBoundaryDataFileWriterTest
    {   
        [Test]
        public void TestRainfallRunoffBoundaryDataFileWriterGivesExpectedResults_BoundaryNodes()
        {
            RainfallRunoffModel model = new RainfallRunoffModel();
            var boundaryNodeData = model.BoundaryData;
            boundaryNodeData.Add(new RunoffBoundaryData(new RunoffBoundary() {Name = "RBC1"}) {Series = new RainfallRunoffBoundaryData() {IsConstant = true, IsTimeSeries = false, Value = 123.4} });

            var t = new DateTime(2000, 1, 1);
            model.StartTime = t;
            model.StopTime = t.AddHours(3);
            model.TimeStep = new TimeSpan(0, 30, 0);
            model.OutputTimeStep = new TimeSpan(0, 30, 0);

            var timeSeries = new TimeSeries();
            timeSeries.Components.Add(new Variable<double>());
            timeSeries[t] = 0.0;
            timeSeries[t.AddHours(1)] = 20.0;
            timeSeries[t.AddHours(2)] = 40.0;
            timeSeries[t.AddHours(3)] = 60.0;
            timeSeries.Arguments[0].InterpolationType = InterpolationType.Constant;
            timeSeries.Arguments[0].ExtrapolationType= ExtrapolationType.Periodic;

            boundaryNodeData.Add(new RunoffBoundaryData(new RunoffBoundary() {Name = "RBC2"}) {Series = new RainfallRunoffBoundaryData() {IsConstant = false, IsTimeSeries = true, Data = timeSeries } });
            var bcFile = Path.GetTempFileName();
            new RainfallRunoffBoundaryDataFileWriter().WriteFile(bcFile, model);

            var delftBcReader = new DelftBcReader();
            var categories = delftBcReader.ReadDelftBcFile(bcFile);
            Assert.AreEqual(1, categories.Count(g => g.Name == GeneralRegion.IniHeader));
            
            // Test BoundaryNode Data
            var boundaryNodeCategories = categories.Where(c => c.Name == BoundaryRegion.BcBoundaryHeader).ToList();
            Assert.AreEqual(2, boundaryNodeCategories.Count); // 2 RR BC

            // Constant WaterLevel
            Assert.AreEqual(2, boundaryNodeCategories[0].Properties.Count);
            Assert.AreEqual("RBC1", boundaryNodeCategories[0].ReadProperty<string>(BoundaryRegion.Name.Key));
            Assert.AreEqual(BoundaryRegion.FunctionStrings.Constant, boundaryNodeCategories[0].ReadProperty<string>(BoundaryRegion.Function.Key));
            Assert.AreEqual(1, boundaryNodeCategories[0].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, boundaryNodeCategories[0].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeCategories[0].Table[0].Unit.Value);
            Assert.AreEqual(1, boundaryNodeCategories[0].Table[0].Values.Count);
            Assert.AreEqual("123.4", boundaryNodeCategories[0].Table[0].Values[0]);

            // WaterLevel TimeSeries
            Assert.AreEqual(4, boundaryNodeCategories[1].Properties.Count);
            Assert.AreEqual("RBC2", boundaryNodeCategories[1].ReadProperty<string>(BoundaryRegion.Name.Key));
            Assert.AreEqual(BoundaryRegion.FunctionStrings.TimeSeries, boundaryNodeCategories[1].ReadProperty<string>(BoundaryRegion.Function.Key));
            Assert.AreEqual(BoundaryRegion.TimeInterpolationStrings.BlockFrom, boundaryNodeCategories[1].ReadProperty<string>(BoundaryRegion.Interpolation.Key));
            Assert.AreEqual(true, boundaryNodeCategories[1].ReadProperty<bool>(BoundaryRegion.Periodic.Key));
            Assert.AreEqual(2, boundaryNodeCategories[1].Table.Count);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.Time, boundaryNodeCategories[1].Table[0].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.TimeMinutes + " " + model.StartTime.ToString(BoundaryRegion.UnitStrings.TimeFormat), boundaryNodeCategories[1].Table[0].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[1].Table[0].Values.Count);
            Assert.AreEqual("0", boundaryNodeCategories[1].Table[0].Values[0]);
            Assert.AreEqual("60", boundaryNodeCategories[1].Table[0].Values[1]);
            Assert.AreEqual("120", boundaryNodeCategories[1].Table[0].Values[2]);
            Assert.AreEqual("180", boundaryNodeCategories[1].Table[0].Values[3]);
            Assert.AreEqual(BoundaryRegion.QuantityStrings.WaterLevelQuantityInRR, boundaryNodeCategories[1].Table[1].Quantity.Value);
            Assert.AreEqual(BoundaryRegion.UnitStrings.WaterLevel, boundaryNodeCategories[1].Table[1].Unit.Value);
            Assert.AreEqual(4, boundaryNodeCategories[1].Table[1].Values.Count);
            Assert.AreEqual("0", boundaryNodeCategories[1].Table[1].Values[0]);
            Assert.AreEqual("20", boundaryNodeCategories[1].Table[1].Values[1]);
            Assert.AreEqual("40", boundaryNodeCategories[1].Table[1].Values[2]);
            Assert.AreEqual("60", boundaryNodeCategories[1].Table[1].Values[3]);
        }
    }
}