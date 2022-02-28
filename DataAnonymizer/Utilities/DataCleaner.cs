using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DataAnonymizer.Utilities;

public static class DataCleaner
{
    private const bool AllowShortenedNames = false;
    private static TextInfo textInfo = CultureInfo.GetCultureInfo("no").TextInfo;

    public static Result<List<List<string>>> Clean(List<List<string>> input, (bool, ColumnTypes)[] columnTypeDict, Dictionary<string, string> idDictionary)
    {
        var header = input.Select(column => column.First()).ToList();
        var data = input.Select(column => column.Skip(1).ToList()).ToList();

        var results = new List<Result<string>>();

        for (var index = 0; index < input.Count; index++)
        {
            var (shouldAnonymize, columnType) = columnTypeDict[index];

            switch (columnType)
            {
                case ColumnTypes.General:
                    var generalResults = Clean(data[index], GeneralStringClean).ToList();

                    if (Result.Combine(generalResults).IsFailure)
                        results.AddRange(generalResults);
                    else
                        data[index] = generalResults.Select(r => r.Value).ToList();
                    break;

                case ColumnTypes.Name:
                    var nameResults = Clean(data[index], NameCleaning).ToList();

                    if (Result.Combine(nameResults).IsFailure)
                        results.AddRange(nameResults);
                    else
                    {
                        data[index] = nameResults.Select(r => r.Value).ToList();
                    }

                    break;

                case ColumnTypes.Company:
                    var companyResults = Clean(data[index], CompanyCleaning).ToList();

                    if (Result.Combine(companyResults).IsFailure)
                        results.AddRange(companyResults);
                    else
                    {
                        data[index] = companyResults.Select(r => r.Value).ToList();
                    }

                    break;

                default:
                    return Result.Failure<List<List<string>>>(
                        $"Failed to clean column: {header[index]}. Did not find a cleaning method matching {columnTypeDict[index]}.");
            }

            if (shouldAnonymize)
                data[index] = SwitchWithIds(data[index], idDictionary).ToList();
        }

        var combinedResults = Result.Combine(results, "\n");

        if (combinedResults.IsFailure)
            return combinedResults.ConvertFailure<List<List<string>>>();

        var output = header.Select(value => new List<string>() {value}).ToList();

        for (var index = 0; index < data.Count; index++)
        {
            output[index].AddRange(data[index]);
        }

        return output;
    }

    public static IEnumerable<string> SwitchWithIds(IEnumerable<string> input, Dictionary<string, string> idDictionary)
    {
        return input.Select(input =>
        {
            if (idDictionary.ContainsKey(input))
                return idDictionary[input];

            var id = (idDictionary.Count + 1).ToString();
            idDictionary[input] = id;

            return id;
        });
    }

    public static IEnumerable<Result<string>> Clean(IEnumerable<string> input, Func<string, Result<string>> cleaningRule)
    {
        return input.Select(cleaningRule);
    }

    public static Result<string> GeneralStringClean(string input)
    {
        return input.Trim();
    }

    public static Result<string> NameCleaning(string input)
    {
        var cleanedInput = input.Trim();

        if (cleanedInput.Contains('(') || cleanedInput.Contains(')'))
            return Result.Failure<string>($"The name '{cleanedInput}' contains parentheses, which is not allowed.");

        if (cleanedInput.LastOrDefault() == ',')
            cleanedInput = cleanedInput[..^1];

        var casedInput = textInfo.ToTitleCase(cleanedInput);
        var commaSplit = casedInput.Split(", ");

        var simplifiedName = commaSplit[0];

        if (commaSplit.Length == 2)
            simplifiedName = $"{commaSplit[1]} {commaSplit[0]}";

        if (commaSplit.Length > 2)
            return Result.Failure<string>($"The name '{cleanedInput}' contains more than one comma and cannot be interpreted.");

        if (input.Contains('.') && !AllowShortenedNames)
            return Result.Failure<string>($"The name '{cleanedInput}' contains a potentially shortened name, which is not allowed.");

        return simplifiedName;
    }

    public static Result<string> CompanyCleaning(string input)
    {
        var companyRegex = new Regex(@"^[aA](\/|\.)?[sS][aA.]?\z");

        var cleanedInput = input.Trim();

        var casedInput = textInfo.ToUpper(cleanedInput);

        var splitInput = casedInput.Split(" ");

        if (companyRegex.IsMatch(splitInput.First()))
            return string.Join(" ", splitInput.Skip(1)) + (splitInput.First().EndsWith("a", StringComparison.InvariantCultureIgnoreCase) ? " ASA" : " AS");

        if (companyRegex.IsMatch(splitInput.Last()))
        {
            return string.Join(" ", splitInput.SkipLast(1)) + (splitInput.Last().EndsWith("a", StringComparison.InvariantCultureIgnoreCase) ? " ASA" : " AS");
        }

        return cleanedInput;
    }
}