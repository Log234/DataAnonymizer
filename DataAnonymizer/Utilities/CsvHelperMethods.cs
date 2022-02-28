using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;

namespace DataAnonymizer.Utilities
{
    internal static class CsvHelperMethods
    {
        internal static Result<List<List<string>>> FlipAxis(IEnumerable<IEnumerable<string>> input)
        {
            var enumeratedInput = input.ToList();

            var output = new List<List<string>>();

            for (var index = 0; index < enumeratedInput.Count; index++)
            {
                var row = enumeratedInput[index].ToList();

                if (!output.Any())
                    output.AddRange(row.Select(entry => new List<string>(enumeratedInput.Count)));

                if (row.Count != output.Count)
                    return Result.Failure<List<List<string>>>($"The row at line {index + 1} had a different length than the previous rows. Expected {output.Count}, found {row.Count}.");

                for (int i = 0; i < row.Count; i++)
                {
                    output[i].Add(row[i]);
                }
            }

            return output;
        }
    }
}