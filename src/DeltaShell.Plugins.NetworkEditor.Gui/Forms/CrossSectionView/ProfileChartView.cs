using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf.Charting;
using DelftTools.Controls.Swf.Charting.Series;
using DelftTools.Controls.Swf.Charting.Tools;
using DelftTools.Hydro.CrossSections;
using DelftTools.Hydro.Helpers;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.ChartEditors;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.ProfileMutators;
using DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView.Sections;
using DeltaShell.Plugins.NetworkEditor.Gui.Properties;
using GeoAPI.Geometries;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public partial class ProfileChartView : UserControl
    {
        private readonly List<IChartViewTool> mutualExclusiveTools = new List<IChartViewTool>();
        private ICrossSectionDefinition crossSectionDefinition;
        private BindingList<CrossSectionSection> crossSectionSections;
        private CrossSectionDefinitionViewSectionRenderer sectionRenderer;
        private CrossSectionDefinitionViewHistoryController historyController;
        private ToolTip toolTip;

        private Cursor cursor;
        private CrossSectionDefinitionViewModel viewModel;

        public event EventHandler SectionSelectionChanged;

        public event EventHandler StatusMessage;

        public ProfileChartView()
        {
            InitializeComponent();
            Initialize();
        }

        public ChartView ChartView
        {
            get
            {
                return chartView;
            }
        }

        public bool HistoryToolEnabled
        {
            get
            {
                return historyTool.Active;
            }
            set
            {
                historyTool.Active = value;

                if (!historyTool.Active)
                {
                    historyController.ClearHistory();
                    RefreshChart();
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (viewModel != null && viewModel.FixOnScreenRatio)
            {
                UpdateChartAxis();
            }
        }

        private Cursor GetAddPointCursor()
        {
            return cursor ?? (cursor = new Cursor(new MemoryStream(Resources.AddPointCursor)));
        }

        private void Initialize()
        {
            ChartView.Title = "Profile";
            ChartView.Chart.Legend.Alignment = LegendAlignment.Bottom;
            ChartView.Chart.Legend.Visible = true;
            ChartView.MouseUp += (s, e) => ReActivateTools();

            historyTool = ChartView.NewHistoryTool();
            historyTool.Active = false;
            historyController = new CrossSectionDefinitionViewHistoryController(historyTool);
        }

        private void ReActivateTools()
        {
            foreach (IChartViewTool tool in mutualExclusiveTools)
            {
                tool.Active = true;
            }
        }

        private void DeActivateOtherTools(IChartViewTool sender)
        {
            foreach (IChartViewTool tool in mutualExclusiveTools)
            {
                if (tool != sender)
                {
                    tool.Active = false;
                }
            }
        }

        private ChartRectangle GetChartExtends()
        {
            var minX = double.MaxValue;
            var maxX = double.MinValue;
            var minY = double.MaxValue;
            var maxY = double.MinValue;

            var defined = false;

            //yz-values, storage and historyTool series
            foreach (IChartSeries series in ChartView.Chart.Series)
            {
                var lst = series.DataSource as IEnumerable<ICoordinate>; /* TODO: ICoordinate should be replaced for Coordinate, check issue SOBEK3-666 */
                if (lst != null && lst.Any())
                {
                    minX = Math.Min(lst.Select(c => c.X).Min(), minX);
                    maxX = Math.Max(lst.Select(c => c.X).Max(), maxX);
                    minY = Math.Min(lst.Select(c => c.Y).Min(), minY);
                    maxY = Math.Max(lst.Select(c => c.Y).Max(), maxY);
                    defined = true;
                }
            }

            if (defined)
            {
                if (viewModel.FixOnScreenRatio)
                {
                    FixOnScreenRatio(ref minX, ref maxX, ref minY, ref maxY);
                }

                return new ChartRectangle
                {
                    Left = minX,
                    Right = maxX,
                    Bottom = minY,
                    Top = maxY
                };
            }

            return new ChartRectangle(0, 0, 0, 0);
        }

        private void FixOnScreenRatio(ref double minX, ref double maxX, ref double minY, ref double maxY)
        {
            double width = maxX - minX;
            double height = maxY - minY;
            double radiusX = width / 2.0;
            double radiusY = height / 2.0;
            double centerX = minX + radiusX;
            double centerY = minY + radiusY;

            double dataRatio = width / height;
            int chartHeight = ChartView.Height - 40; //magic number: about the amount of pixels required to draw the legend
            double chartRatio = (double) ChartView.Width / chartHeight;
            double combinedRatio = chartRatio / dataRatio;

            if (dataRatio > chartRatio)
            {
                radiusY /= combinedRatio;
            }
            else
            {
                radiusX *= combinedRatio;
            }

            minX = centerX - radiusX;
            maxX = centerX + radiusX;
            minY = centerY - radiusY;
            maxY = centerY + radiusY;
        }

        #region Tools

        private IAddPointTool addPointTool;
        private IEditPointTool editFlowProfileTool;
        private IEditPointTool editProfileTool;
        private ShapeModifyTool sectionRectanglesTool;
        private ISelectPointTool selectPointTool;
        private ISeriesBandTool storageAreaTool;
        private ICursorLineTool thalWegMarker;
        private IHistoryTool historyTool;

        #endregion

        #region Initialize on Data

        public void SetData(ICrossSectionDefinition cs, BindingList<CrossSectionSection> css, CrossSectionDefinitionViewModel crossSectionDefinitionViewModel)
        {
            if (crossSectionDefinition != null)
            {
                Cleanup();
            }

            crossSectionDefinition = cs;
            crossSectionSections = css;
            viewModel = crossSectionDefinitionViewModel;
            if (crossSectionDefinition != null)
            {
                Setup();
            }
        }

        /// <summary>
        /// Refreshes the Chart title with the current CrossSectionDefinition
        /// </summary>
        public void RefreshChartTitle()
        {
            string name = crossSectionDefinition == null ? "" : crossSectionDefinition.Name;
            chartView.Title = string.Format("Profile {0}",
                                            string.IsNullOrEmpty(name)
                                                ? ""
                                                : string.Format("'{0}'", name));
        }

        private void Cleanup()
        {
            ChartView.Chart.Series.Clear();

            List<IChartViewTool> toolsToRemove = mutualExclusiveTools.Concat(new[]
            {
                sectionRectanglesTool
            }).ToList();

            foreach (IChartViewTool tool in toolsToRemove)
            {
                var disposableTool = tool as IDisposable;
                if (disposableTool != null)
                {
                    disposableTool.Dispose();
                }

                ChartView.Tools.Remove(tool);
            }

            mutualExclusiveTools.Clear();
            sectionRenderer = null;

            historyController.AddCrossSectionToHistory(crossSectionDefinition);
        }

        private void Setup()
        {
            RefreshChartTitle();

            chartView.Chart.LeftAxis.Title = viewModel.YUnit;
            chartView.Chart.BottomAxis.Title = viewModel.XUnit;

            historyController.RemoveCrossSectionFromHistory(crossSectionDefinition);

            ICrossSectionProfileMutator crossSectionProfileMutator = crossSectionDefinition.GetProfileMutator();

            editFlowProfileTool = AddProfileTools(crossSectionDefinition.GetFlowProfileMutator());
            editProfileTool = AddProfileTools(crossSectionProfileMutator);

            CreateThalWegMarker();
            CreateAddPointTool(crossSectionProfileMutator);
            CreateSelectPointTool();
            CreateSectionRectanglesTool();
            CreateSectionRenderer();

            toolTip = new ToolTip();

            mutualExclusiveTools.AddRange(new IChartViewTool[]
            {
                addPointTool,
                selectPointTool,
                editProfileTool,
                editFlowProfileTool,
                thalWegMarker
            });

            RefreshChart();
        }

        private void CreateSectionRenderer()
        {
            sectionRenderer = new CrossSectionDefinitionViewSectionRenderer(ChartView.Chart, sectionRectanglesTool,
                                                                            crossSectionSections, viewModel.IsSymmetrical);
        }

        private void CreateSelectPointTool()
        {
            selectPointTool = ChartView.NewSelectPointTool();
            selectPointTool.SelectedPointerColor = Color.DarkBlue;
            selectPointTool.HandleDelete = false; //delete is taken care of by editpoint tool
            selectPointTool.Cursor = Cursors.Default;
        }

        private void CreateSectionRectanglesTool()
        {
            sectionRectanglesTool = new ShapeModifyTool(ChartView.Chart)
            {
                ShapeEditMode = ShapeEditMode.ShapeSelect |
                                ShapeEditMode.ShapeMove |
                                ShapeEditMode.ShapeResize
            };
            sectionRectanglesTool.ShapeEditMode = ShapeEditMode.ShapeSelect;
            sectionRectanglesTool.SelectionChanged += (s, e) =>
            {
                if (SectionSelectionChanged != null)
                {
                    SectionSelectionChanged(
                        sectionRenderer.GetSelectedSection(e.ShapeFeature),
                        EventArgs.Empty);
                }
            };

            ChartView.Tools.Add(sectionRectanglesTool);
        }

        private void CreateThalWegMarker()
        {
            bool enabled = !crossSectionDefinition.GeometryBased;

            Color color = enabled ? Color.DarkMagenta : Color.DarkGray;

            thalWegMarker = ChartView.NewCursorLineTool(CursorLineToolStyles.Vertical, color, 3, DashStyle.Dot);
            thalWegMarker.Enabled = enabled;
            thalWegMarker.Drop += (s, e) => crossSectionDefinition.Thalweg = thalWegMarker.XValue;
            thalWegMarker.MouseDown += (s, e) => DeActivateOtherTools(thalWegMarker);
            thalWegMarker.MouseUp += (s, e) => ReActivateTools();
        }

        private void CreateAddPointTool(ICrossSectionProfileMutator crossSectionProfileMutator)
        {
            addPointTool = ChartView.NewAddPointTool();
            addPointTool.PointAdded += (s, e) =>
            {
                crossSectionProfileMutator.AddPoint(e.X, e.Y);
                RefreshChart();
            };
            addPointTool.Button = MouseButtons.Left;
            addPointTool.Cursor = GetAddPointCursor();
            addPointTool.Enabled = crossSectionProfileMutator.CanAdd;
            addPointTool.Insert = false;
            addPointTool.AddOnlyIfOnLine = true;
        }

        private IEditPointTool AddProfileTools(ICrossSectionProfileMutator profileMutator)
        {
            IEditPointTool editTool = ChartView.NewEditPointTool();
            editTool.IsPolygon = false;
            editTool.Enabled = profileMutator.CanMove;
            editTool.MouseHoverPoint += (s, e) => ShowALTMessageIfProfilesOverlap(e.Index);
            editTool.BeforeDrag += EditToolBeforeDrag;
            editTool.AfterPointEdit += (s, e) => profileMutator.MovePoint(e.Index, e.X, e.Y);
            editTool.ClipXValues = profileMutator.ClipHorizontal;
            editTool.ClipYValues = profileMutator.ClipVertical;

            if (profileMutator.FixHorizontal)
            {
                editTool.DragStyles = DragStyle.Y;
            }
            else if (profileMutator.FixVertical)
            {
                editTool.DragStyles = DragStyle.X;
            }

            return editTool;
        }

        private void EditToolBeforeDrag(object sender, PointEventArgs e)
        {
            if (ModifierKeys == Keys.Alt && sender == editFlowProfileTool)
            {
                e.Cancel = true; //alt was pressed, so cancel flow profile tool: let normal profile handle this one
                return;
            }

            DeActivateOtherTools(sender as IChartViewTool);
        }

        private bool ShowingToolTip;

        private void ShowALTMessageIfProfilesOverlap(int seriesIndex)
        {
            var statusText = "";

            if (DoSeriesOverlap(seriesIndex))
            {
                statusText = "Hold ALT to move the Total profile.";
                if (!ShowingToolTip)
                {
                    ShowingToolTip = true;
                    toolTip.Show(statusText, this, PointToClient(Cursor.Position).X + 10, PointToClient(Cursor.Position).Y + 20);
                }
            }
            else
            {
                if (ShowingToolTip)
                {
                    ShowingToolTip = false;
                    toolTip.Hide(this);
                }
            }

            if (StatusMessage != null)
            {
                StatusMessage(statusText, EventArgs.Empty);
            }
        }

        private bool DoSeriesOverlap(int seriesIndex)
        {
            if (seriesIndex == -1)
            {
                return false;
            }

            ILineChartSeries profileSeries = editProfileTool.Series;
            ILineChartSeries flowProfileSeries = editFlowProfileTool.Series;

            double x1 = profileSeries.CalcXPos(seriesIndex);
            double x2 = flowProfileSeries.CalcXPos(seriesIndex);

            double y1 = profileSeries.CalcYPos(seriesIndex);
            double y2 = flowProfileSeries.CalcYPos(seriesIndex);

            return Math.Abs(x1 - x2) < double.Epsilon && Math.Abs(y1 - y2) < double.Epsilon;
        }

        #endregion

        #region Refresh

        private void AddSectionsToChart()
        {
            sectionRectanglesTool.Clear();
            sectionRenderer.SetSeparatorTop(crossSectionDefinition.HighestPoint);
            sectionRenderer.DrawSections();
        }

        private void UpdateChartAxis()
        {
            ChartRectangle chartRectangle = GetChartExtends();

            double horizontalMargin = 0.02 * (chartRectangle.Right - chartRectangle.Left);
            double verticalMargin = 0.04 * (chartRectangle.Top - chartRectangle.Bottom);

            ChartView.Chart.LeftAxis.Automatic = false;
            ChartView.Chart.LeftAxis.Maximum = chartRectangle.Top + verticalMargin;
            ChartView.Chart.LeftAxis.Minimum = chartRectangle.Bottom - (3.0 * verticalMargin);

            ChartView.Chart.BottomAxis.Automatic = false;
            ChartView.Chart.BottomAxis.Maximum = chartRectangle.Right + horizontalMargin;
            ChartView.Chart.BottomAxis.Minimum = chartRectangle.Left - horizontalMargin;
        }

        public void RefreshChart()
        {
            if (crossSectionDefinition == null)
            {
                return;
            }

            selectPointTool.ClearSelection(); //clear selection, selected points belong to old series anyway
            ChartView.Chart.Series.Clear();

            List<Coordinate> profile = crossSectionDefinition.Profile.ToList();
            List<Coordinate> flowProfile = crossSectionDefinition.FlowProfile.ToList();

            ILineChartSeries profileSeries = CreateLineSeries("Total profile", profile, Color.DarkBlue, DashStyle.Solid);
            ILineChartSeries flowProfileSeries = CreateLineSeries("Flow profile", flowProfile, Color.SteelBlue, DashStyle.Dash);

            editProfileTool.Series = profileSeries;
            editFlowProfileTool.Series = flowProfileSeries;
            addPointTool.Series = profileSeries;

            ChartView.Chart.Series.Add(profileSeries);
            ChartView.Chart.Series.Add(flowProfileSeries);

            AddStorageToLegend(profile, flowProfile);

            if (storageAreaTool != null)
            {
                ChartView.Tools.Remove(storageAreaTool);
            }

            storageAreaTool = ChartView.NewSeriesBandTool(profileSeries, flowProfileSeries, Color.LightBlue,
                                                          HatchStyle.BackwardDiagonal, Color.WhiteSmoke);
            ChartView.Tools.Add(storageAreaTool);

            thalWegMarker.XValue = crossSectionDefinition.Thalweg;

            historyController.RefreshHistoryInChart(crossSectionDefinition.Thalweg);

            AddSectionsToChart();

            UpdateChartAxis();
        }

        private void AddStorageToLegend(IEnumerable<Coordinate> profile, IEnumerable<Coordinate> flowProfile)
        {
            double area = CrossSectionHelper.CalculateStorageArea(profile, flowProfile);

            IAreaChartSeries legendPlaceholderStorageSeries = ChartSeriesFactory.CreateAreaSeries();
            legendPlaceholderStorageSeries.Title = string.Format("Storage Area ({0:0.##} m²) ", area);
            legendPlaceholderStorageSeries.UseHatch = true;
            legendPlaceholderStorageSeries.Color = Color.LightBlue;
            legendPlaceholderStorageSeries.HatchStyle = HatchStyle.BackwardDiagonal;
            legendPlaceholderStorageSeries.HatchColor = Color.WhiteSmoke;

            ChartView.Chart.Series.Add(legendPlaceholderStorageSeries);
        }

        private static ILineChartSeries CreateLineSeries(string name, IList<Coordinate> profile, Color lineColor,
                                                         DashStyle lineStyle)
        {
            var profileSeries = (LineChartSeries) ChartSeriesFactory.CreateLineSeries();
            profileSeries.Title = name;
            profileSeries.XValuesDataMember = "X";
            profileSeries.YValuesDataMember = "Y";
            profileSeries.DataSource = profile.OfType<ICoordinate>().ToList(); /* TODO: ICoordinate should be replaced for Coordinate, check issue SOBEK3-666 */
            profileSeries.PointerColor = lineColor;
            profileSeries.PointerSize = 5;
            profileSeries.Color = lineColor;
            profileSeries.DashStyle = lineStyle;
            profileSeries.Width = 2;
            return profileSeries;
        }

        #endregion

        #region Selection

        public void SelectIndex(int index)
        {
            selectPointTool.ClearSelection();

            if (index != -1)
            {
                selectPointTool.AddPointAtIndexToSelection(editProfileTool.Series, index);
                selectPointTool.AddPointAtIndexToSelection(editFlowProfileTool.Series, index);
            }

            selectPointTool.Invalidate();
        }

        public void SelectSection(CrossSectionSection section)
        {
            if (section == null || sectionRenderer == null)
            {
                return;
            }

            sectionRenderer.SelectSection(section);
        }

        #endregion
    }
}