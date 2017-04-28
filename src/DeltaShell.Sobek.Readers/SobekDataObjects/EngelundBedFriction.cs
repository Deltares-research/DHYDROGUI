namespace DeltaShell.Sobek.Readers.SobekDataObjects
{
    public class EngelundBedFriction
    {
        private double alpha1;
        private double alpha2;
        private double as11;
        private double as12;
        private double as21;
        private double as22;
        private double as31;
        private double as32;
        private double deltaD;
        private double rhoAir;
        private double sigma1;
        private double sigma2;

        public double As32
        {
            get { return as32; }
            set { as32 = value; }
        }

        public double As31
        {
            get { return as31; }
            set { as31 = value; }
        }

        public double As22
        {
            get { return as22; }
            set { as22 = value; }
        }

        public double As21
        {
            get { return as21; }
            set { as21 = value; }
        }

        public double As12
        {
            get { return as12; }
            set { as12 = value; }
        }

        public double As11
        {
            get { return as11; }
            set { as11 = value; }
        }

        public double Sigma2
        {
            get { return sigma2; }
            set { sigma2 = value; }
        }

        public double Sigma1
        {
            get { return sigma1; }
            set { sigma1 = value; }
        }

        public double DeltaD
        {
            get { return deltaD; }
            set { deltaD = value; }
        }

        public double Alpha1
        {
            get { return alpha1; }
            set { alpha1 = value; }
        }

        public double Alpha2
        {
            get { return alpha2; }
            set { alpha2 = value; }
        }

        public double RhoAir
        {
            get { return rhoAir; }
            set { rhoAir = value; }
        }
    }
}