using System;
using DelftTools.Shell.Core.Workflow.DataItems;
using DeltaShell.NGHS.IO.DataObjects;

namespace DeltaShell.Plugins.DelftModels.WaterFlowModel.Gui.Forms.ProjectExplorer
{
    /// <summary>
    /// Derivative of DataItem that overrides some linking methods/properties in order to function as an adapter for boundary node data objects
    /// </summary>
    /// <remarks>Only use this class in node presenters (the class is not mapped...!)</remarks>
    /// <remarks>Deriving DataItem is preferred over implementing a random object wrapper class; we want to act exactly like a DataItem (via the DataItemNodePresenter)</remarks>
    // TODO: Shouldn't we refactor WaterFlowModel1DBoundaryNodeData? Hacks like these make the implementation more and more exotic...
    public class WaterFlowModel1DBoundaryNodeDataItem : DataItem
    {
        public override bool LinkTo(IDataItem source)
        {
            throw new InvalidOperationException("Link for user interface-related data items should not be called");
        }

        public override bool IsLinked
        {
            get { return ((Model1DBoundaryNodeData)Value).SeriesDataItem.IsLinked; }
        }

        public override string Name
        {
            get { return ((Model1DBoundaryNodeData)Value).Name; }
            set { }
        }

        public override void Unlink()
        {
            ((Model1DBoundaryNodeData)Value).SeriesDataItem.Unlink();
        }

        protected bool Equals(WaterFlowModel1DBoundaryNodeDataItem other)
        {
            return Equals((object)other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var bc = Value as Model1DBoundaryNodeData;
            if (bc != null)
            {
                if (Equals(obj, bc) || Equals(obj, bc.SeriesDataItem)) // oops, make it look like original data item, for tree view
                {
                    return true;
                }
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            
            return GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
