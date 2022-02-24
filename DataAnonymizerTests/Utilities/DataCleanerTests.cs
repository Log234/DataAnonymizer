using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataAnonymizer.Utilities.Tests;

[TestClass]
public class DataCleanerTests
{
    [DataTestMethod]
    [DataRow("Company Name", "Company Name", "Simple name")]
    [DataRow("Company Name as", "Company Name AS", "as last")]
    [DataRow("Company Name AS", "Company Name AS", "AS last")]
    [DataRow("Company Name a/s", "Company Name AS", "a/s last")]
    [DataRow("Company Name A/S", "Company Name AS", "A/S last")]
    [DataRow("Company Name A.S", "Company Name AS", "A.S last")]
    [DataRow("Company Name A.S.", "Company Name AS", "A.S. last")]
    [DataRow("as Company Name", "Company Name AS", "as first")]
    [DataRow("AS Company Name", "Company Name AS", "AS first")]
    [DataRow("a/s Company Name", "Company Name AS", "a/s first")]
    [DataRow("A/S Company Name", "Company Name AS", "A/S first")]
    [DataRow("A.S Company Name", "Company Name AS", "A.S first")]
    [DataRow("A.S. Company Name", "Company Name AS", "A.S. first")]
    [DataRow("Company Name asa", "Company Name ASA", "asa last")]
    [DataRow("Company Name ASA", "Company Name ASA", "ASA last")]
    [DataRow("asa Company Name", "Company Name ASA", "asa first")]
    [DataRow("ASA Company Name", "Company Name ASA", "ASA first")]
    public void CompanyCleaningTest(string input, string expected)
    {
        var result = DataCleaner.CompanyCleaning(input);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [TestMethod]
    public void Test()
    {
        var stringVal = "";
        stringVal.Should().Be("");
    }
}