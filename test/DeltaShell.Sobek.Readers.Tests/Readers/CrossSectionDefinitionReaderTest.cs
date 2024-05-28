using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.TestUtils;
using DelftTools.Utils.Reflection;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class CrossSectionDefinitionReaderTest
    {
        private const double Delta = 1e-5;

        [Test]
        public void ReadingCorruptDefinitionShouldNotThrow()
        {
            const string inValidDefinition = "invalid definition";
            SobekCrossSectionDefinition definition = null;
            var expectedLogMessage = string.Format("Could not read cross-section definition with specification: \"{0}\"", inValidDefinition);
            Action readDefinitionAction = delegate
                                              {
                                                  definition = new CrossSectionDefinitionReader().GetCrossSectionDefinition(inValidDefinition);
                                              };
            TestHelper.AssertLogMessageIsGenerated(readDefinitionAction, expectedLogMessage);
            Assert.IsNull(definition);
        }

        [Test]
        public void SplitRecords()
        {
            string source =
                @"CRDS id 'Round 500 mm' nm 'Round 500 mm' ty 4 bl 0 rd  .25  crds" + Environment.NewLine +
                @"CRDS id '3' nm 'r_Rectangle 1.4,2.4' ty 0 wm 1.4 w1 0 w2 0 lt lw" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 1.4 1.4 <" + Environment.NewLine +
                @"2.4 1.4 1.4 <" + Environment.NewLine +
                @"2.401 0.001 0.001 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0  1.4  1.4 <" + Environment.NewLine +
                @"2.4  1.4  1.4 <" + Environment.NewLine +
                @"2.4001  .0001  .0001 <" + Environment.NewLine +
                @"tble  crds" + Environment.NewLine +
                @"CRDS  id  '4'  nm  'Estuary'  ty  0  wm  3000  w1  0  w2  0  lt lw" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 2000 2000 <" + Environment.NewLine +
                @"14 3000 3000 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"dk 0 dc 0 db 0 df 0 dt 0 crds";

            // ty 4 round supported for culverts
            // ty 0 tabulated rectangle
            // ty 0 tabulated rectangle

            var crossSectionDefinitions = new CrossSectionDefinitionReader().Parse(source).ToArray();
            Assert.AreEqual(3, crossSectionDefinitions.Count());
            var first = crossSectionDefinitions[1];
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, first.Type);
            Assert.AreEqual("3", first.ID);
            Assert.AreEqual("r_Rectangle 1.4,2.4", first.Name);
            var second = crossSectionDefinitions[2];
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, second.Type);
            Assert.AreEqual("4", second.ID);
            Assert.AreEqual("Estuary", second.Name);
        }

        [Test]
        public void RecordWithTagsInNameAndId()
        {
            string source = @"CRDS id 'CRDS and crds' nm 'with both CRDS and crds' ty 4 bl 0 rd  .25  crds";
            var definition = new CrossSectionDefinitionReader().Parse(source).FirstOrDefault();


            Assert.IsNotNull(definition);
            Assert.AreEqual("with both CRDS and crds", definition.Name);
        }

        [Test]
        public void ParseYZTable()
        {
            string source = 
                @"CRDS id '4' nm 'CS33' ty 10 st 0 lt sw 0 0 gl 0 gu 0 lt yz" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 <" + Environment.NewLine +
                @"10 3 <" + Environment.NewLine +
                @"100 0 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"crds";
            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Yztable, crossSection.Type);
            Assert.AreEqual(3, crossSection.YZ.Count);
            Assert.AreEqual(0.0, crossSection.YZ[0].X, 1.0e-6);
            Assert.AreEqual(0.0, crossSection.YZ[0].Y, 1.0e-6);
            Assert.AreEqual(10.0, crossSection.YZ[1].X, 1.0e-6);
            Assert.AreEqual(3.0, crossSection.YZ[1].Y, 1.0e-6);
            Assert.AreEqual(100.0, crossSection.YZ[2].X, 1.0e-6);
            Assert.AreEqual(0.0, crossSection.YZ[2].Y, 1.0e-6);
        }

        [Test]
        public void ParseTabulatedTable()
        {
            string source =
                @"CRDS id '36' nm 'r_Rect01' ty 0 wm 1.5 w1 0.7 w2 0.3 gl 1 gu 1  lt lw" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 2.5 2.5 <" + Environment.NewLine +
                @"3.5 2.5 2.5 <" + Environment.NewLine +
                @"3.5001 0.0001 0.0001 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"crds";
            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);

            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);
            Assert.AreEqual(1.5, crossSection.MainChannelWidth, 1.0e-6);
            Assert.AreEqual(0.7, crossSection.FloodPlain1Width, 1.0e-6);
            Assert.AreEqual(0.3, crossSection.FloodPlain2Width, 1.0e-6);
            Assert.AreEqual(1.0, crossSection.GroundLayerDepth, 1.0e-6);
            Assert.IsTrue(crossSection.UseGroundLayer);


            Assert.AreEqual(3, crossSection.TabulatedProfile.Count);

            Assert.AreEqual(0.0, crossSection.TabulatedProfile[0].Height, 1.0e-6);
            Assert.AreEqual(2.5, crossSection.TabulatedProfile[0].TotalWidth, 1.0e-6);
            Assert.AreEqual(2.5, crossSection.TabulatedProfile[0].FlowWidth, 1.0e-6);

            Assert.AreEqual(3.5, crossSection.TabulatedProfile[1].Height, 1.0e-6);
            Assert.AreEqual(2.5, crossSection.TabulatedProfile[1].TotalWidth, 1.0e-6);
            Assert.AreEqual(2.5, crossSection.TabulatedProfile[1].FlowWidth, 1.0e-6);

            Assert.AreEqual(3.5001, crossSection.TabulatedProfile[2].Height, 1.0e-6);
            Assert.AreEqual(0.0001, crossSection.TabulatedProfile[2].TotalWidth, 1.0e-6);
            Assert.AreEqual(0.0001, crossSection.TabulatedProfile[2].FlowWidth, 1.0e-6);
        }

        [Test]
        public void ParseSummerdike()
        {
            //Code from Sobek Help
            string source =
                @"CRDS id 'Crdef1' nm 'Tabel1' ty 0 wm 86.23 w1 0 w2 0 sw 0 lt lw" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"-2.55 16.80 16.80 <" + Environment.NewLine +
                @"-1.00 30.13 30.13 <" + Environment.NewLine +
                @"-0.65 30.13 30.13 <" + Environment.NewLine +
                @" 0.00 86.23 86.23 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"dk 1 dc 2.2 db 3.3 df 4.4 dt 5.5" + Environment.NewLine +
                @"gl 0.5 gu 0" + Environment.NewLine +
                @"crds";

            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);

            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);

            //summerdike data
            Assert.AreEqual(true, crossSection.SummerDikeActive);
            Assert.AreEqual(2.2, crossSection.CrestLevel);
            Assert.AreEqual(3.3, crossSection.FloodPlainLevel);
            Assert.AreEqual(4.4, crossSection.FlowArea);
            Assert.AreEqual(5.5, crossSection.TotalArea);

        }

        [Test]
        public void ParseRound()
        {
            const string source = @"CRDS id 'Round 100 mm' nm 'Round 100 mm' ty 4 bl 0.3 rd  .05 crds"; 
            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.ClosedCircle, crossSection.Type);
            Assert.AreEqual(0.3, crossSection.BedLevel, 1.0e-6);
            Assert.AreEqual(0.05, crossSection.Radius, 1.0e-6);
        }

        [Test]
        public void ParseEgg()
        {
            const string source = @"CRDS id 'Egg Shape .95 m' nm 'Egg Shape .95 m' ty 6 bl 0.1 bo .95 crds";
            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.EggShapedWidth, crossSection.Type);
            Assert.AreEqual(0.1, crossSection.BedLevel, 1.0e-6);
            Assert.AreEqual(0.95, crossSection.Width, 1.0e-6);
        }

        [Test]
        public void ParseTrapezoidal()
        {
            const string source = @"CRDS id '21' nm 'TrapProf01' ty 1 bl 0 bw 6 bs 1 aw 16 sw 0  gl 0 gu 0 crds";
            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Trapezoidal, crossSection.Type);
            Assert.AreEqual(6.0, crossSection.BedWidth, 1.0e-6);
            Assert.AreEqual(1.0, crossSection.Slope, 1.0e-6);
            Assert.AreEqual(16.0, crossSection.MaxFlowWidth, 1.0e-6);
        }

        [Test]
        public void ParseArch()
        {
            string source = @"CRDS id '13' nm 'a_ArchProf01' ty 0 wm 4 w1 0 w2 0 gl 0 gu 0 lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 4 4 <" + Environment.NewLine +
                            @"4 4 4 <" + Environment.NewLine +
                            @"4.190983 3.981721 3.981721 <" + Environment.NewLine +
                            @"4.381966 3.926373 3.926373 <" + Environment.NewLine +
                            @"4.572948 3.832352 3.832352 <" + Environment.NewLine +
                            @"4.763931 3.696706 3.696706 <" + Environment.NewLine +
                            @"4.954914 3.51462 3.51462 <" + Environment.NewLine +
                            @"5.145897 3.278366 3.278366 <" + Environment.NewLine +
                            @"5.33688 2.975065 2.975065 <" + Environment.NewLine +
                            @"5.527863 2.58119 2.58119 <" + Environment.NewLine +
                            @"5.718845 2.045063 2.045063 <" + Environment.NewLine +
                            @"5.909828 1.187529 1.187529 <" + Environment.NewLine +
                            @"6 0 0 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);
            Assert.AreEqual(4.0, crossSection.MainChannelWidth, 1.0e-6);

            // arch parameters are optionally specified in the input data
            string source2 = @"CRDS id '14' nm 'a_ArchProf02' ty 0 wm 3 aw 3 ah 4 aa 2 w1 0 w2 0 gl 0 gu 0 lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 3 3 <" + Environment.NewLine +
                            @"2 3 3 <" + Environment.NewLine +
                            @"2.190983 2.986291 2.986291 <" + Environment.NewLine +
                            @"2.381966 2.94478 2.94478 <" + Environment.NewLine +
                            @"2.572948 2.874264 2.874264 <" + Environment.NewLine +
                            @"2.763931 2.77253 2.77253 <" + Environment.NewLine +
                            @"2.954914 2.635965 2.635965 <" + Environment.NewLine +
                            @"3.145897 2.458774 2.458774 <" + Environment.NewLine +
                            @"3.33688 2.231299 2.231299 <" + Environment.NewLine +
                            @"3.527863 1.935893 1.935893 <" + Environment.NewLine +
                            @"3.718845 1.533797 1.533797 <" + Environment.NewLine +
                            @"3.909828 0.8906468 0.8906468 <" + Environment.NewLine +
                            @"4 0 0 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds"; 

            var crossSection2 = reader.GetCrossSectionDefinition(source2);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection2.Type);
            Assert.AreEqual(3.0, crossSection2.MainChannelWidth, 1.0e-6);
            Assert.AreEqual(3.0, crossSection2.Width, 1.0e-6);
            Assert.AreEqual(4.0, crossSection2.Height, 1.0e-6);
            Assert.AreEqual(2.0, crossSection2.ArcHeight, 1.0e-6);
        }

        [Test]
        public void ParseCunette()
        {
            string source = @"CRDS id '15' nm 'c_CunetteProf01' ty 0 wm 9 w1 0 w2 0 gl 0 gu 0 lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 0 0 <" + Environment.NewLine +
                            @"0.1359524 3.148081 3.148081 <" + Environment.NewLine +
                            @"0.2719048 4.435421 4.435421 <" + Environment.NewLine +
                            @"0.4078571 5.411806 5.411806 <" + Environment.NewLine +
                            @"0.5438095 6.225308 6.225308 <" + Environment.NewLine +
                            @"0.6797619 6.933499 6.933499 <" + Environment.NewLine +
                            @"0.8157142 7.566009 7.566009 <" + Environment.NewLine +
                            @"0.9516666 8.140498 8.140498 <" + Environment.NewLine +
                            @"1.087619 8.66851 8.66851 <" + Environment.NewLine +
                            @"1.223571 8.999362 8.999362 <" + Environment.NewLine +
                            @"1.359524 8.992015 8.992015 <" + Environment.NewLine +
                            @"1.495476 8.976428 8.976428 <" + Environment.NewLine +
                            @"1.631428 8.95256 8.95256 <" + Environment.NewLine +
                            @"1.767381 8.920344 8.920344 <" + Environment.NewLine +
                            @"1.903333 8.87969 8.87969 <" + Environment.NewLine +
                            @"2.039285 8.83048 8.83048 <" + Environment.NewLine +
                            @"2.175238 8.772571 8.772571 <" + Environment.NewLine +
                            @"2.31119 8.705788 8.705788 <" + Environment.NewLine +
                            @"2.447143 8.629926 8.629926 <" + Environment.NewLine +
                            @"2.583095 8.544744 8.544744 <" + Environment.NewLine +
                            @"2.719048 8.449959 8.449959 <" + Environment.NewLine +
                            @"2.855 8.345244 8.345244 <" + Environment.NewLine +
                            @"2.990953 8.23022 8.23022 <" + Environment.NewLine +
                            @"3.126905 8.104448 8.104448 <" + Environment.NewLine +
                            @"3.262858 7.967421 7.967421 <" + Environment.NewLine +
                            @"3.39881 7.818543 7.818543 <" + Environment.NewLine +
                            @"3.534763 7.657127 7.657127 <" + Environment.NewLine +
                            @"3.670715 7.482359 7.482359 <" + Environment.NewLine +
                            @"3.806668 7.29328 7.29328 <" + Environment.NewLine +
                            @"3.94262 7.088746 7.088746 <" + Environment.NewLine +
                            @"4.078572 6.867374 6.867374 <" + Environment.NewLine +
                            @"4.214525 6.627479 6.627479 <" + Environment.NewLine +
                            @"4.350477 6.366966 6.366966 <" + Environment.NewLine +
                            @"4.48643 6.083188 6.083188 <" + Environment.NewLine +
                            @"4.622382 5.772714 5.772714 <" + Environment.NewLine +
                            @"4.758335 5.430968 5.430968 <" + Environment.NewLine +
                            @"4.894287 5.051608 5.051608 <" + Environment.NewLine +
                            @"5.03024 4.625387 4.625387 <" + Environment.NewLine +
                            @"5.166192 4.137849 4.137849 <" + Environment.NewLine +
                            @"5.302145 3.56392 3.56392 <" + Environment.NewLine +
                            @"5.438097 2.85191 2.85191 <" + Environment.NewLine +
                            @"5.574049 1.848619 1.848619 <" + Environment.NewLine +
                            @"5.6699 6.001751E-02 6.001751E-02 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);
            Assert.AreEqual(9.0, crossSection.MainChannelWidth, 1.0e-6);

            // cunette parameters are optionally specified in the input data
            string source2 = @"CRDS id '16' nm 'c_CunetteProf02' ty 0 wm 6 cw 6 w1 0 w2 0 gl 0 gu 0 lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 0 0 <" + Environment.NewLine +
                            @"9.047619E-02 2.096895 2.096895 <" + Environment.NewLine +
                            @"0.1809524 2.954396 2.954396 <" + Environment.NewLine +
                            @"0.2714286 3.604781 3.604781 <" + Environment.NewLine +
                            @"0.3619048 4.14668 4.14668 <" + Environment.NewLine +
                            @"0.452381 4.618439 4.618439 <" + Environment.NewLine +
                            @"0.5428572 5.039793 5.039793 <" + Environment.NewLine +
                            @"0.6333334 5.422505 5.422505 <" + Environment.NewLine +
                            @"0.7238096 5.774263 5.774263 <" + Environment.NewLine +
                            @"0.8142858 5.999608 5.999608 <" + Environment.NewLine +
                            @"0.904762 5.994809 5.994809 <" + Environment.NewLine +
                            @"0.9952382 5.984538 5.984538 <" + Environment.NewLine +
                            @"1.085714 5.968765 5.968765 <" + Environment.NewLine +
                            @"1.176191 5.947447 5.947447 <" + Environment.NewLine +
                            @"1.266667 5.920526 5.920526 <" + Environment.NewLine +
                            @"1.357143 5.887922 5.887922 <" + Environment.NewLine +
                            @"1.447619 5.849542 5.849542 <" + Environment.NewLine +
                            @"1.538095 5.805271 5.805271 <" + Environment.NewLine +
                            @"1.628571 5.754972 5.754972 <" + Environment.NewLine +
                            @"1.719048 5.698487 5.698487 <" + Environment.NewLine +
                            @"1.809524 5.63563 5.63563 <" + Environment.NewLine +
                            @"1.9 5.566184 5.566184 <" + Environment.NewLine +
                            @"1.990476 5.489899 5.489899 <" + Environment.NewLine +
                            @"2.080952 5.406487 5.406487 <" + Environment.NewLine +
                            @"2.171428 5.31561 5.31561 <" + Environment.NewLine +
                            @"2.261905 5.21688 5.21688 <" + Environment.NewLine +
                            @"2.352381 5.109841 5.109841 <" + Environment.NewLine +
                            @"2.442857 4.993958 4.993958 <" + Environment.NewLine +
                            @"2.533334 4.868602 4.868602 <" + Environment.NewLine +
                            @"2.62381 4.733018 4.733018 <" + Environment.NewLine +
                            @"2.714286 4.5863 4.5863 <" + Environment.NewLine +
                            @"2.804762 4.427341 4.427341 <" + Environment.NewLine +
                            @"2.895239 4.254769 4.254769 <" + Environment.NewLine +
                            @"2.985715 4.066852 4.066852 <" + Environment.NewLine +
                            @"3.076191 3.86135 3.86135 <" + Environment.NewLine +
                            @"3.166667 3.635282 3.635282 <" + Environment.NewLine +
                            @"3.257144 3.384529 3.384529 <" + Environment.NewLine +
                            @"3.34762 3.103113 3.103113 <" + Environment.NewLine +
                            @"3.438096 2.781744 2.781744 <" + Environment.NewLine +
                            @"3.528573 2.404453 2.404453 <" + Environment.NewLine +
                            @"3.619049 1.938867 1.938867 <" + Environment.NewLine +
                            @"3.709525 1.292877 1.292877 <" + Environment.NewLine +
                            @"3.7799 4.897125E-02 4.897125E-02 <" + Environment.NewLine +
                            @"3.8 0 0 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var crossSection2 = reader.GetCrossSectionDefinition(source2);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection2.Type);
            Assert.AreEqual(6.0, crossSection2.MainChannelWidth, 1.0e-6);
            Assert.AreEqual(6.0, crossSection2.Width, 1.0e-6);
        }

        [Test]
        public void ParseSteelCunette()
        {

            string source = @"CRDS id '18' nm 's_SteelProf02' ty 0 wm 3.699697 w1 0 w2 0  gl 0 gu 0 lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 0 0 <" + Environment.NewLine +
                            @"7.361912E-02 1.41574 1.41574 <" + Environment.NewLine +
                            @"0.1472382 1.984171 1.984171 <" + Environment.NewLine +
                            @"0.2208574 2.332484 2.332484 <" + Environment.NewLine +
                            @"0.2944765 2.582286 2.582286 <" + Environment.NewLine +
                            @"0.3680956 2.780389 2.780389 <" + Environment.NewLine +
                            @"0.4417147 2.944294 2.944294 <" + Environment.NewLine +
                            @"0.5153339 3.082858 3.082858 <" + Environment.NewLine +
                            @"0.588953 3.201299 3.201299 <" + Environment.NewLine +
                            @"0.6625721 3.302987 3.302987 <" + Environment.NewLine +
                            @"0.7361913 3.390236 3.390236 <" + Environment.NewLine +
                            @"0.8098103 3.464701 3.464701 <" + Environment.NewLine +
                            @"0.8834295 3.527598 3.527598 <" + Environment.NewLine +
                            @"0.9570486 3.579836 3.579836 <" + Environment.NewLine +
                            @"1.030668 3.622102 3.622102 <" + Environment.NewLine +
                            @"1.104287 3.654904 3.654904 <" + Environment.NewLine +
                            @"1.177906 3.678616 3.678616 <" + Environment.NewLine +
                            @"1.251525 3.693495 3.693495 <" + Environment.NewLine +
                            @"1.325144 3.699697 3.699697 <" + Environment.NewLine +
                            @"1.398763 3.697288 3.697288 <" + Environment.NewLine +
                            @"1.472383 3.686241 3.686241 <" + Environment.NewLine +
                            @"1.546002 3.666442 3.666442 <" + Environment.NewLine +
                            @"1.619621 3.638325 3.638325 <" + Environment.NewLine +
                            @"1.69324 3.60381 3.60381 <" + Environment.NewLine +
                            @"1.766859 3.562881 3.562881 <" + Environment.NewLine +
                            @"1.840478 3.515314 3.515314 <" + Environment.NewLine +
                            @"1.914097 3.460837 3.460837 <" + Environment.NewLine +
                            @"1.987716 3.399116 3.399116 <" + Environment.NewLine +
                            @"2.061336 3.329749 3.329749 <" + Environment.NewLine +
                            @"2.134954 3.252248 3.252248 <" + Environment.NewLine +
                            @"2.208574 3.166013 3.166013 <" + Environment.NewLine +
                            @"2.282193 3.07031 3.07031 <" + Environment.NewLine +
                            @"2.355812 2.964222 2.964222 <" + Environment.NewLine +
                            @"2.429431 2.846588 2.846588 <" + Environment.NewLine +
                            @"2.50305 2.715908 2.715908 <" + Environment.NewLine +
                            @"2.576669 2.570193 2.570193 <" + Environment.NewLine +
                            @"2.650288 2.406713 2.406713 <" + Environment.NewLine +
                            @"2.723907 2.22155 2.22155 <" + Environment.NewLine +
                            @"2.797527 2.008718 2.008718 <" + Environment.NewLine +
                            @"2.871146 1.758194 1.758194 <" + Environment.NewLine +
                            @"2.944765 1.450582 1.450582 <" + Environment.NewLine +
                            @"3.018384 1.03623 1.03623 <" + Environment.NewLine +
                            @"3.092003 0 0 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);
            Assert.AreEqual(3.699697, crossSection.MainChannelWidth, 1.0e-6);

            // steel cunette parameters are optionally specified in the input data
            string source2 =
                @"CRDS id '31' nm 's_SteelCunProf02' ty 0 wm 3.699697 sh 3.1 sr 1.86 sr1 3.44 sr2 1.26 sr3 0 sa 159 sa1 0 w1 0 w2 0 gl 0 gu 0 lt lw" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 0 0 <" + Environment.NewLine +
                @"7.361912E-02 1.41574 1.41574 <" + Environment.NewLine +
                @"0.1472382 1.984171 1.984171 <" + Environment.NewLine +
                @"0.2208574 2.332484 2.332484 <" + Environment.NewLine +
                @"0.2944765 2.582286 2.582286 <" + Environment.NewLine +
                @"0.3680956 2.780389 2.780389 <" + Environment.NewLine +
                @"0.4417147 2.944294 2.944294 <" + Environment.NewLine +
                @"0.5153339 3.082858 3.082858 <" + Environment.NewLine +
                @"0.588953 3.201299 3.201299 <" + Environment.NewLine +
                @"0.6625721 3.302987 3.302987 <" + Environment.NewLine +
                @"0.7361913 3.390236 3.390236 <" + Environment.NewLine +
                @"0.8098103 3.464701 3.464701 <" + Environment.NewLine +
                @"0.8834295 3.527598 3.527598 <" + Environment.NewLine +
                @"0.9570486 3.579836 3.579836 <" + Environment.NewLine +
                @"1.030668 3.622102 3.622102 <" + Environment.NewLine +
                @"1.104287 3.654904 3.654904 <" + Environment.NewLine +
                @"1.177906 3.678616 3.678616 <" + Environment.NewLine +
                @"1.251525 3.693495 3.693495 <" + Environment.NewLine +
                @"1.325144 3.699697 3.699697 <" + Environment.NewLine +
                @"1.398763 3.697288 3.697288 <" + Environment.NewLine +
                @"1.472383 3.686241 3.686241 <" + Environment.NewLine +
                @"1.546002 3.666442 3.666442 <" + Environment.NewLine +
                @"1.619621 3.638325 3.638325 <" + Environment.NewLine +
                @"1.69324 3.60381 3.60381 <" + Environment.NewLine +
                @"1.766859 3.562881 3.562881 <" + Environment.NewLine +
                @"1.840478 3.515314 3.515314 <" + Environment.NewLine +
                @"1.914097 3.460837 3.460837 <" + Environment.NewLine +
                @"1.987716 3.399116 3.399116 <" + Environment.NewLine +
                @"2.061336 3.329749 3.329749 <" + Environment.NewLine +
                @"2.134954 3.252248 3.252248 <" + Environment.NewLine +
                @"2.208574 3.166013 3.166013 <" + Environment.NewLine +
                @"2.282193 3.07031 3.07031 <" + Environment.NewLine +
                @"2.355812 2.964222 2.964222 <" + Environment.NewLine +
                @"2.429431 2.846588 2.846588 <" + Environment.NewLine +
                @"2.50305 2.715908 2.715908 <" + Environment.NewLine +
                @"2.576669 2.570193 2.570193 <" + Environment.NewLine +
                @"2.650288 2.406713 2.406713 <" + Environment.NewLine +
                @"2.723907 2.22155 2.22155 <" + Environment.NewLine +
                @"2.797527 2.008718 2.008718 <" + Environment.NewLine +
                @"2.871146 1.758194 1.758194 <" + Environment.NewLine +
                @"2.944765 1.450582 1.450582 <" + Environment.NewLine +
                @"3.018384 1.03623 1.03623 <" + Environment.NewLine +
                @"3.092003 0 0 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"crds";


            var crossSection2 = reader.GetCrossSectionDefinition(source2);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection2.Type);
            Assert.AreEqual(3.699697, crossSection2.MainChannelWidth, 1.0e-6);
            Assert.AreEqual(3.1, crossSection2.Height, 1.0e-6);
            Assert.AreEqual(1.86, crossSection2.RadiusR, 1.0e-6);
            Assert.AreEqual(3.44, crossSection2.RadiusR1, 1.0e-6);
            Assert.AreEqual(1.26, crossSection2.RadiusR2, 1.0e-6);
            Assert.AreEqual(0.0, crossSection2.RadiusR3, 1.0e-6);
            Assert.AreEqual(159.0, crossSection2.AngleA, 1.0e-6);
            Assert.AreEqual(0.0, crossSection2.AngleA1, 1.0e-6);
        }

        [Test]
        public void ParseRectangle()
        {
            string source = @"CRDS id '19' nm 'r_Width=10; Height=5' ty 0 wm 10 w1 0 w2 0 gl 0 gu 0  lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 10 10 <" + Environment.NewLine +
                            @"5 10 10 <" + Environment.NewLine +
                            @"5.0001 0.0001 0.0001 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);
            Assert.AreEqual(10.0, crossSection.MainChannelWidth, 1.0e-6);

            // rectangle parameters are optionally specified in the input data
            string source2 = @"CRDS id '20' nm 'r_Width=3; Height=3' ty 0 wm 3 rw 3 rh 3 w1 0 w2 0 gl 0 gu 0 lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 3 3 <" + Environment.NewLine +
                            @"3 3 3 <" + Environment.NewLine +
                            @"3.0001 0.0001 0.0001 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var crossSection2 = reader.GetCrossSectionDefinition(source2);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection2.Type);
            Assert.AreEqual(3.0, crossSection2.MainChannelWidth, 1.0e-6);
            Assert.AreEqual(3.0, crossSection2.Width, 1.0e-6);
            Assert.AreEqual(3.0, crossSection2.Height, 1.0e-6);
        }

        [Test]
        public void ParseElliptical()
        {
            string source = @"CRDS id '29' nm 'e_ProfElliptic01' ty 0 wm 4 w1 0 w2 0 gl 0 gu 0  lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 0 0 <" + Environment.NewLine +
                            @"3.693509E-02 0.6257379 0.6257379 <" + Environment.NewLine +
                            @"0.1468306 1.236068 1.236068 <" + Environment.NewLine +
                            @"0.3269806 1.815962 1.815962 <" + Environment.NewLine +
                            @"0.5729492 2.351141 2.351141 <" + Environment.NewLine +
                            @"0.8786798 2.828427 2.828427 <" + Environment.NewLine +
                            @"1.236645 3.236068 3.236068 <" + Environment.NewLine +
                            @"1.638029 3.564026 3.564026 <" + Environment.NewLine +
                            @"2.072949 3.804226 3.804226 <" + Environment.NewLine +
                            @"2.530697 3.950753 3.950753 <" + Environment.NewLine +
                            @"3 4 4 <" + Environment.NewLine +
                            @"3.469303 3.950753 3.950753 <" + Environment.NewLine +
                            @"3.927051 3.804226 3.804226 <" + Environment.NewLine +
                            @"4.361971 3.564026 3.564026 <" + Environment.NewLine +
                            @"4.763355 3.236068 3.236068 <" + Environment.NewLine +
                            @"5.12132 2.828427 2.828427 <" + Environment.NewLine +
                            @"5.427051 2.351141 2.351141 <" + Environment.NewLine +
                            @"5.673019 1.815962 1.815962 <" + Environment.NewLine +
                            @"5.853169 1.236068 1.236068 <" + Environment.NewLine +
                            @"5.963065 0.6257379 0.6257379 <" + Environment.NewLine +
                            @"6 0 0 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);
            Assert.AreEqual(4.0, crossSection.MainChannelWidth, 1.0e-6);

            // elliptical parameters are optionally specified in the input data
            string source2 = @"CRDS id '30' nm 'e_ProfElliptic02' ty 0 wm 2 ew 2 eh 3 w1 0 w2 0 gl 0 gu 0  lt lw" + Environment.NewLine +
                            @"TBLE" + Environment.NewLine +
                            @"0 0 0 <" + Environment.NewLine +
                            @"1.846755E-02 0.312869 0.312869 <" + Environment.NewLine +
                            @"7.341528E-02 0.6180341 0.6180341 <" + Environment.NewLine +
                            @"0.1634903 0.9079811 0.9079811 <" + Environment.NewLine +
                            @"0.2864746 1.175571 1.175571 <" + Environment.NewLine +
                            @"0.4393399 1.414214 1.414214 <" + Environment.NewLine +
                            @"0.6183223 1.618034 1.618034 <" + Environment.NewLine +
                            @"0.8190144 1.782013 1.782013 <" + Environment.NewLine +
                            @"1.036475 1.902113 1.902113 <" + Environment.NewLine +
                            @"1.265349 1.975377 1.975377 <" + Environment.NewLine +
                            @"1.5 2 2 <" + Environment.NewLine +
                            @"1.734651 1.975377 1.975377 <" + Environment.NewLine +
                            @"1.963525 1.902113 1.902113 <" + Environment.NewLine +
                            @"2.180985 1.782013 1.782013 <" + Environment.NewLine +
                            @"2.381678 1.618034 1.618034 <" + Environment.NewLine +
                            @"2.56066 1.414214 1.414214 <" + Environment.NewLine +
                            @"2.713525 1.175571 1.175571 <" + Environment.NewLine +
                            @"2.83651 0.9079811 0.9079811 <" + Environment.NewLine +
                            @"2.926585 0.6180341 0.6180341 <" + Environment.NewLine +
                            @"2.981533 0.312869 0.312869 <" + Environment.NewLine +
                            @"3 0 0 <" + Environment.NewLine +
                            @"tble" + Environment.NewLine +
                            @"crds";

            var crossSection2 = reader.GetCrossSectionDefinition(source2);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection2.Type);
            Assert.AreEqual(2.0, crossSection2.MainChannelWidth, 1.0e-6);
            Assert.AreEqual(2.0, crossSection2.Width, 1.0e-6);
            Assert.AreEqual(3.0, crossSection2.Height, 1.0e-6);
        }

        /// <summary>
        /// Issue TOOLS-2098 cross section definition not correctly parsed by lex/yacc parser
        /// </summary>
        [Test]
        public void Issue2098Test()
        {
            string source =
            @"CRDS id 'P_NDB_1934' nm 'NDB_MAMO_2' ty 0 wm 1038.16 w1 0 w2 0 sw 9999900000 lt lw PDIN 0 0 '' pdin CLTT 'Level [m]' 'Tot. Width [m]' 'Flow width [m]' cltt CLID '(null)' '(null)' '(null)' clid " + Environment.NewLine +
            @"TBLE " + Environment.NewLine +
            @"-30 0.07 0.07 <" + Environment.NewLine +
            @"-25 6.87 6.87 <" + Environment.NewLine +
            @"-24 335.57 335.57 <" + Environment.NewLine +
            @"-23.5 367.53 367.31 <" + Environment.NewLine +
            @"-21.5 390.53 388.74 <" + Environment.NewLine +
            @"-17.5 513.25 507.84 <" + Environment.NewLine +
            @"-16.5 585.67 578.4 <" + Environment.NewLine +
            @"-15 796.72 785.78 <" + Environment.NewLine +
            @"-14.5 825.43 812.96 <" + Environment.NewLine +
            @"-8.5 960.01 923.29 <" + Environment.NewLine +
            @"-7.5 994.84 953.13 <" + Environment.NewLine +
            @"-2.5 1060.09 971.13 <" + Environment.NewLine +
            @"-1.5 1081.87 972.6 <" + Environment.NewLine +
            @"0 1104.59 973.88 <" + Environment.NewLine +
            @"4.5 1150.49 977.05 <" + Environment.NewLine +
            @"5 1223.93 977.05 <" + Environment.NewLine +
            @"5.5 1223.93 1038.16 <" + Environment.NewLine +
            @"tble  gl 0 gu 0 crds";

            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);
            Assert.AreEqual(17, crossSection.TabulatedProfile.Count);
        }

        [Test]
        [Category(TestCategory.Integration)]
        public void Issue2917Test()
        {
            string source =
                @"CRDS id 'prof_GA24-DP-10' nm 'prof_GA24-DP-10' ty 10 st 0 lt sw 0 0" + Environment.NewLine +
                @"lt yz" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 34.808998 <" + Environment.NewLine +
                @"1.3348682 33.749001 <" + Environment.NewLine +
                @"3.3722408 34.619999 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"gu 0 gl 0   crds";
            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Yztable, crossSection.Type);
        }


        /// <summary>
        /// Issue TOOLS-2098 cross section definition not correctly parsed by lex/yacc parser
        /// </summary>
        [Test]
        public void ExtraSpacesTest()
        {
            string source =
                @"CRDS id '26' nm 'Flat profile' ty 0 wm 300 w1  0 w2  0 sw 0 gl 0 gu 0 lt lw" + Environment.NewLine +
                @"TBLE" + Environment.NewLine +
                @"0 200 200 <" + Environment.NewLine +
                @"0.05 200 200 <" + Environment.NewLine +
                @"0.12 200 200 <" + Environment.NewLine +
                @"0.16 250 250 <" + Environment.NewLine +
                @"0.2 300 300 <" + Environment.NewLine +
                @"tble" + Environment.NewLine +
                @"dk 0 dc 0 db 0 df 0 dt 0 crds";
            var reader = new CrossSectionDefinitionReader();

            var crossSection = reader.GetCrossSectionDefinition(source);
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, crossSection.Type);

        }

        [Test]
        public void TestParsingAsymmetricalTrapezoidalCrossSectionDefinition()
        {
            var asymmetricalTrapezoidal = "CRDS id '2' nm 'Universal Weir 1' ty 11 st 0 lt sw 0 0 gl 0 gu 0 lt yz\r\n" +
                                          "TBLE\r\n" +
                                          "-3.94 2.5 <\r\n" +
                                          "-1.84 2.5 <\r\n" +
                                          "-.8400002 1.5 <\r\n" +
                                          "5.999994E-02 1.5 <\r\n" +
                                          "7.499981E-02 0 <\r\n" +
                                          ".125 0 <\r\n" +
                                          ".1399999 1.5 <\r\n" +
                                          "1.04 1.5 <\r\n" +
                                          "2.64 2.3 <\r\n" +
                                          "3.94 2.3 <\r\n" +
                                          "tble\r\n" +
                                          "crds";

            var reader = new CrossSectionDefinitionReader();
            var crossSection = reader.GetCrossSectionDefinition(asymmetricalTrapezoidal);

            Assert.AreEqual(10, crossSection.YZ.Count);
        }


        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCrossSectionDefinition()
        {
            string definitionFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"CrossSection\vancouver.def");
            IList<SobekCrossSectionDefinition> cs =
                new CrossSectionDefinitionReader().Read(definitionFile).ToList();

            Assert.AreEqual(1, cs.Count);

            SobekCrossSectionDefinition sobekCrossSectionDefinition = cs[0];

            // check type is 10 = yz table 
            Assert.AreEqual(SobekCrossSectionDefinitionType.Yztable, sobekCrossSectionDefinition.Type);
            // number of values
            int numberOfYZValues = sobekCrossSectionDefinition.YZ.Count;
            Assert.AreEqual(32, numberOfYZValues);

            Assert.AreEqual(525, sobekCrossSectionDefinition.YZ[0].X);
            Assert.AreEqual(102.3, sobekCrossSectionDefinition.YZ[0].Y);

            Assert.AreEqual(550, sobekCrossSectionDefinition.YZ[1].X);
            Assert.AreEqual(100.6, sobekCrossSectionDefinition.YZ[1].Y);

            Assert.AreEqual(575, sobekCrossSectionDefinition.YZ[2].X);
            Assert.AreEqual(99.1, sobekCrossSectionDefinition.YZ[2].Y);

            Assert.AreEqual(950, sobekCrossSectionDefinition.YZ[numberOfYZValues - 1].X);
            Assert.AreEqual(101.8, sobekCrossSectionDefinition.YZ[numberOfYZValues - 1].Y);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadCrossSectionLayers()
        {
            string definitionFile = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"CrossSection\profile.def");
            IList<SobekCrossSectionDefinition> cs =
                new CrossSectionDefinitionReader().Read(definitionFile).ToList();

            Assert.AreEqual(55, cs.Count);
            Assert.AreEqual(13, cs.Where(t => t.Type == SobekCrossSectionDefinitionType.Tabulated).Count());
            // check type is 0 = tabulated 
            var first = cs.Where(t => t.Type == SobekCrossSectionDefinitionType.Tabulated).FirstOrDefault();
            Assert.AreEqual(SobekCrossSectionDefinitionType.Tabulated, first.Type);
            // todo add check for converted tabulated cross section
            Assert.AreEqual(20, first.TabulatedProfile.Count);
            Assert.IsNotNull(cs.Where(t => t.Name == "r_Width=20; Height=5").FirstOrDefault());
        }

        [Test]
        public void ExtraLowestPointsAreRemovedWithAWarning()
        {
            string text =
                "CRDS id '1172' nm 'Zandmas3___92.98' ty 0 wm 134 w1 628 w2 0 sw 134 bl 9.9999e+009 lt lw 'Level Width' PDIN 0 0 '' pdin CLTT 'Level [m]' 'Tot. Width [m]' 'Flow width [m]' cltt CLID '(null)' '(null)' '(null)' clid TBLE \r\n" +
                "6.78 0 0 < \r\n" +
                "7.3 0 0 < \r\n" +
                "7.65 3 3 < \r\n" +
                "8.35 56 56 < \r\n" +
                "12.23 203 134 < \r\n" +
                "15.41 242 159 < \r\n" +
                "15.67 284 162 < \r\n" +
                "16.44 290 216 < \r\n" +
                "16.71 321 234 < \r\n" +
                "17.76 365 258 < \r\n" +
                "17.99 441 263 < \r\n" +
                "18.87 468 303 < \r\n" +
                "19.24 711 315 < \r\n" +
                "19.58 881 338 < \r\n" +
                "21.78 918 762 < \r\n" +
                "tble\r\n" +
                " dk 1 dc 19.93 db 18.43 df 508 dt 19 bw 9.9999e+009 bs 9.9999e+009 aw 9.9999e+009 rd 9.9999e+009 ll 9.9999e+009 rl 9.9999e+009 lw 9.9999e+009 rw 9.9999e+009 crds";

            string message =
                "Removing profile row with z=6.78 from crossection Zandmas3___92.98 it is redundant because a higher point also has width 0";
                
            TestHelper.AssertLogMessageIsGenerated(() =>
                                                       {
                                                           var definition =
                                                               new CrossSectionDefinitionReader().
                                                                   Parse(text).ToList()[0];
                                                           //also check the definition has x lines (it is actually removed..)
                                                           Assert.AreEqual(14, definition.TabulatedProfile.Count);
                                                       }, message);

        }
        
        [Test]
        public void YZPointsWithSameYAreAddedADelta()
        {
            const string text = "CRDS id 'P_CoqR_29560' nm 'P_CoqR_29560' ty 10 st 0 lt sw 0 0 lt yz	\r\n" +
                                "TBLE\r\n" +
                                "979.237 7.981 <\r\n" +
                                "997.57 7.692 <	\r\n" +
                                "1002.534 7.446 <\r\n" +
                                "1006.382 6.142 <\r\n" +
                                "1012.635 6.014 <\r\n" +
                                "1017.446 5.38 <\r\n" +
                                "1021.775 5.298 <\r\n" +
                                "1025.624 5.206 <\r\n" +
                                "1029.472 5.142 <\r\n" +
                                "1033.321 4.885 <\r\n" +
                                "1036.207 4.701 <\r\n" +
                                "1039.093 4.646 <\r\n" +
                                "1041.979 4.462 <\r\n" +
                                "1043.904 4.958 <\r\n" +
                                "1044.866 5.38 <\r\n" +
                                "1047.271 6.84 <\r\n" +
                                "1048.552 7.689 <\r\n" +
                                "1051.455 7.809 <\r\n" +
                                "1051.455 8.334 <\r\n" +
                                "1051.719 8.094 <\r\n" +
                                "1054.621 8.832 <\r\n" +
                                "tble  gl 0 gu 0   crds";

            var definition = new CrossSectionDefinitionReader().Parse(text).ToList()[0];
            
            //TODO: check the added delta and the message.
            Assert.AreEqual(21,definition.YZ.Count);
            Assert.AreEqual(definition.YZ[17].X, 1051.455 - Delta, Delta/10);
            Assert.AreEqual(definition.YZ[18].X, 1051.455, Delta/10);
        }

        [Test]
        public void CheckAddingDeltaWorksIfMoreThanTwoPointsHaveTheSameY()
        {
            const string text = "CRDS id 'P_CoqR_29560' nm 'P_CoqR_29560' ty 10 st 0 lt sw 0 0 lt yz	\r\n" +
                                "TBLE\r\n" +
                                "1 7.981 <\r\n" +
                                "1 7.692 <	\r\n" +
                                "1 7.446 <\r\n" +
                                "1 6.142 <\r\n" +
                                "tble  gl 0 gu 0   crds";
            var definition = new CrossSectionDefinitionReader().Parse(text).ToList()[0];

            Assert.AreEqual(definition.YZ[0].X, 1.0d - 2 * Delta, Delta/10);
            Assert.AreEqual(definition.YZ[1].X, 1.0d - Delta, Delta/10);
            Assert.AreEqual(definition.YZ[2].X, 1.0d, Delta/10);
            Assert.AreEqual(definition.YZ[3].X, 1.0d + Delta, Delta/10);
        }

        [Test]
        public void ReadLineWithExclamationMark()
        {
            var text = " CRDS id 'GM4-00A0010-000-1' nm 'GM4-00A0010-000-1!' ty 10 st 0 lt sw 0 0 gl 0 gu 0 lt yz" + Environment.NewLine +
                    "TBLE" + Environment.NewLine +
                    "-26.149 9.080 <" + Environment.NewLine +
                    "-21.149 8.880 <" + Environment.NewLine +
                    "-1.149 8.830 <" + Environment.NewLine +
                    "-0.250 8.020 <" + Environment.NewLine +
                    "-0.01 8.020 <" + Environment.NewLine +
                    "0.000 6.520 <" + Environment.NewLine +
                    "0.01 8.020 <" + Environment.NewLine +
                    "0.250 8.020 <" + Environment.NewLine +
                    "1.149 8.830 <" + Environment.NewLine +
                    "21.149 8.880 <" + Environment.NewLine +
                    "26.149 9.080 <" + Environment.NewLine +
                    "tble" + Environment.NewLine +
                    " crds";

            var definition = new CrossSectionDefinitionReader().Parse(text).FirstOrDefault();

            Assert.IsNotNull(definition);
        }

        [Test]
        public void ReadLineFromWaterTe8()
        {
            var text = " CRDS id 'PRO-CA-1452_' nm 'PRO-CA-1452_' ty 10 lt sw 0 0 lt yz" + Environment.NewLine +
                       "TBLE" + Environment.NewLine +
                       "-19.07 -0.44 <" + Environment.NewLine +
                       "-18.57 -2.16 <" + Environment.NewLine +
                       "-18.07 -2.4 <" + Environment.NewLine +
                       "-17.54 -2.58 <" + Environment.NewLine +
                       "-17.01 -2.72 <" + Environment.NewLine +
                       "-15.91 -2.75 <" + Environment.NewLine +
                       "-14.72 -2.64 <" + Environment.NewLine +
                       "-14.13 -2.58 <" + Environment.NewLine +
                       "-12.88 -2.55 <" + Environment.NewLine +
                       "-11.66 -2.51 <" + Environment.NewLine +
                       "-10.44 -2.5 <" + Environment.NewLine +
                       "-9.35 -2.48 <" + Environment.NewLine +
                       "-8.19 -2.46 <" + Environment.NewLine +
                       "-7.03 -2.46 <" + Environment.NewLine +
                       "-6 -2.41 <" + Environment.NewLine +
                       "-4.85 -2.41 <" + Environment.NewLine +
                       "0 -0.44 <" + Environment.NewLine +
                       "tble" + Environment.NewLine +
                       //"gl 0" + Environment.NewLine +
                       //"gu 0" + Environment.NewLine +
                       "crds ";

            var definition = new CrossSectionDefinitionReader().Parse(text).FirstOrDefault();

            Assert.IsNotNull(definition);
        }

        [Test]
        public void MakeListOfValuesUnique_OutwardValuesAreEqual()
        {
            var originalValues = new List<double> { -25.0, -25.0, -25.0, -20.0, -15.0, -5.0, 5.0, 15.0, 20.0, 25.0, 25.0, 25.0 };
            var uniqueValues = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionDefinitionReader), "MakeValuesUniqueInwards", originalValues);
            Assert.That(uniqueValues, Is.EqualTo(new List<double> { -25.0, -25.0 + Delta, -25.0 + 2 * Delta, -20, -15.0, -5.0, 5.0, 15.0, 20.0, 25.0 - 2 * Delta, 25.0 - Delta, 25.0 }));
        }

        [Test]
        public void MakeListOfValuesUnique_InnerValuesAreEqual()
        {
            var originalValues = new List<double> { -25.0, -25.0, -20.0, -20.0, -15.0, -5.0, 5.0, 20.0, 20.0, 25.0, 25.0, 25.0 };
            var uniqueValues = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionDefinitionReader), "MakeValuesUniqueInwards", originalValues);
            Assert.That(uniqueValues, Is.EqualTo(new List<double> { -25.0, -25.0 + Delta, -20.0, -20.0 + Delta, -15.0, -5.0, 5.0, 20.0 - Delta, 20.0, 25.0 - 2 * Delta, 25.0 - Delta, 25.0 }));
        }

        [Test]
        public void MakeListOfValuesUnique_MoreThanTwoInnerValuesAreEqual()
        {
            var originalValues = new List<double> { -2.0, -2.0, 0.0, 0.0, 0.0, 0.0, 0.0, 2.0, 2.0 };
            var uniqueValues = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionDefinitionReader), "MakeValuesUniqueInwards", originalValues);
            Assert.That(uniqueValues, Is.EqualTo(new List<double> { -2.0, -2.0 + Delta, -2*Delta, -Delta, 0.0, Delta, 2*Delta, 2.0 - Delta, 2.0 }));
        }

        [Test]
        public void MakeListOfValuesUnique_EvenNumberOfValues()
        {
            var originalValues = new List<double> { -2.0, -2.0, 0.0, 0.0, 0.0, 0.0, 2.0, 2.0 };
            var uniqueValues = TypeUtils.CallPrivateStaticMethod(typeof(CrossSectionDefinitionReader), "MakeValuesUniqueInwards", originalValues);
            Assert.That(uniqueValues, Is.EqualTo(new List<double> { -2.0, -2.0 + Delta, - 2*Delta, - Delta, 0.0, Delta, 2.0 - Delta, 2.0 }));
        }
    }
}

