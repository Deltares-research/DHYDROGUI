using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using DeltaShell.Plugins.FMSuite.FlowFM.Sediment;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture]
    public class SedimentFractionsEditorTest
    {
        [Test]
        [Category(TestCategory.Wpf)]
        public void ShowUserControl()
        {
            IEventedList<ISedimentFraction> sedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(3);
            WpfTestHelper.ShowModal(new SedimentFractionsEditor(sedimentFractions, new EventedList<ISedimentProperty>()));
        }
    }
}