using System.Linq;
using DeltaShell.Plugins.FMSuite.Common.Wind;
using NUnit.Framework;

namespace DeltaShell.Plugins.FMSuite.Common.Tests
{
    [TestFixture]
    public class FileBasedWindDefinitionTest
    {
        [Test]
        public void SetTypeShouldUpdateWindFiles()
        {
            var windDefinition = new FileBasedWindDefinition(WindDefinitionType.WindXWindY);

            Assert.AreEqual(
                new[]
                    {
                        FileBasedWindDefinition.FileBasedWindQuantity.VelocityX,
                        FileBasedWindDefinition.FileBasedWindQuantity.VelocityY,
                        FileBasedWindDefinition.FileBasedWindQuantity.AirPressure
                    }, windDefinition.WindFiles.Keys);

            windDefinition.Type = WindDefinitionType.WindXY;

            Assert.AreEqual(
                new[]
                    {
                        FileBasedWindDefinition.FileBasedWindQuantity.VelocityVector,
                        FileBasedWindDefinition.FileBasedWindQuantity.AirPressure
                    }, windDefinition.WindFiles.Keys);

            windDefinition.Type = WindDefinitionType.WindXYP;

            Assert.AreEqual(
                new[]
                    {
                        FileBasedWindDefinition.FileBasedWindQuantity.VelocityVectorAirPressure
                    }, windDefinition.WindFiles.Keys);

            windDefinition.Type = WindDefinitionType.SpiderWebGrid;

            Assert.AreEqual(
                new[]
                    {
                        FileBasedWindDefinition.FileBasedWindQuantity.SpiderWeb
                    }, windDefinition.WindFiles.Keys);
        }

        [Test]
        public void AddSpiderWebShouldUpdateWindFiles()
        {
            var windDefinition = new FileBasedWindDefinition(WindDefinitionType.WindXWindY);
            windDefinition.AddSpiderWeb("cyclone");
            Assert.AreEqual(
                new[]
                    {
                        FileBasedWindDefinition.FileBasedWindQuantity.VelocityX,
                        FileBasedWindDefinition.FileBasedWindQuantity.VelocityY,
                        FileBasedWindDefinition.FileBasedWindQuantity.AirPressure,
                        FileBasedWindDefinition.FileBasedWindQuantity.SpiderWeb
                    }, windDefinition.WindFiles.Keys);
            Assert.AreEqual(new[] {null, null, null, "cyclone"},
                            windDefinition.WindFiles.Values.Select(v => v.FilePathHandler.FilePath).ToList());
        }

        [Test]
        public void CannotAddSpiderWebTwice()
        {
            var windDefinition = new FileBasedWindDefinition(WindDefinitionType.WindXWindY);

            Assert.IsTrue(windDefinition.CanAddSpiderWeb);
            Assert.IsFalse(windDefinition.CanRemoveSpiderWeb);

            windDefinition.AddSpiderWeb();

            Assert.IsFalse(windDefinition.CanAddSpiderWeb);
            Assert.IsTrue(windDefinition.CanRemoveSpiderWeb);

            windDefinition.AddSpiderWeb();

            Assert.AreEqual(4, windDefinition.WindFiles.Keys.Count);
        }

        [Test]
        public void CannotAddSpiderWebToSpiderWeb()
        {
            var windDefinition = new FileBasedWindDefinition(WindDefinitionType.SpiderWebGrid);

            Assert.IsFalse(windDefinition.CanAddSpiderWeb);
            Assert.IsFalse(windDefinition.CanRemoveSpiderWeb);

            windDefinition.AddSpiderWeb();

            Assert.AreEqual(1, windDefinition.WindFiles.Keys.Count);
        }
    }
}
