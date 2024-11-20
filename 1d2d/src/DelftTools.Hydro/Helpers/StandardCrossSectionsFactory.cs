using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Hydro.CrossSections;

namespace DelftTools.Hydro.Helpers
{
    /// <summary>
    /// Provides factory methods to created stand Cs's like like eggshape,cunette etc
    /// </summary>
    public static class StandardCrossSectionsFactory
    {
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromTrapezium(double slope, double bedWidth, double topWidth, bool isClosed = false)
        {
            var height = (topWidth - bedWidth)/(2*slope);
            height = height == 0 ? 0.000001 : height;
            var hfsw = new List<HeightFlowStorageWidth>
                           {
                               new HeightFlowStorageWidth(0, bedWidth, bedWidth),
                               new HeightFlowStorageWidth(height, topWidth, topWidth)
                           };

            if(isClosed)
            {
                //add a closing row
                hfsw.Add(new HeightFlowStorageWidth(height + 0.000001, 0.0, 0));
            }

            
            return new CrossSectionDefinitionZW().SetWithHfswData(hfsw);
        }
        /// <summary>
        /// Creates a profile from width, height and archHeight
        /// based on ProfileArch from ProfileFormules from Sobek UI
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="archHeight"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromArch(double width, double height, double archHeight)
        {
            if ((width < 1.0e-6) || (height < 1.0e-6) || (archHeight < 1.0e-6))
            {
                return GetTabulatedCrossSectionFromRectangle(width, height);
            }
            if (archHeight >= height)
            {
                //round archHeight down
                archHeight = height - 1.0e-6;
            }
            double step = (archHeight / 2 - Math.Sin(((90 - 36) * 3.141593 / 180)) * archHeight / 2);
            int nbSteps = (int) ((archHeight/step) + 1);
            nbSteps = nbSteps + 2;

            
            var hfsw = new List<HeightFlowStorageWidth>();
            hfsw.Add(new HeightFlowStorageWidth(0, width, width));

            for (int i = 0; i < nbSteps - 2; i++)
            {
                double currentHeight = height - archHeight + i*step;
                double widthAtHeight = 2* Math.Sqrt((((((-1 * 
                                                         ((currentHeight - (height - archHeight))*
                                                          (currentHeight - (height - archHeight))))/(archHeight*archHeight)) + 1)*width*width)/4));
                hfsw.Add(new HeightFlowStorageWidth(currentHeight, widthAtHeight, widthAtHeight));
            }

            double widthAtTop = 2*
                                Math.Sqrt(((((-1*(archHeight*archHeight))/(archHeight*archHeight)) + 1)*width*width)/4);
            hfsw.Add(new HeightFlowStorageWidth(height, widthAtTop, widthAtTop));

            return new CrossSectionDefinitionZW().SetWithHfswData(hfsw);
        }
        /// <summary>
        /// Creates a profile from width, height and archHeight
        /// based on ProfileArch from ProfileFormules from Sobek UI
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="archHeight"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromUShape(double width, double height, double archHeight)
        {
            if ((width < 1.0e-6) || (height < 1.0e-6) || (archHeight < 1.0e-6))
            {
                return GetTabulatedCrossSectionFromRectangle(width, height);
            }
            if (archHeight >= height)
            {
                //round archHeight down
                archHeight = height - 1.0e-6;
            }
            double step = (archHeight / 2 - Math.Sin(((90 - 36) * 3.141593 / 180)) * archHeight / 2);
            int nbSteps = (int)((archHeight / step) + 1);
            nbSteps = nbSteps + 2;


            var hfsw = new List<HeightFlowStorageWidth>();
            hfsw.Add(new HeightFlowStorageWidth(height, width, width));
            
            for (int i = 0; i < nbSteps - 2; i++)
            {
                double currentHeight = height - archHeight + i * step;
                double widthAtHeight = 2 * Math.Sqrt((((((-1 *
                                                          ((currentHeight - (height - archHeight)) *
                                                           (currentHeight - (height - archHeight)))) / (archHeight * archHeight)) + 1) * width * width) / 4));
                hfsw.Add(new HeightFlowStorageWidth(-1*currentHeight + height, widthAtHeight, widthAtHeight));
            }

            double widthAtTop = 2 *
                                Math.Sqrt(((((-1 * (archHeight * archHeight)) / (archHeight * archHeight)) + 1) * width * width) / 4);
            hfsw.Add(new HeightFlowStorageWidth(0, widthAtTop, widthAtTop));

            return new CrossSectionDefinitionZW().SetWithHfswData(hfsw,true);
        }

