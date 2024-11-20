using System;
using System.Collections.Generic;
using AutoFixture;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.TimeFrame
{
    [TestFixture]
    public class TimeFrameDataExtensionsTest
    {
        public static IEnumerable<TestCaseData> GetArgumentNullData()
        {
            yield return new TestCaseData(null, Substitute.For<ITimeFrameData>(), "goal");
            yield return new TestCaseData(Substitute.For<ITimeFrameData>(), null, "source");
        }

        [Test]
        [TestCaseSource(nameof(GetArgumentNullData))]
        public void SynchronizeDataWith_ArgumentNull_ThrowsArgumentNullException(ITimeFrameData goal,
                                                                                 ITimeFrameData source,
                                                                                 string expectedParamName)
        {
            void Call() => TimeFrameDataExtensions.SynchronizeDataWith(goal, source);
            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }

        [Test]
        public void SynchronizeDataWith_CopiesDataCorrectly()
        {
            var fixture = new Fixture();
            var source = new TimeFrameData();

            // Input types
            const WindInputDataType windInput = WindInputDataType.FileBased;
            const HydrodynamicsInputDataType hydroInput = HydrodynamicsInputDataType.TimeVarying;

            source.HydrodynamicsInputDataType = hydroInput;
            source.WindInputDataType = windInput;

            // Hydrodynamics Constant Data
            var velocityX = fixture.Create<double>();
            var velocityY = fixture.Create<double>();
            var waterLevel = fixture.Create<double>();

            source.HydrodynamicsConstantData.VelocityX = velocityX;
            source.HydrodynamicsConstantData.VelocityY = velocityY;
            source.HydrodynamicsConstantData.WaterLevel = waterLevel;

            // Wind Constant Data
            var speed = fixture.Create<double>();
            var direction = fixture.Create<double>();

            source.WindConstantData.Speed = speed;
            source.WindConstantData.Direction = direction;

            // Wave Meteo Data
            var xyVectorFilePath = fixture.Create<string>();
            var xComponentFilePath = fixture.Create<string>();
            var yComponentFilePath = fixture.Create<string>();
            var hasSpiderWeb = fixture.Create<bool>();
            var spiderWebFilePath = fixture.Create<string>();

            source.WindFileData.XYVectorFilePath = xyVectorFilePath;
            source.WindFileData.XComponentFilePath = xComponentFilePath;
            source.WindFileData.YComponentFilePath = yComponentFilePath;
            source.WindFileData.HasSpiderWeb = hasSpiderWeb;
            source.WindFileData.SpiderWebFilePath = spiderWebFilePath;

            // Time Varying Data
            source.TimeVaryingData.Arguments[0].SetValues(new[] { DateTime.Today, DateTime.Now });
            source.TimeVaryingData.Components[0].SetValues(new[] { fixture.Create<double>(), fixture.Create<double>() });
            source.TimeVaryingData.Components[1].SetValues(new[] { fixture.Create<double>(), fixture.Create<double>() });
            source.TimeVaryingData.Components[2].SetValues(new[] { fixture.Create<double>(), fixture.Create<double>() });
            source.TimeVaryingData.Components[3].SetValues(new[] { fixture.Create<double>(), fixture.Create<double>() });
            source.TimeVaryingData.Components[4].SetValues(new[] { fixture.Create<double>(), fixture.Create<double>() });

            var goal = new TimeFrameData();

            // Call
            goal.SynchronizeDataWith(source);

            // Assert
            Assert.That(source.WindInputDataType, Is.EqualTo(windInput));
            Assert.That(goal.WindInputDataType, Is.EqualTo(source.WindInputDataType));
            Assert.That(source.HydrodynamicsInputDataType, Is.EqualTo(hydroInput));
            Assert.That(goal.HydrodynamicsInputDataType, Is.EqualTo(source.HydrodynamicsInputDataType));

            Assert.That(source.HydrodynamicsConstantData.WaterLevel, Is.EqualTo(waterLevel));
            Assert.That(goal.HydrodynamicsConstantData.WaterLevel, Is.EqualTo(source.HydrodynamicsConstantData.WaterLevel));
            Assert.That(source.HydrodynamicsConstantData.VelocityX, Is.EqualTo(velocityX));
            Assert.That(goal.HydrodynamicsConstantData.VelocityX, Is.EqualTo(source.HydrodynamicsConstantData.VelocityX));
            Assert.That(source.HydrodynamicsConstantData.VelocityY, Is.EqualTo(velocityY));
            Assert.That(goal.HydrodynamicsConstantData.VelocityY, Is.EqualTo(source.HydrodynamicsConstantData.VelocityY));

            Assert.That(source.WindConstantData.Speed, Is.EqualTo(speed));
            Assert.That(goal.WindConstantData.Speed, Is.EqualTo(source.WindConstantData.Speed));
            Assert.That(source.WindConstantData.Direction, Is.EqualTo(direction));
            Assert.That(goal.WindConstantData.Direction, Is.EqualTo(source.WindConstantData.Direction));

            Assert.That(source.WindFileData.XYVectorFilePath, Is.EqualTo(xyVectorFilePath));
            Assert.That(goal.WindFileData.XYVectorFilePath, Is.EqualTo(source.WindFileData.XYVectorFilePath));
            Assert.That(source.WindFileData.XComponentFilePath, Is.EqualTo(xComponentFilePath));
            Assert.That(goal.WindFileData.XComponentFilePath, Is.EqualTo(source.WindFileData.XComponentFilePath));
            Assert.That(source.WindFileData.YComponentFilePath, Is.EqualTo(yComponentFilePath));
            Assert.That(goal.WindFileData.YComponentFilePath, Is.EqualTo(source.WindFileData.YComponentFilePath));
            Assert.That(source.WindFileData.HasSpiderWeb, Is.EqualTo(hasSpiderWeb));
            Assert.That(goal.WindFileData.HasSpiderWeb, Is.EqualTo(source.WindFileData.HasSpiderWeb));
            Assert.That(source.WindFileData.SpiderWebFilePath, Is.EqualTo(spiderWebFilePath));
            Assert.That(goal.WindFileData.SpiderWebFilePath, Is.EqualTo(source.WindFileData.SpiderWebFilePath));

            Assert.That(source.TimeVaryingData.Arguments[0].Values, Is.EqualTo(goal.TimeVaryingData.Arguments[0].Values));
            Assert.That(source.TimeVaryingData.Components[0].Values, Is.EqualTo(goal.TimeVaryingData.Components[0].Values));
            Assert.That(source.TimeVaryingData.Components[1].Values, Is.EqualTo(goal.TimeVaryingData.Components[1].Values));
            Assert.That(source.TimeVaryingData.Components[2].Values, Is.EqualTo(goal.TimeVaryingData.Components[2].Values));
            Assert.That(source.TimeVaryingData.Components[3].Values, Is.EqualTo(goal.TimeVaryingData.Components[3].Values));
            Assert.That(source.TimeVaryingData.Components[4].Values, Is.EqualTo(goal.TimeVaryingData.Components[4].Values));
        }
    }
}