using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Validation;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class MergeModelValidationView : UserControl, ISuspendibleView, IAdditionalView
    {
        private Stopwatch stopwatch = new Stopwatch();
        private Func<object, object, ValidationReport> onMergeValidate;
        private Func<object, object, bool> onMerge;
        private bool suspend;
        private object data;
        public Image Image { get; set; }

        public MergeModelValidationView()
        {
            InitializeComponent();
            validationReportControl.OnOpenViewForIssue = OpenViewForIssue;
        }

        public object Data
        {
            get { return data; }
            set
            {
                data = value;

                if (data == null)
                {
                    Gui = null;
                }

                SetViewText();
                RefreshReport();
            }
        }

        private void SetViewText()
        {
            if (data == null) return;
            var models = data as ValidateMergeModelObjects;

            var dataNameDestination = (models.DestinationModel is INameable) ? ((INameable)models.DestinationModel).Name : data.ToString();
            var dataNameSource = (models.SourceModel is INameable) ? ((INameable)models.SourceModel).Name : data.ToString();
            Text = string.Format("{0} Model merge with {1} validation Report", dataNameDestination,dataNameSource);
        }

        public void EnsureVisible(object item)
        {
        }

        public ViewInfo ViewInfo { get; set; }

        private bool RefreshReport()
        {
            if (Data == null || OnMergeValidate == null)
                return true;

            stopwatch.Restart();
            var models = Data as ValidateMergeModelObjects;
            if (models == null)
                return true;

            var validationReport = OnMergeValidate(models.DestinationModel, models.SourceModel);
            var oldValidationReport = validationReportControl.Data;

            if (!validationReport.Equals(oldValidationReport)) //only set if changed
            {
                validationReportControl.Data = validationReport;
                mergePanel.Visible = validationReport.ErrorCount <= 0;
                Image = ValidationReportControl.GetImageForSeverity(false, validationReport.Severity());
                
                // TextChanged triggers avalondock to update the image ;-)
                Text = "Refreshing...";
                SetViewText(); 
                // end TextChanged
            }
            
            stopwatch.Stop();

            // check the speed:
            var millisecondsPerRefresh = stopwatch.ElapsedMilliseconds;
            if (millisecondsPerRefresh < 1000) // fast
            {
                if (manualRefreshPanel.Visible)
                    manualRefreshPanel.Visible = false;
            }
            else // slow
            {
                if (!manualRefreshPanel.Visible)
                    manualRefreshPanel.Visible = true;
                return false; //don't restart the timer
            }
            return true;
        }

        public Func<object, object, ValidationReport> OnMergeValidate
        {
            get { return onMergeValidate; }
            set
            {
                onMergeValidate = value;
                RefreshReport();
            }
        }

        public Func<object, object, bool> OnMerge
        {
            get { return onMerge; }
            set
            {
                onMerge = value;
                RefreshReport();
            }
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            if (suspend)
                return;

            refreshTimer.Stop();
            if (RefreshReport())
                refreshTimer.Start();
        }

        public IGui Gui { get; set; }
        
        private void OpenViewForIssue(ValidationIssue issue)
        {
            if (Gui == null || issue.ViewData == null) return;

            var viewOpen = Gui.DocumentViewsResolver.OpenViewForData(issue.ViewData);

            if (viewOpen)
            {
                var views = Gui.DocumentViewsResolver.GetViewsForData(issue.ViewData);
                foreach (var view in views)
                {
                    try
                    {
                        view.EnsureVisible(issue.Subject);
                    }
                    catch (Exception) { } //gulp
                }
                return;
            }

            var fileImporter = issue.ViewData as IFileImporter;
            if (fileImporter != null && fileImporter.CanImportOn(issue.Subject))
            {
                Gui.CommandHandler.ImportOn(issue.Subject, fileImporter);
            }
        }

        public void SuspendUpdates()
        {
            suspend = true;
        }

        public void ResumeUpdates()
        {
            suspend = false;
        }

        private void manualRefreshButton_Click(object sender, EventArgs e)
        {
            if (RefreshReport())
                refreshTimer.Start();
        }

        private void mergeButton_Click(object sender, EventArgs e)
        {
            if (Data == null || OnMerge == null)
                return;
            
            var models = Data as ValidateMergeModelObjects;
            if (models == null)
                return;
            
            if (OnMerge(models.DestinationModel, models.SourceModel))
                label2.Text = "Merge successful";
        }
    }
}
