using Services;

namespace UnitTest;

[TestClass]
public class TestSearchServiceUnitTest
{
    private readonly SearchService _service = new SearchService();
    // [TestMethod]
    // public void FindTestFiles()
    // {
    //     string[] files =
    //     {
    //         "file.spec.ts",
    //         "file.cy.ts",
    //         "cy.file.ts",
    //         "spec.file.ts",
    //         "cy.file.cy",
    //         "spec.file.spec",
    //     };
    //     var result = _service.FindTestFiles(files);
    //     string[] expected =
    //     {
    //         "file.spec.ts",
    //         "file.cy.ts",
    //     };
    //     Assert.AreEqual(2, result.Length);
    //     Assert.AreEqual(expected[0], result[0]);
    //     Assert.AreEqual(expected[1], result[1]);
    // }
}