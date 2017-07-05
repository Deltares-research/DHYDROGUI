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

        public DimrApiDataSet.DimrLoggingLevel CurrentLogfileLevel
        {
            get { return DimrApiDataSet.LogFileLevel; }
            set { DimrApiDataSet.LogFileLevel = value; }
        }

        public DimrApiDataSet.DimrLoggingLevel CurrentFeedbackLevel
        {
            get { return DimrApiDataSet.FeedbackLevel; }
            set { DimrApiDataSet.FeedbackLevel = value; }
        }

        public IEnumerable<DimrApiDataSet.DimrLoggingLevel> Levels
        {
            get
            {
                return Enum.GetValues(typeof(DimrApiDataSet.DimrLoggingLevel)).Cast<DimrApiDataSet.DimrLoggingLevel>(); 
            }
        }
    }
}