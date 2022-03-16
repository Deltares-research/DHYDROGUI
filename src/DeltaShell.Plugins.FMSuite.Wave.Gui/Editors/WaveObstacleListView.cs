using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Shell.Gui;
using GeoAPI.Extensions.Feature;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class WaveObstacleListView : UserControl, ILayerEditorView
    {
        private readonly TableView tableView;
        private readonly ITableViewColumn obsTypeColumn;
        private readonly ITableViewColumn transCoefColumn;
        private readonly ITableViewColumn heightColumn;
        private readonly ITableViewColumn alphaColumn;
        private readonly ITableViewColumn betaColumn;
        private readonly ITableViewColumn reflTypeColumn;
        private readonly ITableViewColumn reflCoefColumn;

        private IList<WaveObstacle> data;

        public event EventHandler SelectedFeaturesChanged;

        public WaveObstacleListView()
        {
            InitializeComponent();

            tableView = new TableView
            {
                AutoGenerateColumns = false,
                AllowAddNewRow = false
            };
            tableView.SelectionChanged += TableViewOnSelectionChanged;

            tableView.AddColumn(nameof(WaveObstacle.Name), "Name");
            obsTypeColumn = tableView.AddColumn(nameof(WaveObstacle.Type), "Type");
            transCoefColumn = tableView.AddColumn(nameof(WaveObstacle.TransmissionCoefficient), "Transmission Coefficient");
            heightColumn = tableView.AddColumn(nameof(WaveObstacle.Height), "Height");
            alphaColumn = tableView.AddColumn(nameof(WaveObstacle.Alpha), "Alpha");
            betaColumn = tableView.AddColumn(nameof(WaveObstacle.Beta), "Beta");
            reflTypeColumn = tableView.AddColumn(nameof(WaveObstacle.ReflectionType), "Reflection Type");
            reflCoefColumn = tableView.AddColumn(nameof(WaveObstacle.ReflectionCoefficient), "Reflection Coefficient");

            tableView.RowDeleteHandler += RowDeleteHandler;
            tableView.ReadOnlyCellFilter += ReadOnlyCellFilter;
            tableView.DisplayCellFilter += DisplayCellFilter;
            tableView.Dock = DockStyle.Fill;
            Controls.Add(tableView);
        }

        public Action<IList<WaveObstacle>> RemoveObstacles { private get; set; }

        public object Data
        {
            get => data;
            set
            {
                data = value as IList<WaveObstacle>;
                tableView.Data = data;
            }
        }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public IEnumerable<IFeature> SelectedFeatures
        {
            get
            {
                if (tableView.Data != null)
                {
                    var obstacles = tableView.Data as IList<WaveObstacle>;
                    if (obstacles != null)
                    {
                        return
                            tableView.SelectedRowsIndices.Select(
                                i => obstacles[tableView.GetDataSourceIndexByRowIndex(i)]);
                    }
                }

                return Enumerable.Empty<IFeature>();
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                List<WaveObstacle> selectedInMap = value.OfType<WaveObstacle>().ToList();
                var allConditions = tableView.Data as IList<WaveObstacle>;
                if (allConditions == null)
                {
                    return;
                }

                IEnumerable<WaveObstacle> selectedBoundaries = allConditions.Intersect(selectedInMap);
                int[] selectedIndices = selectedBoundaries.Select(allConditions.IndexOf).ToArray();

                tableView.ClearSelection();
                tableView.SelectRows(selectedIndices);
                tableView.FocusedRowIndex = tableView.SelectedRowsIndices.FirstOrDefault();
            }
        }

        public ILayer Layer { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public void OnActivated()
        {
            // Nothing to be done, enforced through ILayerEditorView
        }

        public void OnDeactivated()
        {
            // Nothing to be done, enforced through ILayerEditorView
        }

        private void TableViewOnSelectionChanged(object sender, TableSelectionChangedEventArgs e)
        {
            if (SelectedFeaturesChanged != null)
            {
                SelectedFeaturesChanged(this, e);
            }
        }

        private bool RowDeleteHandler()
        {
            if (SelectedFeatures.Any())
            {
                if (RemoveObstacles != null)
                {
                    RemoveObstacles(SelectedFeatures.OfType<WaveObstacle>().ToList());
                }

                return true;
            }

            return false;
        }

        private bool DisplayCellFilter(TableViewCellStyle cellStyle)
        {
            if (ReadOnlyCellFilter(cellStyle))
            {
                cellStyle.BackColor = tableView.ReadOnlyCellBackColor;
                cellStyle.ForeColor = tableView.ReadOnlyCellBackColor;
                return true;
            }

            return false;
        }

        private bool ReadOnlyCellFilter(TableViewCell cell)
        {
            if (data == null)
            {
                return false;
            }

            if (cell.RowIndex > tableView.RowCount - 1)
            {
                return false;
            }

            var type = (ObstacleType) tableView.GetCellValue(cell.RowIndex, obsTypeColumn.AbsoluteIndex);
            var reflType = (ReflectionType) tableView.GetCellValue(cell.RowIndex, reflTypeColumn.AbsoluteIndex);

            if (type == ObstacleType.Dam && cell.Column.AbsoluteIndex == transCoefColumn.AbsoluteIndex)
            {
                return true;
            }

            if (type == ObstacleType.Sheet && cell.Column.AbsoluteIndex == heightColumn.AbsoluteIndex)
            {
                return true;
            }

            if (type == ObstacleType.Sheet && cell.Column.AbsoluteIndex == alphaColumn.AbsoluteIndex)
            {
                return true;
            }

            if (type == ObstacleType.Sheet && cell.Column.AbsoluteIndex == betaColumn.AbsoluteIndex)
            {
                return true;
            }

            if (reflType == ReflectionType.No && cell.Column.AbsoluteIndex == reflCoefColumn.AbsoluteIndex)
            {
                return true;
            }

            return false;
        }
    }
}