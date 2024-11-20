using System;
using System.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Wave.DataAccess;
using DeltaShell.Plugins.FMSuite.Wave.ModelDefinition;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame.DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.DataAccess
{
    [TestFixture]
    public class MdwFileDTOTest
    {
        public static IEnumerable<TestCaseData> GetValidArgumentsData()
        {
            var waveModelDefinition = new WaveModelDefinition();
            var timeFrameData = Substitute.For<ITimeFrameData>();

            void ValidateModelDefinition(MdwFileDTO dto) =>
                Assert.That(dto.WaveModelDefinition, Is.SameAs(waveModelDefinition));

            yield return new TestCaseData(waveModelDefinition,
                                          timeFrameData,
                                          (Action<MdwFileDTO>)ValidateModelDefinition)
                .SetName("Constructor_SetsWaveModelDefinitionCorrectly");

            void ValidateTimeFrameData(MdwFileDTO dto) =>
                Assert.That(dto.TimeFrameData, Is.SameAs(timeFrameData));

            yield return new TestCaseData(waveModelDefinition,
                                          timeFrameData,
                                          (Action<MdwFileDTO>)ValidateTimeFrameData)
                .SetName("Constructor_SetsTimeFrameDataCorrectly");
        }

        [Test]
        [TestCaseSource(nameof(GetValidArgumentsData))]
        public void Constructor_SetsPropertyCorrectly(WaveModelDefinition modelDefinition,
                                                      ITimeFrameData timeFrameData,
                                                      Action<MdwFileDTO> assertCorrectProperty)
        {
            // Call
            var dto = new MdwFileDTO(modelDefinition, timeFrameData);

            // Assert
            assertCorrectProperty(dto);
        }

        public static IEnumerable<TestCaseData> GetInvalidArgumentsData()
        {
            var waveModelDefinition = new WaveModelDefinition();
            var timeFrameData = Substitute.For<ITimeFrameData>();

            yield return new TestCaseData(null, timeFrameData, "waveModelDefinition");
            yield return new TestCaseData(waveModelDefinition, null, "timeFrameData");
        }

        [Test]
        [TestCaseSource(nameof(GetInvalidArgumentsData))]
        public void Constructor_ArgumentNull_ThrowsArgumentNullException(WaveModelDefinition modelDefinition,
                                                                         ITimeFrameData timeFrameData,
                                                                         string expectedParamName)
        {
            // Call | Assert
            void Call() => new MdwFileDTO(modelDefinition, timeFrameData);

            var exception = Assert.Throws<System.ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo(expectedParamName));
        }
    }
}