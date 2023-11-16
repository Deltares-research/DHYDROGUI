using System;
using System.Collections.Generic;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DelftTools.Units;
using DelftTools.Utils.Editing;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.DataObjects;
using DeltaShell.Plugins.DelftModels.WaterQualityModel.Utils;
using GeoAPI.Extensions.Coverages;
using NUnit.Framework;
using Rhino.Mocks;

namespace DeltaShell.Plugins.DelftModels.WaterQualityModel.Tests.Utils
{
    [TestFixture]
    public class FunctionTypeCreatorFactoryTest
    {
        [Test]
        public void TestCreateContstantCreator()
        {
            var function = new Function();
            function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE] = "description";
            function.Components.Add(new Variable<double>("Var1") {Unit = new Unit("unit", "unit")});

            IFunctionTypeCreator constantTypeCreator = FunctionTypeCreatorFactory.CreateConstantCreator();

            Assert.AreEqual("Constant", constantTypeCreator.FunctionTypeName);
            Assert.IsFalse(constantTypeCreator.IsThisFunctionType(function));
            Assert.IsTrue(constantTypeCreator.IsAllowed(null));

            IFunction constFunction = constantTypeCreator.TransformToFunctionType(function);

            Assert.AreEqual(function.Name, constFunction.Name);
            Assert.IsTrue(constantTypeCreator.IsThisFunctionType(constFunction));
            Assert.AreEqual(function.Name, constFunction.Name);
            Assert.AreEqual(function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE], constFunction.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
            Assert.AreEqual(1, constFunction.Arguments.Count);
            Assert.AreEqual(1, constFunction.Components.Count);
            Assert.AreEqual(function.Components[0].Name, constFunction.Components[0].Name);
            Assert.AreEqual(function.Components[0].Unit.Name, constFunction.Components[0].Unit.Name);

            constantTypeCreator.SetDefaultValueForFunction(constFunction, 3.0);
            Assert.AreEqual(3.0, constantTypeCreator.GetDefaultValueForFunction(constFunction));

            constantTypeCreator.SetUnitForFunction(constFunction, "Test");
            Assert.AreEqual("Test", constantTypeCreator.GetUnitForFunction(constFunction));
        }

        [Test]
        public void TestCreateTimeseriesCreator()
        {
            var function = new Function();
            function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE] = "description";
            IFunctionTypeCreator timeSeriesTypeCreator = FunctionTypeCreatorFactory.CreateTimeseriesCreator();
            function.Components.Add(new Variable<double>("Var1") {Unit = new Unit("unit", "unit")});

            Assert.AreEqual("Time series", timeSeriesTypeCreator.FunctionTypeName);
            Assert.IsFalse(timeSeriesTypeCreator.IsThisFunctionType(function));
            Assert.IsTrue(timeSeriesTypeCreator.IsAllowed(null));

            IFunction timeSeries = timeSeriesTypeCreator.TransformToFunctionType(function);

            Assert.IsTrue(timeSeriesTypeCreator.IsThisFunctionType(timeSeries));
            Assert.AreEqual(function.Name, timeSeries.Name);
            Assert.AreEqual(function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE],
                            timeSeries.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
            Assert.AreEqual(1, timeSeries.Arguments.Count);
            Assert.AreEqual(typeof(DateTime), timeSeries.Arguments[0].ValueType);
            Assert.AreEqual(1, timeSeries.Components.Count);
            Assert.AreEqual(function.Components[0].Name, timeSeries.Components[0].Name);
            Assert.AreEqual(function.Components[0].Unit.Name, timeSeries.Components[0].Unit.Name);

            timeSeriesTypeCreator.SetDefaultValueForFunction(timeSeries, 3.0);
            Assert.AreEqual(3.0, timeSeriesTypeCreator.GetDefaultValueForFunction(timeSeries));

