using DeltaShell.Dimr.Gui;

namespace DeltaShell.Dimr.Tests
{
    public class TestDimrGuiPlugin : DimrGuiPlugin
    {
        public bool TestValue { get; set; }

        public override bool IsOnlyDimrModelSelected
        {
            get { return TestValue; }
        }
    }
}