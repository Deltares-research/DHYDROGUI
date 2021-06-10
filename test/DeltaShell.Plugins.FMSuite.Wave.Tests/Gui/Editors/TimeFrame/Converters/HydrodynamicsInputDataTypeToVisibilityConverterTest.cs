using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters;
using DeltaShell.Plugins.FMSuite.Wave.TimeFrame;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Converters
{
    [TestFixture]
    public class HydrodynamicsInputDataTypeToVisibilityConverterTest :
        EnumConverterTestFixture<HydrodynamicsInputDataType>
    {
        protected override bool CollapseHidden => true;
        protected override bool InvertVisibility => false;

        protected override EnumToVisibilityConverter<HydrodynamicsInputDataType> CreateConverter() =>
            new HydrodynamicsInputDataTypeToVisibilityConverter();
    }
}