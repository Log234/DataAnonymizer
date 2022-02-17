using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Anonymizer.Utilities;

public static class CsvWriter
{
    public static void Write(Dictionary<string, List<string>> data, string path)
    {
        var dataList = DictionaryToDynamicList(data);

        using var writer = new StringWriter();
        using var csvWriter = new CsvHelper.CsvWriter(writer, CultureInfo.InvariantCulture);

        csvWriter.WriteRecords(dataList);
        var csvString = writer.ToString();

        File.WriteAllText(path, csvString, Encoding.GetEncoding("iso-8859-1"));
    }

    public static IEnumerable<dynamic> DictionaryToDynamicList(Dictionary<string, List<string>> data)
    {
        if (data.Count == 0)
            return new List<dynamic>();

        var length = data.First().Value.Count;

        List<dynamic> dynamicList = new(length);

        bool first = true;
        foreach (var kvPair in data)
        {
            if (first)
            {
                foreach (var field in kvPair.Value)
                {
                    IDictionary<string, object> newObject = new ExpandoObject();
                    newObject.Add(kvPair.Key, field);

                    dynamicList.Add(newObject);
                }

                first = false;
            }
            else
            {
                for (int i = 0; i < kvPair.Value.Count; i++)
                {
                    ((IDictionary<string, object>)dynamicList[i]).Add(kvPair.Key, kvPair.Value[i]);
                }
            }
        }

        return dynamicList;
    }
}