using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Shell.Gui;
using DelftTools.Utils;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;
using GeoAPI.Extensions.Feature;
using NetTopologySuite.Extensions.Features;
using SharpMap.Api.Layers;

namespace DeltaShell.Plugins.FMSuite.Wave.Gui.Editors
{
    public partial class WaveBoundaryConditionListView : UserControl, ILayerEditorView
    {
        private readonly WaveOverallSpectrumFileSelection specFileSelectionControl;

        private readonly TableView tableView;
        private readonly ITableViewColumn spectralDefColumn;
        private readonly ITableViewColumn spectrumShapeColumn;

        private WaveModel model;

        public event EventHandler SelectedFeaturesChanged;

        public WaveBoundaryConditionListView()
        {
            InitializeComponent();

            specFileSelectionControl = new WaveOverallSpectrumFileSelection();
            specFileSelectionControl.Dock = DockStyle.Fill;

            tableView = new TableView {AutoGenerateColumns = false};

            tableView.AddColumn("Name", "Name");
            tableView.AddColumn("SpatialDefinitionType", "Spatial Definition");
            spectralDefColumn = tableView.AddColumn("DataType", "Spectral Specification");
            spectralDefColumn.ReadOnly = true;
            spectrumShapeColumn = tableView.AddColumn("ShapeType", "Spectral Shape");

            tableView.Dock = DockStyle.Fill;
            tableView.AllowAddNewRow = false;

            tableView.DisplayCellFilter = DisplayCellFilter;
            tableView.ReadOnlyCellFilter = ReadOnlyCellFilter;
            tableView.InputValidator = ValidateInput;
            tableView.CellChanged += TableViewOnCellChanged;
            tableView.DoubleClick += TableViewOnDoubleClick;
            tableView.RowDeleteHandler += RowDeleteHandler;
            tableView.SelectionChanged += TableViewOnSelectionChanged;

            rbBoundarySegments.Checked = true;
            rbSp2File.Checked = false;
            rbSp2File.CheckedChanged += RbBoundarySegmentsOnCheckedChanged;

            boundariesPanel.Controls.Add(tableView);
        }

        public Action<WaveBoundaryCondition, Type> OpenEditorView { private get; set; }

