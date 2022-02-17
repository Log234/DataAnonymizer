using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Anonymizer.Utilities;

internal static class DataCleaner
{
    private const bool AllowShortenedNames = false;

    public static Result<Dictionary<string, List<string>>> Clean(Dictionary<string, List<string>> input, Dictionary<string, (bool, ColumnTypes)> columnTypeDict, Dictionary<string, string> idDictionary)
    {
        var results = new List<Result<string>>();

        foreach (var entry in input.Keys)
        {
            var (shouldAnonymize, columnType) = columnTypeDict[entry];

            switch (columnType)
            {
                case ColumnTypes.General:
                    var generalResults = Clean(input[entry], GeneralStringClean).ToList();

                    if (Result.Combine(generalResults).IsFailure)
                        results.AddRange(generalResults);
                    else
                        input[entry] = generalResults.Select(r => r.Value).ToList();
                    break;

                case ColumnTypes.Name:
                    var nameResults = Clean(input[entry], NameCleaning).ToList();

                    if (Result.Combine(nameResults).IsFailure)
                        results.AddRange(nameResults);
                    else
                    {
                        input[entry] = nameResults.Select(r => r.Value).ToList();
                    }
                    break;

                case ColumnTypes.Company:
                    var companyResults = Clean(input[entry], CompanyCleaning).ToList();

                    if (Result.Combine(companyResults).IsFailure)
                        results.AddRange(companyResults);
                    else
                    {
                        input[entry] = companyResults.Select(r => r.Value).ToList();
                    }
                    break;

                default:
                    return Result.Failure<Dictionary<string, List<string>>>($"Failed to clean column: {entry}. Did not find a cleaning method matching {columnTypeDict[entry]}.");
            }

            if (shouldAnonymize)
                input[entry] = SwitchWithIds(input[entry], idDictionary).ToList();
        }

        var combinedResults = Result.Combine(results);

        if (combinedResults.IsFailure)
            return combinedResults.ConvertFailure<Dictionary<string, List<string>>>();

        return input;
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

        var commaSplit = cleanedInput.Split(", ");

        var simplifiedName = commaSplit[0];

        if (commaSplit.Length == 2)
            simplifiedName = $"{commaSplit[1]} {commaSplit[0]}";

        if (commaSplit.Length > 2)
            Result.Failure<string>($"The name '{input}' contains more than one comma and cannot be interpreted.");

        if (input.Contains('.') && !AllowShortenedNames)
            Result.Failure<string>($"The name '{input}' contains a potentially shortened name, which is not allowed.");

        return simplifiedName;
    }

    public static Result<string> CompanyCleaning(string input)
    {
        var companyRegex = new Regex(@"^[aA]\/?[sS][aA]?\z");

        var cleanedInput = input.Trim();

        var splitInput = cleanedInput.Split(" ");

        if (companyRegex.IsMatch(splitInput.First()))
            return string.Join(" ", splitInput.Skip(1)) + (splitInput.First().EndsWith("a", StringComparison.InvariantCultureIgnoreCase) ? " ASA" : " AS");

        if (companyRegex.IsMatch(splitInput.Last()))
        {
            return string.Join(" ", splitInput.SkipLast(1)) + (splitInput.Last().EndsWith("a", StringComparison.InvariantCultureIgnoreCase) ? " ASA" : " AS");
        }

        return cleanedInput;
    }
}