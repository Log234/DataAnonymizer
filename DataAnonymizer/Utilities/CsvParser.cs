using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace DataAnonymizer.Utilities;

public static class CsvParser
{
    private static readonly Regex QuoteStartRegex = new("^\"");
    private static readonly Regex QuoteEndRegex = new("(?<!\")\"$");

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
            

            if (!lines.Any())
                return new Result<List<List<string>>>();

            var rows = lines.Select(GetRow).ToList();

            var rowResult = rows.Combine();

            if (rowResult.IsFailure)
                return rowResult.ConvertFailure<List<List<string>>>();

            var columns = CsvHelperMethods.FlipAxis(rows.Select(row  => row.Value));

            return columns;
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

    private static Result<IEnumerable<string>> GetRow(string line)
    {
        var splitLine = line.Split(",");

        var output = new List<string>();
        string merger = null;

        foreach (var entry in splitLine)
        {
            if (QuoteStartRegex.IsMatch(entry))
            {
                if (merger is not null)
                    return Result.Failure<IEnumerable<string>>($"Encountered row with unbalanced quotes, a start quote was found within an open quote. The remaining text was: '{merger}'");

                merger = entry[1..];
                continue;
            }

            if (QuoteEndRegex.IsMatch(entry))
            {
                if (merger is null)
                    return Result.Failure<IEnumerable<string>>($"Encountered row with unbalanced quotes, an end quote was found without any start quote before it. Already processed fields: {string.Join(", ", output)}.");

                var merged = merger + "," + entry[..^1];
                output.Add(merged.Replace("\"\"", "\""));
                merger = null;
                continue;
            }

            if (merger is not null)
            {
                merger += "," + entry;
                continue;
            }
            
            output.Add(entry.Replace("\"\"", "\""));
        }

        if (merger is not null)
            return Result.Failure<IEnumerable<string>>($"Encountered row with unbalanced quotes, the remaining text was: '{merger}'");


        return output;
    }
}