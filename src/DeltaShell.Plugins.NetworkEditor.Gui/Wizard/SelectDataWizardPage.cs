using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.NetworkEditor.Import;
using MessageBox = System.Windows.Forms.MessageBox;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    public partial class SelectDataWizardPage : UserControl, IWizardPage
    {
        private HydroRegionFromGisImporter hydroRegionFromGisImporter;
        private string fileFilter = "";
        private string lastVisitPath = "";
        private ISchemaReader schemaReader;
        private string columnNameId;
        private List<RelatedTable> lstRelatedTables;
        private DataTable dataTable;

        public SelectDataWizardPage()
        {
            InitializeComponent();
            InitializeDataTable();
            tableViewImportList.Data = dataTable;
            tableViewImportList.ReadOnly = true;
            tableViewImportList.ReadOnlyCellForeColor = Color.Black;
            tableViewImportList.AllowDeleteRow = true;
            lblNumberOfLevels.Visible = false;
            maskedTextBoxNumberOfLevels.Visible = false;
            lstRelatedTables = new List<RelatedTable>();

            errorProvider1.SetIconAlignment(btnAddToList, ErrorIconAlignment.MiddleLeft);
        }

        public bool CanFinish()
        {
            return true;
        }

        public bool CanDoNext()
        {
            if (hydroRegionFromGisImporter.FeatureFromGisImporters.Count > 0)
            {
                errorProvider1.SetError(btnAddToList, String.Empty); //remove error
                return true;
            }
            errorProvider1.SetError(btnAddToList, "Please add a feature to the import features list.");
            return false;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        public HydroRegionFromGisImporter HydroRegionFromGisImporter
        {
            private get
            {
                return hydroRegionFromGisImporter;
            }
            set
            {
                hydroRegionFromGisImporter = value;

                //types
                comboBoxFeatureType.Enabled = hydroRegionFromGisImporter.AvailableFeatureFromGisImporters.Count != 0;
                if (comboBoxFeatureType.Enabled)
                {
                    comboBoxFeatureType.DataSource = new BindingSource(hydroRegionFromGisImporter.AvailableFeatureFromGisImporters, null);
                    comboBoxFeatureType.DisplayMember = "Key";
                    comboBoxFeatureType.ValueMember = "Value";
                    comboBoxFeatureType.SelectedIndex = 0;
                }
                else
                {
                    comboBoxFeatureType.DataSource = null;
                }
                
                //list of importers
                hydroRegionFromGisImporter.FeatureFromGisImporters.CollectionChanged += importers_CollectionChanged;
                SetDataToTableViewImportList();

                //file formats
                var list = hydroRegionFromGisImporter.FileBasedFeatureProviders.Select(p => p.FileFilter).ToList();
                fileFilter = "";

                foreach (var filter in list)
                {
                    if (fileFilter != "")
                    {
                        fileFilter += "|";
                    }
                    fileFilter += filter;
                }

                fileFilter = "All supported files|*.shp;*.mdb|" + fileFilter;
            }
        }

        private void importers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetDataToTableViewImportList();
        }

        private void InitializeDataTable()
        {
            dataTable = new DataTable();
            dataTable.Columns.Add(new DataColumn("Network feature", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Path", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Table name", typeof(string)));
            dataTable.Columns.Add(new DataColumn("Filter value", typeof(string)));

            dataTable.RowDeleting += DataTableRowDeleting;
        }

        private void DataTableRowDeleting(object sender, DataRowChangeEventArgs e)
        {
           var importer =  hydroRegionFromGisImporter.FeatureFromGisImporters.First(
                imp => imp.FeatureFromGisImporterSettings.FeatureType == e.Row[0].ToString()
                && imp.FeatureFromGisImporterSettings.Path == e.Row[1].ToString());
            hydroRegionFromGisImporter.FeatureFromGisImporters.Remove(importer);
        }

        private void SetDataToTableViewImportList()
        {
            dataTable.Rows.Clear();

            foreach (var featureFromGisImporter in hydroRegionFromGisImporter.FeatureFromGisImporters)
            {
                var row = dataTable.NewRow();
                row[0] = featureFromGisImporter.FeatureFromGisImporterSettings.FeatureType;
                row[1] = featureFromGisImporter.FeatureFromGisImporterSettings.Path;
                row[2] = featureFromGisImporter.FeatureFromGisImporterSettings.TableName;
                row[3] = featureFromGisImporter.FeatureFromGisImporterSettings.DiscriminatorValue;
                dataTable.Rows.Add(row);
            }

            tableViewImportList.BestFitColumns();
        }

        private void comboBoxFeatureType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox) sender;
            txtPath.Enabled = true;
            btnSelectFile.Enabled = true;

            var visible = (comboBox.SelectedValue == typeof(CrossSectionZWFromGisImporter));
            lblNumberOfLevels.Visible = visible;
            maskedTextBoxNumberOfLevels.Enabled = visible;
            maskedTextBoxNumberOfLevels.Visible = visible;
        }

        private void comboBoxTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;

            if(comboBox.SelectedIndex == -1) return;

            var listColumns = schemaReader.GetColumnNames(Convert.ToString(comboBox.SelectedItem), true);
            listColumns.Insert(0, "<None>");
            comboBoxDiscriminatorColumn.DataSource = listColumns;
            comboBoxDiscriminatorColumn.SelectedIndex = 0;
            comboBoxDiscriminatorColumn.Enabled = true;
        }

        private void comboBoxDiscriminatorColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox) sender;
            if(comboBox.SelectedIndex <= 0)
            {
                comboBoxDiscriminatorValue.DataSource = null;
                comboBoxDiscriminatorValue.Enabled = false;             
            }
            else
            {
                comboBoxDiscriminatorValue.DataSource = schemaReader.GetDistinctValues(Convert.ToString(comboBoxTable.SelectedItem), Convert.ToString(comboBox.SelectedItem));
                comboBoxDiscriminatorValue.Enabled = true;
                if(comboBoxDiscriminatorValue.Items.Count > 0)
                {
                    comboBoxDiscriminatorValue.SelectedIndex = 0;
                }
            }
        }

        private static IEnumerable<ISchemaReader> AvailableSchemaReaders
        {
            get
            {
                yield return new OleDbSchemaReader();
                yield return new ShapeFileSchemaReader();
            }
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {

            var openFileDialog =  new OpenFileDialog
            {
                InitialDirectory = lastVisitPath,
                Filter = fileFilter,
                RestoreDirectory = true,
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK )
            {
                var path = openFileDialog.FileName;
                var extension = Path.GetExtension(path);
                lastVisitPath = Path.GetDirectoryName(path);
                txtPath.Text = path;

                var featureProvider = hydroRegionFromGisImporter.FileBasedFeatureProviders.First(p => p.FileFilter.Contains(extension));
                schemaReader = AvailableSchemaReaders.First(sr => sr.FileExtensions.Contains(extension));

                schemaReader.Path = path;
                schemaReader.OpenConnection();

                if(featureProvider.IsRelationalDataBase)
                {
                    comboBoxTable.DataSource = schemaReader.GetTableNames;
                    comboBoxTable.Enabled = true;
                    comboBoxTable.SelectedIndex = 0;
                    btnRelatedTables.Enabled = true;
                }
                else
                {
                    comboBoxTable.DataSource = null;
                    comboBoxTable.Enabled = false;

                    var listColumns = schemaReader.GetColumnNames(null, true);
                    listColumns.Insert(0, "<None>");
                    comboBoxDiscriminatorColumn.DataSource = listColumns;
                    comboBoxDiscriminatorColumn.SelectedIndex = 0;
                    comboBoxDiscriminatorColumn.Enabled = true;
                }

                btnAddToList.Enabled = true;
            }
        }

        private void btnAddToList_Click(object sender, EventArgs e)
        {
            if(AddNetworkFeatureImporter())
            {
                ClearAddFeatureBox();
                btnAddToList.Enabled = false;
            }
        }

        private bool AddNetworkFeatureImporter()
        {
            var mappingColumns = GetPossibleMappingColumns(schemaReader,Convert.ToString(comboBoxTable.SelectedItem),lstRelatedTables);

            var geometryColumn = GetGeometryColumn(mappingColumns);

            if (geometryColumn == null && schemaReader.IsRelationalDataBase)
            {
                MessageBox.Show("There is no (or more than one) geometry column available");
                return false;
            }

            var discriminator = Convert.ToString(comboBoxDiscriminatorColumn.SelectedItem);

            if (discriminator.Contains("<None>"))
            {
                discriminator = null;
            }

            var networkFeatureFromGisImporter = FeatureFromGisImporterBase.CreateNetworkFeatureFromGisImporter((Type)comboBoxFeatureType.SelectedValue);
            if (networkFeatureFromGisImporter == null) return false;

            networkFeatureFromGisImporter.FileBasedFeatureProviders = hydroRegionFromGisImporter.FileBasedFeatureProviders;
            networkFeatureFromGisImporter.HydroRegion = hydroRegionFromGisImporter.HydroRegion;
            networkFeatureFromGisImporter.FeatureFromGisImporterSettings.Path = txtPath.Text;
            networkFeatureFromGisImporter.FeatureFromGisImporterSettings.TableName = Convert.ToString(comboBoxTable.SelectedItem);
            networkFeatureFromGisImporter.FeatureFromGisImporterSettings.DiscriminatorColumn = discriminator;
            networkFeatureFromGisImporter.FeatureFromGisImporterSettings.DiscriminatorValue = Convert.ToString(comboBoxDiscriminatorValue.SelectedItem);

            var crossSectionZwFromGisImporter = networkFeatureFromGisImporter as CrossSectionZWFromGisImporter;
            if(crossSectionZwFromGisImporter != null)
            {
                crossSectionZwFromGisImporter.NumberOfLevels = Convert.ToInt32(maskedTextBoxNumberOfLevels.Text);
            }

            networkFeatureFromGisImporter.FeatureFromGisImporterSettings.GeometryColumn = geometryColumn;
            networkFeatureFromGisImporter.FeatureFromGisImporterSettings.ColumnNameID = columnNameId;
            networkFeatureFromGisImporter.FeatureFromGisImporterSettings.RelatedTables.AddRange(lstRelatedTables);
            networkFeatureFromGisImporter.PossibleMappingColumns.AddRange(mappingColumns);

            HydroRegionFromGisImporter.FeatureFromGisImporters.Add(networkFeatureFromGisImporter);

            tableViewImportList.BestFitColumns();

            return true;
        }

        private MappingColumn GetGeometryColumn(IEnumerable<MappingColumn> columns)
        {
            var lstGeometryColumns = columns.Where(c => c.Alias.ToUpper() == "SHAPE").ToList();
            return lstGeometryColumns.Count() == 1 ? lstGeometryColumns.First() : null;
        }

        private static List<MappingColumn> GetPossibleMappingColumns(ISchemaReader schemaReader, string tableName,
            IEnumerable<RelatedTable> relatedTables)
        {
            var lstColumnNames = schemaReader.GetColumnNames(tableName, true);
            var possibleMappingColumns =
                lstColumnNames.Select(columnName => new MappingColumn(tableName, columnName)).ToList();

            foreach (var relatedTable in relatedTables)
            {
                tableName = relatedTable.TableName;
                lstColumnNames = schemaReader.GetColumnNames(tableName, true);
                possibleMappingColumns.AddRange(
                    lstColumnNames.Select(columnName => new MappingColumn(tableName, columnName)));
            }

            return possibleMappingColumns;
        }

        private void ClearAddFeatureBox()
        {
            if (comboBoxFeatureType.Items.Count > 0)
            {
                comboBoxFeatureType.SelectedIndex = 0;
            }

            txtPath.Text = "";
            comboBoxTable.DataSource = null;
            comboBoxTable.Enabled = false;
            comboBoxDiscriminatorColumn.DataSource = null;
            comboBoxDiscriminatorColumn.Enabled = false;
            comboBoxDiscriminatorValue.DataSource = null;
            comboBoxDiscriminatorValue.Enabled = false;
            btnRelatedTables.Enabled = false;
        }

        private void btnRelatedTables_Click(object sender, EventArgs e)
        {
            var selectDataWizardPageRelatedTablesPopUp = new SelectDataWizardPageRelatedTablesPopUp
            {
                TableName = comboBoxTable.SelectedItem.ToString(),
                SchemaReader = schemaReader
            };
            if(selectDataWizardPageRelatedTablesPopUp.ShowDialog() == DialogResult.OK)
            {
                columnNameId = selectDataWizardPageRelatedTablesPopUp.GetColumnNameID();
                lstRelatedTables = selectDataWizardPageRelatedTablesPopUp.GetRelatedTables();
            }
        }

        private void buttonLoadMappingFile_Click(object sender, EventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Load importers from file",
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "Mapping file (.xml)|*.xml",
                RestoreDirectory = true
            };

            if(openFileDialog.ShowDialog() == DialogResult.OK)
            {
                HydroNetworkFromGisImporterXmlSerializer.Deserialize(hydroRegionFromGisImporter,openFileDialog.FileName);
                SetMappingColumnsToImportersAfterLoading(hydroRegionFromGisImporter);
            }
        }

        private static void SetMappingColumnsToImportersAfterLoading(HydroRegionFromGisImporter importer)
        {
            foreach (var networkFeatureFromGisImporter in importer.FeatureFromGisImporters)
            {
                var filePath = networkFeatureFromGisImporter.FeatureFromGisImporterSettings.Path;
                if (networkFeatureFromGisImporter.PossibleMappingColumns.Count == 0 && File.Exists(filePath))
                {
                    var schemaReader =
                        AvailableSchemaReaders.First(r => r.FileExtensions.Contains(Path.GetExtension(filePath)));

                    schemaReader.Path = filePath;
                    schemaReader.OpenConnection();

                    networkFeatureFromGisImporter.PossibleMappingColumns.AddRange(GetPossibleMappingColumns(
                        schemaReader, networkFeatureFromGisImporter.FeatureFromGisImporterSettings.TableName,
                        networkFeatureFromGisImporter.FeatureFromGisImporterSettings.RelatedTables));

                    schemaReader.CloseConnection();
                }
            }
        }
    }
}
