using System;
using DelftTools.Shell.Core;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Gui;
using log4net;

namespace DeltaShell.Plugins.NetworkEditor.Tests.Forms.CompositeStructureView
{
    public class SimpleSelectionContainer:ISelectionContainer
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ISelectionContainer));
        private object selection;
        private int count;
        public object Selection
        {
            get { return selection; }
            set
            {
                //this is needed otherwise we get an event loop in CompositeStructureView for example
                if (selection == value) 
                    return;

                selection = value;

                if (SelectionChanged != null)
                {
                    if (Logging)
                    {
                        if (value != null)
                        {
                            log.DebugFormat("{0} Sending changed event for {1}", count++,value.ToString());        
                        }
                        else
                        {
                            log.DebugFormat("{0} Sending changed event for null", count++);        
                        }
                    }
                    
                    SelectionChanged(this, new SelectedItemChangedEventArgs(value));
                }
            }
        }

        public bool Logging
        {
            get; set;
        }

        public event EventHandler<SelectedItemChangedEventArgs> SelectionChanged;

        public IModel SelectedModel
        {
            get; set;
        }

        public IProjectItem SelectedProjectItem
        {
            get; set;
        }
    }
}