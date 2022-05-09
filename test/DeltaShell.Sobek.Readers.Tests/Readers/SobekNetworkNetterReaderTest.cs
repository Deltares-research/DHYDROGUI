using System;
using DeltaShell.Sobek.Readers.Readers;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers
{
    [TestFixture]
    public class SobekNetworkNetterReaderTest
    {
        [Test]
        [Category("Quarantine")]
        public void ReadFromStringSobek212Format()
        {
            var fileText = GetTestFile();
            var dictionary = SobekNetworkNetterReader.ParseNodeTypes(fileText);

            Assert.IsTrue(dictionary.ContainsKey("T1_h1"));
            Assert.AreEqual("SBK_BOUNDARY",dictionary["T1_h1"]);

            Assert.IsTrue(dictionary.ContainsKey("2"));
            Assert.AreEqual("SBK_PROFILE",dictionary["2"]);

            Assert.IsTrue(dictionary.ContainsKey("T2_h3"));
            Assert.AreEqual("SBK_CHANNELCONNECTION",dictionary["T2_h3"]);
        }

        private string GetTestFile()
        {
            var text = @"""NTW6.6"",""F:\SBO212_3\271b.lit\CMTWORK\ntrpluv.ini"",""SOBEK-LITE, edit network""" + Environment.NewLine +
                    @"""2"","""",1,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T1_h1"",""T1_h1_Q=0"","""",1,14,""SBK_BOUNDARY"","""",0,0,0,0,""SYS_DEFAULT"",0,""2"","""","""",1,20,""SBK_PROFILE"","""",50,0,-1,50,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""11"","""",1,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""2"","""","""",1,20,""SBK_PROFILE"","""",50,0,-1,50,""SYS_DEFAULT"",0,""T1_Qlat1"",""T1_Qlat1_Linear_NoPeriod"","""",1,19,""SBK_LATERALFLOW"","""",100,0,0,100,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""3"","""",2,1,""SBK_CHANNEL"","""",0,0,0,0,150,0,0,0,""T2_h3"",""T2_h3"","""",2,12,""SBK_CHANNELCONNECTION"","""",500,50,2.2,0,""SYS_DEFAULT"",0,""T2_h2"",""T2_h2"","""",2,17,""SBK_GRIDPOINTFIXED"","""",350,50,0,150,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""21"","""",2,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""8"","""","""",2,20,""SBK_PROFILE"","""",50,50,-1,450,""SYS_DEFAULT"",0,""T2_h1"",""T2_h1_Q=0"","""",2,14,""SBK_BOUNDARY"","""",0,50,0,500,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""T3_R1"",""T3_R1"",3,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T3_h1"",""T3_h1_h=2m"","""",3,14,""SBK_BOUNDARY"","""",0,100,0,0,""SYS_DEFAULT"",0,""9"","""","""",3,20,""SBK_PROFILE"","""",50,100,-1,50,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""31"","""",3,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""9"","""","""",3,20,""SBK_PROFILE"","""",50,100,-1,50,""SYS_DEFAULT"",0,""T3_Qlat1"",""T3_Qlat1_Const_Pos"","""",3,19,""SBK_LATERALFLOW"","""",100,100,0,100,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""4"","""",2,1,""SBK_CHANNEL"","""",0,0,0,0,250,0,0,0,""T2_h2"",""T2_h2"","""",2,17,""SBK_GRIDPOINTFIXED"","""",350,50,0,150,""SYS_DEFAULT"",0,""T2_Qlat1"",""T2_Qlat1_Linear_Period_12hrs"","""",2,19,""SBK_LATERALFLOW"","""",100,50,0,400,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""8"","""",1,1,""SBK_CHANNEL"","""",0,0,0,0,100,0,0,0,""T1_h2"",""T1_h2"","""",1,17,""SBK_GRIDPOINTFIXED"","""",400,0,0,400,""SYS_DEFAULT"",0,""T1_h3"",""T1_h3"","""",1,12,""SBK_CHANNELCONNECTION"","""",500,0,2.5,500,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""10"","""",1,1,""SBK_CHANNEL"","""",0,0,0,0,300,0,0,0,""T1_Qlat1"",""T1_Qlat1_Linear_NoPeriod"","""",1,19,""SBK_LATERALFLOW"","""",100,0,0,100,""SYS_DEFAULT"",0,""T1_h2"",""T1_h2"","""",1,17,""SBK_GRIDPOINTFIXED"","""",400,0,0,400,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""14"","""",3,1,""SBK_CHANNEL"","""",0,0,0,0,300,0,0,0,""T3_Qlat1"",""T3_Qlat1_Const_Pos"","""",3,19,""SBK_LATERALFLOW"","""",100,100,0,100,""SYS_DEFAULT"",0,""T3_h2"",""T3_h2"","""",3,17,""SBK_GRIDPOINTFIXED"","""",400,100,0,400,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""5"","""",3,1,""SBK_CHANNEL"","""",0,0,0,0,100,0,0,0,""T3_h2"",""T3_h2"","""",3,17,""SBK_GRIDPOINTFIXED"","""",400,100,0,400,""SYS_DEFAULT"",0,""T3_h3"",""T3_h3"","""",3,12,""SBK_CHANNELCONNECTION"","""",500,100,-4,500,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""6"","""",2,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T2_Qlat1"",""T2_Qlat1_Linear_Period_12hrs"","""",2,19,""SBK_LATERALFLOW"","""",100,50,0,400,""SYS_DEFAULT"",0,""8"","""","""",2,20,""SBK_PROFILE"","""",50,50,-1,450,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""7"","""",4,1,""SBK_CHANNEL"","""",0,0,0,0,150,0,0,0,""T4_h3"",""T4_h3"","""",4,12,""SBK_CHANNELCONNECTION"","""",500,150,0,0,""SYS_DEFAULT"",0,""T4_h2"",""T4_h2"","""",4,17,""SBK_GRIDPOINTFIXED"","""",350,150,0,150,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""12"","""",4,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""6"","""","""",4,20,""SBK_PROFILE"","""",50,150,0,450,""SYS_DEFAULT"",0,""T4_h1"",""T4_h1_h=2m"","""",4,14,""SBK_BOUNDARY"","""",0,150,0,500,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""T4_R2"",""T4_R2"",4,1,""SBK_CHANNEL"","""",0,0,0,0,250,0,0,0,""T4_h2"",""T4_h2"","""",4,17,""SBK_GRIDPOINTFIXED"","""",350,150,0,150,""SYS_DEFAULT"",0,""T4_Qlat1"",""T4_Qlat1_Const_Neg"","""",4,19,""SBK_LATERALFLOW"","""",100,150,0,400,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""15"","""",4,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T4_Qlat1"",""T4_Qlat1_Const_Neg"","""",4,19,""SBK_LATERALFLOW"","""",100,150,0,400,""SYS_DEFAULT"",0,""6"","""","""",4,20,""SBK_PROFILE"","""",50,150,0,450,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""1"","""",5,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T5_h1"",""T5_h1_Qh_rel"","""",5,14,""SBK_BOUNDARY"","""",0,200,0,0,""SYS_DEFAULT"",0,""11"","""","""",5,20,""SBK_PROFILE"","""",50,200,0,50,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""16"","""",6,1,""SBK_CHANNEL"","""",0,0,0,0,150,0,0,0,""T6_h3"",""T6_h3"","""",6,12,""SBK_CHANNELCONNECTION"","""",500,250,0,0,""SYS_DEFAULT"",0,""T6_h2"",""T6_h2"","""",6,17,""SBK_GRIDPOINTFIXED"","""",350,250,0,150,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""17"","""",5,1,""SBK_CHANNEL"","""",0,0,0,0,100,0,0,0,""T5_h2"",""T5_h2"","""",5,17,""SBK_GRIDPOINTFIXED"","""",400,200,0,400,""SYS_DEFAULT"",0,""T5_h3"",""T5_h3"","""",5,12,""SBK_CHANNELCONNECTION"","""",500,200,0,500,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""18"","""",6,1,""SBK_CHANNEL"","""",0,0,0,0,250,0,0,0,""T6_h2"",""T6_h2"","""",6,17,""SBK_GRIDPOINTFIXED"","""",350,250,0,150,""SYS_DEFAULT"",0,""T6_Qlat1"",""T6_Qlat1_Blocked_Period_24hrs"","""",6,19,""SBK_LATERALFLOW"","""",100,250,0,400,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""19"","""",5,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""11"","""","""",5,20,""SBK_PROFILE"","""",50,200,0,50,""SYS_DEFAULT"",0,""T5_Qlat1"",""T5_Qlat1_Blocked_NoPeriod"","""",5,19,""SBK_LATERALFLOW"","""",100,200,0,100,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""20"","""",6,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""12"","""","""",6,20,""SBK_PROFILE"","""",50,250,0,450,""SYS_DEFAULT"",0,""T6_h1"",""T6_h1_Qh_rel"","""",6,14,""SBK_BOUNDARY"","""",0,250,0,500,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""22"","""",5,1,""SBK_CHANNEL"","""",0,0,0,0,300,0,0,0,""T5_Qlat1"",""T5_Qlat1_Blocked_NoPeriod"","""",5,19,""SBK_LATERALFLOW"","""",100,200,0,100,""SYS_DEFAULT"",0,""T5_h2"",""T5_h2"","""",5,17,""SBK_GRIDPOINTFIXED"","""",400,200,0,400,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""23"","""",6,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T6_Qlat1"",""T6_Qlat1_Blocked_Period_24hrs"","""",6,19,""SBK_LATERALFLOW"","""",100,250,0,400,""SYS_DEFAULT"",0,""12"","""","""",6,20,""SBK_PROFILE"","""",50,250,0,450,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""24"","""",7,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""4"","""","""",7,20,""SBK_PROFILE"","""",50,300,-1,450,""SYS_DEFAULT"",0,""T7_h1"",""T7_h1_h=2m"","""",7,14,""SBK_BOUNDARY"","""",0,300,0,500,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""25"","""",7,1,""SBK_CHANNEL"","""",0,0,0,0,0,0,0,0,""T7_h6"","""","""",7,14,""SBK_BOUNDARY"","""",500,300,0,0,""SYS_DEFAULT"",0,""T7_Qlat4"",""T7_Qlat4"","""",7,19,""SBK_LATERALFLOW"","""",500,300,0,0,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""T7_R5"",""T7_R5"",7,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T7_Qlat4"",""T7_Qlat4"","""",7,19,""SBK_LATERALFLOW"","""",500,300,0,0,""SYS_DEFAULT"",0,""T7_h5"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",450,300,0,50,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""T7_R4"",""T7_R4"",7,1,""SBK_CHANNEL"","""",0,0,0,0,231.079453498438,0,0,0,""T7_h5"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",450,300,0,50,""SYS_DEFAULT"",0,""T7_Qlat3"",""T7_Qlat3"","""",7,19,""SBK_LATERALFLOW"","""",218.920546501562,300,0,281.079453498438,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""T7_R3"",""T7_R3"",7,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T7_h4"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",200,300,0,300,""SYS_DEFAULT"",0,""T7_h3"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",150,300,0,350,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""T7_R1"",""T7_R1"",7,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T7_h2"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",100,300,0,400,""SYS_DEFAULT"",0,""4"","""","""",7,20,""SBK_PROFILE"","""",50,300,-1,450,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""T7_R2"",""T7_R2"",7,1,""SBK_CHANNEL"","""",0,0,0,0,50,0,0,0,""T7_Qlat1"",""T7_Qlat1"","""",7,19,""SBK_LATERALFLOW"","""",150,300,0,350,""SYS_DEFAULT"",0,""T7_h2"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",100,300,0,400,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""27"","""",7,1,""SBK_CHANNEL"","""",0,0,0,0,0,0,0,0,""T7_h3"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",150,300,0,350,""SYS_DEFAULT"",0,""T7_Qlat1"",""T7_Qlat1"","""",7,19,""SBK_LATERALFLOW"","""",150,300,0,350,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""9"",""9"",7,1,""SBK_CHANNEL"","""",0,0,0,0,7.52046035270899,0,0,0,""T7_Qlat2"",""T7_Qlat2"","""",7,19,""SBK_LATERALFLOW"","""",207.520460352709,300,0,292.479539647291,""SYS_DEFAULT"",0,""T7_h4"","""","""",7,17,""SBK_GRIDPOINTFIXED"","""",200,300,0,300,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""13"",""13"",7,1,""SBK_CHANNEL"","""",0,0,0,0,11.400086148853,0,0,0,""T7_Qlat3"",""T7_Qlat3"","""",7,19,""SBK_LATERALFLOW"","""",218.920546501562,300,0,281.079453498438,""SYS_DEFAULT"",0,""T7_Qlat2"",""T7_Qlat2"","""",7,19,""SBK_LATERALFLOW"","""",207.520460352709,300,0,292.479539647291,""SYS_DEFAULT"",0" + Environment.NewLine +
                    @"""*""" + Environment.NewLine +
                       Environment.NewLine +
                    @"[Reach description]" + Environment.NewLine +
                    @"7" + Environment.NewLine +
 			           Environment.NewLine +
                    @"""1"","""",""T1_h1"",""T1_h3"",0,2,0,0,500,0,500,1,1000,-1" + Environment.NewLine +
                    @"""2"","""",""T2_h3"",""T2_h1"",0,2,0,50,500,50,500,1,1000,-1" + Environment.NewLine +
                    @"""3"","""",""T3_h1"",""T3_h3"",0,2,0,100,500,100,500,1,1000,-1" + Environment.NewLine +
                    @"""4"","""",""T4_h3"",""T4_h1"",0,2,0,150,500,150,500,0,1000,-1" + Environment.NewLine +
                    @"""5"","""",""T5_h1"",""T5_h3"",0,2,0,200,500,200,500,0,1000,-1" + Environment.NewLine +
                    @"""6"","""",""T6_h3"",""T6_h1"",0,2,0,250,500,250,500,0,1000,-1" + Environment.NewLine +
                    @"""7"","""",""T7_h6"",""T7_h1"",0,2,0,300,500,300,500,0,1000,-1" + Environment.NewLine +
			            Environment.NewLine +
                    @"[Model connection node]" + Environment.NewLine +
                    @"""1.00""" + Environment.NewLine +
                    @"64,5" + Environment.NewLine +
                    @"12,""SBK_CHANNELCONNECTION"","""",1,""SOBEK"",""4""" + Environment.NewLine +
                    @"14,""SBK_BOUNDARY"","""",3,""SOBEK"",""3"",""SOBEK"",""4"",""SOBEK"",""31""" + Environment.NewLine +
                    @"17,""SBK_GRIDPOINTFIXED"","""",1,""SOBEK"",""0""" + Environment.NewLine +
                    @"19,""SBK_LATERALFLOW"","""",2,""SOBEK"",""10"",""SOBEK"",""31""" + Environment.NewLine +
                    @"20,""SBK_PROFILE"","""",3,""SOBEK"",""5"",""SOBEK"",""16"",""SOBEK"",""21""" + Environment.NewLine +
			            Environment.NewLine +
                    @"[Model connection branch]" + Environment.NewLine +
                    @"""1.00""" + Environment.NewLine +
                    @"22,1" + Environment.NewLine +
                    @"1,""SBK_CHANNEL"","""",2,""SOBEK"",""0"",""SOBEK"",""31""" + Environment.NewLine +
			            Environment.NewLine +
                    @"[Nodes with calculationpoint]" + Environment.NewLine +
                    @"""1.00""" + Environment.NewLine +
                    @"0" + Environment.NewLine +
			            Environment.NewLine +
                    @"[Reach options]" + Environment.NewLine +
                    @"""1.00""" + Environment.NewLine +
                    @"0" + Environment.NewLine +

                    @"[NTW properties]" + Environment.NewLine +
                    @"""1.00""" + Environment.NewLine +
                    @"3" + Environment.NewLine +
                    @"v1=4" + Environment.NewLine +
                    @"v2=0" + Environment.NewLine +
                    @"v3=990";

            return text;

        }

        [Test]
        [Category("Quarantine")]
        public void testNodeIsLinkageNode()
        {

            var text = @"""NTW6.6"",""D:\SOBEK212\linkageN.lit\CMTWORK\ntrpluv.ini"",""SOBEK-LITE, edit network""" +
                       Environment.NewLine +
                       @"""1"","""",1,1,""SBK_CHANNEL"","""",0,0,0,0,17655.4607118648,0,0,0,""1"","""","""",1,14,""SBK_BOUNDARY"","""",118717.630730904,455010.579627601,0,0,""SYS_DEFAULT"",0,""3"","""","""",0,12,""SBK_CHANNELCONNECTION"","""",136338.737192355,456111.442935378,0,0,""SYS_DEFAULT"",0" +
                       Environment.NewLine +
                       @"""2"","""",2,1,""SBK_CHANNEL"","""",0,0,0,0,15334.974627331,0,0,0,""3"","""","""",0,12,""SBK_CHANNELCONNECTION"","""",136338.737192355,456111.442935378,0,0,""SYS_DEFAULT"",0,""4"","""","""",2,15,""SBK_CHANNELLINKAGENODE"","""",151390.09896151,459047.078422782,0,15334.974627331,""SYS_DEFAULT"",0" +
                       Environment.NewLine +
                       @"""3"","""",3,1,""SBK_CHANNEL"","""",0,0,0,0,27227.7618533051,0,0,0,""4"","""","""",2,15,""SBK_CHANNELLINKAGENODE"","""",151390.09896151,459047.078422782,0,15334.974627331,""SYS_DEFAULT"",0,""2"","""","""",3,14,""SBK_BOUNDARY"","""",178555.971422912,460881.85060241,0,27227.7618533051,""SYS_DEFAULT"",0" +
                       Environment.NewLine +
                       @"""*""" + Environment.NewLine +

                       @"[Reach description]" + Environment.NewLine +
                       @"3 " + Environment.NewLine +
                       @"""1"","""",""1"",""3"",0,2,118717.630730904,455010.579627601,136338.737192355,456111.442935378,17655.4607118648,0,1000,-1" +
                       Environment.NewLine +
                       @"""2"","""",""3"",""4"",0,2,136338.737192355,456111.442935378,151390.09896151,459047.078422782,15334.974627331,0,1000,-1" +
                       Environment.NewLine +
                       @"""3"","""",""4"",""2"",0,2,151390.09896151,459047.078422782,178555.971422912,460881.85060241,27227.7618533051,0,1000,-1" +
                       Environment.NewLine +

                       @"[Model connection node]" + Environment.NewLine +
                       @"""1.00""" + Environment.NewLine +
                       @"64,3" + Environment.NewLine +
                       @"12,""SBK_CHANNELCONNECTION"","""",1,""SOBEK"",""4""" + Environment.NewLine +
                       @"14,""SBK_BOUNDARY"","""",3,""SOBEK"",""3"",""SOBEK"",""4"",""SOBEK"",""31""" +
                       Environment.NewLine +
                       @"15,""SBK_CHANNELLINKAGENODE"","""",1,""SOBEK"",""4""" + Environment.NewLine +

                       @"[Model connection branch]" + Environment.NewLine +
                       @"""1.00""" + Environment.NewLine +
                       @"22,1" + Environment.NewLine +
                       @"1,""SBK_CHANNEL"","""",2,""SOBEK"",""0"",""SOBEK"",""31""" + Environment.NewLine +

                       @"[Nodes with calculationpoint]" + Environment.NewLine +
                       @"""1.00""" + Environment.NewLine +
                       @"0" + Environment.NewLine +

                       @"[Reach options]" + Environment.NewLine +
                       @"""1.00""" + Environment.NewLine +
                       @"0" + Environment.NewLine +

                       @"[NTW properties]" + Environment.NewLine +
                       @"""1.00""" + Environment.NewLine +
                       @"3" + Environment.NewLine +
                       @"v1=4" + Environment.NewLine +
                       @"v2=0" + Environment.NewLine +
                       @"v3=990";

            var dictionary = SobekNetworkNetterReader.ParseNodeTypes(text);

            Assert.IsTrue(dictionary.ContainsKey("4"));
            Assert.AreEqual("SBK_CHANNELLINKAGENODE", dictionary["4"]);

        }


    }
}
