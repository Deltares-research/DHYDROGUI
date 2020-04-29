using System.Collections.Generic;
using DeltaShell.NGHS.TestUtils;
using DeltaShell.Plugins.FMSuite.Wave.Boundaries.SpectralData;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Boundaries.SpectralData
{
    [TestFixture]
    public class WaveSpectrumShapeTypeTest : EnumValuesTestFixture<WaveSpectrumShapeType>
    {
        protected override IDictionary<WaveSpectrumShapeType, int> ExpectedValueForEnumValues =>
            new Dictionary<WaveSpectrumShapeType, int>
            {
                { WaveSpectrumShapeType.Jonswap, 0 },
                { WaveSpectrumShapeType.PiersonMoskowitz, 1 },
                { WaveSpectrumShapeType.Gauss, 2 },
            };
    }
}