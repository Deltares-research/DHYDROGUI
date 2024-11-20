using DelftTools.Utils.Aop;

namespace DeltaShell.Plugins.FMSuite.FlowFM.Gui.Editors
{
    [Entity]
    public class HarmonicConditionsDialogViewModel
    {
        private double amplitudeCorrection = 1;

        public bool CorrectionsEnabled { get; set; }

        public double Frequency { get; set; }
        public double Amplitude { get; set; }
        public double Phase { get; set; }

        public double AmplitudeCorrection
        {
            get
            {
                return amplitudeCorrection;
            }
            set
            {
                amplitudeCorrection = value;
            }
        }

        public double PhaseCorrection { get; set; }
    }
}