            timeSeriesTypeCreator.SetUnitForFunction(timeSeries, "Test");
            Assert.AreEqual("Test", timeSeriesTypeCreator.GetUnitForFunction(timeSeries));
        }

        [Test]
        public void TestCreateNetworkCoverageCreator()
        {
            var function = new Function("FunctionName");
            function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE] = "description";
            IFunctionTypeCreator networkCoverageTypeCreator = FunctionTypeCreatorFactory.CreateNetworkCoverageCreator();
            function.Components.Add(new Variable<double>("Var1") {Unit = new Unit("unit", "unit")});

            Assert.AreEqual("Coverage", networkCoverageTypeCreator.FunctionTypeName);
            Assert.IsFalse(networkCoverageTypeCreator.IsThisFunctionType(function));
            Assert.IsTrue(networkCoverageTypeCreator.IsAllowed(null));

            IFunction networkCoverage = networkCoverageTypeCreator.TransformToFunctionType(function);

            Assert.IsTrue(networkCoverageTypeCreator.IsThisFunctionType(networkCoverage));
            Assert.AreEqual(function.Name, networkCoverage.Name);
            Assert.AreEqual(function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE],
                            networkCoverage.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
            Assert.AreEqual(1, networkCoverage.Arguments.Count);
            Assert.AreEqual(typeof(INetworkLocation), networkCoverage.Arguments[0].ValueType);
            Assert.AreEqual(1, networkCoverage.Components.Count);
            Assert.AreEqual(function.Components[0].Name, networkCoverage.Components[0].Name);
            Assert.AreEqual(function.Components[0].Unit.Name, networkCoverage.Components[0].Unit.Name);

            networkCoverageTypeCreator.SetDefaultValueForFunction(networkCoverage, 3.0);
            Assert.AreEqual(3.0, networkCoverageTypeCreator.GetDefaultValueForFunction(networkCoverage));

            networkCoverageTypeCreator.SetUnitForFunction(networkCoverage, "Test");
            Assert.AreEqual("Test", networkCoverageTypeCreator.GetUnitForFunction(networkCoverage));
        }

        [Test]
        public void TestCreateSegmentFilesCreator()
        {
            var function = new Function("FunctionName");
            function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE] = "description";
            IFunctionTypeCreator segmentFileTypeCreator = FunctionTypeCreatorFactory.CreateSegmentFileCreator();
            function.Components.Add(new Variable<double>("Var1") {Unit = new Unit("unit", "unit")});

            Assert.AreEqual("Segment function", segmentFileTypeCreator.FunctionTypeName);
            Assert.IsFalse(segmentFileTypeCreator.IsThisFunctionType(function));
            Assert.IsTrue(segmentFileTypeCreator.IsAllowed(null));

            IFunction segmentFile = segmentFileTypeCreator.TransformToFunctionType(function);

            Assert.IsTrue(segmentFileTypeCreator.IsThisFunctionType(segmentFile));
            Assert.That(function.Name, Is.EqualTo(segmentFile.Name));

            Assert.That(segmentFile.Attributes.Count, Is.EqualTo(1));
            Assert.That(segmentFile.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE],
                        Is.EqualTo("description"));

            //Components
            Assert.That(segmentFile.Components.Count, Is.EqualTo(1));
            Assert.AreEqual(function.Components[0].Name, segmentFile.Components[0].Name);
            Assert.AreEqual(function.Components[0].Unit.Name, segmentFile.Components[0].Unit.Name);

            //setting value
            string file = TestHelper.GetTestFilePath(@"TestCreateSegmentFilesCreator");
            segmentFileTypeCreator.SetUrlForFunction(segmentFile, file);
            Assert.That(segmentFileTypeCreator.GetUrlForFunction(segmentFile), Is.EqualTo(file));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestCreateFunctionFromHydroDynamicsCreator(bool expectedBool)
        {
            var function = new Function("FunctionName");
            function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE] = "description";
            function.Components.Add(new Variable<double>("Var1") {Unit = new Unit("unit", "unit")});

            const string expectedFilePath = "<a file path>";

            IFunctionTypeCreator fromHydroDynamicsCreator = FunctionTypeCreatorFactory.CreateFunctionFromHydroDynamicsCreator(
                f => expectedBool, f => expectedFilePath);

            Assert.AreEqual("From hydro data", fromHydroDynamicsCreator.FunctionTypeName);
            Assert.IsFalse(fromHydroDynamicsCreator.IsThisFunctionType(function));
            Assert.AreEqual(expectedBool, fromHydroDynamicsCreator.IsAllowed(null));

            var functionFromHydoData = (FunctionFromHydroDynamics) fromHydroDynamicsCreator.TransformToFunctionType(function);

            Assert.AreEqual(expectedFilePath, functionFromHydoData.FilePath);
            Assert.IsTrue(fromHydroDynamicsCreator.IsThisFunctionType(functionFromHydoData));
            Assert.AreEqual(function.Name, functionFromHydoData.Name);
            Assert.AreEqual(function.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE],
                            functionFromHydoData.Attributes[WaterQualityFunctionFactory.DESCRIPTION_ATTRIBUTE]);
            Assert.AreEqual(0, functionFromHydoData.Arguments.Count);
            Assert.AreEqual(1, functionFromHydoData.Components.Count);
            Assert.AreEqual(function.Components[0].Name, functionFromHydoData.Components[0].Name);
            Assert.AreEqual(function.Components[0].Unit.Name, functionFromHydoData.Components[0].Unit.Name);

            fromHydroDynamicsCreator.SetDefaultValueForFunction(functionFromHydoData, 3.0);
            Assert.AreEqual(3.0, fromHydroDynamicsCreator.GetDefaultValueForFunction(functionFromHydoData));

            fromHydroDynamicsCreator.SetUnitForFunction(functionFromHydoData, "Test");
            Assert.AreEqual("Test", fromHydroDynamicsCreator.GetUnitForFunction(functionFromHydoData));
        }

        [Test]
        public void TestReplaceFunctionUsingCreator()
        {
            // setup
            IFunction function1 = WaterQualityFunctionFactory.CreateConst("A", 1.1, "B", "C", "A");
            IFunction function2 = WaterQualityFunctionFactory.CreateConst("D", 2.2, "E", "F", "D");
            IFunction function3 = WaterQualityFunctionFactory.CreateConst("G", 3.3, "H", "I", "G");
            var functionCollection = new List<IFunction>
            {
                function1,
                function2,
                function3
            };

            var mocks = new MockRepository();
            var creator = mocks.StrictMock<IFunctionTypeCreator>();
            creator.Expect(c => c.TransformToFunctionType(function2))
                   .Return(WaterQualityFunctionFactory.CreateConst("Everything", 1.1, "is", "awesome!", "alrighty then"));
            creator.Expect(c => c.FunctionTypeName).Return("<functionTypeName>");

            var dataOwner = mocks.StrictMock<IEditableObject>();
            dataOwner.Expect(d => d.BeginEdit("")).IgnoreArguments().WhenCalled(mi =>
            {
                var editActionName = (string) mi.Arguments[0];
                Assert.AreEqual("Changing function type of D const to <functionTypeName>", editActionName);
            });
            dataOwner.Expect(d => d.EndEdit());
            mocks.ReplayAll();

            // call
            IFunction newFunction = FunctionTypeCreator.ReplaceFunctionUsingCreator(functionCollection,
                                                                                    function2, creator, dataOwner, "const ");

            // assert
            Assert.AreNotSame(function2, newFunction);
            Assert.AreSame(function1, functionCollection[0]);
            Assert.AreSame(newFunction, functionCollection[1],
                           "Instance should have been replace with newly created function.");
            Assert.AreSame(function3, functionCollection[2]);

            mocks.VerifyAll();
        }
    }
}