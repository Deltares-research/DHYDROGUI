﻿using System;
using System.Windows.Controls;
using DelftTools.Controls;
using DeltaShell.Plugins.DelftModels.HydroModel.Import;
using Microsoft.Win32;
using Image = System.Drawing.Image;

namespace DeltaShell.Plugins.DelftModels.HydroModel.Gui.Views
{
    /// <summary>
    /// Interaction logic for DimrTemplateView.xaml
    /// </summary>
    public sealed partial class DimrTemplateView : UserControl, IProjectTemplateSettingsView
    {
        private bool disposed;
        public DimrTemplateView()
        {
            InitializeComponent();
            
            ViewModel.GetFilePath = () =>
            {
                var dialog = new OpenFileDialog
                {
                    Multiselect = false,
                    CheckPathExists = true,
                    Title = DHydroConfigXmlImporter.Name,
                    Filter = DHydroConfigXmlImporter.FileFilter
                };

                var result = dialog.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    return dialog.FileName;
                }

                return "";
            };
        }

        public object Data { get; set; }

        public string Text { get; set; }

        public Image Image { get; set; }

        public ViewInfo ViewInfo { get; set; }

        public DHydroConfigXmlImporter DHydroConfigXmlImporter { get; set; }

        public Action<object> ExecuteProjectTemplate
        {
            get { return ViewModel.ExecuteProjectTemplate; }
            set { ViewModel.ExecuteProjectTemplate = value; }
        }

        public Action Cancel
        {
            get { return ViewModel.Cancel; }
            set { ViewModel.Cancel = value; }
        }
        
        public void EnsureVisible(object item)
        {
            // nothing to focus
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Image?.Dispose();
            }

            disposed = true;
        }
    }
}
