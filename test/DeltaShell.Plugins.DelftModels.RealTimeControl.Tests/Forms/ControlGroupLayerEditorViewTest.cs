using DelftTools.TestUtils;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests.Forms
{
    [TestFixture]
    public class ControlGroupLayerEditorViewTest
    {
        [Test, Category(TestCategory.WindowsForms)]
        public void ShowControlGroupLayerEditorViewWithoutData()
        {
            var view = new ControlGroupLayerEditorView();

            WindowsFormsTestHelper.ShowModal(view);
        }

        [Test, Category(TestCategory.WindowsForms)]
        public void ShowControlGroupLayerEditorViewWithData()
        {
            var view = new ControlGroupLayerEditorView();
            var controlGroupList = new EventedList<ControlGroup>
                                       {
                                           new ControlGroup
                                               {
                                                   Rules = new EventedList<RuleBase>{new PIDRule()},
                                                   Conditions = new EventedList<ConditionBase>{new StandardCondition()}
                                               }
                                       };

            view.Data = controlGroupList;

            WindowsFormsTestHelper.ShowModal(view);
        }
    }
}