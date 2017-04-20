using System.Collections.Generic;
using DelftTools.Hydro;
using DeltaShell.Plugins.ImportExport.Sobek.HisData;
using NetTopologySuite.Extensions.Coverages;
using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests.HisData
{
    [TestFixture]
    public class NetworkLocationTypeConvertorTest
    {
        [Test]
        public void NetworkLocationTypeSequenceTest()
        {
            var typeConvertor = new NetworkLocationTypeConvertor();
            var channel1 = new Channel{Id = 1, Name = "channel1"};
            var channel2 = new Channel{ Id = 2, Name = "channel2" };

            var location1 = new NetworkLocation(channel1,50);
            var location2 = new NetworkLocation(channel1, 150);
            var location3 = new NetworkLocation(channel2, 100);
            var location4 = new NetworkLocation(channel2, 200);


            typeConvertor.AddItem("A", location1);
            typeConvertor.AddItem("Z", location2);
            typeConvertor.AddItem("B", location3);
            typeConvertor.AddItem("Y", location4);

            Assert.AreEqual(typeConvertor.LocationMames(), new List<string> { "A", "Z", "B", "Y" });
            Assert.AreEqual(typeConvertor.NetworkLocations(), new List<NetworkLocation> { location1,location2,location3,location4 });

        }
    }
}