        /// <summary>
        /// Fills 2 arrays with z(height) and y(width) of right top ellipse: top -> down
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="steps"></param>
        public static void CalculateEllipseCoord(double width, double height, List<double> y, List<double> z, int steps)
        {
            double p = Math.PI / 180;
            double halfWidth = width / 2;
            for (double theta = 0; theta < 90.0; theta += (90.0 / steps))
            {
                double newY = Math.Sin(theta * p) * halfWidth;
                z.Add(Math.Sqrt((1 - ((newY * newY) / (width * width / 4))) * (height * height / 4)));
                y.Add(newY);
            }
        }

        /// <summary>
        /// *****************************************************************
        /// *****************************************************************
        /// **                                                             **
        /// **      PROGRAMMA  D I W A                                     **
        /// **                                                             **
        /// **      EIGENDOM:  L A N D I N R I C H T I N G S D I E N S T   **
        /// **                                                             **
        /// **      PROGRAMMERING EN ANALYSE :   T. GELOK; J. BOUWKNEGT    **
        /// **      begindatum onderhoud: 09-11-1995                       **
        /// **                       ---------------                       **
        /// **       SUBROUTINE :   |  K O O R D E  |                      **
        /// **                       ---------------                       **
        /// *****************************************************************
        /// **                                                             **
        /// **    Functie Koorde berekent een horizontale koorde ter hoogte**
        /// **    Y (tov middelpunt) in een cirkel met straal R            **
        /// **                                                             **
        /// **    AANGEROEPEN IN: NATOMP_UF                                **
        /// *****************************************************************
        /// *****************************************************************
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static double Koorde(double radius, double y)
        {
            double d = (radius - y)*(radius + y);
            if (d < 0)
            {
                return -1;
            }
            return 2*Math.Sqrt(d);
        }

        /// <summary>
        /// Creates a profile from width and height
        /// based on ProfileCunette from ProfileFormules from Sobek UI
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromCunette(double width, double height, bool addClosingTopRow = true)
        {
            if ((width < 1.0e-6) || (height < 1.0e-6))
            {
                return GetTabulatedCrossSectionFromRectangle(width, height);
            }

            const double epsm = 0.0001;
            double b13 = 0.13*width;

            double sngz = (height/2 - Math.Sin((90 - 18)*3.141593/180)*height/2);
            int lngdivision = (int)Math.Round((height / sngz) + 1);

            List<double> y = new List<double>();
            List<double> z = new List<double>();
            double step = height/lngdivision;

            bool last = false;
            for (sngz = 0; sngz < height + 0.001; sngz+=step)
            {
                if (sngz > 0.63 * width - epsm)
                {
                    sngz = 0.63*width - epsm;
                    last = true; // avoid infinite loop
                }
                z.Add(sngz);

                if (sngz <= b13)
                {
                    double r02 = 1.02*width;
                    y.Add(Koorde(r02, sngz - r02));
                }
                else
                {
                    double r01 = 0.5*width;
                    y.Add(Koorde(r01, sngz - b13));
                }
                if (last)
                {
                    break;
                }
            }
            
            IEnumerable<HeightFlowStorageWidth> heightFlowStorageWidths = y.Select((t, i)  => new HeightFlowStorageWidth(z[i], t, t));
            
            return new CrossSectionDefinitionZW().SetWithHfswData( heightFlowStorageWidths,addClosingTopRow);
        }

        /// <summary>
        /// Creates a profile from width and height
        /// based on ProfileEllipse from ProfileFormules from Sobek UI
        /// </summary>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        /// <param name="radius1"></param>
        /// <param name="radius2"></param>
        /// <param name="radius3"></param>
        /// <param name="angle"></param>
        /// <param name="angle1"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromSteelCunette(double height, double radius, double radius1, 
                                                                              double radius2, double radius3, double angle, double angle1)
        {
            try
            {
                if ((radius3 == 0) || (angle == 0))
                {
                    return ProfileCunnetteGegolfd(radius, radius1, radius2, angle, height);
                }
                return ProfileCunetteUf(radius, radius1, radius2, radius3, angle, angle1, height);
            }
            catch (Exception)
            {
                // assume ProfileCunnetteGegolfd and ProfileCunetteUf as blackbox
                return GetTabulatedCrossSectionFromRectangle(0, 0);
            }
        }

