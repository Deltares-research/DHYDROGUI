using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DelftTools.Hydro;
using DelftTools.TestUtils;
using DeltaShell.NGHS.IO.DataObjects.Friction;
using DeltaShell.NGHS.IO.FileWriters.CrossSectionDefinition;
using log4net.Core;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileWriters.CrossSectionDefinition
{
    [TestFixture]
    public class CrossSectionDefinitionFileWriterTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void WriteFileCrossSectionLocations_AddsInfoMessageToLogIndicatingWhichFileIsBeingWritten()
        {
            // Setup
            using (var temp = new TemporaryDirectory())
            {
                string filePath = Path.Combine(temp.Path, "crsdef.ini");
                var network = Substitute.For<IHydroNetwork>();
                Func<IChannel, bool> writeFrictionFromCrossSectionDefinitionsForChannel = WriteFrictionFromCrossSectionDefinitionsForChannel(new Dictionary<IChannel, ChannelFrictionDefinition>());
                const string defaultFrictionId = "defaultFrictionId";

                // Call
                void Call() => CrossSectionDefinitionFileWriter.WriteFile(filePath,
                                                                          network,
                                                                          writeFrictionFromCrossSectionDefinitionsForChannel,
                                                                          defaultFrictionId);
                
                IEnumerable<string> infoMessages = TestHelper.GetAllRenderedMessages(Call, Level.Info);

                // Assert
                var expectedMessage = $"Writing cross section definitions to {filePath}.";
                Assert.That(infoMessages.Any(m => m.Equals(expectedMessage)));
            }
        }
        
        private static Func<IChannel, bool> WriteFrictionFromCrossSectionDefinitionsForChannel(IReadOnlyDictionary<IChannel, ChannelFrictionDefinition> channelFrictionDefinitionPerChannelLookup)
        {
            return channel =>
            {
                ChannelFrictionDefinition channelFrictionDefinition = channelFrictionDefinitionPerChannelLookup[channel];

                return channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.RoughnessSections
                       || channelFrictionDefinition.SpecificationType == ChannelFrictionSpecificationType.CrossSectionFrictionDefinitions;
            };
        }
    }
}