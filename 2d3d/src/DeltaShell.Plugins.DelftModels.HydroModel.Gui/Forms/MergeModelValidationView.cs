using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Shell.Core;
using DelftTools.Shell.Gui;
using DelftTools.Shell.Gui.Swf.Validation;
using DelftTools.Utils;
using DelftTools.Utils.Validation;
using DeltaShell.Plugins.DelftModels.HydroModel.Gui.Properties;
using log4net;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Forms
{
    public partial class MergeModelValidationView : UserControl, ISuspendibleView, IAdditionalView
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MergeModelValidationView));

        private readonly Stopwatch stopwatch = new Stopwatch();
        private Func<object, object, ValidationReport> onMergeValidate;
        private Func<object, object, bool> onMerge;
        private bool suspend;
        private object data;

        public MergeModelValidationView()
        {
            InitializeComponent();
            validationReportControl.OnOpenViewForIssue = OpenViewForIssue;
        }

        public Func<object, object, ValidationReport> OnMergeValidate
        {
            get
            {
                return onMergeValidate;
            }
            set
            {
                onMergeValidate = value;
                RefreshReport();
            }
        }

        public Func<object, object, bool> OnMerge
        {
            get
            {
                return onMerge;
            }
            set
            {
                onMerge = value;
                RefreshReport();
            }
        }

        public IGui Gui { get; set; }
        public Image Image { get; set; }

        public object Data
        {
            get
            {
                return data;
            }
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

        public ViewInfo ViewInfo { get; set; }

        public void EnsureVisible(object item)
        {
            // Nothing to be done, enforced through IView
        }

        public void SuspendUpdates()
        {
            suspend = true;
        }

        public void ResumeUpdates()
        {
            suspend = false;
        }

        private void SetViewText()
        {
            if (data == null)
            {
                return;
            }

            var models = data as ValidateMergeModelObjects;

            string dataNameDestination = models.DestinationModel is INameable ? models.DestinationModel.Name : data.ToString();
            string dataNameSource = models.SourceModel is INameable ? models.SourceModel.Name : data.ToString();
            Text = string.Format(Resources.MergeModelValidationView__0__Model_merge_with__1__validation_Report, dataNameDestination, dataNameSource);
        }

        private bool RefreshReport()
        {
            if (Data == null || OnMergeValidate == null)
            {
                return true;
            }

            stopwatch.Restart();
            if (!(Data is ValidateMergeModelObjects models))
            {
                return true;
            }

            ValidationReport validationReport = OnMergeValidate(models.DestinationModel, models.SourceModel);
            ValidationReport oldValidationReport = validationReportControl.Data;

            if (!validationReport.Equals(oldValidationReport)) //only set if changed
            {
                validationReportControl.Data = validationReport;
                mergePanel.Visible = validationReport.ErrorCount <= 0;
                Image = ValidationReportControl.GetImageForSeverity(false, validationReport.Severity());

                // TextChanged triggers avalondock to update the image ;-)
                Text = Resources.Refreshing;
                SetViewText();
                // end TextChanged
            }

            stopwatch.Stop();

            // check the speed:
            long millisecondsPerRefresh = stopwatch.ElapsedMilliseconds;
            if (millisecondsPerRefresh < 1000) // fast
            {
                if (manualRefreshPanel.Visible)
                {
                    manualRefreshPanel.Visible = false;
                }
            }
            else // slow
            {
                if (!manualRefreshPanel.Visible)
                {
                    manualRefreshPanel.Visible = true;
                }

                return false; //don't restart the timer
            }

            return true;
        }

        private void RefreshTimerTick(object sender, EventArgs e)
        {
            if (suspend)
            {
                return;
            }

            refreshTimer.Stop();
            if (RefreshReport())
            {
                refreshTimer.Start();
            }
        }

        private void OpenViewForIssue(ValidationIssue issue)
        {
            if (Gui == null || issue.ViewData == null)
            {
                return;
            }

            bool viewOpen = Gui.DocumentViewsResolver.OpenViewForData(issue.ViewData);

            if (viewOpen)
            {
                IList<IView> views = Gui.DocumentViewsResolver.GetViewsForData(issue.ViewData);
                foreach (IView view in views)
                {
                    try
                    {
                        view.EnsureVisible(issue.Subject);
                    }
                    catch
                    {
                        log.DebugFormat("An error occured while calling EnsureVisible on view with name '{0}'", view.Text);
                    }
                }

                return;
            }

            if (issue.ViewData is IFileImporter fileImporter && fileImporter.CanImportOn(issue.Subject))
            {
                Gui.CommandHandler.ImportOn(issue.Subject, fileImporter);
            }
        }

        private void manualRefreshButton_Click(object sender, EventArgs e)
        {
            if (RefreshReport())
            {
                refreshTimer.Start();
            }
        }

        private void mergeButton_Click(object sender, EventArgs e)
        {
            if (Data == null || OnMerge == null)
            {
                return;
            }

            if (!(Data is ValidateMergeModelObjects models))
            {
                return;
            }

            if (OnMerge(models.DestinationModel, models.SourceModel))
            {
                label2.Text = Resources.MergeModelValidationView_Merge_successful;
            }
        }
    }
}