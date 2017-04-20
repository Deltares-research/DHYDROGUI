using System.Linq;
using DelftTools.Hydro.CrossSections;
using DelftTools.TestUtils;
using DelftTools.Utils.UndoRedo;
using NUnit.Framework;

namespace DelftTools.Hydro.Tests
{
    [TestFixture]
    [Category(TestCategory.UndoRedo)]
    public class UndoRedoCrossSectionDataTableTest
    {
        [Test]
        public void AddRowYZ()
        {
            var def = new CrossSectionDefinitionYZ();

            using (var undoManager = new UndoRedoManager(def))
            {
                def.YZDataTable.AddCrossSectionYZRow(5, 5, 5);

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");
                Assert.AreEqual(1, def.YZDataTable.Rows.Count, "#rows");

                undoManager.Undo();

                Assert.AreEqual(0, def.YZDataTable.Rows.Count, "#rows after undo");
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(1, def.YZDataTable.Rows.Count, "#rows after redo");
                Assert.AreEqual(5, def.YZDataTable[0].DeltaZStorage, "value");
            }
        }

        [Test]
        public void RemoveRowYZ()
        {
            var def = new CrossSectionDefinitionYZ();
            def.YZDataTable.AddCrossSectionYZRow(5, 5, 5);
            def.YZDataTable.AddCrossSectionYZRow(6, 5, 5);

            using (var undoManager = new UndoRedoManager(def))
            {
                def.YZDataTable.Rows.RemoveAt(1);

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");
                Assert.AreEqual(1, def.YZDataTable.Rows.Count, "#rows");

                undoManager.Undo();

                Assert.AreEqual(2, def.YZDataTable.Rows.Count, "#rows after undo");
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(1, def.YZDataTable.Rows.Count, "#rows after redo");
                Assert.AreEqual(5, def.YZDataTable[0].DeltaZStorage, "value");
            }
        }

        [Test]
        public void ModifyRowYZ()
        {
            var def = new CrossSectionDefinitionYZ();
            def.YZDataTable.AddCrossSectionYZRow(5, 5, 5);
            def.YZDataTable.AddCrossSectionYZRow(8, 5, 5);

            using (var undoManager = new UndoRedoManager(def))
            {
                def.YZDataTable[0].BeginEdit();
                def.YZDataTable[0].Yq = 9.0;
                def.YZDataTable[0].EndEdit();

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");

                undoManager.Undo();

                Assert.AreEqual(5.0, def.YZDataTable[0].Yq);
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(8.0, def.YZDataTable[0].Yq, "value1");
                Assert.AreEqual(9.0, def.YZDataTable[1].Yq, "value2");
            }
        }

        [Test]
        public void MultipleModifyRowYZ()
        {
            var def = new CrossSectionDefinitionYZ();
            def.YZDataTable.AddCrossSectionYZRow(5, 5, 5);
            def.YZDataTable.AddCrossSectionYZRow(8, 5, 5);

            using (var undoManager = new UndoRedoManager(def))
            {
                def.YZDataTable[0].BeginEdit();
                def.YZDataTable[0].Yq = 9.0;
                def.YZDataTable[0].Z = 10.0;
                def.YZDataTable[0].EndEdit();

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");

                undoManager.Undo();

                Assert.AreEqual(5.0, def.YZDataTable[0].Yq);
                Assert.AreEqual(5.0, def.YZDataTable[0].Z); 
                Assert.AreEqual(8.0, def.YZDataTable[1].Yq);
                Assert.AreEqual(5.0, def.YZDataTable[1].Z);
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(8.0, def.YZDataTable[0].Yq, "value");
                Assert.AreEqual(5.0, def.YZDataTable[0].Z);
                Assert.AreEqual(9.0, def.YZDataTable[1].Yq, "value");
                Assert.AreEqual(10.0, def.YZDataTable[1].Z);
            }
        }

        [Test]
        public void CancelModifyRowYZ()
        {
            var def = new CrossSectionDefinitionYZ();
            def.YZDataTable.AddCrossSectionYZRow(5, 5, 5);
            def.YZDataTable.AddCrossSectionYZRow(8, 5, 5);

            using (var undoManager = new UndoRedoManager(def))
            {
                def.YZDataTable[0].BeginEdit();
                def.YZDataTable[0].Yq = 9.0;
                def.YZDataTable[0].Z = 10.0;
                def.YZDataTable[0].CancelEdit();

                Assert.AreEqual(0, undoManager.UndoStack.Count(), "#undo");

                Assert.AreEqual(5.0, def.YZDataTable[0].Yq);
            }
        }

        [Test]
        public void AddRowZW()
        {
            var def = new CrossSectionDefinitionZW();

            using (var undoManager = new UndoRedoManager(def))
            {
                def.ZWDataTable.AddCrossSectionZWRow(5, 5, 5);

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");
                Assert.AreEqual(1, def.ZWDataTable.Rows.Count, "#rows");

                undoManager.Undo();

                Assert.AreEqual(0, def.ZWDataTable.Rows.Count, "#rows after undo");
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(1, def.ZWDataTable.Rows.Count, "#rows after redo");
                Assert.AreEqual(5, def.ZWDataTable[0].StorageWidth, "value");
            }
        }

        [Test]
        public void RemoveRowZW()
        {
            var def = new CrossSectionDefinitionZW();
            def.ZWDataTable.AddCrossSectionZWRow(5, 5, 5);
            def.ZWDataTable.AddCrossSectionZWRow(6, 5, 5);

            using (var undoManager = new UndoRedoManager(def))
            {
                def.ZWDataTable.Rows.RemoveAt(1);

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");
                Assert.AreEqual(1, def.ZWDataTable.Rows.Count, "#rows");

                undoManager.Undo();

                Assert.AreEqual(2, def.ZWDataTable.Rows.Count, "#rows after undo");
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(1, def.ZWDataTable.Rows.Count, "#rows after redo");
                Assert.AreEqual(5, def.ZWDataTable[0].StorageWidth, "value");
            }
        }

        [Test]
        public void ModifyRowZW()
        {
            var def = new CrossSectionDefinitionZW();
            def.ZWDataTable.AddCrossSectionZWRow(5, 5, 5);
            def.ZWDataTable.AddCrossSectionZWRow(8, 5, 5);

            using (var undoManager = new UndoRedoManager(def))
            {
                def.ZWDataTable[0].BeginEdit();
                def.ZWDataTable[0].Z = 9.0;
                def.ZWDataTable[0].EndEdit();

                Assert.AreEqual(1, undoManager.UndoStack.Count(), "#undo");

                undoManager.Undo();

                Assert.AreEqual(8.0, def.ZWDataTable[0].Z);
                Assert.AreEqual(5.0, def.ZWDataTable[1].Z);
                Assert.AreEqual(1, undoManager.RedoStack.Count(), "#redo");

                undoManager.Redo();

                Assert.AreEqual(9.0, def.ZWDataTable[0].Z, "value1");
                Assert.AreEqual(5.0, def.ZWDataTable[1].Z, "value2");
            }
        }
    }
}