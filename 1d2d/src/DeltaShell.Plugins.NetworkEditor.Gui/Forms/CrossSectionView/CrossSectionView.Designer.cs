using System.Windows.Forms;

namespace DeltaShell.Plugins.NetworkEditor.Gui.Forms.CrossSectionView
{
    partial class CrossSectionView
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
            this.btnShare = new System.Windows.Forms.Button();
            this.bindingSourceCrossSectionViewModel = new System.Windows.Forms.BindingSource(this.components);
            this.btnEdit = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxDefinitions = new System.Windows.Forms.ComboBox();
            this.rbShared = new System.Windows.Forms.RadioButton();
            this.rbLocal = new System.Windows.Forms.RadioButton();
            this.btnShowConveyance = new System.Windows.Forms.Button();
            this.definitionView = new CrossSectionDefinitionView();
            this.panelForConveyanceBtn = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceCrossSectionViewModel)).BeginInit();
            this.panelForConveyanceBtn.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnShare);
            this.panel1.Controls.Add(this.btnEdit);
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.comboBoxDefinitions);
            this.panel1.Controls.Add(this.rbShared);
            this.panel1.Controls.Add(this.rbLocal);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1158, 70);
            this.panel1.TabIndex = 1;
            // 
            // btnShare
            // 
            this.btnShare.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceCrossSectionViewModel, "CanShareDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.btnShare.Location = new System.Drawing.Point(287, 12);
            this.btnShare.Margin = new System.Windows.Forms.Padding(2);
            this.btnShare.Name = "btnShare";
            this.btnShare.Size = new System.Drawing.Size(172, 19);
            this.btnShare.TabIndex = 6;
            this.btnShare.Text = "Share this definition";
            this.btnShare.UseVisualStyleBackColor = true;
            this.btnShare.Click += new System.EventHandler(this.BtnShareClick);
            // 
            // bindingSourceCrossSectionViewModel
            // 
            this.bindingSourceCrossSectionViewModel.DataSource = typeof(CrossSectionViewModel);
            // 
            // btnEdit
            // 
            this.btnEdit.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceCrossSectionViewModel, "UseSharedDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.btnEdit.Location = new System.Drawing.Point(287, 35);
            this.btnEdit.Margin = new System.Windows.Forms.Padding(2);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new System.Drawing.Size(22, 19);
            this.btnEdit.TabIndex = 5;
            this.btnEdit.Text = "...";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new System.EventHandler(this.BtnEditClick);
            // 
            // textBox1
            // 
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourceCrossSectionViewModel, "LevelShift", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N3"));
            this.textBox1.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceCrossSectionViewModel, "UseSharedDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox1.Location = new System.Drawing.Point(383, 34);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(76, 20);
            this.textBox1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(329, 38);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "level shift";
            // 
            // comboBoxDefinitions
            // 
            this.comboBoxDefinitions.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.comboBoxDefinitions.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.comboBoxDefinitions.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceCrossSectionViewModel, "UseSharedDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.comboBoxDefinitions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDefinitions.FormattingEnabled = true;
            this.comboBoxDefinitions.Location = new System.Drawing.Point(154, 35);
            this.comboBoxDefinitions.Margin = new System.Windows.Forms.Padding(2);
            this.comboBoxDefinitions.Name = "comboBoxDefinitions";
            this.comboBoxDefinitions.Size = new System.Drawing.Size(132, 21);
            this.comboBoxDefinitions.TabIndex = 2;
            this.comboBoxDefinitions.SelectedIndexChanged += new System.EventHandler(this.ComboBoxDefinitionsSelectedIndexChanged);
            // 
            // rbShared
            // 
            this.rbShared.AutoSize = true;
            this.rbShared.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourceCrossSectionViewModel, "CanSelectSharedDefinitions", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.rbShared.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceCrossSectionViewModel, "UseSharedDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.rbShared.Location = new System.Drawing.Point(22, 36);
            this.rbShared.Margin = new System.Windows.Forms.Padding(2);
            this.rbShared.Name = "rbShared";
            this.rbShared.Size = new System.Drawing.Size(122, 17);
            this.rbShared.TabIndex = 11;
            this.rbShared.Text = "use shared definition";
            this.rbShared.UseVisualStyleBackColor = true;
            // 
            // rbLocal
            // 
            this.rbLocal.AutoSize = true;
            this.rbLocal.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingSourceCrossSectionViewModel, "UseLocalDefinition", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.rbLocal.Location = new System.Drawing.Point(22, 14);
            this.rbLocal.Margin = new System.Windows.Forms.Padding(2);
            this.rbLocal.Name = "rbLocal";
            this.rbLocal.Size = new System.Drawing.Size(112, 17);
            this.rbLocal.TabIndex = 10;
            this.rbLocal.Text = "use local definition";
            this.rbLocal.UseVisualStyleBackColor = true;
            // 
            // btnShowConveyance
            // 
            this.btnShowConveyance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowConveyance.Location = new System.Drawing.Point(1026, 11);
            this.btnShowConveyance.Margin = new System.Windows.Forms.Padding(2);
            this.btnShowConveyance.Name = "btnShowConveyance";
            this.btnShowConveyance.Size = new System.Drawing.Size(115, 19);
            this.btnShowConveyance.TabIndex = 7;
            this.btnShowConveyance.Text = "Show conveyance";
            this.btnShowConveyance.UseVisualStyleBackColor = true;
            this.btnShowConveyance.Click += new System.EventHandler(this.btnShowConveyance_Click);
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
            // panel2
            // 
            this.panelForConveyanceBtn.Controls.Add(this.btnShowConveyance);
            this.panelForConveyanceBtn.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelForConveyanceBtn.Location = new System.Drawing.Point(0, 745);
            this.panelForConveyanceBtn.Name = "panelForConveyanceBtn";
            this.panelForConveyanceBtn.Size = new System.Drawing.Size(1158, 41);
            this.panelForConveyanceBtn.TabIndex = 8;
            // 
            // CrossSectionView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.definitionView);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panelForConveyanceBtn);
            this.Name = "CrossSectionView";
            this.Size = new System.Drawing.Size(1158, 786);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourceCrossSectionViewModel)).EndInit();
            this.panelForConveyanceBtn.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private CrossSectionDefinitionView definitionView;
        private System.Windows.Forms.RadioButton rbShared;
        private System.Windows.Forms.RadioButton rbLocal;
        private BindingSource bindingSourceCrossSectionViewModel;
        private Button btnEdit;
        private TextBox textBox1;
        private Label label1;
        private ComboBox comboBoxDefinitions;
        private Button btnShare;
        private Button btnShowConveyance;
        private Panel panelForConveyanceBtn;
    }
}