        /// <summary>
        /// DIT IS EEN NIEUWE DUIKER
        /// ONTHOUD DE KARAKTERISTIEKE AFMETINGEN IN
        /// DE VARIABELEN DIE EINDIGEN OP EEN T
        /// EN BEREKEN DE AFMETINGEN DIE NODIG ZIJN VOOR DE
        /// BEPALING VAN NATOM EN NABOV
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="radius1"></param>
        /// <param name="radius2"></param>
        /// <param name="a"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW ProfileCunnetteGegolfd(double radius, double radius1, double radius2, double a, double height)
        {
            try
            {
                /// source calculation is based on centimeters; do no change behaviour is different
                /// 
                double r = radius*100;
                double r1 = radius1*100;
                double r2 = radius2*100;
                double H = height*100;
                double z1 = r - r2;
                double Z3 = r1 - r2;
                double HALPHA, COSHA, SINHA, z2;
                double EPS = 0;

                if (a >= 120)
                {
                    HALPHA = (0.5*3.141593/180)*a;
                    COSHA = Math.Cos(HALPHA);
                    SINHA = Math.Sin(HALPHA);
                }
                else
                {
                    // GEBRUIK HOOGTE H OM ALPHA TE BEPALEN
                    z2 = r1 - H + r;
                    // VOLGENS COSINUSREGEL
                    COSHA = ((z2 - Z3)*(z2 + Z3) + z1*z1)/(2*z1*z2);
                    if (COSHA > 1)
                    {
                        return GetTabulatedCrossSectionFromRectangle(0, 0);
                    }
                    SINHA = Math.Sqrt((1 - COSHA)*(1 + COSHA));
                }

                double AB = z1*SINHA;
                double AB2 = AB + AB;
                double Z4 = Math.Sqrt((Z3 - AB)*(Z3 + AB));
                double h2 = r1*(Z3 - Z4)/Z3;
                double h1 = r1 - Z4;
                double H1S = h1 + COSHA*r2;
                double h3 = h1 - COSHA*z1;
                double HB = h3 + r;
                double HMAX = HB - EPS;

                // MUIL PROFIEL BEPAALD, DWZ
                // H1, H1S, H2, H3, HMAX, AB2, C1, C2, C3, GMIN BEREKEND
                // (MOGELIJK IN EERDERE AANROEP VAN DEZE FUNCTIE)

                double sngz = (HB/2 - Math.Sin((90 - 18)*3.141593/180)*HB/2);
                int lngdivision = (int) Math.Round((HB/sngz) + 1);

                List<double> y = new List<double>();
                List<double> z = new List<double>();
                double GKMIN;
                for (int lngX = 1; lngX <=lngdivision;lngX ++)
                {
                    sngz = (lngX - 1)*HB/lngdivision;
                    z.Add(sngz);
                    if (sngz > HMAX)
                    {
                        sngz = HMAX;
                    }

                    //  GH tussenvoeging voor geval geen grond dan kleine waarde
                    if (sngz <= h2)
                    {
                        GKMIN = sngz - r1;
                        y.Add(Koorde(r1, GKMIN));
                    }
                    else if (sngz <= H1S)
                    {
                        GKMIN = sngz - h1;
                        y.Add(Koorde(r2, GKMIN) + AB2);
                    }
                    else
                    {
                        // GK.LE.HMAX
                        GKMIN = sngz - h3;
                        y.Add(Koorde(r, GKMIN));
                    }
                }
                // Close Profile
                z.Add(HB);
                y.Add(0);

                return SetYZCentimetersToTabulatedProfile(z, y);
            }
            catch (Exception)
            {
                return GetTabulatedCrossSectionFromRectangle(1, 1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="z">Z in centimeters</param>
        /// <param name="y">Y in centimeters</param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW SetYZCentimetersToTabulatedProfile(IList<double> z, IEnumerable<double> y)
        {
            var hwwData = y.Select((t, i) =>
                                   new HeightFlowStorageWidth(z[i] * 0.01, Math.Max(0.0000, t * 0.01), Math.Max(0.0000, t * 0.01))).ToList();

            return new CrossSectionDefinitionZW().SetWithHfswData(hwwData);
        }

        /// <summary>
        /// *****************************************************************
        /// *****************************************************************
        /// **                                                             **
        /// **      PROGRAMMA  D I W A                                     **
        /// **                                                             **
        /// **      EIGENDOM:  L A N D I N R I C H T I N G S D I E N S T   **
        /// **                                                             **
        /// **      PROGRAMMERING EN ANALYSE :   T. GELOK;                 **
        /// **      begindatum onderhoud: 13-11-1995                       **
        /// **                       ---------------                       **
        /// **       SUBROUTINE :   | MuilProfile_UF |                     **
        /// **                       ---------------                       **
        /// *****************************************************************
        /// **                                                             **
        /// **     berekent natte oppervlakte's en/of omtrekken van        **
        /// **     ARMCO profielen type UF                                 **
        /// **     Zie voor verklaring berekening de bijbehorende tekening-**
        /// **     en en beschrijvingen                                    **
        /// **                                                             **
        /// *****************************************************************
        /// *****************************************************************
        /// --VC-------------------------------------------------------------------
        ///        19 juni 1987
        ///       Mei 1984, Hans Veenhof
        ///       Feb 1999  Juzer Dhondia
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="radius1"></param>
        /// <param name="radius2"></param>
        /// <param name="radius3"></param>
        /// <param name="a"></param>
        /// <param name="a1"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW ProfileCunetteUf(double radius, double radius1, double radius2, double radius3, double a, double a1, double height)
        {
            double r = radius * 100;
            double r1 = radius1 * 100;
            double r2 = radius2 * 100;
            double r3 = radius3 * 100;
            double H = height * 100;
            //--VC-------------------------------------------------------------------
            //      R,R1,R2 en R3 : de stralen van de 4 cirkels waaruit het UF profiel Is opgebouwd
            //      A en A1       : de bijbehorende hoeken van resp. R1 en R3
            //      H             : de hoogte van het betreffende profiel
            //      Bovenstaande gegevens worden geleverd door de leverancier van de profielen.

            //--VC-------------------------------------------------------------------
            //      Berekenen konstanten :
            //--HV-------------------------------------------------------------------
            double Pi = 180;
            double B = 0.5*(Pi - a);
            double B1 = B - a1;

            double COSB = Math.Cos(B*3.141593/180);
            double SINB = Math.Sin(B*3.141593/180);
            double COSB1 = Math.Cos(B1*3.141593/180);
            double SINB1 = Math.Sin(B1*3.141593/180);
            //--VC-------------------------------------------------------------------
            //      Berekenen coordinaten (voor zover mogelijk) :
            //--HV-------------------------------------------------------------------
            double YG = 0;

            double YF = r;

            double YE = r*SINB;

            double XM = (r - r3)*COSB;
            double YM = (r - r3)*SINB;

            double YC = YM + r3*SINB1;

            double XL = XM + (r3 - r2)*COSB1;
            double YL = YM + (r3 - r2)*SINB1;

            double XD = 0;
            double YD = r1 + r - H;

            double YP = YM;

            //--VC-------------------------------------------------------------------
            //      Berekenen in 3-hoek DPM :
            //--HV-------------------------------------------------------------------
            double LD = Math.Abs(XM);
            double LM = Math.Abs(YD - YP);
            double LP = Math.Sqrt(LD*LD + LM*LM);
            double C1 = Math.Atan(LD/LM);

            //--VC-------------------------------------------------------------------
            //      Berekenen in 3-hoek DML :
            //--HV-------------------------------------------------------------------
            double LD1 = Math.Sqrt((XL - XM) * (XL - XM) + (YL - YM) * (YL - YM));
            double LM1 = Math.Sqrt((XL - XD) * (XL - XD) + (YL - YD) * (YL - YD));
            double HULP = (LP*LP + LM1*LM1 - LD1*LD1)/(2*LP*LM1);
            double C3 = Math.Acos(HULP);
            double C2 = C3 - C1;

            //--VC-------------------------------------------------------------------
            //      Berekenen hoek A3 en A2 :
            //--HV-------------------------------------------------------------------
            double A3 = 2*C2;

            //--VC-------------------------------------------------------------------
            //      Coordinaten B nu bekend :
            //--HV-------------------------------------------------------------------
            double YB = YD - r1*Math.Cos(0.5*A3);

            //--VC-------------------------------------------------------------------
            //      Hoogten waarop kromte-straal wijzigt nu bekend :
            //--HV-------------------------------------------------------------------
            double HA = r - H;
            double HI = YB;
            double HN = YC;
            double HO = YE;
            double hf = YF;

            //--VC-------------------------------------------------------------------
            //      Hoogte waterstand aanpassen aan coordinatensysteem :
            //--HV-------------------------------------------------------------------

            List<double> y = new List<double>();
            List<double> z = new List<double>();

            double sngz = (H/2 - Math.Sin((90 - 18)*3.141593/180)*H/2);
            int lngdivision = (int)Math.Round((H / sngz) + 1);

            for (int lngX=1; lngX<=lngdivision; lngX++)
            {
                sngz = (lngX - 1)*H/lngdivision;
                z.Add(sngz);
                double WAT = z[lngX-1] + HA;
                if (WAT < HA)
                {
                    //--VC-------------------------------------------------------------------
                    //      Berekenen oppervlakte of omtrek :
                    //      =================================
                    //
                    //      Negatieve waterhoogte :
                    //--HV-------------------------------------------------------------------
                }

                else if (WAT == HA)
                {
                    //--VC-------------------------------------------------------------------
                    //      Waterhoogte nul :
                    //--HV-------------------------------------------------------------------
                    y.Add(0);
                }
                else if (WAT <= HI)
                {
                    //--VC-------------------------------------------------------------------
                    //      Waterhoogte onder I :
                    //--HV-------------------------------------------------------------------
                    double CIR_HOOGTE = Math.Min(HI, WAT) - YD; //!hoogte in cirkel R1 t.o.v.middelp.D
                    y.Add(Koorde(r1, CIR_HOOGTE));
                }
                else if (WAT <= HN)
                {
                    //--VC-------------------------------------------------------------------
                    //      Waterhoogte onder N :
                    //--HV-------------------------------------------------------------------
                    double CIR_HOOGTE = Math.Min(HN, WAT) - YL;// '!Hoogte in cirkel R2 t.o.v.middelp.L
                    y.Add(Koorde(r2, CIR_HOOGTE) + 2*XL);
                }
                else if (WAT <= HO)
                {
                    //--VC-------------------------------------------------------------------
                    //      Waterhoogte onder O :
                    //--HV-------------------------------------------------------------------
                    double CIR_HOOGTE = Math.Min(HO, WAT) - YM; //'!hoogte in cirkel R3 t.o.v.middelp.M
                    y.Add(Koorde(r3, CIR_HOOGTE) + 2*XM);
                }
                else
                {
                    //--VC-------------------------------------------------------------------
                    //      Waterhoogte boven O (evt. zelfs bven F) :
                    //--HV-------------------------------------------------------------------
                    double CIR_HOOGTE = Math.Min(hf, WAT) - YG;// '!hoogte in cirkel R t.o.v.G
                    y.Add(Koorde(r, CIR_HOOGTE));
                }
                // Einde subroutine MuilProfile_UF
            }

            //Close Profile
            z.Add(H);
            y.Add(0);

            return SetYZCentimetersToTabulatedProfile(z, y);
        }

        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromCircle(double diameter)
        {
            return GetTabulatedCrossSectionFromEllipse(diameter, diameter);
        }

        /// <summary>
        /// Creates a profile from width and height
        /// based on ProfileEllipse from ProfileFormules from Sobek UI
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromEllipse(double width, double height)
        {
            if ((width < 1.0e-6) || (height < 1.0e-6))
            {
                return GetTabulatedCrossSectionFromRectangle(width, height);
            }

            
            const int steps = 10;
            List<double> y = new List<double>();
            List<double> z = new List<double>();

            CalculateEllipseCoord(width, height, y, z, steps);

            //build up the data
            var hfswData = new List<HeightFlowStorageWidth>();
            // lower part
            for (int i = 0; i < steps; i++)
            {
                hfswData.Add(new HeightFlowStorageWidth(z[0] - z[i], y[i]*2, y[i]*2));
            }
            hfswData.Add(new HeightFlowStorageWidth(z[0], width, width));
            // upper part
            for (int i = steps - 1; i >= 0; i--)
            {
                hfswData.Add(new HeightFlowStorageWidth(z[0] + z[i], y[i] * 2, y[i] * 2));
            }

            return new CrossSectionDefinitionZW().SetWithHfswData(hfswData);
        }

        /// <summary>
        /// Creates a profile from width and height
        /// based on ProfileEllipse from ProfileFormules from Sobek UI
        /// if width = 0.50
        /// <-------------0.50------------->^
        ///                                 |
        ///                                 | 0.25 -+
        ///                                 |       |
        ///  --------------------------------       |
        ///                                 |       | 0.75
        ///                                 |       |
        ///                                 |       |
        ///                                 | 0.50 -+
        ///                                 | 
        ///                                 |
        ///  --------------------------------
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromEgg(double width)
        {
            double height = 1.5*width;
            if ((width < 1.0e-6) || (height < 1.0e-6))
            {
                return GetTabulatedCrossSectionFromRectangle(width, height);
            }

            const int steps = 10;
            List<double> y = new List<double>();
            List<double> z = new List<double>();

            CalculateEllipseCoord(width, width, y, z, steps);

            var hfswData = new List<HeightFlowStorageWidth>();
            // lower part
            for (int i = 0; i < steps; i++)
            {
                hfswData.Add(new HeightFlowStorageWidth(2 * z[0] - 2 * z[i], y[i] * 2, y[i] * 2));
            }
            hfswData.Add(new HeightFlowStorageWidth(2 * z[0], width, width));
            // upper part
            for (int i = steps - 1; i >= 0; i--)
            {
                hfswData.Add(new HeightFlowStorageWidth(2 * z[0] + z[i], y[i] * 2, y[i] * 2));
            }

            return new CrossSectionDefinitionZW().SetWithHfswData(hfswData);
        }

        /// <summary>
        /// Creates a profile from width and height
        /// based on ProfileEllipse from ProfileFormules from Sobek UI
        /// if width = 0.50
        /// <-------------0.50------------->^
        ///                                 |
        ///                                 | 0.50 -+
        ///                                 |       |
        ///                                 |       | 0.75
        ///                                 |       |
        ///                                 |       |
        ///  --------------------------------       |
        ///                                 | 0.25 -+
        ///                                 | 
        ///                                 |
        ///  --------------------------------
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromInvertedEgg(double width)
        {
            double height = 1.5 * width;
            if ((width < 1.0e-6) || (height < 1.0e-6))
            {
                return GetTabulatedCrossSectionFromRectangle(width, height);
            }

            const int steps = 10;
            List<double> y = new List<double>();
            List<double> z = new List<double>();

            CalculateEllipseCoord(width, width, y, z, steps);

            var hfswData = new List<HeightFlowStorageWidth>();
            // lower part
            for (int i = 0; i < steps; i++)
            {
                hfswData.Add(new HeightFlowStorageWidth(2 * z[0] - z[i], y[i] * 2, y[i] * 2));
            }
            hfswData.Add(new HeightFlowStorageWidth(2 * z[0], width, width));
            // upper part
            for (int i = steps - 1; i >= 0; i--)
            {
                hfswData.Add(new HeightFlowStorageWidth(2 * z[0] + 2 * z[i], y[i] * 2, y[i] * 2));
            }

            return new CrossSectionDefinitionZW().SetWithHfswData(hfswData);
        }

        public static CrossSectionDefinitionZW GetTabulatedCrossSectionFromRectangle(double width, double height,bool isClosed = false)
        {
            //create a single section. Reference level is not used since the crossection is defined absolute. (Ref = 0)
            var crossSection = new CrossSectionDefinitionZW();
            crossSection.SetAsRectangle(0, Math.Max(0.001,width), Math.Max(0.001,height),isClosed);
            return crossSection;
        }
    }
}