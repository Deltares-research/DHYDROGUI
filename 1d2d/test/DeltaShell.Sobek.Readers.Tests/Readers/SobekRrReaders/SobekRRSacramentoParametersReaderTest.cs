using System.Linq;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRSacramentoParametersReaderTest
    {
        [Test]
        public void ReadSacramentoParameters()
        {
            var record =
                "OPAR id 'OtherParameters' zperc 5.0 rexp 9.0 pfree 0.2 rserv 0.95 pctim 0" +
                " adimp 0.5 sarva 0.0 side 0.0 ssout 0.0 pm 0.1 pt1 500 pt2 500 opar";
            
            var parameters = new SobekRRSacramentoParametersReader().Parse(record).FirstOrDefault();

            Assert.IsNotNull(parameters);

            Assert.AreEqual("OtherParameters", parameters.Id);
            Assert.AreEqual(5, parameters.PercolationIncrease);
            Assert.AreEqual(0.0, parameters.RatioUnobservedToObservedBaseFlow);
        }
    }
}
