using System;
using System.Linq;
using DelftTools.TestUtils;
using DeltaShell.Plugins.ImportExport.Sobek.Tests;
using DeltaShell.Sobek.Readers.Readers.SobekRrReaders;
using DeltaShell.Sobek.Readers.SobekDataObjects;
using NUnit.Framework;

namespace DeltaShell.Sobek.Readers.Tests.Readers.SobekRrReaders
{
    [TestFixture]
    public class SobekRRSeepageReaderTest
    {
      //id   =          seepage identification
      //nm  =          name 
      //co =          computation option seepage
      //                  1 = constant seepage (Default)
      //                  2 = variable seepage, using C and a table for H0
      //                  3 = variable seepage, using C and H0 from Modflow    
      //                  If the co field is missing, co 1 will be assumed. 
      //sp   =          Seepage or percolation  (mm/day)
      //                  Positive numbers represent seepage, negative numbers represent percolation.
      //                  Default 0.
      //ss   =          salt concentration seepage (mg/l). Default 500 mg/l. 
      //                  This value is only important for positive seepage values. 
      //cv   =          Resistance value C for aquitard 
      //h0  =          reference to a table with H0 values 

 


        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSeepageLineFromManual()
        {
            string line =
                @"SEEP  id 'Seep_2' nm 'simple variable Seepage'   co 2 cv 3.0 h0 'H0Table' ss 501. seep";

            var seepageData = new SobekRRSeepageReader().Parse(line).First();

            Assert.AreEqual("Seep_2", seepageData.Id);
            Assert.AreEqual("simple variable Seepage", seepageData.Name);
            Assert.AreEqual(SeepageComputationOption.VariableH0, seepageData.ComputationOption);
            Assert.AreEqual(3.0, seepageData.ResistanceValue);
            Assert.AreEqual("H0Table", seepageData.H0TableName);
            Assert.AreEqual(501.0, seepageData.SaltConcentration);

        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSeepageLineFromTholen()
        {
            string line =
                @"SEEP id 'GFE978' nm 'Kwel GFE978' co 5  PDIN 1 0  pdin ss 'GFE978' " + Environment.NewLine + 
                @"TBLE" + Environment.NewLine + 
                @"'1998/01/01;00:00:00' .022 <" + Environment.NewLine + 
                @"'1998/01/11;00:00:00' .019 <" + Environment.NewLine + 
                @"'1998/01/21;00:00:00' .151 <" + Environment.NewLine + 
                @"'1998/02/01;00:00:00' .045 <" + Environment.NewLine + 
                @"'1998/02/11;00:00:00' .399 <" + Environment.NewLine + 
                @"'1998/02/21;00:00:00' .021 <" + Environment.NewLine + 
                @"'1998/03/01;00:00:00' .027 <" + Environment.NewLine + 
                @"'1998/03/11;00:00:00' .032 <" + Environment.NewLine + 
                @"'1998/03/21;00:00:00' .216 <" + Environment.NewLine + 
                @"'1998/04/01;00:00:00' .022 <" + Environment.NewLine + 
                @"'1998/04/11;00:00:00' .022 <" + Environment.NewLine + 
                @"'1998/04/21;00:00:00' .068 <" + Environment.NewLine + 
                @"'1998/05/01;00:00:00' .448 <" + Environment.NewLine + 
                @"'1998/05/11;00:00:00' 3.455 <" + Environment.NewLine + 
                @"'1998/05/21;00:00:00' .031 <" + Environment.NewLine + 
                @"'1998/06/01;00:00:00' .025 <" + Environment.NewLine + 
                @"'1998/06/11;00:00:00' .022 <" + Environment.NewLine + 
                @"'1998/06/21;00:00:00' .432 <" + Environment.NewLine + 
                @"'1998/07/01;00:00:00' 1.332 <" + Environment.NewLine +
                @"'1998/07/11;00:00:00' .124 <" + Environment.NewLine + 
                @"tble seep";

            var seepageData = new SobekRRSeepageReader().Parse(line).First();
            Assert.AreEqual("GFE978", seepageData.Id);
            Assert.AreEqual("Kwel GFE978", seepageData.Name);
            Assert.IsNotNull(seepageData.SaltTableConcentration);
            Assert.Greater(seepageData.SaltTableConcentration.Rows.Count,0);


        }

        [Test]
        [Category(TestCategory.DataAccess)]
        public void ReadSeepageWithKeywordsInIdAndNameAndWithSomeWhitespace()
        {
            string line =
                "  SEEP id 'seep_1' nm 'SEEP siep' co 5  PDIN 1 0  pdin ss 'GFE978' " + Environment.NewLine +
                "TBLE" + Environment.NewLine +
                "'1998/01/01;00:00:00' .022 <" + Environment.NewLine +
                "'1998/01/11;00:00:00' .019 <" + Environment.NewLine +
                "tble seep  ";

            var seepageData = new SobekRRSeepageReader().Parse(line).First();
            Assert.AreEqual("seep_1", seepageData.Id);
            Assert.AreEqual("SEEP siep", seepageData.Name);
            Assert.AreEqual(2, seepageData.SaltTableConcentration.Rows.Count);
        }

        [Test]
        [Category(TestCategory.DataAccess)]
        [Category(TestCategory.Slow)]
        public void ReadSeepageFile()
        {
            var path = TestHelper.GetTestDataDirectoryPathForAssembly(typeof(SobekNetworkImporterTest).Assembly, @"Tholen.lit\29\Unpaved.sep");
            var lstSeepage = new SobekRRSeepageReader().Read(path);
            Assert.AreEqual(274, lstSeepage.Count());
        }

    }
}