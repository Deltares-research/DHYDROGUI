using System;
using System.Collections.Generic;
using DeltaShell.Plugins.DelftModels.RainfallRunoff.IO.Files.Evaporation;
using NUnit.Framework;

namespace DeltaShell.Plugins.DelftModels.RainfallRunoff.Tests.IO.Files.Evaporation
{
    public class EvaporationFileTest
    {
        private const string headerString = "testHeader";
        
        private IReadOnlyCollection<string> ExpectedHeader =>
            new List<string>
            {
                headerString
            };

        [Test]
        public void Constructor_InitializesInstanceCorrectly()
        {
            // Call
            IEvaporationFile file = new TestEvaporationFile();

            // Assert
            Assert.That(file.Header, Is.EqualTo(ExpectedHeader));
            Assert.That(file.Evaporation, Is.Empty);
        }
        
        [Test]
        public void Add_EvaporationValuesNull_ThrowsArgumentNullException()
        {
            // Setup
            IEvaporationFile file = new TestEvaporationFile();

            // Call
            void Call() => file.Add(DateTime.Today, null);

            // Assert
            Assert.That(Call, Throws.ArgumentNullException
                                    .With.Property(nameof(ArgumentException.ParamName))
                                    .EqualTo("evaporationValues"));
        }

        [Test]
        public void Add_AddsToEvaporation_AndDataIsSorted()
        {
            // Setup
            TestEvaporationFile file = new TestEvaporationFile();
            DateTime date = new DateTime(1, 1, 1);
            double[] evaporationValues =
            {
                1
            };
            
            // Call
            file.Add(date, evaporationValues);
            
            // Assert
            Assert.That(file.Date, Is.EqualTo(date));
            Assert.That(file.EvaporationValues, Is.EqualTo(evaporationValues));
        }

        private class TestEvaporationFile : EvaporationFile
        {
            public DateTime Date;
            public double[] EvaporationValues;
            
            public override IReadOnlyCollection<string> Header =>
                new List<string>
                {
                    headerString
                };
        
            protected override void SetEvaporationValues(DateTime date, double[] evaporationValues)
            {
                EvaporationValues = evaporationValues;
                Date = date;
            }
        }
    }
}