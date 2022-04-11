using System.Collections.Generic;
using System.Collections.ObjectModel;
using DelftTools.Hydro;
using DelftTools.Hydro.CrossSections;
using DeltaShell.NGHS.IO.FileReaders;
using DeltaShell.NGHS.IO.FileReaders.Definition.Structures;
using DeltaShell.NGHS.IO.Helpers;
using DeltaShell.NGHS.IO.Properties;
using GeoAPI.Extensions.Networks;
using NUnit.Framework;

namespace DeltaShell.NGHS.IO.Tests.FileReaders.Definition.Structures
{
    [TestFixture]
    public class StructureDefinitionParserTest
    {
        private const string structuresFilename = "structures.ini";

        [Test]
        public void ReadStructure_CategoryNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = null;
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            string type = "bridge";

            // Call
            TestDelegate call = () => category.ReadStructure(crossSectionDefinitions, branch, type, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void ReadStructure_CrossSectionDefinitionsNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = null;
            IBranch branch = new Channel();
            string type = "bridge";

            // Call
            TestDelegate call = () => category.ReadStructure(crossSectionDefinitions, branch, type, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void ReadStructure_BranchNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = null;
            string type = "bridge";

            // Call
            TestDelegate call = () => category.ReadStructure(crossSectionDefinitions, branch, type, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void ReadStructure_TypeNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            string type = null;

            // Call
            TestDelegate call = () => category.ReadStructure(crossSectionDefinitions, branch, type, structuresFilename);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }
        
        [Test]
        public void ReadStructure_FilenameNull_ThrowsArgumentNullException()
        {
            // Setup
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            string type = "bridge";

            // Call
            TestDelegate call = () => category.ReadStructure(crossSectionDefinitions, branch, type, null);

            // Assert
            Assert.That(call, Throws.ArgumentNullException);
        }

        [Test]
        public void ReadStructure_UnknownType_ThrowsFileReadingException()
        {
            // Setup
            IDelftIniCategory category = new DelftIniCategory("structure");
            ICollection<ICrossSectionDefinition> crossSectionDefinitions = new Collection<ICrossSectionDefinition>();
            IBranch branch = new Channel();
            string type = "UnknownStructureType";

            // Call
            TestDelegate call = () => category.ReadStructure(crossSectionDefinitions, branch, type, structuresFilename);

            // Assert
            string expectedMessage = string.Format(Resources.StructureDefinitionParser_Could_not_parse_structure_type, type);
            Assert.That(call, Throws.Exception
                                    .TypeOf<FileReadingException>()
                                    .With.Message.EqualTo(expectedMessage));
        }
    }
}