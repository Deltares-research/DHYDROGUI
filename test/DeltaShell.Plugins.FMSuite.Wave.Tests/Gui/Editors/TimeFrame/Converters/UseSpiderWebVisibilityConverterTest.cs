using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.DomainSpecificDataEditor.ViewModels;
using DeltaShell.Plugins.FMSuite.Wave.Gui.Editors.TimeFrame.Converters;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Wave.Tests.Gui.Editors.TimeFrame.Converters
{
    [TestFixture]
    public class UseSpiderWebVisibilityConverterTest :
        EnumConverterTestFixture<WindInputType>
    {
        protected override bool CollapseHidden => true;
        protected override bool InvertVisibility => true;

        protected override EnumToVisibilityConverter<WindInputType> CreateConverter() =>
            new UseSpiderWebVisibilityConverter();
    }
}