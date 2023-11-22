using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.IntegrationTestUtils;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Gui.Forms;
using Netron.GraphLib;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class ControlGroupEditorViewContextTests
    {
        [Test]
        [Category(TestCategory.Integration)]
        [Category(TestCategory.Slow)]
        public void RestoredViewContextIsCorrectBoundToDomainObjects()
        {
            using (var gui = DeltaShellCoreFactory.CreateGui())
            {
                gui.Run();
                gui.Application.CreateNewProject();
                
                var controlGroup = new ControlGroup();
                controlGroup.Rules.Add(new PIDRule {Name = "testRule"});

                // ControlGroupEditor is composed in CompositeView (ControlGroupGraphView)
                // :: ControlGroupEditorViewContext.ViewType should return type of View, not CompositeParent, but this doesn't work yet.
                // var view = new ControlGroupEditor { Data = controlGroup };

                var model = new RealTimeControlModel {ControlGroups = {controlGroup}};
                var view = new ControlGroupGraphView
                {
                    Data = controlGroup,
                    Model = model
                };

                gui.DocumentViews.Add(view);

                Shape shape = view.ControlGroupEditor.GraphControl.GetShapes<Shape>().First();
                ControlGroupEditorController.MoveShape(shape, 10, 20);
                Assert.IsNotNull(shape.Tag);
                Assert.AreEqual(10, shape.X);
                Assert.AreEqual(20, shape.Y);

                gui.DocumentViews.Remove(view);
                var newView = new ControlGroupGraphView
                {
                    Data = controlGroup,
                    Model = model
                };
                gui.DocumentViews.Add(newView);

                shape = newView.ControlGroupEditor.GraphControl.GetShapes<Shape>().First();
                Assert.IsNotNull(shape.Tag);
                Assert.AreEqual(10, shape.X);
                Assert.AreEqual(20, shape.Y);

                ControlGroupEditorController.MoveShape(shape, 100, 200);
                Assert.IsNotNull(shape.Tag);
                Assert.AreEqual(100, shape.X);
                Assert.AreEqual(200, shape.Y);

                gui.DocumentViews.Remove(view);

                var newerView = new ControlGroupGraphView
                {
                    Data = controlGroup,
                    Model = model
                };
                gui.DocumentViews.Add(newerView);

                shape = newerView.ControlGroupEditor.GraphControl.GetShapes<Shape>().First();
                Assert.IsNotNull(shape.Tag);
                Assert.AreEqual(100, shape.X);
                Assert.AreEqual(200, shape.Y);
            }
        }
    }
}