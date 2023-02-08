namespace DeltaShell.Plugins.FMSuite.FlowFM.IO.FouFile
{
    internal class RowFactory
    {
        private readonly double startTime;
        private readonly double stopTime;

        public RowFactory(double startTime, double stopTime)
        {
            this.startTime = startTime;
            this.stopTime = stopTime;
        }

        public FouFileRow WaterLevelRow(string elpValue)
        {
            var wlRow = new FouFileRow()
            {
                Var = "wl",
                Tsrts = startTime,
                Sstop = stopTime,
                Numcyc = 0,
                Knfac = 1,
                V0plu = 0,
                Layno = null,
                Elp = elpValue
            };

            return wlRow;
        }

        public FouFileRow VelocityMagnitudeRow(string elpValue)
        {
            var wlRow = new FouFileRow()
            {
                Var = "uc",
                Tsrts = startTime,
                Sstop = stopTime,
                Numcyc = 0,
                Knfac = 1,
                V0plu = 0,
                Layno = 1,
                Elp = elpValue
            };

            return wlRow;
        }

        public FouFileRow Freeboard(string elpValue)
        {
            var wlRow = new FouFileRow()
            {
                Var = "fb",
                Tsrts = startTime,
                Sstop = stopTime,
                Numcyc = 0,
                Knfac = 1,
                V0plu = 0,
                Layno = null,
                Elp = elpValue
            };

            return wlRow;
        }
        
        public FouFileRow WaterDepthOnGround(string elpValue)
        {
            var wlRow = new FouFileRow()
            {
                Var = "wdog",
                Tsrts = startTime,
                Sstop = stopTime,
                Numcyc = 0,
                Knfac = 1,
                V0plu = 0,
                Layno = null,
                Elp = elpValue
            };

            return wlRow;
        }
        
        public FouFileRow VolumeOnGround(string elpValue)
        {
            var wlRow = new FouFileRow()
            {
                Var = "vog",
                Tsrts = startTime,
                Sstop = stopTime,
                Numcyc = 0,
                Knfac = 1,
                V0plu = 0,
                Layno = null,
                Elp = elpValue
            };

            return wlRow;
        }
    }
}