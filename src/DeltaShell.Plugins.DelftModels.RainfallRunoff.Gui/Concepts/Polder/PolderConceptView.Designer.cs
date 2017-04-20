using System.Windows.Forms;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Polder
{
    partial class PolderConceptView
    {
        private Panel pnlGeneral;
        private Panel pnlSummary;
        private Panel pnlLine;
        private Label lblTotaal;
        private Label lblUnit4;
        private Label lblUnit3;
        private Label lblUnit2;
        private Label lblUnit1;
        private TextBox txtAreaGreenhousePercentage;
        private TextBox txtAreaOpenwaterPercentage;
        private TextBox txtAreaUnpavedPercentage;
        private TextBox txtAreaPavedPercentage;
        private TextBox txtAreaOpenwater;
        private TextBox txtAreaGreenhouse;
        private TextBox txtAreaUnpaved;
        private TextBox txtAreaPaved;
        private Label lblOpenwater;
        private Label lblGreenhouse;
        private Label lblUnpaved;
        private Label lblDummy;
        private Label lblPaved;
        private Label lblPercentage4;
        private Label lblPercentage3;
        private Label lblPercentage2;
        private Label lblPercentage1;
        private Label lblTotalAreaPercentage;
        private Label lblTotalArea;
        private Label lblAreaPercentage;
        private Label lblArea;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pnlGeneral = new System.Windows.Forms.Panel();
            this.PolderPieChart = new DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Concepts.Polder.PolderPieChart();
            this.pnlSummary = new System.Windows.Forms.Panel();
            this.btnAddOpenWater = new System.Windows.Forms.Button();
            this.bindingSourcePolderViewData = new System.Windows.Forms.BindingSource(this.components);
            this.btnAddGreenhouse = new System.Windows.Forms.Button();
            this.btnAddUnpaved = new System.Windows.Forms.Button();
            this.btnAddPaved = new System.Windows.Forms.Button();
            this.lblTotalAreaPercentage = new System.Windows.Forms.Label();
            this.lblTotalArea = new System.Windows.Forms.Label();
            this.lblAreaPercentage = new System.Windows.Forms.Label();
            this.lblArea = new System.Windows.Forms.Label();
            this.lblPercentage5 = new System.Windows.Forms.Label();
            this.lblPercentage4 = new System.Windows.Forms.Label();
            this.lblPercentage3 = new System.Windows.Forms.Label();
            this.lblPercentage2 = new System.Windows.Forms.Label();
            this.lblPercentage1 = new System.Windows.Forms.Label();
            this.pnlLine = new System.Windows.Forms.Panel();
            this.lblTotaal = new System.Windows.Forms.Label();
            this.lblUnit5 = new System.Windows.Forms.Label();
            this.lblUnit4 = new System.Windows.Forms.Label();
            this.lblUnit3 = new System.Windows.Forms.Label();
            this.lblUnit2 = new System.Windows.Forms.Label();
            this.lblUnit1 = new System.Windows.Forms.Label();
            this.txtAreaGreenhousePercentage = new System.Windows.Forms.TextBox();
            this.txtAreaOpenwaterPercentage = new System.Windows.Forms.TextBox();
            this.txtAreaUnpavedPercentage = new System.Windows.Forms.TextBox();
            this.txtAreaPavedPercentage = new System.Windows.Forms.TextBox();
            this.txtAreaOpenwater = new System.Windows.Forms.TextBox();
            this.txtAreaGreenhouse = new System.Windows.Forms.TextBox();
            this.txtAreaUnpaved = new System.Windows.Forms.TextBox();
            this.txtAreaPaved = new System.Windows.Forms.TextBox();
            this.lblOpenwater = new System.Windows.Forms.Label();
            this.lblGreenhouse = new System.Windows.Forms.Label();
            this.lblUnpaved = new System.Windows.Forms.Label();
            this.lblDummy = new System.Windows.Forms.Label();
            this.lblPaved = new System.Windows.Forms.Label();
            this.pnlLine2 = new System.Windows.Forms.Panel();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPagePaved = new System.Windows.Forms.TabPage();
            this.tabPageUnpaved = new System.Windows.Forms.TabPage();
            this.tabPageGreenhouse = new System.Windows.Forms.TabPage();
            this.tabPageOpenWater = new System.Windows.Forms.TabPage();
            this.polderImageList = new System.Windows.Forms.ImageList(this.components);
            this.pnlGeneral.SuspendLayout();
            this.pnlSummary.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePolderViewData)).BeginInit();
            this.tabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlGeneral
            // 
            this.pnlGeneral.Controls.Add(this.PolderPieChart);
            this.pnlGeneral.Controls.Add(this.pnlSummary);
            this.pnlGeneral.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlGeneral.Location = new System.Drawing.Point(0, 0);
            this.pnlGeneral.MaximumSize = new System.Drawing.Size(0, 160);
            this.pnlGeneral.MinimumSize = new System.Drawing.Size(0, 160);
            this.pnlGeneral.Name = "pnlGeneral";
            this.pnlGeneral.Size = new System.Drawing.Size(748, 160);
            this.pnlGeneral.TabIndex = 0;
            // 
            // PolderPieChart
            // 
            this.PolderPieChart.Data = null;
            this.PolderPieChart.Location = new System.Drawing.Point(382, 5);
            this.PolderPieChart.Name = "PolderPieChart";
            this.PolderPieChart.Size = new System.Drawing.Size(158, 150);
            this.PolderPieChart.TabIndex = 1;
            this.PolderPieChart.TotalArea = 0D;
            // 
            // pnlSummary
            // 
            this.pnlSummary.Controls.Add(this.btnAddOpenWater);
            this.pnlSummary.Controls.Add(this.btnAddGreenhouse);
            this.pnlSummary.Controls.Add(this.btnAddUnpaved);
            this.pnlSummary.Controls.Add(this.btnAddPaved);
            this.pnlSummary.Controls.Add(this.lblTotalAreaPercentage);
            this.pnlSummary.Controls.Add(this.lblTotalArea);
            this.pnlSummary.Controls.Add(this.lblAreaPercentage);
            this.pnlSummary.Controls.Add(this.lblArea);
            this.pnlSummary.Controls.Add(this.lblPercentage5);
            this.pnlSummary.Controls.Add(this.lblPercentage4);
            this.pnlSummary.Controls.Add(this.lblPercentage3);
            this.pnlSummary.Controls.Add(this.lblPercentage2);
            this.pnlSummary.Controls.Add(this.lblPercentage1);
            this.pnlSummary.Controls.Add(this.pnlLine);
            this.pnlSummary.Controls.Add(this.lblTotaal);
            this.pnlSummary.Controls.Add(this.lblUnit5);
            this.pnlSummary.Controls.Add(this.lblUnit4);
            this.pnlSummary.Controls.Add(this.lblUnit3);
            this.pnlSummary.Controls.Add(this.lblUnit2);
            this.pnlSummary.Controls.Add(this.lblUnit1);
            this.pnlSummary.Controls.Add(this.txtAreaGreenhousePercentage);
            this.pnlSummary.Controls.Add(this.txtAreaOpenwaterPercentage);
            this.pnlSummary.Controls.Add(this.txtAreaUnpavedPercentage);
            this.pnlSummary.Controls.Add(this.txtAreaPavedPercentage);
            this.pnlSummary.Controls.Add(this.txtAreaOpenwater);
            this.pnlSummary.Controls.Add(this.txtAreaGreenhouse);
            this.pnlSummary.Controls.Add(this.txtAreaUnpaved);
            this.pnlSummary.Controls.Add(this.txtAreaPaved);
            this.pnlSummary.Controls.Add(this.lblOpenwater);
            this.pnlSummary.Controls.Add(this.lblGreenhouse);
            this.pnlSummary.Controls.Add(this.lblUnpaved);
            this.pnlSummary.Controls.Add(this.lblDummy);
            this.pnlSummary.Controls.Add(this.lblPaved);
            this.pnlSummary.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSummary.Location = new System.Drawing.Point(0, 0);
            this.pnlSummary.Name = "pnlSummary";
            this.pnlSummary.Size = new System.Drawing.Size(378, 160);
            this.pnlSummary.TabIndex = 0;
            // 
            // btnAddOpenWater
            // 
            this.btnAddOpenWater.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.bindingSourcePolderViewData, "HasNoOpenWater", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.btnAddOpenWater.Image = global::DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties.Resources.add;
            this.btnAddOpenWater.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddOpenWater.Location = new System.Drawing.Point(284, 102);
            this.btnAddOpenWater.Name = "btnAddOpenWater";
            this.btnAddOpenWater.Size = new System.Drawing.Size(85, 23);
            this.btnAddOpenWater.TabIndex = 29;
            this.btnAddOpenWater.Text = "Add";
            this.btnAddOpenWater.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAddOpenWater.UseVisualStyleBackColor = true;
            this.btnAddOpenWater.Click += new System.EventHandler(this.BtnAddOpenWaterClick);
            // 
            // bindingSourcePolderViewData
            // 
            this.bindingSourcePolderViewData.AllowNew = false;
            this.bindingSourcePolderViewData.DataSource = typeof(DeltaShell.Plugins.DelftModels.RainfallRunoff.Domain.Concepts.Polder.PolderConceptViewData);
            // 
            // btnAddGreenhouse
            // 
            this.btnAddGreenhouse.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.bindingSourcePolderViewData, "HasNoGreenhouse", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.btnAddGreenhouse.Image = global::DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties.Resources.add;
            this.btnAddGreenhouse.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddGreenhouse.Location = new System.Drawing.Point(284, 79);
            this.btnAddGreenhouse.Name = "btnAddGreenhouse";
            this.btnAddGreenhouse.Size = new System.Drawing.Size(85, 23);
            this.btnAddGreenhouse.TabIndex = 29;
            this.btnAddGreenhouse.Text = "Add";
            this.btnAddGreenhouse.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAddGreenhouse.UseVisualStyleBackColor = true;
            this.btnAddGreenhouse.Click += new System.EventHandler(this.BtnAddGreenhouseClick);
            // 
            // btnAddUnpaved
            // 
            this.btnAddUnpaved.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.bindingSourcePolderViewData, "HasNoUnpaved", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.btnAddUnpaved.Image = global::DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties.Resources.add;
            this.btnAddUnpaved.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddUnpaved.Location = new System.Drawing.Point(284, 56);
            this.btnAddUnpaved.Name = "btnAddUnpaved";
            this.btnAddUnpaved.Size = new System.Drawing.Size(85, 23);
            this.btnAddUnpaved.TabIndex = 29;
            this.btnAddUnpaved.Text = "Add";
            this.btnAddUnpaved.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAddUnpaved.UseVisualStyleBackColor = true;
            this.btnAddUnpaved.Click += new System.EventHandler(this.BtnAddUnpavedClick);
            // 
            // btnAddPaved
            // 
            this.btnAddPaved.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.bindingSourcePolderViewData, "HasNoPaved", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.btnAddPaved.Image = global::DeltaShell.Plugins.DelftModels.RainfallRunoff.Gui.Properties.Resources.add;
            this.btnAddPaved.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAddPaved.Location = new System.Drawing.Point(284, 33);
            this.btnAddPaved.Name = "btnAddPaved";
            this.btnAddPaved.Size = new System.Drawing.Size(85, 23);
            this.btnAddPaved.TabIndex = 29;
            this.btnAddPaved.Text = "Add";
            this.btnAddPaved.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnAddPaved.UseVisualStyleBackColor = true;
            this.btnAddPaved.Click += new System.EventHandler(this.BtnAddPavedClick);
            // 
            // lblTotalAreaPercentage
            // 
            this.lblTotalAreaPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "SumPercentages", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N1"));
            this.lblTotalAreaPercentage.Location = new System.Drawing.Point(287, 135);
            this.lblTotalAreaPercentage.Name = "lblTotalAreaPercentage";
            this.lblTotalAreaPercentage.Size = new System.Drawing.Size(64, 20);
            this.lblTotalAreaPercentage.TabIndex = 26;
            this.lblTotalAreaPercentage.Text = "0.0";
            this.lblTotalAreaPercentage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblTotalAreaPercentage.TextChanged += new System.EventHandler(this.LblTotalAreaPercentageTextChanged);
            // 
            // lblTotalArea
            // 
            this.lblTotalArea.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "SumAreas", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged, null, "N2"));
            this.lblTotalArea.Location = new System.Drawing.Point(137, 135);
            this.lblTotalArea.Name = "lblTotalArea";
            this.lblTotalArea.Size = new System.Drawing.Size(100, 20);
            this.lblTotalArea.TabIndex = 25;
            this.lblTotalArea.Text = "0.0";
            this.lblTotalArea.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblAreaPercentage
            // 
            this.lblAreaPercentage.BackColor = System.Drawing.Color.Transparent;
            this.lblAreaPercentage.Location = new System.Drawing.Point(284, 2);
            this.lblAreaPercentage.Name = "lblAreaPercentage";
            this.lblAreaPercentage.Size = new System.Drawing.Size(88, 31);
            this.lblAreaPercentage.TabIndex = 24;
            this.lblAreaPercentage.Text = "Percentage of geometry area";
            // 
            // lblArea
            // 
            this.lblArea.AutoSize = true;
            this.lblArea.BackColor = System.Drawing.Color.Transparent;
            this.lblArea.Location = new System.Drawing.Point(137, 7);
            this.lblArea.Name = "lblArea";
            this.lblArea.Size = new System.Drawing.Size(29, 13);
            this.lblArea.TabIndex = 23;
            this.lblArea.Text = "Area";
            // 
            // lblPercentage5
            // 
            this.lblPercentage5.AutoSize = true;
            this.lblPercentage5.Location = new System.Drawing.Point(357, 137);
            this.lblPercentage5.Name = "lblPercentage5";
            this.lblPercentage5.Size = new System.Drawing.Size(15, 13);
            this.lblPercentage5.TabIndex = 22;
            this.lblPercentage5.Text = "%";
            // 
            // lblPercentage4
            // 
            this.lblPercentage4.AutoSize = true;
            this.lblPercentage4.Location = new System.Drawing.Point(357, 106);
            this.lblPercentage4.Name = "lblPercentage4";
            this.lblPercentage4.Size = new System.Drawing.Size(15, 13);
            this.lblPercentage4.TabIndex = 22;
            this.lblPercentage4.Text = "%";
            // 
            // lblPercentage3
            // 
            this.lblPercentage3.AutoSize = true;
            this.lblPercentage3.Location = new System.Drawing.Point(357, 83);
            this.lblPercentage3.Name = "lblPercentage3";
            this.lblPercentage3.Size = new System.Drawing.Size(15, 13);
            this.lblPercentage3.TabIndex = 21;
            this.lblPercentage3.Text = "%";
            // 
            // lblPercentage2
            // 
            this.lblPercentage2.AutoSize = true;
            this.lblPercentage2.Location = new System.Drawing.Point(357, 62);
            this.lblPercentage2.Name = "lblPercentage2";
            this.lblPercentage2.Size = new System.Drawing.Size(15, 13);
            this.lblPercentage2.TabIndex = 20;
            this.lblPercentage2.Text = "%";
            // 
            // lblPercentage1
            // 
            this.lblPercentage1.AutoSize = true;
            this.lblPercentage1.Location = new System.Drawing.Point(357, 38);
            this.lblPercentage1.Name = "lblPercentage1";
            this.lblPercentage1.Size = new System.Drawing.Size(15, 13);
            this.lblPercentage1.TabIndex = 19;
            this.lblPercentage1.Text = "%";
            // 
            // pnlLine
            // 
            this.pnlLine.BackColor = System.Drawing.Color.Black;
            this.pnlLine.Location = new System.Drawing.Point(9, 129);
            this.pnlLine.Name = "pnlLine";
            this.pnlLine.Size = new System.Drawing.Size(358, 1);
            this.pnlLine.TabIndex = 18;
            // 
            // lblTotaal
            // 
            this.lblTotaal.Location = new System.Drawing.Point(6, 133);
            this.lblTotaal.Name = "lblTotaal";
            this.lblTotaal.Size = new System.Drawing.Size(100, 20);
            this.lblTotaal.TabIndex = 17;
            this.lblTotaal.Text = "Total";
            this.lblTotaal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblUnit5
            // 
            this.lblUnit5.AutoSize = true;
            this.lblUnit5.Location = new System.Drawing.Point(243, 137);
            this.lblUnit5.Name = "lblUnit5";
            this.lblUnit5.Size = new System.Drawing.Size(21, 13);
            this.lblUnit5.TabIndex = 16;
            this.lblUnit5.Text = "m2";
            // 
            // lblUnit4
            // 
            this.lblUnit4.AutoSize = true;
            this.lblUnit4.Location = new System.Drawing.Point(242, 105);
            this.lblUnit4.Name = "lblUnit4";
            this.lblUnit4.Size = new System.Drawing.Size(21, 13);
            this.lblUnit4.TabIndex = 16;
            this.lblUnit4.Text = "m2";
            // 
            // lblUnit3
            // 
            this.lblUnit3.AutoSize = true;
            this.lblUnit3.Location = new System.Drawing.Point(242, 82);
            this.lblUnit3.Name = "lblUnit3";
            this.lblUnit3.Size = new System.Drawing.Size(21, 13);
            this.lblUnit3.TabIndex = 15;
            this.lblUnit3.Text = "m2";
            // 
            // lblUnit2
            // 
            this.lblUnit2.AutoSize = true;
            this.lblUnit2.Location = new System.Drawing.Point(242, 60);
            this.lblUnit2.Name = "lblUnit2";
            this.lblUnit2.Size = new System.Drawing.Size(21, 13);
            this.lblUnit2.TabIndex = 14;
            this.lblUnit2.Text = "m2";
            // 
            // lblUnit1
            // 
            this.lblUnit1.AutoSize = true;
            this.lblUnit1.Location = new System.Drawing.Point(242, 35);
            this.lblUnit1.Name = "lblUnit1";
            this.lblUnit1.Size = new System.Drawing.Size(21, 13);
            this.lblUnit1.TabIndex = 13;
            this.lblUnit1.Text = "m2";
            // 
            // txtAreaGreenhousePercentage
            // 
            this.txtAreaGreenhousePercentage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "GreenhousePercentage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N1"));
            this.txtAreaGreenhousePercentage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasGreenhouse", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaGreenhousePercentage.Location = new System.Drawing.Point(287, 80);
            this.txtAreaGreenhousePercentage.Name = "txtAreaGreenhousePercentage";
            this.txtAreaGreenhousePercentage.Size = new System.Drawing.Size(64, 20);
            this.txtAreaGreenhousePercentage.TabIndex = 11;
            this.txtAreaGreenhousePercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAreaOpenwaterPercentage
            // 
            this.txtAreaOpenwaterPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "OpenwaterPercentage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N1"));
            this.txtAreaOpenwaterPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasOpenWater", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaOpenwaterPercentage.Location = new System.Drawing.Point(287, 103);
            this.txtAreaOpenwaterPercentage.Name = "txtAreaOpenwaterPercentage";
            this.txtAreaOpenwaterPercentage.Size = new System.Drawing.Size(64, 20);
            this.txtAreaOpenwaterPercentage.TabIndex = 12;
            this.txtAreaOpenwaterPercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAreaUnpavedPercentage
            // 
            this.txtAreaUnpavedPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "UnpavedPercentage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N1"));
            this.txtAreaUnpavedPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasUnpaved", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaUnpavedPercentage.Location = new System.Drawing.Point(287, 57);
            this.txtAreaUnpavedPercentage.Name = "txtAreaUnpavedPercentage";
            this.txtAreaUnpavedPercentage.Size = new System.Drawing.Size(64, 20);
            this.txtAreaUnpavedPercentage.TabIndex = 10;
            this.txtAreaUnpavedPercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAreaPavedPercentage
            // 
            this.txtAreaPavedPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "PavedPercentage", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N1"));
            this.txtAreaPavedPercentage.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasPaved", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaPavedPercentage.Location = new System.Drawing.Point(287, 34);
            this.txtAreaPavedPercentage.Name = "txtAreaPavedPercentage";
            this.txtAreaPavedPercentage.Size = new System.Drawing.Size(64, 20);
            this.txtAreaPavedPercentage.TabIndex = 9;
            this.txtAreaPavedPercentage.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAreaOpenwater
            // 
            this.txtAreaOpenwater.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "OpenWaterArea", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.txtAreaOpenwater.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasOpenWater", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaOpenwater.Location = new System.Drawing.Point(137, 103);
            this.txtAreaOpenwater.Name = "txtAreaOpenwater";
            this.txtAreaOpenwater.Size = new System.Drawing.Size(100, 20);
            this.txtAreaOpenwater.TabIndex = 8;
            this.txtAreaOpenwater.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAreaGreenhouse
            // 
            this.txtAreaGreenhouse.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "GreenhouseArea", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.txtAreaGreenhouse.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasGreenhouse", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaGreenhouse.Location = new System.Drawing.Point(137, 80);
            this.txtAreaGreenhouse.Name = "txtAreaGreenhouse";
            this.txtAreaGreenhouse.Size = new System.Drawing.Size(100, 20);
            this.txtAreaGreenhouse.TabIndex = 7;
            this.txtAreaGreenhouse.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAreaUnpaved
            // 
            this.txtAreaUnpaved.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "UnpavedArea", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.txtAreaUnpaved.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasUnpaved", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaUnpaved.Location = new System.Drawing.Point(137, 57);
            this.txtAreaUnpaved.Name = "txtAreaUnpaved";
            this.txtAreaUnpaved.Size = new System.Drawing.Size(100, 20);
            this.txtAreaUnpaved.TabIndex = 6;
            this.txtAreaUnpaved.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // txtAreaPaved
            // 
            this.txtAreaPaved.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.bindingSourcePolderViewData, "PavedArea", true, System.Windows.Forms.DataSourceUpdateMode.OnValidation, null, "N2"));
            this.txtAreaPaved.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.bindingSourcePolderViewData, "HasPaved", true, System.Windows.Forms.DataSourceUpdateMode.Never));
            this.txtAreaPaved.Location = new System.Drawing.Point(137, 34);
            this.txtAreaPaved.Name = "txtAreaPaved";
            this.txtAreaPaved.Size = new System.Drawing.Size(100, 20);
            this.txtAreaPaved.TabIndex = 5;
            this.txtAreaPaved.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblOpenwater
            // 
            this.lblOpenwater.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOpenwater.ForeColor = System.Drawing.Color.RoyalBlue;
            this.lblOpenwater.Location = new System.Drawing.Point(6, 103);
            this.lblOpenwater.Name = "lblOpenwater";
            this.lblOpenwater.Size = new System.Drawing.Size(100, 22);
            this.lblOpenwater.TabIndex = 4;
            this.lblOpenwater.Text = "Open water";
            // 
            // lblGreenhouse
            // 
            this.lblGreenhouse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblGreenhouse.ForeColor = System.Drawing.Color.Goldenrod;
            this.lblGreenhouse.Location = new System.Drawing.Point(6, 80);
            this.lblGreenhouse.Name = "lblGreenhouse";
            this.lblGreenhouse.Size = new System.Drawing.Size(100, 22);
            this.lblGreenhouse.TabIndex = 3;
            this.lblGreenhouse.Text = "Greenhouse";
            // 
            // lblUnpaved
            // 
            this.lblUnpaved.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUnpaved.ForeColor = System.Drawing.Color.Green;
            this.lblUnpaved.Location = new System.Drawing.Point(6, 57);
            this.lblUnpaved.Name = "lblUnpaved";
            this.lblUnpaved.Size = new System.Drawing.Size(100, 22);
            this.lblUnpaved.TabIndex = 2;
            this.lblUnpaved.Text = "Unpaved";
            // 
            // lblDummy
            // 
            this.lblDummy.Location = new System.Drawing.Point(3, 7);
            this.lblDummy.Name = "lblDummy";
            this.lblDummy.Size = new System.Drawing.Size(100, 20);
            this.lblDummy.TabIndex = 1;
            // 
            // lblPaved
            // 
            this.lblPaved.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPaved.ForeColor = System.Drawing.Color.Red;
            this.lblPaved.Location = new System.Drawing.Point(6, 34);
            this.lblPaved.Name = "lblPaved";
            this.lblPaved.Size = new System.Drawing.Size(100, 22);
            this.lblPaved.TabIndex = 0;
            this.lblPaved.Text = "Paved";
            // 
            // pnlLine2
            // 
            this.pnlLine2.BackColor = System.Drawing.Color.DimGray;
            this.pnlLine2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLine2.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLine2.Location = new System.Drawing.Point(0, 160);
            this.pnlLine2.Name = "pnlLine2";
            this.pnlLine2.Size = new System.Drawing.Size(748, 2);
            this.pnlLine2.TabIndex = 3;
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabPagePaved);
            this.tabControl.Controls.Add(this.tabPageUnpaved);
            this.tabControl.Controls.Add(this.tabPageGreenhouse);
            this.tabControl.Controls.Add(this.tabPageOpenWater);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.ImageList = this.polderImageList;
            this.tabControl.Location = new System.Drawing.Point(0, 162);
            this.tabControl.Name = "tabControl";
            this.tabControl.Padding = new System.Drawing.Point(10, 5);
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(748, 360);
            this.tabControl.TabIndex = 4;
            // 
            // tabPagePaved
            // 
            this.tabPagePaved.ImageKey = "paved.png";
            this.tabPagePaved.Location = new System.Drawing.Point(4, 27);
            this.tabPagePaved.Name = "tabPagePaved";
            this.tabPagePaved.Padding = new System.Windows.Forms.Padding(10);
            this.tabPagePaved.Size = new System.Drawing.Size(740, 329);
            this.tabPagePaved.TabIndex = 1;
            this.tabPagePaved.Text = "Paved";
            // 
            // tabPageUnpaved
            // 
            this.tabPageUnpaved.ImageKey = "unpaved.png";
            this.tabPageUnpaved.Location = new System.Drawing.Point(4, 27);
            this.tabPageUnpaved.Name = "tabPageUnpaved";
            this.tabPageUnpaved.Padding = new System.Windows.Forms.Padding(10);
            this.tabPageUnpaved.Size = new System.Drawing.Size(740, 329);
            this.tabPageUnpaved.TabIndex = 0;
            this.tabPageUnpaved.Text = "Unpaved";
            // 
            // tabPageGreenhouse
            // 
            this.tabPageGreenhouse.ImageKey = "greenhouse.png";
            this.tabPageGreenhouse.Location = new System.Drawing.Point(4, 27);
            this.tabPageGreenhouse.Name = "tabPageGreenhouse";
            this.tabPageGreenhouse.Padding = new System.Windows.Forms.Padding(10);
            this.tabPageGreenhouse.Size = new System.Drawing.Size(740, 329);
            this.tabPageGreenhouse.TabIndex = 2;
            this.tabPageGreenhouse.Text = "Greenhouse";
            // 
            // tabPageOpenWater
            // 
            this.tabPageOpenWater.ImageKey = "openwater.png";
            this.tabPageOpenWater.Location = new System.Drawing.Point(4, 27);
            this.tabPageOpenWater.Name = "tabPageOpenWater";
            this.tabPageOpenWater.Padding = new System.Windows.Forms.Padding(10);
            this.tabPageOpenWater.Size = new System.Drawing.Size(740, 329);
            this.tabPageOpenWater.TabIndex = 3;
            this.tabPageOpenWater.Text = "Open water";
            // 
            // polderImageList
            // 
            this.polderImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.polderImageList.ImageSize = new System.Drawing.Size(16, 16);
            this.polderImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // PolderConceptView
            // 
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlLine2);
            this.Controls.Add(this.pnlGeneral);
            this.Name = "PolderConceptView";
            this.Size = new System.Drawing.Size(748, 522);
            this.pnlGeneral.ResumeLayout(false);
            this.pnlSummary.ResumeLayout(false);
            this.pnlSummary.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcePolderViewData)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        private BindingSource bindingSourcePolderViewData;
        private System.ComponentModel.IContainer components;
        private PolderPieChart PolderPieChart;
        private Label lblPercentage5;
        private Label lblUnit5;
        private Panel pnlLine2;
        private TabControl tabControl;
        private TabPage tabPagePaved;
        private TabPage tabPageUnpaved;
        private TabPage tabPageGreenhouse;
        private TabPage tabPageOpenWater;
        private ImageList polderImageList;
        private Button btnAddPaved;
        private Button btnAddOpenWater;
        private Button btnAddGreenhouse;
        private Button btnAddUnpaved;
    }
}

