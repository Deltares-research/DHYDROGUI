using System;
using System.Collections.Generic;
using System.Linq;
using BasicModelInterface;
using DelftTools.Utils.Aop;

namespace DeltaShell.Dimr.Gui.ViewModels
{
    [Entity]
    public class DimrConfigRibbonViewModel
    {
        public Level CurrentLogfileLevel
        {
            get
            {
                return DimrLogging.LogFileLevel;
            }
            set
            {
                DimrLogging.LogFileLevel = value;
            }
        }

        public Level CurrentFeedbackLevel
        {
            get
            {
                return DimrLogging.FeedbackLevel;
            }
            set
            {
                DimrLogging.FeedbackLevel = value;
            }
        }

        public IEnumerable<Level> Levels
        {
            get
            {
                return Enum.GetValues(typeof(Level)).Cast<Level>();
            }
        }
    }
}