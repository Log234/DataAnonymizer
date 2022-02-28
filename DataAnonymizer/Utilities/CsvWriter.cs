using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DataAnonymizer.Utilities;

public static class CsvWriter
{
    public static void Write(List<List<string>> data, string path)
    {
        var flippedAxisData = CsvHelperMethods.FlipAxis(data);
        var csvString = SerializeCsv(flippedAxisData.Value);

        File.WriteAllText(path, csvString, Encoding.GetEncoding("iso-8859-1"));
    }

    private static string SerializeCsv(List<List<string>> data)
    {
        var separator = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
        var sb = new StringBuilder();

        for (int i = 0; i < data.Count; i++)
        {
            var row = data[i].Select(column =>
            {
                var escaped = column.Replace("\"", "\"\"");

                if (!(column.Contains(separator) || column.Contains('\n')))
                    return escaped;

                return $"\"{escaped}\"";
            });

            sb.AppendLine(string.Join(separator, row));
        }

        return sb.ToString();
    }
}