using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Table;
using DelftTools.Controls.Swf.Table.Validation;
using DelftTools.Utils.Aop;
using DelftTools.Utils.Collections;
using DelftTools.Utils.Collections.Generic;
using DeltaShell.Plugins.FMSuite.Common.DepthLayers;
using DeltaShell.Plugins.FMSuite.Common.FeatureData;

namespace DeltaShell.Plugins.FMSuite.Common.Gui.Editors
{
    public partial class VerticalProfileControl : UserControl
    {
        [Entity]
        private class ProfileDepth
        {
            public ProfileDepth()
            {
                Offset = 0;
            }

            public ProfileDepth(double d)
            {
                Offset = d;
            }

            public double Offset { get; set; }
        }

        private VerticalProfileDefinition verticalProfileDefinition;
        private readonly IEventedList<ProfileDepth> profilePointDepths;
        private DepthLayerDefinition modelDepthLayerDefinition;
        private IList<VerticalProfileType> supportedVerticalProfileTypes;

        private bool updatingProfile;

        public VerticalProfileControl()
        {
            InitializeComponent();
            supportedVerticalProfileTypes =
                Enum.GetValues(typeof (VerticalProfileType)).Cast<VerticalProfileType>().ToList();
            profileTypeComboBox.Items.AddRange(supportedVerticalProfileTypes.OfType<object>().ToArray());
            verticalProfileDefinition = new VerticalProfileDefinition();
            profilePointDepths = new EventedList<ProfileDepth>();
            
            ((INotifyPropertyChanged)profilePointDepths).PropertyChanged += PointDepthValuesChanged;
            profilePointDepths.CollectionChanged += ProfilePointsCollectionChanged;

            profileTypeComboBox.SelectedIndexChanged += ProfileTypeComboBoxOnSelectedIndexChanged;
            picture.Paint += PicturePaint;

            ((TableView)TableView).ExceptionMode = DelftTools.Controls.Swf.Table.TableView.ValidationExceptionMode.NoAction;
            TableView.SelectionChanged += TableViewSelectionChanged;
            TableView.RowValidator = RowValidator;

            TableView.Data = new BindingList<ProfileDepth>(profilePointDepths) { AllowNew = false, AllowRemove = false };
        }

        private IRowValidationResult RowValidator(int i, object[] objects)
        {
            var cellValue = objects[0];
            if (!(cellValue is double))
            {
                return new RowValidationResult("");
            }
            var depth = (double) objects[0];
            switch (SelectedVerticalProfileType)
            {
                case VerticalProfileType.PercentageFromBed:
                    if (depth < 0)
                    {
                        return new RowValidationResult(0, "Point depth is below bed level");
                    }
                    if (depth > 100)
                    {
                        return new RowValidationResult(0, "Point depth is above surface level");
                    }
                    break;
                case VerticalProfileType.PercentageFromSurface:
                    if (depth < 0)
                    {
                        return new RowValidationResult(0, "Point depth is above surface level");
                    }
                    if (depth > 100)
                    {
                        return new RowValidationResult(0, "Point depth is below bed level");
                    }
                    break;
                case VerticalProfileType.ZFromBed:
                    if (depth < 0)
                    {
                        return new RowValidationResult(0, "Point depth is below bed level");
                    }
                    if (depth > 10000)
                    {
                        return new RowValidationResult(0, "Point depth outside bounds");
                    }
                    break;
                case VerticalProfileType.ZFromSurface:
                    if (depth < 0)
                    {
                        return new RowValidationResult(0, "Point depth is above surface level");
                    }
                    if (depth > 10000)
                    {
                        return new RowValidationResult(0, "Point depth outside bounds");
                    }
                    break;
                default:
                    if (depth < -10000 || depth > 10000)
                    {
                        return new RowValidationResult(0, "Point depth outside bounds");
                    }
                    break;
            }
            return new RowValidationResult("");
        }

