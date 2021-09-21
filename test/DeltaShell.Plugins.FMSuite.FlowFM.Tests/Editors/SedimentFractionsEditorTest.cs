using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class SedimentFractionsEditorTest
    {
        [Category(TestCategory.WindowsForms)]
        [Test]
        public void ShowUserControl()
        {
            var sedimentFractions = SedimentFractionsEditorTestHelper.GetExampleSedimentFractions(3);
            WpfTestHelper.ShowModal(new SedimentFractionsEditor(sedimentFractions, new EventedList<ISedimentProperty>()));
        }
    }

}
