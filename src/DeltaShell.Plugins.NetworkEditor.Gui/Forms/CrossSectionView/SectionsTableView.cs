using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Controls.Swf.Editors;
using DelftTools.Hydro.CrossSections;
using DelftTools.Utils.Collections.Generic;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    public partial class SectionsTableView : UserControl, IView
    {
        private const int MinYColumn = 0;
        private const int MaxYColumn = 1;
        private const int SectionTypeColumn = 2;
        private SectionsBindingList sections;
        private IEventedList<CrossSectionSectionType> sectionTypeList = new EventedList<CrossSectionSectionType>();

        public event EventHandler SelectionChanged;

        public SectionsTableView()
        {
            InitializeComponent();
            SetupTableRoughnessView();
            Enabled = true;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IEventedList<CrossSectionSectionType> SectionTypeList
        {
            get
            {
                return sectionTypeList;
            }
            set
            {
                UnSubscribeSectionTypeListChange();
                sectionTypeList = value;
                SubscribeSectionTypeListChange();
                ResetSelectionTypeEditor();
            }
        }

        public bool Enabled
        {
            get
            {
                return tableViewSections.Enabled;
            }
            set
            {
                tableViewSections.Enabled = value;

                foreach (ITableViewColumn column in tableViewSections.Columns)
                {
                    column.ReadOnly = !tableViewSections.Enabled;
                }

                SetTableMutability();
            }
        }

        public double MaxY { get; set; }
        public double MinY { get; set; }

        public void Select(CrossSectionSection section)
        {
            tableViewSections.ClearSelection();

            if (section == null)
            {
                return;
            }

            int displayRowIndex = tableViewSections.GetRowIndexByDataSourceIndex(sections.IndexOf(section));
            tableViewSections.SelectRow(displayRowIndex);
            tableViewSections.FocusedRowIndex = displayRowIndex;
            tableViewSections.Invalidate();
        }

        private void SubscribeSectionTypeListChange()
        {
            sectionTypeList.CollectionChanged += SectionTypeListChanged;
            ((INotifyPropertyChanged) sectionTypeList).PropertyChanged += SectionTypeListChanged;
        }

        private void SectionTypeListChanged(object sender, object e)
        {
            ResetSelectionTypeEditor();
        }

        private void ResetSelectionTypeEditor()
        {
            var typeEditor = new ComboBoxTypeEditor
            {
                CustomFormatter = new CrossSectionSectionTypeFormatter(),
                Items = SectionTypeList
            };

            ITableViewColumn selectionTypeColumn = tableViewSections.Columns[SectionTypeColumn];

            selectionTypeColumn.Editor = typeEditor;
            selectionTypeColumn.CustomFormatter = new CrossSectionSectionTypeFormatter();
        }

        private void UnSubscribeSectionTypeListChange()
        {
            sectionTypeList.CollectionChanged -= SectionTypeListChanged;
            ((INotifyPropertyChanged) sectionTypeList).PropertyChanged -= SectionTypeListChanged;
        }

        private void SetupTableRoughnessView()
        {
            tableViewSections.AutoGenerateColumns = false;
            tableViewSections.AddColumn("MinY", "Start");
            tableViewSections.AddColumn("MaxY", "End");
            tableViewSections.AddColumn("SectionType", "Roughness");

            ResetSelectionTypeEditor();

            tableViewSections.InputValidator = TableViewEditorValidator;
            tableViewSections.AllowColumnSorting = false;
            tableViewSections.ShowRowNumbers = true;
            tableViewSections.ReadOnlyCellFilter = cell =>
            {
                //lefttop-most and rightbottom-most can't be edited
                return cell.Column.AbsoluteIndex == 0 && cell.RowIndex == 0 ||
                       cell.Column.AbsoluteIndex == 1 &&
                       cell.RowIndex == tableViewSections.RowCount - 1;
            };

            tableViewSections.RowSelect = true;
            tableViewSections.FocusedRowChanged += TableViewSectionsFocusedRowChanged; //todo: use selectedrowchanged (not fired correctly now)
        }

        private void TableViewSectionsFocusedRowChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(tableViewSections.CurrentFocusedRowObject as CrossSectionSection, new EventArgs());
            }
        }

        private DelftTools.Utils.Tuple<string, bool> TableViewEditorValidator(TableViewCell arg1, object arg2)
        {
            //TODO: Evaluate correctness of below code

            if (arg1.RowIndex < 0)
            {
                // we are apparanetly editing a newly added line; The recoprd has been added to the Data collection 
                if (arg1.Column.AbsoluteIndex == MinYColumn && sections.Count == 1)
                {
                    return new DelftTools.Utils.Tuple<string, bool>("The first from value can not be edited.", false);
                }

                if (arg1.Column.AbsoluteIndex == MaxYColumn)
                {
                    return new DelftTools.Utils.Tuple<string, bool>("The last to value can not be edited.", false);
                }

                return new DelftTools.Utils.Tuple<string, bool>("", true);
            }

            if (!(arg1.Column.AbsoluteIndex == MinYColumn || arg1.Column.AbsoluteIndex == MaxYColumn))
            {
                return new DelftTools.Utils.Tuple<string, bool>("", true);
            }

            var newValue = Convert.ToDouble(arg2);
            if (arg1.Column.AbsoluteIndex == MinYColumn)
            {
                if (newValue > GetRoughnessSection(arg1.RowIndex).MaxY)
                {
                    return new DelftTools.Utils.Tuple<string, bool>(
                        string.Format("Min value {0} must be less than max value {1}", newValue,
                                      GetRoughnessSection(arg1.RowIndex).MaxY), false);
                }

                // the first real value that is actually smaller
                CrossSectionSection prevSection = sections.FirstOrDefault(csr => csr.MinY <= newValue);

                var previous = double.MinValue;
                if (prevSection != null)
                {
                    previous = prevSection.MinY;
                }

                if (newValue <= previous)
                {
                    return new DelftTools.Utils.Tuple<string, bool>(
                        string.Format("Roughness segments can not overlap {0} < {1}", newValue,
                                      GetRoughnessSection(arg1.RowIndex - 1).MinY), false);
                }

                return new DelftTools.Utils.Tuple<string, bool>("", true);
            }

            if (newValue < GetRoughnessSection(arg1.RowIndex).MinY)
            {
                return new DelftTools.Utils.Tuple<string, bool>(
                    string.Format("Max value {0} must be greater than min value {1}", newValue,
                                  GetRoughnessSection(arg1.RowIndex).MinY), false);
            }

            CrossSectionSection nextSection = sections.FirstOrDefault(csr => csr.MaxY > newValue);
            var next = double.MaxValue;
            if (nextSection != null)
            {
                next = nextSection.MaxY;
            }

            if (newValue > next)
            {
                return new DelftTools.Utils.Tuple<string, bool>(
                    string.Format("Roughness segments can not overlap ({0} > {1})", newValue,
                                  GetRoughnessSection(arg1.RowIndex + 1).MaxY), false);
            }

            return new DelftTools.Utils.Tuple<string, bool>("", true);
        }

        private CrossSectionSection GetRoughnessSection(int i)
        {
            return ((IList<CrossSectionSection>) sections)[i];
        }

        private void UnSubscribeFromData()
        {
            if (sections != null)
            {
                sections.ListChanged -= SectionTableSectionsListChanged;
                sections.BeforeAddItem = null;
            }
        }

        private void SubscribeToData()
        {
            if (sections != null)
            {
                sections.ListChanged += SectionTableSectionsListChanged;
                sections.BeforeAddItem = SetCrossSectionSectionDefaultValues;
            }
        }

        private void SectionTableSectionsListChanged(object sender, ListChangedEventArgs e)
        {
            SetTableMutability();
        }

        private void SetTableMutability()
        {
            if (sections != null)
            {
                tableViewSections.AllowDeleteRow = sections.Count > 1; //can't delete the last row
                tableViewSections.AllowAddNewRow = sections.Count < MaximumNumberOfRoughnessSections;
            }
        }

        private void SetCrossSectionSectionDefaultValues(CrossSectionSection crossSectionSection)
        {
            UnSubscribeFromData();
            crossSectionSection.MinY = sections.Count == 0 ? MinY : MaxY;
            crossSectionSection.MaxY = MaxY;
            crossSectionSection.SectionType = sectionTypeList.FirstOrDefault();
            SubscribeToData();
        }

        private class CrossSectionSectionTypeFormatter : ICustomFormatter
        {
            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                return arg is CrossSectionSectionType
                           ? ((CrossSectionSectionType) arg).Name
                           : arg.ToString();
            }
        }

        #region IView<IBindingList> Members

        public object Data
        {
            get
            {
                return sections;
            }
            set
            {
                UnSubscribeFromData();
                sections = (SectionsBindingList) value;
                SubscribeToData();

                tableViewSections.Data = sections;
                SetTableMutability();
            }
        }

        public Image Image { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int MaximumNumberOfRoughnessSections { get; set; }

        public void EnsureVisible(object item) {}

        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}