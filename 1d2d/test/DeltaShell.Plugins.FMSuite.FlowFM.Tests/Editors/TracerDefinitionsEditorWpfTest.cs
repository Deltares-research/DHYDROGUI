using System.Threading;
using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Tests.Editors
{
    [TestFixture, Apartment(ApartmentState.STA)]
    public class TracerDefinitionsEditorWpfTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void ShowTracerDefinitionsEditorWpfWithData()
        {
            var editor = new TracerDefinitionsEditorWpf
            {
                Tracers = new EventedList<string>{"abc", "def"}
            };

            WpfTestHelper.ShowModal(editor);
        }
    }
}