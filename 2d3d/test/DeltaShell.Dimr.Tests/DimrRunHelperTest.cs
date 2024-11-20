using System.IO;
using System.Text;
using DelftTools.Shell.Core.Workflow;
using DelftTools.Shell.Core.Workflow.DataItems;
using DelftTools.TestUtils;
using DelftTools.Utils;
using DelftTools.Utils.Collections.Generic;
using NSubstitute;
using NUnit.Framework;

namespace DeltaShell.Dimr.Tests
{
    [TestFixture]
    public class DimrRunHelperTest
    {
        [Test]
        [Category(TestCategory.DataAccess)]
        public void ConnectDimrRunLogFile_ShouldReadLogFileAndStoreInfoInDataItem()
        {
            using (var tempDirectory = new TemporaryDirectory())
            {
                // Arrange
                string path = Path.Combine(tempDirectory.Path, "dimr_redirected.log");
                const string text = "This is some text in the file.";

                using (FileStream fs = File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(text);
                    fs.Write(info, 0, info.Length);
                }

                var model = Substitute.For<IModel>();

                var dataItems = new EventedList<IDataItem>();
                var dataItem = Substitute.For<IDataItem>();
                dataItem.Tag.Returns("DimrRunLog");
                dataItem.Value.Returns(new TextDocument());
                dataItems.Add(dataItem);

                model.DataItems = dataItems;

                // Act
                DimrRunHelper.ConnectDimrRunLogFile(model, tempDirectory.Path);

                // Assert
                Assert.AreEqual(text, ((TextDocument) dataItem.Value).Content);
            }
        }

        [Test]
        public void DimrRunLogfileDataItemTag_ShouldReturnCorrectTagName()
        {
            Assert.AreEqual("DimrRunLog", DimrRunHelper.dimrRunLogfileDataItemTag);
        }
    }
}