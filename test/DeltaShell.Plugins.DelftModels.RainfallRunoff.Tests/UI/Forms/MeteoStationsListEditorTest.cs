using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.NetworkEditor.Gui;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Forms
{
    [TestFixture]
    public class MeteoStationsListEditorTest
    {
        [Test]
        [Category(TestCategory.WindowsForms)]
        public void ShowMeteoStationList()
        {
            var meteoList = new MeteoStationsListEditor {Data = new EventedList<string>(new[] {"a", "b", "c"})};

            WindowsFormsTestHelper.ShowModal(meteoList);
        }
    }
}