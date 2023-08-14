using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DelftTools.Controls;
using DelftTools.Utils.Aop;
using DeltaShell.Plugins.FMSuite.Common.ModelSchema;
using DeltaShell.Plugins.FMSuite.FlowFM.Gui.NodePresenters;
using DeltaShell.Plugins.FMSuite.FlowFM.Model;
using DeltaShell.Plugins.FMSuite.FlowFM.ModelDefinition;
using NetTopologySuite.Extensions.Features;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    public class FileBasedItemProperties
    {
        public FileBasedItemProperties(FileBasedModelItem item)
        {
            Property = item.PropertyName;
            FullPath = item.FileExists ? item.FilePath : "<no file created yet>";
        }

        public string Property { get; private set; }

        [DisplayName("Full path")]
        public string FullPath { get; private set; }
    }

    public partial class WaterFlowFMFileStructureView : UserControl, IAdditionalView
    {
        private WaterFlowFMModel model;

        public WaterFlowFMFileStructureView()
        {
            InitializeComponent();
            treeView.NodePresenters.Add(new WaterFlowFMFileBasedItemNodePresenter { Model = Model });
            treeView.SelectedNodeChanged += SelectedNodeChanged;
        }

        public WaterFlowFMModel Model
        {
            get
            {
                return model;
            }
            set
            {
                if (model != null)
                {
                    ((INotifyPropertyChanged)model).PropertyChanged -= ModelPropertyChanged;
                    ((INotifyCollectionChanged)model).CollectionChanged -= ModelCollectionChanged;
                }

                model = value;
                if (model != null)
                {
                    ((INotifyPropertyChanged)model).PropertyChanged += ModelPropertyChanged;
                    ((INotifyCollectionChanged)model).CollectionChanged += ModelCollectionChanged;
                }

                if (model != null)
                {
                    if (treeView.NodePresenters.FirstOrDefault() is WaterFlowFMFileBasedItemNodePresenter nodePresenter)
                    {
                        nodePresenter.Model = model;
                    }
                }

                RefreshTree();
            }
        }

        private void SelectedNodeChanged(object sender, EventArgs eventArgs)
        {
            var item = treeView.SelectedNode.Tag as FileBasedModelItem;
            propertyGrid1.SelectedObject = item == null ? null : new FileBasedItemProperties(item);
        }

        private void ModelCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RefreshTree();
        }

        private void ModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Equals(sender, Model) && e.PropertyName == nameof(Model.Name) ||
                Model.Boundaries.Contains(sender) && e.PropertyName == nameof(Feature2D.Name) ||
                //  also listen to renames of boundaries, because if the files are new, they also change file name when saving.
                Model.Pipes.Contains(sender) && e.PropertyName == nameof(Feature2D.Name) ||
                Model.LateralFeatures.Contains(sender) && e.PropertyName == nameof(Feature2D.Name))
            {
                RefreshTree();
            }
        }

        [InvokeRequired]
        private void RefreshTree()
        {
            if (treeView.Data is FileBasedModelItem parentNode)
            {
                parentNode.Clear();
            }

            treeView.Data = model == null ? null : WaterFlowFMFileBasedItemFactory.CreateParentNode(model);
        }

        #region IView

        public object Data
        {
            get
            {
                return Model;
            }
            set
            {
                Model = value as WaterFlowFMModel;
            }
        }

        public Image Image { get; set; }

        public void EnsureVisible(object item)
        {
            treeView.EnsureVisible(item);
        }

        public ViewInfo ViewInfo { get; set; }

        #endregion
    }
}