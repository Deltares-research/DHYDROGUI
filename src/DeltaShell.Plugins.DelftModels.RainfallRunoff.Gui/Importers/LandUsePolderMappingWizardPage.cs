using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls.Swf;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.Importers;
using DeltaShell.Plugins.NetworkEditor.Import;
using GeoAPI.Extensions.Feature;
using SharpMap.Data.Providers;
using MessageBox = DelftTools.Controls.Swf.MessageBox;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Importers
{
    public partial class LandUsePolderMappingWizardPage : UserControl, IWizardPage
    {
        private readonly ShapeFileSchemaReader schemaReader;
        private ShapeFile shapeFile;
        private bool validFile;

        public LandUsePolderMappingWizardPage()
        {
            InitializeComponent();
            schemaReader = new ShapeFileSchemaReader();

            selectLandUseFileControl.Filter = "Land use shape file (*.shp)|*.shp";
            selectLandUseFileControl.FileSelected += LandUseFileSelected;

            unitComboBox.DataSource = Enum.GetValues(typeof (RainfallRunoffEnums.AreaUnit));
            unitComboBox.SelectedIndex = 0;

            UpdateEnabledness();
        }

        public PolderFromGisImporter Importer { get; set; }

        #region IWizardPage Members

        public bool CanFinish()
        {
            return CanDoNext();
        }

        public bool CanDoNext()
        {
            bool noneValid = radioNone.Checked;
            bool attributeValid = radioAttributes.Checked; //todo: and something?
            bool landUseValid = radioLandUseFile.Checked && validFile;

            bool canDoNext = noneValid || attributeValid || landUseValid;

            if (canDoNext)
            {
                FillImporter();
            }

            return canDoNext;
        }

        public bool CanDoPrevious()
        {
            return true;
        }

        #endregion

        private void LandUseFileSelected(object sender, EventArgs e)
        {
            validFile = false;
            IList<string> columns = null;
            try
            {
                string file = selectLandUseFileControl.FileName;
                shapeFile = new ShapeFile(file);
                schemaReader.Path = file;
                schemaReader.OpenConnection();
                columns = schemaReader.GetColumnNames(null);
                validFile = columns.Count > 0;

                if (columns.Count == 0)
                {
                    MessageBox.Show(
                        "The selected file contains no valid columns/attributes to use, please select another.",
                        "Invalid file",
                        MessageBoxButtons.OK);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show("Invalid file, unable to open.\n\n" + ee.Message,
                                "Invalid file", MessageBoxButtons.OK);
            }

            UpdateEnabledness();

            if (validFile)
            {
                landUseCombobox.DataSource = columns;
            }
            else
            {
                landUseCombobox.DataSource = null;
            }
            OnLandUseCategorySelected();
        }

        private void FillImporter()
        {
            var landUseMapping = Importer.LandUseMappingConfiguration;
            landUseMapping.Use = radioLandUseFile.Checked;
            Importer.UseAttributeMapping = radioAttributes.Checked;

            if (landUseMapping.Use)
            {
                landUseMapping.Column = (string) landUseCombobox.SelectedValue;
                landUseMapping.Mapping = landUseMappingControl.GetMappingDictionary();
                landUseMapping.LandUseFeatureProvider = shapeFile;
            }
            if (Importer.UseAttributeMapping)
            {
                Importer.AttributeUnit = (RainfallRunoffEnums.AreaUnit) unitComboBox.SelectedItem;
                Importer.AttributeMapping = GenerateAttributeMapping();
            }
        }

        private IDictionary<PolderSubTypes, string> GenerateAttributeMapping()
        {
            return new Dictionary<PolderSubTypes, string>
                {
                    {PolderSubTypes.Grass, unpavedComboBox.SelectedItem.ToString()},
                    {PolderSubTypes.Paved, pavedComboBox.SelectedItem.ToString()},
                    {PolderSubTypes.lessThan500, greenhouseComboBox.SelectedItem.ToString()},
                    {PolderSubTypes.OpenWater, openwaterComboBox.SelectedItem.ToString()},
                };
        }

        private void LandUseComboboxSelectionChangeCommitted(object sender, EventArgs e)
        {
            OnLandUseCategorySelected();
        }

        private void OnLandUseCategorySelected()
        {
            var column = (string) landUseCombobox.SelectedValue;

            var categories = new List<object>();

            foreach (var feature in shapeFile.Features.Cast<IFeature>())
            {
                categories.Add(feature.Attributes[column]);
            }

            landUseMappingControl.LandUseCategories = categories.Distinct();
        }

        private void UpdateEnabledness()
        {
            attributeMappingPanel.Visible = radioAttributes.Checked;
            landUseMappingPanel.Visible = radioLandUseFile.Checked;
            bool showMapping = validFile && radioLandUseFile.Checked;
            landUseCombobox.Enabled = showMapping;
            mappingPanel.Visible = showMapping;
        }

        private void radioCheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledness();
        }

        public void OnCatchmentDataSourceSelected()
        {
            var catchmentImporter = Importer.CatchmentImporter;
            var possibleMappingColumns = (new[] {PolderFromGisImporter.NoneAttribute}.
                Concat(catchmentImporter.PossibleMappingColumns.Select(m => m.ColumnName))).ToArray();

            SetPossibleMappingColumns(unpavedComboBox, possibleMappingColumns);
            SetPossibleMappingColumns(pavedComboBox, possibleMappingColumns);
            SetPossibleMappingColumns(greenhouseComboBox, possibleMappingColumns);
            SetPossibleMappingColumns(openwaterComboBox, possibleMappingColumns);
        }

        private void SetPossibleMappingColumns(ComboBox comboBox, string[] possibleMappingColumns)
        {
            comboBox.Items.Clear();
            comboBox.Items.AddRange(possibleMappingColumns);
            comboBox.SelectedIndex = 0;
        }
    }
}