        public void SetSupportedProfileTypes(IEnumerable<VerticalProfileType> verticalProfileTypes)
        {
            profileTypeComboBox.Items.Clear();
            profileTypeComboBox.Items.AddRange(verticalProfileTypes.Distinct().OfType<object>().ToArray());
            var index = verticalProfileDefinition == null
                ? -1
                : verticalProfileTypes.ToList().IndexOf(verticalProfileDefinition.Type);
            profileTypeComboBox.SelectedIndex = index;
        }

        void TableViewSelectionChanged(object sender, TableSelectionChangedEventArgs e)
        {
            picture.Invalidate();
        }

        void PointDepthValuesChanged(object sender, PropertyChangedEventArgs e)
        {
            if (updatingProfile) return;
            picture.Invalidate();
            var index = profilePointDepths.IndexOf(sender as ProfileDepth);
            if (index != -1 && index < verticalProfileDefinition.ProfilePoints)
            {
                updatingProfile = true;
                verticalProfileDefinition.PointDepths[index] = ((ProfileDepth) sender).Offset;
                updatingProfile = false;
            }
        }
        
        public DepthLayerDefinition ModelDepthLayerDefinition
        {
            get { return modelDepthLayerDefinition; }
            set
            {
                modelDepthLayerDefinition = value;
                picture.Invalidate();
            }
        }

        private IEnumerable<double> Depths
        {
            get { return profilePointDepths.Select(p => p.Offset); }
        }

        private bool CanDrawModelLayers(DepthLayerType depthLayerType, VerticalProfileType verticalProfileType)
        {
            if (depthLayerType == DepthLayerType.Single)
            {
                return false;
            }
            if (depthLayerType == DepthLayerType.Sigma)
            {
                return verticalProfileType == VerticalProfileType.PercentageFromBed ||
                       verticalProfileType == VerticalProfileType.PercentageFromSurface ||
                       verticalProfileType == VerticalProfileType.Uniform ||
                       verticalProfileType == VerticalProfileType.TopBottom;
            }
            return true;
        }

        public bool AllowAddRemoveProfilePoints
        {
            get
            {
                return SelectedVerticalProfileType != VerticalProfileType.Uniform &&
                       SelectedVerticalProfileType != VerticalProfileType.TopBottom;
            }
        }

        void PicturePaint(object sender, PaintEventArgs e)
        {
            if (ModelDepthLayerDefinition == null) return;

            if (!tableView.Validate()) return;

            var graphics = e.Graphics;
            var picture = (PictureBox)sender;
            graphics.Clear(picture.BackColor);
            float offset = 0;
            var modelThickness = ModelDepthLayerDefinition.LayerThicknesses.Sum();
            var selectedVerticalProfileType = SelectedVerticalProfileType;

            if (CanDrawModelLayers(ModelDepthLayerDefinition.Type, selectedVerticalProfileType))
            {
                var i = 0;
                foreach (var layerThickness in ModelDepthLayerDefinition.LayerThicknesses.Reverse())
                {
                    var height = (float) (picture.Height*layerThickness/modelThickness);
                    var color = DepthLayerControl.BlueShade(i++, ModelDepthLayerDefinition.NumLayers);
                    graphics.FillRectangle(new SolidBrush(color), 0, offset,
                        picture.Width, height);
                    offset += height;
                    ControlPaint.Dark(color, (float) 0.001);
                }
            }
            else
            {
                graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0,
                    picture.Width, picture.Height);
            }

