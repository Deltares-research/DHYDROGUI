using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Aop;

namespace DeltaShell.Dimr.Gui.ViewModels
{
    [Entity]
    public class DIMRConfigRibbonViewModel
    {
        public DIMRConfigRibbonViewModel()
        {
            
        }

        public DimrApiDataSet.DebugLevel CurrentLogfileLevel
        {
            get { return DimrApiDataSet.LogFileLevel; }
            set { DimrApiDataSet.LogFileLevel = value; }
        }

        public DimrApiDataSet.DebugLevel CurrentFeedbackLevel
        {
            get { return DimrApiDataSet.FeedbackLevel; }
            set { DimrApiDataSet.FeedbackLevel = value; }
        }

        public IEnumerable<DimrApiDataSet.DebugLevel> Levels
        {
            get
            {
                return Enum.GetValues(typeof(DimrApiDataSet.DebugLevel)).Cast<DimrApiDataSet.DebugLevel>(); 
            }
        }
    }
}