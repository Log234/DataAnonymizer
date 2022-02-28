using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DataAnonymizer.Utilities;

public static class CsvParser
{
    private static readonly Regex QuoteStartRegex = new("^\"", RegexOptions.Singleline);
    private static readonly Regex QuoteEndRegex = new("(?<!\")\"$", RegexOptions.Singleline);

    /// <summary>
    /// Parses a CSV file and returns a dictionary of header titles to lists of the respective values.
    /// </summary>
    /// <param name="path">Path to the CSV file</param>
    /// <returns>A Dictionary of the file content.</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public static Result<List<List<string>>> ParseFile(string path)
    {
        try
        {
            var lines = File.ReadAllLines(path, Encoding.GetEncoding("iso-8859-1"));
            var mergedLines = MergeQuotedNewLines(lines).ToList();

            if (!mergedLines.Any())
                return new Result<List<List<string>>>();

            var rows = mergedLines.Select(GetRow);

            var rowResult = rows.Combine();

            if (rowResult.IsFailure)
                return rowResult.ConvertFailure<List<List<string>>>();

            var columns = CsvHelperMethods.FlipAxis(rows.Select(row  => row.Value));

            if (columns.IsFailure)
                return columns;

            var columnEndIndex = columns.Value.Count - 1;

            for (; columnEndIndex > 0; columnEndIndex--)
            {
                if (columns.Value[columnEndIndex].Any(value => !string.IsNullOrWhiteSpace(value)))
                    break;
            }

            return columns.Value.Take(columnEndIndex+1).ToList();
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            return Result.Failure<List<List<string>>>($"Could not find a csv file at: \"{path}\"");
        }
        catch (IOException e)
        {
            return Result.Failure<List<List<string>>>($"An error occurred while trying to load the file, is the file currently open in Excel? Error message: {e.Message}");
        }
        catch (Exception e) when (e is ArgumentException or ArgumentNullException or NotSupportedException)
        {
            return Result.Failure<List<List<string>>>($"An unexpected error occurred while trying to load the file: {e.Message}");
        }
    }

    private static IEnumerable<string> MergeQuotedNewLines(IEnumerable<string> lines)
    {
        var mergedLines = new List<string>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(mergedLines.LastOrDefault()))
            {
                mergedLines.Add(line);
                continue;
            }

            if (mergedLines.Last().Count(character => character == '"') % 2 != 0)
            {
                mergedLines[mergedLines.Count - 1] = mergedLines.Last() + "\n" + line;
                continue;
            }

            mergedLines.Add(line);
        }

        return mergedLines;
    }

    private static Result<IEnumerable<string>> GetRow(string line)
    {
        var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;

        var splitLine = line.Split(separator);

        var output = new List<string>();
        string merger = null;

        foreach (var entry in splitLine)
        {
            if (QuoteStartRegex.IsMatch(entry))
            {
                if (merger is not null)
                    return Result.Failure<IEnumerable<string>>($"Encountered row with unbalanced quotes, a start quote was found within an open quote. The remaining text was: '{merger}'");

                if (QuoteEndRegex.IsMatch(entry))
                {
                    output.Add(entry);
                    continue;
                }

                merger = entry[1..];
                continue;
            }

            if (QuoteEndRegex.IsMatch(entry))
            {
                if (merger is null)
                    return Result.Failure<IEnumerable<string>>($"Encountered row with unbalanced quotes, an end quote was found without any start quote before it. Already processed fields: {string.Join(", ", output)}.");

                var merged = merger + separator + entry[..^1];
                output.Add(merged.Replace("\"\"", "\""));
                merger = null;
                continue;
            }

            if (merger is not null)
            {
                merger += separator + entry;
                continue;
            }
            
            output.Add(entry.Replace("\"\"", "\""));
        }

        if (merger is not null)
            return Result.Failure<IEnumerable<string>>($"Encountered row with unbalanced quotes, the remaining text was: '{merger}'");


        return output;
    }
}