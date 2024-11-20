using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class CrossSectionPipeView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnNewSharedDefinition = new System.Windows.Forms.Button();
            this.bindingSourceCrossSectionViewModel = new System.Windows.Forms.BindingSource(this.components);
            this.btnEdit = new System.Windows.Forms.Button();
            this.comboBoxDefinitions = new System.Windows.Forms.ComboBox();
            this.definitionView = new CrossSectionDefinitionView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceCrossSectionViewModel)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnNewSharedDefinition);
            this.panel1.Controls.Add(this.btnEdit);
            this.panel1.Controls.Add(this.comboBoxDefinitions);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1158, 70);
            this.panel1.TabIndex = 1;
            // 
            // btnNewSharedDefinition
            // 
            this.btnNewSharedDefinition.Location = new System.Drawing.Point(22, 12);
            this.btnNewSharedDefinition.Margin = new System.Windows.Forms.Padding(2);
            this.btnNewSharedDefinition.Name = "btnNewSharedDefinition";
            this.btnNewSharedDefinition.Size = new System.Drawing.Size(172, 19);
            this.btnNewSharedDefinition.TabIndex = 6;
            this.btnNewSharedDefinition.Text = "Create new shared definition";
            this.btnNewSharedDefinition.UseVisualStyleBackColor = true;
            this.btnNewSharedDefinition.Click += new System.EventHandler(this.BtnShareClick);
            // 
            // bindingSourceCrossSectionViewModel
            // 
            this.bindingSourceCrossSectionViewModel.DataSource = typeof(CrossSectionViewModel);
            // 
            // btnEdit
            // 
            this.btnEdit.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceCrossSectionViewModel, "UseSharedDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.btnEdit.Location = new System.Drawing.Point(154, 35);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(22, 20);
            this.btnEdit.TabIndex = 5;
            this.btnEdit.Text = "...";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.BtnEditClick);
            // 
            // comboBoxDefinitions
            // 
            this.comboBoxDefinitions.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxDefinitions.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxDefinitions.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceCrossSectionViewModel, "UseSharedDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxDefinitions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDefinitions.FormattingEnabled = true;
            this.comboBoxDefinitions.Location = new System.Drawing.Point(22, 35);
            this.comboBoxDefinitions.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxDefinitions.Name = "comboBoxDefinitions";
            this.comboBoxDefinitions.Size = new System.Drawing.Size(132, 21);
            this.comboBoxDefinitions.TabIndex = 2;
            this.comboBoxDefinitions.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDefinitionsSelectedIndexChanged);
            // 
            // definitionView
            // 
            this.definitionView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.definitionView.HistoryToolEnabled = false;
            this.definitionView.Image = null;
            this.definitionView.Locked = false;
            this.definitionView.Location = new System.Drawing.Point(0, 70);
            this.definitionView.Margin = new System.Windows.Forms.Padding(4);
            this.definitionView.Name = "definitionView";
            this.definitionView.Size = new System.Drawing.Size(1158, 675);
            this.definitionView.TabIndex = 0;
            // 
            // CrossSectionView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.definitionView);
            this.Controls.Add(this.panel1);
            this.Name = "CrossSectionPipeView";
            this.Size = new System.Drawing.Size(1158, 786);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceCrossSectionViewModel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private CrossSectionDefinitionView definitionView;
        private BindingSource bindingSourceCrossSectionViewModel;
        private Button btnEdit;
        private ComboBox comboBoxDefinitions;
        private Button btnNewSharedDefinition;
    }
}
