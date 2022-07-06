using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Controls;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Editors.MeteoDataEditor.Adapters;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.UI.Editors.MeteoDataEditor.Adapters
{
    [TestFixture]
    public class TableViewMeteoStationSelectionAdapterTest
    {
        [Test]
        public void Constructor_ExpectedResult()
        {
            var tableView = Substitute.For<ITableView>();
            var adapter = new TableViewMeteoStationSelectionAdapter(tableView);
            Assert.That(adapter, Is.InstanceOf<ITableViewMeteoStationSelectionAdapter>());
        }

        [Test]
        public void Constructor_TableViewNull_ThrowsArgumentNullException()
        {
            void Call() => new TableViewMeteoStationSelectionAdapter(null);
            Assert.That(Call, Throws.ArgumentNullException);
        }

        [Test]
        public void SetSelection_ColumnsNull_DoesNothing()
        {
            // Setup
            var tableView = Substitute.For<ITableView>();
            tableView.Columns.Returns(_ => null);
            var adapter = new TableViewMeteoStationSelectionAdapter(tableView);

            // Call
            adapter.SetSelection(new[] { "a", "b", "c"});

            // Assert
            IList<ITableViewColumn> __ = tableView.Received(1).Columns;
            Assert.That(tableView.ReceivedCalls().Count(), Is.EqualTo(1));
        }

        private static ITableViewColumn CreateColumn(string name)
        {
            var column = Substitute.For<ITableViewColumn>();
            column.Name.Returns(name);
            return column;
        }

        [Test]
        public void SetSelection_EmptySet_ClearsSelection()
        {
            // Setup
            var tableView = Substitute.For<ITableView>();
            IDictionary<string, ITableViewColumn> columnsDict = 
                Enumerable.Range(0, 5)
                          .Select(i => CreateColumn(i.ToString()))
                          .ToDictionary(v => v.Name);
            IList<ITableViewColumn> columns = columnsDict.Values.ToList();
            tableView.GetColumnByName(Arg.Any<string>())
                     .Returns(x => columnsDict[x.ArgAt<string>(0)]);
            tableView.Columns.Returns(columns);

            var adapter = new TableViewMeteoStationSelectionAdapter(tableView);

            // Call
            adapter.SetSelection(Array.Empty<string>());

            // Assert
            tableView.Received(1).ClearSelection();
            tableView.DidNotReceiveWithAnyArgs().SelectCells(0, 0, 0, 0);
        }

        [Test]
        public void SetSelection_SetsSelectionInTableView()
        {
            // Setup
            const int focusedRowIndex = 5;

            var tableView = Substitute.For<ITableView>();
            IDictionary<string, ITableViewColumn> columnsDict = 
                Enumerable.Range(0, 5)
                          .Select(i => CreateColumn(i.ToString()))
                          .ToDictionary(v => v.Name);
            IList<ITableViewColumn> columns = columnsDict.Values.ToList();
            tableView.GetColumnByName(Arg.Any<string>())
                     .Returns(x => columnsDict[x.ArgAt<string>(0)]);
            tableView.Columns.Returns(columns);
            tableView.FocusedRowIndex = focusedRowIndex;

            var adapter = new TableViewMeteoStationSelectionAdapter(tableView);

            // Call
            adapter.SetSelection(new[] { "2", "4" });

            // Assert
            tableView.Received(1).ClearSelection();
            tableView.ReceivedWithAnyArgs(2).SelectCells(0, 0, 0, 0);
            tableView.Received(1).SelectCells(focusedRowIndex, 2, focusedRowIndex, 2, false);
            tableView.Received(1).SelectCells(focusedRowIndex, 4, focusedRowIndex, 4, false);
        }
    }
}