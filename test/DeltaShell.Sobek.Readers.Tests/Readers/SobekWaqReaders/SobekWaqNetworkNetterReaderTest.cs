using System;
using System.Collections.Generic;
using System.Linq;
using DelftTools.Utils.Reflection;
using DeltaShell.Sobek.Readers.Readers.SobekWaqReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekWaqReaders
{
    [TestFixture]
    public class SobekWaqNetworkNetterReaderTest
    {
        # region Sobek212

        [Test]
        public void ReadUserDefinedNodeTypeIdsFromSobek212()
        {
            var nodeObjects = (IEnumerable<SobekWaqNetworkNetterReader.NetterNodeObject>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNetworkNetterReader), "ParseNodeTypeIds", new[] { GetTestFile() });
            Assert.AreEqual(10, nodeObjects.Count());
            var n1 = nodeObjects.Where(n => n.Id == "N1").FirstOrDefault();
            var n2 = nodeObjects.Where(n => n.Id == "N2").FirstOrDefault();
            var n3 = nodeObjects.Where(n => n.Id == "N3").FirstOrDefault();
            var ln2 = nodeObjects.Where(n => n.Id == "LN2").FirstOrDefault();
            Assert.AreEqual("SBK_BOUNDARY_BNTYPE1", n1.UserDefinedType);
            Assert.AreEqual("SBK_BOUNDARY_BNTYPE2", n2.UserDefinedType);
            Assert.AreEqual("", n3.UserDefinedType);
            Assert.AreEqual("SBK_CHANNEL_STORCONN&LAT_LONELYNODE", ln2.UserDefinedType);
        }

        [Test]
        public void ReadUserDefinedBranchTypeIdsFromSobek212()
        {
            var branchObjects = (IEnumerable<SobekWaqNetworkNetterReader.NetterBranchObject>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNetworkNetterReader), "ParseBranchTypeIds", new[] { GetTestFile() });
            Assert.AreEqual(8, branchObjects.Count());
            var b1 = branchObjects.Where(bo => bo.Id == "b1").FirstOrDefault();
            var b2 = branchObjects.Where(bo => bo.Id == "b2").FirstOrDefault();
            var b4 = branchObjects.Where(bo => bo.Id == "b4").FirstOrDefault();
            var b5 = branchObjects.Where(bo => bo.Id == "b5").FirstOrDefault();
            var b6 = branchObjects.Where(bo => bo.Id == "b6").FirstOrDefault();
            var b3 = branchObjects.Where(bo => bo.Id == "b3").FirstOrDefault();
            Assert.AreEqual("SBK_CHANNEL_BRANCHTYPENORMAL", b1.UserDefinedType);
            Assert.AreEqual("SBK_CHANNEL_BRANCHTYPENOTNORMAL", b2.UserDefinedType);
            Assert.AreEqual("SBK_CHANNEL_BRANCHTYPENORMAL", b4.UserDefinedType);
            Assert.AreEqual("SBK_CHANNEL_BRANCHTYPENORMAL", b5.UserDefinedType);
            Assert.AreEqual(1, b1.ReachIndex);
            Assert.AreEqual(2, b2.ReachIndex);
            Assert.AreEqual(1, b4.ReachIndex);
            Assert.AreEqual(2, b5.ReachIndex);
            Assert.AreEqual("SBK_CHANNEL", b1.ParentType);
            // branch b6 is predefined type
            Assert.AreEqual("", b6.UserDefinedType);
            // branch b3 has different parent type
            Assert.AreEqual("SBK_CHANNEL&LAT", b3.ParentType);
        }

        [Test]
        public void ReadReachesFromSobek212()
        {
            var reaches = (IDictionary<string, int>)TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNetworkNetterReader), "ParseReachIds", new[] { GetTestFile() });
            Assert.AreEqual(5, reaches.Count);
            Assert.AreEqual(1, reaches["r1"]);
            Assert.AreEqual(5, reaches["r5"]);
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no valid data was found")]
        public void ReadUserDefinedNodeTypeIdsFromSobek212InvalidFile()
        {
            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNetworkNetterReader), "ParseNodeTypeIds", new[] { GetInvalidTestFile() });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no valid data was found")]
        public void ReadUserDefinedBranchTypeIdsFromSobek212InvalidFile()
        {
            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNetworkNetterReader), "ParseBranchTypeIds", new[] { GetInvalidTestFile() });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        [Test]
        [ExpectedException(typeof(FormatException), ExpectedMessage = "no valid data was found")]
        public void ReadReachesFromSobek212InvalidFile()
        {
            try
            {
                TypeUtils.CallPrivateStaticMethod(typeof(SobekWaqNetworkNetterReader), "ParseReachIds", new[] { GetInValidReachTestFile() });
            }
            catch (Exception e)
            {
                throw e.InnerException;
            }
        }

        private static string GetTestFile()
        {
            return @"""NTW6.6"",""D:\SOBEK212\NewTest.lit\CMTWORK\ntrpluv.ini"",""SOBEK-LITE-DELWAQ, edit network""" + Environment.NewLine +
                   @"""b1"",""Link1"",1,23,""SBK_CHANNEL"",""SBK_CHANNEL_BRANCHTYPENORMAL"",0,0,0,0,368.089888978055,0,0,0,""N1"",""Node1"","""",1,65,""SBK_BOUNDARY"",""SBK_BOUNDARY_BNTYPE1"",254902.698666233,471482.595349359,0,0,""SYS_DEFAULT"",0,""LS1"",""LateralSource1"","""",1,67,""SBK_LATERALFLOW"",""SBK_LATERALFLOW_LSTYPE1"",255205.071885633,471692.501524965,0,368.089888978055,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @"""b2"",""Link2"",2,24,""SBK_CHANNEL"",""SBK_CHANNEL_BRANCHTYPENOTNORMAL"",0,0,0,0,355.591480904288,0,0,0,""N3"",""Node3"","""",0,12,""SBK_CHANNELCONNECTION"","""",255445.558183461,471859.446068411,0,0,""SYS_DEFAULT"",0,""LS2"",""LateralSource2"","""",2,68,""SBK_LATERALFLOW"",""SBK_LATERALFLOW_LSTYPE2"",255745.093896196,472051.080246177,0,355.591480904288,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @"""b3"",""Link3"",3,25,""SBK_CHANNEL&LAT"",""SBK_CHANNEL&LAT_BRANCHTYPETEST"",0,0,0,0,575.047057021267,0,0,0,""N3"",""Node3"","""",0,12,""SBK_CHANNELCONNECTION"","""",255445.558183461,471859.446068411,0,0,""SYS_DEFAULT"",0,""N2a"",""Node2a"","""",3,66,""SBK_BOUNDARY"",""SBK_BOUNDARY_BNTYPE2"",255551.114200699,472424.72214699,0,575.047057021267,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @"""b4"",""Link4"",1,23,""SBK_CHANNEL"",""SBK_CHANNEL_BRANCHTYPENORMAL"",0,0,0,0,292.752694316215,0,0,0,""LS1"",""LateralSource1"","""",1,67,""SBK_LATERALFLOW"",""SBK_LATERALFLOW_LSTYPE1"",255205.071885633,471692.501524965,0,368.089888978055,""SYS_DEFAULT"",0,""N3"",""Node3"","""",0,12,""SBK_CHANNELCONNECTION"","""",255445.558183461,471859.446068411,0,0,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @"""b5"",""Link5"",2,23,""SBK_CHANNEL"",""SBK_CHANNEL_BRANCHTYPENORMAL"",0,0,0,0,315.712096343275,0,0,0,""LS2"",""LateralSource2"","""",2,68,""SBK_LATERALFLOW"",""SBK_LATERALFLOW_LSTYPE2"",255745.093896196,472051.080246177,0,355.591480904288,""SYS_DEFAULT"",0,""N2"",""Node2"","""",2,66,""SBK_BOUNDARY"",""SBK_BOUNDARY_BNTYPE2"",256011.036847239,472221.222758701,0,671.303577247563,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @"""b6"",""Link6"",4,1,""SBK_CHANNEL"","""",0,0,0,0,264.274891655822,0,0,0,""N3"",""Node3"","""",0,12,""SBK_CHANNELCONNECTION"","""",255445.558183461,471859.446068411,0,0,""SYS_DEFAULT"",0,""LS3"",""LateralSource3"","""",4,19,""SBK_LATERALFLOW"","""",255693.306904324,471767.458081856,0,264.274891655822,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @"""b7"",""Link7"",4,1,""SBK_CHANNEL"","""",0,0,0,0,298.710961957881,0,0,0,""LS3"",""LateralSource3"","""",4,19,""SBK_LATERALFLOW"","""",255693.306904324,471767.458081856,0,264.274891655822,""SYS_DEFAULT"",0,""N4"",""Node4"","""",4,14,""SBK_BOUNDARY"","""",255973.338269654,471663.483694504,0,562.985853613703,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @"""b8"",""Link8"",5,1,""SBK_CHANNEL"","""",0,0,0,0,477.259110007224,0,0,0,""N3"",""Node3"","""",0,12,""SBK_CHANNELCONNECTION"","""",255445.558183461,471859.446068411,0,0,""SYS_DEFAULT"",0,""LN2"",""LonelyNode2"","""",5,69,""SBK_CHANNEL_STORCONN&LAT"",""SBK_CHANNEL_STORCONN&LAT_LONELYNODE"",255922.445189914,471878.288604365,0,477.259110007224,""SYS_DEFAULT"",0" + Environment.NewLine +
                   @""""","""",0,0,"""","""",0,0,0,0,0,0,0,0,""LN1"",""LonelyNode1"","""",0,69,""SBK_CHANNEL_STORCONN&LAT"",""SBK_CHANNEL_STORCONN&LAT_LONELYNODE"",255045.953261057,472115.704557367,.1,0,""SYS_DEFAULT"",0,"""","""","""",0,0,"""","""",0,0,0,0,"""",0" + Environment.NewLine +
                   @"""*""" + Environment.NewLine +
                   @"" + Environment.NewLine +
                   @"[Reach description]" + Environment.NewLine +
                   @"" + Environment.NewLine +
                   @" 5 " + Environment.NewLine +
                   @"""r1"",""Reach1"",""N1"",""N3"",0,2,254902.698666233,471482.595349359,255445.558183461,471859.446068411,660.84258329427,0,100,-1" + Environment.NewLine +
                   @"""r2"",""Reach2"",""N3"",""N2"",0,2,255445.558183461,471859.446068411,256011.036847239,472221.222758701,671.303577247563,0,100,-1" + Environment.NewLine +
                   @"""r3"",""Reach3"",""N3"",""N2a"",0,2,255445.558183461,471859.446068411,255551.114200699,472424.72214699,575.047057021267,0,100,-1" + Environment.NewLine +
                   @"""r4"",""Reach4"",""N3"",""N4"",0,2,255445.558183461,471663.483694504,255973.338269654,471859.446068411,562.985853613703,0,100,-1" + Environment.NewLine +
                   @"""r5"",""Reach5"",""N3"",""LN2"",0,2,255445.558183461,471859.446068411,255922.445189914,471878.288604365,477.259110007224,0,100,-1" + Environment.NewLine +
                   @"" + Environment.NewLine +
                   @"[Model connection node]" +  Environment.NewLine;
        }

        private static string GetInvalidTestFile()
        {
            return @"""NTW6.6"",""D:\SOBEK212\NewTest.lit\CMTWORK\ntrpluv.ini"",""SOBEK-LITE-DELWAQ, edit network""" + Environment.NewLine +
                   @"""b1"",""Link1"",1,23,""SBK_CHANNEL"",""SBK_CHANNEL_BRANCHTYPENORMAL"",0,0,0,0,368.089888978055,0,0,0,""N1"",""Node1"","""",1,65,""SBK_BOUNDARY"",""SBK_BOUNDARY_BNTYPE1"",254902.698666233,471482.595349359,0,0,""SYS_DEFAULT"",0,""LS1"",""LateralSource1"","""",1,67,""SBK_LATERALFLOW"",""SBK_LATERALFLOW_LSTYPE1"",255205.071885633,471692.501524965,0,368.089888978055,""SYS_DEFAULT""" + Environment.NewLine;
        }

        private static string GetInValidReachTestFile()
        {
            return @"[Reach description]" + Environment.NewLine +
                   @"" + Environment.NewLine +
                   @" x " + Environment.NewLine +
                   @"""r1"",""Reach1"",""N1"",""N3"",0,2,254902.698666233,471482.595349359,255445.558183461,471859.446068411,660.84258329427,0,100,-1" + Environment.NewLine +
                   @"""r2"",""Reach2"",""N3"",""N2"",0,2,255445.558183461,471859.446068411,256011.036847239,472221.222758701,671.303577247563,0,100,-1" + Environment.NewLine +
                   @"""r3"",""Reach3"",""N3"",""N2a"",0,2,255445.558183461,471859.446068411,255551.114200699,472424.72214699,575.047057021267,0,100,-1" + Environment.NewLine +
                   @"""r4"",""Reach4"",""N3"",""N4"",0,2,255445.558183461,471663.483694504,255973.338269654,471859.446068411,562.985853613703,0,100,-1" + Environment.NewLine +
                   @"""r5"",""Reach5"",""N3"",""LN2"",0,2,255445.558183461,471859.446068411,255922.445189914,471878.288604365,477.259110007224,0,100,-1" + Environment.NewLine;
        }

        # endregion
    }
}
