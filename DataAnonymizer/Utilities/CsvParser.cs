using System;
using CsvHelper;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Anonymizer.Exceptions;
using CSharpFunctionalExtensions;
using CsvHelper.Configuration;

namespace Anonymizer.Utilities;

internal static class CsvParser
{
    /// <summary>
    /// Parses a CSV file and returns a dictionary of header titles to lists of the respective values.
    /// </summary>
    /// <param name="path">Path to the CSV file</param>
    /// <returns>A Dictionary of the file content.</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public static Result<Dictionary<string, List<string>>> ParseFile(string path)
    {
        try
        {
            using var reader = new StreamReader(path, Encoding.GetEncoding("iso-8859-1"));
            using var csv = new CsvReader(reader, CultureInfo.CurrentCulture);

            List<dynamic> records = csv.GetRecords<dynamic>().ToList();

            if (!records.Any())
                return new Dictionary<string, List<string>>();

            IDictionary<string, object> recordAsDict = records.First();

            var header = recordAsDict.Keys;
            var columns = new Dictionary<string, List<string>>();

            foreach (var title in header)
                columns[title] = new List<string>();

            for (int i = 0; i < records.Count; i++)
            {
                IDictionary<string, object> entry = records[i];

                foreach (var title in header)
                {
                    if (!entry.TryGetValue(title, out object value) || value is null)
                        throw new CsvException($"The field {title} in row {i} is null.");

                    columns[title].Add((string) value);
                }
            }

            return columns;
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            return Result.Failure<Dictionary<string, List<string>>>($"Could not find a csv file at: \"{path}\"");
        }
        catch (IOException e)
        {
            return Result.Failure<Dictionary<string, List<string>>>($"An error occurred while trying to load the file, is the file currently open in Excel? Error message: {e.Message}");
        }
        catch (Exception e) when (e is ArgumentException or ArgumentNullException or NotSupportedException)
        {
            return Result.Failure<Dictionary<string, List<string>>>($"An unexpected error occurred while trying to load the file: {e.Message}");
        }
    }
}