        public object Data
        {
            get => model;
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChange) model).PropertyChanged -= OnModelPropertyChanged;
                }

                model = value as WaveModel;

                if (model != null)
                {
                    ((INotifyPropertyChange) model).PropertyChanged += OnModelPropertyChanged;
                }

                tableView.Data = model != null
                                     ? model.BoundaryConditions
                                     : null;
                specFileSelectionControl.Data = model;

                if (model != null)
                {
                    UpdateView();
                }
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
                    var conditions = tableView.Data as IEventedList<WaveBoundaryCondition>;
                    if (conditions != null)
                    {
                        return
                            tableView.SelectedRowsIndices.Select(
                                         i => conditions[tableView.GetDataSourceIndexByRowIndex(i)].Feature)
                                     .Distinct();
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

                List<WaveBoundaryCondition> selectedInMap = value.OfType<WaveBoundaryCondition>().ToList();
                var allConditions = tableView.Data as IEventedList<WaveBoundaryCondition>;
                if (allConditions == null)
                {
                    return;
                }

                IEnumerable<WaveBoundaryCondition> selectedBoundaries = allConditions.Intersect(selectedInMap);
                int[] selectedIndices = selectedBoundaries.Select(allConditions.IndexOf).ToArray();

                tableView.ClearSelection();
                tableView.SelectRows(selectedIndices);
                tableView.FocusedRowIndex = tableView.SelectedRowsIndices.FirstOrDefault();
            }
        }

        public ILayer Layer { get; set; }
        public void EnsureVisible(object item) {}

        public void OnActivated() {}

        public void OnDeactivated() {}

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
                List<Feature2D> selectedConditions =
                    SelectedFeatures.OfType<WaveBoundaryCondition>().Select(c => c.Feature).ToList();
                model.Boundaries.RemoveAllWhere(selectedConditions.Contains);
                return true;
            }

            return false;
        }

        private void RbBoundarySegmentsOnCheckedChanged(object sender, EventArgs eventArgs)
        {
            if (model == null)
            {
                return;
            }

            if (rbSp2File.Checked && model.BoundaryConditions.Any())
            {
                // to be replaced when we have undo/redo...
                DialogResult dialogResult = MessageBox.Show(
                    "This will delete all currently defined boundary conditions in this model. Continue?",
                    "Change Boundary Definition",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dialogResult != DialogResult.Yes)
                {
                    // rewind
                    rbBoundarySegments.Checked = true;
                    return;
                }
            }

            if (rbSp2File.Checked)
            {
                DialogResult dialogResult = specFileSelectionControl.SelectSp2File();
                if (dialogResult != DialogResult.OK)
                {
                    // rewind
                    rbBoundarySegments.Checked = true;
                    return;
                }
            }

            model.BoundaryIsDefinedBySpecFile = rbSp2File.Checked;

            var modelGroupLayer = Layer as IGroupLayer;
            if (modelGroupLayer != null)
            {
                ILayer boundaryLayer =
                    modelGroupLayer.Layers.FirstOrDefault(
                        l => l.Name == WaveModelMapLayerProvider.BoundaryLayerName);
                if (boundaryLayer != null)
                {
                    boundaryLayer.Selectable = !model.BoundaryIsDefinedBySpecFile;
                }
            }

            UpdateBottomPanel();
        }

        private void UpdateBottomPanel()
        {
            boundariesPanel.Controls.Clear();
            if (rbSp2File.Checked)
            {
                boundariesPanel.Controls.Add(specFileSelectionControl);
            }
            else
            {
                boundariesPanel.Controls.Add(tableView);
            }
        }

        private void TableViewOnDoubleClick(object sender, EventArgs eventArgs)
        {
            List<int> indices = tableView.SelectedRowsIndices.ToList();
            if (indices.Count != 1)
            {
                return;
            }

            var conditions = tableView.Data as IEventedList<WaveBoundaryCondition>;
            if (conditions == null)
            {
                return;
            }

            WaveBoundaryCondition bc = conditions[tableView.GetDataSourceIndexByRowIndex(indices[0])];

            if (OpenEditorView != null && bc != null)
            {
                OpenEditorView(bc, null);
            }
        }

        private bool ReadOnlyCellFilter(TableViewCell cell)
        {
            if (!cell.Column.Equals(spectrumShapeColumn))
            {
                return false;
            }

            if (cell.RowIndex > tableView.RowCount - 1)
            {
                return false;
            }

            var definitionType =
                (BoundaryConditionDataType) tableView.GetCellValue(cell.RowIndex, spectralDefColumn.AbsoluteIndex);
            return definitionType == BoundaryConditionDataType.SpectrumFromFile;
        }

        private bool DisplayCellFilter(TableViewCellStyle tableViewCellStyle)
        {
            if (ReadOnlyCellFilter(tableViewCellStyle))
            {
                var definitionType =
                    (BoundaryConditionDataType)
                    tableView.GetCellValue(tableViewCellStyle.RowIndex, spectralDefColumn.AbsoluteIndex);
                if (definitionType == BoundaryConditionDataType.SpectrumFromFile)
                {
                    tableViewCellStyle.BackColor = tableView.ReadOnlyCellBackColor;
                    tableViewCellStyle.ForeColor = tableView.ReadOnlyCellBackColor;
                    return true;
                }
            }

            return false;
        }

        private void TableViewOnCellChanged(object sender, EventArgs<TableViewCell> e) {}

        private DelftTools.Utils.Tuple<string, bool> ValidateInput(TableViewCell tableViewCell, object o)
        {
            return new DelftTools.Utils.Tuple<string, bool>("", true);
        }

        private void UpdateView()
        {
            tableView.BestFitColumns();
            SetBoundaryTypeSelection();
            UpdateBottomPanel();
        }

        private void SetBoundaryTypeSelection()
        {
            if (rbSp2File.Checked != model.BoundaryIsDefinedBySpecFile)
            {
                rbSp2File.CheckedChanged -= RbBoundarySegmentsOnCheckedChanged;
                rbSp2File.Checked = model.BoundaryIsDefinedBySpecFile;
                rbSp2File.CheckedChanged += RbBoundarySegmentsOnCheckedChanged;
            }
        }

        private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(WaveModel.IsEditing))
            {
                // to prevent cross thread access..
                if (model.IsEditing)
                {
                    BeforeModelEdit();
                }
                else
                {
                    AfterModelEdit();
                }
            }
        }

        [InvokeRequired]
        private void BeforeModelEdit()
        {
            tableView.Data = null;
            specFileSelectionControl.Data = null;
        }

        [InvokeRequired]
        private void AfterModelEdit()
        {
            tableView.Data = model.BoundaryConditions;
            specFileSelectionControl.Data = model;
            UpdateView();
        }
    }
}