            if (selectedVerticalProfileType == VerticalProfileType.ZFromBed)
            {
                var profileThickness = ModelDepthLayerDefinition.Type != DepthLayerType.Z
                                           ? Depths.Max()
                                           : modelThickness;

                foreach (var profilePointDepth in profilePointDepths)
                {
                    var factor = profileThickness <= 0 ? 0 : profilePointDepth.Offset/profileThickness;
                    var height = (float)(1 - factor) * picture.Height;
                    DrawProfileLine(graphics, height, picture, profilePointDepth);
                }
            }
            if (selectedVerticalProfileType == VerticalProfileType.ZFromSurface)
            {
                var profileThickness = ModelDepthLayerDefinition.Type != DepthLayerType.Z
                                           ? Depths.Max()
                                           : modelThickness;

                foreach (var profilePointDepth in profilePointDepths)
                {
                    var factor = profileThickness <= 0 ? 0 : profilePointDepth.Offset / profileThickness;
                    var height = (float)factor * picture.Height;
                    DrawProfileLine(graphics, height, picture, profilePointDepth);
                }
            }
            if (selectedVerticalProfileType == VerticalProfileType.ZFromDatum)
            {
                var minZ = Depths.Min();
                var profileThickness = ModelDepthLayerDefinition.Type != DepthLayerType.Z
                    ? Depths.Max() - minZ
                    : modelThickness;

                foreach (var profilePointDepth in profilePointDepths)
                {
                    var factor = profileThickness <= 0 ? 0 : (profilePointDepth.Offset - minZ)/profileThickness;
                    var height = (float) (1 - factor)*picture.Height;
                    DrawProfileLine(graphics, height, picture, profilePointDepth);
                }
            }
            if (selectedVerticalProfileType == VerticalProfileType.PercentageFromBed)
            {
                foreach (var profilePointDepth in profilePointDepths)
                {
                    var height = (float) ((1 - 0.01*profilePointDepth.Offset)*picture.Height);
                    DrawProfileLine(graphics, height, picture, profilePointDepth);
                }
            }
            if (selectedVerticalProfileType == VerticalProfileType.PercentageFromSurface)
            {
                foreach (var profilePointDepth in profilePointDepths)
                {
                    var height = (float) (0.01*profilePointDepth.Offset*picture.Height);
                    DrawProfileLine(graphics, height, picture, profilePointDepth);
                }
            }
        }

        private IEnumerable<ProfileDepth> SortedProfilePoints
        {
            get
            {
                return SelectedVerticalProfileType == VerticalProfileType.PercentageFromSurface ||
                       SelectedVerticalProfileType == VerticalProfileType.ZFromSurface
                    ? profilePointDepths.OrderByDescending(p => p.Offset)
                    : profilePointDepths.OrderBy(p => p.Offset);
            }
        }

        private void DrawProfileLine(Graphics graphics, float height, PictureBox picture, ProfileDepth profilePointDepth)
        {
            var selectedProfilePointDepths =
                TableView.SelectedRowsIndices.Select(i => profilePointDepths[TableView.GetDataSourceIndexByRowIndex(i)]);

            var color = selectedProfilePointDepths.Contains(profilePointDepth) ? Color.GreenYellow : Color.Red;

            graphics.DrawLine(new Pen(color, (float) 1.5), 0, height, picture.Width, height);

            var index = SortedProfilePoints.ToList().IndexOf(profilePointDepth) + 1;

            graphics.DrawString(index.ToString(), new Font(FontFamily.GenericSansSerif, 14),
                new SolidBrush(color), (float) 0.35*picture.Width, height - 20);
        }

        public Action<VerticalProfileDefinition> AfterProfileDefinitionCreated { private get; set; }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VerticalProfileDefinition VerticalProfileDefinition
        {
            get { return verticalProfileDefinition; }
            set
            {
                
                verticalProfileDefinition = value;
                
                if (verticalProfileDefinition == null)
                {                  
                    profileTypeComboBox.SelectedItem = VerticalProfileType.Uniform;
                    profileTypeComboBox.Enabled = false;
                }
                else
                {
                    profileTypeComboBox.SelectedIndexChanged -= ProfileTypeComboBoxOnSelectedIndexChanged;
                    profileTypeComboBox.SelectedItem = verticalProfileDefinition.Type;
                    var bindingList = TableView.Data as BindingList<ProfileDepth>;
                    if (bindingList != null)
                    {
                        bindingList.AllowNew = AllowAddRemoveProfilePoints;
                        bindingList.AllowRemove = AllowAddRemoveProfilePoints;
                    }
                    profileTypeComboBox.SelectedIndexChanged += ProfileTypeComboBoxOnSelectedIndexChanged;
                    profileTypeComboBox.Enabled = true;

                    profilePointDepths.CollectionChanged -= ProfilePointsCollectionChanged;
                    ((INotifyPropertyChanged)profilePointDepths).PropertyChanged -= PointDepthValuesChanged;
                    profilePointDepths.Clear();
                    profilePointDepths.AddRange(verticalProfileDefinition.PointDepths.Select(d => new ProfileDepth(d)).ToList());
                    ((INotifyPropertyChanged)profilePointDepths).PropertyChanged += PointDepthValuesChanged;
                    profilePointDepths.CollectionChanged += ProfilePointsCollectionChanged;
                    
                    UpdateView();
                }
            }
        }

