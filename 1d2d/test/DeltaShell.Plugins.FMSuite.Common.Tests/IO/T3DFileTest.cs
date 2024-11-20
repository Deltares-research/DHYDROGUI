using System;
using System.IO;
using System.Linq;
using DelftTools.Functions;
using DelftTools.Functions.Generic;
using DelftTools.TestUtils;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using DeltaShell.Plugins.FMSuite.Common.IO;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests.IO
{
    [TestFixture]
    public class T3DFileTest
    {
        [Test]
        public void ReadT3DFile()
        {
            var path = TestHelper.GetTestFilePath(@"timFiles\testFile.t3D");
            Assert.IsTrue(File.Exists(path));

            var reader = new T3DFile();
            VerticalProfileDefinition verticalProfileDefinition;
            var data=reader.Read(path, out verticalProfileDefinition);

            Assert.AreEqual(VerticalProfileType.PercentageFromBed, verticalProfileDefinition.Type);
            Assert.AreEqual(new[] {0.0, 50.0, 100.0}, verticalProfileDefinition.SortedPointDepths);

            var refDate = new DateTime(2006, 1, 1);

            Assert.AreEqual(
                new[] {refDate, refDate + new TimeSpan(0, 0, 0, 9999999)},
                data.Arguments.First().Values);
            Assert.AreEqual(new[] {40.0, 40.0}, data.Components[0].Values);
            Assert.AreEqual(new[] { 35.0, 35.0 }, data.Components[1].Values);
            Assert.AreEqual(new[] { 30.0, 30.0 }, data.Components[2].Values);
        }

        [Test]
        public void ReadT3DFileSalinity()
        {
            var path = TestHelper.GetTestFilePath(@"timFiles\salinity.t3D");
            Assert.IsTrue(File.Exists(path));

            var reader = new T3DFile();
            VerticalProfileDefinition verticalProfileDefinition;
            reader.Read(path, out verticalProfileDefinition);

            Assert.AreEqual(VerticalProfileType.PercentageFromBed, verticalProfileDefinition.Type);
            Assert.AreEqual(new[]
            {
                0.05, 0.15, 0.25, 0.35, 0.45, 0.55, 0.65, 0.75, 0.85, 0.95
            }, verticalProfileDefinition.SortedPointDepths.Select(x => Math.Round(0.01*x, 3)));
        }

        [Test]
        public void WriteReadSigmaLayeredProfileData()
        {
            var outputProfile = new VerticalProfileDefinition(VerticalProfileType.PercentageFromBed, 0.0, 24.0, 57.0, 88.0,
                93.0);
            var outputData = new Function();
            var refDate = new DateTime(2014, 6, 1);

            outputData.Arguments.Add(new Variable<DateTime>("Time"));
            outputData.Components.Add(new Variable<double>("Layer_1"));
            outputData.Components.Add(new Variable<double>("Layer_2"));
            outputData.Components.Add(new Variable<double>("Layer_3"));
            outputData.Components.Add(new Variable<double>("Layer_4"));
            outputData.Components.Add(new Variable<double>("Layer_5"));
            
            outputData.Arguments[0].Values.AddRange(new[]
            {refDate, refDate.AddHours(1), refDate.AddHours(1).AddMinutes(5)});
            outputData.Components[0].SetValues(new[] {10.0, 12.1, 14.2});
            outputData.Components[1].SetValues(new[] { 11.3, 14.2, 13.4 });
            outputData.Components[2].SetValues(new[] { 12.6, 16.3, 12.6 });
            outputData.Components[3].SetValues(new[] { 13.9, 18.4, 11.8 });
            outputData.Components[4].SetValues(new[] { 14.2, 10.5, 10.0 });

            var writer = new T3DFile();
            writer.Write("test3DFile", outputData, outputProfile, refDate);

            VerticalProfileDefinition inputProfile;
            var inputData = writer.Read("test3DFile", out inputProfile);

            Assert.AreEqual(outputProfile.Type, inputProfile.Type);
            for (var i = 0; i < inputProfile.ProfilePoints; ++i)
            {
                Assert.AreEqual(outputProfile.SortedPointDepths.ElementAt(i),
                    inputProfile.SortedPointDepths.ElementAt(i), 0.000001);
            }
            Assert.AreEqual(outputData.Arguments.Count, inputData.Arguments.Count);
            for (var i = 0; i < outputData.Arguments.Count; ++i)
            {
                Assert.AreEqual(outputData.Arguments[i].Values, inputData.Arguments[i].Values);
            }
            Assert.AreEqual(outputData.Components.Count, inputData.Components.Count);
            for (var i = 0; i < outputData.Components.Count; ++i)
            {
                Assert.AreEqual(outputData.Components[i].Values, inputData.Components[i].Values);
            }
        }

        [Test]
        public void WriteReadZLayeredProfileData()
        {
            var outputProfile = new VerticalProfileDefinition(VerticalProfileType.ZFromBed, 0.0, 2.4, 5.7, 8.8,
                9.3);
            var outputData = new Function();
            var refDate = new DateTime(2014, 6, 1);

            outputData.Arguments.Add(new Variable<DateTime>("Time"));
            outputData.Components.Add(new Variable<double>("Layer_1"));
            outputData.Components.Add(new Variable<double>("Layer_2"));
            outputData.Components.Add(new Variable<double>("Layer_3"));
            outputData.Components.Add(new Variable<double>("Layer_4"));
            outputData.Components.Add(new Variable<double>("Layer_5"));

            outputData.Arguments[0].Values.AddRange(new[] { refDate, refDate.AddHours(1), refDate.AddHours(1).AddMinutes(5) });
            outputData.Components[0].SetValues(new[] { 10.0, 12.1, 14.2 });
            outputData.Components[1].SetValues(new[] { 11.3, 14.2, 13.4 });
            outputData.Components[2].SetValues(new[] { 12.6, 16.3, 12.6 });
            outputData.Components[3].SetValues(new[] { 13.9, 18.4, 11.8 });
            outputData.Components[4].SetValues(new[] { 14.2, 10.5, 10.0 });

            var writer = new T3DFile();
            writer.Write("test3DFile", outputData, outputProfile, refDate);

            VerticalProfileDefinition inputProfile;
            var inputData = writer.Read("test3DFile", out inputProfile);

            Assert.AreEqual(outputProfile.Type, inputProfile.Type);
            Assert.AreEqual(outputProfile.SortedPointDepths, inputProfile.SortedPointDepths);
            Assert.AreEqual(outputData.Arguments.Count, inputData.Arguments.Count);
            for (var i = 0; i < outputData.Arguments.Count; ++i)
            {
                Assert.AreEqual(outputData.Arguments[i].Values, inputData.Arguments[i].Values);
            }
            Assert.AreEqual(outputData.Components.Count, inputData.Components.Count);
            for (var i = 0; i < outputData.Components.Count; ++i)
            {
                Assert.AreEqual(outputData.Components[i].Values, inputData.Components[i].Values);
            }
        }
    }
}
