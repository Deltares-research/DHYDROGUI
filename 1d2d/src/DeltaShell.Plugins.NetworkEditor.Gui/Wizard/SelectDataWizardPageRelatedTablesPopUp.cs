using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DeltaShell.Plugins.NetworkEditor.Import;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Wizard
{
    public partial class SelectDataWizardPageRelatedTablesPopUp : Form
    {
        private ISchemaReader schemaReader;
        private string tableName;
        private string strNone = "<none>";

        public SelectDataWizardPageRelatedTablesPopUp()
        {
            InitializeComponent();
        }



        public ISchemaReader SchemaReader
        {
            set
            {
                schemaReader = value;
                DataBindingGUI();
            }
        }



        public string TableName
        {
            set 
            { 
                tableName = value;
                DataBindingGUI();
            }
        }

        public string GetColumnNameID()
        {
            return comboBoxID.SelectedItem.ToString(); 
        }

        public List<RelatedTable> GetRelatedTables()
        {
            var relatedTables = new List<RelatedTable>();
            if(comboBoxRelatedTables1.SelectedItem.ToString() != strNone)
            {
                relatedTables.Add(new RelatedTable(comboBoxRelatedTables1.SelectedItem.ToString(), comboBoxForeignKey1.SelectedItem.ToString()));
            }
            if (comboBoxRelatedTables2.SelectedItem.ToString() != strNone)
            {
                relatedTables.Add(new RelatedTable(comboBoxRelatedTables2.SelectedItem.ToString(), comboBoxForeignKey2.SelectedItem.ToString()));
            }
            return relatedTables;
        }

        private void DataBindingGUI()
        {
           if(schemaReader == null || tableName == null)
           {
               return;
           }
           var lstTableNames = schemaReader.GetTableNames;
           lstTableNames.Remove(tableName);
           lstTableNames.Insert(0,strNone);

           var lst = new List<string>();
           lst.AddRange(lstTableNames);
           comboBoxRelatedTables1.DataSource = lst;

           lst = new List<string>();
           lst.AddRange(lstTableNames);
           comboBoxRelatedTables2.DataSource = lst;

           comboBoxID.DataSource = schemaReader.GetColumnNames(tableName);
           comboBoxID.SelectedIndex = 0;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void comboBoxRelatedTables1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox) sender;
            if(comboBox.SelectedIndex == 0)
            {
                comboBoxForeignKey1.DataSource = null;
                comboBoxForeignKey1.Enabled = false;
            }
            else
            {
                comboBoxForeignKey1.DataSource = schemaReader.GetColumnNames(comboBox.SelectedItem.ToString());
                comboBoxForeignKey1.Enabled = true;  
            }
        }

        private void comboBoxRelatedTables2_SelectedIndexChanged(object sender, EventArgs e)
        {
            var comboBox = (ComboBox)sender;
            if (comboBox.SelectedIndex == 0)
            {
                comboBoxForeignKey2.DataSource = null;
                comboBoxForeignKey2.Enabled = false;
            }
            else
            {
                comboBoxForeignKey2.DataSource = schemaReader.GetColumnNames(comboBox.SelectedItem.ToString());
                comboBoxForeignKey2.Enabled = true;
            }
        }
    }
}
