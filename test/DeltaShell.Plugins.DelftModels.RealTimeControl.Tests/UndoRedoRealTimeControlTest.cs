using System.Linq;
using DelftTools.Utils.UndoRedo;
using DeltaShell.Plugins.DelftModels.RealTimeControl.Domain;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RealTimeControl.Tests
{
    [TestFixture]
    public class UndoRedoRealTimeControlTest
    {
        [Test]
        public void UndoRedoChangeFactorOfFactorRule()
        {
            var factorRule = new FactorRule();

            using (var manager = new UndoRedoManager(factorRule))
            {
                factorRule.Factor = 5;

                Assert.AreEqual(1, manager.UndoStack.Count(), "num undo1");

                manager.Undo();

                Assert.AreEqual(-1, factorRule.Factor, "undo factor");
                Assert.AreEqual(1.0, factorRule.Function[-1.0], "undo function");
                Assert.AreEqual(0, manager.UndoStack.Count(), "num undo2");
                Assert.AreEqual(1, manager.RedoStack.Count(), "num redo1");

                manager.Redo();

                Assert.AreEqual(5, factorRule.Factor, "redo factor");
                Assert.AreEqual(-5, factorRule.Function[-1.0], "redo function");
                Assert.AreEqual(1, manager.UndoStack.Count(), "num undo3");
                Assert.AreEqual(0, manager.RedoStack.Count(), "num redo2");
            }
        }
    }
}