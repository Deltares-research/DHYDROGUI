using System.Drawing;
using DeltaShell.NGHS.Common.Gui;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Gui
{
    [TestFixture]
    public class DeltaresColorTest
    {
        [Test]
        public void Colors_ReturnCorrectValues()
        {
            Assert.That(DeltaresColor.Black.Equals(Color.FromArgb(0, 0, 0)));
            Assert.That(DeltaresColor.DarkBlue.Equals(Color.FromArgb(8, 12, 128)));
            Assert.That(DeltaresColor.Blue.Equals(Color.FromArgb(13, 56, 224)));
            Assert.That(DeltaresColor.LightBlue.Equals(Color.FromArgb(14, 187, 240)));
            Assert.That(DeltaresColor.DarkGreen.Equals(Color.FromArgb(0, 179, 137)));
            Assert.That(DeltaresColor.Green.Equals(Color.FromArgb(0, 204, 150)));
            Assert.That(DeltaresColor.LightGreen.Equals(Color.FromArgb(0, 230, 161)));
            Assert.That(DeltaresColor.LightGray.Equals(Color.FromArgb(242, 242, 242)));
            Assert.That(DeltaresColor.MediumGray.Equals(Color.FromArgb(230, 230, 230)));
            Assert.That(DeltaresColor.Yellow.Equals(Color.FromArgb(255, 216, 20)));
            Assert.That(DeltaresColor.Academy.Equals(Color.FromArgb(255, 150, 13)));
        }
    }
}