using System;
using DeltaShell.NGHS.Common.Utils;
using NUnit.Framework;

namespace DeltaShell.NGHS.Common.Tests.Utils
{
    [TestFixture]
    public class RepeatTest
    {
        private readonly Random random = new Random();

        [Test]
        public void Action_RepeatsActionCorrectAmountOfTimes()
        {
            // Setup
            int n = random.Next(0, 10);
            var count = 0;
            void Action() => count++;

            // Call
            Repeat.Action(n, Action);

            // Assert
            Assert.That(count, Is.EqualTo(n));
        }

        [Test]
        public void Action_ActionNull_ThrowsArgumentNullException()
        {
            // Setup
            int n = random.Next(0, 10);

            // Call
            void Call() => Repeat.Action(n, null);

            // Assert
            var exception = Assert.Throws<ArgumentNullException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("action"));
        }

        [Test]
        public void Action_NegativeNumber_ThrowsArgumentOutOfRangeException()
        {
            // Setup
            int n = -random.Next(1, 10);

            // Call
            void Call() => Repeat.Action(n, () => {});

            // Assert
            var exception = Assert.Throws<ArgumentOutOfRangeException>(Call);
            Assert.That(exception.ParamName, Is.EqualTo("n"));
            StringAssert.StartsWith("Number of times cannot be a negative integer.", exception.Message);
        }
    }
}