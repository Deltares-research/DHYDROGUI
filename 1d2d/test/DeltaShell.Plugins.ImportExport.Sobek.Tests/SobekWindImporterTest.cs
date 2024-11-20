using NUnit.Framework;

namespace DeltaShell.Plugins.ImportExport.Sobek.Tests
{
    [TestFixture]
    public class SobekWindImporterTest
    {
    //     readonly string windFile = @"\FIXED\WINDSER.WND";
    //     SobekWindImporter importer;
    //     WindFunction windFunction;
    //
    //     [SetUp]
    //     public void SetUp()
    //     {
    //         windFunction = new WindFunction();
    //         importer = new SobekWindImporter();
    //     }
    //
    //     [Test]
    //     public void TestImportDefault()
    //     {
    //         var file = TestHelper.GetTestDataDirectory() + @"\FIXED\DEFAULT.WND";
    //         var retrievedWindFunction = importer.ImportItem(file, windFunction);
    //
    //         Assert.AreSame(retrievedWindFunction, windFunction); //target item, so should be same instance
    //
    //         //contains correct wind data
    //         Assert.AreEqual(40, windFunction.Direction.Values.Count);
    //         Assert.AreEqual(40, windFunction.Velocity.Values.Count);
    //         Assert.AreEqual(40, windFunction.Arguments.First().Values.Count);
    //         Assert.IsTrue(windFunction.Direction.Values.OfType<double>().All(v => v== 0.0));
    //         Assert.IsTrue(windFunction.Velocity.Values.OfType<double>().All(v => v == 0.0));
    //     }
    //
    //     [Test]
    //     public void TestImportOtherFile()
    //     {
    //         var retrievedWindFunction = importer.ImportItem(TestHelper.GetTestDataDirectory() + windFile, windFunction);
    //
    //         Assert.AreSame(retrievedWindFunction, windFunction); //target item, so should be same instance
    //
    //         //contains correct wind data
    //         Assert.AreEqual(2, windFunction.Direction.Values.Count);
    //         Assert.AreEqual(2, windFunction.Velocity.Values.Count);
    //         Assert.AreEqual(2, windFunction.Arguments.First().Values.Count);
    //         Assert.AreEqual(new[] {181.0, 183.0}, windFunction.Direction.Values.OfType<double>().ToArray());
    //         Assert.AreEqual(new[] { 11.0, 31.0 }, windFunction.Velocity.Values.OfType<double>().ToArray());
    //     }
    //
    //     [Test]
    //     public void TestImportWithExistingData()
    //     {
    //         //edit windfunction first
    //         windFunction[new DateTime(2000, 1, 1)] = new[] {12.0, 34.0};
    //
    //         Assert.AreEqual(1, windFunction.Direction.Values.Count);
    //         Assert.AreEqual(1, windFunction.Velocity.Values.Count);
    //         Assert.AreEqual(1, windFunction.Arguments.First().Values.Count);
    //
    //         var retrievedWindFunction = importer.ImportItem(TestHelper.GetTestDataDirectory() + windFile, windFunction);
    //
    //         Assert.AreSame(retrievedWindFunction, windFunction); //target item, so should be same instance
    //
    //         //contains correct wind data
    //         Assert.AreEqual(2, windFunction.Direction.Values.Count);
    //         Assert.AreEqual(2, windFunction.Velocity.Values.Count);
    //         Assert.AreEqual(2, windFunction.Arguments.First().Values.Count);
    //         Assert.AreEqual(new[] { 181.0, 183.0 }, windFunction.Direction.Values.OfType<double>().ToArray());
    //         Assert.AreEqual(new[] { 11.0, 31.0 }, windFunction.Velocity.Values.OfType<double>().ToArray());
    //     }
     }
}