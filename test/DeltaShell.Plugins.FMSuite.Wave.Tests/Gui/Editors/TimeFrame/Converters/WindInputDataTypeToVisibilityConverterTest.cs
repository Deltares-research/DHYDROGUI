using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Converters
{
    [TestFixture]
    public class WindInputDataTypeToVisibilityConverterTest :
        EnumConverterTestFixture<WindInputDataType>
    {
        protected override bool CollapseHidden => true;
        protected override bool InvertVisibility => false;

        protected override EnumToVisibilityConverter<WindInputDataType> CreateConverter() =>
            new WindInputDataTypeToVisibilityConverter();
    }
}