        private ITableView TableView { get { return tableView; } }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        private VerticalProfileType SelectedVerticalProfileType
        {
            get
            {
                return profileTypeComboBox.SelectedItem == null
                           ? VerticalProfileType.Uniform
                           : (VerticalProfileType) profileTypeComboBox.SelectedItem;
            }
        }

        private void ProfileTypeComboBoxOnSelectedIndexChanged(object sender, EventArgs eventArgs)
        {
            if (VerticalProfileDefinition == null) return;
            if (VerticalProfileDefinition.Type != SelectedVerticalProfileType)
            {
                GenerateProfile();
            }
            UpdateView();
        }

        private void GenerateProfile()
        {
            updatingProfile = true;

            switch (SelectedVerticalProfileType)
            {
                case VerticalProfileType.TopBottom:
                    VerticalProfileDefinition = VerticalProfileDefinition.Create(SelectedVerticalProfileType,
                        new[] {0.0, 1.0});
                    break;
                default:
                    VerticalProfileDefinition = VerticalProfileDefinition.Create(SelectedVerticalProfileType,
                        new[] {0.0});
                    break;
            }

            if (AfterProfileDefinitionCreated != null)
            {
                AfterProfileDefinitionCreated(VerticalProfileDefinition);
            }

            updatingProfile = false;
        }

        private void UpdateView()
        {
            var selectedVerticalProfileType = SelectedVerticalProfileType;

            tableView.Enabled =
                !(selectedVerticalProfileType == VerticalProfileType.Uniform ||
                  selectedVerticalProfileType == VerticalProfileType.TopBottom);

            if (TableView.Columns.Any())
            {
                TableView.Columns[0].Caption = ColumnCaption(selectedVerticalProfileType);
                TableView.Columns[0].SortOrder = selectedVerticalProfileType ==
                                                 VerticalProfileType.PercentageFromSurface ||
                                                 selectedVerticalProfileType == VerticalProfileType.ZFromSurface
                    ? SortOrder.Descending
                    : SortOrder.Ascending;

                TableView.RefreshData();
            }
            picture.Invalidate();
        }

        private static string ColumnCaption(VerticalProfileType profileType)
        {
            switch (profileType)
            {
                case VerticalProfileType.Uniform:
                case VerticalProfileType.TopBottom:
                    return "";
                case VerticalProfileType.ZFromBed:
                case VerticalProfileType.ZFromSurface:
                case VerticalProfileType.ZFromDatum:
                    return "z [m]";
                case VerticalProfileType.PercentageFromBed:
                case VerticalProfileType.PercentageFromSurface:
                    return "z [%]";
                default:
                    throw new NotImplementedException(string.Format("Unknown vertical profile definition type {0}",
                                                                    profileType));
            }
        }

        private void ProfilePointsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (updatingProfile || verticalProfileDefinition == null) return;

            updatingProfile = true;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    ((ProfileDepth) e.GetRemovedOrAddedItem()).Offset = profilePointDepths.Select(p => p.Offset).Max() + 1;
                    verticalProfileDefinition.PointDepths.Add(((ProfileDepth)e.GetRemovedOrAddedItem()).Offset);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    verticalProfileDefinition.PointDepths.Remove(((ProfileDepth) e.GetRemovedOrAddedItem()).Offset);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    var index=verticalProfileDefinition.PointDepths.IndexOf(((ProfileDepth) e.OldItems[0]).Offset);
                    verticalProfileDefinition.PointDepths[index] = ((ProfileDepth) e.GetRemovedOrAddedItem()).Offset;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    verticalProfileDefinition.PointDepths.Clear();
                    break;
                default:
                    updatingProfile = false;
                    throw new NotImplementedException("NotifyCollectionChanged action not recognized.");
            }

            updatingProfile = false;
        }
    }
}
